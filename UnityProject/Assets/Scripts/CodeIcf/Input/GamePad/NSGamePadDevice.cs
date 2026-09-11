using UnityEngine;
using UnityEngine.InputSystem;

using CodeIcf.Extensions;
using System;

namespace CodeIcf.Input.GamePad
{
    [System.Serializable]
    /// <summary>
    /// 入力デバイス(基本コントローラー)の入力・状態データ
    /// </summary>
    public class NSGamePadDevice
    {
        /// <summary>どのゲームパッドかを判別するID</summary>
        /// <remarks>実行環境によって格納される元の値が異なる<br></br>UnityEditorでは<see cref="InputDevice.deviceId"/>(int型)<br></br>NintendoSwitchでは<see cref="NPad.NpadId"/>(byte型)</remarks>
        private int m_DeviceId;
        /// <summary>デバイスID</summary>
        public int DeviceId => m_DeviceId;
        /// <summary>コントローラーの持ち方</summary>
        /// <remarks>UnityEditor上ではコントローラーが繋がっていれば常に<see cref="ControllerStyle.FullKey"/>を返す</remarks>
        public ControllerStyle ControllerStyle { get; private set; } = ControllerStyle.None;
        /// <summary>コントローラーが接続されているkaのフラグ</summary>
        public bool IsConnected { get; private set; } = false;
        /// <summary>コントローラーの接続状態が最後に変わった際の時間</summary>
        public float LastUpdateTime { get; private set; } = -1;
        /// <summary>入力データ</summary>
        public NSGamePadInputData InputData { get; private set; }
        /// <summary>左スティックの入力を十字キーに変換</summary>
        private NSAxisToDPadConvertion m_LeftAxisToDPad = new NSAxisToDPadConvertion();
        public NSAxisToDPadConvertion LeftAxisToDPad => m_LeftAxisToDPad;
        /// <summary>右スティックの入力を十字キーに変換</summary>
        private NSAxisToDPadConvertion m_RightAxisToDPad = new NSAxisToDPadConvertion();
        public NSAxisToDPadConvertion RightAxisToDPad => m_RightAxisToDPad;
        /// <summary>キーのリピート入力判定</summary>
        private NSGamePadKeyRepeatChecker m_KeyRepeatChacker = new NSGamePadKeyRepeatChecker();
        /// <summary>コントローラーの色</summary>
        public Color[] ControllerColor { get; private set; } = null;
        /// <summary>プレイヤーランプの点パターン</summary>
        /// <remarks>下位4Bitを使用して表現 最下位Bitが左端のプレイヤーランプの点灯状況</remarks>
        public byte LeaPattern { get; private set; } = 0;

        public bool IsInputtemporarilyDisabled { get; private set; } = false;

        public void InputtemporarilyDisabled() => IsInputtemporarilyDisabled = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private NSGamePadDevice() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_deviceId"></param>
        public NSGamePadDevice( int _deviceId )
        {
            m_DeviceId = _deviceId;

            ControllerStyle = NSInputManager.GetControllerStyle( DeviceId );
            ControllerColor = NSInputManager.GetControllerColor( DeviceId );
            LeaPattern = NSInputManager.GetPlayerLedPattern( DeviceId );
        }

        /// <summary>
        /// 入力データの設定
        /// </summary>
        /// <param name="_inputData"></param>
        public void SetInputData( ref NSGamePadInputData _inputData )
        {
            InputData = _inputData;

            m_LeftAxisToDPad.Update( InputData.LStickAxisH, InputData.LStickAxisV );
            m_RightAxisToDPad.Update( InputData.RStickAxisH, InputData.RStickAxisV );

            foreach( GamepadKeyId keyid in Enum.GetValues( typeof( GamepadKeyId ) ) )
            {
                if( keyid == GamepadKeyId.None ) continue;

                if( InputData.IsDown( keyid ) ) m_KeyRepeatChacker.SetKeyDownTime( keyid );
                if( InputData.IsHold( keyid ) ) m_KeyRepeatChacker.CheckHoldRepeat( keyid );
                if( InputData.IsUp( keyid ) ) m_KeyRepeatChacker.SetKeyUpTime( keyid );
            }

            if( InputData.IsEnable ) LastUpdateTime = Time.unscaledTime;

            IsInputtemporarilyDisabled = false;
        }

