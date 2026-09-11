using System.Runtime.InteropServices;
using CodeIcf.Extensions;
using CodeIcf.Input.Keyboard;
using UnityEngine;

namespace CodeIcf.Input.NSKeyboard
{
    public class NSKeyboardData
    {
        private const int KEYS_SIZE = 8;
        public const int INT_BYTE_SIZE = 32;

        /// <summary>マウスの状態</summary>
        public int Attribute { get; private set; } = 0;
        public int[] Keys { get; private set; } = null;
        public int Modifier { get; private set; } = 0;

        public KeyboardKeyState[] KeyStateList { get; private set; } = null;

        /// <summary> キーボードが接続状態かののビットフラグ</summary>
        private const byte ATTRIBUTE_IS_CONNECTED = 0x1;

        /// <summary>キーボードが接続状態か</summary>
        public bool IsConnected => Attribute.EqualLogicAnd( ATTRIBUTE_IS_CONNECTED );

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NSKeyboardData()
        {
            Keys = new int[ KEYS_SIZE ];
            KeyStateList = new KeyboardKeyState[ KEYS_SIZE * INT_BYTE_SIZE ];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_nowState"></param>
        /// <param name="_prev"></param>
        public NSKeyboardData( NSKeyboatdState _nowState, NSKeyboardData _prev )
        {
            Attribute = _nowState.Attribute;
            Modifier = _nowState.Modifier;
            Keys = new int[ KEYS_SIZE ];

            Marshal.Copy( _nowState.KeysPtr, Keys, 0, KEYS_SIZE );

            if( IsConnected )
            {
                KeyStateList = new KeyboardKeyState[ KEYS_SIZE * INT_BYTE_SIZE ];

                for( int i = 0; i < KeyStateList.Length; i++ )
                {
                    int index = i / INT_BYTE_SIZE;
                    int bitShift = i % INT_BYTE_SIZE;
                    int targetBit = 0x1 << bitShift;

                    KeyStateList[ i ] = GetKeyState( Keys[ index ].EqualLogicAnd( targetBit ), _prev.Keys[ index ].EqualLogicAnd( targetBit ) );
                }
            }
        }

        /// <summary>
        /// ボタンの状態を取得
        /// </summary>
        /// <param name="_now"></param>
        /// <param name="_prev"></param>
        /// <returns></returns>
        private KeyboardKeyState GetKeyState( bool _now, bool _prev )
        {
            KeyboardKeyState result = KeyboardKeyState.None;

            if( _now )
            {
                result = _prev ? KeyboardKeyState.Hold : KeyboardKeyState.Down;
            }
            else
            {
                result = _prev ? KeyboardKeyState.Up : KeyboardKeyState.None;
            }

            return result;
        }
    }
}