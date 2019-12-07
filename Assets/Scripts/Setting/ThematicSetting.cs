using UnityEngine;

[CreateAssetMenu(fileName = "Thematic Setting", menuName = "Setting/Thematic", order = 1)]
public class ThematicSetting : ScriptableObject
{
    public ColorDuo[] platformColorSequence;
    public ColorPallete[] ChapterPalletes;

    [System.Serializable]
    public struct ColorPallete
    {
        public ColorDuo backgroundColor;
        public ColorDuo platformColor;
        public Color borderColor;
        public Color ballColor;
        public Color trailColor;
        public Color fontColor;
    }

    [System.Serializable]
    public struct ColorDuo
    {
        public Color colorA;
        public Color colorB;
    }
}
