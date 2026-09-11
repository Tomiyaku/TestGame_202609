using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;

using jp.co.liica.q2.Common;
using jp.co.liica.q2.SaveDataManagement;

namespace jp.co.liica.q2.Achievement
{
    /// <summary>
    /// 実績管理
    /// </summary>
    /// <remarks>Steamworksの実績に関連する処理は全てこのクラスで呼び出すことを想定</remarks>
    public class AchievementManager : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static AchievementManager m_Instance = null;

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        public static void CreateInstance()
        {
            if( m_Instance != null ) return;

            GameObject obj = new GameObject( "AchievementManager" );
            m_Instance = obj.AddComponent<AchievementManager>();

            DontDestroyOnLoad( m_Instance );

#if STEAMWORKS_NET
            SteamBridge.Instance.UserStatsReceivedHandler = UserStatsReceived;
            SteamBridge.Instance.InitUserStats();
#endif
        }

        /// <summary>実績データ</summary>
        [SerializeField]
        private SteamAchievementData m_SteamAchivementData = null;
#if STEAMWORKS_NET
        public static SteamAchievementData SteamAchievementData => m_Instance != null ? m_Instance.m_SteamAchivementData : null;
#endif
        /// <summary><see cref="m_SteamAchivementData"/>が読み込み済み科のフラグ</summary>
        public static bool IsLoadedAchivementData => m_Instance != null ? m_Instance.m_SteamAchivementData != null : false;
        /// <summary>描いた線の長さ</summary>
        [SerializeField]
        private uint m_DrawLength = 0;
        /// <summary>リトライ回数</summary>
        [SerializeField]
        private uint m_RetryCount = 0;
        /// <summary>実績を解除する処理を実行できるかのフラグ</summary>
        private static bool IsEnableAchivementProcess { get; set; } = false;
        /// <summary>現在のフレーム中に1度でも<see cref="SteamBridge.SetAchievement(string)"/>が実行された科のフラグ</summary>
        public static bool IsAchived { get; private set; } = false;

        /// <summary>
        /// 実績関連の処理の準備ができているかのフラグ
        /// </summary>
        /// <remarks>PC(Steam)以外の場合は常にtrueを返すが、実績関連の処理は呼び出しても実行されない</remarks>
        public static bool IsReadyAchivement => IsLoadedAchivementData && IsEnableAchivementProcess;
        private void OnApplicationQuit()
        {
            SaveCountData();
        }

#if STEAMWORKS_NET
        private void Update()
        {
            if( m_SteamAchivementData != null ) return;
            if( !AddressableResourcesManager.Instance.IsLoadComplitedInfoList ) return;

            AddressableLoadResult loadResult = AddressableResourcesManager.Instance.LoadAsset( AchievementIdentifierDefine.STEAM_ACHIEVEMENT_DATA, out AddressableStorage storage );

            if( loadResult == AddressableLoadResult.Complited )
            {
                m_SteamAchivementData = storage.GetAsset<SteamAchievementData>();

                // 昇順ソート
                System.Array.Sort( m_SteamAchivementData.UnlockCharacterAchivementList, ( a, b ) => a.TargetParam - b.TargetParam );
                System.Array.Sort( m_SteamAchivementData.ClearedStageCountAchivementList, ( a, b ) => a.TargetParam - b.TargetParam );
                System.Array.Sort( m_SteamAchivementData.GetMedalCountAchivementList, ( a, b ) => a.TargetParam - b.TargetParam );
                System.Array.Sort( m_SteamAchivementData.DrawLengthAchivementList, ( a, b ) => a.TargetParam - b.TargetParam );
                System.Array.Sort( m_SteamAchivementData.RetryCountAchivementList, ( a, b ) => a.TargetParam - b.TargetParam );
            }
        }

        private void LateUpdate()
        {
            if( IsEnableAchivementProcess && IsAchived )
            {
                StoreStats();
            }

            IsAchived = false;
        }
#endif

        /// <summary>
        /// SteamBridge.InitUserStatsの処理が終了したコールバックされる処理
        /// </summary>
        /// <param name="_result"></param>
        private static void UserStatsReceived( bool _result )
        {
#if STEAMWORKS_NET
            IsEnableAchivementProcess = _result;
            SteamBridge.Instance.UnlockAchivementStoredProcess = m_Instance.CheckCompliteAchievement;
#endif
        }

