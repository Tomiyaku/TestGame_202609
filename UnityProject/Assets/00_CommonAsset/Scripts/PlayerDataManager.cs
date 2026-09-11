using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CodeIcf.Input;
using CodeIcf.Input.GamePad;

using jp.co.liica.q2.SaveDataManagement;

namespace jp.co.liica.q2.Common
{
    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public class PlayerData
    {
        /// <summary>プレイヤーID</summary>
        public int PlayerID { get; private set; } = 0;
        /// <summary>ステージ中に操作するプレイヤーかのフラグ</summary>
        public bool IsControlStagePlaying;
        /// <summary>操作するコントローラーID</summary>
        public int ControllerDeviceID;
        /// <summary>キャラクターID</summary>
        public int CharacterID;
        /// <summary>スキンID</summary>
        public int SkinID;

        /// <summary>
        /// 未選択状態のデータを返す
        /// </summary>
        /// <returns></returns>
        public static PlayerData UnusedData( int _id )
        {
            PlayerData info = new PlayerData
            {
                PlayerID = _id,
                IsControlStagePlaying = false,
                ControllerDeviceID = -1,
                CharacterID = 0,
                SkinID = -1,
            };

            return info;
        }

    }

    /// <summary>
    /// プレイヤー情報管理
    /// </summary>
    public class PlayerDataManager
    {
        /// <summary>インスタンス</summary>
        private static PlayerDataManager m_Instance = null;
        /// <summary>プレイヤー情報一覧</summary>
        private PlayerData[] m_PlayerDataList = null;
        /// <summary>プレイヤー毎の色</summary>
        public static Color[] PlayerColorList { get; private set; } = null;
        /// <summary>プレイヤーとコントローラーの紐づけを更新するかのフラグ</summary>
        public static bool IsUpdateUseController = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PlayerDataManager()
        {
            Reset();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Reset()
        {
            m_PlayerDataList = null;
        }

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        private static void CreateInstance()
        {
            if( m_Instance == null )
            {
                m_Instance = new PlayerDataManager();

                PlayerColorList = new Color[ ApplicationDefine.PLAYER_COLOR_CODE_LIST.Length ];

                for( int i = 0; i < ApplicationDefine.PLAYER_COLOR_CODE_LIST.Length; i++ )
                {
                    ColorUtility.TryParseHtmlString( ApplicationDefine.PLAYER_COLOR_CODE_LIST[ i ], out PlayerColorList[ i ] );
                }
            }
        }

        /// <summary>
        /// インスタンスの破棄
        /// </summary>
        public static void RemoveInstance()
        {
            if( m_Instance == null ) return;

            m_Instance = null;
            IsUpdateUseController = true;
        }

        /// <summary>
        /// プレイヤーデータリストの作成
        /// </summary>
        private static void CreatePlayerDataList()
        {
            CreateInstance();

            if( m_Instance.m_PlayerDataList != null ) return;

            m_Instance.m_PlayerDataList = new PlayerData[ ApplicationDefine.PLAYER_MAX_COUNT ];

            for( int i = 0; i < ApplicationDefine.PLAYER_MAX_COUNT; i++ )
            {
                m_Instance.m_PlayerDataList[ i ] = PlayerData.UnusedData( i );
                m_Instance.m_PlayerDataList[ i ].CharacterID = SaveDataManager.Instance.GetPlayerUseCharaID( i );
                m_Instance.m_PlayerDataList[ i ].SkinID = SaveDataManager.Instance.GetPlayerUseCharaSkinID( i );
            }
        }

        /// <summary>
        /// プレイヤー情報一覧を取得
        /// </summary>        
        /// <returns></returns>
        public static PlayerData[] GetPlayerDataList()
        {
            CreatePlayerDataList();

            return m_Instance.m_PlayerDataList;
        }

        /// <summary>
        /// プレイヤー情報取得
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <returns></returns>
        public static PlayerData GetPlayerData( int _playerId )
        {
            CreatePlayerDataList();

            return System.Array.Find( m_Instance.m_PlayerDataList, ( playerData ) => playerData.PlayerID == _playerId );
        }

        /// <summary>
        /// プレイヤーが使用するキャラクターの更新
        /// </summary>
        public static void UpdateUseCharacter()
        {
            CreateInstance();

            if( m_Instance.m_PlayerDataList == null )
            {
                CreatePlayerDataList();
                return;
            }

            for( int i = 0; i < ApplicationDefine.PLAYER_MAX_COUNT; i++ )
            {
                m_Instance.m_PlayerDataList[ i ].CharacterID = SaveDataManager.Instance.GetPlayerUseCharaID( i );
                m_Instance.m_PlayerDataList[ i ].SkinID = SaveDataManager.Instance.GetPlayerUseCharaSkinID( i );
            }
        }

        /// <summary>
        /// 操作するコントローラーIDのの更新
        /// </summary>
        public static void UpdateUseControllerID()
        {
            CreatePlayerDataList();

            if( !IsUpdateUseController ) return;

            // デバイスID一覧を取得
            List<int> deviceIdList = InputDeviceManager.Instance.GetGamepadDeviceIdList();

            // 既にプレイヤースロットに設定済みのコントローラーIDをリストから除外
            foreach( PlayerData playerData in m_Instance.m_PlayerDataList )
            {
                int controllerDeviceId = playerData.ControllerDeviceID;

                if( controllerDeviceId < 0 ) continue;

                int index = deviceIdList.FindIndex( ( id ) => id == controllerDeviceId );

                if( index >= 0 )
                {//既にプレイヤーに割り当てられているのでデバイスID一覧での値を-1に変更
                    deviceIdList.RemoveAt( index );
                }
                else
                {//紐づけされているコントローラーが見つからないので未設定のデバイスがあれば割り当て
                    int j = 0;

                    for( j = 0; j < deviceIdList.Count; j++ )
                    {
                        if( deviceIdList[ j ] < 0 ) continue;
                        if( System.Array.FindIndex( m_Instance.m_PlayerDataList, ( playerData ) => playerData.ControllerDeviceID == deviceIdList[ j ] ) >= 0 ) continue;

                        UpdateUseControllerID( playerData.PlayerID, deviceIdList[ j ] );

                        break;
                    }

                    if( j < deviceIdList.Count )
                    {// コントローラーとプレイヤーを紐づけたのでデバイスIDをリストから削除
                        deviceIdList.RemoveAt( j );
                    }
                    else
                    {// プレイヤーの紐づけられたデバイスIDとの紐づけを解除
                        UnlinkUseController( playerData.PlayerID );
                    }
                }
            }

            // コントローラーが未設定のプレイヤーに紐づけされていないコントローラーを設定
            foreach( int devideId in deviceIdList )
            {
                int index = System.Array.FindIndex( m_Instance.m_PlayerDataList, ( info ) => info.ControllerDeviceID < 0 );

                if( index >= 0 ) UpdateUseControllerID( index, devideId );
            }
        }

        /// <summary>
        /// ステージ中でのプレイヤー毎の操作するコントローラーIDの更新
        /// </summary>
        public static void UpdateUseControllerIDToStagePlaying()
        {
            CreatePlayerDataList();

            if( !IsUpdateUseController ) return;

            // デバイスID一覧を取得
            List<int> deviceIdList = InputDeviceManager.Instance.GetGamepadDeviceIdList();

            // 既にプレイヤースロットに設定済みのコントローラーIDをリストから除外
            foreach( PlayerData playerData in m_Instance.m_PlayerDataList )
            {
                if( !playerData.IsControlStagePlaying ) continue;
                if( playerData.ControllerDeviceID < 0 ) continue;

                int index = deviceIdList.FindIndex( ( id ) => id == playerData.ControllerDeviceID );

                if( index >= 0 )
                {
                    deviceIdList.RemoveAt( index );
                }
                else
                {//紐づけされているコントローラーが見つからない場合紐づけを解除
                    GamePadDevice gamePadDevice = InputDeviceManager.Instance.GetGamePadDevice( playerData.ControllerDeviceID );

                    if( gamePadDevice == null || !gamePadDevice.IsConnected ) UnlinkUseController( playerData.PlayerID );
                }
            }

            // コントローラーが未設定のプレイヤーに紐づけされていないコントローラーを設定
            foreach( int deviceId in deviceIdList )
            {
                int index = System.Array.FindIndex( m_Instance.m_PlayerDataList, ( info ) => info.IsControlStagePlaying && info.ControllerDeviceID < 0 );

                if( index >= 0 ) UpdateUseControllerID( index, deviceId );
            }
        }

        /// <summary>
        /// 操作するコントローラーIDのの更新
        /// </summary>
        /// <param name="_playerId"></param>
        /// <param name="_controllerId"></param>
        public static void UpdateUseControllerID( int _playerId, int _controllerId )
        {
            CreatePlayerDataList();

            if( _playerId < 0 || _playerId >= m_Instance.m_PlayerDataList.Length ) return;

            //Debug.Log( "UpdateUseControllerID( " + _playerId + ", " + _controllerId + " )" );

            m_Instance.m_PlayerDataList[ _playerId ].ControllerDeviceID = _controllerId;
        }

        /// <summary>
        /// プレイヤーとコントローラーの紐づけを解除する
        /// </summary>
        /// <param name="_playerId"></param>
        public static void UnlinkUseController( int _playerId )
        {
            UpdateUseControllerID( _playerId, -1 );
        }

        /// <summary>
        /// プレイヤーを操作するコントローラーのデバイスIDを取得
        /// </summary>
        /// <param name="_playerId"></param>
        /// <returns></returns>
        public static int GetPlayerControllDeviceID( int _playerId )
        {
            CreatePlayerDataList();

            if( _playerId < 0 || _playerId >= m_Instance.m_PlayerDataList.Length ) return -1;

            PlayerData playerData = System.Array.Find( m_Instance.m_PlayerDataList, ( playerData ) => playerData.PlayerID == _playerId );

            return playerData != null ? playerData.ControllerDeviceID : -1;
        }

        /// <summary>
        /// プレイヤーを操作する最初のデバイスIDを取得
        /// 0:接続無し
        /// 1:接続無し
        /// 2:接続あり
        /// となっていたら[2]を操作しているデバイスIDを渡す
        /// </summary>

        public static int GetFirstControllDeviceID()
        {
            CreatePlayerDataList();
            if (m_Instance.m_PlayerDataList.All(data => data.ControllerDeviceID == -1)) return -1;
            return m_Instance.m_PlayerDataList.Select(data => data.ControllerDeviceID).First(id => id != -1);
        }

        /// <summary>
        /// 指定のコントローラーのデバイスIDを割り当てられているプレイヤーのIDを取得
        /// </summary>
        /// <param name="_controllDeviceId"></param>
        /// <returns></returns>
        public static int GetPlayerIDToControlDeviceID( int _controllDeviceId )
        {
            CreatePlayerDataList();

            return System.Array.FindIndex( m_Instance.m_PlayerDataList, ( playerDaya ) => playerDaya.ControllerDeviceID == _controllDeviceId );
        }

        /// <summary>
        /// 有効なプレイヤーのIDを昇順で取得
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerIDToLeard()
        {
            CreatePlayerDataList();

            return System.Array.FindIndex( m_Instance.m_PlayerDataList, ( playerDaya ) => playerDaya.IsControlStagePlaying );
        }

        /// <summary>
        /// プレイヤーIDの昇順で接続されているコントローラーIDを取得
        /// </summary>
        /// <returns></returns>
        public static GamePadDevice GetPlayerControllDeviceToLeadConnected()
        {
            CreatePlayerDataList();

            foreach( PlayerData playerData in m_Instance.m_PlayerDataList )
            {
                if( playerData.ControllerDeviceID < 0 ) continue;

                GamePadDevice gamePadDevice = InputDeviceManager.Instance.GetGamePadDevice( playerData.ControllerDeviceID );

                if( gamePadDevice != null && gamePadDevice.IsConnected ) return gamePadDevice;
            }

            return null;
        }

        /// <summary>
        /// 指定のキーを押下したプレイヤーIDを取得
        /// </summary>
        /// <param name="_keyPadId">キーID</param>
        /// <returns>指定のキーを押下したコントローラーIDが設定されているプレイヤーID 複数ある場合にはプレイヤーIDが一番小さいものを返す</returns>
        public static int GetKeyDownPlayerID( GamepadKeyId _keyPadId )
        {
            CreatePlayerDataList();

            for( int i = 0; i < m_Instance.m_PlayerDataList.Length; i++ )
            {
                if( m_Instance.m_PlayerDataList[ i ].ControllerDeviceID < 0 ) continue;

                GamePadDevice gamePadDevice = InputDeviceManager.Instance.GetGamePadDevice( m_Instance.m_PlayerDataList[ i ].ControllerDeviceID );

                if( gamePadDevice == null ) continue;

                if( gamePadDevice.IsDown( _keyPadId ) ) return i;
            }

            return -1;
        }

        /// <summary>
        /// 全てのプレイヤーが操作するコントローラーIDをリセットする
        /// </summary>
        public static void ResetPlayerControlDeviceID()
        {
            CreatePlayerDataList();

            for( int i = 0; i < m_Instance.m_PlayerDataList.Length; i++ )
            {
                m_Instance.m_PlayerDataList[ i ].ControllerDeviceID = -1;
            }
        }

        /// <summary>
        /// ステージ中に使用するプレイヤーを決定する
        /// </summary>
        public static void UpdateControlPlayerInStagePlaying()
        {
            CreatePlayerDataList();

            //プレイヤーを操作するコントローラーID一覧
            List<int> playerControlDeviceIdLIst = new List<int>();

            foreach( PlayerData playerData in m_Instance.m_PlayerDataList )
            {
                playerData.IsControlStagePlaying = false;

                //デバイスIDが設定されていないプレイヤーは操作しない
                if( playerData.ControllerDeviceID < 0 ) continue;

                GamePadDevice gamePadDevice = InputDeviceManager.Instance.GetGamePadDevice( playerData.ControllerDeviceID );

                if( gamePadDevice == null || !gamePadDevice.IsConnected || gamePadDevice.ControllerStyle == ControllerStyle.None || gamePadDevice.ControllerStyle == ControllerStyle.Invalid )
                {//コントローラーが正常に取得できないプレイヤーは操作しない
                    playerData.IsControlStagePlaying = false;
                    InputDeviceManager.Instance.DisconnectGamePad( playerData.ControllerDeviceID );
                    UnlinkUseController( playerData.PlayerID );
                }
                else
                {// ステージ中に操作するキャラに指定
                    playerData.IsControlStagePlaying = true;
                    playerControlDeviceIdLIst.Add( playerData.ControllerDeviceID );
                }
            }

            InputDeviceManager.Instance.CheckRemoveNotConnectedGamePad();

            //プレイヤーを操作するコントローラー以外を切断する
            List<int> deviceIdList = InputDeviceManager.Instance.GetGamepadDeviceIdList();

            foreach( int devideId in deviceIdList )
            {
                if( playerControlDeviceIdLIst.FindIndex( ( id ) => id == devideId ) < 0 )
                {
                    InputDeviceManager.Instance.DisconnectGamePad( devideId );
                }
            }
        }
    }
}