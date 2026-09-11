#if UNITY_EDITOR
//エディタ用　Assetの読み込みをファイルから直接行う 
#define EDITOR_LOAD_ASSET_PATH
#endif

// Nintendo Switch 実行時のみ
#if UNITY_SWITCH && !UNITY_EDITOR
#define UNITY_ONLY_SWITCH
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

#if !EDITOR_LOAD_ASSET_PATH && UNITY_SWITCH
using nn.aoc;
using nn.fs;
using File = nn.fs.File;
using Result = nn.Result;
#endif 

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// リソース管理クラス
    /// </summary>
    public class AssetBundleResourcesManager : MonoBehaviour
    {
        /// <summary>Unityエディタ用　Assetの読み込みをファイルから直接行うかのフラグ</summary>
#if EDITOR_LOAD_ASSET_PATH
        public const bool IS_EDITOR_LOAD_ASSET_PATH = true;
#else
        public const bool IS_EDITOR_LOAD_ASSET_PATH = false;
#endif

        /// <summary>インスタンス</summary>
        private static AssetBundleResourcesManager m_Instance = null;

        /// <summary>インスタンス</summary>
        public static AssetBundleResourcesManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    m_Instance = FindAnyObjectByType<AssetBundleResourcesManager>();

                    if( m_Instance == null )
                    {
                        GameObject obj = new GameObject( "AssetBundleResourcesManager" );
                        obj.transform.position = Vector3.zero;
                        obj.transform.eulerAngles = Vector3.zero;
                        obj.transform.localScale = Vector3.one;

                        m_Instance = obj.AddComponent<AssetBundleResourcesManager>();
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>AssetBundle定義データ一覧</summary>
        [SerializeField]
        private List<AssetBundleInfo> m_AssetBundleDefineList = new List<AssetBundleInfo>();
        /// <summary>AssetBundle定義データの読み込みが終わったかのフラグ</summary>
        public bool IsLoadedDefineData { get; private set; } = false;
        /// <summary> <see cref="AssetBundleStorage"/>一覧 </summary>
        [SerializeField]
        private List<AssetBundleStorage> m_AssetBundleStorageList = new List<AssetBundleStorage>();

#if EDITOR_LOAD_ASSET_PATH
        /// <summary>AssetBundleを読み込んでいるふりをしている数</summary>
        private int m_AssetBundlePseudoLoadCount = 0;
#endif

        private void Awake()
        {
            DontDestroyOnLoad( this );
        }

        private void OnApplicationQuit()
        {
            if( m_AssetBundleStorageList != null )
            {
                foreach( AssetBundleStorage assetBundleStorage in m_AssetBundleStorageList )
                {
                    assetBundleStorage.Unload();
                }
            }

            m_AssetBundleStorageList = null;
        }

        /// <summary>
        /// AssetBundle定義ファイルの読み込み開始
        /// </summary>
        public void LoadAssetBundleDefineData()
        {
            if( IsLoadedDefineData ) return;

#if EDITOR_LOAD_ASSET_PATH
            //特定のフォルダ直下にあるAssetファイルを全て取得
            string[] fileList = Directory.GetFiles( AssetBundleDefine.ASSET_BUNDLE_INFO_DIRECTORY_PATH, "*" + ExtensionDefine.ASSET, SearchOption.TopDirectoryOnly );

            for( int i = 0; i < fileList.Length; i++ )
            {
                AssetBundleInfo info = AssetDatabase.LoadAssetAtPath<AssetBundleInfo>( fileList[ i ] );

                if( info != null ) m_AssetBundleDefineList.Add( info );
            }

            //カテゴリ順にソート
            m_AssetBundleDefineList.Sort( ( a, b ) => a.Category - b.Category );

#pragma warning disable CS0162 // 到達できないコードが検出されました
            if( AssetBundleDefine.ASSET_BUNDLE_LOAD_PSEUDO_DELAY > 0 ) StartCoroutine( PseudoWaitAssetBundleDefineData() );
            else IsLoadedDefineData = true;
#pragma warning restore CS0162 // 到達できないコードが検出されました

#else
            StartCoroutine( LoadAssetBundleDefineDataAsync() );
#endif
        }

#if EDITOR_LOAD_ASSET_PATH
        /// <summary>
        /// AssetBundle定義ファイルの読み込みを疑似的に再現させた待ち時間
        /// </summary>
        /// <returns></returns>
        private IEnumerator PseudoWaitAssetBundleDefineData()
        {
            IsLoadedDefineData = false;

            yield return new WaitForSeconds( AssetBundleDefine.ASSET_BUNDLE_LOAD_PSEUDO_DELAY );

            //読み込み終了
            IsLoadedDefineData = true;
        }
#else
        /// <summary>
        /// 非同期でのAssetBundle定義ファイルの読み込み
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadAssetBundleDefineDataAsync()
        {
            IsLoadedDefineData = false;

            //AssetBundle定義ファイルが含まれているAssetBundleを読み込み
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( AssetBundleDefine.ASSETBUNDLE_DEFINITAION_INFO_PATH );

            yield return request;

            AssetBundleStorage abs = new AssetBundleStorage( AssetBundleCategory.AssetBundleInfo, request.assetBundle );
            m_AssetBundleStorageList.Add( abs );

            //AssetBundleに含まれている全てのAssetを読み込む
            yield return LoadDesignationAssetAsync( AssetBundleCategory.AssetBundleInfo );

            //AssetBundleDefineInfoを取得し格納
            foreach( AssetStorage assetStorage in abs.AssetStorageList )
            {
                if( assetStorage == null ) continue;

                m_AssetBundleDefineList.Add( assetStorage.Asset as AssetBundleInfo );
            }

            //カテゴリ順にソート
            m_AssetBundleDefineList.Sort( ( a, b ) => a.Category - b.Category );

            //読み込み終了
            IsLoadedDefineData = true;
        }
#endif

        /// <summary>
        /// Asset識別子を指定してAssetのパスを取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>指定したAssetのパス</returns>
        public string GetAssetPath( uint _identifier )
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint assetIndex );

            return GetAssetPath( category, assetIndex );
        }

        /// <summary>
        /// Asset識別子を指定してAssetのパスを取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>指定したAssetのパス</returns>
        public string GetAssetPath( AssetBundleCategory _category, uint _index )
        {
            AssetBundleInfo categoyInfo = m_AssetBundleDefineList.Find( ( data ) => data.Category == _category );
            AssetBundleInfo.AssetData assetInfo = categoyInfo.AssetDataList[ _index ];

            string path = string.IsNullOrEmpty( assetInfo.Path ) ? "" : assetInfo.Path + "/";

            return string.Format( AssetBundleDefine.ASSET_FILE_PATH_BASE, path, assetInfo.AssetName, assetInfo.Extention );
        }

        /// <summary>
        /// Asset名を取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>指定したAssetのファイル名</returns>
        public string GetAssetName( uint _identifier )
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return GetAssetName( category, index );
        }

        /// <summary>
        /// Asset名を取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>指定したAssetのファイル名</returns>
        public string GetAssetName( AssetBundleCategory _category, uint _index )
        {
            AssetBundleInfo categoyInfo = m_AssetBundleDefineList.Find( ( data ) => data.Category == _category );
            AssetBundleInfo.AssetData assetInfo = categoyInfo.AssetDataList[ _index ];

            return assetInfo.AssetName;
        }

        /// <summary>
        /// 全てのの AssetBundleを読み込む
        /// </summary>
        public void LoadAllAssetBundle()
        {
            foreach( AssetBundleInfo info in m_AssetBundleDefineList )
            {
#if !TRIAL_MODE_ON
                if( info.IsTrialAssetBundle ) continue;
#endif // !TRIAL_MODE_ON
                LoadAssetBundle( info.Category, out AssetBundleStorage storage );
            }
        }

        /// <summary>
        /// 全ての AssetBundleが読み込み済みかの確認
        /// </summary>
        /// <returns></returns>
        public bool IsLoadedAllAssetBundle()
        {
            foreach( AssetBundleInfo info in m_AssetBundleDefineList )
            {
#if !TRIAL_MODE_ON
                if( info.IsTrialAssetBundle ) continue;
#endif //!TRIAL_MODE_ON
                AssetBundleStorage storage = GetAssetBundleStorage( info.Category );

#if !EDITOR_LOAD_ASSET_PATH
                if( storage == null ) return false;
#endif //!EDITOR_LOAD_ASSET_PATH
                if( storage.State != AssetBundleStorage.AssetBundleState.Enable ) return false;
            }

            return true;
        }


        /// <summary>
        /// 指定のAssetBundleを読み込み開始
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns>指定したカテゴリの<see cref="AssetBundleStorage"/></returns>
        public AssetLoadResultCode LoadAssetBundle( AssetBundleCategory _category, out AssetBundleStorage _outStorage )
        {
            AssetBundleInfo defineInfo = GetAssetBundleDefineInfo( _category );
            _outStorage = GetAssetBundleStorage( _category );

            if( _outStorage == null )
            {
                _outStorage = new AssetBundleStorage( _category );
                m_AssetBundleStorageList.Add( _outStorage );
            }

            if( defineInfo.RequiredCategoryList != null || defineInfo.RequiredCategoryList.Length > 0 )
            {
                foreach( AssetBundleCategory requiredCategory in defineInfo.RequiredCategoryList )
                {
                    LoadAssetBundle( requiredCategory, out AssetBundleStorage required );
                }
            }

            if( _outStorage.State == AssetBundleStorage.AssetBundleState.Enable ) return AssetLoadResultCode.Complited;
            if( _outStorage.State == AssetBundleStorage.AssetBundleState.Loading ) return AssetLoadResultCode.NowLoading;

            _outStorage.StartLoading();

            if( defineInfo.IsAddOnContent )
            {//追加コンテンツ用AssetBundle
#if EDITOR_LOAD_ASSET_PATH
                SetEnableAssetBundleFromEditor( _category );
#else //EDITOR_LOAD_ASSET_PATH

#if UNITY_ONLY_SWITCH
                //追加コンテンツ用の読み込み処理へ                    
                if( _outStorage.State != AssetBundleStorage.AssetBundleState.Enable ) StartCoroutine( LoadAocAssetBundleAsync( _category ) );
#else //UNITY_ONLY_SWITCH
                //通常の読み込み処理へ
                if( _outStorage.State != AssetBundleStorage.AssetBundleState.Enable ) StartCoroutine( LoadAssetBundleFromFileAsync( _category ) );
#endif //UNITY_ONLY_SWITCH

#endif //EDITOR_LOAD_ASSET_PATH
            }
            else
            {//通常のAssetbundle
#if EDITOR_LOAD_ASSET_PATH
                SetEnableAssetBundleFromEditor( _category );
#else
                if( _outStorage.State != AssetBundleStorage.AssetBundleState.Enable ) StartCoroutine( LoadAssetBundleFromFileAsync( _category ) );
#endif
            }

            return AssetLoadResultCode.NowLoading; ;
        }

