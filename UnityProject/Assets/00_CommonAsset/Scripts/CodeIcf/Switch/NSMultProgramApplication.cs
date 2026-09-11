#if UNITY_SWITCH && !UNITY_EDITOR
#define ENABLE_MULTI_PROGRAM_APPLICATION
#endif

#if UNITY_SWITCH
using System;
using nn.account;
#endif

#if ENABLE_MULTI_PROGRAM_APPLICATION
using System.Runtime.InteropServices;
#endif

namespace CodeIcf.Switch
{
    /// <summary>
    /// Nintendo Switchのマルチプログラムアプリケーション関連のAPIを実行するためのクラス
    /// </summary>
    public static class NSMultProgramApplication
    {
        /// <summary>
        /// 現在のプログラムを終了し、指定したプログラムを起動します。
        /// </summary>
        /// <param name="prigramIndex">移行対象のプログラムのインデックス</param>
        /// <param name="databuffer">受け渡すデータ</param>
        /// <param name="dataSize">受け渡すデータのサイズ</param>
        public static bool ExecuteProgram( int prigramIndex, byte[] databuffer = null, int dataSize = 0 )
        {
            if( prigramIndex < 0 || prigramIndex > 15 )
            {
                return false;
            }

            if( ( databuffer != null && databuffer.Length > 4096 ) || dataSize > 4096 )
            {
                return false;
            }

#if ENABLE_MULTI_PROGRAM_APPLICATION
            GCHandle handle = GCHandle.Alloc( databuffer, GCHandleType.Pinned );
            IntPtr ptr = handle.AddrOfPinnedObject();

            nn_oe_ExecuteProgram( prigramIndex, ptr, dataSize );
#endif
            return true;
        }

        /// <summary>
        /// 直前に走っていたプログラムのインデックスを所得
        /// </summary>
        /// <returns>直前に走っていたプログラムのインデックス 最初に起動したプログラムの場合は-1</returns>
        public static int GetPreviousProgramIndex()
        {
#if ENABLE_MULTI_PROGRAM_APPLICATION
            return nn_oe_GetPreviousProgramIndex();
#else
            return 0;
#endif
        }

        public static bool TryPopLaunchParameter( ref int refParamSize, ref byte[] refBuffer, int bufferSize )
        {

#if ENABLE_MULTI_PROGRAM_APPLICATION
            GCHandle handle = GCHandle.Alloc( refBuffer, GCHandleType.Pinned );
            IntPtr ptr = handle.AddrOfPinnedObject();

            return nn_oe_TryPopLaunchParameter( ref refParamSize, ptr, bufferSize );            
#else 
            return false;
#endif
        }

        /// <summary>
        /// Open状態のユーザーアカウントを、状態を維持したまま異なるプログラム間で利用可能にします。
        /// </summary>
        public static void PushOpenUsers()
        {
#if ENABLE_MULTI_PROGRAM_APPLICATION
            nn_acount_PushOpenUsers();
#endif
        }

        /// <summary>
        /// プログラム間でOpen状態を維持しているユーザーアカウントの UserHandle を取得します。
        /// </summary>
        /// <param name="refOut">取得したユーザーハンドルの個数</param>
        /// <param name="refHandles">取得したユーザーハンドル</param>
        /// <param name="handleCount">_refHandleの要素数</param>
#if UNITY_SWITCH
        public static void PopOpenUsers( ref int refOut, ref UserHandle[] refHandles, int handleCount )
        {
#if ENABLE_MULTI_PROGRAM_APPLICATION
            GCHandle handle = GCHandle.Alloc( refHandles, GCHandleType.Pinned );
            IntPtr ptr = handle.AddrOfPinnedObject();

            nn_acount_PopOpenUsers( ref refOut, ptr, handleCount );
#endif
        }
#endif

#if ENABLE_MULTI_PROGRAM_APPLICATION
        [ DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_oe_ExecuteProgram( int programIndex, IntPtr data, int dataSize);

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern int nn_oe_GetPreviousProgramIndex();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool nn_oe_TryPopLaunchParameter( ref int pOutParameterSize, IntPtr pOutBuffer, int bufferSize );

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_acount_PushOpenUsers();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_acount_PopOpenUsers( ref int refOut, IntPtr outHandles, int handleCount );
#endif
    }
}