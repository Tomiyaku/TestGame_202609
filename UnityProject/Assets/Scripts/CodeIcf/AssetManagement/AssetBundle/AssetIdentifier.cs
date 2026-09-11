using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using CodeIcf.Extensions;

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// Inspector上でAssetにアクセスするAsset識別子(uint型)を設定するためめのクラス<br></br>
    /// </summary>
    /// <remarks>
    /// AssetにアクセスするためのAsset識別子がuint型なので、Inspectorにuint型を表示するだけではどのAssetか判別が難しいので<br></br>
    /// カテゴリとAsset名をPopupで選択してAsset識別子を設定するためのクラス<br></br>
    /// <see cref="AssetBundleCategory"/>とインデックスでuint型のAsset識別子を作成したり、逆に分離する処理もこのクラスで行う
    /// </remarks>
    [System.Serializable]
    public class AssetIdentifier
    {
        /// <summary>このクラスが保持しているIdentifierの状態</summary>
        public enum ResultCode : ushort
        {
            /// <summary>未設定(初期状態)</summary>
            NotSet = 0,
            /// <summary>有効な値</summary>
            Success = 0x1 << 15,
            /// <summary>AdressableAssetのIdentiferなので無効</summary>
            TypeError_Adressable  = 0x1 << 11,
        }

        /// <summary>AssrtBundleのカテゴリ</summary>
        [SerializeField]
        private AssetBundleCategory m_Category;
        /// <summary>AssrtBundleのカテゴリ</summary>
        public AssetBundleCategory Category => m_Category;
        /// <summary>カテゴリ毎の設定したAssetのインデックス</summary>
        [SerializeField]
        private uint m_Index;
        /// <summary>カテゴリ毎の設定したAssetのインデックス</summary>
        public uint Index => m_Index;
        /// <summary>AssetBundleからAssetを取得するためのAsset識別子</summary>
        public uint Identifier => CreateIdentifier( m_Category, m_Index );

        /// <summary>このクラスの状態</summary>
        public ResultCode ResultState { get; private set; } = ResultCode.NotSet;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        public AssetIdentifier( uint _identifier )
        {
            SetIdentifer( _identifier );
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のインデックス</param>
        public AssetIdentifier( AssetBundleCategory _category, uint _index )
        {
            SetIdentifer( _category, _index );
        }

        /// <summary>
        /// このクラスが出力するIdentifierと等しいか
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns></returns>
        public bool IsEquals( uint _identifier )
        {
            return Identifier == _identifier;
        }

        /// <summary>
        /// Asset識別子を設定
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        public ResultCode SetIdentifer( uint _identifier )
        {
            ResultState = ResultCode.NotSet;

            if( ( _identifier & ( AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES ) ) == AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES )
            {
                Debug.LogError( "Identifier Type Not Match! Current Identifier : " + _identifier + " Type of Adressables" );
                ResultState = ResultCode.TypeError_Adressable;
            }
            else
            {
                DivideIdentifier( _identifier, out m_Category, out m_Index );

                ResultState = ResultCode.Success;
            }

            return ResultState;
        }

        /// <summary>
        /// Asset識別子を設定
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        public ResultCode SetIdentifer( AssetBundleCategory _category, uint _index )
        {
            ResultState = ResultCode.NotSet;

            m_Category = _category;
            m_Index = _index;

            ResultState = ResultCode.Success;

            return ResultCode.Success;
        }

        /// <summary>
        /// カテゴリとint型のIDを組み合わせてuint型のIdentifierを返す
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns></returns>
        public static uint CreateIdentifier( AssetBundleCategory _category, uint _index = 0 )
        {
            return ( _category.ToUint() << AssetCommonDefine.INDEX_BIT_SIZE ) + _index;
        }

        /// <summary>
        /// カテゴリとint型のIDを組み合わせてIdentifierを返す
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>Asset識別子</returns>
        public static uint CreateIdentifier( AssetBundleCategory _category, int _index )
        {
            return CreateIdentifier( _category, ( uint )_index );
        }

        /// <summary>
        /// uint型のIdentifierからカテゴリを取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>カテゴリ</returns>
        public static AssetBundleCategory ConvertCategoty( uint _identifier )
        {
            if( ( _identifier & ( AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES ) ) == AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES )
            {
                Debug.LogError( "Identifier Type Not Match! Current Identifier : " + _identifier + " Type of Adressables" );

                return 0;
            }

            return ( _identifier >> AssetCommonDefine.INDEX_BIT_SIZE ).ToEnum<AssetBundleCategory>();
        }

        /// <summary>
        /// Asset識別子をカテゴリとAssetインデックスに分解
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">Assetインデックス</param>
        public static void DivideIdentifier( uint _identifier, out AssetBundleCategory _category, out uint _index )
        {
            _category = ConvertCategoty( _identifier );
            _index = _identifier - ( _category.ToUint() << AssetCommonDefine.INDEX_BIT_SIZE );
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspectorでこのクラスを表示させるためのクラス
    /// </summary>
    [CustomPropertyDrawer( typeof( AssetIdentifier ) )]
    public class AssetIdentifierDrawer : PropertyDrawer
    {
        private const string PROPARTY_NAME_CATEGORY = "m_Category";
        private const string PROPARTY_NAME_INDEX = "m_Index";

        public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
        {
            EditorGUI.BeginProperty( _position, _label, _property );

            float defaultX = _position.x;
            float labelwidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 35f;
            float width = _position.width;

            SerializedProperty categoryProperty = _property.FindPropertyRelative( PROPARTY_NAME_CATEGORY );
            SerializedProperty indexProperty = _property.FindPropertyRelative( PROPARTY_NAME_INDEX );
            AssetBundleCategory nowCategory = categoryProperty.enumValueIndex.ToEnum<AssetBundleCategory>();

            _position.width = width * 0.25f;

            EditorGUI.LabelField( _position, new GUIContent( _label.text, "値 : " + AssetIdentifier.CreateIdentifier( nowCategory, indexProperty.intValue ).ToString() ) );

            _position.x = width * 0.26f;
            _position.width = width * 0.35f;

            AssetBundleCategory nextCategory = ( AssetBundleCategory )EditorGUI.EnumPopup( _position, "カテゴリ", nowCategory );
            
            if( nowCategory != nextCategory )
            {
                indexProperty.intValue = 0;                
                categoryProperty.enumValueIndex = nextCategory.ToInt();
            }

            _position.x += width * 0.36f;
            _position.width = width * 0.4f;

            EditorGUIUtility.labelWidth = 45f;

            string[] assetNameList = null;
            string[] fileList = Directory.GetFiles( AssetBundleDefine.ASSET_BUNDLE_INFO_DIRECTORY_PATH, "*" + ExtensionDefine.ASSET, SearchOption.TopDirectoryOnly );

            for( int i = 0; i < fileList.Length; i++ )
            {
                AssetBundleInfo info = AssetDatabase.LoadAssetAtPath<AssetBundleInfo>( fileList[ i ] );

                if( info.Category == categoryProperty.intValue.ToEnum<AssetBundleCategory>() )
                {
                    assetNameList = new string[ info.AssetDataList.Length ];

                    for( int j = 0; j < assetNameList.Length; j++ ) assetNameList[ j ] = info.AssetDataList[ j ].AssetName;
                }
            }

            if( assetNameList == null )
            {
                Debug.LogWarning( "Missing AssetDefineInfoList! Category : " + ( uint )categoryProperty.intValue + " Index : " + indexProperty.intValue );
            }
            else if( assetNameList.Length <= 0 )
            {
                Debug.LogWarning( "Nothing AssetBundleDefineInfo File! Category : " + ( uint )categoryProperty.intValue );
            }
            else
            {
                indexProperty.intValue = EditorGUI.Popup( _position, "Asset名", indexProperty.intValue, assetNameList );
            }

            EditorGUIUtility.labelWidth = labelwidth;
            EditorGUI.EndProperty();
        }
    }
#endif

}