#if EDITOR_LOAD_ASSET_PATH
        /// <summary>
        /// UnityEditor上でAssetBundleの読み込みのふりをして待つか、即座に有効化する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        private void SetEnableAssetBundleFromEditor( AssetBundleCategory _category )
        {
            AssetBundleStorage abs = GetAssetBundleStorage( _category );

#pragma warning disable CS0162 // 到達できないコードが検出されました
            if( AssetBundleDefine.ASSET_BUNDLE_LOAD_PSEUDO_DELAY > 0 )
            {//一定時間後にAssetBundleをロードした扱いで有効化
                float delay = AssetBundleDefine.ASSET_BUNDLE_LOAD_PSEUDO_DELAY;

                if( AssetBundleDefine.IS_PSEUDO_DELAY_ADDITION_COUNT ) delay *= m_AssetBundlePseudoLoadCount + 1;

                StartCoroutine( EnableAssetBundleDelay( abs, delay ) );

                m_AssetBundlePseudoLoadCount++;
            }
            else
            {//即座にAssetbundleを有効化
                abs.DebugSetEnableAssetbundle();
            }
#pragma warning restore CS0162 // 到達できないコードが検出されました
        }

        /// <summary>
        /// AssetBundleを読み込んでいるふりをして、指定時間後の有効可する
        /// </summary>
        /// <param name="_categoty">カテゴリ</param>
        /// <param name="_delayTime">待ち時間</param>
        /// <returns></returns>
        private IEnumerator EnableAssetBundleDelay( AssetBundleStorage _abs, float _delayTime )
        {
            yield return new WaitForSeconds( _delayTime );

            AssetBundleInfo defineInfo = GetAssetBundleDefineInfo( _abs.Category );

            while( !defineInfo.IsLoadedRequiredCategory() ) yield return null;

            _abs.DebugSetEnableAssetbundle();

            m_AssetBundlePseudoLoadCount = Mathf.Max( 0, m_AssetBundlePseudoLoadCount - 1 );
        }
