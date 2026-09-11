using UnityEngine;

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// AssetBundle情報
    /// </summary>    
    public class AssetBundleInfo : ScriptableObject
    {
        /// <summary>
        /// Asset情報
        /// </summary>
        [System.Serializable]
        public class AssetData
        {
            /// <summary>Asset名</summary>
            public string AssetName;
            /// <summary><see cref="AssetBundleDefine.ASSET_FILE_DIRECTRY_PATH"/>からこのファイルまでのパス</summary>
            public string Path;
            /// <summary>ファイル拡張子</summary>
            public string Extention;
        }

        /// <summary>種類識別</summary>
        public AssetBundleCategory Category;
        /// <summary>AssetBundle名</summary>
        public string AssetBundleName;
        /// <summary>このAssetBundleに含まれるAssetを使用する場合に読み込みが必要なAssetBundleのカテゴリ</summary>
        public AssetBundleCategory[] RequiredCategoryList;

        /// <summary>体験版のみに使用するAssetBundleかのフラグ</summary>
        public bool IsTrialAssetBundle;
        /// <summary>trueの場合にゲーム終了以外でAssetの破棄を行わなくなる</summary>
        public bool IsNotRelease;

        /// <summary>追加コンテンツ用のAssetBundleかのフラグ</summary>
        public bool IsAddOnContent;
        /// <summary>追加コンテンツ名</summary>
        public string AocName;
        /// <summary>追加コンテンツのインデックス</summary>
        public int AocIndex;

        /// <summary>Asset定義ファイル一覧</summary>
        public AssetData[] AssetDataList;

        /// <summary>
        /// 指定のAssetのIdentifierを取得
        /// </summary>
        /// <param name="_assetName"></param>
        /// <returns></returns>
        public uint GetIdentifier( string _assetName )
        {
            for( int i = 0; i < AssetDataList.Length; i++ )
            {
                if( AssetDataList[ i ] != null && AssetDataList[ i ].AssetName == _assetName )
                {
                    return AssetIdentifier.CreateIdentifier( Category, i );
                }
            }

            return 0;
        }

        /// <summary>
        /// Assetを使用する場合に読み込みが必要なAssetBundleが読み込まれているか
        /// </summary>
        /// <returns>全て読み込み済みであればtrue</returns>
        public bool IsLoadedRequiredCategory()
        {
            if( RequiredCategoryList == null || RequiredCategoryList.Length <= 0 ) return true;

            foreach( AssetBundleCategory category in RequiredCategoryList )
            {
                AssetBundleStorage abs = AssetBundleResourcesManager.Instance.GetAssetBundleStorage( category );

                if( abs.State != AssetBundleStorage.AssetBundleState.Enable ) return false;
            }

            return true;
        }
    }
}