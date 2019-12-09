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
    public int DesiredPlatformCount { get; private set; }
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

    public void Init(int index, float span, int platformCount, int channelCount)
    {
        Index = index;
        Span = span;
        DesiredPlatformCount = platformCount;
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
        instance.Init(index, 360.0f / _mainSetting.segmentCount, (int)_levelSetting.platformCountCurve.Evaluate(index), (int)_levelSetting.channelCountCurve.Evaluate(360.0f / _mainSetting.segmentCount));

        instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, instance.Span * instance.Index);

        ConstructPlatformsAndObstacles(instance);
        //ConstructObstacles(instance);

        CustomDebug.Log($"Creating level {instance} with span {instance.Span}, and {instance.ChannelCount} channels");

        return instance;
    }

    private void ConstructPlatformsAndObstacles(Level level)
    {
        var desiredPlatformPercentage = _levelSetting.platformPercentageCurve.Evaluate(level.Index);
        var desiredColorChangerPercentage = _levelSetting.colorChangerPlatformPercentageCurve.Evaluate(level.Index);
        var desiredTransienPercentage = _levelSetting.transientPlatformPercentageCurve.Evaluate(level.Index);
        var obstacleChance = _levelSetting.obstacleChance.Evaluate(level.Index);

        var channelCount = level.ChannelCount;
        var rowCount = Mathf.CeilToInt(level.DesiredPlatformCount / desiredPlatformPercentage / channelCount);
        var cellCount = channelCount * rowCount;

        CustomDebug.Log($"DesiredPlatformCount: {level.DesiredPlatformCount}, rowCount: {rowCount}");

        var channelSpan = level.ChannelSpan;

        var closeRadius = _mainSetting.coreRadius + _levelSetting.nearestPlatformOffset;
        var farRadius = closeRadius + _mainSetting.jumpHeight * rowCount;
        var radialInterval = _mainSetting.jumpHeight * 0.5f;

        var occupiedCellCoord = new List<Vector2>();

        while (level.Platforms.Count < level.DesiredPlatformCount)
        {
            var lastChannel = -1;

            for (var row = 0; row < rowCount; row++)
            {
                var channel = Random.Range(0, channelCount);

                while (channel == lastChannel && channelCount != 1)
                {
                    channel = Random.Range(0, channelCount);
                }

                var r = closeRadius + row * radialInterval;
                var theta = 90.0f - level.Span / 2.0f + (2.0f * (channel + 1) - 1) * channelSpan / 2.0f;

                if (!occupiedCellCoord.Contains(new Vector2(r, theta)))
                {
                    _platformFactory.Create(ConstructPlatformState(r, theta), level);
                    occupiedCellCoord.Add(new Vector2(r, theta));
                }
                else
                {
                    CustomDebug.Log("Skipping occupied cell");
                }

                lastChannel = channel;

                if (level.Platforms.Count >= level.DesiredPlatformCount)
                {
                    break;
                }
            }
        }

        var desiredColorChangerCount = desiredColorChangerPercentage * level.Platforms.Count;
        var lastIndex = -1;
        var index = -1;

        while (level.Platforms.Count(p => p.ColorChanger) < desiredColorChangerCount)
        {
            index = Random.Range(0, _thematicSetting.platformColorSequence.Length);

            while (index == lastIndex)
            {
                index = Random.Range(0, _thematicSetting.platformColorSequence.Length);
            }

            level.Platforms.Where(p => !p.ColorChanger).ElementAt(Random.Range(0, level.Platforms.Count(p => !p.ColorChanger))).SetAsColorChanger(index);
            lastIndex = index;
        }

        var desiredTransientCount = desiredTransienPercentage * level.Platforms.Count(p => p.ColorChanger);

        while ((float)level.Platforms.Count(p => p.Transient) / (float)level.Platforms.Count(p => p.ColorChanger) < desiredTransienPercentage)
        {
            level.Platforms.Where(p => p.ColorChanger && !p.Transient).ElementAt(Random.Range(0, level.Platforms.Count(p => p.ColorChanger && !p.Transient))).SetAsTransient();
        }

        for (var row = 0; row < rowCount; row++)
        {
            var r = closeRadius + row * radialInterval;

            for (var channelEdge = 1; channelEdge < channelCount; channelEdge++)
            {
                var theta = 90.0f - level.Span / 2.0f + channelEdge * channelSpan;

                if (Random.Range(0.0f, 1.0f) < obstacleChance)
                {
                    _obstacleFactory.Create(ConstructObstacleState(r, theta), level);
                }
            }
        }
    }

    private static Platform.Coord ConstructPlatformState(float r, float theta)
    {
        Platform.Coord platformState;

        platformState.radius = r;
        platformState.theta = theta;

        return platformState;
    }

    private static Obstacle.Coord ConstructObstacleState(float r, float theta)
    {
        Obstacle.Coord obstacleState;

        obstacleState.radius = r;
        obstacleState.theta = theta;

        return obstacleState;
    }
}