#else
        /// <summary>
        /// 非同期でファイルからAssetBundleを読み込む
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns></returns>
        private IEnumerator LoadAssetBundleFromFileAsync( AssetBundleCategory _category )
        {
            float startTime = Time.unscaledTime;

            AssetBundleInfo assetBundleDefineInfo = GetAssetBundleDefineInfo( _category );

            string path = Application.streamingAssetsPath + "/" + assetBundleDefineInfo.AssetBundleName;

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync( path );

            yield return request;

            while( !assetBundleDefineInfo.IsLoadedRequiredCategory() ) yield return null;

            AssetBundleStorage assetBundleStorage = GetAssetBundleStorage( _category );

            if( assetBundleStorage == null )
            {
                assetBundleStorage = new AssetBundleStorage( _category, null );
                m_AssetBundleStorageList.Add( assetBundleStorage );
            }

            if( request.assetBundle != null ) Debug.Log( "AssetBundle.LoadFromFileAsync Success:" + assetBundleDefineInfo.AssetBundleName );
            else Debug.LogError( "AssetBundle.LoadFromFileAsync Failed:" + assetBundleDefineInfo.AssetBundleName );

            assetBundleStorage.SetAssetBundle( request.assetBundle );

            Debug.Log( assetBundleDefineInfo.AssetBundleName + " Load AssetBundle Start:" + startTime + " End:" + Time.unscaledTime + " Progress:" + ( Time.unscaledTime - startTime ) + " Result :" + assetBundleStorage.State.ToString() );
        }
