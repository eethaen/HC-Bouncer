using System;
using UnityEngine;
using Zenject;

public class World : MonoBehaviour
{
    private SignalBus _signalBus;
    private Background _background;
    private MainSetting _mainSetting;
    private ThematicSetting _thematicSetting;

    private Vector2[] _corePositions;

    public Transform Transform { get; private set; }
    public LineRenderer LineRenderer { get; private set; }
    public EdgeCollider2D EdgeCollider { get; private set; }

    [Inject]
    public void Construct(SignalBus signalBus,MainSetting mainSetting,ThematicSetting thematicSetting,Background background, LineRenderer lineRenderer, EdgeCollider2D edgeCollider)
    {
        _signalBus = signalBus;
        _background = background;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        Transform = transform;
        LineRenderer = lineRenderer;
        EdgeCollider = edgeCollider;

        _signalBus.Subscribe<LevelLoaded>(OnLevelLoaded);
    }

    private void Start()
    {
        ConstructCore();
    }

    private void ConstructCore()
    {
        var segmentCount = _mainSetting.segmentCount;
        var span = 360.0f / segmentCount;

        _corePositions = new Vector2[segmentCount + 1];
        LineRenderer.positionCount = segmentCount + 1;

        var theta = 90.0f - span / 2.0f;

        for (var i = 0; i < _corePositions.Length; i++)
        {
            _corePositions[i] = new Vector2(_mainSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _mainSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
            LineRenderer.SetPosition(i, _corePositions[i]);
            theta += span;
        }

        EdgeCollider.points = _corePositions;
        LineRenderer.useWorldSpace = false;
    }

    private void OnLevelLoaded(LevelLoaded msg)
    {
        LineRenderer.startColor = LineRenderer.endColor = _thematicSetting.ChapterPalletes[msg.level.Index/ _mainSetting.levelsPerChapter].borderColor;
        _background.Init(_thematicSetting.ChapterPalletes[msg.level.Index / _mainSetting.levelsPerChapter].backgroundColor.colorA, _thematicSetting.ChapterPalletes[msg.level.Index / _mainSetting.levelsPerChapter].backgroundColor.colorB);
    }
}
