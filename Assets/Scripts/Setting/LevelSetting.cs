using UnityEngine;

[CreateAssetMenu(fileName = "Level Setting", menuName = "Setting/Level", order = 2)]
public class LevelSetting : ScriptableObject
{
    public Level levelPrefab;

    [Tooltip("* Vertical Axis: Channel count per level *\n* Horizontal Axis: level index*")]
    public AnimationCurve channelCountCurve;

    [Tooltip("")]
    public float nearestPlatformOffset;

    [Tooltip("* Vertical Axis: Farthest platform placement offset from core surface *\n* Horizontal Axis: Level index *")]
    public AnimationCurve farthestPlatformOffset;

    [Tooltip("* Vertical Axis: Platform placement interval *\n* Horizontal Axis: Level index *")]
    public AnimationCurve platformPlacementInterval;

    [Tooltip("* Vertical Axis: Platform abundance index (0:less abundant 1:More abundant) *\n* Horizontal Axis: Level index *")]
    public AnimationCurve platformAbundance;

    [Tooltip("* Vertical Axis: Obstacle abundance index (0:less abundant 1:More abundant) *\n* Horizontal Axis: Level index *")]
    public AnimationCurve obstacleAbundance;
    public float platfromPlatformAllowableDistance = 1.0f;
    public float platfromObstacleAllowableDistance=2.0f;
    public float obstacleObstacleAllowableDistance=2.0f;

    public int MaxTransientPlatforms = 2;
    internal int MaxColorChangerPlatforms=3;
    internal int MaxSolidPlatforms=3;
}
