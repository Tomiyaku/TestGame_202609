#if UNITY_SWITCH && !UNITY_EDITOR
#define SWITCH_INPUT_ENABLE
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Time = UnityEngine.Time;

#if SWITCH_INPUT_ENABLE
using UnityEngine.Switch;
using UnityEngine.InputSystem.Switch;
using nn.hid;
#endif

using CodeIcf.Extensions;
using CodeIcf.Input.GamePad;
using CodeIcf.Input.Mouse;
using CodeIcf.Input.Touch;

namespace CodeIcf.Input
{
    /// <summary>
    /// InputSystemを利用した複数コントローラー対応Nintendo Switch向け入力管理クラス
    /// </summary>
    [DefaultExecutionOrder( -90 )]
    public class NSInputManager : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static NSInputManager m_Instance = null;
        /// <summary>インスタンス</summary>
        public static NSInputManager Instance
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
        private List<NSGamePadDevice> m_GamePadDeviceList = new List<NSGamePadDevice>();
        /// <summary>有効なゲームパッドID一覧</summary>
        public List<int> GamePadIdList { get; private set; } = new List<int>();

        /// <summary>全てのタッチ入力データ</summary>
        private List<NSTouchData> m_TouchDataList = new List<NSTouchData>();
        /// <summary>全てのタッチ入力データ</summary>
        public List<NSTouchData> TouchDataList => m_TouchDataList;

#pragma warning disable CS0649
        /// <summary>マウス入力情報</summary>
        /// <remarks>Nintendo Switchでの動作中のみ有効 UnityEditor上ではマウスの入力はタッチに割り当て</remarks>
        private NSMouseState m_MouseState;
#pragma warning restore CS0649
        /// <summary>マウス入力情報</summary>
        public NSMouseState MouseState => m_MouseState;
        /// <summary>マウスの入力情報を取得するか</summary>
        public bool IsGetMouseState { get; set; } = false;

        /// <summary>接続されているデバイスの数</summary>
        [SerializeField]
        private int m_ConnectedDeviceCount = -1;
        /// <summary>最後に入力データが更新された時間</summary>
        private float m_LastGotInputDataTime = -1;

        /// <summary>入力データの履歴</summary>
        private List<NSHistory> m_InputHistoryList = new List<NSHistory>();
        /// <summary>入力データの履歴の最大サイズ</summary>
        [SerializeField]
        private int m_HistoryMaxSize = 256;
        /// <summary>１フレーム中に入力データが取得できたデバイスID一覧</summary>
        private List<int> m_DeviceIdListToInputDataAdd = new List<int>();
        /// <summary>コントローラーサポートアプレットを閉じた後に呼び出す処理</summary>
        public System.Action ResetedDeviceProcess { get; set; }