        /// <summary>
        /// 初回起動の実績の解除判定
        /// </summary>
        /// <remarks>No.1</remarks>
        public static void CheckAchievementFirstBoot()
        {
#if STEAMWORKS_NET
            CreateInstance();

            if( !IsReadyAchivement ) return;
            if( m_Instance.m_SteamAchivementData == null ) return;

            // 初回起動の実績の解除
            m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstBootApiName );
#endif
        }

        /// <summary>
        /// キャラ獲得の実績解除判定
        /// </summary>
        /// <remarks>No.8~25</remarks>
        /// <param name="_charaId"></param>
        public static void CheckAchievementUnlockCharacter( int _charaId )
        {
#if STEAMWORKS_NET
            CreateInstance();

            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            foreach( TargetParamAchievement achivement in m_Instance.m_SteamAchivementData.UnlockCharacterAchivementList )
            {
                if( achivement.TargetParam == _charaId ) m_Instance.SetAchivement( achivement.ApiName );
            }
#endif
        }

        /// <summary>
        /// 特定のステージをクリアした実績の解除判定
        /// </summary>
        /// <remarks>No.2,4,5,6,</remarks>
        /// <param name="_stageId"></param>
        public static void CheckAchievementStageClear( int _stageId )
        {
#if STEAMWORKS_NET
            CreateInstance();

            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( m_Instance.m_SteamAchivementData.FirstClearedStage.TargetParam == _stageId )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstClearedStage.ApiName );
            }

            if( m_Instance.m_SteamAchivementData.CrearedTutorialStage.TargetParam == _stageId )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.CrearedTutorialStage.ApiName );
            }

            m_Instance.SetClearedStageCount();
            m_Instance.CheckAchievementAllCleardSurfaceStage();
            m_Instance.CheckAchievementAllCleardReverseStage();
#endif
        }

        /// <summary>
        /// ステージ選択画面での操作可能になる前に、特定のステージのクリアによるステージの開放実績の解除判定
        /// </summary>
        /// <param name="_clearedStageId"></param>
        public static void CheckAchievementUnlockStage()
        {
#if STEAMWORKS_NET
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( SaveDataManager.Instance.GetStageCondition( m_Instance.m_SteamAchivementData.FirstUnlockkNewArea.TargetParam ) >= StageCondition.Cleared )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstUnlockkNewArea.ApiName );
            }

            if( SaveDataManager.Instance.GetStageCondition( m_Instance.m_SteamAchivementData.UnlockRevaerceSide.TargetParam ) >= StageCondition.Cleared )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.UnlockRevaerceSide.ApiName );
            }
#endif
        }

#if STEAMWORKS_NET
        /// <summary>
        /// クリアした問題の数に関する実績の解除判定
        /// </summary>
        /// <remarks>No.26~37</remarks>
        private void SetClearedStageCount()
        {
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            int clearedStageCount = Mathf.Clamp( SaveDataManager.Instance.GetClearedStoryModeStageCount(), 0, ApplicationDefine.ALL_STAGE_COUNT );

            if( SteamBridge.Instance.GetStats( m_SteamAchivementData.ClearedStageCountDataApiName, out int currentCount ) )
            {
                // 現在値と異なる場合はフレーム後に更新を実行する
                if( currentCount != clearedStageCount )
                {
                    // ステージクリア数の進行状況を更新
                    SteamBridge.Instance.SetStats( m_SteamAchivementData.ClearedStageCountDataApiName, clearedStageCount );
                    IsAchived = true;
                }
            }
        }

        /// <summary>
        /// 表面の全てのステージをクリアした実績の解除判定
        /// </summary>
        /// <remarks>No.38</remarks>
        private void CheckAchievementAllCleardSurfaceStage()
        {
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( SaveDataManager.Instance.IsAllClearedSurfaceStage( out int clearedStageCount ) )
            {
                // 表面の全てのステージの実績の解除
                SetAchivement( m_SteamAchivementData.AllCleardSurfaceStageApiName );
            }
        }

        /// <summary>
        /// 裏面の全てのステージをクリアした実績の解除判定
        /// </summary>
        /// <remarks>No.39</remarks>
        private void CheckAchievementAllCleardReverseStage()
        {
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( SaveDataManager.Instance.IsAllClearedReverseStage( out int clearedStageCount ) )
            {
                // 裏面の全てのステージの実績の解除
                SetAchivement( m_SteamAchivementData.AllCleardReverseStageApiName );
            }
        }
