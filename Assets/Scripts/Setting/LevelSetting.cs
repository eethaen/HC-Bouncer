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
    public float ObstaclePlatformMinDistance = 1.0f;
    public float ObstaclePlatformMaxDistance = 3.0f;
}
