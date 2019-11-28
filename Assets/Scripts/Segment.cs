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

    [System.Serializable]
    public class Setting
    {
        public Segment prefab;
        public AnimationCurve channelCountCurve;
        public AnimationCurve rndCriterionSeed;
        public float nearestPlatformRadius;
        public float farthestPlatformRadius;
        public float platformPlacementIntervalRadius;
    }

    public class Factory : PlaceholderFactory<int, Level, Segment>
    {
    }
}

public class SegmentFactory : IFactory<int, Level, Segment>
{
    private readonly DiContainer _container;
    private readonly World _world;
    private readonly Level.Setting _levelSetting;
    private readonly Segment.Setting _segmentSetting;
    private readonly Platform.Factory _platfromFactory;
    private readonly Border.Factory _borderFactory;
    private Segment _instance;

    public SegmentFactory(DiContainer container, World world, Level.Setting levelSetting, Segment.Setting segmentSettings, Platform.Factory platfromFactory, Border.Factory borderFactory)
    {
        _container = container;
        _world = world;
        _levelSetting = levelSetting;
        _segmentSetting = segmentSettings;
        _platfromFactory = platfromFactory;
        _borderFactory = borderFactory;
    }

    public Segment Create(int segment, Level level)
    {
        var span = level.SegmentSpan;

        _instance = _container.InstantiatePrefabForComponent<Segment>(_segmentSetting.prefab, level.Transform);

        _instance.Init(segment);
        _instance.Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, segment * span);

        if (segment == 0)
        {
            level.Ball = _container.InstantiatePrefabForComponent<Ball>(_levelSetting.ballPrefab, _world.Transform);
            level.Ball.Transform.position += _instance.Transform.up * (_levelSetting.coreRadius + _levelSetting.startPoinOffset.Evaluate(level.Index));
            level.Trail = _container.InstantiatePrefabForComponent<Trail>(_levelSetting.trailPrefab, _instance.Transform);
        }
        else if (segment == level.SegmentCount - 1)
        {
            CustomDebug.Log($"Creating end point in segment {segment}");
            level.EndPoint = _container.InstantiatePrefabForComponent<EndPoint>(_levelSetting.endPointPrefab, _instance.Transform);

            var rnd = Random.Range(0.0f, 1.0f);
            var r = Mathf.Lerp(_segmentSetting.nearestPlatformRadius, _segmentSetting.farthestPlatformRadius, rnd);
            var theta = 90.0f - span / 2.0f + Mathf.Lerp(0.25f * span, 0.75f * span, rnd);

            level.EndPoint.Transform.localPosition = new Vector2(r * Mathf.Cos(theta * Mathf.Deg2Rad), r * Mathf.Sin(theta * Mathf.Deg2Rad));
        }

        ConstructBorders(span, _instance, level);
        ConstructPlatforms(span, _instance, level);

        return _instance;
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

        for (var r = _segmentSetting.nearestPlatformRadius; r <= _segmentSetting.farthestPlatformRadius; r += _segmentSetting.platformPlacementIntervalRadius)
        {
            for (var channel = 0; channel < channelCount; channel++)
            {
                maxLength = r * channelSpan * Mathf.Deg2Rad;
                theta = 90.0f - span / 2.0f + (2.0f * (channel + 1) - 1) * channelSpan / 2.0f;
                rnd = Random.Range(0.0f, 1.0f);

                if (rnd > rndCrit)
                {
                    platform = _platfromFactory.Create(r /** Mathf.Lerp(0.9f, 1.1f, rnd)*/, theta, maxLength, _instance.Transform, level.Index);
                    segment.Platforms.Add(platform);

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
