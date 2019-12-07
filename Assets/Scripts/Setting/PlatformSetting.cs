using UnityEngine;

[CreateAssetMenu(fileName = "Platform Setting", menuName = "Setting/Platform", order = 4)]
public class PlatformSetting : ScriptableObject
{
    [Header("General Properties")]
    [Space]
    public Platform[] prefabs;

    [Tooltip("* Vertical Axis: Platform min length in world units *\n* Horizontal Axis: Level index *")]
    public AnimationCurve minLengthCurve;

    [Tooltip("* Vertical Axis: Platform max length in world units *\n* Horizontal Axis: Level index *")]
    public AnimationCurve maxLengthCurve;

    [Tooltip("Determines how fast platform reorients itself towards up\n0.1: fast 1: lazy")]
    public float customTickInterval;

    [Space]
    [Header("Rotate Upward Properties")]
    [Space]
    public AnimationCurve rotateCurve;
    public float TransientColorChangeInterval=5.0f;
}