        /// <summary>
        /// 接続状態の更新
        /// </summary>
        /// <param name="_isConnected">コントローラーの接続状態</param>
        public void UpdateState( bool _isConnected )
        {
            if( IsConnected != _isConnected )
            {
                IsConnected = _isConnected;
                LastUpdateTime = Time.unscaledTime;
            }

            if( IsConnected )
            {//コントローラー接続中の場合のみ、持ち方・色・プレイヤーランプの状態を更新
                ControllerStyle nowStyle = NSInputManager.GetControllerStyle( DeviceId );

                if( nowStyle != ControllerStyle )
                {
                    ControllerStyle = nowStyle;
                    ControllerColor = NSInputManager.GetControllerColor( DeviceId );
                    LeaPattern = NSInputManager.GetPlayerLedPattern( DeviceId );
                }
            }
        }

        /// <summary>
        /// 入力データの削除
        /// </summary>
        public void RemoveInputData()
        {
            InputData = default;
        }

        /// <summary>
        /// プレイヤーランプのLEDが点灯しているかの判定
        /// </summary>
        /// <param name="_index">点灯状況を確認するLED(横持ちの場合、左端を0)のインデックス</param>
        /// <returns>tureの場合は点灯 falseの場合は点灯していないor引数が範囲外</returns>
        public bool IsPLayerLedLit( int _index )
        {
            if( _index < 0 || _index >= NSGamePadDefine.PLAYER_LED_COUNT ) return false;

            byte ledIndex = ( byte )( 0x1 << _index );

            return LeaPattern.EqualLogicAnd( ledIndex );
        }

        /// <summary>
        /// 指定のキーのリピート入力が有効かの判定
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        /// <returns>有効になっていればtrue</returns>
        public bool IsEnableRepeat( GamepadKeyId _keyId )
        {
            return m_KeyRepeatChacker.IsEnableRepeat( _keyId );
        }

        /// <summary>
        /// 指定のキーのリピート入力を有効にする
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_interval"></param>
        /// <returns></returns>
        public bool EnableRepaet( GamepadKeyId _keyId, float _interval )
        {
            return m_KeyRepeatChacker.EnableRepaet( _keyId, _interval );
        }

        /// <summary>
        /// 指定のキーのリピート入力を停止する
        /// </summary>
        /// <param name="_keyId"></param>
        public void StopRepeat( GamepadKeyId _keyId )
        {
            m_KeyRepeatChacker.StopRepeat( _keyId );
        }

        /// <summary>
        /// 全てのリピート入力を停止する
        /// </summary>
        public void StopAllRepeat()
        {
            m_KeyRepeatChacker.StopAllRepeat();
        }

        /// <summary>
        /// 指定のボタンを押下したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsDown( GamepadKeyId _keyId )
        {
            if( IsInputtemporarilyDisabled ) return false;

            return InputData.IsDown( _keyId );
        }

        /// <summary>
        /// 指定のボタンを押下し続けているか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsHold( GamepadKeyId _keyId )
        {
            if( IsInputtemporarilyDisabled ) return false;

            return InputData.IsHold( _keyId );
        }

        /// <summary>
        /// 指定のボタンを離したか
        /// </summary>
        /// <param name="_keyId">ボタンID</param>
        /// <returns></returns>
        public bool IsUp( GamepadKeyId _keyId )
        {
            if( IsInputtemporarilyDisabled ) return false;

            return InputData.IsUp( _keyId );
        }

        /// <summary>
        /// いづれかのボタンを押下したか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyDown => !IsInputtemporarilyDisabled && InputData.IsAnyKeyDown;

        /// <summary>
        /// いづれかのボタンを押下し続けているか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyHold => !IsInputtemporarilyDisabled && InputData.IsAnyKeyHold;

        /// <summary>
        /// いづれかのボタンを離したか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyUp => !IsInputtemporarilyDisabled && InputData.IsAnyKeyUp;

        /// <summary>
        /// 指定のキーがリピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsRepeat( GamepadKeyId _keyId )
        {
            if( IsInputtemporarilyDisabled ) return false;

            return m_KeyRepeatChacker.IsRepeat( _keyId );
        }