        /// <summary>コントローラーサポートアプレットを呼び出すまでの待ち時間(秒)</summary>
        private float m_WaitTimeToShowControllerSupport = NSGamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

#if SWITCH_INPUT_ENABLE
        ///有効なコントローラーのNpadId一覧
        private List<NpadId> m_EnableNpadIdList = new List<NpadId>();
#endif
        /// <summary>入力データの取得を更新するかのフラグ</summary>
        private bool m_IsCorrectInput = true;

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

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        public static void CreateInstance()
        {
            if( m_Instance == null )
            {
                m_Instance = FindObjectOfType<NSInputManager>();

                if( m_Instance == null )
                {
                    GameObject obj = new GameObject( "NSInputManager" );
                    m_Instance = obj.AddComponent<NSInputManager>();
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
        }

        private void Start()
        {
#if SWITCH_INPUT_ENABLE
            NotificationFromSwitch.Instance.AddOperationModeChangedNotification( ChangedOperationMode );
            NotificationFromSwitch.Instance.AddFocusStateChangedNotification( ChangeFocusState );

            IsConsole = Operation.mode == Operation.OperationMode.Console;

            MaxDeviceCount = 1;
            m_EnableNpadIdList.Add( NpadId.No1 );
            m_EnableNpadIdList.Add( NpadId.Handheld );
#else
            IsConsole = false;
#endif
        }

        public void Update()
        {
            if( !m_IsCorrectInput ) return;

            if( m_LastGotInputDataTime >= 0 )
            {
                NSHistory history = new NSHistory( m_LastGotInputDataTime, m_GamePadDeviceList, TouchDataList, m_MouseState );

                m_InputHistoryList.Insert( 0, history );

                if( m_InputHistoryList.Count > m_HistoryMaxSize )
                {
                    m_InputHistoryList.RemoveRange( m_InputHistoryList.Count - 1, m_InputHistoryList.Count - m_HistoryMaxSize );
                }

                foreach( NSGamePadDevice deviceData in m_GamePadDeviceList ) deviceData.RemoveInputData();

                m_TouchDataList.Clear();
                m_LastGotInputDataTime = -1;
            }

            m_DeviceIdListToInputDataAdd.Clear();

            foreach( InputDevice inputDevice in InputSystem.devices )
            {
                if( inputDevice is Gamepad ) AddGamePadData( inputDevice );
#if UNITY_EDITOR
                else if( inputDevice is Joystick ) AddGamePadDataForJoyStick( inputDevice );
#endif
                else if( inputDevice is Pointer ) AddTouchData( inputDevice );
                //else
                //{
                //    Debug.Log( "Unknown Device : " + inputDevice.name );
                //}
            }

            UpdateDeviceState();

#if SWITCH_INPUT_ENABLE
            m_MouseState.Reset();

            if( IsGetMouseState ) m_MouseState = NSMouseManager.Instance.GetMouseState();
#endif
        }

        /// <summary>
        /// コントローラからの入力データを追加
        /// </summary>
        /// <remarks>このメソッドで取得できるコントローラーの種類はPC(Editor含む)の場合はXInput対応 Nintendo Swichでは対応コントローラー </remarks>
        /// <param name="_device"></param>
        private void AddGamePadData( InputDevice _device )
        {
            NSGamePadInputData inputData = default;
            int deviceId = NSGamePadInputData.CreateData( _device, ref inputData );

            if( deviceId > NSGamePadDefine.INVALID_DEVICE_ID && inputData.IsEnable )
            {
                NSGamePadDevice deviceData = m_GamePadDeviceList.Find( ( NSGamePadDevice data ) => data.DeviceId == deviceId );

                if( deviceData == null )
                {
                    //Debug.Log( "Create InputData : " + deviceId );
                    deviceData = new NSGamePadDevice( deviceId );
                    m_GamePadDeviceList.Add( deviceData );
                }

                deviceData.SetInputData( ref inputData );

                if( m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
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
            NSGamePadInputData inputData = default;
            int deviceId = NSGamePadInputData.CreateDataForJoyStick( _device, ref inputData );

            if( deviceId > NSGamePadDefine.INVALID_DEVICE_ID && inputData.IsEnable )
            {
                NSGamePadDevice deviceData = m_GamePadDeviceList.Find( ( NSGamePadDevice data ) => data.DeviceId == deviceId );

                if( deviceData == null )
                {
                    //Debug.Log( "Create InputData : " + deviceId );
                    deviceData = new NSGamePadDevice( deviceId );
                    m_GamePadDeviceList.Add( deviceData );
                }

                deviceData.SetInputData( ref inputData );

                if( m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
            }
        }
#endif //UNITY_EDITOR

        /// <summary>
        /// タッチ入力の追加
        /// </summary>
        /// <remarks> UnityEditorの場合はマウスの入力を追加する</remarks>
        /// <param name="_device"></param>
        private void AddTouchData( InputDevice _device )
        {
            List<NSTouchData> touchDataList = NSTouchData.CreateDataList( _device );

            if( touchDataList == null || touchDataList.Count == 0 ) return;

            m_TouchDataList.AddRange( touchDataList );

            if( m_TouchDataList.Count > 0 && m_LastGotInputDataTime < 0 ) m_LastGotInputDataTime = Time.unscaledTime;
        }

        /// <summary>
        /// コントローラーの状態更新
        /// </summary>
        private void UpdateDeviceState()
        {
            int connectDeviceCount = 0;

            CheckRemoveTarget();

            //入力データがあるコントローラーの状態を接続状態へ更新・ない場合は新規追加
            for( int i = 0; i < m_GamePadDeviceList.Count; i++ )
            {
                bool isConnect = m_GamePadDeviceList[ i ].InputData.IsEnable;

                m_GamePadDeviceList[ i ].UpdateState( isConnect );

                if( isConnect )
                {
                    connectDeviceCount++;
                }
                else
                {
                    if( m_GamePadDeviceList[ i ].ControllerStyle == ControllerStyle.Handheld )
                    {
                        m_GamePadDeviceList.RemoveAt( i );
                        i--;
                        continue;
                    }
                }

                if( IsAutoCheckDeviceConnect )
                {
                    if( !m_GamePadDeviceList[ i ].IsConnected )
                    {
                        if( m_GamePadDeviceList[ i ].ControllerStyle == ControllerStyle.JoyLeft || m_GamePadDeviceList[ i ].ControllerStyle == ControllerStyle.JoyRight )
                        {//Joy-Con1本持ちの場合
                            if( IsLRSAssignmentMode )
                            {
                                Debug.Log( "Remove DeviceData Index : " + i + "DeviceId : " + m_GamePadDeviceList[ i ].DeviceId );
                                // Joy-Con の L/R ボタン押しによる操作スタイル割り当てモードが有効の場合、
                                // 1本持ちから2本持ちに変えた場合にどちらかの1本持ちのデバイスが未接続扱いになってしまうので削除する                             
                                m_GamePadDeviceList.RemoveAt( i );
                                i--;
                                continue;
                            }
                        }

                        if( Time.unscaledTime - m_GamePadDeviceList[ i ].LastUpdateTime > m_WaitTimeToShowControllerSupport )
                        {//コントローラーの接続が切れてから一定時間経過したのでサポートアプレットを表示                        
                            ShowControllerSupport();
                            return;
                        }
                    }
                }
            }

            //コントローラー接続数を更新
            if( m_ConnectedDeviceCount != connectDeviceCount )
            {
                m_ConnectedDeviceCount = connectDeviceCount;
                m_LastUpdatedDeviceCountTime = Time.unscaledTime;
            }

            CheckControllerMinimumNum();

            //ゲームパッドID一覧を更新
            GamePadIdList.Clear();

            m_GamePadDeviceList.Sort( ( a, b ) => a.DeviceId - b.DeviceId );

            foreach( NSGamePadDevice device in m_GamePadDeviceList )
            {
                GamePadIdList.Add( device.DeviceId );
            }

#if SWITCH_INPUT_ENABLE
            //if( GamePadIdList.Count > 1 )
            //{
            //    bool isInputHandheld = false; //本体に接続したJoyConからの入力があるか
            //    bool isInputGamePad = false;  //それ以外のコントローラーからの入力があるか

            //    foreach( NSGamePadDevice device in m_GamePadDeviceList )
            //    {
            //        NpadId npadId = ( NpadId )device.DeviceId;

            //        if( npadId == NpadId.Handheld && device.ise) 
            //    }
            //}


            if( !IsConsole )
            {
                if( GamePadIdList.FindIndex( ( id ) => id == ( int )NPad.NpadId.Handheld ) >= 0 && GamePadIdList.Count > 1 )
                {
                    NSGamePadDevice handlehdDevice = GetInputDevice( ( int )NPad.NpadId.Handheld );

                    if( handlehdDevice != null && handlehdDevice.InputData.IsSomeInput() )
                    {//Handheldがあり、いずれかの入力がある場合はHandheldのIDだけ保持
                        GamePadIdList.Clear();
                        GamePadIdList.Add( ( int )NPad.NpadId.Handheld );
                    }
                    else
                    {//HandheldのIDを削除
                        GamePadIdList.Remove( ( int )NPad.NpadId.Handheld );
                    }
                }
            }
#endif

        }

        /// <summary>
        /// コントローラーの最小接続数未満の場合に一定時間経過したらサポートアプレットを表示する
        /// </summary>
        private void CheckControllerMinimumNum()
        {
            if( !IsAutoCheckMinimumDeviceCount ) return;

            //コントローラーが最小接続数より少なく、更新時間から一定時間経過している場合はコントローラーサポートアプレットを表示
            float progressTime = Time.unscaledTime - m_LastUpdatedDeviceCountTime;

            if( progressTime > m_WaitTimeToShowControllerSupport )
            {
                if( m_ConnectedDeviceCount < MinDeviceCount )
                {
                    ShowControllerSupport();
                }

                m_WaitTimeToShowControllerSupport = NSGamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;
            }
        }

        /// <summary>
        /// 不要になった等で対象のデバイスデータを探して削除
        /// </summary>
        private void CheckRemoveTarget()
        {
            //無効なNpadIdの状態データを事前に削除
            //m_GamePadDeviceList.RemoveAll( ( NSGamePadDevice inputDevice ) => !IsEnableNpadId( inputDevice.DeviceId ) );

#if SWITCH_INPUT_ENABLE            
            NSGamePadDevice handheldDevice = m_GamePadDeviceList.Find( ( NSGamePadDevice device ) => device.DeviceId == ( int )NPad.NpadId.Handheld );
            NSGamePadDevice no1Device = m_GamePadDeviceList.Find( ( NSGamePadDevice device ) => device.DeviceId == ( int )NPad.NpadId.No1 );

            if( handheldDevice != null )
            {
                if( m_GamePadDeviceList.Count > 2 )
                {// Handheldのコントローラーがあり、かつその他に2つ以上のコントローラーが接続されている場合はHandheldの入力を無視する
                    m_GamePadDeviceList.Remove( handheldDevice );
                }
            }

            if( m_GamePadDeviceList.Count <= 2 && handheldDevice != null && no1Device != null )
            {//HandheldとNo1のコントローラーのみがある場合、接続が切れているコントローラーを削除する
                if( !handheldDevice.IsConnected) m_GamePadDeviceList.Remove( handheldDevice );
                if( !no1Device.IsConnected ) m_GamePadDeviceList.Remove( no1Device );
            }

            ////NpadIdがHandheldかNo1の場合で切断している場合、No1かHandHeldが接続されている場合は差し替え扱いとして切断状態のデバイスデータを削除する
            //List<NSGamePadDevice> targetList = m_GamePadDeviceList.FindAll( ( NSGamePadDevice device ) => !device.IsConnected && ( device.DeviceId == ( int )NPad.NpadId.No1 || device.DeviceId == ( int )NPad.NpadId.Handheld ) );

            //if( targetList == null || targetList.Count <= 0 ) return;

            //foreach( NSGamePadDevice targetDevice in targetList )
            //{
            //    NPad.NpadId targetId = NPad.NpadId.Invalid;

            //    if( targetDevice.DeviceId == ( int )NPad.NpadId.No1 ) targetId = NPad.NpadId.Handheld;
            //    else if( targetDevice.DeviceId == ( int )NPad.NpadId.Handheld ) targetId = NPad.NpadId.No1;

            //    if( targetId == NPad.NpadId.Invalid ) continue;

            //    if( m_GamePadDeviceList.FindIndex( ( NSGamePadDevice device ) => device.DeviceId == ( int )targetId ) >= 0 )
            //    {
            //        m_GamePadDeviceList.Remove( targetDevice );
            //    }
            //}
#endif
        }

        /// <summary>
        /// デバイスデータの取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns></returns>
        public NSGamePadDevice GetInputDevice( int _deviceId )
        {
            if( m_GamePadDeviceList.Count <= 0 )
            {
                //Debug.Log( "Nothing DeviceData DeviceId : " + _deviceId );
                return null;
            }

            return m_GamePadDeviceList.Find( ( NSGamePadDevice deviceData ) => deviceData.DeviceId == _deviceId );
        }

        /// <summary>
        /// 現在接続されているコントローラーで、一番最初にあるものを取得
        /// </summary>
        /// <returns></returns>
        public NSGamePadDevice GetInputDeviceToLeadConnected()
        {
            if( GamePadIdList.Count > 0 ) return GetInputDevice( GamePadIdList[ 0 ] );

            return null;
        }

        /// <summary>
        /// <see cref="GamePadIdList"/>のインデックスを指定してデバイスデータを取得
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public NSGamePadDevice GetInputDeviceToIdLIstIndex( int _index )
        {
            if( GamePadIdList.Count <= _index )
            {
                return null;
            }

            return GetInputDevice( GamePadIdList[ _index ] );
        }

        /// <summary>
        /// デバイスID一覧を取得
        /// </summary>
        /// <returns></returns>
        public int[] GetGamepadDeviceIdList()
        {
            if( InputSystem.devices.Count <= 0 ) return null;

            List<int> idList = new List<int>();

            Debug.Log( "All Device Count : " + InputSystem.devices.Count );

            for( int i = 0; i < InputSystem.devices.Count; i++ )
            {
                InputDevice device = InputSystem.devices[ i ];

                Debug.Log( device.name );

#if SWITCH_INPUT_ENABLE
                if( !( device is NPad ) ) continue;

                NPad gamepad = device as NPad;

                idList.Add( ( int )gamepad.npadId );
#else //SWITCH_INPUT_ENABLE
                if( !( device is Gamepad ) && !( device is Joystick ) ) continue;

                idList.Add( device.deviceId );
#endif //SWITCH_INPUT_ENABLE
            }

            Debug.Log( "Enable Device Count : " + idList.Count );

            return idList.ToArray();
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
        /// 基本的に<see cref="NSGamePadInputData.IsHoldingKeyForKeepTime(GamepadKeyId, float)"/>から呼び出されていることを想定<br></br>
        /// それ以外からでも呼び出しは可能
        /// </remarks>
        /// <returns><see cref="HoldingResult"/></returns>
        public HoldingResult IsHoldingKeyForKeepTime( int _deviceId, GamepadKeyId _keyId, float _time )
        {
            NSGamePadDevice gamepad = GetInputDevice( _deviceId );

            if( gamepad == null ) return HoldingResult.InvalidDeviceId;
            if( !gamepad.InputData.IsHold( _keyId ) ) return HoldingResult.NotNowHold;

            int i = 0;

            for( i = 0; i < m_InputHistoryList.Count; i++ )
            {
                NSGamePadInputData inputData = m_InputHistoryList[ i ].InputDataList.Find( ( NSGamePadInputData p ) => p.DeviceId == _deviceId );

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
                if( !( inputDevice is Gamepad ) && !(inputDevice is Joystick ) ) continue;

                if( inputDevice.deviceId == _deviceId ) return true;
#endif //SWITCH_INPUT_ENABLE
            }

            return false;
        }

        /// <summary>
        /// Touch入力が有効かの判定
        /// </summary>        
        /// <returns>Nintendo Swicthの場合は携帯モードであればture<br></br>UnityEditorではポインタデバイス(マウスとか)が繋がっていればtrue</returns>
        public static bool IsEnableTouchDevice()
        {
#if SWITCH_INPUT_ENABLE
            return !Instance.IsConsole;
#else //SWITCH_INPUT_ENABLE
            foreach( InputDevice inputDevice in InputSystem.devices )
            {
                if( inputDevice is Pointer ) return true;
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

            Instance.m_WaitTimeToShowControllerSupport = NSGamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

            if( Instance.IsConsole ) Instance.m_WaitTimeToShowControllerSupport += NSGamePadDefine.ADD_WAIT_TIME_CONTROLLER_SUPPORT;
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

            Instance.IsCorrectInput = false;

            //結果にかかわらずデバイスの状態と入力を一旦削除
            Instance.m_WaitTimeToShowControllerSupport = NSGamePadDefine.WAIT_TIME_SHOW_CONTROLLER_SUPPOERT;

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

            Instance.ResetedDeviceProcess?.Invoke();
        }

        /// <summary>
        /// 現在のコントローラーの持ち方・数の設定を取得
        /// </summary>
        /// <returns></returns>
        public NSGamePadSetting GetInputSetting()
        {
            NSGamePadSetting inputSetting = new NSGamePadSetting();
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
            inputSetting.IsJoyLeft = true;
            inputSetting.IsJoyRight = true;
            inputSetting.IsHandheld = true;
            inputSetting.IsHoldHorizontalType = true;
#endif
            inputSetting.MinGamePadCount = MinDeviceCount;
            inputSetting.MaxGamePadCount = MaxDeviceCount;
            inputSetting.IsAutoCheckDeviceConnect = isActiveAndEnabled;
            inputSetting.IsAutoCheckMinimumDeviceCount = IsAutoCheckMinimumDeviceCount;

            Debug.Log( "GetInputSetting  DeviceStyle" +
                " IsFullkey : " + inputSetting.IsFullKey +
                " IsJoyDuel : " + inputSetting.IsJoyDuel +
                " IsJoyLeft : " + inputSetting.IsJoyLeft +
                " IsJoyRight : " + inputSetting.IsJoyRight +
                " IsHandHeld : " + inputSetting.IsHandheld +
                " MinCount : " + inputSetting.MinGamePadCount +
                " MaxCount : " + inputSetting.MaxGamePadCount +
                " IsHoldHorizontal : " + inputSetting.IsHoldHorizontalType +
                " isActiveAndEnabled : " + inputSetting.IsAutoCheckDeviceConnect +
                " IsAutoCheckMinimumDeviceCount : " + inputSetting.IsAutoCheckMinimumDeviceCount
                );

            return inputSetting;
        }

        /// <summary>
        /// コントローラーの持ち方・数を設定する
        /// </summary>
        /// <param name="_inputSetting">設定情報</param>
        public void SetInputSetting( NSGamePadSetting _inputSetting )
        {
            Debug.Log( "SetInputSetting  DeviceStyle" +
                " IsFullkey : " + _inputSetting.IsFullKey +
                " IsJoyDuel : " + _inputSetting.IsJoyDuel +
                " IsJoyLeft : " + _inputSetting.IsJoyLeft +
                " IsJoyRight : " + _inputSetting.IsJoyRight +
                " IsHandHeld : " + _inputSetting.IsHandheld +
                " MinCount : " + _inputSetting.MinGamePadCount +
                " MaxCount : " + _inputSetting.MaxGamePadCount +
                " IsHoldHorizontal : " + _inputSetting.IsHoldHorizontalType +
                " isActiveAndEnabled : " + _inputSetting.IsAutoCheckDeviceConnect +
                " IsAutoCheckMinimumDeviceCount : " + _inputSetting.IsAutoCheckMinimumDeviceCount
                );

            MinDeviceCount = _inputSetting.MinGamePadCount;
            MaxDeviceCount = ( byte )Mathf.Max( MinDeviceCount, _inputSetting.MaxGamePadCount );
            IsAutoCheckDeviceConnect = _inputSetting.IsAutoCheckDeviceConnect;
            IsAutoCheckMinimumDeviceCount = _inputSetting.IsAutoCheckMinimumDeviceCount;

#if SWITCH_INPUT_ENABLE
            NpadStyle npadStyle = NpadStyle.None;
            if( _inputSetting.IsFullKey ) npadStyle |= NpadStyle.FullKey;
            if( _inputSetting.IsJoyDuel ) npadStyle |= NpadStyle.JoyDual;
            if( _inputSetting.IsJoyLeft ) npadStyle |= NpadStyle.JoyLeft;
            if( _inputSetting.IsJoyRight ) npadStyle |= NpadStyle.JoyRight;
            if( _inputSetting.IsHandheld ) npadStyle |= NpadStyle.Handheld;

            m_EnableNpadIdList.Clear();

            for( int i = 0; i < MaxDeviceCount; i++ ) m_EnableNpadIdList.Add( ( NpadId )i );

            if( _inputSetting.IsHandheld ) m_EnableNpadIdList.Add( NpadId.Handheld );

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
#if SWITCH_INPUT_ENABLE
            return m_EnableNpadIdList.FindIndex( ( NpadId id ) => id == ( NpadId )_deviceId ) >= 0;
#else
            return true;
#endif
        }

        /// <summary>
        /// 指定のデバイスIDの現在からの入力履歴を取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns>入力履歴 先頭が最新の入力データ</returns>
        public List<NSGamePadHistory> GetGamePadHistory( int _deviceId )
        {
            List<NSGamePadHistory> historyList = new List<NSGamePadHistory>();

            NSGamePadDevice inputDevice = GetInputDevice( _deviceId );

            if( inputDevice == null ) return null;

            NSGamePadHistory inputHistory = default;

            if( inputDevice.InputData.IsEnable )
            {//現在の入力データがある場合は先頭に追加
                inputHistory = new NSGamePadHistory( m_LastGotInputDataTime, inputDevice.InputData );
                historyList.Add( inputHistory );
            }

            foreach( NSHistory history in m_InputHistoryList )
            {
                NSGamePadInputData inputData = history.InputDataList.Find( ( NSGamePadInputData input ) => input.DeviceId == _deviceId );

                if( !inputData.IsEnable ) continue;

                inputHistory = new NSGamePadHistory( history.GetDataTime, inputData );
                historyList.Add( inputHistory );
            }

            return historyList;
        }

        /// <summary>
        /// 指定のTouchIDからの入力を現在からの入力履歴を取得
        /// </summary>
        /// <param name="_touchId">TouchID</param>
        /// <returns>入力履歴 先頭が最新の入力データ</returns>
        public List<NSTouchHistory> GetTouchHIstory( int _touchId )
        {
            List<NSTouchHistory> historyList = new List<NSTouchHistory>();
            NSTouchData touchData = m_TouchDataList.Find( ( NSTouchData td ) => td.TouchId == _touchId );

            if( touchData.IsEnable )
            {//現在の入力データがある場合は先頭に追加
                historyList.Add( new NSTouchHistory( m_LastGotInputDataTime, touchData ) );
            }

            foreach( NSHistory history in m_InputHistoryList )
            {
                touchData = history.TouchDataList.Find( ( NSTouchData td ) => td.TouchId == _touchId );

                if( touchData.IsEnable )
                {
                    historyList.Add( new NSTouchHistory( history.GetDataTime, touchData ) );
                }
            }

            return historyList;
        }

        /// <summary>
        /// マウスの入力履歴を取得
        /// </summary>
        /// <returns></returns>
        public List<NSMouseHistory> GetMouseHistory()
        {
            List<NSMouseHistory> historyList = new List<NSMouseHistory>();

            foreach( NSHistory history in m_InputHistoryList )
            {
                historyList.Add( new NSMouseHistory( history.GetDataTime, history.MouseState ) );
            }

            return historyList;
        }

        /// <summary>
        /// デバイスIDを取得
        /// </summary>
        /// <param name="_device">InputDevice</param>
        /// <returns>Nintendo Switchの場合h<see cref="NPad.npadId"/>をint型にキャストした値 その他では<see cref="InputDevice.deviceId"/>の値</returns>
        public static int GetDeviceID( InputDevice _device )
        {
            int id = NSGamePadDefine.INVALID_DEVICE_ID;
#if SWITCH_INPUT_ENABLE
            //コントローラー以外のデバイスの場合は無効のIDを返す
            if( !( _device is NPad gamepad ) ) return NSGamePadDefine.INVALID_DEVICE_ID;
            // DebugPadは無効なIDとして返す
            if( gamepad.npadId == NPad.NpadId.Debug ) return NSGamePadDefine.INVALID_DEVICE_ID;

            //Joyコンの持ち方が2本持ちの場合、片方だけでも接続が切れたら入力を取得しない
            if( gamepad.styleMask == NPad.NpadStyles.JoyDual )
            {
                if( !gamepad.isLeftConnected || !gamepad.isRightConnected ) return NSGamePadDefine.INVALID_DEVICE_ID;
            }

            id = ( int )gamepad.npadId;
#else //SWITCH_INPUT_ENABLE
            if( _device is Gamepad ) id = _device.deviceId;
            else if( _device is Joystick ) id = _device.deviceId;
#endif //SWITCH_INPUT_ENABLE

            return id;
        }

        /// <summary>
        /// 指定のデバイスIDのコントローラーを切断する
        /// </summary>
        /// <remarks>Nintendo Switch以外の環境では何もしない</remarks>
        /// <param name="_deviceId"></param>
        public void DisconnectGamePad( int _deviceId )
        {
            NSGamePadDevice gamePadDevice = GetInputDevice( _deviceId );

            if( gamePadDevice != null ) m_GamePadDeviceList.Remove( gamePadDevice );

#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_deviceId;
            Npad.Disconnect( npadId );
#endif            
        }

        /// <summary>
        /// コントローラーの持ち方を取得
        /// </summary>
        /// <remarks>UnityEditor上ではコントローラーが繋がっていれば常に<see cref="ControllerStyle.FullKey"/>を返す</remarks>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns></returns>
        public static ControllerStyle GetControllerStyle( int _deviceId )
        {
#if SWITCH_INPUT_ENABLE
            NpadId npadId = ( NpadId )_deviceId;
            NpadStyle style = Npad.GetStyleSet( npadId );

            return ( ControllerStyle )style;
#else
            return ControllerStyle.FullKey;
#endif
        }

        /// <summary>
        /// コントローラーのプレイヤーランプの点灯パターンを取得
        /// </summary>
        /// <remarks>下位4bitを使用して表現 最下位bitが左端(縦持ちの場合は上端)の点灯状況</remarks>
        /// <returns>プレイヤーランプの点灯状況 Nintendo Switch以外の動作環境では常に0</returns>
        public static byte GetPlayerLedPattern( int _deviceId )
        {
#if SWITCH_INPUT_ENABLE
            return Npad.GetPlayerLedPattern( ( NpadId )_deviceId );
#else
            return 0;
#endif
        }

        /// <summary>
        /// コントローラーの色を取得
        /// </summary>
        /// <param name="_deviceId">デバイスID</param>
        /// <returns>左メイン・左サブ・右メイン・右サブの順で色を格納 Fullkeyでは左に取得した色 右は<see cref="Color.clear"/>を格納 NintendoSwitcj以外の環境では左右・メインサブともにメインは<see cref="Color.black"/> を返す</returns>
        public static Color[] GetControllerColor( int _deviceId )
        {
            Color[] colorList = new Color[ NSGamePadDefine.ControllerColorIndex.DATA_SIZE ];
            colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LM ] = Color.black;
            colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LS ] = Color.black;
            colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.black;
            colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.black;

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
                    Debug.Log( "Fullkey Color : " + leftColor.ToString() );
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LM ] = leftColor.main.ConvertColor();
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LS ] = leftColor.sub.ConvertColor();
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.clear;
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.clear;
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
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LM ] = leftColor.main.ConvertColor();
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LS ] = leftColor.sub.ConvertColor();
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RM ] = rightColor.main.ConvertColor();
                    colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RS ] = rightColor.sub.ConvertColor();

                    //Joyコン1本持ちの場合、接続されていない側の色を全て透明に設定
                    if( style == NpadStyle.JoyLeft )
                    {
                        colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RM ] = Color.clear;
                        colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_RS ] = Color.clear;
                    }
                    else if( style == NpadStyle.JoyRight )
                    {
                        colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LM ] = Color.clear;
                        colorList[ NSGamePadDefine.ControllerColorIndex.INDEX_LS ] = Color.clear;
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
    }
}
