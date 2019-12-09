using UnityEngine;

[CreateAssetMenu(fileName = "Level Setting", menuName = "Setting/Level", order = 2)]
public class LevelSetting : ScriptableObject
{
    public Level levelPrefab;

    public AnimationCurve platformCountCurve;
    public AnimationCurve platformPercentageCurve;
    public AnimationCurve obstacleChance;
    public AnimationCurve colorChangerPlatformPercentageCurve;
    public AnimationCurve transientPlatformPercentageCurve;
    [Tooltip("* Vertical Axis: Channel count per level *\n* Horizontal Axis: level span*")]
    public AnimationCurve channelCountCurve;
    [Tooltip("")]
    public float nearestPlatformOffset = 1.0f;
    public float ObstaclePlatformAllowableDistance = 1.0f;
}
