using System.Collections.Generic;
using UnityEngine;

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// AssetBundleとそれに含まれるAssetの格納クラス
    /// </summary>
    [System.Serializable]
    public class AssetBundleStorage
    {
        /// <summary>
        /// AssetBundle格納クラスの状態
        /// </summary>
        public enum AssetBundleState
        {
            /// <summary>初期状態</summary>
            Initial,
            /// <summary>読み込み中(必須AssetBundleの読み込み中も含む)</summary>
            Loading,
            /// <summary>使用可能</summary>
            Enable,
            /// <summary>何らかの理由で読み込みに失敗</summary>
            FailedLoading,
            /// <summary>インストールされていない</summary>
            /// <remarks>AssetBundleが追加コンテンツ用の場合のみ設定される</remarks>
            NOT_INSTALLED
        }

        /// <summary>AssetBudleの種類</summary>
        [SerializeField]
        public AssetBundleCategory Category { get; private set; }
        /// <summary>状態</summary>
        [field: SerializeField]
        /// <summary>状態</summary>
        public AssetBundleState State { get; private set; }
        /// <summary>AssetBundle</summary>
        [SerializeField]
        private AssetBundle m_AssetBundle = null;
        /// <summary>読み込み済みのAsset格納クラス一覧</summary>
        [field: SerializeField]
        public List<AssetStorage> AssetStorageList { get; private set; } = new List<AssetStorage>();
        /// <summary>全てのAssetを読み込んだかのフラグ</summary>
        [field: SerializeField]
        public bool IsLoadingAllAssets { get; private set; } = false;
        /// <summary>全てのAssetの読み込み終了</summary>
        public void EndLoadAllAssets() => IsLoadingAllAssets = false;
        /// <summary>
        /// 未使用時に解放しないためのフラグ スクリプト上で変更する場合
        /// </summary>
        [field: SerializeField]
        public bool IsNotRelease { get; set; } = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_assetBundle">AssetBundle</param>
        public AssetBundleStorage( AssetBundleCategory _category, AssetBundle _assetBundle = null )
        {
            Category = _category;
            State = _assetBundle != null ? AssetBundleState.Enable : AssetBundleState.Initial;
            m_AssetBundle = _assetBundle;
        }

#if UNITY_EDITOR
        /// <summary>
        /// AssetBundleの状態を使用可能にする
        /// </summary>
        /// <remarks>
        /// UnityEditor上でのみ動作することが前提のデバッグ用
        /// </remarks>
        public void DebugSetEnableAssetbundle()
        {
            State = AssetBundleState.Enable;

            Debug.Log( "DebugSetEnableAssetbundle : " + Category.ToString() );
        }
#endif

        /// <summary>
        /// このAssetBundleを読み込み中にする
        /// </summary>
        public void StartLoading()
        {
            State = AssetBundleState.Loading;
        }

        /// <summary>
        /// AssetBundleを設定
        /// </summary>
        /// <param name="_assetbundle"></param>
        public void SetAssetBundle( AssetBundle _assetbundle )
        {
            if( _assetbundle == null )
            {
                State = AssetBundleState.FailedLoading;
                return;
            }

            m_AssetBundle = _assetbundle;
            State = AssetBundleState.Enable;
        }

        /// <summary>
        /// AssetStorageを取得
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_isCreate"><see cref="AssetStorage"/>がない場合に新規作成するかのフラグ</param>
        /// <returns></returns>
        public AssetStorage GetAssetStorage( AssetBundleCategory _category, uint _index, bool _isCreate = true )
        {
            return GetAssetStorage( AssetIdentifier.CreateIdentifier( _category, _index ), _isCreate );
        }

        /// <summary>
        /// AssetStorageを取得
        /// </summary>
        /// <param name="_identifier">アセット識別子</param>
        /// <param name="_isCreate"><see cref="AssetStorage"/>がない場合に新規作成するかのフラグ</param>
        /// <returns></returns>
        public AssetStorage GetAssetStorage( uint _identifier, bool _isCreate = true )
        {
            AssetStorage assetStorage = AssetStorageList.Find( ( la ) => la.Identifier == _identifier );

            if( _isCreate && assetStorage == null )
            {
                assetStorage = new AssetStorage( _identifier );
                AssetStorageList.Add( assetStorage );
            }

            return assetStorage;
        }

        /// <summary>
        /// Assetを格納
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_asset">Asset本体</param>
        public void StoreAsset( AssetBundleCategory _category, uint _index, Object _asset )
        {
            StoreAsset( AssetIdentifier.CreateIdentifier( _category, _index ), _asset );
        }

        /// <summary>
        /// Assetを格納
        /// </summary>
        /// <param name="_identifier">アセット識別子</param>
        /// <param name="_asset">Asset本体</param>
        public void StoreAsset( uint _identifier, Object _asset )
        {
            AssetStorage assetStorage = GetAssetStorage( _identifier );
            assetStorage.StoreAsset( _asset );
        }

        /// <summary>
        /// 指定のAssetをAssetBundleから読み込み
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        public AssetBundleRequest LoadAssetAsync( string _path )
        {
            if( m_AssetBundle == null )
            {
                Debug.LogError( "Missing AssetBundle : " + _path );
                return null;
            }

            return m_AssetBundle.LoadAssetAsync( _path );
        }

        /// <summary>
        /// AssetBundleに含まれるAssetをすべて読み込み
        /// </summary>
        /// <returns></returns>
        public AssetBundleRequest LoadAllAssetsAsync()
        {
            if( m_AssetBundle == null ) return null;

            IsLoadingAllAssets = true;

            return m_AssetBundle.LoadAllAssetsAsync();
        }

        /// <summary>
        /// 読み込み中かの確認
        /// </summary>
        /// <returns></returns>
        public bool IsLoading()
        {
            if( IsLoadingAllAssets ) return true;

            foreach( AssetStorage assetStorage in AssetStorageList )
            {
                if( assetStorage.State == AssetStorage.AssetState.Loading ) return true;
                if( assetStorage.Asset == null ) return true;
            }

            return false;
        }

        /// <summary>
        /// 読み込み済みのAssetを破棄対象に変更
        /// </summary>
        public void SetAllAssetNotUse()
        {
            foreach( AssetStorage assetStorage in AssetStorageList )
            {
                assetStorage.Notuse();
            }
        }

        /// <summary>
        /// 未使用のAssetを破棄
        /// </summary>
        public void UnloadAsset()
        {
            for( int i = 0; i < AssetStorageList.Count; i++ )
            {
                if( AssetStorageList[ i ].State != AssetStorage.AssetState.UnloadTarget ) continue;

                AssetStorageList.RemoveAt( i );

                i--;
            }

            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// AssetBundleを含めてすべて破棄
        /// </summary>
        public void Unload()
        {
            AssetStorageList.Clear();
            Resources.UnloadUnusedAssets();

            if( m_AssetBundle != null )
            {
                m_AssetBundle.Unload( true );
                m_AssetBundle = null;
            }
        }

        /// <summary>
        /// AssetBundleの状態をインストールされていないに変更
        /// </summary>
        public void NotInstalled()
        {
            if( !AssetBundleResourcesManager.Instance.IsAddOnContent( Category ) ) return;

            State = AssetBundleState.NOT_INSTALLED;
        }

        /// <summary>
        /// このAssetBundleに含まれるSceneのパスを取得
        /// </summary>
        /// <returns></returns>
        public string GetScenePath()
        {
            if( m_AssetBundle == null ) return null;
            if( !m_AssetBundle.isStreamedSceneAssetBundle ) return null;

            string[] pathList = m_AssetBundle.GetAllScenePaths();

            if( pathList == null || pathList.Length <= 0 ) return null;

            return pathList[ 0 ];
        }
    }
}