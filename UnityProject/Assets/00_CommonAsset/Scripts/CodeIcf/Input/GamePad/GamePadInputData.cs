#if UNITY_SWITCH && !UNITY_EDITOR
#define SWITCH_INPUT_ENABLE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;

using CodeIcf.Extensions;
using CodeIcf.Input.Keyboard;

#if SWITCH_INPUT_ENABLE
using UnityEngine.InputSystem.Switch;
using nn.hid;
#else
using UnityEngine.InputSystem.XInput;
#endif

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// コントローラー入力情報
    /// </summary>
    public struct GamePadInputData
    {
        /// <summary>スティックの入力値を保持する時の小数点を切り捨る値</summary>
        private const float FLOOR_MUGNIFICATE = 1000f;

        /// <summary>ボタンを押下した瞬間のビットフラグ</summary>
        private GamepadKeyId m_ButtonDown;
        /// <summary>ボタンを押下した瞬間のビットフラグ</summary>
        public GamepadKeyId ButtonDown => m_ButtonDown;
        /// <summary>ボタンを押下し続けている時のビットフラグ</summary>
        private GamepadKeyId m_ButtonHold;
        /// <summary>ボタンを押下し続けている時のビットフラグ</summary>
        public GamepadKeyId ButtonHold => m_ButtonHold;
        /// <summary>ボタンを離した瞬間のビットフラグ</summary>
        private GamepadKeyId m_ButtonUp;
        /// <summary>ボタンを離した瞬間のビットフラグ</summary>
        public GamepadKeyId ButtonUp => m_ButtonUp;

        /// <summary>この構造体のデータが有効か</summary>
        public bool IsEnable { get; private set; }
        /// <summary>デバイスID</summary>
        public int DeviceId { get; private set; }
        /// <summary>左スティック水平方向入力値</summary>
        public float LStickAxisH { get; private set; }
        /// <summary>左スティック垂直方向入力値</summary>
        public float LStickAxisV { get; private set; }
        /// <summary>右スティック水平方向入力値</summary>
        public float RStickAxisH { get; private set; }
        /// <summary>右スティック垂直方向入力値</summary>
        public float RStickAxisV { get; private set; }
        /// <summary>左スティック入力値</summary>
        public Vector2 LStickAxis => new Vector2(LStickAxisH, LStickAxisV);
        /// <summary>右スティック入力値</summary>
        public Vector2 RStickAxis => new Vector2(RStickAxisH, RStickAxisV);

#if SWITCH_INPUT_ENABLE
        /// <summary>Nitendo Switchでのコントローラの状態データ</summary>
        private NpadState m_NpadState;
#endif

        /// <summary>
        /// 未入力状態のデータにする
        /// </summary>
        /// <remarks>
        /// <see cref="IsEnable"/>と<see cref="DeviceId"/>以外を初期化し未入力状態のデータにする<br></br>
        /// 入力の判定後の処理で他のキー入力があると困る場合を想定
        /// </remarks>
        public void EmptyInput()
        {
            m_ButtonDown = GamepadKeyId.None;
            m_ButtonHold = GamepadKeyId.None;
            m_ButtonUp = GamepadKeyId.None;

            LStickAxisH = 0;
            LStickAxisV = 0;
            RStickAxisH = 0;
            RStickAxisV = 0;

#if SWITCH_INPUT_ENABLE
            m_NpadState = default;
#endif
        }

        public static void EmptyInput(ref GamePadInputData _refInputData)
        {
            _refInputData.m_ButtonDown = GamepadKeyId.None;
            _refInputData.m_ButtonHold = GamepadKeyId.None;
            _refInputData.m_ButtonUp = GamepadKeyId.None;

            _refInputData.LStickAxisH = 0;
            _refInputData.LStickAxisV = 0;
            _refInputData.RStickAxisH = 0;
            _refInputData.RStickAxisV = 0;

#if SWITCH_INPUT_ENABLE
            _refInputData.m_NpadState = default;
#endif
        }

        /// <summary>
        /// 指定のボタンを押下したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns>trueなら指定のキー押下</returns>
        public bool IsDown(GamepadKeyId _keyId)
        {
            return (m_ButtonDown & _keyId) > 0;
        }

        /// <summary>
        /// 指定のボタンを押下し続けているか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns>trueなら指定のキーを押し続けている</returns>
        public bool IsHold(GamepadKeyId _keyId)
        {
            return (m_ButtonHold & _keyId) > 0;
        }

        /// <summary>
        /// 指定のボタンを離したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns>trueなら指定のキーを離した</returns>
        public bool IsUp(GamepadKeyId _keyId)
        {
            return (m_ButtonUp & _keyId) > 0;
        }

        /// <summary>
        /// 指定のボタンが未入力状態か
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns><see cref="IsDown">,<see cref="IsHold"/>,<see cref="IsUp"/>のいづれもfalseが帰ってくる状態であればtrue</returns>
        public bool IsNotInput(GamepadKeyId _keyId)
        {
            return !IsDown(_keyId) && !IsHold(_keyId) && !IsUp(_keyId);
        }

        /// <summary>
        /// いづれかのボタンを押下したか
        /// </summary>
        /// <returns>trueならいづれかのキーを押下</returns>
        public bool IsAnyKeyDown => m_ButtonDown > 0;

        /// <summary>
        /// いづれかのボタンを押下し続けているか
        /// </summary>
        /// <returns>trueならいづれかのキーを押し続けている</returns>
        public bool IsAnyKeyHold => m_ButtonHold > 0;

        /// <summary>
        /// いづれかのボタンを離したか
        /// </summary>
        /// <returns>trueならいづれかのキーを離した</returns>
        public bool IsAnyKeyUp => m_ButtonUp > 0;

        /// <summary>
        /// 指定のキーの同時押下
        /// </summary>
        /// <param name="_keyId">同時押しを判定するキー 論理OR演算子で複数キーを指定</param>
        /// <param name="_validityTime">同時押しの有効時間</param>
        /// <returns><see cref="MultipleDownResult"/></returns>
        public MultipleDownResult IsMultipleDown(GamepadKeyId _keyId, float _validityTime = GamePadDefine.MULTIPLE_DOWN_TIME)
        {
            //同時押し判定のキーをリスト化
            List<GamepadKeyId> keyIdList = new List<GamepadKeyId>();

            foreach (GamepadKeyId key in Enum.GetValues(typeof(GamepadKeyId)))
            {
                if (key == GamepadKeyId.None) continue;

                if ((_keyId & key) == key) keyIdList.Add(key);
            }

            //引数で渡されたキーの数が1以下の場合は同時押し無しとして返す
            if (keyIdList.Count <= 1) return MultipleDownResult.MissingKey;

            List<GamePadHistory> inputHistoryList = InputDeviceManager.Instance.GetGamePadHistory(DeviceId);

            //履歴データが取得できなかったので同時押し無しとして返す
            if (inputHistoryList == null) return MultipleDownResult.MissingHistory;

            //キー毎の押下時間のリスト -1で初期化
            float[] keyDownTimeList = Enumerable.Repeat<float>(-1, keyIdList.Count).ToArray();
            bool isNowDown = false;

            //入力履歴から指定のキーの押下した時間を取得
            for (int i = 0; i < keyIdList.Count; i++)
            {
                if (IsDown(keyIdList[i])) isNowDown = true;

                GamePadHistory history = inputHistoryList.Find((GamePadHistory inputHisty) => inputHisty.InputData.IsDown(keyIdList[i]));

                if (history != null && history.IsEnable) keyDownTimeList[i] = history.GetDataTime;
            }

            //いづれかのボタンが今押された瞬間ではない場合は同時押し無しとする
            if (!isNowDown) return MultipleDownResult.Failure;

            float newestTime = -1;
            float oldestTime = -1;

            //押下時間の最新と最古の時間を探す
            for (int i = 0; i < keyDownTimeList.Length; i++)
            {
                //押下した時間が取得でき無かったキーがある場合は同時押し無しとして返す
                if (keyDownTimeList[i] < 0) return MultipleDownResult.Failure;

                if (newestTime < 0 || newestTime < keyDownTimeList[i]) newestTime = keyDownTimeList[i];
                if (oldestTime < 0 || oldestTime > keyDownTimeList[i]) oldestTime = keyDownTimeList[i];
            }

            //最新と最古の時間差が有効時間内であれば同時押し判定とする
            bool result = newestTime - oldestTime <= _validityTime;

            return result ? MultipleDownResult.Success : MultipleDownResult.Failure;
        }

        /// <summary>
        /// 指定のキーを指定時間以上押し続けているか
        /// </summary>
        /// <param name="_keyId">キーID</param>
        /// <param name="_time">押し続けている時間</param>
        /// <returns><see cref="HoldingResult"/></returns>
        public HoldingResult IsHoldingKeyForKeepTime(GamepadKeyId _keyId, float _time)
        {
            if (!IsHold(_keyId)) return HoldingResult.NotNowHold;

            return InputDeviceManager.Instance.IsHoldingKeyForKeepTime(DeviceId, _keyId, _time);
        }

        /// <summary>
        /// ゲームパッドの入力データを作成
        /// </summary>
        /// <param name="_device">InputDevice</param>
        /// <param name="_outInputData">取得した入力データが格納</param>
        /// <returns>デバイスID</returns>
        public static int CreateData(InputDevice _device, GamePadStickDeadZone _stickDeadZone, out GamePadInputData _outInputData)
        {
            _outInputData = default;

            GetGamePadDeviceIDResult result = InputDeviceManager.GetGamePadDeviceID(_device, out int deviceId);

            if (result < GetGamePadDeviceIDResult.Success)
            {
                Debug.Log("InputDeviceManager.GetGamePadDeviceID Result Failed : " + result.ToString());
                return GamePadDefine.INVALID_DEVICE_ID;
            }

            if (deviceId <= GamePadDefine.INVALID_DEVICE_ID)
            {
                Debug.Log("DeviceID INVALID_DEVICE_ID");
                return GamePadDefine.INVALID_DEVICE_ID;
            }

#if SWITCH_INPUT_ENABLE
            if( !( _device is NPad gamepad ) )
            {
                Debug.Log( "InputDevice Not NPad" );
                return GamePadDefine.INVALID_DEVICE_ID;
            }

            NpadId npadId = ( NpadId )deviceId;
            NpadStyle npadStyle = ( NpadStyle )InputDeviceManager.GetControllerStyle( deviceId );
            _outInputData.m_NpadState = default;

            if( InputDeviceManager.Instance.GetGamepadInputDataPrevious( deviceId, out GamePadInputData prevInputData ) )
            {//以前の状態データを取得
                _outInputData.m_NpadState = prevInputData.m_NpadState;
            }

            Npad.GetState( ref _outInputData.m_NpadState, npadId, npadStyle );

            if( _outInputData.m_NpadState.samplingNumber <= 0 )
            {
                Debug.Log( "Faild  Npad.GetState" );
                return GamePadDefine.INVALID_DEVICE_ID;
            }

            _outInputData.IsEnable = true;
            _outInputData.DeviceId = deviceId;

            // JoyCon１本持ちとそれ以外で取得する入力データを振り分け
            if( npadStyle == NpadStyle.JoyLeft )
            {//JoyCon左側１本持ち
                _outInputData.LStickAxisH = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickL.fy * -1f, _stickDeadZone.LStickDeadZone);
                _outInputData.LStickAxisV = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickL.fx, _stickDeadZone.LStickDeadZone);

                _outInputData.UpdateKeyState( GamepadKeyId.A, in _outInputData.m_NpadState, NpadButton.Down );
                _outInputData.UpdateKeyState( GamepadKeyId.B, in _outInputData.m_NpadState, NpadButton.Left );
                _outInputData.UpdateKeyState( GamepadKeyId.X, in _outInputData.m_NpadState, NpadButton.Right );
                _outInputData.UpdateKeyState( GamepadKeyId.Y, in _outInputData.m_NpadState, NpadButton.Up );

                _outInputData.UpdateKeyState( GamepadKeyId.L, in _outInputData.m_NpadState, NpadButton.LeftSL );
                _outInputData.UpdateKeyState( GamepadKeyId.R, in _outInputData.m_NpadState, NpadButton.LeftSR );

                _outInputData.UpdateKeyState( GamepadKeyId.LS, in _outInputData.m_NpadState, NpadButton.StickL );

                _outInputData.UpdateKeyState( GamepadKeyId.Start, in _outInputData.m_NpadState, NpadButton.Minus );
            }
            else if( npadStyle == NpadStyle.JoyRight )
            {//JoyCon右側１本持ち
                _outInputData.LStickAxisH = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickR.fy, _stickDeadZone.RStickDeadZone);
                _outInputData.LStickAxisV = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickR.fx * -1f, _stickDeadZone.RStickDeadZone);

                _outInputData.UpdateKeyState( GamepadKeyId.A, in _outInputData.m_NpadState, NpadButton.X );
                _outInputData.UpdateKeyState( GamepadKeyId.B, in _outInputData.m_NpadState, NpadButton.A );
                _outInputData.UpdateKeyState( GamepadKeyId.X, in _outInputData.m_NpadState, NpadButton.Y );
                _outInputData.UpdateKeyState( GamepadKeyId.Y, in _outInputData.m_NpadState, NpadButton.B );

                _outInputData.UpdateKeyState( GamepadKeyId.L, in _outInputData.m_NpadState, NpadButton.RightSL );
                _outInputData.UpdateKeyState( GamepadKeyId.R, in _outInputData.m_NpadState, NpadButton.RightSR );

                _outInputData.UpdateKeyState( GamepadKeyId.LS, in _outInputData.m_NpadState, NpadButton.StickR );

                _outInputData.UpdateKeyState( GamepadKeyId.Start, in _outInputData.m_NpadState, NpadButton.Plus );
            }
            else
            {// JouCon２本持ち・HandHeld・Proコン
                _outInputData.LStickAxisH = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickL.fx, _stickDeadZone.LStickDeadZone);
                _outInputData.LStickAxisV = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickL.fy, _stickDeadZone.LStickDeadZone);
                _outInputData.RStickAxisH = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickR.fx, _stickDeadZone.RStickDeadZone);
                _outInputData.RStickAxisV = GamePadStickDeadZone.CheckDeadZone(_outInputData.m_NpadState.analogStickR.fy, _stickDeadZone.RStickDeadZone);

                _outInputData.UpdateKeyState( GamepadKeyId.A, in _outInputData.m_NpadState, NpadButton.A );
                _outInputData.UpdateKeyState( GamepadKeyId.B, in _outInputData.m_NpadState, NpadButton.B );
                _outInputData.UpdateKeyState( GamepadKeyId.X, in _outInputData.m_NpadState, NpadButton.X );
                _outInputData.UpdateKeyState( GamepadKeyId.Y, in _outInputData.m_NpadState, NpadButton.Y );

                _outInputData.UpdateKeyState( GamepadKeyId.L, in _outInputData.m_NpadState, NpadButton.L );
                _outInputData.UpdateKeyState( GamepadKeyId.R, in _outInputData.m_NpadState, NpadButton.R );
                _outInputData.UpdateKeyState( GamepadKeyId.LT, in _outInputData.m_NpadState, NpadButton.ZL );
                _outInputData.UpdateKeyState( GamepadKeyId.RT, in _outInputData.m_NpadState, NpadButton.ZR );

                _outInputData.UpdateKeyState( GamepadKeyId.LS, in _outInputData.m_NpadState, NpadButton.StickL );
                _outInputData.UpdateKeyState( GamepadKeyId.RS, in _outInputData.m_NpadState, NpadButton.StickR );

                _outInputData.UpdateKeyState( GamepadKeyId.D_Pad_Up, in _outInputData.m_NpadState, NpadButton.Up );
                _outInputData.UpdateKeyState( GamepadKeyId.D_Pad_Down, in _outInputData.m_NpadState, NpadButton.Down );
                _outInputData.UpdateKeyState( GamepadKeyId.D_Pad_Left, in _outInputData.m_NpadState, NpadButton.Left );
                _outInputData.UpdateKeyState( GamepadKeyId.D_Pad_Right, in _outInputData.m_NpadState, NpadButton.Right );

                _outInputData.UpdateKeyState( GamepadKeyId.Start, in _outInputData.m_NpadState, NpadButton.Plus );
                _outInputData.UpdateKeyState( GamepadKeyId.Select, in _outInputData.m_NpadState, NpadButton.Minus );
            }