#endif

        /// <summary>
        /// ２人以上でステージをプレイした実績の解除判定
        /// </summary>
        /// <remarks>No.7</remarks>
        public static void CheckAchievementMultiPlay()
        {
#if STEAMWORKS_NET
            CreateInstance();

            if( !IsReadyAchivement ) return;

            PlayerData[] playerList = PlayerDataManager.GetPlayerDataList();

            if( playerList == null || playerList.Length <= 0 ) return;

            int stagePlayingCount = 0;

            foreach( PlayerData player in playerList )
            {
                if( player.IsControlStagePlaying ) stagePlayingCount++;
            }

            if( stagePlayingCount > 1 )
            {
                // 2人以上でステージをプレイした実績の解除
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.MultiPlayApiName );
            }
#endif
        }

        /// <summary>
        /// メダルの取得数に関する実績の解除判定
        /// </summary>
        /// <remarks>No.40~51</remarks>
        public static void CheckAchievementGetMedal()
        {
#if STEAMWORKS_NET
            CreateInstance();

            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            int gettedMedalCount = Mathf.Clamp( SaveDataManager.Instance.GetUnlockCountsToStoryModeMedal(), 0, ApplicationDefine.ALL_MEDAL_COUNT );

            if( gettedMedalCount >= m_Instance.m_SteamAchivementData.FirstGetMedal.TargetParam ) m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstGetMedal.ApiName );

            m_Instance.SetGetMedalCount( gettedMedalCount );
            m_Instance.CheckAchievementAllUnlockMedalToSurfaceStage();
            m_Instance.CheckAchievementAllUnlockMedalToReverseStage();
            m_Instance.CheckAchievementAllStageCleredAndAllUnlockMedal();
#endif
        }

#if STEAMWORKS_NET
        /// <summary>
        /// 取得済みメダルの数を更新
        /// </summary>
        /// <param name="_medalCount"></param>
        private void SetGetMedalCount( int _medalCount )
        {
            if( SteamBridge.Instance.GetStats( m_SteamAchivementData.GetMedalCountDataApiName, out int currentCount ) )
            {
                // 現在値と異なる場合はフレーム後に更新を実行する
                if( currentCount != _medalCount )
                {
                    // メダル獲得数の進行状況を更新
                    SteamBridge.Instance.SetStats( m_SteamAchivementData.GetMedalCountDataApiName, _medalCount );
                    IsAchived = true;
                }
            }
        }

        /// <summary>
        /// 表面に配置されている全てのメダルを取得した実績の解除判定
        /// </summary>
        /// <remarks>No.52</remarks>
        private void CheckAchievementAllUnlockMedalToSurfaceStage()
        {
            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( SaveDataManager.Instance.IsAllUnlockMedalToSurfaceStage( out int unlockMedalCount ) )
            {
                // 表面に配置されている全てのメダルを取得の実績の解除
                SetAchivement( m_SteamAchivementData.AllGetMedalToSurfaceStageApiName );
            }
        }

        /// <summary>
        /// 裏面に配置されている全てのメダルを取得した実績の解除判定
        /// </summary>
        /// <remarks>No.53<remarks>
        private void CheckAchievementAllUnlockMedalToReverseStage()
        {
            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            if( SaveDataManager.Instance.IsAllUnlockMedalToReverseStage( out int unlockMedalCount ) )
            {
                // 表面に配置されている全てのメダルを取得の実績の解除
                SetAchivement( m_SteamAchivementData.AllGetMedalToReverseStageApiName );
            }
        }

        /// <summary>
        /// 表・裏ステージを全てクリアし、配置されている全てのメダルを取得した実績の解除判定
        /// </summary>
        /// <remarks>No.54</remarks>
        private void CheckAchievementAllStageCleredAndAllUnlockMedal()
        {
            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;
            if( !SaveDataManager.Instance.IsAllClearedStage() ) return;
            if( !SaveDataManager.Instance.IsAllUnlockMedal() ) return;

            // 全てのメダルを取得していれば全ステージクリアしているので、実績の解除
            SetAchivement( m_SteamAchivementData.AllStageCleredAndAllGetkMedalApiName );
        }
