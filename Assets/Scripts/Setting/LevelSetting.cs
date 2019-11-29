using UnityEngine;

[CreateAssetMenu(fileName = "Level Setting", menuName = "Setting/Level", order = 2)]
public class LevelSetting : ScriptableObject
{
    public Level levelPrefab;
    public Ball ballPrefab;
    public Trail trailPrefab;
    public EndPoint endPointPrefab;

    [Tooltip("The circle radius bounding the core")]
    public float coreRadius;

    [Tooltip("* Vertical Axis: Segment Count *\n* Horizontal Axis: Level index *")]
    public AnimationCurve segmentCount;

    [Tooltip("The initial spawn point offset from core surfcae")]
    public float spawnPoinOffset = 1.0f;
}
