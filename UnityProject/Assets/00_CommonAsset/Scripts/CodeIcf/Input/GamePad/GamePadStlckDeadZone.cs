using UnityEngine;

namespace CodeIcf.Input.GamePad
{
    public class GamePadStickDeadZone
    {
        public const float DEAD_ZONE_DEFAULT_VALUE = 0.1f;
        /// <summary>Lスティック入力のデッドゾーン値</summary>
        public float LStickDeadZone { get; private set; } = DEAD_ZONE_DEFAULT_VALUE;
        /// <summary>Rスティック入力のデッドゾーン値</summary>
        public float RStickDeadZone { get; private set; } = DEAD_ZONE_DEFAULT_VALUE;

        public void ResetDefault()
        {
            LStickDeadZone = DEAD_ZONE_DEFAULT_VALUE;
            RStickDeadZone = DEAD_ZONE_DEFAULT_VALUE;
        }

        public void Update(float lStick, float rStick)
        {
            LStickDeadZone = lStick;
            RStickDeadZone = rStick;
        }

        public static float CheckDeadZone(float input, float deadZone)
        {
            return Mathf.Abs(input) <= deadZone ? 0 : input;
        }
    }
}