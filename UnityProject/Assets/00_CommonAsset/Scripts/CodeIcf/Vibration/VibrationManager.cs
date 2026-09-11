using System.Collections.Generic;
using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;
using CodeIcf.Input;

namespace CodeIcf.VibrationManagement
{
    /// <summary>
    /// 現在再生している振動データ
    /// </summary>
    [System.Serializable]
    public class VibrationPlayingData
    {
        /// <summary>対象のコントローラーのデバイスID</summary>
        public int DeviceID;
        /// <summary>振動データ</summary>
        public VibrationData VibrationData;
        /// <summary>再生時間</summary>
        public float StartTime;
        /// <summary>再生中フラグ</summary>
        public bool IsPlaying => VibrationData != null && StartTime >= 0 && Time.unscaledTime - StartTime <= VibrationData.Time;
    }

    /// <summary>
    /// 振動管理ベースクラス
    /// </summary>
    public class VibrationManager : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static VibrationManager m_Instance = null;
        /// <summary>インスタンス</summary>
        public static VibrationManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    GameObject obj = new GameObject( "VibrationManager" );
                    m_Instance = obj.AddComponent<VibrationManager>();

                    m_Instance.IsEnableVibration = new FlagCheckFunc();

                    DontDestroyOnLoad( m_Instance );
                }

                return m_Instance;
            }
        }

        /// <summary>振動データ一覧</summary>
        [SerializeField]
        private List<VibrationData> m_DataList = new List<VibrationData>();
        /// <summary>再生中の振動データ一覧</summary>
        [SerializeField]
        private List<VibrationPlayingData> m_PlayDataList = new List<VibrationPlayingData>();
        /// <summary>読み込み中の振動データの<see cref="AddressableStorage"/>一覧</summary>
        private List<AddressableStorage> m_LoadingStorageList = new List<AddressableStorage>();
        /// <summary>読み込み中フラグ</summary>        
        public bool IsLoading { get; private set; } = false;
        /// <summary>振動をさせるかの判定返すdelegate</summary>
        public FlagCheckFunc IsEnableVibration { get; set; }

        private void Update()
        {
            if( IsLoading )
            {
                foreach( AddressableStorage storage in m_LoadingStorageList )
                {
                    if( storage.State == AddressableStorage.AssetState.Enable )
                    {
                        VibrationData vibrationData = storage.GetAsset<VibrationData>();

                        if( vibrationData != null ) m_DataList.Add( vibrationData );
                    }
                }

                m_LoadingStorageList.RemoveAll( ( storage ) => storage.State == AddressableStorage.AssetState.Enable );

                IsLoading = m_LoadingStorageList.Count > 0;
            }

            foreach( VibrationPlayingData playData in m_PlayDataList )
            {
                if( !playData.IsPlaying ) InputDeviceManager.Instance.StopVibration( playData.DeviceID );
            }

            m_PlayDataList.RemoveAll( ( playData ) => !playData.IsPlaying );
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        public void Load()
        {
            int count = AddressableResourcesManager.Instance.LoadAssetAllInCategory( AddressableCategory.Vibration, out _, out AddressableStorage[] storagrList );

            m_DataList.Clear();
            m_LoadingStorageList = new List<AddressableStorage>( storagrList );

            IsLoading = true;
        }

        /// <summary>
        /// 振動を再生
        /// </summary>
        /// <param name="_type"> 振動の種類</param>
        /// <param name="_deviceId">対象のコントローラーのデバイスID</param>
        /// <param name="_isForced">セーブデータの状態にかかわらず振動させるかのフラグ</param>
        public void Play( VibrationType _type, int _deviceId, bool _isForced = false )
        {
            if( IsLoading ) return;
            if( !_isForced && !IsEnableVibration.IsFlag() ) return;

            VibrationData vibrationData = m_DataList.Find( ( data ) => data.Type == _type );

            if( vibrationData == null ) return;

            InputDeviceManager.Instance.PlayVibration( _deviceId, vibrationData.AmplitudeLow, vibrationData.AmplitudeHigh );

            VibrationPlayingData playData = new VibrationPlayingData()
            {
                DeviceID = _deviceId,
                VibrationData = vibrationData,
                StartTime = Time.unscaledTime,
            };

            m_PlayDataList.Add( playData );
        }

        /// <summary>
        /// 振動を再生
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_deviceIdList">対象のコントローラーのデバイスID一覧</param>
        /// <param name="_isForced"></param>
        public void Play( VibrationType _type, int[] _deviceIdList, bool _isForced = false )
        {
            if( _deviceIdList == null ) return;

            foreach( int deviceId in _deviceIdList )
            {
                Play( _type, deviceId, _isForced );
            }
        }

        /// <summary>
        /// 振動の停止
        /// </summary>
        /// <param name="_deviceId"></param>
        public void Stop( int _deviceId )
        {
            InputDeviceManager.Instance.PlayVibration( _deviceId, 0, 0 );
        }

        /// <summary>
        /// 振動の停止
        /// </summary>
        /// <param name="_deviceIdList"></param>
        public void Stop( int[] _deviceIdList )
        {
            if( _deviceIdList == null ) return;

            foreach( int deviceId in _deviceIdList )
            {
                Stop( deviceId );
            }
        }

        /// <summary>
        /// 振動の停止
        /// </summary>
        public void StopAll()
        {
            InputDeviceManager.Instance.StopVibrationToAllDevice();
            m_PlayDataList.Clear();
        }

        /// <summary>
        /// いづれかの振動が再生中か
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if( m_PlayDataList.Count <= 0 ) return false;

            foreach( VibrationPlayingData playData in m_PlayDataList )
            {
                if( playData.IsPlaying ) return true;
            }

            return false;
        }

        /// <summary>
        /// 指定のデバイスIDのコントローラーで振動が再生中か
        /// </summary>
        /// <param name="_deviceId"></param>
        /// <returns></returns>
        public bool IsPlaying( int _deviceId )
        {
            foreach( VibrationPlayingData playData in m_PlayDataList )
            {
                if( playData.DeviceID != _deviceId ) continue;

                return playData.IsPlaying;
            }

            return false;
        }
    }
}