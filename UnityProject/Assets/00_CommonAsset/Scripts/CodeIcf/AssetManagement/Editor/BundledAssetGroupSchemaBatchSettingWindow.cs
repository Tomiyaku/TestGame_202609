using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace CodeIcf.AssetManagement.AdressableEditor
{
    public class BundledAssetGroupSchemaBatchSettingWindow : EditorWindow
    {
        private readonly string TARGET_DIRECTORY_PATH = "Assets/AddressableAssetsData/AssetGroups/Schemas/";
        public BundledAssetGroupSchema.BundleCompressionMode m_CompressionMode = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        public BundledAssetGroupSchema.BundleNamingStyle m_NamingStyle = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;


        [MenuItem( "Tools/BundledAssetGroupSchema一括設定変更ウインドウを表示" )]
        static void OpenWindow()
        {
            GetWindow<BundledAssetGroupSchemaBatchSettingWindow>( "BundledAssetGroupSchemaBatchSettingWindow" );
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField( "対象のパス : " + TARGET_DIRECTORY_PATH );

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject( target );
            SerializedProperty compresstionProperty = so.FindProperty( "m_CompressionMode" );
            SerializedProperty namingProperty = so.FindProperty( "m_NamingStyle" );

            m_CompressionMode = ( BundledAssetGroupSchema.BundleCompressionMode )EditorGUILayout.EnumPopup( "Compression", m_CompressionMode );
            m_NamingStyle = ( BundledAssetGroupSchema.BundleNamingStyle )EditorGUILayout.EnumPopup( "BundleNaming", m_NamingStyle );

            GUILayout.Space( 50 );

            if( GUILayout.Button( "一括設定変更開始" ) )
            {
                BatchSetting();
            }
        }

        private void BatchSetting()
        {
            string[] filePathList = Directory.GetFiles( TARGET_DIRECTORY_PATH, "*.asset", SearchOption.AllDirectories );

            foreach( string path in filePathList )
            {
                string assetName = Path.GetFileNameWithoutExtension( path );
                string extention = Path.GetExtension( path );

                if( extention != ".asset" ) continue;

                Object obj = AssetDatabase.LoadAssetAtPath<Object>( path );

                if( obj == null ) continue;
                if( !( obj is BundledAssetGroupSchema schema ) ) continue;               

                schema.Compression = m_CompressionMode;
                schema.BundleNaming = m_NamingStyle;

                EditorUtility.SetDirty( schema );
            }

            AssetDatabase.SaveAssets();
        }
    }
}