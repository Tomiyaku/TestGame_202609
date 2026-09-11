using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// コントローラー入力情報
    /// </summary>
    public struct NSGamePadInputData
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
        public Vector2 LStickAxis => new Vector2( LStickAxisH, LStickAxisV );
        /// <summary>右スティック入力値</summary>
        public Vector2 RStickAxis => new Vector2( RStickAxisH, RStickAxisV );

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
        }

        /// <summary>
        /// 指定のボタンを押下したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsDown( GamepadKeyId _keyId )
        {
            return ( m_ButtonDown & _keyId ) == _keyId;
        }

        /// <summary>
        /// 指定のボタンを押下し続けているか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsHold( GamepadKeyId _keyId )
        {
            return ( m_ButtonHold & _keyId ) == _keyId;
        }

        /// <summary>
        /// 指定のボタンを離したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsUp( GamepadKeyId _keyId )
        {
            return ( m_ButtonUp & _keyId ) == _keyId;
        }

        /// <summary>
        /// いづれかのボタンを押下したか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyDown => m_ButtonDown > 0;

        /// <summary>
        /// いづれかのボタンを押下し続けているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyHold => m_ButtonHold > 0;

        /// <summary>
        /// いづれかのボタンを離したか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyUp => m_ButtonUp > 0;

        /// <summary>
        /// 指定のキーの同時押下
        /// </summary>
        /// <param name="_keyId">同時押しを判定するキー 論理OR演算子で複数キーを指定</param>
        /// <param name="_validityTime">同時押しの有効時間</param>
        /// <returns><see cref="MultipleDownResult"/></returns>
        public MultipleDownResult IsMultipleDown( GamepadKeyId _keyId, float _validityTime = NSGamePadDefine.MULTIPLE_DOWN_TIME )
        {
            //同時押し判定のキーをリスト化
            List<GamepadKeyId> keyIdList = new List<GamepadKeyId>();

            foreach( GamepadKeyId key in Enum.GetValues( typeof( GamepadKeyId ) ) )
            {
                if( key == GamepadKeyId.None ) continue;

                if( ( _keyId & key ) == key ) keyIdList.Add( key );
            }

            //引数で渡されたキーの数が1以下の場合は同時押し無しとして返す
            if( keyIdList.Count <= 1 ) return MultipleDownResult.MissingKey;

            List<NSGamePadHistory> inputHistoryList = NSInputManager.Instance.GetGamePadHistory( DeviceId );

            //履歴データが取得できなかったので同時押し無しとして返す
            if( inputHistoryList == null ) return MultipleDownResult.MissingHistory;

            //キー毎の押下時間のリスト -1で初期化
            float[] keyDownTimeList = Enumerable.Repeat<float>( -1, keyIdList.Count ).ToArray();
            bool isNowDown = false;

            //入力履歴から指定のキーの押下した時間を取得
            for( int i = 0; i < keyIdList.Count; i++ )
            {
                if( IsDown( keyIdList[ i ] ) ) isNowDown = true;

                NSGamePadHistory history = inputHistoryList.Find( ( NSGamePadHistory inputHisty ) => inputHisty.InputData.IsDown( keyIdList[ i ] ) );

                if( history != null && history.IsEnable ) keyDownTimeList[ i ] = history.GetDataTime;
            }

            //いづれかのボタンが今押された瞬間ではない場合は同時押し無しとする
            if( !isNowDown ) return MultipleDownResult.Failure;

            float newestTime = -1;
            float oldestTime = -1;

            //押下時間の最新と最古の時間を探す
            for( int i = 0; i < keyDownTimeList.Length; i++ )
            {
                //押下した時間が取得でき無かったキーがある場合は同時押し無しとして返す
                if( keyDownTimeList[ i ] < 0 ) return MultipleDownResult.Failure;

                if( newestTime < 0 || newestTime < keyDownTimeList[ i ] ) newestTime = keyDownTimeList[ i ];
                if( oldestTime < 0 || oldestTime > keyDownTimeList[ i ] ) oldestTime = keyDownTimeList[ i ];
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
        public HoldingResult IsHoldingKeyForKeepTime( GamepadKeyId _keyId, float _time )
        {
            if( !IsHold( _keyId ) ) return HoldingResult.NotNowHold;

            return NSInputManager.Instance.IsHoldingKeyForKeepTime( DeviceId, _keyId, _time );
        }

        /// <summary>
        /// ゲームパッドの入力データを作成
        /// </summary>
        /// <param name="_device">InputDevice</param>
        /// <param name="_inputData">取得した入力データが格納</param>
        /// <returns>デバイスID</returns>
        public static int CreateData( InputDevice _device, ref NSGamePadInputData _inputData )
        {
            int id = NSInputManager.GetDeviceID( _device );

            if( id <= NSGamePadDefine.INVALID_DEVICE_ID ) return NSGamePadDefine.INVALID_DEVICE_ID;
            if( !( _device is Gamepad gamepad ) ) return NSGamePadDefine.INVALID_DEVICE_ID;

            _inputData.IsEnable = true;
            _inputData.DeviceId = id;

            Vector2 lStickAxis = FloorVector2( gamepad.leftStick.ReadValue() );
            Vector2 rSrickAxis = FloorVector2( gamepad.rightStick.ReadValue() );

            _inputData.LStickAxisH = lStickAxis.x;
            _inputData.LStickAxisV = lStickAxis.y;
            _inputData.RStickAxisH = rSrickAxis.x;
            _inputData.RStickAxisV = rSrickAxis.y;

            _inputData.UpdateKeyState( GamepadKeyId.A, gamepad.buttonEast );
            _inputData.UpdateKeyState( GamepadKeyId.B, gamepad.buttonSouth );
            _inputData.UpdateKeyState( GamepadKeyId.X, gamepad.buttonNorth );
            _inputData.UpdateKeyState( GamepadKeyId.Y, gamepad.buttonWest );

            _inputData.UpdateKeyState( GamepadKeyId.L, gamepad.leftShoulder );
            _inputData.UpdateKeyState( GamepadKeyId.R, gamepad.rightShoulder );
            _inputData.UpdateKeyState( GamepadKeyId.ZL, gamepad.leftTrigger );
            _inputData.UpdateKeyState( GamepadKeyId.ZR, gamepad.rightTrigger );

            _inputData.UpdateKeyState( GamepadKeyId.LS, gamepad.leftStickButton );
            _inputData.UpdateKeyState( GamepadKeyId.RS, gamepad.rightStickButton );

            _inputData.UpdateKeyState( GamepadKeyId.D_Pad_Up, gamepad.dpad.up );
            _inputData.UpdateKeyState( GamepadKeyId.D_Pad_Down, gamepad.dpad.down );
            _inputData.UpdateKeyState( GamepadKeyId.D_Pad_Left, gamepad.dpad.left );
            _inputData.UpdateKeyState( GamepadKeyId.D_Pad_Right, gamepad.dpad.right );

            _inputData.UpdateKeyState( GamepadKeyId.Plus, gamepad.startButton );
            _inputData.UpdateKeyState( GamepadKeyId.Minus, gamepad.selectButton );

            return id;
        }

#if UNITY_EDITOR
        /// <summary>
        /// ゲームパッドの入力データを作成
        /// </summary>
        /// <remarks>Editorでの動作の場合にDirectInput対応のコントローラーの入力を取得する場合 キー配置はホリパッド for Nintendo Switchを想定</remarks>
        /// <param name="_device"></param>
        /// <param name="_inputData"></param>
        /// <returns></returns>
        public static int CreateDataForJoyStick( InputDevice _device, ref NSGamePadInputData _inputData )
        {
            int id = NSInputManager.GetDeviceID( _device );

            if( id <= NSGamePadDefine.INVALID_DEVICE_ID ) return NSGamePadDefine.INVALID_DEVICE_ID;
            if( !( _device is Joystick joystick ) ) return NSGamePadDefine.INVALID_DEVICE_ID;

            _inputData.IsEnable = true;
            _inputData.DeviceId = id;

            Vector2 lStickAxis = FloorVector2( joystick.stick.ReadValue() );
            _inputData.LStickAxisH = lStickAxis.x;
            _inputData.LStickAxisV = lStickAxis.y;

            int index = joystick.allControls.IndexOf( ( control ) => control.path.LastIndexOf( "/z" ) >= 0 );

            if( index >= 0 )
            {
                AxisControl axisControl = ( AxisControl )joystick.allControls[ index ];
                _inputData.RStickAxisH = axisControl.ReadValue();
            }

            index = joystick.allControls.IndexOf( ( control ) => control.path.LastIndexOf( "/rz" ) >= 0 );

            if( index >= 0 )
            {
                AxisControl axisControl = ( AxisControl )joystick.allControls[ index ];
                _inputData.RStickAxisV = axisControl.ReadValue();
            }

            UpdateKeyStateToJoystick( ref _inputData, joystick, "button3", GamepadKeyId.A );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button2", GamepadKeyId.B );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button4", GamepadKeyId.X );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "trigger", GamepadKeyId.Y );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button5", GamepadKeyId.L );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button6", GamepadKeyId.R );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button7", GamepadKeyId.ZL );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button8", GamepadKeyId.ZR );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button9", GamepadKeyId.Minus );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button10", GamepadKeyId.Plus );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button11", GamepadKeyId.LS );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "button12", GamepadKeyId.RS );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "hat/up", GamepadKeyId.D_Pad_Up );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "hat/down", GamepadKeyId.D_Pad_Down );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "hat/left", GamepadKeyId.D_Pad_Left );
            UpdateKeyStateToJoystick( ref _inputData, joystick, "hat/right", GamepadKeyId.D_Pad_Right );

            return id;
        }

        private static void UpdateKeyStateToJoystick( ref NSGamePadInputData _inputData, Joystick _joystick, string _name, GamepadKeyId _keyId )
        {
            int index = _joystick.allControls.IndexOf( ( control ) => control.path.LastIndexOf( _name ) >= 0 );

            if( index >= 0 )
            {
                InputControl inputControl = _joystick.allControls[ index ];
                _inputData.UpdateKeyState( _keyId, ( ButtonControl )inputControl );
            }

        }