        /// <summary>
        /// 指定のキーが押されたか or リピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsDownOrRepeat( GamepadKeyId _keyId )
        {
            if( IsInputtemporarilyDisabled ) return false;

            return IsDown( _keyId ) || IsRepeat( _keyId );
        }

        /// <summary>
        /// 十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか
        /// </summary>
        /// <param name="_dpadKeyId">十字キーのキーID</param>
        /// <param name="_confirm">入力判定指定</param>        
        /// <returns></returns>
        public bool IsDpadDown( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            if( IsInputtemporarilyDisabled ) return false;

            if( _dpadKeyId == DPad.None ) return false;

            if( ( _confirm & SelectConfirmInput.Dpad ) == SelectConfirmInput.Dpad )
            {
                GamepadKeyId keyId = GamepadKeyId.None;

                if( ( _dpadKeyId & DPad.Up ) == DPad.Up ) keyId |= GamepadKeyId.D_Pad_Up;
                if( ( _dpadKeyId & DPad.Down ) == DPad.Down ) keyId |= GamepadKeyId.D_Pad_Down;
                if( ( _dpadKeyId & DPad.Left ) == DPad.Left ) keyId |= GamepadKeyId.D_Pad_Left;
                if( ( _dpadKeyId & DPad.Right ) == DPad.Right ) keyId |= GamepadKeyId.D_Pad_Right;

                if( keyId != GamepadKeyId.None && InputData.IsDown( keyId ) ) return true;
            }

            if( ( _confirm & SelectConfirmInput.LeftStick_Digital ) == SelectConfirmInput.LeftStick_Digital )
            {
                if( LeftAxisToDPad.IsDown( _dpadKeyId ) ) return true;
            }

            if( ( _confirm & SelectConfirmInput.RightStick_Digital ) == SelectConfirmInput.RightStick_Digital )
            {
                if( RightAxisToDPad.IsDown( _dpadKeyId ) ) return true;
            }

            return false;
        }

        /// <summary>
        /// 十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーがリピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId">十字キーのキーID</param>
        /// <param name="_confirm">入力判定指定</param>        
        /// <returns></returns>
        public bool IsDpadRepeat( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            if( IsInputtemporarilyDisabled ) return false;

            if( _dpadKeyId == DPad.None ) return false;

            if( ( _confirm & SelectConfirmInput.Dpad ) == SelectConfirmInput.Dpad )
            {
                GamepadKeyId keyId = GamepadKeyId.None;

                if( ( _dpadKeyId & DPad.Up ) == DPad.Up ) keyId |= GamepadKeyId.D_Pad_Up;
                if( ( _dpadKeyId & DPad.Down ) == DPad.Down ) keyId |= GamepadKeyId.D_Pad_Down;
                if( ( _dpadKeyId & DPad.Left ) == DPad.Left ) keyId |= GamepadKeyId.D_Pad_Left;
                if( ( _dpadKeyId & DPad.Right ) == DPad.Right ) keyId |= GamepadKeyId.D_Pad_Right;

                if( keyId != GamepadKeyId.None && IsRepeat( keyId ) ) return true;
            }

            if( ( _confirm & SelectConfirmInput.LeftStick_Digital ) == SelectConfirmInput.LeftStick_Digital )
            {
                if( LeftAxisToDPad.IsRepeat( _dpadKeyId ) ) return true;
            }

            if( ( _confirm & SelectConfirmInput.RightStick_Digital ) == SelectConfirmInput.RightStick_Digital )
            {
                if( RightAxisToDPad.IsRepeat( _dpadKeyId ) ) return true;
            }

            return false;
        }

        /// <summary>
        /// 十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか or リピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId">十字キーのキーID</param>
        /// <param name="_confirm">入力判定指定</param>        
        /// <returns></returns>
        public bool IsDpadDownOrRepeat( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            if( IsInputtemporarilyDisabled ) return false;

            if( IsDpadDown( _dpadKeyId, _confirm ) ) return true;
            if( IsDpadRepeat( _dpadKeyId, _confirm ) ) return true;

            return false;
        }
    }
}
