using UnityEngine;

[CreateAssetMenu(fileName = "Segment Setting", menuName = "Setting/Segment", order = 2)]
public class SegmentSetting : ScriptableObject
{
    public Segment prefab;

    [Tooltip("* Vertical Axis: Channel count per segment *\n* Horizontal Axis: Segment span in degrees*")]
    public AnimationCurve channelCountCurve;

    [Tooltip("* Vertical Axis: Initial random criterion *\n* Horizontal Axis: Level index *")]
    public AnimationCurve rndCriterionSeed;

    [Tooltip("Nearset platform placement offset from core surface")]
    public float nearestPlatformOffset = 1.0f;

    [Tooltip("Farthest platform placement offset from core surface")]
    public float farthestPlatformOffset = 5.0f;

    [Tooltip("Platform placement interavl")]
    public float platformPlacementInterval = 1.0f;
}