#endif

        /// <summary>
        /// Vector2の各値を<see cref="FLOOR_MUGNIFICATE"/>で切り捨てた値に変換
        /// </summary>
        /// <param name="_base"></param>
        /// <returns></returns>
        private static Vector2 FloorVector2( Vector2 _base )
        {
            float x = Mathf.FloorToInt( _base.x * FLOOR_MUGNIFICATE ) / FLOOR_MUGNIFICATE;
            float y = Mathf.FloorToInt( _base.y * FLOOR_MUGNIFICATE ) / FLOOR_MUGNIFICATE;

            return new Vector2( x, y );
        }

        /// <summary>
        /// キー状態の更新
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_buttonControl"></param>
        private void UpdateKeyState( GamepadKeyId _keyId, ButtonControl _buttonControl )
        {
            UpdateKeyDown( _keyId, ( _buttonControl.IsPressed() && _buttonControl.wasPressedThisFrame ) );
            UpdateKeyHold( _keyId, ( _buttonControl.IsPressed() && !_buttonControl.wasPressedThisFrame ) );
            UpdateKeyUp( _keyId, ( !_buttonControl.IsPressed() && _buttonControl.wasReleasedThisFrame ) );
        }

        /// <summary>
        /// 指定のボタンを押下したかのフラグを更新
        /// </summary>
        /// <param name="_flag">ボタンの種類</param>
        /// <param name="_isDown">押下している場合はtrue</param>
        private void UpdateKeyDown( GamepadKeyId _keyId, bool _isDown )
        {
            if( _isDown ) m_ButtonDown |= _keyId;
            else m_ButtonDown &= ~_keyId;
        }

        /// <summary>
        /// 指定のボタンを押下し続けているかのフラグを更新
        /// </summary>
        /// <param name="_keyId">ボタンの種類</param>
        /// <param name="_isHold">trueなら押し続けている</param>
        private void UpdateKeyHold( GamepadKeyId _keyId, bool _isHold )
        {
            if( _isHold ) m_ButtonHold |= _keyId;
            else m_ButtonHold &= ~_keyId;
        }

        /// <summary>
        /// 指定のボタンを離したかどうかのフラグを更新
        /// </summary>
        /// <param name="_keyId">ボタンの種類</param>
        /// <param name="_isUp">trueならボタンを離した</param>
        private void UpdateKeyUp( GamepadKeyId _keyId, bool _isUp )
        {
            if( _isUp ) m_ButtonUp |= _keyId;
            else m_ButtonUp &= ~_keyId;
        }

        /// <summary>
        /// この入力データにボタン・アナログスティックのいづれかの入力データが存在するかの判定
        /// </summary>
        /// <remarks>入力データの取得できないボタン(Nitendo Switchの場合ホーム・スクリーンショット)は判定対象外</remarks>
        /// <returns></returns>
        public bool IsSomeInput()
        {
            if( IsAnyKeyDown ) return true;
            if( IsAnyKeyHold ) return true;
            if( IsAnyKeyUp ) return true;

            if( !Mathf.Approximately( LStickAxisH, 0 ) ) return true;
            if( !Mathf.Approximately( LStickAxisV, 0 ) ) return true;
            if( !Mathf.Approximately( RStickAxisH, 0 ) ) return true;
            if( !Mathf.Approximately( RStickAxisV, 0 ) ) return true;

            return false;
        }
    }
}