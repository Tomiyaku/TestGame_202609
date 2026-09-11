using UnityEngine;

#if UNITY_SWITCH
using UnityEngine.Switch;
#endif

namespace CodeIcf
{
    /// <summary>
    /// Switchからの通知を受け取り、設定された処理へ振り分けを管理
    /// </summary>
    public class NotificationFromSwitch
    {
        /// <summary>インスタンス</summary>
        private static NotificationFromSwitch m_Instance = null;
        /// <summary>
        /// インスタンス
        /// </summary>
        public static NotificationFromSwitch Instance
        {
            get
            {
                if( m_Instance == null ) m_Instance = new NotificationFromSwitch();

                return m_Instance;
            }
        }

#if UNITY_SWITCH
        /// <summary>動作モードが切り替わったときに実行する処理</summary>
        private System.Action<Operation.OperationMode> m_OperationModeChangeEvent = null;
        /// <summary>フォーカスが切り替わった時に実行する処理 </summary>
        private System.Action<Notification.FocusState> m_FocusStateChangeEvent = null;
        /// <summary>性能モードが切り替わった時に実行する処理</summary>
        private System.Action<Performance.PerformanceMode> m_PerformanceChangeEvent = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private NotificationFromSwitch()
        {
            Notification.notificationMessageReceived += NotificationMessageReceived;
            Notification.SetFocusHandlingMode( Notification.FocusHandlingMode.Notify );
        }

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="_message"></param>
        static void NotificationMessageReceived( Notification.Message _message )
        {
            Debug.Log( "NotificationMessageReceived:" + _message );

            if( _message == Notification.Message.OperationModeChanged )
            {
                Operation.OperationMode operationMode = Operation.mode;

                Debug.Log( "OperationModeChanged Now: " + operationMode );

                Instance.m_OperationModeChangeEvent?.Invoke( operationMode );
            }
            else if( _message == Notification.Message.FocusStateChanged )
            {
                Notification.FocusState focusState = Notification.GetCurrentFocusState();

                Debug.Log( "FocusStateChanged Now: " + focusState );

                Instance.m_FocusStateChangeEvent?.Invoke( focusState );
            }
            else  if( _message == Notification.Message.PerformanceModeChanged )
            {
                Performance.PerformanceMode performanceMode = Performance.mode;

                Debug.Log( "PerformanceChanged Now: " + performanceMode );

                Instance.m_PerformanceChangeEvent?.Invoke( performanceMode );
            }
        }

        /// <summary>
        /// 動作モード切り替え時に実行するイベントの設定
        /// </summary>
        /// <param name="_action"></param>
        public void AddOperationModeChangedNotification( System.Action<Operation.OperationMode> _action )
        {
            Notification.SetOperationModeChangedNotificationEnabled( true );
            m_OperationModeChangeEvent += _action;
        }

        /// <summary>
        /// フォーカスの切り替え時に実行するイベントの設定
        /// </summary>
        /// <param name="_action"></param>
        public void AddFocusStateChangedNotification( System.Action<Notification.FocusState> _action )
        {
            m_FocusStateChangeEvent += _action;
        }

        public void AddPerformanceChangedNotification( System.Action<Performance.PerformanceMode> _action )
        {
            Notification.SetPerformanceModeChangedNotificationEnabled( true );
            m_PerformanceChangeEvent += _action;
        }
#endif

        public void RemoveAllOperationModeChangedNotification()
        {
#if UNITY_SWITCH
            Notification.SetOperationModeChangedNotificationEnabled( false );
            m_Instance.m_OperationModeChangeEvent = null;
#endif
        }

        public void RemoveAllFocusStateChangedNotification()
        {
#if UNITY_SWITCH
            m_Instance.m_FocusStateChangeEvent = null;
#endif
        }

        public void RemoveAllPerformanceChangedNotification()
        {
#if UNITY_SWITCH
            m_Instance.m_PerformanceChangeEvent = null;
#endif
        }
    }
}
