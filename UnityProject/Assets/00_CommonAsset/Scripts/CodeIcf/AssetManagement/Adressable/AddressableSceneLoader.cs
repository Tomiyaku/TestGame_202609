using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// AddressablesAssetを使用したシーンの読み込み
    /// </summary>
    public class AddressableSceneLoader : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        private static AddressableSceneLoader m_Instance = null;

        /// <summary>インスタンスが確保されているか</summary>
        public static bool IsEnableInstance => m_Instance != null;
        /// <summary>シーンの非同期読み込みのHandle</summary>
        private AsyncOperationHandle<SceneInstance> m_SceneInstanceHandle;
        /// <summary>最新の読み込み処理結果</summary>
        public LoadSceneResult LoadSceneResult { get; private set; } = LoadSceneResult.None;
        /// <summary>シーン切り替え開始時間</summary>
        private float m_SceneChangeStartTime = 0;

        /// <summary>インスタンス</summary>
        public static AddressableSceneLoader Instance
        {
            get
            {
                if( !IsEnableInstance )
                {
                    m_Instance = FindAnyObjectByType<AddressableSceneLoader>();

                    if( !!IsEnableInstance )
                    {
                        CreateInstance();
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// インスタンスの作成
        /// </summary>
        public static void CreateInstance()
        {
            if( IsEnableInstance ) return;

            GameObject obj = new GameObject( "AddressableSceneLoader" );
            obj.transform.position = Vector3.zero;
            obj.transform.eulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            m_Instance = obj.AddComponent<AddressableSceneLoader>();
            DontDestroyOnLoad( m_Instance );
        }

        /// <summary>
        /// AddressableAssetでのシーンの読み込みとシーンの初期化
        /// </summary>
        ///<param name="_category">カテゴリ</param>
        ///<param name="_index">インデックス</param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public LoadSceneResult LoadSceneAndSetActive( AddressableCategory _category, uint _index )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _category, _index );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success ) return LoadSceneResult.Error;

            return LoadSceneAndSetActive( identifier.Identifier );
        }

        /// <summary>
        /// AddressableAssetでのシーンの読み込みとシーンの初期化
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns><see cref="AddressableLoadResult"/>を参照</returns>
        public LoadSceneResult LoadSceneAndSetActive( uint _identifier )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _identifier );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! BadIdentifier" );
                return LoadSceneResult.Error_BadIdentifier;
            }

            AddressableInfo info = AddressableResourcesManager.Instance.GetAddressableInfo( identifier.Category );

            if( info == null )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! MissingAddressableInfo" );
                return LoadSceneResult.Error_MissingAddressableInfo;
            }

            if( info.AdressList.Length <= identifier.Index )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! MissingAssetData" );
                return LoadSceneResult.Error_MissingAssetData;
            }

            if( !info.AdressList[ identifier.Index ].IsSceneAsset )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! Not SceneAsset" );
                return LoadSceneResult.Error_NotSceneAsset;
            }

            Addressables.LoadSceneAsync( info.AdressList[ identifier.Index ].Address );

            return LoadSceneResult.Success_LoadSceneAndSetActive;
        }

        /// <summary>
        /// シーンの読み込み
        /// </summary>
        /// <param name="_identifier">シーンのAsset識別子</param>
        /// <returns></returns>
        public LoadSceneResult LoadScene( uint _identifier )
        {
            AddressableIdentifier identifier = new AddressableIdentifier( _identifier );

            if( identifier.ResultState != AddressableIdentifier.ResultCode.Success )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! BadIdentifier" );
                LoadSceneResult = LoadSceneResult.Error_BadIdentifier;
                return LoadSceneResult;
            }

            AddressableInfo info = AddressableResourcesManager.Instance.GetAddressableInfo( identifier.Category );

            if( info == null )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! MissingAddressableInfo" );
                LoadSceneResult = LoadSceneResult.Error_MissingAddressableInfo;
                return LoadSceneResult;
            }

            if( info.AdressList.Length <= identifier.Index )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! MissingAssetData" );
                LoadSceneResult = LoadSceneResult.Error_MissingAssetData;
                return LoadSceneResult;
            }

            if( !info.AdressList[ identifier.Index ].IsSceneAsset )
            {
                Debug.LogError( " AddressableLoadResult LoadSceneAndSetActive Error! Not SceneAsset" );
                LoadSceneResult = LoadSceneResult.Error_NotSceneAsset;
                return LoadSceneResult;
            }

            //if( m_SceneInstanceHandle.Status == AsyncOperationStatus.Succeeded ) Addressables.Release( m_SceneInstanceHandle );

            m_SceneInstanceHandle = Addressables.LoadSceneAsync( info.AdressList[ identifier.Index ].Address, loadMode: LoadSceneMode.Single, activateOnLoad: false );

            StartCoroutine( LoadSceneAsync() );
            LoadSceneResult = LoadSceneResult.Success_StartLoadScene;

            return LoadSceneResult;
        }

        private IEnumerator LoadSceneAsync()
        {
            float startTime = Time.unscaledTime;

            yield return m_SceneInstanceHandle;

            Debug.Log( "ltdl LoadSceneAsync SceneName=" + m_SceneInstanceHandle.Result.Scene.name + " LoadTime=" + ( Time.unscaledTime - startTime ) );

            LoadSceneResult = LoadSceneResult.Success_LoadedScene;
        }

        /// <summary>
        /// 最後に読み込んだシーンをアクティブにする
        /// </summary>
        /// <returns></returns>
        public LoadSceneResult SetActiveScene()
        {
            if( LoadSceneResult != LoadSceneResult.Success_LoadedScene )
            {
                LoadSceneResult = LoadSceneResult.Error_NotLoadedScene;
                return LoadSceneResult;
            }

            m_SceneChangeStartTime = Time.unscaledTime;
            SceneManager.activeSceneChanged += ActiveSceneChanged;

            StartCoroutine( ActiveSceneAsync() );
            LoadSceneResult = LoadSceneResult.Success_SetActiveScene;

            return LoadSceneResult;
        }

        private IEnumerator ActiveSceneAsync()
        {
            yield return m_SceneInstanceHandle.Result.ActivateAsync();

            SceneManager.SetActiveScene( m_SceneInstanceHandle.Result.Scene );
            LoadSceneResult = LoadSceneResult.Success_SetActiveScene;
        }

        public void ActiveSceneChanged( Scene _prev, Scene _next )
        {
            float time = Time.unscaledTime - m_SceneChangeStartTime;
            Debug.Log( "ltdl ActiveSceneChanged Next=" + _next.name + " ChangeTime=" + time );

            SceneManager.activeSceneChanged -= ActiveSceneChanged;
        }
    }
}