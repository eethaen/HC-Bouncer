using UnityEngine;

[CreateAssetMenu(fileName = "VFX Setting", menuName = "Setting/VFX", order = 1)]
public class VFXSetting : ScriptableObject
{
    public BallHitPlatformSetting OnBallHitPlatformSetting;
    public BallHitOrbSetting OnBallHitOrbSetting;

    [System.Serializable]
    public class BallHitPlatformSetting
    {
        [Space]
        [Header("On Hit Displacement Properties")]
        [Space]
        public float onHitDisplacementDuration;
        public float onHitDisplacement;
        public AnimationCurve onHitOutboardDisplacementCurve;
        public AnimationCurve onHitInboardDisplacementCurve;

        [Space]
        [Header("On Hit Color Shift FX Properties")]
        [Space]
        public float onHitColorShiftFXDuration;
        public AnimationCurve onHitColorShiftCurve;

        [Space]
        [Header("On Hit Glow FX Properties")]
        [Space]
        public float onHitGlowFXDuration;
        public float onHitGlowFXStartAlpha;
        public float onHitGlowFXTargetAlpha;
        public AnimationCurve onHitGlowFadeCurve;
        public float onHitGlowFXStartScale;
        public float onHitGlowFXTargetScale;
        public AnimationCurve onHitGlowScaleCurve;

        [Space]
        [Header("On Hit Hollow Expand FX Properties")]
        [Space]
        public float onHitHollowExpandFXDuration;
        public float onHitHollowExpandFXStartAlpha;
        public float onHitHollowExpandFXTargetAlpha;
        public AnimationCurve onHitHollowExpandFadeCurve;
        public float onHitHollowExpandFXStartScale;
        public float onHitHollowExpandFXTargetScale;
        public AnimationCurve onHitHollowExpandScaleCurve;

       // [Space]
    }

    [System.Serializable]
    public class BallHitOrbSetting
    {

    }
}