#endif

        /// <summary>
        /// Assetを読み込む
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>Assetとその状態を格納した<see cref="AssetStorage"/></returns>
        public AssetLoadResultCode LoadAsset( uint _identifier, out AssetStorage _outValue )
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return LoadAsset( category, index, out _outValue );
        }


        /// <summary>
        /// Assetを読み込む
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>Assetとその状態を格納した<see cref="AssetStorage"/></returns>
        public AssetLoadResultCode LoadAsset( AssetBundleCategory _category, uint _index, out AssetStorage _outStorage )
        {
            _outStorage = null;
#if EDITOR_LOAD_ASSET_PATH
            Object asset = LoadAssetAtPath<Object>( GetAssetPath( _category, _index ) );
            _outStorage = new AssetStorage( _category, _index, asset );
#else
            _outStorage = GetAssetStorage( _category, _index );

            if( _outStorage.Asset != null ) _outStorage.Reuse();
            else StartCoroutine( LoadAssetAsync( _category, _index ) );
#endif
            if( _outStorage.State == AssetStorage.AssetState.Enable ) return AssetLoadResultCode.Complited;
            else if( _outStorage.State == AssetStorage.AssetState.Loading ) return AssetLoadResultCode.NowLoading;

            return AssetLoadResultCode.Error;
        }

#if !EDITOR_LOAD_ASSET_PATH
        /// <summary>
        /// 非同期でAssetを読み込む
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns></returns>
        private IEnumerator LoadAssetAsync( AssetBundleCategory _category, uint _index )
        {
            string filePath = GetAssetPath( _category, _index );

            AssetBundleRequest request = GetAssetBundleStorage( _category ).LoadAssetAsync( filePath );

            if( request == null )
            {
                Debug.Log( "Missing AssetBundleRequest Category:" + _category.ToString() );
                yield break;
            }

            yield return request;

            if( request.asset != null )
            {
                AssetStorage assetStorage = GetAssetStorage( _category, _index );

                assetStorage.StoreAsset( request.asset );

                Debug.Log( "LoadAssetAsync Success:" + filePath );
            }
        }
#endif

        /// <summary>
        /// 指定のAssetBundleに含まれおり、<see cref="AssetBundleInfo"/>に登録されているAssetを全て読み込む
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns>trueなら読み込みの要求が成功</returns>
        public bool LoadAllDefinitionAsset( AssetBundleCategory _category )
        {
#if !EDITOR_LOAD_ASSET_PATH
            AssetBundleInfo info = GetAssetBundleDefineInfo( _category );

            if( info == null ) return false;

            for( uint i = 0; i < info.AssetDataList.Length; i++ )
            {
                Debug.Log( "LoadAllDefinitionAsset Category : " + info.Category + " AssetName:" + info.AssetDataList[ i ].AssetName );
                LoadAsset( info.Category, i, out AssetStorage storage );
            }
#endif
            return true;
        }

