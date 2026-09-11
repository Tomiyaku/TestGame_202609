using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using CodeIcf.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// Inspector上でAssetにアクセスするAsset識別子(uint型)を設定するためのクラス<br></br>
    /// </summary>
    /// <remarks>
    /// AssetにアクセスするためのAsset識別子がuint型なので、Inspectorにuint型を表示するだけではどのAssetか判別が難しいので<br></br>
    /// カテゴリとAsset名をPopupで選択してAsset識別子を設定するためのクラス<br></br>
    /// <see cref="AddressableCategory"/>とインデックスでuint型のAsset識別子を作成したり、逆に分離する処理もこのクラスで行う
    /// </remarks>
    [System.Serializable]
    public class AddressableIdentifier 
    {
        /// <summary>このクラスが保持しているIdentifierの状態</summary>
        public enum ResultCode : ushort
        {
            /// <summary>未設定(初期状態)</summary>
            NotSet = 0,
            /// <summary>有効な値</summary>
            Success = 0x1 << 15,
            /// <summary>AdressableAssetのIdentiferなので無効</summary>
            TypeError_AssetBundle = 0x1 << 11,
        }

        /// <summary>AssrtBundleのカテゴリ</summary>
        [SerializeField]
        private AddressableCategory m_Category;
        /// <summary>AssrtBundleのカテゴリ</summary>
        public AddressableCategory Category => m_Category;
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
        public AddressableIdentifier( uint _identifier )
        {
            SetIdentifer( _identifier );
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のインデックス</param>
        public AddressableIdentifier( AddressableCategory _category, int _index )
        {
            SetIdentifer( _category, ( uint )_index );
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のインデックス</param>
        public AddressableIdentifier( AddressableCategory _category, uint _index )
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

            if( ( _identifier & ( AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES ) ) != AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES )
            {//AddressableAsset用のIdentifierではない場合
                Debug.LogError( "Identifier Type Not Match! Current Identifier : " + _identifier + " Type of AssetBundle" );
                ResultState = ResultCode.TypeError_AssetBundle;                
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
        public ResultCode SetIdentifer( AddressableCategory _category, uint _index )
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
        public static uint CreateIdentifier( AddressableCategory _category, uint _index = 0 )
        {
            uint categoryValue = _category.ToUint();

            return AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES +  ( categoryValue << AssetCommonDefine.INDEX_BIT_SIZE ) + _index;
        }

        /// <summary>
        /// カテゴリとint型のIDを組み合わせてIdentifierを返す
        /// </summary>
        /// <param name="_category">カテゴリ</param>
        /// <param name="_index">カテゴリ毎のAssetのインデックス</param>
        /// <returns>Asset識別子</returns>
        public static uint CreateIdentifier( AddressableCategory _category, int _index )
        {
            return CreateIdentifier( _category, ( uint )_index );
        }

        /// <summary>
        /// uint型のIdentifierからカテゴリを取得
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <returns>カテゴリ</returns>
        public static AddressableCategory ConvertCategoty( uint _identifier )
        {
            if( ( _identifier & AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES ) != AssetCommonDefine.IDENTIFIER_FLAG_ADRESSABLES )
            {
                Debug.LogError( "Identifier Type Not Match! Current Identifier : " + _identifier + " Type of AssetBundle" );

                return 0;
            }

            uint categoryValue = ( _identifier & ~AssetCommonDefine.IDENTIFIER_FLAG_RANGE ) >> AssetCommonDefine.INDEX_BIT_SIZE;

            return categoryValue.ToEnum<AddressableCategory>();
        }

        /// <summary>
        /// Asset識別子をカテゴリとAssetインデックスに分解
        /// </summary>
        /// <param name="_identifier">Asset識別子</param>
        /// <param name="_outCategory">カテゴリ</param>
        /// <param name="_outIndex">Assetインデックス</param>
        public static void DivideIdentifier( uint _identifier, out AddressableCategory _outCategory, out uint _outIndex )
        {
            _outCategory = ConvertCategoty( _identifier );
            _outIndex = _identifier & ~( AssetCommonDefine.IDENTIFIER_FLAG_RANGE + AssetCommonDefine.IDENTIFER_CATEGORY_RANGE );
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspectorでこのクラスを表示させるためのクラス
    /// </summary>
    [CustomPropertyDrawer( typeof( AddressableIdentifier ) )]
    public class AddressableIdentifierDrawer : PropertyDrawer
    {
        private const string PROPARTY_NAME_CATEGORY = "m_Category";
        private const string PROPARTY_NAME_INDEX = "m_Index";

        private List<AddressableInfo> m_AddressableInfoList = new  List<AddressableInfo>();

        public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
        {
            EditorGUI.BeginProperty( _position, _label, _property );
                       
            float labelwidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50f;
            float width = _position.width;

            SerializedProperty categoryProperty = _property.FindPropertyRelative( PROPARTY_NAME_CATEGORY );
            SerializedProperty indexProperty = _property.FindPropertyRelative( PROPARTY_NAME_INDEX );
            AddressableCategory nowCategory = categoryProperty.enumValueIndex.ToEnum<AddressableCategory>();

            _position.width = width * 0.25f;

            EditorGUI.LabelField( _position, new GUIContent( _label.text, "値 : " + AddressableIdentifier.CreateIdentifier( nowCategory, indexProperty.intValue ).ToString() ) );

            _position.x = width * 0.26f;
            _position.width = width * 0.35f;

            AddressableCategory nextCategory = ( AddressableCategory )EditorGUI.EnumPopup( _position, "カテゴリ", nowCategory );

            if( nowCategory != nextCategory )
            {
                indexProperty.intValue = 0;
                categoryProperty.enumValueIndex = nextCategory.ToInt();
            }

            _position.x += width * 0.36f;
            _position.width = width * 0.4f;           

            string[] assetNameList = null;

            GeAddressableInfoFileList();

            foreach( AddressableInfo info in m_AddressableInfoList )
            {
                if( info.Category == categoryProperty.intValue.ToEnum<AddressableCategory>() )
                {
                    assetNameList = new string[ info.AdressList.Length ];

                    for( int j = 0; j < assetNameList.Length; j++ ) assetNameList[ j ] = info.AdressList[ j ].AssetName;

                    break;
                }
            }

            if( assetNameList == null || assetNameList.Length <= 0 )
            {
                Debug.LogWarning( "Nothing AddressableInfo TargetAsset! Category : " + ( uint )categoryProperty.intValue );
            }
            else
            {
                indexProperty.intValue = EditorGUI.Popup( _position, "Asset名", indexProperty.intValue, assetNameList );
            }

            EditorGUIUtility.labelWidth = labelwidth;
            EditorGUI.EndProperty();
        }

        private void GeAddressableInfoFileList()
        {
            if(  m_AddressableInfoList.Count > 0 ) return;

            AsyncOperationHandle<IList<AddressableInfo>> handle = Addressables.LoadAssetsAsync<AddressableInfo>( AddressableDefine.ADRESSABLES_INFO_NAME, null );
            handle.WaitForCompletion();

            foreach( AddressableInfo info in handle.Result )
            {
                m_AddressableInfoList.Add( info );
            }
        }

    }
#endif
}