using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語別リソースをInspectorで指定するためのクラス
    /// </summary>
    [System.Serializable]
    public class LanguageAssetAssignment
    {
        /// <summary>言語別リソースの種類</summary>
        [SerializeField]
        private LanguagePackCategory m_PackCategory;
        /// <summary>言語別リソースの種類</summary>
        public LanguagePackCategory PackCategory => m_PackCategory;
        /// <summary>種類毎のAssetのインデックス</summary>
        [SerializeField]
        private int m_AssetIndex;
        /// <summary>種類毎のAssetのインデックス</summary>
        public int AssetIndex => m_AssetIndex;

#if UNITY_EDITOR
        [CustomPropertyDrawer( typeof( LanguageAssetAssignment ) )]
        public class LanguageAssetAssigmentDrawer : PropertyDrawer
        {            
            private const string PROPERY_NAME_PACK_CATEGORY = "m_PackCategory";
            private const string PROPERY_NAME_ASSET_INDEX = "m_AssetIndex";

            private const string LANGUAGE_PACK_DIRECTORY_PATH = "Assets/02_LanguageResources/LanguageResources/00_en/LanguagePack/";

            public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
            {
                EditorGUI.BeginProperty( _position, _label, _property );                
                SerializedProperty packCateogryProperty = _property.FindPropertyRelative( PROPERY_NAME_PACK_CATEGORY );
                SerializedProperty assetIndexProperty = _property.FindPropertyRelative( PROPERY_NAME_ASSET_INDEX );

                float width = _position.width;
                _position.width = width * 0.3f;

                EditorGUI.LabelField( _position, new GUIContent( _label.text ) );

                _position.x = width * 0.33f;
                _position.width = width * 0.3f;
                                
                packCateogryProperty.enumValueIndex = EditorGUI.Popup( _position, packCateogryProperty.enumValueIndex, packCateogryProperty.enumDisplayNames );

                LanguagePackCategory packCategory = Enum.Parse<LanguagePackCategory>( packCateogryProperty.enumNames[ packCateogryProperty.enumValueIndex ] );
                string[] filePathList = Directory.GetFiles( LANGUAGE_PACK_DIRECTORY_PATH, "*.asset" );

                foreach( string path in filePathList )
                {
                    LanguageAssetPack assetPack = AssetDatabase.LoadAssetAtPath<LanguageAssetPack>( path );

                    if( assetPack == null ) continue;
                    if( assetPack.PackCategory != packCategory ) continue;

                    string[] assetNameList = new string[ assetPack.AssetList.Length ];

                    for( int i = 0; i < assetPack.AssetList.Length; i++ )
                    {
                        assetNameList[ i ] = assetPack.AssetList[ i ].name;
                    }

                    _position.x = width * 0.63f;
                    _position.width = width * 0.4f;

                    assetIndexProperty.intValue = EditorGUI.Popup( _position, assetIndexProperty.intValue, assetNameList );

                    break;
                }

                EditorGUI.EndProperty();
            }
}
#endif
    }
}