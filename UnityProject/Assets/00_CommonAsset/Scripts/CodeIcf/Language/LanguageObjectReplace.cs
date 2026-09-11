using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;

namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語別ゲームオブジェクトの読み込みと切り替え
    /// </summary>
    public class LanguageObjectReplace
    {
        /// <summary><see cref="AddressableInfo"/>が含まれる<see cref="LanguagePackCategory"/></summary>
        public LanguagePackCategory Category { get; private set; }
        /// <summary>対象のAssetインデックス</summary>
        public int AssetIndex { get; private set; }
        /// <summary>言語切り替え後に実行するリソース差し替え処理</summary>
        private System.Action<GameObject> m_ReplaceProcess;
        /// <summary>切り替え済みフラグ</summary>
        public bool IsReplaceAsset { get; private set; } = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private LanguageObjectReplace() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_cateegory">言語別カテゴリ</param>
        /// <param name="_index">Assetのインデックス</param>
        /// <param name="_replaceProcess">リソースの読み込みが終了した際に実行する処理</param>
        public LanguageObjectReplace( LanguagePackCategory _cateegory, int _index, System.Action<GameObject> _replaceProcess )
        {
            Category = _cateegory;
            AssetIndex = _index;
            m_ReplaceProcess = _replaceProcess;
            IsReplaceAsset = false;
        }

        /// <summary>
        /// リソースを開放する
        /// </summary>
        public void ReleaseAsset()
        {
            AddressableInfo addressableInfo = LanguageResourcesManager.Instance.GetAsset<AddressableInfo>( Category, 0 );

            if( addressableInfo != null ) AddressableResourcesManager.Instance.ReleaseStorage( addressableInfo.Category, AssetIndex );

            IsReplaceAsset = false;
        }

        /// <summary>
        /// リソースを読み込む
        /// </summary>
        /// <returns></returns>
        public bool LoadAsset()
        {
            if( IsReplaceAsset ) return true;

            AddressableInfo addressableInfo = LanguageResourcesManager.Instance.GetAsset<AddressableInfo>( Category, 0 );

            if( addressableInfo == null ) return false;

            AddressableLoadResult loadResult = AddressableResourcesManager.Instance.LoadAsset( addressableInfo.Category, AssetIndex );

            if( loadResult == AddressableLoadResult.Complited )
            {
                if( m_ReplaceProcess != null ) m_ReplaceProcess?.Invoke( InstantateGameObject() );

                IsReplaceAsset = true;

                return true;
            }

            return false;
        }

        public GameObject InstantateGameObject()
        {
            AddressableInfo addressableInfo = LanguageResourcesManager.Instance.GetAsset<AddressableInfo>( Category, 0 );

            if( addressableInfo == null ) return null;

            AddressableLoadResult loadResult = AddressableResourcesManager.Instance.LoadAsset( addressableInfo.Category, AssetIndex, out AddressableStorage storage );

            if( loadResult == AddressableLoadResult.Complited )
            {
                return storage.InstantiateAssetToGameObject( null );
            }

            return null;
        }
    }
}