#endif

        /// <summary>
        /// 描いた線の長さの追加
        /// </summary>
        /// <param name="_length"></param>
        public static void AddDrawLength( int _length )
        {
            AddDrawLength( ( uint )_length );
        }

        /// <summary>
        /// 描いた線の長さの追加
        /// </summary>
        /// <param name="_length"></param>
        public static void AddDrawLength( float _length )
        {
            AddDrawLength( ( uint )_length );
        }

        /// <summary>
        /// 描いた線の長さの追加
        /// </summary>
        /// <param name="_length"></param>
        public static void AddDrawLength( uint _length )
        {
            CreateInstance();

            m_Instance.m_DrawLength += _length;
#if STEAMWORKS_NET
            m_Instance.CheckAchievementDrawLength();
#endif
        }

#if STEAMWORKS_NET
        /// <summary>
        /// 描いた線の長さに関する実績の解除判定
        /// </summary>
        /// <remarks>No.55~64</remarks>
        private void CheckAchievementDrawLength()
        {
            if( !IsReadyAchivement ) return;

            if( !SaveDataManager.Instance.IsNetworkMode )
            {
                uint totalDrawLength = SaveDataManager.Instance.GetDrawLineLength() + m_DrawLength;

                if( totalDrawLength > AchievementCountDataDeifne.LIMIT_DRAW_LINE_LENGTH ) totalDrawLength = AchievementCountDataDeifne.LIMIT_DRAW_LINE_LENGTH;

                SetDrawLength( ( int )totalDrawLength );
            }

            CheckAchievementDrawLengthToOneStage();
        }

        private void SetDrawLength( int _drawLength )
        {
            if( SteamBridge.Instance.GetStats( m_SteamAchivementData.DrawLehgthDataApiName, out int currentCount ) )
            {
                // 現在値と異なる場合はフレーム後に更新を実行する
                if( currentCount != _drawLength )
                {
                    // 描いた線の長さの進行状況を更新
                    SteamBridge.Instance.SetStats( m_SteamAchivementData.DrawLehgthDataApiName, _drawLength );
                    IsAchived = true;
                }
            }
        }

        /// <summary>
        /// 1ステージ内での描いた線の長さの実績の解除判定
        /// </summary>
        /// <remarks>No.65</remarks>
        private void CheckAchievementDrawLengthToOneStage()
        {
            if( !IsReadyAchivement ) return;
            if( m_DrawLength < m_SteamAchivementData.DrawLength1000mToOneStage.TargetParam ) return;

            // m_DrawLengthの値を元に実績を解除
            SetAchivement( m_SteamAchivementData.DrawLength1000mToOneStage.ApiName );
        }
#endif

        /// <summary>
        /// リトライ回数の追加
        /// </summary>
        /// <param name="_count"></param>
        public static void AddRetryCount( uint _count = 1 )
        {
            CreateInstance();

            m_Instance.m_RetryCount += _count;
#if STEAMWORKS_NET
            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            m_Instance.SetRetryCount();
#endif
        }

#if STEAMWORKS_NET
        /// <summary>
        /// リトライ回数に関する実績の解除判定
        /// </summary>
        /// <remarks>No.66,67</remarks>
        private void SetRetryCount()
        {
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            uint totalRetryCount = SaveDataManager.Instance.GetRetryCount() + m_RetryCount;

            if( totalRetryCount > AchievementCountDataDeifne.LIMIT_RETRY_COUNT ) totalRetryCount = AchievementCountDataDeifne.LIMIT_RETRY_COUNT;

            if( SteamBridge.Instance.GetStats( m_SteamAchivementData.RetryCountDataApiName, out int currentCount ) )
            {
                // 現在値と異なる場合はフレーム後に更新を実行する
                if( currentCount != totalRetryCount )
                {
                    // リトライ回数の進行状況を更新
                    SteamBridge.Instance.SetStats( m_SteamAchivementData.RetryCountDataApiName, ( int )totalRetryCount );
                    IsAchived = true;
                }
            }
        }