#else
            if (!(_device is Gamepad gamepad))
            {
                Debug.Log("InputDevice Not GamePad");
                return GamePadDefine.INVALID_DEVICE_ID;
            }

            _outInputData.IsEnable = true;
            _outInputData.DeviceId = deviceId;

            Vector2 lStickAxis = FloorVector2(gamepad.leftStick.ReadValue());
            Vector2 rSrickAxis = FloorVector2(gamepad.rightStick.ReadValue());

            _outInputData.LStickAxisH = GamePadStickDeadZone.CheckDeadZone(lStickAxis.x, _stickDeadZone.LStickDeadZone);
            _outInputData.LStickAxisV = GamePadStickDeadZone.CheckDeadZone(lStickAxis.y, _stickDeadZone.LStickDeadZone);
            _outInputData.RStickAxisH = GamePadStickDeadZone.CheckDeadZone(rSrickAxis.x, _stickDeadZone.RStickDeadZone);
            _outInputData.RStickAxisV = GamePadStickDeadZone.CheckDeadZone(rSrickAxis.y, _stickDeadZone.RStickDeadZone);

            if (gamepad is XInputController || gamepad is DualShockGamepad)
            {
                // XInput もしくは DualShock(×が決定)
                _outInputData.UpdateKeyState(GamepadKeyId.A, gamepad.buttonSouth);
                _outInputData.UpdateKeyState(GamepadKeyId.B, gamepad.buttonEast);
                _outInputData.UpdateKeyState(GamepadKeyId.X, gamepad.buttonWest);
                _outInputData.UpdateKeyState(GamepadKeyId.Y, gamepad.buttonNorth);
            }
            else
            {
                //  Switch Proコントローラー もしくは それ以外
                _outInputData.UpdateKeyState(GamepadKeyId.A, gamepad.buttonEast);
                _outInputData.UpdateKeyState(GamepadKeyId.B, gamepad.buttonSouth);
                _outInputData.UpdateKeyState(GamepadKeyId.X, gamepad.buttonNorth);
                _outInputData.UpdateKeyState(GamepadKeyId.Y, gamepad.buttonWest);
            }

            _outInputData.UpdateKeyState(GamepadKeyId.L, gamepad.leftShoulder);
            _outInputData.UpdateKeyState(GamepadKeyId.R, gamepad.rightShoulder);
            _outInputData.UpdateKeyState(GamepadKeyId.LT, gamepad.leftTrigger);
            _outInputData.UpdateKeyState(GamepadKeyId.RT, gamepad.rightTrigger);

            _outInputData.UpdateKeyState(GamepadKeyId.LS, gamepad.leftStickButton);
            _outInputData.UpdateKeyState(GamepadKeyId.RS, gamepad.rightStickButton);

            _outInputData.UpdateKeyState(GamepadKeyId.D_Pad_Up, gamepad.dpad.up);
            _outInputData.UpdateKeyState(GamepadKeyId.D_Pad_Down, gamepad.dpad.down);
            _outInputData.UpdateKeyState(GamepadKeyId.D_Pad_Left, gamepad.dpad.left);
            _outInputData.UpdateKeyState(GamepadKeyId.D_Pad_Right, gamepad.dpad.right);

            _outInputData.UpdateKeyState(GamepadKeyId.Start, gamepad.startButton);
            _outInputData.UpdateKeyState(GamepadKeyId.Select, gamepad.selectButton);
