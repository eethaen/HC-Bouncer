using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour
{
    private SpriteRenderer _renderer;

    public Transform Transform { get; private set; }

    [Inject]
    public void Construct(SpriteRenderer renderer)
    {
        Transform = transform;
        _renderer = renderer;
    }

    public void Init(Color color)
    {
        _renderer.DOColor(color, 0.3f);

        var scale = Transform.localScale.x;
        var toScale = 1.25f * scale;
        var fromScale = 0.75f * scale;

        Transform.DOScale(new Vector3(toScale, toScale, 1.0f), 1.0f)
                 .From(new Vector3(fromScale, fromScale, 1.0f))
                 .SetEase(Ease.InElastic)
                 .SetLoops(-1, LoopType.Yoyo);
    }

    public struct Coord
    {
        public float radius;
        public float theta;
    }

    public class Factory : PlaceholderFactory<Obstacle.Coord, Level, Obstacle> { }
}

public class ObstacleFactory : IFactory<Obstacle.Coord, Level, Obstacle>
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

    public Obstacle Create(Obstacle.Coord state, Level level)
    {
        _instance = _container.InstantiatePrefabForComponent<Obstacle>(_obstacleSetting.prefab, level.Transform);

        var scale = 0.3f;
        var maxLength = 0.8f * state.radius * level.ChannelSpan / 2.0f * Mathf.Deg2Rad;
        state.theta += (scale * 2.0f - 1.0f) * (maxLength - scale) / 2.0f / state.radius * Mathf.Rad2Deg;

        _instance.Transform.localPosition = new Vector2(state.radius * Mathf.Cos(state.theta * Mathf.Deg2Rad), state.radius * Mathf.Sin(state.theta * Mathf.Deg2Rad));

        if (IsNearPlatform(_levelSetting.ObstaclePlatformMinDistance, level) || IsAlone(_levelSetting.ObstaclePlatformMaxDistance, level))
        {
            CustomDebug.Log("Destroying obstacle");
            GameObject.Destroy(_instance.gameObject);
            return null;
        }
        else
        {
            level.Obstacles.Add(_instance);
            _instance.Transform.localScale = new Vector3(scale, scale, 1.0f);
            _instance.Init(_thematicSetting.ChapterPalletes[level.Index / _mainSetting.levelsPerChapter].obstacleColor);
        }

        return _instance;
    }

    private bool IsNearPlatform(float minDistance, Level level)
    {
        return level.Platforms.Any(p => (p.Transform.position - _instance.Transform.position).sqrMagnitude < minDistance * minDistance);
    }

    private bool IsAlone(float maxDistance, Level level)
    {
        return level.Platforms.Any(p => (p.Transform.position - _instance.Transform.position).sqrMagnitude > maxDistance * maxDistance);
    }

    private bool IsNearObstacle(float allowableDistance, Level level)
    {
        return level.Obstacles.Any(o => (o.Transform.position - _instance.Transform.position).sqrMagnitude < allowableDistance * allowableDistance);
    }
}