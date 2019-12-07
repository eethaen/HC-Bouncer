using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class Level : MonoBehaviour
{
    public Transform Transform { get; private set; }

    public int Index { get; private set; }
    public float Span { get; private set; }
    public int ChannelCount { get; private set; }
    public float ChannelSpan { get; private set; }
    public List<Platform> Platforms { get; set; }
    public List<Obstacle> Obstacles { get; set; }
    public bool Revealed { get; set; }

    [Inject]
    public void Construct()
    {
        Transform = transform;

        Platforms = new List<Platform>();
        Obstacles = new List<Obstacle>();
    }

    internal void Init(int index, float span, int channelCount)
    {
        Index = index;
        Span = span;
        ChannelCount = channelCount;
        ChannelSpan = span / channelCount;
    }

    public class Factory : PlaceholderFactory<int, Level> { }
}

public class LevelFactory : IFactory<int, Level>
{
    private readonly DiContainer _container;
    private readonly World _world;
    private readonly Platform.Factory _platformFactory;
    private readonly Obstacle.Factory _obstacleFactory;
    private readonly Border.Factory _borderFactory;
    private readonly MainSetting _mainSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly LevelSetting _levelSetting;
    public LevelFactory(DiContainer container, World world, Platform.Factory platformFactory, Obstacle.Factory obstacleFactory, Border.Factory borderFactory, MainSetting mainSetting, ThematicSetting thematicSetting, LevelSetting levelSetting)
    {
        _container = container;
        _world = world;
        _platformFactory = platformFactory;
        _obstacleFactory = obstacleFactory;
        _borderFactory = borderFactory;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _levelSetting = levelSetting;
    }

    public Level Create(int index)
    {
        var instance = _container.InstantiatePrefabForComponent<Level>(_levelSetting.levelPrefab, _world.Transform);
        instance.Init(index, 360.0f / _mainSetting.segmentCount, (int)_levelSetting.channelCountCurve.Evaluate(index));

        instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, instance.Span * instance.Index);

        ConstructBorders(instance);
        ConstructPlatformsAndObstacles(instance);
        //ConstructObstacles(instance);

        CustomDebug.Log($"Creating level {instance} with span {instance.Span}, and {instance.ChannelCount} channels");

        return instance;
    }

    private void ConstructBorders(Level level)
    {
        _borderFactory.Create(90.0f - level.Span / 2.0f, level.Index == 0, level);
        _borderFactory.Create(90.0f + level.Span / 2.0f, level.Index == 0, level);
    }

    private void ConstructPlatformsAndObstacles(Level level)
    {
        var channelSpan = level.ChannelSpan;
        var platformRndCrit = 0.0f;
        var obstacleRndCrit = 0.5f;
        var transientRndCrit = 0.9f;
        var colorChangeRndCrit = 0.5f;

        for (var r = _mainSetting.coreRadius + _levelSetting.nearestPlatformOffset; r <= _mainSetting.coreRadius + _levelSetting.farthestPlatformOffset.Evaluate(level.Index); r += _levelSetting.platformPlacementInterval.Evaluate(level.Index))
        {
            platformRndCrit = Random.Range(0.0f, 1.0f);

            if (level.ChannelCount == 1)
            {
                _platformFactory.Create(ConstructPlatformState(r, 90.0f, IsTransient(level, transientRndCrit), IsColorChanger(level, colorChangeRndCrit)), level);

                if (ShouldTerminateProceduralGeneration(level))
                {
                    return;
                }
            }
            else
            {
                for (var channel = 0; channel < level.ChannelCount; channel++)
                {
                    var theta = 90.0f - level.Span / 2.0f + (2.0f * (channel + 1) - 1) * channelSpan / 2.0f;

                    if (Random.Range(0.0f, 1.0f) > platformRndCrit)
                    {
                        _platformFactory.Create(ConstructPlatformState(r, theta, IsTransient(level, transientRndCrit), IsColorChanger(level, colorChangeRndCrit)), level);
                        platformRndCrit = Mathf.Clamp01(platformRndCrit + 1.0f / (level.ChannelCount - 1) * Mathf.Lerp(1.5f, 0.5f, _levelSetting.platformAbundance.Evaluate(level.Index)));
                    }
                    else
                    {
                        if (Mathf.Abs(theta - 90.0f) > 5.0f && Random.Range(0.0f, 1.0f) > obstacleRndCrit)
                        {
                            _obstacleFactory.Create(ConstructObstacleState(r, theta), level);
                            obstacleRndCrit = Mathf.Clamp01(obstacleRndCrit + 0.3f * Mathf.Lerp(1.5f, 0.5f, _levelSetting.obstacleAbundance.Evaluate(level.Index)));
                        }
                        else
                        {
                            obstacleRndCrit = Mathf.Clamp01(obstacleRndCrit - 3.0f * Mathf.Lerp(0.5f, 1.5f, _levelSetting.obstacleAbundance.Evaluate(level.Index)));
                        }

                        platformRndCrit = Mathf.Clamp01(platformRndCrit - 1.0f / (level.ChannelCount - 1));
                    }

                    if (ShouldTerminateProceduralGeneration(level))
                    {
                        return;
                    }
                }
            }
        }
    }

    private bool IsTransient(Level level, float rndCrit)
    {
        var transient = Random.Range(0.0f, 1.0f) > rndCrit;

        if (level.Platforms.Count(p => p.Transient) > _levelSetting.MaxTransientPlatforms)
        {
            transient = false;
        }

        return transient;
    }

    private bool IsColorChanger(Level level, float rndCrit)
    {
        var colorChanger = Random.Range(0.0f, 1.0f) > rndCrit;

        if (level.Platforms.Count < 3)
        {
            colorChanger = true;
        }

        if (level.Platforms.Count(p => p.ColorChanger) > _levelSetting.MaxColorChangerPlatforms)
        {
            colorChanger = false;
        }

        return colorChanger;
    }

    private bool ShouldTerminateProceduralGeneration(Level level)
    {
        return level.Platforms.Count(p => !p.ColorChanger) >= _levelSetting.MaxSolidPlatforms;
    }

    private static Platform.State ConstructPlatformState(float r, float theta, bool transient, bool colorChanger)
    {
        Platform.State platformState;

        platformState.radius = r;
        platformState.theta = theta;
        platformState.transient = transient;
        platformState.colorChanger = colorChanger;

        return platformState;
    }

    private static Obstacle.State ConstructObstacleState(float r, float theta)
    {
        Obstacle.State obstacleState;

        obstacleState.radius = r;
        obstacleState.theta = theta;

        return obstacleState;
    }
}