#if !EDITOR_LOAD_ASSET_PATH
        /// <summary>
        /// 指定のAssetBundleに含まれるAssetをすべて読み込む
        /// </summary>
        /// <remarks>現状は<see cref="AssetBundleDefineInfo"/>が含まれるAssetBundleからの読み込みのみを想定</remarks>
        /// <param name="_category">読み込むAssetBundleのカテゴリ</param>
        /// <returns></returns>
        private IEnumerator LoadDesignationAssetAsync( AssetBundleCategory _category )
        {
            AssetBundleStorage assetBundleStorage = GetAssetBundleStorage( _category );
            AssetBundleRequest request = assetBundleStorage.LoadAllAssetsAsync();

            if( request == null )
            {
                Debug.Log( "Missing AssetBundleRequest Category:" + _category );
                yield break;
            }

            yield return request;

            assetBundleStorage.EndLoadAllAssets();

            for( int i = 0; i < request.allAssets.Length; i++ )
            {
                Object asset = request.allAssets[ i ];

                uint identifier = AssetIdentifier.CreateIdentifier( _category, 0 );

                if( _category == AssetBundleCategory.AssetBundleInfo )
                {
                    identifier += ( uint )i;
                }
                else
                {
                    AssetBundleInfo info = GetAssetBundleDefineInfo( _category );
                    identifier = info.GetIdentifier( asset.name );
                }

                AssetStorage assetStorage = GetAssetStorage( identifier );
                assetStorage.StoreAsset( asset );

                Debug.Log( "LoadDesignationAssetAsync Category:" + _category + " Identifier: " + identifier + " AssetName:" + asset.name );
            }
        }
#endif

        /// <summary>
        /// ゲーム終了まで破棄しないAssetBundleの読み込み
        /// </summary>
        public void LoadKeepAsset()
        {
#if !EDITOR_LOAD_ASSET_PATH
            foreach( AssetBundleInfo info in m_AssetBundleDefineList )
            {
#if !TRIAL_MODE_ON
                if( info.IsTrialAssetBundle ) continue;
#endif //!TRIAL_MODE_ON
                if( !info.IsNotRelease ) continue;

                LoadAllDefinitionAsset( info.Category );
            }
#endif //!EDITOR_LOAD_ASSET_PATH
        }

        /// <summary>
        /// 指定したAssetが読み込み済みか確認
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns></returns>
        public bool IsLoadAsset( AssetBundleCategory _category, uint _index )
        {
            return IsLoadAsset( AssetIdentifier.CreateIdentifier( _category, _index ) );
        }

        /// <summary>
        /// 指定したAssetが読み込み済みか確認
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>trueなら読み込み済み</returns>
        public bool IsLoadAsset( uint _identifier )
        {
#if EDITOR_LOAD_ASSET_PATH
            return true;
#else
            AssetStorage assetStorage = GetAssetStorage( _identifier, false );

            if( assetStorage == null ) return false;
            if( assetStorage.State == AssetStorage.AssetState.Loading ) return false;
            if( assetStorage.Asset == null ) return false;

            return true;
#endif
        }

        /// <summary>
        /// AssetBundleの読み込み中かの確認
        /// </summary>
        /// <returns>trueの場合は読み込み中 falseはそれ以外</returns>
        public bool IsLoadingAssetBundle()
        {
#if !EDITOR_LOAD_ASSET_PATH
            foreach( AssetBundleStorage assetBundleStorage in m_AssetBundleStorageList )
            {
                if( assetBundleStorage == null ) return true;
                if( assetBundleStorage.State == AssetBundleStorage.AssetBundleState.Loading ) return true;
            }
#endif //EDITOR_LOAD_ASSET_PATH

            return false;
        }

        /// <summary>
        /// Assetの読み込み中かの確認
        /// </summary>
        /// <returns>trueなら読み込み中</returns>
        public bool IsLoadingAsset()
        {
#if !EDITOR_LOAD_ASSET_PATH
            foreach( AssetBundleStorage assetBundleStorage in m_AssetBundleStorageList )
            {
                if( assetBundleStorage == null ) continue;
                if( assetBundleStorage.IsLoading() ) return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// 読み込んだAssetを取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>指定した型のAsset nullの場合は失敗</returns>
        public T GetAssetOrigin<T>( uint _identifier ) where T : Object
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return GetAssetOrigin<T>( category, index );
        }

        /// <summary>
        /// 読み込んだAssetを取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>指定した型のAsset nullの場合は失敗</returns>
        public T GetAssetOrigin<T>( AssetBundleCategory _category, uint _index ) where T : Object
        {
#if EDITOR_LOAD_ASSET_PATH
            return LoadAssetAtPath<T>( GetAssetPath( _category, _index ) );
#else
            AssetStorage assetStorage = GetAssetStorage( _category, _index, false );

            if( assetStorage == null )
            {
                Debug.LogWarning( "Not Load AssetBundle Category : " + _category.ToString() + " Index : " + _index );
                return null;
            }

            if( assetStorage.State == AssetStorage.AssetState.Loading )
            {
                Debug.LogWarning( "Loading Asset Identifier Category : " + _category.ToString() + " Index : " + _index );
                return null;
            }

            return assetStorage.GetAssetOrigin<T>();
#endif
        }

        /// <summary>
        /// 読み込んだAssetをSpriteで取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_isUseSpriteAtlas">SpriteAtlasに含まれているSpriteを読み込む場合はtrue</param>
        /// <returns><see cref="Sprite"/> nullの場合は失敗</returns>
        public Sprite GetAssetOriginSprite( uint _identifier, bool _isUseSpriteAtlas = true )
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return GetAssetOriginSprite( category, index, _isUseSpriteAtlas );
        }

        /// <summary>
        /// 読み込んだAssetをSpriteで取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_isUseSpriteAtlas">SpriteAtlasに含まれているSpriteを読み込む場合はtrue</param>
        /// <returns><see cref="Sprite"/> nullの場合は失敗</returns>
        public Sprite GetAssetOriginSprite( AssetBundleCategory _category, uint _index, bool _isUseSpriteAtlas = true )
        {
#if EDITOR_LOAD_ASSET_PATH
            Texture2D texture = GetAssetOrigin<Texture2D>( _category, _index );

            if( texture == null )
            {
                Debug.LogWarning( "Missing Sprite File! " + " Category:" + _category.ToString() + " Index" + _index );
                return null;
            }

            return Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );
