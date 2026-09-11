using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using CodeIcf.LanguageResoucrsManagement;
#endif

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// リソース管理クラス
    /// </summary>
    public class AddressableResourcesManager : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static AddressableResourcesManager m_Instance = null;

        /// <summary>インスタンスが確保されているか</summary>
        public static bool IsEnableInstance => m_Instance != null;

        /// <summary>インスタンス</summary>
        public static AddressableResourcesManager Instance
        {
            get
            {
                if( !IsEnableInstance )
                {
                    m_Instance = FindAnyObjectByType<AddressableResourcesManager>();

                    if( !IsEnableInstance )
                    {
                        CreateInstance();
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>AddressableAsset定義データ一覧</summary>
        [SerializeField]
        private List<AddressableInfo> m_AddressableInfoList = new List<AddressableInfo>();
        /// <summary>>AddressableAsset定義データの読み込みが終わったかのフラグ</summary>
        public bool IsLoadComplitedInfoList { get; private set; } = false;
        /// <summary><see cref="AddressableStorage"/>一覧</summary>
        [SerializeField]
        private List<AddressableStorage> m_AddressableStorageList = new List<AddressableStorage>();

        private void OnApplicationQuit()
        {
            if( m_AddressableStorageList != null && m_AddressableStorageList.Count > 0 )
            {
                foreach( AddressableStorage storage in m_AddressableStorageList )
                {
                    storage.ReleaseStorage();
                }

                m_AddressableStorageList.Clear();
            }
        }

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        public static void CreateInstance()
        {
            if( IsEnableInstance ) return;

            GameObject obj = new GameObject( "AdressableResourcesManager" );
            obj.transform.position = Vector3.zero;
            obj.transform.eulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            m_Instance = obj.AddComponent<AddressableResourcesManager>();
            DontDestroyOnLoad( m_Instance );

            AddressableSceneLoader.CreateInstance();

#if UNITY_EDITOR
            IList<AddressableInfo> list = Addressables.LoadAssetsAsync<AddressableInfo>( AddressableDefine.ADRESSABLES_INFO_NAME, null ).WaitForCompletion();

            foreach( AddressableInfo info in list )
            {
                m_Instance.m_AddressableInfoList.Add( info );
            }

            m_Instance.m_AddressableInfoList.Sort( ( a, b ) => a.Category - b.Category );

            m_Instance.IsLoadComplitedInfoList = true;

            m_Instance.LoadAsset( MenuIdentifierDefine.MENU_CANVAS );
            m_Instance.LoadAsset( LoadingViewIdentifierDefine.LOADING_VIEW );
            m_Instance.LoadAsset( AudioManagementIdentifierDefine.AUDIO_MANAGER );
#endif
        }

        /// <summary>
        /// <see cref="AddressableInfo"/>の取得
        /// </summary>
        /// <param name="_category">AddressableAssetの種類</param>        
        /// <returns>AddressableAssetの種類に該当する<see cref="Addressablei"/></returns>
        public AddressableInfo GetAddressableInfo( AddressableCategory _category )
        {
            if( m_AddressableInfoList == null || m_AddressableInfoList.Count <= 0 ) return null;

            return m_AddressableInfoList.Find( ( define ) => define.Category == _category );
        }

        /// <summary>
        /// AddressableAsset定義ファイルの読み込み開始
        /// </summary>
        public async void LoadAddressableDefineData()
        {
            if( IsLoadComplitedInfoList ) return;

            float loadStartTIme = Time.unscaledTime;

            AsyncOperationHandle<IList<AddressableInfo>> handle = Addressables.LoadAssetsAsync<AddressableInfo>( AddressableDefine.ADRESSABLES_INFO_NAME, null );
            await handle.Task;

            if( handle.Status == AsyncOperationStatus.Succeeded )
            {
                foreach( AddressableInfo info in handle.Result )
                {
                    m_AddressableInfoList.Add( info );
                }

                m_AddressableInfoList.Sort( ( a, b ) => a.Category - b.Category );

                IsLoadComplitedInfoList = true;

                Debug.Log( "ltdl LoadAddressableDefineData Success! LoadTime=" + ( Time.unscaledTime - loadStartTIme ) );
            }
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>        
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, int _index )
        {
            return LoadAsset( _category, _index, out AddressableStorage storage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>        
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, uint _index )
        {
            return LoadAsset( _category, _index, out AddressableStorage storage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <remarks>既に読み込み済みの<see cref="AddressableStorage"/>を取得する場合もこのメソッドを実行することで取得できる</remarks>
        /// <param name="_identifier">Asset識別子</param>        
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( uint _identifier )
        {
            return LoadAsset( _identifier, out AddressableStorage storage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        /// <param name="_outStorage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, int _index, out AddressableStorage _outStorage )
        {
            return LoadAsset( _category, ( uint )_index, out _outStorage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        /// <param name="_outStorage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, uint _index, out AddressableStorage _outStorage )
        {
            _outStorage = null;
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Bad Identifier  Category : " + _category + " Index : " + _index );
                return AddressableLoadResult.Error_BadIdentifier;
            }

            return LoadAsset( identifier.Identifier, out _outStorage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <remarks>既に読み込み済みの<see cref="AddressableStorage"/>を取得する場合もこのメソッドを実行することで取得できる</remarks>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_outStorage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( uint _identifier, out AddressableStorage _outStorage )
        {
            _outStorage = null;

            if( m_AddressableStorageList.Count > 0 )
            {
                _outStorage = m_AddressableStorageList.Find( ( storage ) => storage.Identifier == _identifier );

                if( _outStorage != null )
                {
                    if( _outStorage.State == AddressableStorage.AssetState.Loading )
                    {
                        return AddressableLoadResult.NowLoading;
                    }
                    else if( _outStorage.State == AddressableStorage.AssetState.Enable )
                    {
                        return AddressableLoadResult.Complited;
                    }
                    else
                    {
                        Debug.LogError( " AddressableLoadResult LoadAsset Error" );
                        return AddressableLoadResult.Error;
                    }
                }
            }

            AddressableIdentifier addressableIdentifier = new AddressableIdentifier( _identifier );

            if( addressableIdentifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Bad Identifier  : " + _identifier );
                return AddressableLoadResult.Error_BadIdentifier;
            }

            AddressableInfo info = GetAddressableInfo( addressableIdentifier.Category );

            if( info == null )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! MissingAddressableInfo  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_MissingAddressableInfo;
            }

            if( info.AdressList.Length <= addressableIdentifier.Index )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! MissingAssetData  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_MissingAssetData;
            }

            if( info.AdressList[ addressableIdentifier.Index ].IsSceneAsset )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! SceneAsset  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_SceneAsset;
            }

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            // Developのフラグが立っていて、DevelopmentBuildではない場合は対象にしない
            if( info.AdressList[ addressableIdentifier.Index ].UseContidion.HasFlag( UseContidionType.DevelopmentOnly ) )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available DevelopmentBuild Only!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailableCondition;
            }
#endif

#if UNITY_SWITCH
            if( !info.AdressList[ addressableIdentifier.Index ].UsePlatform.HasFlag( UsePlatformType.Switch ) )
            {//Switch版では読み込めないAssetを読み込もうとした
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available Switch Platform!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailablePlatform;
            }
#if !SWITCH_LIICA_PHYSICAL_EDITION
            if( info.AdressList[ addressableIdentifier.Index ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly ) )
            {//Switch 物理エディション版では読み込めないAssetを読み込もうとした
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available Swich PhysicalEdition Only!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailableCondition;
            }
#endif // SWITCH_LIICA_PHYSICAL_EDITION

#if SWITCH_AKSYSGAMES_PUBLISH
            if( info.AdressList[ addressableIdentifier.Index ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_NotIncluded ) )
            {//Switch Aksys Games販売版では読み込めないAssetを読み込もうとした
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available Swich PhysicalEdition Only!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailableCondition;
            }
#else // SWITCH_AKSYSGAMES_PUBLISH
            if( info.AdressList[ addressableIdentifier.Index ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_Only ) )
            {//Switch Aksys Games販売版以外では読み込めないAssetを読み込もうとした
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available Swich PhysicalEdition Only!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailableCondition;
            }
#endif // SWITCH_AKSYSGAMES_PUBLISH

#else // UNITY_SWITCH
            if( !info.AdressList[ addressableIdentifier.Index ].UsePlatform.HasFlag( UsePlatformType.PC ) )
            {//PC版では読み込めないAssetを読み込もうとした
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Not Available PC Platform!  Identifier  : " + _identifier );
                return AddressableLoadResult.Error_NotAvailablePlatform;
            }
#endif // UNITY_SWITCH

            _outStorage = new AddressableStorage( _identifier, info.AdressList[ addressableIdentifier.Index ].Address );
            m_AddressableStorageList.Add( _outStorage );

#if UNITY_EDITOR
            m_AddressableStorageList.Sort( ( a, b ) => ( int )( a.Identifier - b.Identifier ) );
#endif
            return AddressableLoadResult.StartLoading;
        }

        /// <summary>
        /// 指定のカテゴリのAssetを全て読み込む
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_outResults">読み込み処理の結果一覧</param>
        /// <param name="_outStorages"><see cref="AddressableStorage"/>一覧</param>
        /// <returns>読み込み処理結果一覧とAddressableAtorage一覧の素数 0未満の場合は定義データが存在しない</returns>
        public int LoadAssetAllInCategory( AddressableCategory _category, out AddressableLoadResult[] _outResults, out AddressableStorage[] _outStorages )
        {
            _outResults = null;
            _outStorages = null;

            AddressableInfo info = GetAddressableInfo( _category );

            if( info == null ) return -1;

            _outResults = new AddressableLoadResult[ info.AdressList.Length ];
            _outStorages = new AddressableStorage[ info.AdressList.Length ];

            for( int i = 0; i < info.AdressList.Length; i++ )
            {


#if UNITY_SWITCH
                if( !info.AdressList[ i ].UsePlatform.HasFlag( UsePlatformType.Switch ) ) continue;
#if !SWITCH_LIICA_PHYSICAL_EDITION
                // ゲームカードのみのリソースは読み込まない
                if( info.AdressList[ i ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly ) ) continue;
#endif // !SWITCH_LIICA_PHYSICAL_EDITION

#if SWITCH_AKSYSGAMES_PUBLISH
                // Switch Aksys Games販売版では読み込まない
                if( info.AdressList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_NotIncluded ) ) continue;
#else // SWITCH_AKSYSGAMES_PUBLISH
                // Switch Aksys Games販売版以外では読み込まない    
                if( info.AdressList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_Only ) ) continue;
#endif // SWITCH_AKSYSGAMES_PUBLISH

#else // UNITY_SWITCH
                if( !info.AdressList[ i ].UsePlatform.HasFlag( UsePlatformType.PC ) ) continue;
#endif // UNITY_SWITCH

                _outResults[ i ] = LoadAsset( _category, i, out _outStorages[ i ] );
            }

            return info.AdressList.Length;
        }

        /// <summary>
        /// Asset識別子を複数指定してのAddressableAssetnの読み込み
        /// </summary>
        /// <param name="_identiferList">Asset識別子一覧</param>
        /// <returns></returns>
        public int LoadAssets( uint[] _identiferList )
        {
            return LoadAssets( _identiferList, out _, out _ );
        }

        /// <summary>
        /// Asset識別子を複数指定してのAddressableAssetnの読み込み
        /// </summary>
        /// <param name="_identiferList">Asset識別子一覧</param>
        /// <param name="_outResults">Asset識別子毎の読込結果</param>
        /// <returns></returns>
        public int LoadAssets( uint[] _identiferList, out AddressableLoadResult[] _outResults )
        {
            _outResults = null;

            return LoadAssets( _identiferList, out _outResults, out _ );
        }

        /// <summary>
        /// Asset識別子を複数指定してのAddressableAssetnの読み込み
        /// </summary>
        /// <param name="_identiferList">Asset識別子一覧</param>
        /// <param name="_outResults">Asset識別子毎の読込結果</param>
        /// <param name="_outStorages">Asset識別子毎の<see cref="AddressableStorage"/></param>
        /// <returns></returns>
        public int LoadAssets( uint[] _identiferList, out AddressableLoadResult[] _outResults, out AddressableStorage[] _outStorages )
        {
            _outResults = new AddressableLoadResult[ _identiferList.Length ];
            _outStorages = new AddressableStorage[ _identiferList.Length ];

            for( int i = 0; i < _identiferList.Length; i++ )
            {
                _outResults[ i ] = LoadAsset( _identiferList[ i ], out _outStorages[ i ] );
            }

            return _identiferList.Length;
        }

        /// <summary>
        /// <see cref="AddressableStorage"/>の取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        /// <returns>見つからない場合はnull</returns>
        public AddressableStorage GetStorage( AddressableCategory _category, int _index )
        {
            return GetStorage( _category, ( uint )_index );
        }

        /// <summary>
        /// <see cref="AddressableStorage"/>の取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        /// <returns>見つからない場合はnull</returns>
        public AddressableStorage GetStorage( AddressableCategory _category, uint _index )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState == AddressableIdentifier.ResultCode.Success ) return GetStorage( identifier.Identifier );

            return null;
        }

        /// <summary>
        /// <see cref="AddressableStorage"/>の取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>見つからない場合はnull</returns>
        public AddressableStorage GetStorage( uint _identifier )
        {
            if( m_AddressableStorageList.Count <= 0 ) return null;

            return m_AddressableStorageList.Find( ( storage ) => storage.Identifier == _identifier );
        }

        /// <summary>
        /// 指定のカテゴリの<see cref="AddressableStorage"/>を全て取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns></returns>
        public List<AddressableStorage> GetStoragesToCategory( AddressableCategory _category )
        {
            if( m_AddressableStorageList.Count <= 0 ) return null;

            return m_AddressableStorageList.FindAll( ( storage ) => AddressableIdentifier.ConvertCategoty( storage.Identifier ) == _category );
        }

        /// <summary>
        /// 指定のカテゴリとアドレスから、<see cref="AddressableIdentifier"/>を取得する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_address">アドレス</param>
        /// <returns><see cref="AddressableSerachResult"/>を参照</returns>
        public AddressableSerachResult GetAddressableIdentifier( AddressableCategory _category, string _address, out AddressableIdentifier _outValue )
        {
            _outValue = null;

            if( m_AddressableInfoList.Count <= 0 ) return AddressableSerachResult.Nothing_InfoList;

            AddressableInfo info = GetAddressableInfo( _category );

            if( info == null ) return AddressableSerachResult.NotFind_Info;

            int index = System.Array.FindIndex( info.AdressList, ( ad ) => ad.Address == _address );

            if( index < 0 ) return AddressableSerachResult.NotFind_AssetData;

            _outValue = new AddressableIdentifier( _category, index );

            return _outValue.ResultState == AddressableIdentifier.ResultCode.Success ? AddressableSerachResult.Success : AddressableSerachResult.TypeError;
        }

        /// <summary>
        /// 指定の<see cref="AddressableStorage"/>を解放
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        public void ReleaseStorage( AddressableCategory _category, int _index )
        {
            ReleaseStorage( _category, ( uint )_index );
        }

        /// <summary>
        /// 指定の<see cref="AddressableStorage"/>を解放
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        public void ReleaseStorage( AddressableCategory _category, uint _index )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState == AddressableIdentifier.ResultCode.Success ) ReleaseStorage( identifier.Identifier );
        }

        /// <summary>
        /// 指定の<see cref="AddressableStorage"/>を解放
        /// </summary>
        /// <param name="_Identifier">Asset識別子</param>
        public void ReleaseStorage( uint _Identifier )
        {
            AddressableStorage storage = GetStorage( _Identifier );

            if( storage == null ) return;

            m_AddressableStorageList.Remove( storage );

#if UNITY_EDITOR
            m_AddressableStorageList.Sort( ( a, b ) => ( int )( a.Identifier - b.Identifier ) );
#endif

            storage.ReleaseHandle();
        }

        /// <summary>
        ///  指定の<see cref="AddressableStorage"/>を解放
        /// </summary>
        /// <param name="_identiferList">開放するAsset識別子一覧</param>
        public void ReleaseStorages( uint[] _identiferList )
        {
            foreach( uint identifer in _identiferList )
            {
                ReleaseStorage( identifer );
            }
        }

        /// <summary>
        /// 指定のカテゴリの<see cref="AddressableStorage"/>を全て開放する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_exclusionIdentifierList">除外Asset識別子リスト</param>
        public static void ReleaseStorageToTargetCategory( AddressableCategory _category, params uint[] _exclusionIdentifierList )
        {
            if( m_Instance == null ) return;

            List<AddressableStorage> releaseTargetList = m_Instance.m_AddressableStorageList.FindAll( ( storage ) => AddressableIdentifier.ConvertCategoty( storage.Identifier ) == _category );

            if( releaseTargetList == null || releaseTargetList.Count == 0 ) return;

            foreach( AddressableStorage storage in releaseTargetList )
            {
                if( _exclusionIdentifierList != null && _exclusionIdentifierList.Length > 0 )
                {
                    if( System.Array.FindIndex( _exclusionIdentifierList, ( identifier ) => identifier == storage.Identifier ) >= 0 ) continue;
                }

                m_Instance.ReleaseStorage( storage.Identifier );
            }
        }

        /// <summary>
        /// 複数指定カテゴリの<see cref="AddressableStorage"/>を全て開放する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        public static void ReleaseStorageToTargetCategorys( params AddressableCategory[] _categorys )
        {
            if( m_Instance == null ) return;

            foreach( AddressableCategory category in _categorys )
            {
                List<AddressableStorage> releaseTargetList = m_Instance.m_AddressableStorageList.FindAll( ( storage ) => AddressableIdentifier.ConvertCategoty( storage.Identifier ) == category );

                foreach( AddressableStorage storage in releaseTargetList )
                {
                    m_Instance.ReleaseStorage( storage.Identifier );
                }
            }
        }

        /// <summary>
        /// 指定のカテゴリの全てのAsset識別子を取得
        /// </summary>
        /// <param name="_category"></param>
        /// <returns></returns>
        public List<uint> GetAllIdentiferForCategory( AddressableCategory _category )
        {
            AddressableInfo target = null;

            foreach( AddressableInfo info in m_AddressableInfoList )
            {
                if( info.Category != _category ) continue;

                target = info;
                break;
            }

            if( target == null ) return null;

            List<uint> result = new List<uint>();

            for( int i = 0; i < target.AdressList.Length; i++ )
            {
#if UNITY_SWITCH
                if( !target.AdressList[ i ].UsePlatform.HasFlag( UsePlatformType.Switch ) ) continue;
#if !SWITCH_LIICA_PHYSICAL_EDITION
                if( target.AdressList[ i ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly ) ) continue;
#endif // !SWITCH_LIICA_PHYSICAL_EDITION
#else // UNITY_SWITCH
                if( !target.AdressList[ i ].UsePlatform.HasFlag( UsePlatformType.PC ) ) continue;
#endif // UNITY_SWITCH
                result.Add( AddressableIdentifier.CreateIdentifier( _category, i ) );
            }

            return result;
        }

        /// <summary>
        /// Assetの読み込み中の<see cref="AddressableStorage"/>があるかの判定
        /// </summary>
        /// <returns></returns>
        public bool IsLoadingAsset()
        {
            foreach( AddressableStorage storage in m_AddressableStorageList )
            {
                if( storage.State == AddressableStorage.AssetState.Loading ) return true;
            }

            return false;
        }

        /// <summary>
        /// 対象のAssetが現在のプラットフォームで使用可能かを判定
        /// </summary>
        /// <param name="_identifer"></param>
        /// <returns></returns>
        public bool IsEnablePlatform( uint _identifer )
        {
            AddressableIdentifier.DivideIdentifier( _identifer, out AddressableCategory category, out uint index );

            return IsEnablePlatform( category, ( int )index );
        }

        /// <summary>
        /// 対象のAssetが現在のプラットフォームで使用可能かを判定
        /// </summary>
        /// <param name="_category"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public bool IsEnablePlatform( AddressableCategory _category, int _index )
        {
            AddressableInfo info = GetAddressableInfo( _category );

            if( info == null ) return false;
            if( _index < 0 || _index >= info.AdressList.Length ) return false;

            bool result = false;

#if UNITY_SWITCH
            bool isPhysicalEditionOnly = info.AdressList[ _index ].UsePlatform.HasFlag( UsePlatformType.Switch ) && info.AdressList[ _index ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly );
            bool isAksysGamesOnly = info.AdressList[ _index ].UsePlatform.HasFlag( UsePlatformType.Switch ) && info.AdressList[ _index ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_Only );
            bool IsAksysGameNotInclude = info.AdressList[ _index ].UsePlatform.HasFlag( UsePlatformType.Switch ) && info.AdressList[ _index ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_NotIncluded );

#if SWITCH_LIICA_PHYSICAL_EDITION
            // Aksys Games販売版のAsset以外
            result = !isAksysGamesOnly;
#elif SWITCH_AKSYSGAMES_PUBLISH // SWITCH_LIICA_PHYSICAL_EDITION == false
            // 物理エディションのみ以外、かつAksys Games販売版には含まないAsset以外
            result = !isPhysicalEditionOnly && !IsAksysGameNotInclude;
#else // SWITCH_LIICA_PHYSICAL_EDITION == false  SWITCH_AKSYSGAMES_PUBLISH == false
            // 物理エディションのみ以外、かつAksys Games販売版のみ以外
            result = !isPhysicalEditionOnly && !isAksysGamesOnly;
#endif // SWITCH_LIICA_PHYSICAL_EDITION SWITCH_AKSYSGAMES_PUBLISH

#else // UNITY_SWITCH
            result =  info.AdressList[ _index ].UsePlatform.HasFlag( UsePlatformType.PC );
#endif // UNITY_SWITCH

            return result;
        }
    }
}