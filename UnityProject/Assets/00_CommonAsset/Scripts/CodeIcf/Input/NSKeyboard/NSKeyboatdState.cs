using System;
using System.Runtime.InteropServices;

namespace CodeIcf.Input.NSKeyboard
{
    /// <summary>
    /// Nintendo Switch専用のキーボードの状態データ
    /// </summary>
    /// <remarks>Switchのネイティブ側からキーボードの入力データを受け取る際にこの構造体で取得</remarks>
    [StructLayout( LayoutKind.Sequential )]
    public struct NSKeyboatdState
    {
        /// <summary>入力状態がサンプリングされる度に増加する値</summary>
        public long SamplingNumber;
        /// <summary>キーボードの状態</summary>
        public int Attribute;
        /// <summary>修飾キーの状態</summary>
        public int Modifier;
        /// <summary>キーの状態</summary>
        public IntPtr KeysPtr;
    }
}