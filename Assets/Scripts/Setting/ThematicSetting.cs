using UnityEngine;

[CreateAssetMenu(fileName = "Thematic Setting", menuName = "Setting/Thematic", order = 1)]
public class ThematicSetting : ScriptableObject
{
    public Color[] platformColorSequence;
    public ColorPallete[] ChapterPalletes;

    [System.Serializable]
    public struct ColorPallete
    {
        public Color backgroundColor_A;
        public Color backgroundColor_B;
        public Color borderColor;
        public Color platformColor_A;
        public Color platformColor_B;
        public Color ballColor;
        public Color trailColor;
        public Color fontColor;
    }
}
