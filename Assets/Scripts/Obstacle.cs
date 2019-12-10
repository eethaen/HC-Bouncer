using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _scale;

    public Transform Transform { get; private set; }

    [Inject]
    public void Construct(SpriteRenderer renderer)
    {
        Transform = transform;
        _renderer = renderer;
    }

    public void SetColor(Color color)
    {
        _renderer.DOColor(color, 0.3f);
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
        CustomDebug.Log("Creating obstacle");

        _instance = _container.InstantiatePrefabForComponent<Obstacle>(_obstacleSetting.prefab, level.Transform);

        var scale = 0.3f;
        var maxLength = 0.8f * state.radius * level.ChannelSpan / 2.0f * Mathf.Deg2Rad;
        state.theta += (scale * 2.0f - 1.0f) * (maxLength - scale) / 2.0f / state.radius * Mathf.Rad2Deg;

        _instance.Transform.localPosition = new Vector2(state.radius * Mathf.Cos(state.theta * Mathf.Deg2Rad), state.radius * Mathf.Sin(state.theta * Mathf.Deg2Rad));

        if (IsNearPlatform(_levelSetting.ObstaclePlatformAllowableDistance,level))
        {
            CustomDebug.Log("Destroying obstacle for being close to a platform");
            GameObject.Destroy(_instance);
            return null;
        }
        else
        {
            level.Obstacles.Add(_instance);
           //_instance.Transform.localScale = new Vector3(scale, scale, 1.0f);
            _instance.SetColor(_thematicSetting.ChapterPalletes[level.Index/_mainSetting.levelsPerChapter].obstacleColor);
        }
        
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