using UnityEngine;

[CreateAssetMenu(fileName = "Orb Setting", menuName = "Setting/Orb", order = 7)]
public class OrbSetting : ScriptableObject
{
    public Orb prefab;
    public float rotationalSpeed = 45f;
}
