using UnityEngine;

[CreateAssetMenu(fileName = "Main Setting", menuName = "Setting/Main", order = 0)]
public class MainSetting : ScriptableObject
{
    public float respawnTime = 1.5f;

    [Tooltip("Maximum rotational speed of the world upon swiping")]
    public float maxRotationaSpeed = 50.0f;

    [Tooltip("Number of levels per chapter")]
    public int levelsPerChapter = 10;
}
