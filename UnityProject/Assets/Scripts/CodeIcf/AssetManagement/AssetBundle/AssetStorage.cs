using UnityEngine;

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// 読み込んだAssetの格納クラス
    /// </summary>
    [System.Serializable]
    public class AssetStorage
    {
        /// <summary>
        /// Asset格納クラスの状態
        /// </summary>
        public enum AssetState
        {
            /// <summary>読み込み中</summary>
            Loading,
            /// <summary>使用可能</summary>
            Enable,
            /// <summary>廃棄対象</summary>
            UnloadTarget
        }

        /// <summary>Asset識別子</summary>
        [field: SerializeField]
        public uint Identifier { get; private set; }
        /// <summary>状態</summary>
        [field: SerializeField]
        public AssetState State { get; private set; }
        /// <summary>読み込んだAsset</summary>
        [field: SerializeField]
        public Object Asset { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_obj">Asset</param>
        public AssetStorage( AssetBundleCategory _category, uint _index, Object _obj = null )
        {
            uint identifier = AssetIdentifier.CreateIdentifier( _category, _index );

            Init( identifier, _obj );
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_obj">Asset</param>
        public AssetStorage( uint _identifier, Object _obj = null )
        {
            Init( _identifier, _obj );
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_obj">Asset</param>
        private void Init( uint _identifier, Object _obj )
        {
            Identifier = _identifier;
            State = AssetState.Loading;
            Asset = null;

            StoreAsset( _obj );
        }

        /// <summary>
        /// Assetを格納
        /// </summary>
        /// <param name="_obj"></param>
        public void StoreAsset( Object _obj )
        {
            if( _obj == null ) return;

            Asset = _obj;
            State = AssetState.Enable;
        }

        /// <summary>
        /// Assetの複製を取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public T Instantiate<T>( Transform _parent ) where T : Component
        {
            if( State == AssetState.Loading ) return null;
            if( Asset == null ) return null;

            GameObject obj = Object.Instantiate( Asset ) as GameObject;
            obj.name = Asset.name;
            obj.transform.SetParent( _parent );
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            Reuse();

            return typeof( T ) == typeof( GameObject ) ? obj as T : obj.GetComponent<T>();
        }

        /// <summary>
        /// 廃棄対象へ変更
        /// </summary>
        public void Notuse()
        {
            if( State == AssetState.Loading ) return;

            State = AssetState.UnloadTarget;
        }

        /// <summary>
        /// 再利用
        /// </summary>
        public void Reuse()
        {
            if( State == AssetState.Loading ) return;

            State = AssetState.Enable;
        }

        /// <summary>
        /// 読み込んだAssetを取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <returns>指定した型のAsset nullの場合は失敗</returns>
        public T GetAssetOrigin<T>() where T : Object
        {
            if( Asset == null ) return null;

            return Asset as T;
        }
    }
}