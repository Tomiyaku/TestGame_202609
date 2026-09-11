using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

using CodeIcf.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// Addressableで読み込んだAssetの格納クラス
    /// </summary>
    [System.Serializable]
    public class AddressableStorage
    {
        /// <summary>
        /// 状態
        /// </summary>
        public enum AssetState
        {
            /// <summary>初期状態</summary>
            Init,
            /// <summary>Assetの読み込み中</summary>
            Loading,
            /// <summary>Assetの読み込み完了</summary>
            Enable,
            /// <summary>何らかのエラー</summary>
            Error,
        }

        /// <summary>Asset識別子</summary>
        [SerializeField]
        private uint m_Identifier = 0;
        /// <summary>Asset識別子</summary>        
        public uint Identifier => m_Identifier;
        /// <summary>状態</summary>        
        [SerializeField]
        private AssetState m_State = AssetState.Init;
        /// <summary>状態</summary>        
        public AssetState State => m_State;
        /// <summary>読み込みに使用したHandle</summary>        
        public AsyncOperationHandle m_Handle;
        /// <summary>読み込んだAsset</summary>
        private Object m_Asset = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <param name="_handle">Handle</param>
        public AddressableStorage( AddressableCategory _category, uint _index, AsyncOperationHandle _handle )
        {
            uint identifier = AddressableIdentifier.CreateIdentifier( _category, _index );

            Init( identifier, _handle );
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_handle">Asset</param
        public AddressableStorage( uint _identifier, AsyncOperationHandle _handle )
        {
            Init( _identifier, _handle );
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_handlel">Handle</param>
        private void Init( uint _identifier, AsyncOperationHandle _handlel )
        {
            m_Identifier = _identifier;
            m_Handle = _handlel;
            m_State = AssetState.Loading;
        }

        /// <summary>
        /// 読み込みの結果確認
        /// </summary>
        public void CheckLoadResult()
        {
            if( m_Handle.Status == AsyncOperationStatus.Succeeded )
            {
                m_Asset = m_Handle.Result as Object;
                m_State = AssetState.Enable;
            }
            else
            {
                m_Asset = null;
                m_State = AssetState.Error;
                ReleaseHandle();
            }
        }

        /// <summary>
        /// 読み込んだAssetを取得
        /// </summary>
        /// <typeparam name="T">Assetの型</typeparam>
        /// <returns>指定した型のAsset nullの場合は失敗</returns>
        public T GetAsset<T>() where T : Object
        {
            if( State != AssetState.Enable ) return null;
            if( m_Asset == null ) return null;

            if( typeof( T ) == typeof( Sprite ) )
            {
                Texture2D texture = m_Asset as Texture2D;

                if( texture == null )
                {
                    Debug.LogWarning( "Missing Sprite File! " + " Identifier:" + Identifier );
                    return null;
                }

                Sprite result = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );

                return result as T;
            }

            return m_Asset as T;
        }


        /// <summary>
        /// 読み込んだAssetをSpriteで取得
        /// </summary>
        /// <param name="_isUseSpriteAtlas">SpriteAtlasに含まれているSpriteを読み込む場合はtrue</param>
        /// <returns><see cref="Sprite"/> nullの場合は失敗</returns>
        public Sprite GetSpriteAsset( bool _isUseSpriteAtlas = false )
        {
            //SpriteAtlasに含まれているSpriteの場合
            if( _isUseSpriteAtlas ) return m_Asset as Sprite;

            Texture2D texture = m_Asset as Texture2D;

            if( texture == null )
            {
                Debug.LogWarning( "Missing Sprite File! " + " Identifier:" + Identifier );
                return null;
            }

            return Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );
        }

        /// <summary>
        /// Assetを複製して<see cref="GameObject"/>で返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public GameObject InstantiateAssetToGameObject( Transform _parent )
        {
            if( State != AssetState.Enable ) return null;
            if( m_Asset == null ) return null;

            GameObject obj = Object.Instantiate( m_Asset ) as GameObject;

            if( _parent != null )
            {
                obj.transform.SetParent( _parent );
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localScale = Vector3.one;
            }

            return obj;
        }

        /// <summary>
        /// 指定した型のAssetを複製して返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public T InstantiateAsset<T>( Transform _parent = null ) where T : Component
        {
            GameObject obj = InstantiateAssetToGameObject( _parent );

            return obj != null ? obj.GetComponent<T>() : null;
        }

        /// <summary>
        /// ストレージの開放処理
        /// </summary>
        public void ReleaseStorage()
        {
            m_Asset = null;
            m_State = AssetState.Init;

            ReleaseHandle( true );
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        /// <param name="_isForced">強制開放フラグ(アプリ終了時を想定)</param>
        public void ReleaseHandle( bool _isForced = false )
        {
            if( State != AssetState.Enable ) return;
            //if( Handle.Status != AsyncOperationStatus.Succeeded ) return;

            if( _isForced )
            {
                List<IAssetBundleResource> resourcesList = m_Handle.Result as List<IAssetBundleResource>;

                if( resourcesList != null )
                {
                    foreach( IAssetBundleResource resource in resourcesList )
                    {
                        if( resource == null ) continue;

                        AssetBundle ab = resource.GetAssetBundle();
                        ab.Unload( true );
                    }
                }
            }

            Addressables.Release( m_Handle );
            m_Asset = null;
            m_State = AssetState.Init;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspectorでこのクラスを表示させるためのクラス
    /// </summary>
    [CustomPropertyDrawer( typeof( AddressableStorage ) )]
    public class AddressableStorageDrawer : PropertyDrawer
    {
        private const string PROPARTY_NAME_IDENTIFIER = "m_Identifier";
        private const string PROPARTY_NAME_STATE = "m_State";

        public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
        {
            EditorGUI.BeginProperty( _position, _label, _property );

            SerializedProperty identifierProperty = _property.FindPropertyRelative( PROPARTY_NAME_IDENTIFIER );
            SerializedProperty stateProperty = _property.FindPropertyRelative( PROPARTY_NAME_STATE );

            uint identifier = ( uint )identifierProperty.intValue;
            AddressableIdentifier.DivideIdentifier( identifier, out AddressableCategory category, out uint index );

            AsyncOperationHandle<IList<AddressableInfo>> handle = Addressables.LoadAssetsAsync<AddressableInfo>( AddressableDefine.ADRESSABLES_INFO_NAME, null );
            handle.WaitForCompletion();

            string assetName = "";

            foreach( AddressableInfo info in handle.Result )
            {
                if( info.Category == category && info.AdressList.Length > index )
                {
                    assetName = info.AdressList[ index ].AssetName;
                    break;
                }
            }

            AddressableStorage.AssetState assetState = stateProperty.enumValueIndex.ToEnum<AddressableStorage.AssetState>();

            EditorGUI.LabelField( _position,  "種類 : " + category.ToString()  + " Asset : " + assetName + " 状態 : " + assetState.ToString() );
            EditorGUI.EndProperty();
        }
    }
#endif
}