#endif

        /// <summary>
        /// 実績カウントのセーブデータ領域への設定
        /// </summary>
        /// <remarks>書き込み処理はここでは行わない</remarks>
        public static bool SetCountData()
        {
            if( m_Instance == null ) return false;

            if( !SaveDataManager.Instance.IsNetworkMode )
            {
                bool isSave = false;

                if( m_Instance.m_DrawLength > 0 )
                {
                    SaveDataManager.Instance.SetDrawLineLength( SaveDataManager.Instance.GetDrawLineLength() + m_Instance.m_DrawLength );
                    isSave = true;
                }

                if( m_Instance.m_RetryCount > 0 )
                {
                    SaveDataManager.Instance.SetRetryCount( SaveDataManager.Instance.GetRetryCount() + m_Instance.m_RetryCount );
                    isSave = true;
                }

                if( !isSave ) return false;
            }

            m_Instance.m_DrawLength = 0;
            m_Instance.m_RetryCount = 0;

            return true;
        }

        /// <summary>
        /// 実績カウントの保存
        /// </summary>
        public static void SaveCountData()
        {
            if( SaveDataManager.Instance.IsNetworkMode ) return;
            if( !SetCountData() ) return;

            SaveDataManager.Instance.SaveAchievementCount();
        }

#if STEAMWORKS_NET
        /// <summary>
        /// 指定のAPI名の実績が解除されていなければ解除する
        /// </summary>
        /// <param name="apiName">実績のAPI名</param>
        /// <param name="isStoreStats">実績を解除した際に実績データをサーバーへ送信するかのフラグ</param>
        /// <returns></returns>
        private void SetAchivement( string apiName )
        {
            //Debug.Log( "SetAchivement( " + apiName + " )" );

            if( !IsEnableAchivementProcess ) return;
            if( !SteamBridge.Instance.GetAchievement( apiName, out bool achieved ) ) return;
            if( achieved ) return;

            Debug.Log( "Unlock Achievement! API Name : " + apiName );

            SteamBridge.Instance.SetAchievement( apiName );

            IsAchived = true;
        }

        private static void StoreStats()
        {
            SteamBridge.Instance.StoreStats();
        }
#endif

        /// <summary>
        /// 指定のAPI名の実績の解除状態をリセット
        /// </summary>
        /// <param name="apiName">実績のAPI名</param>
        /// <param name="isStoreStats">実績データをサーバーへ送信するかのフラグ</param>
        public static void ClearAchivement( string apiName )
        {
#if STEAMWORKS_NET && ( DEVELOPMENT_BUILD || UNITY_EDITOR )
            if( !IsEnableAchivementProcess ) return;

            Debug.Log( "ClearAchivement( " + apiName + " )" );

            SteamBridge.Instance.ClearAchievement( apiName );

            IsAchived = true;
#endif
        }

        /// <summary>
        /// 起動後に既に実績解放条件を満たしているものについて解放する
        /// </summary>
        public static void CheckUnlockAchievementToTitle()
        {
#if STEAMWORKS_NET
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            CreateInstance();

            // No.2 初めて問題を解いた
            if( SaveDataManager.Instance.GetStageCondition( m_Instance.m_SteamAchivementData.FirstClearedStage.TargetParam ) >= StageCondition.Cleared )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstClearedStage.ApiName );
            }

            // No.4 チュートリアルをクリアした
            if( SaveDataManager.Instance.GetStageCondition( m_Instance.m_SteamAchivementData.CrearedTutorialStage.TargetParam ) >= StageCondition.Cleared )
            {
                m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.CrearedTutorialStage.ApiName );
            }

            // No.8~25 キャラクター取得
            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.UnlockCharacterAchivementList )
            {
                if( SaveDataManager.Instance.IsUnlockContentsToCharacter( targetParamAchievement.TargetParam ) ) m_Instance.SetAchivement( targetParamAchievement.ApiName );
            }

            int clearedStageCount = SaveDataManager.Instance.GetClearedStoryModeStageCount();

            // No26~37 問題を指定数以上クリア
            m_Instance.SetClearedStageCount();
            // No.38 表面のすべての問題をクリアした
            m_Instance.CheckAchievementAllCleardSurfaceStage();
            // No.39 裏面のすべての問題をクリアした
            m_Instance.CheckAchievementAllCleardReverseStage();

            int gettedMedalCount = SaveDataManager.Instance.GetUnlockCountsToStoryModeMedal();

            // No.3 初めてメダルを取得した
            if( gettedMedalCount >= m_Instance.m_SteamAchivementData.FirstGetMedal.TargetParam ) m_Instance.SetAchivement( m_Instance.m_SteamAchivementData.FirstGetMedal.ApiName );

            // No.40~51 メダルを指定枚数以上取得
            CheckAchievementGetMedal();

            // No.52 表面のすべてのメダルを取得
            m_Instance.CheckAchievementAllUnlockMedalToSurfaceStage();
            // No.53 裏面のすべてのメダルを取得
            m_Instance.CheckAchievementAllUnlockMedalToReverseStage();
            // No.54 すべての問題をクリアし、すべてのメダルを取得
            m_Instance.CheckAchievementAllStageCleredAndAllUnlockMedal();

            uint drawLength = SaveDataManager.Instance.GetDrawLineLength();

            // No.55~64 描いた長さが指定m以上
            m_Instance.CheckAchievementDrawLength();

            uint retryCount = SaveDataManager.Instance.GetRetryCount();

            // No.66~67 リトライ回数が指定数以上
            m_Instance.SetRetryCount();
