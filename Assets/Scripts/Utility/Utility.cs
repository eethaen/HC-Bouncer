using UnityEngine;

public static class Utility
{
    public static int DetermineSegmentByPosition(Level level, Vector3 localPosition)
    {
        CustomDebug.Log($"local position {localPosition}");
        return DetermineSegmentByAngle(level, Vector3.SignedAngle(level.Transform.right, localPosition, level.Transform.forward));
    }

    public static int DetermineSegmentByAngle(Level level, float angle)
    {
        angle = angle - 90.0f + level.SegmentSpan / 2.0f;

        if (angle < 0.0f)
        {
            angle += 360.0f;
        }

        return (int)(angle / level.SegmentSpan);
    }
}
