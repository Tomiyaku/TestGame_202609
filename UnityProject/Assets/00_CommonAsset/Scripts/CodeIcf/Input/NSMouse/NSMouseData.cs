using UnityEngine;

using CodeIcf.Extensions;
using CodeIcf.Input.PointerDevice;

namespace CodeIcf.Input.NSMouse
{
    /// <summary>
    /// Nintendo Switch用マウスの情報格納クラス
    /// </summary>
    public class NSMouseData
    {
        /// <summary>マウス左ボタンの状態のインデックス</summary>
        private const int BUTTON_INDEX_LEFT = 0;
        /// <summary>マウス右ボタンの状態のインデックス</summary>
        private const int BUTTON_INDEX_RIGHT = 1;
        /// <summary>マウス中央ボタンの状態のインデックス</summary>
        private const int BUTTON_INDEX_MIDDLE = 2;
        /// <summary>マウス進むボタンの状態のインデックス</summary>
        private const int BUTTON_INDEX_FORWARD = 3;
        /// <summary>マウス戻るボタンの状態のインデックス</summary>
        private const int BUTTON_INDEX_BACK = 4;
        /// <summary>マウスボタンの数</summary>
        private const int BUTTON_COUNT = 5;

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

        /// <summary>マウスカーソル座標</summary>
        public Vector2Int Position { get; private set; } = Vector2Int.zero;
        /// <summary>マウスカーソル座標の差分</summary>
        /// <remarks>この差分はNintendo Swichから取得した値となり、前フレームからの差分とは異なる場合がある</remarks>
        public Vector2Int PositionDelta { get; private set; } = Vector2Int.zero;
        /// <summary>マウスホイールの移動量差分</summary>
        public Vector2Int WheelDelta { get; private set; } = Vector2Int.zero;

        /// <summary>マウスボタンのON/OFFのビットフラグ</summary>
        private int m_MouseButtonStates = 0;
        /// <summary>マウスの状態</summary>
        private int m_MouseAttribute = 0;
        /// <summary>マウスボタンの状態一覧</summary>
        private PointerData.ButtonState[] m_ButtonStetaList = null;

        /// <summary>マウスが接続状態か</summary>
        public bool IsConnected => m_MouseAttribute.EqualLogicAnd( ATTRIBUTE_IS_CONNECTED );
        /// <summary>上位システムへマウス操作を委譲可能か</summary>
        /// <remarks>Windows環境下でカーソルがウインドウのフォーカスを失ったかの判定で使える模様 Unityで開発しているので基本不要かも</remarks>
        public bool Transferable => m_MouseAttribute.EqualLogicAnd( ATTRIBUTE_TRANSFERABLE );
        /// <summary>左ボタンが押下しているか</summary>
        public bool IsLeftDown => m_MouseButtonStates.EqualLogicAnd( MOUSE_BUTTON_LEFT );
        /// <summary>右ボタンが押下しているか</summary>
        public bool IsRightDown => m_MouseButtonStates.EqualLogicAnd( MOUSE_BUTTON_RIGHT );
        /// <summary>中応ボタンが押下しているか</summary>
        public bool IsMiddleDown => m_MouseButtonStates.EqualLogicAnd( MOUSE_BUTTON_MIDDLE );
        /// <summary>フォワードボタンが押下しているか</summary>
        public bool IsForwardDown => m_MouseButtonStates.EqualLogicAnd( MOUSE_BUTTON_FORWARD );
        /// <summary>バックボタンが押下しているか</summary>
        public bool IsBackDown => m_MouseButtonStates.EqualLogicAnd( MOUSE_BUTTON_BACK );

        /// <summary>マウス左ボタンの状態を取得</summary>
        public PointerData.ButtonState LeftButtonState => m_ButtonStetaList == null || m_ButtonStetaList.Length < BUTTON_COUNT ? PointerData.ButtonState.None : m_ButtonStetaList[ BUTTON_INDEX_LEFT ];
        /// <summary>マウス右ボタンの状態を取得</summary>
        public PointerData.ButtonState RightButtonState => m_ButtonStetaList == null || m_ButtonStetaList.Length < BUTTON_COUNT ? PointerData.ButtonState.None : m_ButtonStetaList[ BUTTON_INDEX_RIGHT ];
        /// <summary>マウス中央ボタンの状態を取得</summary>
        public PointerData.ButtonState MiddleButtonState => m_ButtonStetaList == null || m_ButtonStetaList.Length < BUTTON_COUNT ? PointerData.ButtonState.None : m_ButtonStetaList[ BUTTON_INDEX_MIDDLE ];
        /// <summary>マウス進むボタンの状態を取得</summary>
        public PointerData.ButtonState ForwardButtonState => m_ButtonStetaList == null || m_ButtonStetaList.Length < BUTTON_COUNT ? PointerData.ButtonState.None : m_ButtonStetaList[ BUTTON_INDEX_FORWARD ];
        /// <summary>マウス戻るボタンの状態を取得</summary>
        public PointerData.ButtonState BackButtonState => m_ButtonStetaList == null || m_ButtonStetaList.Length < BUTTON_COUNT ? PointerData.ButtonState.None : m_ButtonStetaList[ BUTTON_INDEX_BACK ];

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NSMouseData()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_nowState"></param>
        /// <param name="_prev"></param>
        public NSMouseData( NSMouseState _nowState, NSMouseData _prev )
        {
            Position = new Vector2Int( _nowState.X, _nowState.Y );
            PositionDelta = new Vector2Int( _nowState.DeltaX, _nowState.DeltaY );
            WheelDelta = new Vector2Int( 0, _nowState.WheelDelta );

            m_MouseButtonStates = _nowState.Buttons;
            m_MouseAttribute = _nowState.Attribute;

            m_ButtonStetaList = new PointerData.ButtonState[ BUTTON_COUNT ];
            m_ButtonStetaList[ BUTTON_INDEX_LEFT ] = GetButtonState( IsLeftDown, _prev == null ? false : _prev.IsLeftDown );
            m_ButtonStetaList[ BUTTON_INDEX_RIGHT ] = GetButtonState( IsRightDown, _prev == null ? false : _prev.IsRightDown );
            m_ButtonStetaList[ BUTTON_INDEX_MIDDLE ] = GetButtonState( IsMiddleDown, _prev == null ? false : _prev.IsMiddleDown );
            m_ButtonStetaList[ BUTTON_INDEX_FORWARD ] = GetButtonState( IsForwardDown, _prev == null ? false : _prev.IsForwardDown );
            m_ButtonStetaList[ BUTTON_INDEX_BACK ] = GetButtonState( IsBackDown, _prev == null ? false : _prev.IsBackDown );
        }

        /// <summary>
        /// ボタンの状態を取得
        /// </summary>
        /// <param name="_now"></param>
        /// <param name="_prev"></param>
        /// <returns></returns>
        private PointerData.ButtonState GetButtonState( bool _now, bool _prev )
        {
            PointerData.ButtonState result = PointerData.ButtonState.None;

            if( _now )
            {
                result = _prev ? PointerData.ButtonState.Hold : PointerData.ButtonState.Down;
            }
            else
            {
                result = _prev ? PointerData.ButtonState.Up : PointerData.ButtonState.None;
            }

            return result;
        }
    }
}