#endif
            return deviceId;
        }

#if UNITY_EDITOR
        /// <summary>
        /// ゲームパッドの入力データを作成
        /// </summary>
        /// <remarks>Editorでの動作の場合にDirectInput対応のコントローラーの入力を取得する場合 キー配置はホリパッド for Nintendo Switchを想定</remarks>
        /// <param name="_device"></param>
        /// <param name="_outInputData"></param>
        /// <returns></returns>
        public static int CreateDataForJoyStick(InputDevice _device, GamePadStickDeadZone _stickDeadZone, out GamePadInputData _outInputData)
        {
            _outInputData = default;
            GetGamePadDeviceIDResult result = InputDeviceManager.GetGamePadDeviceID(_device, out int deviceId);

            if (deviceId <= GamePadDefine.INVALID_DEVICE_ID) return GamePadDefine.INVALID_DEVICE_ID;
            if (!(_device is Joystick joystick)) return GamePadDefine.INVALID_DEVICE_ID;

            _outInputData.IsEnable = true;
            _outInputData.DeviceId = deviceId;

            Vector2 lStickAxis = FloorVector2(joystick.stick.ReadValue());
            _outInputData.LStickAxisH = GamePadStickDeadZone.CheckDeadZone(lStickAxis.x, _stickDeadZone.LStickDeadZone);
            _outInputData.LStickAxisV = GamePadStickDeadZone.CheckDeadZone(lStickAxis.y, _stickDeadZone.LStickDeadZone);

            int index = joystick.allControls.IndexOf((control) => control.path.LastIndexOf("/z") >= 0);

            if (index >= 0)
            {
                AxisControl axisControl = (AxisControl)joystick.allControls[index];
                _outInputData.RStickAxisH = GamePadStickDeadZone.CheckDeadZone(axisControl.ReadValue(), _stickDeadZone.RStickDeadZone);
            }

            index = joystick.allControls.IndexOf((control) => control.path.LastIndexOf("/rz") >= 0);

            if (index >= 0)
            {
                AxisControl axisControl = (AxisControl)joystick.allControls[index];
                _outInputData.RStickAxisV = GamePadStickDeadZone.CheckDeadZone(axisControl.ReadValue(), _stickDeadZone.RStickDeadZone);
            }

            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button3", GamepadKeyId.A);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button2", GamepadKeyId.B);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button4", GamepadKeyId.X);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "trigger", GamepadKeyId.Y);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button5", GamepadKeyId.L);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button6", GamepadKeyId.R);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button7", GamepadKeyId.LT);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button8", GamepadKeyId.RT);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button9", GamepadKeyId.Select);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button10", GamepadKeyId.Start);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button11", GamepadKeyId.LS);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "button12", GamepadKeyId.RS);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "hat/up", GamepadKeyId.D_Pad_Up);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "hat/down", GamepadKeyId.D_Pad_Down);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "hat/left", GamepadKeyId.D_Pad_Left);
            UpdateKeyStateToJoystick(ref _outInputData, joystick, "hat/right", GamepadKeyId.D_Pad_Right);

            return deviceId;
        }

        /// <summary>
        /// DirectInput対応コントローラーでのキー状態の更新
        /// </summary>
        /// <param name="_refInputData">入力情報</param>
        /// <param name="_joystick">コントローラー</param>
        /// <param name="_name">キーの名前</param>
        /// <param name="_keyId">キーの名前に対応するボタン</param>
        private static void UpdateKeyStateToJoystick(ref GamePadInputData _refInputData, Joystick _joystick, string _name, GamepadKeyId _keyId)
        {
            int index = _joystick.allControls.IndexOf((control) => control.path.LastIndexOf(_name) >= 0);

            if (index >= 0)
            {
                InputControl inputControl = _joystick.allControls[index];
                _refInputData.UpdateKeyState(_keyId, (ButtonControl)inputControl);
            }

        }
