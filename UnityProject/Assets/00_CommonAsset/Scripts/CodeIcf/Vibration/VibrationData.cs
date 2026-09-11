using UnityEngine;

namespace CodeIcf.VibrationManagement
{
    [CreateAssetMenu( fileName = "VibrationData", menuName = "ScriptableObjects/Create VibrationData" )]
    public class VibrationData : ScriptableObject
    {
        /// <summary>振動の種類</summary>
        public VibrationType Type;
        /// <summary>低帯域の振幅 (最大振幅を 1.0f とする単位)</summary>
        public float AmplitudeLow;
        /// <summary>高帯域の振幅 (最大振幅を 1.0f とする単位)</summary>
        public float AmplitudeHigh;
        /// <summary>再生時間</summary>
        public float Time;
    }
}