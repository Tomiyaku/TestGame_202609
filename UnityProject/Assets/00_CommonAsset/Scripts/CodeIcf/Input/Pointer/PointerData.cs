#if UNITY_SWITCH && !UNITY_EDITOR
#define SWITCH_INPUT_ENABLE
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

using CodeIcf.Input.NSMouse;
using CodeIcf.Extensions;

using jp.co.liica.q2.Common;

namespace CodeIcf.Input.PointerDevice
{
    /// <summary>
    /// ポインティングデバイスでの入力データ
    /// </summary>
    /// <remarks>Nintendo Switch以外の環境(Editor含む)の場合、マウスの入力データが格納</remarks>    
    public struct PointerData
    {
        /// <summary>
        /// デバイスの種類
        /// </summary>
        public enum PointerDeviceType
        {
            /// <summary>マウス(PC)</summary>
            Mouse,
            /// <summary>マウス(Nintendo Swtich)</summary>
            NSMouse,
            /// <summary>タッチパネル</summary>
            Touch,
        }
        /// <summary>
        /// ボタンの状態
        /// </summary>
        public enum ButtonState
        {
            None,
            Down,
            Hold,
            Up
        }

        /// <summary>この構造体のデータが有効か</summary>
        public bool IsEnable { get; private set; }
        /// <summary>デバイスの種類</summary>
        public PointerDeviceType DeviceType { get; private set; }
        /// <summary>ID</summary>
        public int PointerID { get; private set; }
        /// <summary>座標</summary>
        public Vector2 Position { get; private set; }
        /// <summary>最後のフレームからの差</summary>
        public Vector2 Delta { get; private set; }
        /// <summary>左クリックボタン(タッチ)の状態</summary>
        public ButtonState LeftButtonState { get; private set; }
        /// <summary>右クリックボタンの状態</summary>
        public ButtonState RightButtonState { get; private set; }
        /// <summary> 中央ボタンの状態</summary>
        public ButtonState MiddleButtonState { get; private set; }
        /// <summary>スクロール</summary>
        public Vector2 Scroll { get; private set; }
        /// <summary>進むボタンの状態/summary>
        public ButtonState ForwardButtonState { get; private set; }
        /// <summary>戻るボタンの状態</summary>
        public ButtonState BackButtonState { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_deviceType"></param>
        /// <param name="_id"></param>
        /// <param name="_pos"></param>
        /// <param name="_delta"></param>
        /// <param name="_left"></param>
        /// <param name="_right"></param>
        /// <param name="_middle"></param>
        /// <param name="_forward"></param>
        /// <param name="_back"></param>
        /// <param name="_scroll"></param>
        private PointerData( PointerDeviceType _deviceType, int _id, Vector2 _pos, Vector2 _delta, ButtonState _left, ButtonState _right, ButtonState _middle, ButtonState _forward, ButtonState _back, Vector2 _scroll )
        {
            IsEnable = true;
            DeviceType = _deviceType;
            PointerID = _id;
            Position = _pos;
            Delta = _delta;
            LeftButtonState = _left;
            RightButtonState = _right;
            MiddleButtonState = _middle;
            ForwardButtonState = _forward;
            BackButtonState = _back;
            Scroll = _scroll;
        }

        /// <summary>
        /// 入力データの作成
        /// </summary>
        /// <param name="_device"></param>
        /// <returns></returns>
        public static List<PointerData> CreateDataList( InputDevice _device )
        {
            List<PointerData> list = new List<PointerData>();

#if SWITCH_INPUT_ENABLE
            Touchscreen touchscreen = _device as Touchscreen;

            if( touchscreen == null ) return null;

            foreach( TouchControl touchControl in touchscreen.touches )
            {
                PointerData touchData = new PointerData(
                    PointerDeviceType.Touch,
                    touchControl.touchId.ReadValue(),
                    touchControl.position.ReadValue(),
                    touchControl.delta.ReadValue(),
                    ConvertTouchControlToButtonState( touchControl ),
                    ButtonState.None,
                    ButtonState.None,
                    ButtonState.None,
                    ButtonState.None,
                    Vector2.zero );

                list.Add( touchData );
            }
#else //SWITCH_INPUT_ENABLE
            if( !( _device is Pointer ) ) return null;

            Pointer pointer = _device as Pointer;

            PointerDeviceType deviceType = PointerDeviceType.Mouse;
            int id = _device.deviceId;
            Vector2 pos = pointer.position.ReadValue();
            Vector2 delta = pointer.delta.ReadValue();

            ButtonState leftPhase = ConvertButtonControlToButtonState( pointer.press );
            ButtonState rightPhase = ButtonState.None;
            ButtonState middlePhase = ButtonState.None;
            ButtonState forwardPhase = ButtonState.None;
            ButtonState backPhase = ButtonState.None;
            Vector2 scroll = Vector2.zero;

            if( _device is Mouse mouse )
            {// マウスの場合、右ボタン、中央ボタン、スクロールを取得
                leftPhase = ConvertButtonControlToButtonState( mouse.leftButton );
                rightPhase = ConvertButtonControlToButtonState( mouse.rightButton );
                middlePhase = ConvertButtonControlToButtonState( mouse.middleButton );
                forwardPhase = ConvertButtonControlToButtonState( mouse.forwardButton );
                backPhase = ConvertButtonControlToButtonState( mouse.backButton );
                scroll = mouse.scroll.ReadValue();
            }
            else if( _device is Touchscreen touch )
            {
                deviceType = PointerDeviceType.Touch;
                leftPhase = ConvertTouchControlToButtonState( touch.primaryTouch );
            }

            PointerData touchData = new PointerData( deviceType, id, pos, delta, leftPhase, rightPhase, middlePhase, forwardPhase, backPhase, scroll );
            list.Add( touchData );
#endif //SWITCH_INPUT_ENABLE

            return list;
        }

#if SWITCH_INPUT_ENABLE
        /// <summary>
        /// <see cref="NSMouseData"/>から変換
        /// </summary>
        /// <param name="_mouseData"></param>
        /// <returns></returns>
        public static PointerData ConvertFromNSMouseData( NSMouseData _mouseData )
        {
            if( !_mouseData.IsConnected ) return default;

            float mx = _mouseData.Position.x / SwitchDefine.SCREEN_WIDTH;
            float my = _mouseData.Position.y / SwitchDefine.SCREEN_HEIGHT;

            Vector2 pos = new Vector2( Screen.width * mx, Screen.height - ( Screen.height * my ) );

            float diffW = Screen.width / SwitchDefine.SCREEN_WIDTH;
            float diffH = Screen.height / SwitchDefine.SCREEN_HEIGHT;

            Vector2 delta = new Vector2( _mouseData.PositionDelta.x * diffW, -( _mouseData.PositionDelta.y * diffH ) );

            ButtonState leftPhase = _mouseData.LeftButtonState;
            ButtonState rightPhase = _mouseData.RightButtonState;
            ButtonState middlePhase = _mouseData.MiddleButtonState;
            ButtonState forwardPhase = _mouseData.ForwardButtonState;
            ButtonState backPhase = _mouseData.BackButtonState;
            Vector2 scroll = _mouseData.WheelDelta;

            return new PointerData( PointerDeviceType.NSMouse, 0, pos, delta, leftPhase, rightPhase, middlePhase, forwardPhase, backPhase, scroll );
        }
#endif

        /// <summary>
        /// マウスのボタンの状態を<see cref="ButtonState"/>に変換
        /// </summary>
        /// <param name="_button"></param>
        /// <returns></returns>
        private static ButtonState ConvertButtonControlToButtonState( ButtonControl _button )
        {
            ButtonState result = ButtonState.None;

            if( _button.IsPressed() )
            {
                result = _button.wasPressedThisFrame ? ButtonState.Down : ButtonState.Hold;
            }
            else
            {
                if( _button.wasReleasedThisFrame ) result = ButtonState.Up;
            }

            return result;
        }

        /// <summary>
        /// マウスのボタンの状態を<see cref="ButtonState"/>に変換
        /// </summary>
        /// <param name="_button"></param>
        /// <returns></returns>
        private static ButtonState ConvertTouchControlToButtonState( TouchControl _touch )
        {
            ButtonState result = ButtonState.None;
            TouchPhase touchPhase = _touch.phase.ReadValue();

            if( touchPhase == TouchPhase.Began ) result = ButtonState.Down;
            else if( touchPhase == TouchPhase.Stationary || touchPhase == TouchPhase.Moved ) result = ButtonState.Hold;
            else if( touchPhase == TouchPhase.Ended || touchPhase == TouchPhase.Canceled ) result = ButtonState.Up;

            // 同じTouchIDの履歴がある場合に既にUpが記録されている場合はNoneとして返す
            if( touchPhase == TouchPhase.Ended )
            {
                List<PointerHistory> historyList = InputDeviceManager.Instance.GetPointerHistory( _touch.touchId.ReadValue() );

                if( historyList != null && historyList.Count > 0 )
                {
                    if( historyList[ 0 ].TouchData.LeftButtonState == ButtonState.Up || historyList[ 0 ].TouchData.LeftButtonState == ButtonState.None ) return ButtonState.None;
                }
            }

            return result;
        }

        /// <summary>
        /// <see cref="TouchPhase"/>から<see cref="ButtonState"/>へ返還
        /// </summary>
        /// <param name="_touchPhase"></param>
        /// <returns></returns>
        private static ButtonState ConvertTouchPhaseToButtonState( TouchPhase _touchPhase )
        {
            ButtonState result = ButtonState.None;

            if( _touchPhase == TouchPhase.Began ) result = ButtonState.Down;
            else if( _touchPhase == TouchPhase.Moved || _touchPhase == TouchPhase.Stationary ) result = ButtonState.Hold;
            else if( _touchPhase == TouchPhase.Ended || _touchPhase == TouchPhase.Canceled ) result = ButtonState.Up;

            return result;
        }

        /// <summary>
        /// 画面をタッチしているか
        /// </summary>
        /// <returns></returns>
        public bool IsTouch()
        {
            if( !IsEnable ) return false;

            return LeftButtonState == ButtonState.Down || LeftButtonState == ButtonState.Hold;
        }

        /// <summary>
        /// マウスの左ボタンを押下
        /// </summary>
        /// <returns></returns>
        public bool IsDownLeftButton()
        {
            if( !IsEnable ) return false;

            return LeftButtonState == ButtonState.Down;
        }

        /// <summary>
        /// マウスの左ボタンを押し続けているか
        /// </summary>
        /// <returns></returns>
        public bool IsHoldLeftButton()
        {
            if( !IsEnable ) return false;

            return LeftButtonState == ButtonState.Hold;
        }

        /// <summary>
        /// マウスの左ボタンを離したか
        /// </summary>
        /// <returns></returns>
        public bool IsUpLeftButton()
        {
            if( !IsEnable ) return false;

            return LeftButtonState == ButtonState.Up;
        }

        /// <summary>
        /// マウスの右ボタンを押下
        /// </summary>
        /// <returns></returns>
        public bool IsDownRightButton()
        {
            if( !IsEnable ) return false;

            return RightButtonState == ButtonState.Down;
        }

        /// <summary>
        /// マウスの右ボタンを押し続けているか
        /// </summary>
        /// <returns></returns>
        public bool IsHoldRightButton()
        {
            if( !IsEnable ) return false;

            return RightButtonState == ButtonState.Hold;
        }

        /// <summary>
        /// マウスの右ボタンを離したか
        /// </summary>
        /// <returns></returns>
        public bool IsUpRighttButton()
        {
            if( !IsEnable ) return false;

            return RightButtonState == ButtonState.Up;
        }

        /// <summary>
        /// マウスの中央ボタンを押下
        /// </summary>
        /// <returns></returns>
        public bool IsDownMiddleButton()
        {
            if( !IsEnable ) return false;

            return MiddleButtonState == ButtonState.Down;
        }

        /// <summary>
        /// マウスの中央ボタンを押し続けているか
        /// </summary>
        /// <returns></returns>
        public bool IsHoldMiddleButton()
        {
            if( !IsEnable ) return false;

            return MiddleButtonState == ButtonState.Hold;
        }

        /// <summary>
        /// マウスの中央ボタンを離したか
        /// </summary>
        /// <returns></returns>
        public bool IsUpMiddleButton()
        {
            if( !IsEnable ) return false;

            return MiddleButtonState == ButtonState.Up;
        }

        /// <summary>
        /// いづれかのボタンが押されているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyButtonDown()
        {
            if( LeftButtonState == ButtonState.Down ) return true;
            if( RightButtonState == ButtonState.Down ) return true;
            if( MiddleButtonState == ButtonState.Down ) return true;

            return false;
        }

        /// <summary>
        /// いづれかのボタンが押されているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyButtonDownOrHold()
        {
            if( LeftButtonState == ButtonState.Down || LeftButtonState == ButtonState.Hold ) return true;
            if( RightButtonState == ButtonState.Down || RightButtonState == ButtonState.Hold ) return true;
            if( MiddleButtonState == ButtonState.Down || MiddleButtonState == ButtonState.Hold ) return true;

            return false;
        }

        /// <summary>
        /// いづれかのボタンが押されているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyButtonUp()
        {
            if( LeftButtonState == ButtonState.Up ) return true;
            if( RightButtonState == ButtonState.Up ) return true;
            if( MiddleButtonState == ButtonState.Up ) return true;

            return false;
        }
    }
}