#else
            if( !_isUseSpriteAtlas )
            {
                Texture2D texture = GetAssetOrigin<Texture2D>( _category, _index );

                if( texture == null )
                {
                    Debug.LogWarning( "Missing Sprite File! " + " Category:" + _category.ToString() + " Index" + _index );
                    return null;
                }

                return Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );
            }

            return GetAssetOrigin<Sprite>( _category, _index );
#endif
        }

        /// <summary>
        /// Assetを複製し指定したComponentで取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_parent">親オブジェクトのTransform</param>
        /// <returns>複製された指定した型のオブジェクト</returns>
        public T InstantiateAsset<T>( uint _identifier, Transform _parent ) where T : Component
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return InstantiateAsset<T>( category, index, _parent );
        }

        /// <summary>
        /// Assetを複製し指定したComponentで取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_parent">親オブジェクトのTransform</param>
        /// <returns>複製された指定した型のオブジェクト</returns>
        public T InstantiateAsset<T>( AssetBundleCategory _category, uint _index, Transform _parent ) where T : Component
        {
#if EDITOR_LOAD_ASSET_PATH
            return InstantiateAssetAtPath<T>( GetAssetPath( _category, _index ), _parent );
#else
            return InstantiateToAssetStorege<T>( _category, _index, _parent );
#endif
        }

#if !EDITOR_LOAD_ASSET_PATH
        /// <summary>
        /// 読み込み済みAssetを複製し指定したComponentで取得
        /// </summary>
        /// <typeparam name="T">Assetの型  </typeparam>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_parent">親オブジェクトのTransform</param>
        /// <returns>複製された指定した型のオブジェクト</returns>
        private T InstantiateToAssetStorege<T>( AssetBundleCategory _category, uint _index, Transform _parent ) where T : Component
        {
            AssetStorage assetStorage = GetAssetStorage( _category, _index, false );

            if( assetStorage == null ) return null;
            if( assetStorage.State == AssetStorage.AssetState.Loading ) return null;

            T obj = assetStorage.Instantiate<T>( _parent );

            return obj;
        }