#endif
        }

        /// <summary>
        ///  No.68 すべての実績を解放 判定
        /// </summary>
        private void CheckCompliteAchievement()
        {
#if STEAMWORKS_NET
            if( !IsReadyAchivement ) return;
            if( SaveDataManager.Instance.IsNetworkMode ) return;

            bool isAchieved = false;

            if( SteamBridge.Instance.GetAchievement( m_SteamAchivementData.CompliteAchivementApiName, out isAchieved ) && isAchieved ) return;

            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.FirstBootApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.FirstClearedStage.ApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.FirstGetMedal.ApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.CrearedTutorialStage.ApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.FirstUnlockkNewArea.ApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.UnlockRevaerceSide.ApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.MultiPlayApiName, out isAchieved ) || !isAchieved ) return;

            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.UnlockCharacterAchivementList )
            {
                if( !SteamBridge.Instance.GetAchievement( targetParamAchievement.ApiName, out isAchieved ) || !isAchieved ) return;
            }

            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.ClearedStageCountAchivementList )
            {
                if( !SteamBridge.Instance.GetAchievement( targetParamAchievement.ApiName, out isAchieved ) || !isAchieved ) return;
            }

            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.AllCleardSurfaceStageApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.AllCleardReverseStageApiName, out isAchieved ) || !isAchieved ) return;

            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.GetMedalCountAchivementList )
            {
                if( !SteamBridge.Instance.GetAchievement( targetParamAchievement.ApiName, out isAchieved ) || !isAchieved ) return;
            }

            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.AllGetMedalToSurfaceStageApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.AllGetMedalToReverseStageApiName, out isAchieved ) || !isAchieved ) return;
            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.AllStageCleredAndAllGetkMedalApiName, out isAchieved ) || !isAchieved ) return;

            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.DrawLengthAchivementList )
            {
                if( !SteamBridge.Instance.GetAchievement( targetParamAchievement.ApiName, out isAchieved ) || !isAchieved ) return;
            }

            if( !SteamBridge.Instance.GetAchievement( m_SteamAchivementData.DrawLength1000mToOneStage.ApiName, out isAchieved ) || !isAchieved ) return;

            foreach( TargetParamAchievement targetParamAchievement in m_Instance.m_SteamAchivementData.RetryCountAchivementList )
            {
                if( !SteamBridge.Instance.GetAchievement( targetParamAchievement.ApiName, out isAchieved ) || !isAchieved ) return;
            }

            // No.68 すべての実績を解放した
            SetAchivement( m_SteamAchivementData.CompliteAchivementApiName );
#endif
        }
    }
}