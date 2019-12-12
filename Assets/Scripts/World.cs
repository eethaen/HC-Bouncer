using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class World : MonoBehaviour
{
    private SignalBus _signalBus;
    private Border.Factory _borderFactory;
    private Background _background;
    private MainSetting _mainSetting;
    private ThematicSetting _thematicSetting;

    private Vector2[] _corePositions;
    private List<Border> _borders;
    private int _segmentCount;
    private float _span;

    public Transform Transform { get; private set; }
    public LineRenderer LineRenderer { get; private set; }
    public EdgeCollider2D EdgeCollider { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus, MainSetting mainSetting, ThematicSetting thematicSetting, Border.Factory borderFactory, Background background, LineRenderer lineRenderer, EdgeCollider2D edgeCollider)
    {
        _signalBus = signalBus;
        _background = background;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _borderFactory = borderFactory;
        _borders = new List<Border>();
        Transform = transform;
        LineRenderer = lineRenderer;
        EdgeCollider = edgeCollider;

        _signalBus.Subscribe<ThemeUpdated>(OnThemeUpdated);
    }

    private void OnEnable()
    {
        Construct();
    }

    public void Construct()
    {
        _segmentCount = _mainSetting.segmentCount;
        _span = 360.0f / _segmentCount;

        ConstructCore();
        ConstructBorders();
    }

    private void ConstructCore()
    {
        _corePositions = new Vector2[_segmentCount + 1];
        LineRenderer.positionCount = _segmentCount + 1;

        var theta = 90.0f - _span / 2.0f;

        for (var i = 0; i < _corePositions.Length; i++)
        {
            _corePositions[i] = new Vector2(_mainSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _mainSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
            LineRenderer.SetPosition(i, _corePositions[i]);
            theta += _span;
        }

        EdgeCollider.points = _corePositions;
        LineRenderer.useWorldSpace = false;
    }

    private void ConstructBorders()
    {
        var theta = 90.0f - _span / 2.0f;

        for (var i = 0; i < _segmentCount; i++)
        {
            _borders.Add(_borderFactory.Create(theta));
            theta += _span;
        }
    }

    private void OnThemeUpdated(ThemeUpdated msg)
    {
        var palette = _thematicSetting.ChapterPalletes[msg.levelIndex / _mainSetting.levelsPerChapter];

        LineRenderer.startColor = LineRenderer.endColor = palette.borderColor;
        _background.SetColor(palette.backgroundColor.colorA, palette.backgroundColor.colorB);

        for (var i = 0; i < _borders.Count; i++)
        {
            _borders[i].SetColor(palette.borderColor);
        }
    }

    public class Factory : PlaceholderFactory<World> { }
}

public class WorldFactory : IFactory<World>
{
    private readonly DiContainer _container;
    private readonly MainSetting _mainSetting;

    public WorldFactory(DiContainer container,MainSetting mainSetting)
    {
        _container = container;
        _mainSetting = mainSetting;
    }

    public World Create()
    {
        return _container.InstantiatePrefabForComponent<World>(_mainSetting.worldPrefab);
    }
}