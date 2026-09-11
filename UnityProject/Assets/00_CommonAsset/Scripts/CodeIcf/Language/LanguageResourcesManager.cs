using System.Collections.Generic;
using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;

namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語別リソース管理
    /// </summary>
    public class LanguageResourcesManager : MonoBehaviour
    {
        /// <summary>
        /// 状態
        /// </summary>
        public enum State
        {
            None,
            LoadingPack,
            LoadingObject,            
            Ready,
        }

        /// <summary>言語別リソースカテゴリリスト</summary>
        public static readonly AddressableCategory[] CATEGORY_LIST = { AddressableCategory.LanguageResources_EN, AddressableCategory.LanguageResources_JP };
        /// <summary>インスタンス</summary>
        private static LanguageResourcesManager m_Instance = null;
        /// <summary>インスタンス</summary>
        public static LanguageResourcesManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    m_Instance = FindAnyObjectByType<LanguageResourcesManager>();

                    if( m_Instance == null )
                    {
                        GameObject obj = new GameObject( "LanguageResourcesManager" );
                        m_Instance = obj.AddComponent<LanguageResourcesManager>();
                        DontDestroyOnLoad( m_Instance );
                    }
                }

                return m_Instance;
            }
        }

        [field:SerializeField]
        /// <summary>状態</summary>
        public State NowState { get; private set; } = State.None;
        /// <summary>初期化済みフラグ</summary>
        public bool IsInitialized { get; private set; } = false;
        /// <summary>言語設定を変更した後にリソースの差し替えを行う対象一覧</summary>
        private List<ILanguageAssetReplace> m_TargetReplaceList = new List<ILanguageAssetReplace>();
        /// <summary>言語設定を変更した後にリソースの差し替えを行う対象Image一覧</summary>
        [SerializeField]
        private List<LanguageImageReplace> m_TargetImageReplaceList = new List<LanguageImageReplace>();
        /// <summary>言語設定を変更した後にリソースの差し替えを行う対象オブジェクト一覧</summary>
        private List<LanguageObjectReplace> m_ObjectReplaceList = new List<LanguageObjectReplace>();
        /// <summary>現在の言語設定でのリソースカテゴリ</summary>
        private AddressableCategory m_NowCategory = AddressableCategory.LanguageResources_EN;
        /// <summary>以前の言語設定でのリソースカテゴリ</summary>
        private AddressableCategory m_PrevCategory = AddressableCategory.LanguageResources_EN;
        [SerializeField]
        private List<LanguageAssetPack> m_AssetPackList = new List<LanguageAssetPack>();
        /// <summary>読み込み対象のAsset識別子一覧</summary>
        private List<uint> m_LoadTargetAssetIdentifierList;
        /// <summary>メッセージ</summary>
        private string[][] m_MessageTextList = null;
        /// <summary>イベントシーン用メッセージ</summary>
        private string[][] m_EventSceneTextList = null;

        public System.Action LanguageChangedAfterProcess = null;

        // Update is called once per frame
        void Update()
        {
            switch( NowState )
            {
                case State.LoadingPack: LoadingLanguagePack(); break;
                case State.LoadingObject: LoadingObject(); break;
            }           
        }

        /// <summary>
        /// 言語別リソースパックの読み込み
        /// </summary>
        private void LoadingLanguagePack()
        {
            bool isLoadComplete = true;

            foreach( uint index in m_LoadTargetAssetIdentifierList )
            {
                AddressableLoadResult loadresult = AddressableResourcesManager.Instance.LoadAsset( index );

                if( loadresult != AddressableLoadResult.Complited ) isLoadComplete = false;
            }

            if( !isLoadComplete ) return;

            m_AssetPackList.Clear();

            List<AddressableStorage> storageList = AddressableResourcesManager.Instance.GetStoragesToCategory( m_NowCategory );

            foreach( AddressableStorage storage in storageList )
            {
                m_AssetPackList.Add( storage.GetAsset<LanguageAssetPack>() );
            }

            m_MessageTextList = null;
            m_EventSceneTextList = null;

            if( m_ObjectReplaceList.Count <= 0 ) ReplaseAsset();
            else NowState = State.LoadingObject;
        }

        /// <summary>
        /// 言語別のゲームオブジェクトを読み込み
        /// </summary>
        private void LoadingObject()
        {
            if( m_ObjectReplaceList.Count <= 0 )
            {
                ReplaseAsset();
                return;
            }

            bool isLoadedObject = true;

            foreach( LanguageObjectReplace objectReplace in m_ObjectReplaceList )
            {
                if( !objectReplace.LoadAsset() ) isLoadedObject = false;
            }

            if( !isLoadedObject ) return;

            ReplaseAsset();
        }

        /// <summary>
        /// リソース差し替え
        /// </summary>
        private void ReplaseAsset()
        {
            NowState = State.Ready;

            foreach( ILanguageAssetReplace replaceTarget in m_TargetReplaceList )
            {
                if( replaceTarget == null ) continue;

                replaceTarget.ReplaceAsset();
            }

            m_TargetReplaceList.Remove( null );

            foreach( LanguageImageReplace imageRelace in m_TargetImageReplaceList )
            {
                if( imageRelace == null ) continue;

                imageRelace.ReplaceImage();
            }

            if( m_NowCategory != m_PrevCategory ) AddressableResourcesManager.ReleaseStorageToTargetCategory( m_PrevCategory );

            LanguageChangedAfterProcess?.Invoke();

            IsInitialized = true;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="_category"></param>
        /// <param name="_indexList"></param>
        public void Init( AddressableCategory _category )
        {
            if( IsInitialized ) return;

            m_NowCategory = _category;
            m_LoadTargetAssetIdentifierList = AddressableResourcesManager.Instance.GetAllIdentiferForCategory( m_NowCategory );

            NowState = State.LoadingPack;
            IsInitialized = false;
        }

        /// <summary>
        /// 言語別リソース切り替え完了時にリソース切り替え対象クラスを登録する
        /// </summary>
        /// <param name="_target"></param>
        public static void AddReplaceTarget( ILanguageAssetReplace _target )
        {
            if( m_Instance == null ) return;

            if( m_Instance.m_TargetReplaceList.Find( ( target ) => target == _target ) != null ) return;

            m_Instance.m_TargetReplaceList.Add( _target );
        }

        /// <summary>
        /// リソース切り替え対象クラスを解除する
        /// </summary>
        /// <param name="_target"></param>
        public static void RemoveRepralceTarget( ILanguageAssetReplace _target )
        {
            if( m_Instance == null ) return;

            m_Instance.m_TargetReplaceList.Remove( _target );
        }
        /// <summary>
        /// 言語別リソース切り替え完了時に画像切り替え対象を登録する
        /// </summary>
        /// <param name="_target"></param>
        public static void AddImageRepace( LanguageImageReplace _target )
        {
            if( m_Instance == null ) return;
            if( m_Instance.m_TargetImageReplaceList.Find( ( target ) => target == _target ) != null ) return;

            m_Instance.m_TargetImageReplaceList.Add( _target );
        }

        /// <summary>
        /// 画像切り替え対象を解除する
        /// </summary>
        /// <param name="_target"></param>
        public static void RemoveImageRepralce( LanguageImageReplace _target )
        {
            if( m_Instance == null ) return;

            m_Instance.m_TargetImageReplaceList.Remove( _target );
        }

        /// <summary>
        /// 言語を変更
        /// </summary>
        /// <param name="_category"></param>
        public void ChangeLanguage( AddressableCategory _category )
        {
            if( m_NowCategory == _category ) return;

            m_PrevCategory = m_NowCategory;
            m_NowCategory = _category;
            m_LoadTargetAssetIdentifierList = AddressableResourcesManager.Instance.GetAllIdentiferForCategory( m_NowCategory );

            foreach( LanguageObjectReplace objectReplace in m_ObjectReplaceList ) objectReplace.ReleaseAsset();

            NowState = State.LoadingPack;
        }

        /// <summary>
        /// メッセージのテキストを取得
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public string[] GetMessageText( int _index )
        {
            if( NowState != State.Ready ) return null;

            if( m_MessageTextList == null )
            {
#if UNITY_SWITCH
                TextAsset messageTextAsset = GetAsset<TextAsset>( LanguagePackCategory.MessageWindow_Switch, 0 );
#else
                TextAsset messageTextAsset = GetAsset<TextAsset>( LanguagePackCategory.MessageWindow_PC, 0 );
#endif
                m_MessageTextList = messageTextAsset.text.SplitCsv();
            }

            if( _index < 0 || _index >= m_MessageTextList.Length ) return null;

            return m_MessageTextList[ _index ];
        }
        
        /// <summary>
        /// メッセージのテキストを取得
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public string[] GetEventSceneText( int _index )
        {
            if( NowState != State.Ready ) return null;

            if( m_EventSceneTextList == null )
            {
                TextAsset eventSceneTextAsset = GetAsset<TextAsset>( LanguagePackCategory.EventScene, 0 );
                m_EventSceneTextList = eventSceneTextAsset.text.SplitCsv();
            }

            if( _index < 0 || _index >= m_EventSceneTextList.Length ) return null;

            return m_EventSceneTextList[ _index ];
        }

        /// <summary>
        /// Assetを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_index"></param>
        /// <returns></returns>
        public T GetAsset<T>( LanguagePackCategory _category, int _index ) where T : Object
        {
            if( NowState != State.Ready ) return null;
            if( _index < 0 ) return null;

            LanguageAssetPack assetPack = m_AssetPackList.Find( ( pack ) => pack.PackCategory == _category );

            if( assetPack == null )
            {
                Debug.LogError( "Missing LanguageAssetPack! Category ; " + _category.ToString() );
                return null;
            }

            if( _index >= assetPack.AssetList.Length ) return null;

            return assetPack.AssetList[ _index ] as T;
        }

        /// <summary>
        /// Spriteを取得
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="_isUseSpriteAtlas"></param>
        /// <returns></returns>
        public Sprite GetSprite( LanguagePackCategory _category, int _index )
        {
            if( NowState != State.Ready ) return null;
            if( _index < 0 ) return null;

            LanguageAssetPack assetPack = m_AssetPackList.Find( ( pack ) => pack.PackCategory == _category );

            if( assetPack == null )
            {
                Debug.LogError( "Missing LanguageAssetPack! Category ; " + _category.ToString() );
                return null;
            }

            if( _index >= assetPack.AssetList.Length ) return null;

            Texture2D texture = assetPack.AssetList[ _index ] as Texture2D;

            if( texture == null ) return null;

            return Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( 0.5f, 0.5f ) );
        }

        /// <summary>
        /// ゲームオブジェクトを複製
        /// </summary>
        /// <param name="_category"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public GameObject InstantateGameObject( LanguagePackCategory _category, int _index )
        {
            if( NowState != State.Ready ) return null;
            if( _index < 0 ) return null;

            foreach( LanguageObjectReplace objectReplace in m_ObjectReplaceList )
            {
                if( !objectReplace.IsReplaceAsset ) continue;
                if( objectReplace.Category != _category ) continue;
                if( objectReplace.AssetIndex != _index ) continue;

                return objectReplace.InstantateGameObject();
            }

            return null;
        }
    }
}