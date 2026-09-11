#if !UNITY_EDITOR && UNITY_SWITCH
//Nitendo Switchでマウスを使用する場合は下記のプリプロセッサを有効にする
#define USE_KEYBOARD_FOR_NINTENDO_SWITCH
#endif

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;
using CodeIcf.Input.Keyboard;

namespace CodeIcf.Input.NSKeyboard
{
    public class NSKeyboardManager
    {
        /// <summary>インスタンス</summary>
        private static NSKeyboardManager m_Instance = null;
        /// <summary>インスタンス</summary>
        /// <remarks>呼び出し時にインスタンスがない場合に初期化も同時の行う</remarks>
        public static NSKeyboardManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    m_Instance = new NSKeyboardManager();
#if USE_KEYBOARD_FOR_NINTENDO_SWITCH
                    nn_keyboard_Initialize();
#endif
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// マウスの情報の履歴
        /// </summary>
        private List<NSKeyboardData> m_KeyboardDataHIstroyList = new List<NSKeyboardData>();

        /// <summary>
        /// 最新のマウスの情報を取得
        /// </summary>
        public NSKeyboardData NowData => m_KeyboardDataHIstroyList.Count <= 0 ? new NSKeyboardData() : m_KeyboardDataHIstroyList[ 0 ];

        private List<NSKeycodeConvertData> m_KeyCodeConvertDataList = new List<NSKeycodeConvertData>();

        private NSKeyboardManager()
        {
            AddressableStorage storage = AddressableResourcesManager.Instance.GetStorage( InputSystemIdentifierDefine.SWITCH_KEYCODE_CONVERT_DATA );

            if( storage == null )
            {
                Debug.LogError( "Missing SwichKeycodeConvartData AddressableStorage" );
                return;
            }

            if( storage.State != AddressableStorage.AssetState.Enable )
            {
                Debug.LogError( "Not Enable SwichKeycodeConvartData AddressableStorage! Status : " + storage.State.ToString() );
                return;
            }

            TextAsset textAsset = storage.GetAsset<TextAsset>();
            string[][] csv = textAsset.text.SplitCsv();

            foreach( string[] dataList in csv )
            {
                Key unityKeyCode = int.TryParse( dataList[ 1 ], out int keyID ) ? keyID.ToEnum<Key>() : Key.None;
                int switchKeyCode = int.TryParse( dataList[ 2 ], out int switchKeyID ) ? switchKeyID : -1;

                NSKeycodeConvertData convertData = new NSKeycodeConvertData( unityKeyCode, switchKeyCode );

                m_KeyCodeConvertDataList.Add( convertData );
            }
        }

        /// <summary>
        /// キーボードの情報を更新
        /// </summary>
        public void UpdateKeyboardData()
        {
            NSKeyboatdState KeyboardState = GetKeyboardState();
            NSKeyboardData keyboardData = new NSKeyboardData( KeyboardState, NowData );

            m_KeyboardDataHIstroyList.Insert( 0, keyboardData );

            if( m_KeyboardDataHIstroyList.Count > InputDeviceManager.HistoryMaxSize )
            {
                m_KeyboardDataHIstroyList.RemoveRange( InputDeviceManager.HistoryMaxSize, m_KeyboardDataHIstroyList.Count - InputDeviceManager.HistoryMaxSize );
            }
        }

        /// <summary>
        /// <see cref="NSKeyboardData"/>を<see cref="KeyboardInputData"/>に変換する
        /// </summary>
        /// <returns></returns>
        public KeyboardInputData ConvertNSKeyboardDataToKeyboardInputData()
        {
            KeyboardKeyState[] keyStateList = new KeyboardKeyState[ UnityEngine.InputSystem.Keyboard.KeyCount ];

            if( NowData != null && NowData.KeyStateList != null )
            {
                for( int i = 0; i < m_KeyCodeConvertDataList.Count; i++ )
                {
                    if( m_KeyCodeConvertDataList[ i ].SwichKeyCode < 0 ) continue;
                       
                    int index = m_KeyCodeConvertDataList[ i ].UnityKeyCode.ToInt() - 1;

                    if( index < 0 || index >= keyStateList.Length ) continue;

                    keyStateList[ index ] = NowData.KeyStateList[ m_KeyCodeConvertDataList[ i ].SwichKeyCode ];
                }
            }

            return new KeyboardInputData( NowData.IsConnected, keyStateList );
        }

        /// <summary>
        /// キーボードの入力状態を取得
        /// </summary>
        /// <returns>マウスの入力状態</returns>
        private NSKeyboatdState GetKeyboardState()
        {
            NSKeyboatdState keybaordState = new NSKeyboatdState();

#if USE_KEYBOARD_FOR_NINTENDO_SWITCH
            nn_keyboard_GetState( ref keybaordState );
#endif
            return keybaordState;
        }

#if USE_KEYBOARD_FOR_NINTENDO_SWITCH
        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_keyboard_Initialize();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_keyboard_GetState( ref NSKeyboatdState _pOutValue );
#endif
    }
}