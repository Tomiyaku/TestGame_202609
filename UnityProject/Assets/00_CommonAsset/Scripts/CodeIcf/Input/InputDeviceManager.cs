#if UNITY_SWITCH && !UNITY_EDITOR
#define SWITCH_INPUT_ENABLE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if SWITCH_INPUT_ENABLE
using UnityEngine.Switch;
using UnityEngine.InputSystem.Switch;
using nn.hid;
#endif

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;
using CodeIcf.Input.GamePad;
using CodeIcf.Input.Keyboard;
using CodeIcf.Input.NSKeyboard;
using CodeIcf.Input.NSMouse;
using CodeIcf.Input.PointerDevice;
using CodeIcf.Switch;

using Time = UnityEngine.Time;

namespace CodeIcf.Input
{
    /// <summary>
    /// InputSystemを利用した複数コントローラー対応Nintendo Switch向け入力管理クラス
    /// </summary>
    [DefaultExecutionOrder( -90 )]
    public class InputDeviceManager : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static InputDeviceManager m_Instance = null;
        /// <summary>インスタンス</summary>
        public static InputDeviceManager Instance
        {
            get
            {
                CreateInstance();

                return m_Instance;
            }
        }
        /// <summary>現在の状態がTVモードかのフラグ</summary>
        [field: SerializeField]
        public bool IsConsole { get; private set; } = false;
        /// <summary>コントローラーの接続が切れた場合、一定時間後に自動でコントローラーサポートアプレットを表示するか</summary>
        [field: SerializeField]
        public bool IsAutoCheckDeviceConnect { get; private set; } = true;
        /// <summary>コントローラーの最小接続数未満の場合、一定時間後にサポートアプレットを表示するか</summary>
        [field: SerializeField]
        public bool IsAutoCheckMinimumDeviceCount { get; private set; } = true;
        /// <summary>既に接続済みのコントローラーの接続を維持するか</summary>
        [field: SerializeField]
        public bool IsEnableTakeOverConnection { get; private set; } = true;
        /// <summary>入力デバイスの最小接続数数</summary>
        [field: SerializeField]
        public byte MinDeviceCount { get; private set; } = 0;
        /// <summary>入力デバイスの最大接続数</summary>
        [field: SerializeField]
        public byte MaxDeviceCount { get; private set; } = 0;
        /// <summary>デバイスの数が最後に更新された時の時間</summary>
        [SerializeField]
        private float m_LastUpdatedDeviceCountTime = -1;
        /// <summary>全てのコントローラーの入力・状態データ一覧</summary>
        [SerializeField]
        private List<GamePadDevice> m_GamePadDeviceList = new List<GamePadDevice>();
        /// <summary>全てのポインティングデバイスの入力データ</summary>
        private List<PointerData> m_PointerDataList = new List<PointerData>();
        /// <summary>全てのポインティングデバイスの入力データ</summary>
        public List<PointerData> PointerDataList => m_PointerDataList;
        /// <summary>Nintendo Swichでキーボードの入力情報を取得するか</summary>
        public bool IsGetNSKeyboardState { get; set; } = false;
        /// <summary>Nintendo Swichでマウスの入力情報を取得するか</summary>
        public bool IsGetNSMouseState { get; set; } = false;
        /// <summary>キーボードの入力アサインデータ</summary>
        [SerializeField]
        private KeyboardControlAssignmentData m_KeyboardControlAssignmentData = null;
        /// <summary>キーボードの入力をコントローラー入力に変換する際の種類</summary>
        public KeyboardInputConvertType KeyboardInputConvertType { get; set; } = KeyboardInputConvertType.UI;

        public KeyboardInputData KeyboardInputData { get; private set; } = null;

        /// <summary>キーボード入力をコントローラー入力に変換するかの判定</summary>
        public FlagCheckFunc IsConvertKeyboardToGamePadInput { get; set; }

        /// <summary>接続されているデバイスの数</summary>
        [SerializeField]
        private int m_ConnectedDeviceCount = -1;
        /// <summary>接続されているデバイスの数</summary>
        public int ConnectedDeviceCount => m_ConnectedDeviceCount;
        /// <summary>最後に入力データが更新された時間</summary>
        private float m_LastGotInputDataTime = -1;

        /// <summary>入力データの履歴</summary>
        private List<InputHistory> m_InputHistoryList = new List<InputHistory>();
        /// <summary>入力データの履歴の最大サイズ</summary>
        [SerializeField]
        private int m_HistoryMaxSize = 256;
        /// <summary>入力データの履歴の最大サイズ</summary>
        public static int HistoryMaxSize => m_Instance == null ? 0 : m_Instance.m_HistoryMaxSize;
        /// <summary>１フレーム中に入力データが取得できたデバイスID一覧</summary>
        private List<int> m_DeviceIdListToInputDataAdd = new List<int>();
        /// <summary>コントローラーサポートアプレットを閉じた後に呼び出す処理</summary>
        public System.Action ResetedDeviceProcess { get; set; }

        /// <summary>コントローラーサポートアプレットを呼び出すまでの待ち時間(秒)</summary>
        private float m_WaitTimeToShowControllerSupport = GamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

#if SWITCH_INPUT_ENABLE
        ///有効なコントローラーのNpadId一覧
        private List<NpadId> m_EnableNpadIdList = new List<NpadId>();
#endif
        /// <summary>入力データの取得を更新するかのフラグ</summary>
        private bool m_IsCorrectInput = true;

        /// <summary>コントローラーの最小接続数未満の場合に一定時間経過したときの処理を行うか </summary>
        //※各箇所で設定しているIsAutoCheckMinimumDeviceCountとは別に、インプット内部処理として止めたいので別物を用意
        private bool m_IsCheckControllerMinimumNum = true;

        private GamePadStickDeadZone m_StickDeadZone = new GamePadStickDeadZone();

        /// <summary>入力データの取得を更新するかのフラグ</summary>
        /// <remarks>falseを設定すると現状の履歴等は全て削除し、入力の取得やコントローラーの状態チェックを行わない</remarks>
        public bool IsCorrectInput
        {
            get => m_IsCorrectInput;
            set
            {
                m_IsCorrectInput = value;

                if( !m_IsCorrectInput )
                {
                    Instance.m_DeviceIdListToInputDataAdd.Clear();
                    Instance.m_GamePadDeviceList.Clear();
                    Instance.ClearHistory();
                    Instance.m_LastGotInputDataTime = -1;
                    Instance.m_LastUpdatedDeviceCountTime = -1;
                    Instance.m_ConnectedDeviceCount = -1;
                }
            }
        }

        /// <summary>Joy-Con の L/R ボタン押しによる操作スタイル割り当てモードが有効かのフラグ</summary>
        public bool IsLRSAssignmentMode { get; private set; } = false;

        private const float ANY_KEY_CHECK_DELAY = 0.15f;
        private float m_AnyGamePadKeyCheckDelayStartTime = -1;

        /// <summary>ゲームパッドの数が変更されたときに実行する処理</summary>
        public System.Action GamepadCountChangedProcess { get; set; } = null;
        /// <summary>コントローラーの状態を更新するかのフラグ</summary>
        public bool IsUpdateDeviceState { get; set; } = true;
        /// <summary>コントローラーが接続されていない場合等でコントローラーサポートアプレットを表示する代わりに実行する処理</summary>
        public System.Action ControllerSuportProcerss { get; set; } = null;

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        public static void CreateInstance()
        {
            if( m_Instance == null )
            {
                m_Instance = FindAnyObjectByType<InputDeviceManager>();

                if( m_Instance == null )
                {
                    GameObject obj = new GameObject( "InputDeviceManager" );
                    m_Instance = obj.AddComponent<InputDeviceManager>();

                    m_Instance.IsConvertKeyboardToGamePadInput = new FlagCheckFunc();

                    DontDestroyOnLoad( m_Instance );
                }
            }
        }

