using UnityEngine;
using Zenject;

public class Level : MonoBehaviour
{
    public Transform Transform { get; private set; }
    public LineRenderer LineRenderer { get; private set; }
    public EdgeCollider2D EdgeCollider { get; private set; }
    public int Index { get; private set; }
    public int SegmentCount { get; private set; }
    public float SegmentSpan { get; private set; }
    public Segment[] Segments { get; set; }
    public Ball Ball { get; set; }
    public EndPoint EndPoint { get; set; }
    public Trail Trail { get; set; }

    [Inject]
    public void Construct(LineRenderer lineRenderer, EdgeCollider2D edgeCollider)
    {
        LineRenderer = lineRenderer;
        EdgeCollider = edgeCollider;
        Transform = transform;
    }

    internal void Init(int index, int segmentCount, Color color)
    {
        Index = index;
        SegmentCount = segmentCount;
        SegmentSpan = 360.0f / segmentCount;

        Segments = new Segment[segmentCount];

        LineRenderer.positionCount = segmentCount + 1;
        LineRenderer.useWorldSpace = false;
        LineRenderer.startColor = LineRenderer.endColor = color;

    }

    public class Factory : PlaceholderFactory<int, Level> { }
}

public class LevelFactory : IFactory<int, Level>
{
    private readonly DiContainer _container;
    private readonly World _world;
    private readonly Background _background;
    private readonly MainSetting _mainSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly Segment.Factory _segmentFactory;
    private readonly LevelSetting _levelSetting;

    private Level _instance;
    private Vector2[] _corePositions;

    public LevelFactory(DiContainer container, World world, Background background, MainSetting mainSetting, ThematicSetting thematicSetting, Segment.Factory segmentFactory, LevelSetting levelSetting)
    {
        _container = container;
        _world = world;
        _background = background;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _segmentFactory = segmentFactory;
        _levelSetting = levelSetting;
    }

    public Level Create(int level)
    {
        _background.Init(_thematicSetting.ChapterPalletes[level / _mainSetting.levelsPerChapter].backgroundColor_A, _thematicSetting.ChapterPalletes[level / _mainSetting.levelsPerChapter].backgroundColor_B);
        _instance = _container.InstantiatePrefabForComponent<Level>(_levelSetting.levelPrefab, _world.Transform);

        var segmentCount = (int)_levelSetting.segmentCount.Evaluate(level);

        _instance.Init(level, segmentCount, _thematicSetting.ChapterPalletes[level / _mainSetting.levelsPerChapter].borderColor);

        _corePositions = new Vector2[segmentCount + 1];

        var theta = 90.0f - _instance.SegmentSpan / 2.0f;

        for (var i = 0; i < _corePositions.Length; i++)
        {
            _corePositions[i] = new Vector2(_levelSetting.coreRadius * Mathf.Cos(theta * Mathf.Deg2Rad), _levelSetting.coreRadius * Mathf.Sin(theta * Mathf.Deg2Rad));
            _instance.LineRenderer.SetPosition(i, _corePositions[i]);
            theta += _instance.SegmentSpan;
        }

        _instance.EdgeCollider.points = _corePositions;

        for (var i = 0; i < segmentCount; i++)
        {
            _instance.Segments[i] = _segmentFactory.Create(i, _instance);
        }

        return _instance;
    }
}
