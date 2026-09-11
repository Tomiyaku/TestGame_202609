//Nitendo Switchでマウスを使用する場合は下記のプリプロセッサを有効にする
//#define USE_MOUSE_FOR_NINTENDO_SWITCH

using System;
using System.Runtime.InteropServices;

namespace CodeIcf.Input.Mouse
{
    /// <summary>
    /// マウス管理クラス
    /// </summary>
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
#if !UNITY_EDITOR && USE_MOUSE_FOR_NINTENDO_SWITCH
                    nn_mouse_Initialize();
#endif
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// マウスの入力状態を取得
        /// </summary>
        /// <returns>マウスの入力状態</returns>
        public NSMouseState GetMouseState()
        {
            NSMouseState mouseState = new NSMouseState();

#if !UNITY_EDITOR && USE_MOUSE_FOR_NINTENDO_SWITCH
            nn_mouce_GetState( ref mouseState );
#endif
            return mouseState;
        }

        /// <summary>
        /// マウスの入力状態を過去に遡って取得
        /// </summary>
        /// <param name="_outValue">入力状態を読み出すバッファ</param>
        /// <param name="_count">バッファのサイズ</param>
        /// <returns>実際に取得した数</returns>
        public int GetMouseStates( ref NSMouseState[] _outValue, int _count )
        {
            int result = 0;
#if !UNITY_EDITOR && USE_MOUSE_FOR_NINTENDO_SWITCH
            _outValue = new NSMouseState[ _count ];

            GCHandle handle = GCHandle.Alloc( _outValue, GCHandleType.Pinned );
            IntPtr ptr = handle.AddrOfPinnedObject();

            result = nn_mouce_GetStates( ptr, _count );

            handle.Free();
#else
            _outValue = null;
#endif
            return result;

        }

#if !UNITY_EDITOR && USE_MOUSE_FOR_NINTENDO_SWITCH
        [ DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_mouse_Initialize();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_mouce_GetState( ref NSMouseState _pOutValue );
        
        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern int nn_mouce_GetStates( IntPtr _pOutValue, int _count );
#endif
    }
}
