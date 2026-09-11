#if !UNITY_EDITOR && UNITY_SWITCH
//Nitendo Switchでマウスを使用する場合は下記のプリプロセッサを有効にする
#define USE_MOUSE_FOR_NINTENDO_SWITCH
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CodeIcf.Input.NSMouse
{
    /// <summary>
    /// Nintendo Switch専用マウス入力管理クラス
    /// </summary>
    /// <remarks>Nintendo Switch以外の場合、マウス(ポインティングデバイス)は<see cref="CodeIcf.Input.PointerDevice.PointerData"/>に含まれる</remarks>
    public class NSMouseManager
    {
        /// <summary>インスタンス</summary>
        private static NSMouseManager m_Instance = null;
        /// <summary>インスタンス</summary>
        /// <remarks>呼び出し時にインスタンスがない場合に初期化も同時の行う</remarks>
        public static NSMouseManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    m_Instance = new NSMouseManager();
#if USE_MOUSE_FOR_NINTENDO_SWITCH
                    nn_mouse_Initialize();
#endif
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// マウスの情報の履歴
        /// </summary>
        private List<NSMouseData> m_MouseDataHIstroyList = new List<NSMouseData>();

        /// <summary>
        /// 最新のマウスの情報を取得
        /// </summary>
        public NSMouseData NowData => m_MouseDataHIstroyList.Count <= 0 ? new NSMouseData() : m_MouseDataHIstroyList[ 0 ];

        /// <summary>
        /// マウスの情報を更新
        /// </summary>
        public void UpdateMouseData()
        {
            NSMouseState mouseState = GetMouseState();
            NSMouseData mouseData = new NSMouseData( mouseState, NowData );

            m_MouseDataHIstroyList.Insert( 0, mouseData );

            if( m_MouseDataHIstroyList.Count > InputDeviceManager.HistoryMaxSize )
            {
                m_MouseDataHIstroyList.RemoveRange( InputDeviceManager.HistoryMaxSize, m_MouseDataHIstroyList.Count - InputDeviceManager.HistoryMaxSize );
            }
        }

        /// <summary>
        /// マウスの入力状態を取得
        /// </summary>
        /// <returns>マウスの入力状態</returns>
        private NSMouseState GetMouseState()
        {
            NSMouseState mouseState = new NSMouseState();

#if USE_MOUSE_FOR_NINTENDO_SWITCH
            nn_mouce_GetState( ref mouseState );
#endif
            return mouseState;
        }

        /// <summary>
        /// マウスの入力状態を過去に遡って取得
        /// </summary>
        /// <param name="_refMousState">入力状態を読み出すバッファ</param>
        /// <param name="_count">バッファのサイズ</param>
        /// <returns>実際に取得した数</returns>
        private int GetMouseStates( ref NSMouseState[] _refMousState, int _count )
        {
            int result = 0;
#if USE_MOUSE_FOR_NINTENDO_SWITCH
            _refMousState = new NSMouseState[ _count ];

            GCHandle handle = GCHandle.Alloc( _refMousState, GCHandleType.Pinned );
            IntPtr ptr = handle.AddrOfPinnedObject();

            result = nn_mouce_GetStates( ptr, _count );

            handle.Free();
#else
            _refMousState = null;
#endif
            return result;

        }

#if USE_MOUSE_FOR_NINTENDO_SWITCH
        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_mouse_Initialize();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_mouce_GetState( ref NSMouseState _pOutValue );
        
        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern int nn_mouce_GetStates( IntPtr _pOutValue, int _count );
#endif
    }
}
