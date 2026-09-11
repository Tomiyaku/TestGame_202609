using UnityEngine;

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// AddressableAssetの定義ファイル
    /// </summary>
    public class AddressableInfo : ScriptableObject
    {
        /// <summary>
        /// Address定義データ
        /// </summary>
        [System.Serializable]
        public class AssetData
        {
            /// <summary>Asset名</summary>
            public string AssetName;
            /// <summary>Adressablesを使用して読み込む際のアドレス</summary>
            public string Address;
            /// <summary>このデータがScene(.unity)ファイルかどうかのフラグ</summary>
            public bool IsSceneAsset;
        }

        /// <summary>種類</summary>
        public AddressableCategory Category;
        /// <summary>Asset定義データ一覧</summary>
        [NonReorderable]
        public AssetData[] AdressList;

        /// <summary>
        /// 指定のAssetのIdentifierを取得
        /// </summary>
        /// <param name="_assetName"></param>
        /// <returns></returns>
        public uint GetIdentifier( string _assetName )
        {
            for( int i = 0; i < AdressList.Length; i++ )
            {
                if( AdressList[ i ] != null && AdressList[ i ].AssetName == _assetName )
                {
                    return AddressableIdentifier.CreateIdentifier( Category, i );
                }
            }

            return 0;
        }
    }
}
