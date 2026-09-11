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
            /// <summary>このAssetを使用することができるPlatform</summary>
            public UsePlatformType UsePlatform;
            /// <summary>このAssetが使用できる条件</summary>
            public UseContidionType UseContidion;

            /// <summary>
            /// 更新
            /// </summary>
            /// <param name="_assetName"></param>
            /// <param name="_address"></param>
            /// <param name="_isSceneAsset"></param>
            /// <param name="_usePlatform"></param>
            /// <param name="_useContidion"></param>
            public void Updata( string _assetName, string _address, bool _isSceneAsset, UsePlatformType _usePlatform, UseContidionType _useContidion )
            {
                AssetName = _assetName;
                Address = _address;
                IsSceneAsset = _isSceneAsset;
                UsePlatform = _usePlatform;
                UseContidion = _useContidion;
            }
        }

        /// <summary>種類</summary>
        [HideInInspector]
        public AddressableCategory Category;
        /// <summary>Asset定義データ一覧</summary>
        [HideInInspector]
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
