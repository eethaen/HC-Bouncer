using System.Linq;
using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Collider2D _levelGenCollider;

    private SpriteRenderer _renderer;
    private float _scale;


    public Transform Transform { get; private set; }
    public Collider2D LevelGenCollider { get => _levelGenCollider; set => _levelGenCollider = value; }

    [Inject]
    public void Construct(SpriteRenderer renderer)
    {
        Transform = transform;
        _renderer = renderer;
    }

    public void Init(Color color)
    {
        _renderer.color = color;
    }

    public struct State
    {
        public float radius;
        public float theta;
    }

    public class Factory : PlaceholderFactory<Obstacle.State, Level, Obstacle> { }
}

public class ObstacleFactory : IFactory<Obstacle.State, Level, Obstacle>
{
    private readonly DiContainer _container;
    private readonly MainSetting _mainSetting;
    private readonly LevelSetting _levelSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly ObstacleSetting _obstacleSetting;
    private Obstacle _instance;

    public ObstacleFactory(DiContainer container, MainSetting mainSetting, LevelSetting levelSetting, ThematicSetting thematicSetting, ObstacleSetting obstacleSetting)
    {
        _container = container;
        _mainSetting = mainSetting;
        _levelSetting = levelSetting;
        _thematicSetting = thematicSetting;
        _obstacleSetting = obstacleSetting;
    }

    public Obstacle Create(Obstacle.State state, Level level)
    {
        _instance = _container.InstantiatePrefabForComponent<Obstacle>(_obstacleSetting.prefab, level.Transform);
        _instance.Init(_thematicSetting.ChapterPalletes[level.Index / _mainSetting.levelsPerChapter].borderColor);

        var scale = Random.Range(0.1f, 0.3f);
        var maxLength = 0.8f * state.radius * level.ChannelSpan / 2.0f * Mathf.Deg2Rad;
        state.theta += (scale * 2.0f - 1.0f) * (maxLength - scale) / 2.0f / state.radius * Mathf.Rad2Deg;

        _instance.Transform.localPosition = new Vector2(state.radius * Mathf.Cos(state.theta * Mathf.Deg2Rad), state.radius * Mathf.Sin(state.theta * Mathf.Deg2Rad));

        if (IsNearPlatform(_levelSetting.platfromObstacleAllowableDistance, level) || IsNearObstacle(_levelSetting.obstacleObstacleAllowableDistance, level))
        {
            CustomDebug.Log("Destrying obstacle for being near another platform or obstacle");
            GameObject.Destroy(_instance.gameObject);
            return null;
        }


        level.Obstacles.Add(_instance);

        _instance.Transform.localScale = new Vector3(scale, scale, 1.0f);

        return _instance;
    }

    private bool IsNearPlatform(float allowableDistance, Level level)
    {
        return level.Platforms.Any(p => (p.Transform.position - _instance.Transform.position).sqrMagnitude < allowableDistance * allowableDistance);
    }

    private bool IsNearObstacle(float allowableDistance, Level level)
    {
        return level.Obstacles.Any(o => (o.Transform.position - _instance.Transform.position).sqrMagnitude < allowableDistance * allowableDistance);
    }
}