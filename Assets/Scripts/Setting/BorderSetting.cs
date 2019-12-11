using UnityEngine;

[CreateAssetMenu(fileName = "Border Setting", menuName = "Setting/Border", order = 5)]
public class BorderSetting : ScriptableObject
{
    public Border prefab;
    internal float minWidth;
    internal float maxWidth;
}
