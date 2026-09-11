#if UNITY_SWITCH && !UNITY_EDITOR
#define ENABLE_SWITCH_PARENTAL_CONTROL
#endif

#if ENABLE_SWITCH_PARENTAL_CONTROL
using System.Runtime.InteropServices;
#endif

namespace CodeIcf.Switch
{
    /// <summary>
    /// Nintendo Switchでのペアレンタルコントロール関連のAPIを呼び出すためのクラス
    /// </summary>
    public static class NSParentalControl
    {
        /// <summary>
        /// 「他の人との自由なコミュニケーション」機能の利用開始を試みます。
        /// </summary>
        /// <param name="isShowUi">「他の人との自由なコミュニケーション」機能の利用が許可されなかった場合に、関数内部でその旨を表示するかどうかを表す</param>
        /// <returns>「他の人との自由なコミュニケーション」機能が利用可能であれば true、制限されており利用できない場合は false</returns>
        public static bool TryBeginFreeCommunication( bool isShowUi )
        {
#if ENABLE_SWITCH_PARENTAL_CONTROL
            return nn_pctl_TryBeginFreeCommunication( isShowUi );
#else
            return true;
#endif
        }

        /// <summary>
        /// 「他の人との自由なコミュニケーション」機能の利用を終了します。
        /// </summary>
        public static void EndFreeCommunication()
        {
#if ENABLE_SWITCH_PARENTAL_CONTROL
            nn_pctl_EndFreeCommunication();
#endif
        }

        /// <summary>
        /// 「他の人との自由なコミュニケーション」機能が利用可能か否かを判定します。
        /// </summary>
        /// <returns>「他の人との自由なコミュニケーション」機能が利用可能であれば true、不可能であれば false を返します。</returns>
        public static bool IsFreeCommunicationAvailable()
        {
#if ENABLE_SWITCH_PARENTAL_CONTROL
            return nn_pctl_IsFreeCommunicationAvailable();
#else
            return true;
#endif
        }

#if ENABLE_SWITCH_PARENTAL_CONTROL
        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool nn_pctl_TryBeginFreeCommunication( bool isShowUi );

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern void nn_pctl_EndFreeCommunication();

        [DllImport( "__Internal", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool nn_pctl_IsFreeCommunicationAvailable();
#endif
    }
}