#endif

#if !SWITCH_INPUT_ENABLE
        /// <summary>
        /// キーボードの入力データからコントローラーの入力データを作成
        /// </summary>
        /// <param name="_keyboardInputData">キーボードの入力データ</param>
        /// <param name="_outGamepadInputData">コントローラーの入力データ</param>
        /// <returns>デバイスID</returns>
        public static int CreateDataFromKeyboard(KeyboardControlAssignmentData _assignmentData, out GamePadInputData _outGamepadInputData)
        {
            _outGamepadInputData = default;

            if (UnityEngine.InputSystem.Keyboard.current == null) return -1;

            KeyboardKeyState[] keyStateList = KeyboardToGamePadCoverter.ConvertKeyboatdInputToGamePadKeyState(_assignmentData);

            if (keyStateList == null || keyStateList.Length < KeyboardDefine.KEY_STATE_COUNT) return -1;

            _outGamepadInputData.IsEnable = true;
            _outGamepadInputData.DeviceId = UnityEngine.InputSystem.Keyboard.current.deviceId;

            Vector2 lStick = ConvertKeyboardInputToAxis(
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_UP],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_DOWN],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_LEFT],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_RIGHT]);

            Vector2 rStick = ConvertKeyboardInputToAxis(
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_UP],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_DOWN],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_LEFT],
                keyStateList[KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_RIGHT]);

            _outGamepadInputData.LStickAxisH = lStick.x;
            _outGamepadInputData.LStickAxisV = lStick.y;
            _outGamepadInputData.RStickAxisH = rStick.x;
            _outGamepadInputData.RStickAxisV = rStick.y;

            if (InputDeviceManager.Instance.KeyboardInputConvertType == KeyboardInputConvertType.AnyKey)
            {
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.A, KeyboardToGamePadCoverter.GetAnyKeyState());
            }
            else if (InputDeviceManager.Instance.KeyboardInputConvertType == KeyboardInputConvertType.UI)
            {
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.A, keyStateList[KeyboardDefine.KEY_STATES_INDEX_OK]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.B, keyStateList[KeyboardDefine.KEY_STATES_INDEX_OK]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.X, keyStateList[KeyboardDefine.KEY_STATES_INDEX_CANCEL]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.Y, keyStateList[KeyboardDefine.KEY_STATES_INDEX_CANCEL]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.L, keyStateList[KeyboardDefine.KEY_STATES_INDEX_SPACIAL]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.R, keyStateList[KeyboardDefine.KEY_STATES_INDEX_CATCH]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.Start, keyStateList[KeyboardDefine.KEY_STATES_INDEX_MENU]);
            }
            else if (InputDeviceManager.Instance.KeyboardInputConvertType == KeyboardInputConvertType.Player)
            {
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.A, keyStateList[KeyboardDefine.KEY_STATES_INDEX_OK]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.B, keyStateList[KeyboardDefine.KEY_STATES_INDEX_OK]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.L, keyStateList[KeyboardDefine.KEY_STATES_INDEX_SPACIAL]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.R, keyStateList[KeyboardDefine.KEY_STATES_INDEX_CATCH]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.RT, keyStateList[KeyboardDefine.KEY_STATES_INDEX_PUNCH]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.Start, keyStateList[KeyboardDefine.KEY_STATES_INDEX_MENU]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.D_Pad_Up, keyStateList[KeyboardDefine.KEY_STATES_INDEX_EMOTE_1]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.D_Pad_Right, keyStateList[KeyboardDefine.KEY_STATES_INDEX_EMOTE_2]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.D_Pad_Down, keyStateList[KeyboardDefine.KEY_STATES_INDEX_EMOTE_3]);
                UpdateKeyStateFromKeyboard(ref _outGamepadInputData, GamepadKeyId.D_Pad_Left, keyStateList[KeyboardDefine.KEY_STATES_INDEX_EMOTE_4]);
            }

            return _outGamepadInputData.DeviceId;
        }

        /// <summary>
        /// キーボードの入力をアナログスティックの入力に変換
        /// </summary>
        /// <param name="_up">上方向のキー状態</param>
        /// <param name="_down">下方向のキー状態</param>
        /// <param name="_left">左方向のキー状態</param>
        /// <param name="_right">右方向のキー状態</param>
        /// <returns>アナログスティックの入力値に見立てたVerctor2</returns>
        private static Vector2 ConvertKeyboardInputToAxis(KeyboardKeyState _up, KeyboardKeyState _down, KeyboardKeyState _left, KeyboardKeyState _right)
        {
            float x = 0;
            float y = 0;

            x += _left == KeyboardKeyState.Down || _left == KeyboardKeyState.Hold ? -1 : 0;
            x += _right == KeyboardKeyState.Down || _right == KeyboardKeyState.Hold ? 1 : 0;
            y += _up == KeyboardKeyState.Down || _up == KeyboardKeyState.Hold ? 1 : 0;
            y += _down == KeyboardKeyState.Down || _down == KeyboardKeyState.Hold ? -1 : 0;

            if (!Mathf.Approximately(x, 0) && !Mathf.Approximately(y, 0))
            {
                x *= 0.75f;
                y *= 0.75f;
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// キーボードの入力からコントローラーのボタン入力の状態を更新
        /// </summary>
        /// <param name="_refInputData">コントローラーの入力データ</param>
        /// <param name="_keyId">対象のキー</param>
        /// <param name="_keyStateList">キーボードのキーの状態リスト</param>
        private static void UpdateKeyStateFromKeyboard(ref GamePadInputData _refInputData, GamepadKeyId _keyId, params KeyboardKeyState[] _keyStateList)
        {
            KeyboardKeyState result = KeyboardKeyState.None;

            foreach (KeyboardKeyState keyState in _keyStateList)
            {
                if (keyState == KeyboardKeyState.Hold)
                {
                    result = KeyboardKeyState.Hold;
                    break;
                }

                if (keyState == KeyboardKeyState.Down)
                {
                    result = KeyboardKeyState.Down;
                }
                else if (keyState == KeyboardKeyState.Up && result == KeyboardKeyState.None)
                {
                    result = KeyboardKeyState.Up;
                }
            }

            switch (result)
            {
                case KeyboardKeyState.Down: _refInputData.UpdateKeyDown(_keyId, true); break;
                case KeyboardKeyState.Hold: _refInputData.UpdateKeyHold(_keyId, true); break;
                case KeyboardKeyState.Up: _refInputData.UpdateKeyUp(_keyId, true); break;
            }
        }
#endif

        /// <summary>
        /// Vector2の各値を<see cref="FLOOR_MUGNIFICATE"/>で切り捨てた値に変換
        /// </summary>
        /// <param name="_base">元の値</param>
        /// <returns></returns>
        private static Vector2 FloorVector2(Vector2 _base)
        {
            float x = Mathf.FloorToInt(_base.x * FLOOR_MUGNIFICATE) / FLOOR_MUGNIFICATE;
            float y = Mathf.FloorToInt(_base.y * FLOOR_MUGNIFICATE) / FLOOR_MUGNIFICATE;

            return new Vector2(x, y);
        }

        /// <summary>
        /// キー状態の更新
        /// </summary>
        /// <param name="_keyId">キーID</param>
        /// <param name="_buttonControl"></param>
        private void UpdateKeyState(GamepadKeyId _keyId, ButtonControl _buttonControl)
        {
            UpdateKeyDown(_keyId, (_buttonControl.IsPressed() && _buttonControl.wasPressedThisFrame));
            UpdateKeyHold(_keyId, (_buttonControl.IsPressed() && !_buttonControl.wasPressedThisFrame));
            UpdateKeyUp(_keyId, (!_buttonControl.IsPressed() && _buttonControl.wasReleasedThisFrame));
        }

#if SWITCH_INPUT_ENABLE
        /// <summary>
        /// キー状態の更新
        /// </summary>
        /// <remarks>Nitendo Switch実機での動作の場合のみ</remarks>
        /// <param name="_keyId">キーID</param>
        /// <param name="_npadState">コントローラーの状態</param>
        /// <param name="_npadButton">キーIDに該当する実際のコントローラーのボタン</param>
        private void UpdateKeyState( GamepadKeyId _keyId, in NpadState _npadState, NpadButton _npadButton )
        {
            UpdateKeyDown( _keyId, _npadState.GetButtonDown( _npadButton ) );
            UpdateKeyHold( _keyId, _npadState.GetButton( _npadButton ) && !_npadState.GetButtonDown( _npadButton ) && !_npadState.GetButtonUp( _npadButton ) );
            UpdateKeyUp( _keyId, _npadState.GetButtonUp( _npadButton ) );
        }
#endif

        /// <summary>
        /// 指定のボタンを押下したかのフラグを更新
        /// </summary>
        /// <param name="_keyId">ボタンの種類</param>
        /// <param name="_isDown">押下している場合はtrue</param>
        private void UpdateKeyDown(GamepadKeyId _keyId, bool _isDown)
        {
            if (_isDown) m_ButtonDown |= _keyId;
            else m_ButtonDown &= ~_keyId;
        }

        /// <summary>
        /// 指定のボタンを押下し続けているかのフラグを更新
        /// </summary>
        /// <param name="_keyId">ボタンの種類</param>
        /// <param name="_isHold">trueなら押し続けている</param>
        private void UpdateKeyHold(GamepadKeyId _keyId, bool _isHold)
        {
            if (_isHold) m_ButtonHold |= _keyId;
            else m_ButtonHold &= ~_keyId;
        }

        /// <summary>
        /// 指定のボタンを離したかどうかのフラグを更新
        /// </summary>
        /// <param name="_keyId">ボタンの種類</param>
        /// <param name="_isUp">trueならボタンを離した</param>
        private void UpdateKeyUp(GamepadKeyId _keyId, bool _isUp)
        {
            if (_isUp) m_ButtonUp |= _keyId;
            else m_ButtonUp &= ~_keyId;
        }

        /// <summary>
        /// この入力データにボタン・アナログスティックのいづれかの入力データが存在するかの判定
        /// </summary>
        /// <remarks>入力データの取得できないボタン(Nitendo Switchの場合ホーム・スクリーンショット)は判定対象外</remarks>
        /// <returns></returns>
        public bool IsSomeInput()
        {
            if (IsAnyKeyDown) return true;
            if (IsAnyKeyHold) return true;
            if (IsAnyKeyUp) return true;

            if (!Mathf.Approximately(LStickAxisH, 0)) return true;
            if (!Mathf.Approximately(LStickAxisV, 0)) return true;
            if (!Mathf.Approximately(RStickAxisH, 0)) return true;
            if (!Mathf.Approximately(RStickAxisV, 0)) return true;

            return false;
        }

        public override string ToString()
        {
            string result = "";

            result += " Down[" + ButtonDown.ToInt().ToString() + "]";
            result += " Hold[" + ButtonHold.ToInt().ToString() + "]";
            result += " Up[" + ButtonUp.ToInt().ToString() + "]";
            result += " LS:" + LStickAxis;
            result += " RS:" + RStickAxis;

            return result;
        }

        ///// <summary>
        ///// <see cref="NetworkGamepadInputData"/>に変換
        ///// </summary>
        ///// <returns></returns>
        //public NetworkGamepadInputData ToNetworkGamepadInputData()
        //{
        //    NetworkGamepadInputData result = new NetworkGamepadInputData()
        //    {
        //        ButtonDown = ButtonDown,
        //        ButtonHold = ButtonHold,
        //        ButtonUp = ButtonUp,
        //        isEnable = IsEnable,
        //        LStickAxisH = LStickAxisH,
        //        LStickAxisV = LStickAxisV,
        //        RStickAxisH = RStickAxisH,
        //        RStickAxisV = RStickAxisV,
        //    };

        //    return result;
        //}

        ///// <summary>
        ///// <see cref="NetworkGamepadInputData"/>から取得
        ///// </summary>
        ///// <param name="_data"></param>
        //public void FromNetworkGamepadInputData( NetworkGamepadInputData _data )
        //{
        //    m_ButtonDown = _data.ButtonDown;
        //    m_ButtonHold = _data.ButtonHold;
        //    m_ButtonUp = _data.ButtonUp;
        //    IsEnable = _data.isEnable;
        //    LStickAxisH = _data.LStickAxisH;
        //    LStickAxisV = _data.LStickAxisV;
        //    RStickAxisH = _data.RStickAxisH;
        //    RStickAxisV = _data.RStickAxisV;
        //}

        //public void ResetButtonDownAndUp()
        //{
        //    m_ButtonDown = GamepadKeyId.None;
        //    m_ButtonUp = GamepadKeyId.None;
        //}
    }
}