#endif

        /// <summary>
        /// 読み込み済みのAssetを破棄対象に変更
        /// </summary>
        public void SetAllAssetNotUse()
        {
            foreach( AssetBundleStorage assetBundleStorage in m_AssetBundleStorageList )
            {
                AssetBundleInfo info = GetAssetBundleDefineInfo( assetBundleStorage.Category );

                if( info == null ) continue;
                if( info.IsNotRelease ) continue;

                if( !assetBundleStorage.IsNotRelease ) assetBundleStorage.SetAllAssetNotUse();
            }
        }

        /// <summary>
        /// 指定のAssetBundleに含まれるAssetを破棄対象に変更する
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_isForced">trueの場合、Assetを破棄しないフラグを無視して強制的に破棄対象にする</param>
        public void SetAssetNotUse( AssetBundleCategory _category, bool _isForced = false )
        {
            AssetBundleInfo info = GetAssetBundleDefineInfo( _category );

            if( info == null ) return;
            if( info.IsNotRelease ) return;

            AssetBundleStorage assetBundleStorage = GetAssetBundleStorage( _category );
            if( _isForced ) assetBundleStorage.IsNotRelease = false;

            if( assetBundleStorage != null && !assetBundleStorage.IsNotRelease ) assetBundleStorage.SetAllAssetNotUse();
        }

        /// <summary>
        /// 未使用のAssetを破棄
        /// </summary>
        public void UnloadAsset()
        {
            foreach( AssetBundleStorage assetBundleStorage in m_AssetBundleStorageList )
            {
                assetBundleStorage.UnloadAsset();
            }
        }

        /// <summary>
        /// AssetBundleDefineInfoの取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns>指定したカテゴリの<see cref="AssetBundleInfo"/></returns>
        public AssetBundleInfo GetAssetBundleDefineInfo( AssetBundleCategory _category )
        {
            return m_AssetBundleDefineList.Find( ( data ) => data.Category == _category );
        }

        /// <summary>
        /// AssetBundleStorageを取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>指定したAsset識別子の<see cref="AssetBundleStorage"/></returns>
        public AssetBundleStorage GetAssetBundleStorage( uint _identifier ) => GetAssetBundleStorage( AssetIdentifier.ConvertCategoty( _identifier ) );

        /// <summary>
        /// AssetBundleStorageを取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns>指定したカテゴリの<see cref="AssetBundleStorage"/></returns>
        public AssetBundleStorage GetAssetBundleStorage( AssetBundleCategory _category )
        {
            if( m_AssetBundleStorageList == null ) return null;
            if( m_AssetBundleStorageList.Count <= 0 ) return null;

            AssetBundleStorage assetBundleStorage = m_AssetBundleStorageList.Find( ( abs ) => abs.Category == _category );

            return assetBundleStorage;
        }

        /// <summary>
        /// 指定のAssetBundleStorageを削除する
        /// </summary>
        /// <param name="_category"></param>
        public void RemoveAssetBundle( AssetBundleCategory _category )
        {
            if( m_AssetBundleStorageList == null ) return;
            if( m_AssetBundleStorageList.Count <= 0 ) return;
            if( _category == AssetBundleCategory.AssetBundleInfo ) return;

            AssetBundleStorage storage = GetAssetBundleStorage( _category );

            if( storage != null )
            {
                storage.Unload();
                m_AssetBundleStorageList.Remove( storage );
            }
        }

        /// <summary>
        /// AssetStorageを取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_isCreate">AssetStorageがない場合に新規作成するかのフラグ</param>
        /// <returns>指定したAsset識別子の<see cref="AssetStorage"/></returns>
        private AssetStorage GetAssetStorage( uint _identifier, bool _isCreate = true )
        {
            AssetIdentifier.DivideIdentifier( _identifier, out AssetBundleCategory category, out uint index );

            return GetAssetStorage( category, index, _isCreate );
        }

        /// <summary>
        /// AssetStorageを取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_isCreate">AssetStorageがない場合に新規作成するかのフラグ</param>
        /// <returns>指定したカテゴリとインデックスの<see cref="AssetStorage"/></returns>
        private AssetStorage GetAssetStorage( AssetBundleCategory _category, uint _index, bool _isCreate = true )
        {
            AssetBundleStorage assetBundleStorage = GetAssetBundleStorage( _category );
            AssetStorage assetStorage = assetBundleStorage.GetAssetStorage( _category, _index, _isCreate );

            return assetStorage;
        }

        #region AddOnContent
        /// <summary>
        /// 指定のAssetBundleが追加コンテンツ用か
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns>追加コンテンツ用のAssetBundleであればture</returns>
        public bool IsAddOnContent( AssetBundleCategory _category )
        {
            AssetBundleInfo defineInfo = GetAssetBundleDefineInfo( _category );

            return defineInfo.IsAddOnContent;
        }

        /// <summary>
        /// インストール済みで権利を保有する追加コンテンツの数の取得
        /// </summary>
        /// <remarks>Nintendo Switchj以外の環境では常に0</remarks>
        /// <returns>インストール済みで権利を保有する追加コンテンツの数</returns>
        public int GetInstalledAocCount()
        {
#if UNITY_ONLY_SWITCH
            return Aoc.CountAddOnContent();
#else
            return 0;
#endif
        }

        /// <summary>
        /// 指定の追加コンテンツがインストール済みかを確認する
        /// </summary>
        /// <remarks>Nintendo Switchj以外の環境では常にfalse</remarks>
        /// <param name="_category">カテゴリ</param>
        /// <returns>インストール済みならtrue</returns>
        public bool IsInstalledAoc( AssetBundleCategory _category )
        {
#if UNITY_ONLY_SWITCH
            int[] aocListupBuffer = new int[ AssetBundleDefine.AOC_LISTUP_COUNT_MAX ];
            int aocCount = GetInstalledAocCount();

            if( aocCount <= 0 ) return false;

            AssetBundleInfo info = GetAssetBundleDefineInfo( _category );

            if( !info.IsAddOnContent ) return false;

            int listupCount = Aoc.ListAddOnContent( aocListupBuffer, 0, AssetBundleDefine.AOC_LISTUP_COUNT_MAX );

            return System.Array.FindIndex( aocListupBuffer, ( int index ) => index == info.AocIndex ) >= 0;
#else
            return false;
#endif
        }