        private void Awake()
        {
            if( m_Instance == null )
            {
                m_Instance = this;
            }
            else if( m_Instance != this )
            {
                Destroy( gameObject );
                return;
            }

            DontDestroyOnLoad( this );
            SetInputSetting( GamePadSetting.DefaultSetting() );
            m_StickDeadZone.ResetDefault();
        }

        private void Start()
        {
#if SWITCH_INPUT_ENABLE
            NotificationFromSwitch.Instance.AddOperationModeChangedNotification( ChangedOperationMode );
            NotificationFromSwitch.Instance.AddFocusStateChangedNotification( ChangeFocusState );

            IsConsole = Operation.mode == Operation.OperationMode.Console;
#else
            IsConsole = false;
#endif           
        }

        public void Update()
        {
            if( !m_IsCorrectInput ) return;

            if( m_LastGotInputDataTime >= 0 )
            {
                InputHistory history = new InputHistory( m_LastGotInputDataTime, m_GamePadDeviceList, PointerDataList, KeyboardInputData );

                m_InputHistoryList.Insert( 0, history );

                if( m_InputHistoryList.Count > m_HistoryMaxSize )
                {
                    m_InputHistoryList.RemoveRange( m_HistoryMaxSize, m_InputHistoryList.Count - m_HistoryMaxSize );
                }

                foreach( GamePadDevice deviceData in m_GamePadDeviceList ) deviceData.RemoveInputData();

                m_PointerDataList.Clear();
                m_LastGotInputDataTime = -1;
            }

            m_DeviceIdListToInputDataAdd.Clear();
#if SWITCH_INPUT_ENABLE
            if( IsGetNSKeyboardState )
            {
                NSKeyboardManager.Instance.UpdateKeyboardData();
                KeyboardInputData = NSKeyboardManager.Instance.ConvertNSKeyboardDataToKeyboardInputData();
            }

            if( IsGetNSMouseState )
            {
                NSMouseManager.Instance.UpdateMouseData();

                if( NSMouseManager.Instance.NowData.IsConnected )
                {//Nintendo Switchでマウスの情報を通常のポインティングデバイスとして格納する
                    PointerData pointerData = PointerData.ConvertFromNSMouseData( NSMouseManager.Instance.NowData );

                    if( pointerData.IsEnable )
                    {
                        m_PointerDataList.Add( pointerData );

                        m_LastGotInputDataTime = Time.unscaledTime;
                    }
                }
            }
#else
            KeyboardInputData = new KeyboardInputData();

            if( IsConvertKeyboardToGamePadInput.IsFlag() ) CreateKeyboardInputData();
#endif
            foreach( InputDevice inputDevice in InputSystem.devices )
            {
                if( inputDevice is Gamepad ) AddGamePadData( inputDevice );
#if UNITY_EDITOR
                else if( inputDevice is Joystick ) AddGamePadDataForJoyStick( inputDevice );
#endif
                else if( inputDevice is Pointer ) AddPointerData( inputDevice );
                //else
                //{
                //    Debug.Log( "Unknown Device : " + inputDevice.name );
                //}
            }

            UpdateDeviceState();
        }

