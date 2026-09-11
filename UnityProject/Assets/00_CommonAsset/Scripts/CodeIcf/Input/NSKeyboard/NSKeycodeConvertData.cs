using UnityEngine.InputSystem;

namespace CodeIcf.Input.NSKeyboard
{
    public class NSKeycodeConvertData
    {
        public Key UnityKeyCode { get; private set; } = Key.None;
        public int SwichKeyCode { get; private set; } = -1;

        public NSKeycodeConvertData( Key _unityKeyCode, int _swichKeyCode )
        {
            UnityKeyCode = _unityKeyCode;
            SwichKeyCode = _swichKeyCode;
        }
    }
}