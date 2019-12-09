using UnityEngine;

[CreateAssetMenu(fileName = "Main Setting", menuName = "Setting/Main", order = 0)]
public class MainSetting : ScriptableObject
{
    public MainSetting()
    {
        var jumpTime = VelocityAfterCollision / gravity;
        jumpHeight = -0.5f * gravity * jumpTime * jumpTime + VelocityAfterCollision * jumpTime;

        CustomDebug.Log($" jumpTime: {jumpTime}, jumpHeight: {jumpHeight}");
    }

    public Trail trailPrefab;
    public Ball ballPrefab;

    public float respawnTime = 1.5f;

    [Tooltip("Maximum rotational speed of the world upon swiping")]
    public float maxRotationaSpeed = 50.0f;

    [Tooltip("Number of levels per chapter")]
    public int levelsPerChapter = 10;
    public float gravity = 10f;
    public float VelocityAfterCollision = 7.0f;
    [Tooltip("The circle radius bounding the core")]
    public float coreRadius = 3.0f;
    public int segmentCount = 5;

    [Tooltip("The initial spawn point offset from core surfcae")]
    public float spawnPoinOffset = 3.0f;

    public readonly float jumpHeight;
}
