using System.Runtime.InteropServices;

namespace CodeIcf.Input.NSMouse
{
    /// <summary>
    /// Nintendo Switch専用のマウスの状態データ
    /// </summary>
    /// <remarks>Switchのネイティブ側からマウスの入力データを受け取る際にこの構造体で取得</remarks>
    [StructLayout( LayoutKind.Sequential )]
    public struct NSMouseState
    {
        /// <summary>入力状態がサンプリングされる度に増加する値</summary>
        public long SamplingNumber;
        /// <summary>カーソルのX座標</summary>
        public int X;
        /// <summary>カーソルのY座標</summary>
        public int Y;
        /// <summary>カーソルのX座標の移動差分</summary>
        public int DeltaX;
        /// <summary>カーソルのY座標の移動差分</summary>
        public int DeltaY;
        /// <summary>ホイールの回転差分</summary>
        /// <remarks>回転方向で奥に正、手前に負の値が格納 ホイールが15度づつ回転する物が多いので、120が一つの目安</remarks>
        public int WheelDelta;
        /// <summary>ボタンの状態</summary>
        public int Buttons;
        /// <summary>マウスの状態</summary>
        public int Attribute;
    }
}