        /// <summary>
        /// コントローラからの入力データを追加
        /// </summary>
        /// <remarks>このメソッドで取得できるコントローラーの種類はPC(Editor含む)の場合はXInput対応 Nintendo Swichでは対応コントローラー </remarks>
        /// <param name="_device"></param>
        private void AddGamePadData( InputDevice _device )
        {
            int deviceId = GamePadInputData.CreateData( _device, m_StickDeadZone, out GamePadInputData inputData );

            if( deviceId > GamePadDefine.INVALID_DEVICE_ID && inputData.IsEnable )
            {
                GamePadDevice deviceData = m_GamePadDeviceList.Find( ( GamePadDevice data ) => data.DeviceId == deviceId );

                if( deviceData == null )
                {
                    //Debug.Log( "Create InputData : " + deviceId );
                    deviceData = new GamePadDevice( deviceId, _device );
                    m_GamePadDeviceList.Add( deviceData );
#if SWITCH_INPUT_ENABLE
                    Npad.BindStyleSetUpdateEvent( ( NpadId )deviceData.DeviceId );
#endif
                }

                deviceData.SetInputData( in inputData );

                if( m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
            }
            else
            {
                //Debug.Log( "deviceId : " + deviceId + " Device Name : " + _device.name + " Display Name :  " + _device.displayName );
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// コントローラ(DirectInput対応のみ)からの入力データを追加
        /// </summary>
        /// <remarks>Editorでの動作の場合にDirectInput対応のコントローラーの入力を取得する場合はこちら</remarks>
        /// <param name="_device"></param>
        private void AddGamePadDataForJoyStick( InputDevice _device )
        {
            int deviceId = GamePadInputData.CreateDataForJoyStick( _device, m_StickDeadZone, out GamePadInputData inputData );

            if( deviceId > GamePadDefine.INVALID_DEVICE_ID && inputData.IsEnable )
            {
                GamePadDevice deviceData = m_GamePadDeviceList.Find( ( GamePadDevice data ) => data.DeviceId == deviceId );

                if( deviceData == null )
                {
                    //Debug.Log( "Create InputData : " + deviceId );
                    deviceData = new GamePadDevice( deviceId, _device );
                    m_GamePadDeviceList.Add( deviceData );
                }

                deviceData.SetInputData( in inputData );

                if( m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
            }
        }
#endif //UNITY_EDITOR
        /// <summary>
        /// ポインティングデバイスでの入力の追加
        /// </summary>
        /// <remarks> UnityEditorの場合はマウスの入力を追加する</remarks>
        /// <param name="_device"></param>
        private void AddPointerData( InputDevice _device )
        {
            List<PointerData> pointerDataList = PointerData.CreateDataList( _device );

            if( pointerDataList == null || pointerDataList.Count == 0 ) return;

            m_PointerDataList.AddRange( pointerDataList );

            if( m_PointerDataList.Count > 0 && m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
        }

#if !SWITCH_INPUT_ENABLE
        /// <summary>
        /// キーボードの入力情報を作成
        /// </summary>
        private void CreateKeyboardInputData()
        {
            if( m_KeyboardControlAssignmentData != null )
            {
                if( UnityEngine.InputSystem.Keyboard.current != null )
                {
                    int deviceId = GamePadInputData.CreateDataFromKeyboard( m_KeyboardControlAssignmentData, out GamePadInputData inputData );

                    GamePadDevice deviceData = m_GamePadDeviceList.Find( ( GamePadDevice data ) => data.DeviceId == deviceId );

                    if( deviceData == null )
                    {
                        deviceData = new GamePadDevice( deviceId, UnityEngine.InputSystem.Keyboard.current );
                        m_GamePadDeviceList.Add( deviceData );
                    }

                    deviceData.SetInputData( in inputData );

                    if( m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
                }
                else
                {
                    Debug.Log( "Failed Get Keyboard Input Data" );
                }
            }
            else
            {//キーボードのキーアサインデータの読み込み待ち
                if( AddressableResourcesManager.IsEnableInstance && AddressableResourcesManager.Instance.IsLoadComplitedInfoList )
                {
                    if( AddressableResourcesManager.Instance.LoadAsset( InputSystemIdentifierDefine.KEYBOARD_CONTROL_ASSIGNMENT_DATA, out AddressableStorage storage ) == AddressableLoadResult.Complited )
                    {
                        m_KeyboardControlAssignmentData = storage.GetAsset<KeyboardControlAssignmentData>();
                    }
                }
            }
        }
#endif

        /// <summary>
        /// コントローラーの状態更新
        /// </summary>
        private void UpdateDeviceState()
        {
            int connectDeviceCount = 0;

            //入力データがあるコントローラーの状態を接続状態へ更新・ない場合は新規追加
            for( int i = 0; i < m_GamePadDeviceList.Count; i++ )
            {
                bool isConnect = m_GamePadDeviceList[ i ].InputData.IsEnable;

                m_GamePadDeviceList[ i ].UpdateState( isConnect );

#if SWITCH_INPUT_ENABLE
                if( Npad.IsStyleSetUpdated( ( NpadId )m_GamePadDeviceList[ i ].DeviceId ) )
                {
                    Debug.Log( "IsStyleSetUpdated DeviceID : " + m_GamePadDeviceList[i].DeviceId );
                }
#endif
                if( isConnect ) connectDeviceCount++;

                //コントローラーの状態更新フラグがfalseの場合、接続フラグだけ更新して以降の処理は実行しない
                if( !IsUpdateDeviceState ) continue;

                if( !m_GamePadDeviceList[ i ].IsConnected )
                {
                    if( IsAutoCheckDeviceConnect )
                    {
                        if( m_GamePadDeviceList[ i ].ControllerStyle == ControllerStyle.JoyLeft || m_GamePadDeviceList[ i ].ControllerStyle == ControllerStyle.JoyRight )
                        {//Joy-Con1本持ちの場合
                            if( IsLRSAssignmentMode )
                            {
                                Debug.Log( "Remove DeviceData Index : " + i + "DeviceId : " + m_GamePadDeviceList[ i ].DeviceId );
                                // Joy-Con の L/R ボタン押しによる操作スタイル割り当てモードが有効の場合、
                                // 1本持ちから2本持ちに変えた場合にどちらかの1本持ちのデバイスが未接続扱いになってしまうので削除する                             
                                DisconnectGamePad( m_GamePadDeviceList[ i ].DeviceId );
                                i--;
                                continue;
                            }
                        }

                        if( Time.unscaledTime - m_GamePadDeviceList[ i ].LastUpdateTime > m_WaitTimeToShowControllerSupport )
                        {//コントローラーの接続が切れてから一定時間経過したのでサポートアプレットを表示                        
                            DisconnectGamePad( m_GamePadDeviceList[ i ].DeviceId );
                            ShowControllerSupport();
                            return;
                        }
                    }
                    else
                    {
                        DisconnectGamePad( m_GamePadDeviceList[ i ].DeviceId );
                    }
                }
            }

            bool isUpdateDeviceCount = false;

            //コントローラー接続数を更新
            if( m_ConnectedDeviceCount != connectDeviceCount )
            {
                Debug.Log( "Update Device Count old : " + m_ConnectedDeviceCount + " New : " + connectDeviceCount );
                m_ConnectedDeviceCount = connectDeviceCount;
                m_LastUpdatedDeviceCountTime = Time.unscaledTime;
                isUpdateDeviceCount = true;
            }

            if( IsUpdateDeviceState ) CheckControllerMinimumNum();

            if( isUpdateDeviceCount )
            {
                m_GamePadDeviceList.Sort( ( a, b ) => a.DeviceId - b.DeviceId );

#if SWITCH_INPUT_ENABLE
                // Handheldのコントローラーがある場合は先頭に移動
                const int HANDHELD_ID = ( int )NPad.NpadId.Handheld;

                GamePadDevice handheldDevice = m_GamePadDeviceList.Find( ( device ) => device.DeviceId == HANDHELD_ID );

                if( handheldDevice != null )
                {
                    m_GamePadDeviceList.Remove( handheldDevice );
                    m_GamePadDeviceList.Insert( 0, handheldDevice );
                }
#endif
                if( IsUpdateDeviceState ) GamepadCountChangedProcess?.Invoke();
            }
        }

        /// <summary>
        /// コントローラーの最小接続数未満の場合に一定時間経過したらサポートアプレットを表示する
        /// </summary>
        private void CheckControllerMinimumNum()
        {
            if( !IsAutoCheckMinimumDeviceCount ) return;
            if( !m_IsCheckControllerMinimumNum ) return;

            //コントローラーが最小接続数より少なく、更新時間から一定時間経過している場合はコントローラーサポートアプレットを表示
            float progressTime = Time.unscaledTime - m_LastUpdatedDeviceCountTime;

            if( progressTime > m_WaitTimeToShowControllerSupport )
            {
                if( m_ConnectedDeviceCount < MinDeviceCount )
                {
                    ShowControllerSupport();
                }

                m_WaitTimeToShowControllerSupport = GamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;
            }
        }

        /// <summary>
        /// コントローラーのデバイスデータの取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns></returns>
        public GamePadDevice GetGamePadDevice( int _deviceId )
        {
            if( m_GamePadDeviceList.Count <= 0 )
            {
                //Debug.Log( "Nothing DeviceData DeviceId : " + _deviceId );
                return null;
            }

            return m_GamePadDeviceList.Find( ( GamePadDevice deviceData ) => deviceData.DeviceId == _deviceId );
        }

        /// <summary>
        /// 現在保持しているデバイス一覧のデータからデバイスID一覧を取得
        /// </summary>
        /// <returns></returns>
        public List<int> GetGamepadDeviceIdList()
        {
            List<int> idList = new List<int>();

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;
                if( !gamePadDevice.IsConnected ) continue;

                idList.Add( gamePadDevice.DeviceId );
            }

            idList.Sort();

            return idList;
        }

        /// <summary>
        /// 履歴の削除
        /// </summary>
        public void ClearHistory()
        {
            m_InputHistoryList.Clear();
        }

        /// <summary>
        /// 指定のキーを指定時間以上押し続けているか
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <param name="_keyId">キーID</param>
        /// <param name="_time">押し続けている時間</param>
        /// <remarks>
        /// 基本的に<see cref="GamePadInputData.IsHoldingKeyForKeepTime(GamepadKeyId, float)"/>から呼び出されていることを想定<br></br>
        /// それ以外からでも呼び出しは可能
        /// </remarks>
        /// <returns><see cref="HoldingResult"/></returns>
        public HoldingResult IsHoldingKeyForKeepTime( int _deviceId, GamepadKeyId _keyId, float _time )
        {
            GamePadDevice gamepad = GetGamePadDevice( _deviceId );

            if( gamepad == null ) return HoldingResult.InvalidDeviceId;
            if( !gamepad.InputData.IsHold( _keyId ) ) return HoldingResult.NotNowHold;

            int i = 0;

            for( i = 0; i < m_InputHistoryList.Count; i++ )
            {
                GamePadInputData inputData = m_InputHistoryList[ i ].InputDataList.Find( ( GamePadInputData p ) => p.DeviceId == _deviceId );

                if( !inputData.IsEnable ) break;
                if( !inputData.IsHold( _keyId ) ) break;

                if( Time.unscaledTime - m_InputHistoryList[ i ].GetDataTime >= _time ) return HoldingResult.Success;
            }

            return i >= m_HistoryMaxSize ? HoldingResult.HistoryEnd : HoldingResult.Failure;
        }

        /// <summary>
        /// 指定のデバイスIDの入力デバイスがコントローラーかを判定
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns>trueならコントローラー(持ち方は不明)</returns>
        public static bool IsControllerByDeviceId( int _deviceId )
        {
            //Debug.Log( "Device Count : " + InputSystem.devices.Count );

            foreach( InputDevice inputDevice in InputSystem.devices )
            {
                //Debug.Log( "Device : " + inputDevice.name );
#if SWITCH_INPUT_ENABLE
                if( !( inputDevice is NPad ) ) continue;

                NPad gamepad = inputDevice as NPad;

                if( gamepad.npadId == ( NPad.NpadId )_deviceId ) return true;
#else //SWITCH_INPUT_ENABLE
                if( !( inputDevice is Gamepad ) && !( inputDevice is Joystick ) ) continue;

                if( inputDevice.deviceId == _deviceId ) return true;
#endif //SWITCH_INPUT_ENABLE
            }

            return false;
        }

        /// <summary>
        /// ポインティングデバイスでの入力が有効かの判定
        /// </summary>        
        /// <returns>Nintendo Swicthの場合は携帯モードであればture<br></br>UnityEditorではポインタデバイス(マウスとか)が繋がっていればtrue</returns>
        public static bool IsEnablePointerDevice()
        {
#if SWITCH_INPUT_ENABLE
            return !Instance.IsConsole;
#else //SWITCH_INPUT_ENABLE
            foreach( InputDevice inputDevice in InputSystem.devices )
            {
                if( inputDevice is UnityEngine.InputSystem.Pointer ) return true;
            }

            return false;
#endif //SWITCH_INPUT_ENABLE
        }

        /// <summary>
        /// コントローラー接続状態を自動での監視を開始
        /// </summary>
        public void EnableAutoCheckDeviceConnect()
        {
            IsAutoCheckDeviceConnect = true;
        }

        /// <summary>
        /// コントローラー接続状態を自動での監視を停止
        /// </summary>
        public void DisenableAutoCheckDeviceConnect()
        {
            IsAutoCheckDeviceConnect = false;
        }

#if SWITCH_INPUT_ENABLE
        /// <summary>
        /// TVモード/携帯モード切り替え時に実行するイベント
        /// </summary>
        /// <param name="_mode">現在の動作モード</param>
        private static void ChangedOperationMode( Operation.OperationMode _mode )
        {
            Instance.IsConsole = _mode == Operation.OperationMode.Console;

            Debug.Log( "ChangedOperationMode( " + _mode.ToString() + " )" );

            Instance.m_WaitTimeToShowControllerSupport = GamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

            if( Instance.IsConsole ) Instance.m_WaitTimeToShowControllerSupport += GamePadDefine.ADD_WAIT_TIME_CONTROLLER_SUPPORT;
        }

        private static void ChangeFocusState( Notification.FocusState _state )
        {
        }
#endif //SWITCH_INPUT_ENABLE

        /// <summary>
        /// コントローラーサポートアプレットを表示
        /// </summary>
        public static void ShowControllerSupport()
        {
            Debug.Log( "Show ControllerSupport!" );

            //結果にかかわらずデバイスの状態と入力を一旦削除
            Instance.m_WaitTimeToShowControllerSupport = GamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

            if( Instance.ControllerSuportProcerss != null )
            {
                Instance.ControllerSuportProcerss?.Invoke();
                return;
            }

            Instance.IsCorrectInput = false;
            Instance.IsUpdateDeviceState = false;

#if SWITCH_INPUT_ENABLE
            bool isSingleMode = false;

            if( !Instance.IsConsole )
            {//携帯モードの場合、コントローラー最大接続数が1であり、Handheldが有効の場合は１人用特殊モードを有効にする
                NpadStyle npadStyle = Npad.GetSupportedStyleSet();

                if( Instance.MaxDeviceCount == 1 && ( npadStyle & NpadStyle.Handheld ) == NpadStyle.Handheld ) isSingleMode = true;
            }

            ControllerSupportArg controllerSupportArg = new ControllerSupportArg();
            controllerSupportArg.SetDefault();
            controllerSupportArg.enableSingleMode = isSingleMode;
            controllerSupportArg.playerCountMin = Instance.MinDeviceCount;
            controllerSupportArg.playerCountMax = Instance.MaxDeviceCount;
            controllerSupportArg.enableTakeOverConnection = Instance.IsEnableTakeOverConnection;

            ControllerSupportResultInfo resultInfo = new ControllerSupportResultInfo();

            nn.Result result = ControllerSupport.Show( ref resultInfo, controllerSupportArg, true );

            if( result.IsSuccess() )
            {
                result.abortUnlessSuccess();

                Debug.Log( "ControllerSupport Sucess! " + resultInfo.ToString() );
            }
            else
            {
                Debug.Log( result );
            }
#endif //SWITCH_INPUT_ENABLE

            Instance.IsCorrectInput = true;
            Instance.IsUpdateDeviceState = true;
            Instance.StartCoroutine( Instance.ResetDeviceWait() );
        }

        private IEnumerator ResetDeviceWait()
        {
            yield return null;

            InitializeDeviceList();
            ResetedDeviceProcess?.Invoke();
        }

        /// <summary>
        /// 初回、もしくはコントローラーサポートアプレットを閉じた時に実行するデバイス一覧更新
        /// </summary>
        public void InitializeDeviceList()
        {
            m_GamePadDeviceList.Clear();
            //GamePadIdList.Clear();
            m_PointerDataList.Clear();
            m_ConnectedDeviceCount = 0;
            m_LastGotInputDataTime = -1;

            m_DeviceIdListToInputDataAdd.Clear();

            m_IsCheckControllerMinimumNum = true;
        }

        /// <summary>
        /// 現在のコントローラーの持ち方・数の設定を取得
        /// </summary>
        /// <returns></returns>
        public GamePadSetting GetInputSetting()
        {
            GamePadSetting inputSetting = new GamePadSetting();
#if SWITCH_INPUT_ENABLE
            NpadStyle npadStyle = Npad.GetSupportedStyleSet();
            NpadJoyHoldType holdtype = NpadJoy.GetHoldType();

            inputSetting.IsFullKey = ( ( npadStyle & NpadStyle.FullKey ) == NpadStyle.FullKey );
            inputSetting.IsJoyDuel = ( ( npadStyle & NpadStyle.JoyDual ) == NpadStyle.JoyDual );
            inputSetting.IsJoyLeft = ( ( npadStyle & NpadStyle.JoyLeft ) == NpadStyle.JoyLeft );
            inputSetting.IsJoyRight = ( ( npadStyle & NpadStyle.JoyRight ) == NpadStyle.JoyRight );
            inputSetting.IsHandheld = ( ( npadStyle & NpadStyle.Handheld ) == NpadStyle.Handheld );
            inputSetting.IsHoldHorizontalType = holdtype == NpadJoyHoldType.Horizontal;
#else
            inputSetting.IsFullKey = true;
            inputSetting.IsJoyDuel = true;
            inputSetting.IsJoyLeft = false;
            inputSetting.IsJoyRight = false;
            inputSetting.IsHandheld = true;
            inputSetting.IsHoldHorizontalType = false;
#endif
            inputSetting.MinGamePadCount = MinDeviceCount;
            inputSetting.MaxGamePadCount = MaxDeviceCount;
            inputSetting.IsAutoCheckDeviceConnect = IsAutoCheckDeviceConnect;
            inputSetting.IsAutoCheckMinimumDeviceCount = IsAutoCheckMinimumDeviceCount;

            Debug.Log( "GetInputSetting  DeviceStyle " +
                "IsFullkey : " + inputSetting.IsFullKey + "\n" +
                "IsJoyDuel : " + inputSetting.IsJoyDuel + "\n" +
                "IsJoyLeft : " + inputSetting.IsJoyLeft + "\n" +
                "IsJoyRight : " + inputSetting.IsJoyRight + "\n" +
                "IsHandHeld : " + inputSetting.IsHandheld + "\n" +
                "MinCount : " + inputSetting.MinGamePadCount + "\n" +
                "MaxCount : " + inputSetting.MaxGamePadCount + "\n" +
                "IsHoldHorizontal : " + inputSetting.IsHoldHorizontalType + "\n" +
                "IsAutoCheckDeviceConnect : " + inputSetting.IsAutoCheckDeviceConnect + "\n" +
                "IsAutoCheckMinimumDeviceCount : " + inputSetting.IsAutoCheckMinimumDeviceCount + "\n" +
                "IsEnableTakeOverConnection : " + inputSetting.IsEnableTakeOverConnection
                );

            return inputSetting;
        }

        /// <summary>
        /// コントローラーの持ち方・数を設定する
        /// </summary>
        /// <param name="_inputSetting">設定情報</param>
        public void SetInputSetting( GamePadSetting _inputSetting )
        {
            Debug.Log( "SetInputSetting  DeviceStyle" +
                "IsFullkey : " + _inputSetting.IsFullKey + "\n" +
                "IsJoyDuel : " + _inputSetting.IsJoyDuel + "\n" +
                "IsJoyLeft : " + _inputSetting.IsJoyLeft + "\n" +
                "IsJoyRight : " + _inputSetting.IsJoyRight + "\n" +
                "IsHandHeld : " + _inputSetting.IsHandheld + "\n" +
                "MinCount : " + _inputSetting.MinGamePadCount + "\n" +
                "MaxCount : " + _inputSetting.MaxGamePadCount + "\n" +
                "IsHoldHorizontal : " + _inputSetting.IsHoldHorizontalType + "\n" +
                "IsAutoCheckDeviceConnect : " + _inputSetting.IsAutoCheckDeviceConnect + "\n" +
                "IsAutoCheckMinimumDeviceCount : " + _inputSetting.IsAutoCheckMinimumDeviceCount + "\n" +
                "IsEnableTakeOverConnection : " + _inputSetting.IsEnableTakeOverConnection
                );

            if( IsAutoCheckMinimumDeviceCount != _inputSetting.IsAutoCheckMinimumDeviceCount ) m_LastUpdatedDeviceCountTime = Time.unscaledTime;

            MinDeviceCount = _inputSetting.MinGamePadCount;
            MaxDeviceCount = ( byte )Mathf.Max( MinDeviceCount, _inputSetting.MaxGamePadCount );
            IsAutoCheckDeviceConnect = _inputSetting.IsAutoCheckDeviceConnect;
            IsAutoCheckMinimumDeviceCount = _inputSetting.IsAutoCheckMinimumDeviceCount;
            IsEnableTakeOverConnection = _inputSetting.IsEnableTakeOverConnection;

#if SWITCH_INPUT_ENABLE
            NpadStyle npadStyle = NpadStyle.None;
            if( _inputSetting.IsFullKey ) npadStyle |= NpadStyle.FullKey;
            if( _inputSetting.IsJoyDuel ) npadStyle |= NpadStyle.JoyDual;
            if( _inputSetting.IsJoyLeft ) npadStyle |= NpadStyle.JoyLeft;
            if( _inputSetting.IsJoyRight ) npadStyle |= NpadStyle.JoyRight;
            if( _inputSetting.IsHandheld ) npadStyle |= NpadStyle.Handheld;

            m_EnableNpadIdList.Clear();

            List<int> connectedGamepadIdList = GetGamepadDeviceIdList();

            foreach( int deviceId in connectedGamepadIdList )
            {
                m_EnableNpadIdList.Add( ( NpadId )deviceId );
            }

            //  Handheldを除いた現在接続中のコントローラーの数
            int enableCountNotHandheld = connectedGamepadIdList.FindAll( ( id ) => id != ( int )NpadId.Handheld ).Count;

            //  Handheldを除いた現在接続中のコントローラーの数がコントローラー最大接続数未満の場合、追加で接続可能なコントローラーを追加する
            if( enableCountNotHandheld < MaxDeviceCount )
            {
                for( int i = 0; i < ApplicationDefine.PLAYER_MAX_COUNT; i++ )
                {
                    NpadId npadId = ( NpadId )i;

                    if( m_EnableNpadIdList.FindIndex( ( id ) => id == npadId ) < 0 ) m_EnableNpadIdList.Add( npadId );
                }
            }

            if( _inputSetting.IsHandheld )
            {// Handheldが使用可能な設定の場合、使用可能なIDに無ければ追加する
                if( m_EnableNpadIdList.FindIndex( ( id ) => id == NpadId.Handheld ) < 0 ) m_EnableNpadIdList.Add( NpadId.Handheld );
            }


            Npad.SetSupportedIdType( m_EnableNpadIdList.ToArray() );
            Npad.SetSupportedStyleSet( npadStyle );
            NpadJoy.SetHoldType( _inputSetting.IsHoldHorizontalType ? NpadJoyHoldType.Horizontal : NpadJoyHoldType.Vertical );
#endif

            m_LastUpdatedDeviceCountTime = Time.unscaledTime;
        }

        /// <summary>
        /// 履歴サイズの更新
        /// </summary>
        /// <remarks>最小サイズは1</remarks>
        /// <param name="_size"></param>
        public void UpdateHistorySize( int _size ) => m_HistoryMaxSize = Mathf.Max( 1, _size );

        /// <summary>
        /// 指定のNpadIdが有効かを判定
        /// </summary>
        /// <remarks>現状Nintendo Switch以外の開発環境では常にture</remarks>
        /// <param name="_deviceId"></param>
        /// <returns>有効なDeviceIDの場合はture </returns>
        public bool IsEnableNpadId( int _deviceId )
        {
            if( _deviceId < 0 ) return false;
#if SWITCH_INPUT_ENABLE
            return m_EnableNpadIdList.FindIndex( ( NpadId id ) => id == ( NpadId )_deviceId ) >= 0;
#else
            return true;
#endif
        }

        /// <summary>
        /// ポインティングデバイスの入力データにある先頭のデータを取得
        /// </summary>
        /// <returns></returns>
        public PointerData GetPointerDataToTop()
        {
            if( m_PointerDataList == null || m_PointerDataList.Count == 0 ) return default;

            return m_PointerDataList[ 0 ];
        }

        /// <summary>
        /// 指定のデバイスIDの現在からの入力履歴を取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns>入力履歴 先頭が最新の入力データ</returns>
        public List<GamePadHistory> GetGamePadHistory( int _deviceId )
        {
            List<GamePadHistory> historyList = new List<GamePadHistory>();

            GamePadDevice inputDevice = GetGamePadDevice( _deviceId );

            if( inputDevice == null ) return null;

            GamePadHistory inputHistory = default;

            if( inputDevice.InputData.IsEnable )
            {//現在の入力データがある場合は先頭に追加
                inputHistory = new GamePadHistory( m_LastGotInputDataTime, inputDevice.InputData );
                historyList.Add( inputHistory );
            }

            foreach( InputHistory history in m_InputHistoryList )
            {
                GamePadInputData inputData = history.InputDataList.Find( ( GamePadInputData input ) => input.DeviceId == _deviceId );

                if( !inputData.IsEnable ) continue;

                inputHistory = new GamePadHistory( history.GetDataTime, inputData );
                historyList.Add( inputHistory );
            }

            return historyList;
        }

        /// <summary>
        /// 指定のデバイスの１つ前の入力データを取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <param name="_outValue">取得した１つ前の入力データ</param>
        /// <returns></returns>
        public bool GetGamepadInputDataPrevious( int _deviceId, out GamePadInputData _outValue )
        {
            _outValue = default;

            if( m_InputHistoryList.Count <= 0 ) return false;

            InputHistory history = m_InputHistoryList[ 0 ];

            int index = history.InputDataList.FindIndex( ( GamePadInputData input ) => input.DeviceId == _deviceId );

            if( index >= 0 ) _outValue = history.InputDataList[ index ];

            return index >= 0;
        }

        /// <summary>
        /// 指定のPointerIDからの入力を現在からの入力履歴を取得
        /// </summary>
        /// <param name="_touchId">TouchID</param>
        /// <returns>入力履歴 先頭が最新の入力データ</returns>
        public List<PointerHistory> GetPointerHistory( int _touchId )
        {
            List<PointerHistory> historyList = new List<PointerHistory>();
            PointerData touchData = m_PointerDataList.Find( ( PointerData td ) => td.PointerID == _touchId );

            if( touchData.IsEnable )
            {//現在の入力データがある場合は先頭に追加
                historyList.Add( new PointerHistory( m_LastGotInputDataTime, touchData ) );
            }

            foreach( InputHistory history in m_InputHistoryList )
            {
                touchData = history.PointerDataList.Find( ( PointerData td ) => td.PointerID == _touchId );

                if( touchData.IsEnable )
                {
                    historyList.Add( new PointerHistory( history.GetDataTime, touchData ) );
                }
            }

            return historyList;
        }

        /// <summary>
        /// コントローラーのデバイスIDを取得
        /// </summary>
        /// <param name="_device">InputDevice</param>
        /// <param name="_outDeviceId"> Nintendo Switchの場合h<see cref="NPad.npadId"/>をint型にキャストした値 その他では<see cref="InputDevice.deviceId"/>の値</param>
        /// <returns>取得結果 <see cref="GetGamePadDeviceIDResult.Success"/>以外は失敗</returns>
        public static GetGamePadDeviceIDResult GetGamePadDeviceID( InputDevice _device, out int _outDeviceId )
        {
            _outDeviceId = GamePadDefine.INVALID_DEVICE_ID;
#if SWITCH_INPUT_ENABLE
            //コントローラー以外のデバイスの場合は無効のIDを返す
            if( !( _device is NPad gamepad ) ) return GetGamePadDeviceIDResult.Failed_NotGamePad;
            // DebugPadは無効なIDとして返す
            if( gamepad.npadId == NPad.NpadId.Debug ) return GetGamePadDeviceIDResult.Failed_DebugPad;

            //Joyコンの持ち方が2本持ちの場合、片方だけでも接続が切れたら入力を取得しない
            if( gamepad.styleMask == NPad.NpadStyles.JoyDual )
            {
                if( !gamepad.isLeftConnected && gamepad.isRightConnected ) return GetGamePadDeviceIDResult.Failed_JoyDuel_Missing_Left;
                if( gamepad.isLeftConnected && !gamepad.isRightConnected ) return GetGamePadDeviceIDResult.Failed_JoyDuel_Missing_Right;
                if( !gamepad.isLeftConnected && !gamepad.isRightConnected ) return GetGamePadDeviceIDResult.Failed_JoyDuel_Missing_Both;
            }

            _outDeviceId = ( int )gamepad.npadId;
#else //SWITCH_INPUT_ENABLE
            if( _device is Gamepad ) _outDeviceId = _device.deviceId;
            else if( _device is Joystick ) _outDeviceId = _device.deviceId;
#endif //SWITCH_INPUT_ENABLE

            return GetGamePadDeviceIDResult.Success;
        }

        /// <summary>
        /// 指定のデバイスIDのコントローラーを切断する
        /// </summary>
        /// <remarks>Nintendo Switch以外の環境では何もしない</remarks>
        /// <param name="_deviceId"></param>
        public void DisconnectGamePad( int _deviceId )
        {
            GamePadDevice gamePadDevice = GetGamePadDevice( _deviceId );

            if( gamePadDevice != null ) m_GamePadDeviceList.Remove( gamePadDevice );

            if( _deviceId < 0 ) return;

#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_deviceId;

            Npad.DestroyStyleSetUpdateEvent( npadId );
            Npad.Disconnect( npadId );
#endif
        }

        /// <summary>
        /// 未接続状態のコントローラーデータが存在している場合は削除する
        /// </summary>
        public void CheckRemoveNotConnectedGamePad()
        {
            for( int i = 0; i < m_GamePadDeviceList.Count; i++ )
            {
                if( m_GamePadDeviceList[ i ] == null )
                {
                    m_GamePadDeviceList.RemoveAt( i );
                    i--;

                    return;
                }

                if( m_GamePadDeviceList[ i ].IsConnected ) continue;

#if SWITCH_INPUT_ENABLE
                if( Enum.IsDefined( typeof( NpadId ), m_GamePadDeviceList[ i ].DeviceId ) )
                {
                    NpadId npadId = ( NpadId )m_GamePadDeviceList[ i ].DeviceId;
                    Npad.Disconnect( npadId );
                }
#endif
                m_GamePadDeviceList.RemoveAt( i );
                i--;
            }
        }

#if SWITCH_INPUT_ENABLE
        /// <summary>
        /// コントローラーの持ち方を取得
        /// </summary>
        /// <remarks>UnityEditor上ではコントローラーが繋がっていれば常に<see cref="ControllerStyle.FullKey"/>を返す</remarks>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns></returns>
        public static ControllerStyle GetControllerStyle( int _deviceId )
        {
            if( _deviceId < 0 ) return ControllerStyle.None;

            NpadId npadId = ( NpadId )_deviceId;
            NpadStyle style = Npad.GetStyleSet( npadId );

            return ( ControllerStyle )style;
        }
#endif

        /// <summary>
        /// コントローラーの持ち方を取得
        /// </summary>
        /// <remarks>UnityEditor上ではコントローラーが繋がっていれば常に<see cref="ControllerStyle.FullKey"/>を返す</remarks>
        /// <param name="_device">デバイス</param>
        /// <returns></returns>
        public static ControllerStyle GetControllerStyle( GamePadDevice _device )
        {
            if( _device.DeviceId < 0 ) return ControllerStyle.None;

#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_device.DeviceId;
            NpadStyle style = Npad.GetStyleSet( npadId );

            return ( ControllerStyle )style;
#else
            return _device.IsKeyboardControl ? ControllerStyle.Keyboard : ControllerStyle.FullKey;
#endif
        }

        /// <summary>
        /// コントローラーのプレイヤーランプの点灯パターンを取得
        /// </summary>
        /// <remarks>下位4bitを使用して表現 最下位bitが左端(縦持ちの場合は上端)の点灯状況</remarks>
        /// <returns>プレイヤーランプの点灯状況 Nintendo Switch以外の動作環境では常に0</returns>
        public static byte GetPlayerLedPattern( int _deviceId )
        {
            if( _deviceId < 0 ) return 0;

#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_deviceId;

            return Npad.GetPlayerLedPattern( ( NpadId )_deviceId );
#else
            return 0;
#endif
        }

        /// <summary>
        /// コントローラーの色を取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns>左メイン・左サブ・右メイン・右サブの順で色を格納 Fullkeyでは左に取得した色 右は<see cref="Color.clear"/>を格納 NintendoSwitcj以外の環境では左右ともにメインは<see cref="Color.black"/> サブは<see cref="Color.gray"/> を返す</returns>
        public static Color[] GetControllerColor( int _deviceId )
        {
            Color[] colorList = new Color[ GamePadDefine.ControllerColorIndex.DATA_SIZE ];
            colorList[ GamePadDefine.ControllerColorIndex.INDEX_LM ] = Color.gray;
            colorList[ GamePadDefine.ControllerColorIndex.INDEX_LS ] = Color.black;
            colorList[ GamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.gray;
            colorList[ GamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.black;

#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_deviceId;
            NpadStyle style = Npad.GetStyleSet( npadId );

            NpadControllerColor leftColor = new NpadControllerColor();
            NpadControllerColor rightColor = new NpadControllerColor();
            nn.Result result;

            if( style == NpadStyle.FullKey )
            {//FullKeyの場合
                result = Npad.GetControllerColor( ref leftColor, npadId );

                if( result.IsSuccess() )
                {//左側に取得した色を格納 右側は透明を設定
                    //Debug.Log( "Fullkey Color : " + leftColor.ToString() );
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_LM ] = leftColor.main.ConvertColor();
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_LS ] = leftColor.sub.ConvertColor();
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.clear;
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.clear;
                }
                else
                {
                    Debug.Log( "Failed GetControllerColor : " + result.ToString() );
                }
            }
            else
            {//それ以外(JoyCon)
                result = Npad.GetControllerColor( ref leftColor, ref rightColor, npadId );

                if( result.IsSuccess() )
                {
                    //Debug.Log( "Left Color : " + leftColor.ToString() + " Right Color : " + rightColor.ToString() );
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_LM ] = leftColor.main.ConvertColor();
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_LS ] = leftColor.sub.ConvertColor();
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_RM ] = rightColor.main.ConvertColor();
                    colorList[ GamePadDefine.ControllerColorIndex.INDEX_RS ] = rightColor.sub.ConvertColor();

                    //Joyコン1本持ちの場合、接続されていない側の色を全て透明に設定
                    if( style == NpadStyle.JoyLeft )
                    {
                        colorList[ GamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.clear;
                        colorList[ GamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.clear;
                    }
                    else if( style == NpadStyle.JoyRight )
                    {
                        colorList[ GamePadDefine.ControllerColorIndex.INDEX_LM ] = Color.clear;
                        colorList[ GamePadDefine.ControllerColorIndex.INDEX_LS ] = Color.clear;
                    }
                }
                else
                {
                    Debug.Log( "Failed GetControllerColor2 : " + result.ToString() );
                }
            }
#endif
            return colorList;
        }

        /// <summary>
        /// Joy-Con の L/R ボタン押しによる操作スタイル割り当てモードを開始
        /// </summary>
        public static void StartLrAssignmentMode()
        {
#if SWITCH_INPUT_ENABLE
            NpadJoy.StartLrAssignmentMode();
#endif
            Instance.IsLRSAssignmentMode = true;
        }

        /// <summary>
        /// Joy-Con の L/R ボタン押しによる操作スタイル割り当てモードを終了
        /// </summary>
        public static void StopLrAssignmentMode()
        {
#if SWITCH_INPUT_ENABLE
            NpadJoy.StopLrAssignmentMode();
#endif
            Instance.IsLRSAssignmentMode = false;
        }

        /// <summary>
        /// いづれかのコントローラーでの入力判定で、一定時間入力を無かったことにする判定の開始
        /// </summary>
        public void StartAnyGamePadKeyCheckDelay()
        {
            m_AnyGamePadKeyCheckDelayStartTime = Time.unscaledTime;
        }

        /// <summary>
        /// いづれかのコントローラーでの入力判定で、入力判定が無効かどうか
        /// </summary>
        /// <returns></returns>
        private bool IsAnyGamePadKeyCheckDelay()
        {
            if( m_AnyGamePadKeyCheckDelayStartTime < 0 ) return false;

            if( Time.unscaledTime - m_AnyGamePadKeyCheckDelayStartTime > ANY_KEY_CHECK_DELAY )
            {
                m_AnyGamePadKeyCheckDelayStartTime = -1;
                return false;
            }

            return true;
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_outDeviceIds">指定のキーが押下されたコントローラーのデバイスID</param>
        /// <returns></returns>
        public bool IsDownAnyGamePad( GamepadKeyId _keyId, out int[] _outDeviceIds )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsDown( _keyId ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押されたか
        /// </summary>
        /// <param name="_leyId"></param>
        /// <returns></returns>
        public bool IsDownAnyGamePad( GamepadKeyId _keyId )
        {
            return IsDownAnyGamePad( _keyId, out _ );
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押され続けているか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_outDeviceIds">指定のキーが押され続けているされたコントローラーのデバイスID</param>
        /// <returns></returns>
        public bool IsHoldAnyGamePad( GamepadKeyId _keyId, out int[] _outDeviceIds )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsHold( _keyId ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押され続けているか
        /// </summary>
        /// <param name="_keyId"></param>        
        /// <returns></returns>
        public bool IsHoldAnyGamePad( GamepadKeyId _keyId )
        {
            return IsHoldAnyGamePad( _keyId, out _ );
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが離されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_deviceIdList">指定のキーが離されたコントローラーのデバイスID一覧</param>
        /// <returns></returns>
        public bool IsUpAnyGamePad( GamepadKeyId _keyId, out int[] _deviceIdList )
        {
            _deviceIdList = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsUp( _keyId ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _deviceIdList = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが離されたか
        /// </summary>
        /// <param name="_keyId"></param>        
        /// <returns></returns>
        public bool IsUpAnyGamePad( GamepadKeyId _keyId )
        {
            return IsUpAnyGamePad( _keyId, out _ );
        }

        /// <summary>
        /// いづれかのコントローラーでリピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_outDeviceIds"></param>
        /// <returns></returns>
        public bool IsRepeatAnyGamePad( GamepadKeyId _keyId, out int[] _outDeviceIds )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsRepeat( _keyId ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーでリピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsRepeatAnyGamePad( GamepadKeyId _keyId )
        {
            return IsRepeatAnyGamePad( _keyId, out _ );
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押されたか or リピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsDownOrRepeatAnyGamePad( GamepadKeyId _keyId, out int[] _outDeviceIds )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsDownOrRepeat( _keyId ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで指定のキーが押されたか or リピート入力されたか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsDownOrRepeatAnyGamePad( GamepadKeyId _keyId )
        {
            return IsDownOrRepeatAnyGamePad( _keyId, out _ );
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>
        /// <param name="_outDeviceIds">対象の方向キーが押されたデバイスID一覧 無い場合はnull</param>
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadDownAnyGamePad( DPad _dpadKeyId, out int[] _outDeviceIds, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsDpadDown( _dpadKeyId, _confirm ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>        
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadDownAnyGamePad( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            return IsDpadDownAnyGamePad( _dpadKeyId, out _, _confirm );
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーを押し続けているか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>
        /// <param name="_outDeviceIds">対象の方向キーが押されたデバイスID一覧 無い場合はnull</param>
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadHoldAnyGamePad( DPad _dpadKeyId, out int[] _outDeviceIds, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsDpadHold( _dpadKeyId, _confirm ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーを押し続けているか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>        
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadHoldAnyGamePad( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            return IsDpadHoldAnyGamePad( _dpadKeyId, out _, _confirm );
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーを離したか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>
        /// <param name="_outDeviceIds">対象の方向キーが押されたデバイスID一覧 無い場合はnull</param>
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadUpAnyGamePad( DPad _dpadKeyId, out int[] _outDeviceIds, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsDpadUp( _dpadKeyId, _confirm ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }


        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーを離したか
        /// </summary>
        /// <param name="_dpadKeyId">対象の方向キー</param>        
        /// <param name="_confirm">入力の対象</param>
        /// <returns></returns>
        public bool IsDpadUpAnyGamePad( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            return IsDpadUpAnyGamePad( _dpadKeyId, out _, _confirm );
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーがリピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId"></param>
        /// <param name="_outDeviceIds"></param>
        /// <param name="_confirm"></param>
        /// <returns></returns>
        public bool IsDpadRepeatAnyGamePad( DPad _dpadKeyId, out int[] _outDeviceIds, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsDpadRepeat( _dpadKeyId, _confirm ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );

                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーがリピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId"></param>
        /// <param name="_confirm"></param>
        /// <returns></returns>
        public bool IsDpadRepeatAnyGamePad( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            return IsDpadRepeatAnyGamePad( _dpadKeyId, out _, _confirm );
        }

        /// <summary>
        ///  いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか or リピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId"></param>
        /// <param name="_outDeviceIds"></param>
        /// <param name="_confirm"></param>
        /// <returns></returns>
        public bool IsDpadDownOrRepeatAnyGamePad( DPad _dpadKeyId, out int[] _outDeviceIds, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsDpadDownOrRepeat( _dpadKeyId, _confirm ) )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );

                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        ///  いづれかのコントローラーで十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換 のうち、指定したキーが押下されたか or リピート入力されたか
        /// </summary>
        /// <param name="_dpadKeyId"></param>
        /// <param name="_confirm"></param>
        /// <returns></returns>
        public bool IsDpadDownOrRepeatAnyGamePad( DPad _dpadKeyId, SelectConfirmInput _confirm = SelectConfirmInput.Dpad | SelectConfirmInput.LeftStick_Digital )
        {
            return IsDpadDownOrRepeatAnyGamePad( _dpadKeyId, out _, _confirm );
        }

        /// <summary>
        /// いづれかのコントローラーでいづれかのキーが押下されたか
        /// </summary>
        /// <param name="_outDeviceIds">いづれかのキーが押されたコントローラーのデバイスID</param>
        /// <returns>いづれかのキーが押された場合はtrue</returns>
        public bool IsAnyKeyDownAnyGamePad( out int[] _outDeviceIds )
        {
            _outDeviceIds = null;

            if( IsAnyGamePadKeyCheckDelay() ) return false;

            List<int> keyInputDeviceList = new List<int>();
            bool result = false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsAnyKeyDown )
                {
                    keyInputDeviceList.Add( gamePadDevice.DeviceId );
                    result = true;
                }
            }

            if( keyInputDeviceList.Count > 0 ) _outDeviceIds = keyInputDeviceList.ToArray();

            return result;
        }

        /// <summary>
        /// いづれかのコントローラーでいづれかのキーが押下されたか
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKeyDownAnyGamePad()
        {
            return IsAnyKeyDownAnyGamePad( out _ );
        }

        /// <summary>
        /// 全てのコントローラーの左スティック入力のリピート時間を設定
        /// </summary>
        /// <param name="_time"></param>
        public void SetLeftAxisToDPadRepeatTimeAllGamePad( float _time )
        {
            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                gamePadDevice.LeftAxisToDPad.RepeatTime = _time;
            }
        }

        /// <summary>
        /// 全てのコントローラーの右スティック入力のリピート時間を設定
        /// </summary>
        /// <param name="_time"></param>
        public void SetRightAxisToDPadRepeatTimeAllGamePad( float _time )
        {
            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                gamePadDevice.RightAxisToDPad.RepeatTime = _time;
            }
        }

        /// <summary>
        /// 全てのコントローラーでのリピート入力の時間を設定
        /// </summary>
        /// <param name="_keyId"></param>
        /// <param name="_interval"></param>
        public void EnableRepaetAllGamePad( GamepadKeyId _keyId, float _interval )
        {
            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                gamePadDevice.EnableRepaet( _keyId, _interval );
            }
        }

        /// <summary>
        /// 全てのコントローラーでのリピート入力を停止
        /// </summary>
        /// <param name="_keyId"></param>
        public void StopRepeatAllGamePad( GamepadKeyId _keyId )
        {
            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                gamePadDevice.StopRepeat( _keyId );
            }
        }

        /// <summary>
        /// 指定のデバイスIDをもつコントローラーを振動させる
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <param name="_lowFrequency">低周波モーターの速度</param>
        /// <param name="_highFrequency">高周波モーターの速度</param>
        public void PlayVibration( int _deviceId, float _lowFrequency, float _highFrequency )
        {
            GamePadDevice gamePadDevice = m_GamePadDeviceList.Find( ( device ) => device.DeviceId == _deviceId );

            if( gamePadDevice != null ) gamePadDevice.PlayVibration( _lowFrequency, _highFrequency );
        }

        /// <summary>
        /// 指定のデバイスIDをもつコントローラーの振動を停止
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        public void StopVibration( int _deviceId )
        {
            GamePadDevice gamePadDevice = m_GamePadDeviceList.Find( ( device ) => device.DeviceId == _deviceId );

            if( gamePadDevice != null ) gamePadDevice.PlayVibration( 0, 0 );
        }

        /// <summary>
        /// 全てのコントローラーの振動を停止
        /// </summary>
        public void StopVibrationToAllDevice()
        {
            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                gamePadDevice.PlayVibration( 0, 0 );
            }
        }

        /// <summary>
        /// キーボードからの入力を取得しているか
        /// </summary>
        /// <returns></returns>
        public bool IsKeyboardControl()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            if( keyboard == null ) return false;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.DeviceId == keyboard.deviceId ) return true;
            }

            return false;
        }

        /// <summary>
        /// 接続されているコントローラーの数を取得
        /// </summary>
        /// <remarks>PC版ではキーボードをコントローラー扱いとしているが、キーボードは除外</remarks>
        /// <returns></returns>
        public int GetDeviceCountGamepadOnly()
        {
            int result = 0;

            foreach( GamePadDevice gamePadDevice in m_GamePadDeviceList )
            {
                if( gamePadDevice.IsKeyboardControl ) continue;

                result++;
            }

            return result;
        }
    }
}
