using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

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
                    m_Instance = FindObjectOfType<AddressableResourcesManager>();

                    if( !!IsEnableInstance )
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

        private void Awake()
        {
            DontDestroyOnLoad( this );
        }

        private void OnApplicationQuit()
        {
            if( m_AddressableStorageList != null )
            {
                foreach( AddressableStorage storage in m_AddressableStorageList )
                {
                    storage.ReleaseStorage();
                }
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

#if UNITY_EDITOR
            IList<AddressableInfo> list = Addressables.LoadAssetsAsync<AddressableInfo>( AddressableDefine.ADRESSABLES_INFO_NAME, null ).WaitForCompletion();

            foreach( AddressableInfo info in list )
            {
                m_Instance.m_AddressableInfoList.Add( info );
            }

            m_Instance.m_AddressableInfoList.Sort( ( a, b ) => a.Category - b.Category );

            m_Instance.IsLoadComplitedInfoList = true;

            m_Instance.LoadAsset( MenuIdentifierDefine.MENU_CANVAS );
            m_Instance.LoadAsset( NewCharacterPerformanceIdentifierDefine.NEW_CHARACER_PERFORMANCE );
            m_Instance.LoadAsset( ButtonGuideIdentifierDefine.BUTTON_GUIDE_MANAGER );
            m_Instance.LoadAsset( LoadingViewIdentifierDefine.LOADING_VIEW );
#endif
        }

        /// <summary>
        /// AddressableAsset定義ファイルの読み込み開始
        /// </summary>
        public async void LoadAddressableDefineData()
        {
            if( IsLoadComplitedInfoList ) return;

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
        /// <param name="_storage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, int _index, out AddressableStorage _storage )
        {
            return LoadAsset( _category, ( uint )_index, out _storage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">インデックス</param>
        /// <param name="_storage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( AddressableCategory _category, uint _index, out AddressableStorage _storage )
        {
            _storage = null;
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Bad Identifier  Category : " + _category + " Index : " + _index );
                return AddressableLoadResult.Error_BadIdentifier;
            }

            return LoadAsset( identifier.Identifier, out _storage );
        }

        /// <summary>
        /// AddressableAssetの読み込み
        /// </summary>
        /// <remarks>既に読み込み済みの<see cref="AddressableStorage"/>を取得する場合もこのメソッドを実行することで取得できる</remarks>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_storage"><see cref="AddressableStorage"/></param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadAsset( uint _identifier, out AddressableStorage _storage )
        {
            _storage = null;

            if( m_AddressableStorageList.Count > 0 )
            {
                _storage = m_AddressableStorageList.Find( ( storage ) => storage.Identifier == _identifier );

                if( _storage != null )
                {
                    if( _storage.State == AddressableStorage.AssetState.Loading )
                    {
                        return AddressableLoadResult.NowLoading;
                    }
                    else if( _storage.State == AddressableStorage.AssetState.Enable )
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

            AddressableIdentifier identifier = new AddressableIdentifier( _identifier );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! Bad Identifier  : " + _identifier );
                return AddressableLoadResult.Error_BadIdentifier;
            }

            AddressableInfo info = m_AddressableInfoList.Find( ( define ) => define.Category == identifier.Category );

            if( info == null )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! MissingAddressableInfo" );
                return AddressableLoadResult.Error_MissingAddressableInfo;
            }

            if( info.AdressList.Length <= identifier.Index )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! MissingAssetData" );
                return AddressableLoadResult.Error_MissingAssetData;
            }

            if( info.AdressList[ identifier.Index ].IsSceneAsset )
            {
                Debug.LogError( " AddressableLoadResult LoadAsset Error! SceneAsset" );
                return AddressableLoadResult.Error_SceneAsset;
            }

            AsyncOperationHandle handle = Addressables.LoadAssetAsync<Object>( info.AdressList[ identifier.Index ].Address );

            _storage = new AddressableStorage( _identifier, handle );
            m_AddressableStorageList.Add( _storage );

            LoadAssetAsync( _storage );

            return AddressableLoadResult.StartLoading;
        }

        private async void LoadAssetAsync( AddressableStorage _storage )
        {
            await _storage.m_Handle.Task;

            _storage.CheckLoadResult();
        }

        /// <summary>
        /// AddressableAssetでのシーンの読み込み
        /// </summary>
        ///<param name="_category">カテゴリ</param>
        ///<param name="_index">インデックス</param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadScene( AddressableCategory _category, uint _index )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success ) return AddressableLoadResult.Error;

            return LoadScene( identifier.Identifier );
        }

        /// <summary>
        /// AddressableAssetでのシーンの読み込み
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public AddressableLoadResult LoadScene( uint _identifier )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _identifier );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadScene Error! BadIdentifier" );
                return AddressableLoadResult.Error_BadIdentifier;
            }

            AddressableInfo info = m_AddressableInfoList.Find( ( define ) => define.Category == identifier.Category );

            if( info == null )
            {
                Debug.LogError( " AddressableLoadResult LoadScene Error! MissingAddressableInfo" );
                return AddressableLoadResult.Error_MissingAddressableInfo;
            }

            if( info.AdressList.Length <= identifier.Index )
            {
                Debug.LogError( " AddressableLoadResult LoadScene Error! MissingAssetData" );
                return AddressableLoadResult.Error_MissingAssetData;
            }

            if( !info.AdressList[ identifier.Index ].IsSceneAsset )
            {
                Debug.LogError( " AddressableLoadResult LoadScene Error! Not SceneAsset" );
                return AddressableLoadResult.Error_NotSceneAsset;
            }

            Addressables.LoadSceneAsync( info.AdressList[ identifier.Index ].Address );

            return AddressableLoadResult.StartLoading;
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
        public AddressableSerachResult GetAddressableIdentifier( AddressableCategory _category, string _address, out AddressableIdentifier _identifier )
        {
            _identifier = null;

            if( m_AddressableInfoList.Count <= 0 ) return AddressableSerachResult.Nothing_InfoList;

            AddressableInfo info = m_AddressableInfoList.Find( ( defineInfo ) => defineInfo.Category == _category );

            if( info == null ) return AddressableSerachResult.NotFind_Info;

            int index = System.Array.FindIndex( info.AdressList, ( ad ) => ad.Address == _address );

            if( index < 0 ) return AddressableSerachResult.NotFind_AssetData;

            _identifier = new AddressableIdentifier( _category, index );

            return _identifier.ResultState == AddressableIdentifier.ResultCode.Success ? AddressableSerachResult.Success : AddressableSerachResult.TypeError;
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

            storage.ReleaseHandle();
        }

        /// <summary>
        /// 指定のカテゴリの<see cref="AddressableStorage"/>を全て開放する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_exclusionIdentifierList">除外Asset識別子リスト</param>
        public void ReleaseStorageToTargetCategory( AddressableCategory _category, params uint[] _exclusionIdentifierList )
        {
            List<AddressableStorage> releaseTargetList = m_AddressableStorageList.FindAll( ( storage ) => AddressableIdentifier.ConvertCategoty( storage.Identifier ) == _category );

            if( releaseTargetList == null || releaseTargetList.Count == 0 ) return;

            foreach( AddressableStorage storage in releaseTargetList )
            {
                if( _exclusionIdentifierList != null && _exclusionIdentifierList.Length > 0 )
                {
                    if( System.Array.FindIndex( _exclusionIdentifierList, ( identifier ) => identifier == storage.Identifier ) >= 0 ) continue;
                }

                ReleaseStorage( storage.Identifier );
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
                result.Add( AddressableIdentifier.CreateIdentifier( _category, i ) );
            }

            return result;
        }
    }
}