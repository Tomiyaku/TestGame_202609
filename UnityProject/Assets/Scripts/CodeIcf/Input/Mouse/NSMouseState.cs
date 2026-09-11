using System.Runtime.InteropServices;

using CodeIcf.Extensions;

namespace CodeIcf.Input.Mouse
{
    /// <summary>
    /// マウスの状態
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct NSMouseState
    {
        /// <summary> 上位システムへマウス操作を委譲可能かの値のビットフラグ</summary>
        private const byte ATTRIBUTE_TRANSFERABLE = 0x1;
        /// <summary> マウスが接続状態かののビットフラグ</summary>
        private const byte ATTRIBUTE_IS_CONNECTED = 0x1 << 1;

        /// <summary>マウスの左ボタン押下のビットフラグ</summary>
        private const byte MOUSE_BUTTON_LEFT = 0x1;
        /// <summary>マウスの右ボタン押下のビットフラグ</summary>
        private const byte MOUSE_BUTTON_RIGHT = 0x1 << 1;
        /// <summary>マウスの中央ボタン押下のビットフラグ</summary>
        private const byte MOUSE_BUTTON_MIDDLE = 0x1 << 2;
        /// <summary>マウスのフォワードボタン押下のビットフラグ</summary>
        private const byte MOUSE_BUTTON_FORWARD = 0x1 << 3;
        /// <summary>マウスのバックボタン押下のビットフラグ</summary>
        private const byte MOUSE_BUTTON_BACK = 0x1 << 4;

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

        /// <summary>マウスが接続状態か</summary>
        public bool IsConnected => Attribute.EqualLogicAnd( ATTRIBUTE_IS_CONNECTED );
        /// <summary>上位システムへマウス操作を委譲可能か</summary>
        /// <remarks>Windows環境下でカーソルがウインドウのフォーカスを失ったかの判定で使える模様 Unityで開発しているので基本不要かも</remarks>
        public bool Transferable => Attribute.EqualLogicAnd( ATTRIBUTE_TRANSFERABLE );
        /// <summary>左ボタンが押下しているか</summary>
        public bool IsLeftDown => Buttons.EqualLogicAnd( MOUSE_BUTTON_LEFT );
        /// <summary>右ボタンが押下しているか</summary>
        public bool IsRightDown => Buttons.EqualLogicAnd( MOUSE_BUTTON_RIGHT ); 
        /// <summary>中応ボタンが押下しているか</summary>
        public bool IsMiddleDown => Buttons.EqualLogicAnd( MOUSE_BUTTON_MIDDLE );
        /// <summary>フォワードボタンが押下しているか</summary>
        public bool IsForwardDown => Buttons.EqualLogicAnd( MOUSE_BUTTON_FORWARD );
        /// <summary>バックボタンが押下しているか</summary>
        public bool IsBackDown => Buttons.EqualLogicAnd( MOUSE_BUTTON_BACK );

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            SamplingNumber = 0;
            X = 0;
            Y = 0;
            DeltaX = 0;
            DeltaY = 0;
            WheelDelta = 0;
            Buttons = 0;
            Attribute = 0;
        }
    }
}