#if !EDITOR_LOAD_ASSET_PATH && UNITY_ONLY_SWITCH
        /// <summary>
        /// 追加コンテンツのAssetBundleの読み込み
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <returns></returns>
        private IEnumerator LoadAocAssetBundleAsync( AssetBundleCategory _category )
        {
            float startTime = Time.unscaledTime;

            AssetBundleInfo info = GetAssetBundleDefineInfo( _category );
            AssetBundleStorage assetBundleStorage = GetAssetBundleStorage( _category );

            if( assetBundleStorage == null )
            {
                assetBundleStorage = new AssetBundleStorage( _category, null );
                m_AssetBundleStorageList.Add( assetBundleStorage );
            }

            if( !IsInstalledAoc( _category ) )
            {
                assetBundleStorage.NotInstalled();
                yield break;
            }

            long cacheSize = 0;

            Result result = AddOnContent.QueryMountCacheSize( ref cacheSize, info.AocIndex );
            result.abortUnlessSuccess();

            byte[] mountCacheBuffer = new byte[ cacheSize ];

            result = AddOnContent.Mount( AssetBundleDefine.AOC_MOUNT_NAME, info.AocIndex, mountCacheBuffer, cacheSize );
            result.abortUnlessSuccess();

            FileHandle fileHandle = new FileHandle();
            string path = string.Format( AssetBundleDefine.AOC_PAHT_BASE, info.AocName );

            result = File.Open( ref fileHandle, path, OpenFileMode.Read );
            result.abortUnlessSuccess();

            long fileSize = 0;

            result = File.GetSize( ref fileSize, fileHandle );
            result.abortUnlessSuccess();

            byte[] data = new byte[ fileSize ];

            result = File.Read( fileHandle, 0, data, fileSize );
            result.abortUnlessSuccess();

            File.Close( fileHandle );
            FileSystem.Unmount( AssetBundleDefine.AOC_MOUNT_NAME );

            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync( data );

            yield return request;

            if( request.assetBundle != null ) Debug.Log( "LoadAocAssetBundleAsync Success:" + _category.ToString() );
            else Debug.LogError( "LoadAocAssetBundleAsync Failed:" + _category.ToString() );

            assetBundleStorage.SetAssetBundle( request.assetBundle );

            Debug.Log( info.AssetBundleName + " Load AssetBundle Start:" + startTime + " End:" + Time.unscaledTime + " Progress:" + ( Time.unscaledTime - startTime ) + " Result :" + assetBundleStorage.State.ToString() );
        }
#endif
        #endregion //AddOnContent

#if UNITY_EDITOR
        /// <summary>
        /// 指定のパスのAssetの読み込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadAssetAtPath<T>( string _path ) where T : Object
        {
            Debug.Log( _path );

            return AssetDatabase.LoadAssetAtPath<T>( _path );
        }

        /// <summary>
        /// 指定のパスのAssetの複製を取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_path">ファイルパス</param>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public static T InstantiateAssetAtPath<T>( string _path, Transform _parent ) where T : Component
        {
            GameObject obj = Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>( _path ) );
            obj.transform.SetParent( _parent );
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            return typeof( T ) == typeof( GameObject ) ? obj as T : obj.GetComponent<T>();
        }
#endif //UNITY_EDITOR
    }
}