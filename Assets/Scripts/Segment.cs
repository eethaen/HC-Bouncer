using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class Segment : MonoBehaviour
{
    public Transform Transform { get; private set; }
    public List<Platform> Platforms { get; set; }
    public int Index { get; private set; }
    public bool Revealed { get; internal set; }

    [Inject]
    public void Construct()
    {
        Transform = transform;
        Platforms = new List<Platform>();
    }

    public void Init(int index)
    {
        Index = index;
        Revealed = false;
    }

    public class Factory : PlaceholderFactory<int, Level, Segment>
    {
    }
}

public class SegmentFactory : IFactory<int, Level, Segment>
{
    private readonly DiContainer _container;
    private readonly World _world;
    private readonly MainSetting _mainSetting;
    private readonly ThematicSetting _thematicSetting;
    private readonly LevelSetting _levelSetting;
    private readonly SegmentSetting _segmentSetting;
    private readonly Platform.Factory _platfromFactory;
    private readonly Border.Factory _borderFactory;
    private readonly Orb.Factory _orbFactory;
    private Segment _instance;

    public SegmentFactory(DiContainer container, World world, MainSetting mainSetting,ThematicSetting thematicSetting, LevelSetting levelSetting, SegmentSetting segmentSettings, Platform.Factory platfromFactory, Border.Factory borderFactory, Orb.Factory orbFactory)
    {
        _container = container;
        _world = world;
        _mainSetting = mainSetting;
        _thematicSetting = thematicSetting;
        _levelSetting = levelSetting;
        _segmentSetting = segmentSettings;
        _platfromFactory = platfromFactory;
        _borderFactory = borderFactory;
        _orbFactory = orbFactory;
    }

    public Segment Create(int segment, Level level)
    {
        var span = level.SegmentSpan;

        _instance = _container.InstantiatePrefabForComponent<Segment>(_segmentSetting.prefab, level.Transform);

        _instance.Init(segment);
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, segment * span);

        if (segment == 0)
        {
            ConstructBall(level);
            ConstructTrail(level);
        }
        else if (segment == level.SegmentCount - 1)
        {
            CustomDebug.Log($"Creating end point in segment {segment}");
            level.EndPoint = _container.InstantiatePrefabForComponent<EndPoint>(_levelSetting.endPointPrefab, _instance.Transform);

            var rnd = Random.Range(0.0f, 1.0f);
            var r = _levelSetting.coreRadius + Mathf.Lerp(_segmentSetting.nearestPlatformOffset, _segmentSetting.farthestPlatformOffset, rnd);
            var theta = 90.0f - span / 2.0f + Mathf.Lerp(0.25f * span, 0.75f * span, rnd);

            level.EndPoint.Transform.localPosition = new Vector2(r * Mathf.Cos(theta * Mathf.Deg2Rad), r * Mathf.Sin(theta * Mathf.Deg2Rad));
        }

        ConstructBorders(span, _instance, level);
        ConstructPlatforms(span, _instance, level);
        ConstructOrbs(span, _instance, level);

        return _instance;
    }

    private void ConstructTrail(Level level)
    {
        level.Trail = _container.InstantiatePrefabForComponent<Trail>(_levelSetting.trailPrefab, _instance.Transform);
    }

    private void ConstructBall(Level level)
    {
        level.Ball = _container.InstantiatePrefabForComponent<Ball>(_levelSetting.ballPrefab, _world.Transform);
        level.Ball.Transform.position += _instance.Transform.up * (_levelSetting.coreRadius + _levelSetting.spawnPoinOffset);
    }

    private void ConstructOrbs(float span, Segment instance, Level level)
    {
        var rnd = Random.Range(0.0f, 1.0f);

        if (rnd > 0.7f)
        {
            var rnd2 = Random.Range(0.0f, 1.0f);
            var theta = 90.0f - span / 2.0f + Mathf.Lerp(0.25f * span, 0.75f * span, rnd2);
            var r = _levelSetting.coreRadius + Mathf.Lerp(0.5f * _segmentSetting.farthestPlatformOffset, 0.85f * _segmentSetting.farthestPlatformOffset, rnd2);
            _orbFactory.Create(r, theta, instance.Transform, level.Index);
            CustomDebug.Log("Orb Created");
        }
    }

    private void ConstructBorders(float span, Segment segment, Level level)
    {
        _borderFactory.Create(-span / 2.0f, segment.Index == 0, _instance.Transform, level.Index);
    }

    private void ConstructPlatforms(float span, Segment segment, Level level)
    {
        var channelCount = (int)_segmentSetting.channelCountCurve.Evaluate(span);
        var channelSpan = span / channelCount;

        CustomDebug.Log($"Creating level {level} with span {span}, and {channelCount} channels");

        var rndCrit = _segmentSetting.rndCriterionSeed.Evaluate(level.Index);

        float maxLength;
        float theta;
        float rnd;

        Platform platform = null;

        for (var r = _levelSetting.coreRadius + _segmentSetting.nearestPlatformOffset; r <= _levelSetting.coreRadius + _segmentSetting.farthestPlatformOffset; r += _segmentSetting.platformPlacementInterval)
        {
            for (var channel = 0; channel < channelCount; channel++)
            {
                maxLength = r * channelSpan * Mathf.Deg2Rad;
                theta = 90.0f - span / 2.0f + (2.0f * (channel + 1) - 1) * channelSpan / 2.0f;
                rnd = Random.Range(0.0f, 1.0f);

                if (rnd > rndCrit)
                {
                    platform = _platfromFactory.Create(r /** Mathf.Lerp(0.7f, 1.0f, rnd)*/, theta, maxLength, _instance.Transform, level.Index);
                    segment.Platforms.Add(platform);
                    platform.Init(_thematicSetting.ChapterPalletes[level.Index / _mainSetting.levelsPerChapter].platformColor_A, _thematicSetting.ChapterPalletes[level.Index / _mainSetting.levelsPerChapter].platformColor_B);

                    if (channelCount == 1)
                    {
                        rndCrit = 0.0f;
                    }
                    else
                    {
                        rndCrit = Mathf.Clamp01(rndCrit + 1.0f / (channelCount - 1) * Mathf.Lerp(0.9f, 1.1f, rnd));
                    }
                }
                else
                {
                    if (channelCount == 1)
                    {
                        rndCrit = 0.0f;
                    }
                    else
                    {
                        rndCrit = Mathf.Clamp01(rndCrit - 1.0f / (channelCount - 1) * Mathf.Lerp(0.9f, 1.2f, rnd));
                    }
                }

                //CustomDebug.Log($"rndCrit {rndCrit}");
            }
        }
    }
}
