using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using CodeIcf.Extensions;

namespace CodeIcf.Input.Keyboard
{
    /// <summary>
    /// キーボードの入力情報
    /// </summary>
    public class KeyboardInputData
    {
        public bool IsConnected { get; private set; } = false;
        /// <summary>キー状態一覧</summary>
        public KeyboardKeyState[] KeyState { get; private set; }

        public KeyboardInputData()
        {
            if( UnityEngine.InputSystem.Keyboard.current == null ) return;

            IsConnected = true;

            UnityEngine.InputSystem.Keyboard keyboardDevice = UnityEngine.InputSystem.Keyboard.current;

            KeyState = new KeyboardKeyState[ keyboardDevice.allKeys.Count ];

            for( int i = 0; i < keyboardDevice.allKeys.Count; i++ )
            {
                KeyState[ i ] = ComvertKeyControlToKeyboardKeyState( keyboardDevice.allKeys[ i ] );
            }
        }

        public KeyboardInputData( bool _isConnected, KeyboardKeyState[] _keyStateList )
        {
            IsConnected = _isConnected;
            KeyState = _keyStateList;
        }

        /// <summary>
        /// 指定のキーの状態を取得
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public KeyboardKeyState this[ Key _key ]
        {
            get
            {
                int index = _key.ToInt() - 1;

                if( index < 0 || index >= KeyState.Length ) return KeyboardKeyState.None;

                return KeyState[ index ];
            }
            set
            {
                int index = _key.ToInt() - 1;

                if( index >= 0 && index < KeyState.Length ) KeyState[ index ] = value;
            }
        }

        /// <summary>
        /// <see cref="KeyControl"/>から<see cref="KeyboardKeyState"/>へ返還
        /// </summary>
        /// <param name="_keyList"></param>
        /// <returns></returns>
        private static KeyboardKeyState ComvertKeyControlToKeyboardKeyState( KeyControl _keyControl )
        {
            if( _keyControl == null ) return KeyboardKeyState.None;

            KeyboardKeyState result = KeyboardKeyState.None;

            if( _keyControl.isPressed )
            {
                if( _keyControl.wasPressedThisFrame ) result = KeyboardKeyState.Down;
                else return KeyboardKeyState.Hold;
            }
            else if( _keyControl.wasReleasedThisFrame && result == KeyboardKeyState.None )
            {
                result = KeyboardKeyState.Up;
            }

            return result;
        }
    }
}