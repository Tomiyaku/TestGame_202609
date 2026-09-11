using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.Extensions;
using AssetData = CodeIcf.AssetManagement.AdressableManagement.AddressableInfo.AssetData;

namespace CodeIcf.AssetManagement.AdressableEditor
{
    [CreateAssetMenu( fileName = "AdressableInfoCreater", menuName = "ScriptableObjects/Create AdressableInfoCreater File" )]
    public class AddressableInfoCreater : ScriptableObject
    {
        /// <summary>更新時に上書きの対象になる<see cref="AddressableInfo"/></summary>
        public AddressableInfo OverwriteTarget;
        /// <summary>種類識別</summary>
        public AddressableCategory Category;
        /// <summary>対象のAssetファイル一覧</summary>
        public Object[] TargetAssetList;

        /// <summary>
        /// <see cref="AddressableInfo"/>を作成
        /// </summary>
        /// <param name="_isOverride">trueの場合は上書き</param>
        public void CreateAddressablesDefineInfo( bool _isOverride )
        {
            if( OverwriteTarget == null || !_isOverride )
            {//AssetBundleDefineInfoがまだない or 新規作成の場合は作成
                OverwriteTarget = CreateInstance<AddressableInfo>();
            }

            OverwriteTarget.Category = Category;

            AssetDatabase.Refresh();

            AddressableAssetGroup group = FindGroupAndCreate( Category.ToString() );

            //対象のAssetからAssetDefineInfoを作成
            if( TargetAssetList != null && TargetAssetList.Length > 0 )
            {
                AssetData[] adressList = new AssetData[ TargetAssetList.Length ];

                for( int i = 0; i < TargetAssetList.Length; i++ )
                {
                    CheckAddressableEntry( TargetAssetList[ i ], group, null, out string path );

                    string assetName = Path.GetFileNameWithoutExtension( path );
                    string extention = Path.GetExtension( path );

                    // TODO: PrimaryKeyの値(アドレス)が即座に更新されない模様なので使用せず、ファイルパスをアドレスとして設定
                    //AssetReference assetReference = new AssetReference( guid );
                    //string primaryKey = Addressables.LoadResourceLocationsAsync( assetReference ).WaitForCompletion().First()?.PrimaryKey;
                    bool isScene = extention == ExtensionDefine.SCENE;

                    adressList[ i ] = new AssetData() { AssetName = assetName, Address = path, IsSceneAsset = isScene };
                }

                OverwriteTarget.AdressList = adressList;
            }
        }

        public bool CheckAssetStatus()
        {
            bool result = true;

            for( int i = 0; i < TargetAssetList.Length; i++ )
            {
                if( TargetAssetList[ i ] == null )
                {
                    Debug.LogError( i + " : Assetがありません" );
                    result = false;
                    continue;
                }

                if( !AssetDatabase.IsMainAsset( TargetAssetList[ i ] ) )
                {
                    Debug.LogError( i + " : 対象のAssetがMainAssetではありません " + TargetAssetList[ i ].name );
                    result = false;
                    continue;
                }

                string path = AssetDatabase.GetAssetPath( TargetAssetList[ i ] );

                if( string.IsNullOrEmpty( path ) )
                {
                    Debug.LogError( i + " : 対象のAssetのファイルパスが取得できません " + TargetAssetList[ i ].name );
                    result = false;
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// 指定名のグループの検索と、存在しない場合の新規作成
        /// </summary>
        /// <param name="_groupName"></param>
        /// <returns></returns>
        public AddressableAssetGroup FindGroupAndCreate( string _groupName )
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = null;

            if( settings != null )
            {
                group = settings.FindGroup( _groupName );

                if( group == null )
                {
                    group = settings.CreateGroup( _groupName, false, false, true, null, typeof( ContentUpdateGroupSchema ), typeof( BundledAssetGroupSchema ) );
                }
            }
            else
            {
                Debug.LogError( "Missing AddressableAssetSettingsDefaultObject.Settings" );
            }

            return group;
        }

        /// <summary>
        /// 指定のAssetがAddressableが有効になってない場合に有効に変更してラベルを追加する
        /// </summary>
        /// <param name="_source">対象のAsset</param>
        /// <param name="_group"><see cref="AddressableAssetGroup"/></param>
        /// <param name="_label">ラベル</param>
        /// <param name="_path">_sourceへのファイルパス 読み込みの際のアドレスに設定される</param>
        private void CheckAddressableEntry( Object _source, AddressableAssetGroup _group, string _label, out string _path )
        {
            _path = null;

            if( _source == null || !AssetDatabase.Contains( _source ) ) return;

            _path = AssetDatabase.GetAssetPath( _source );
            string guid = AssetDatabase.AssetPathToGUID( _path );

            AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = addressableAssetSettings.FindAssetEntry( guid );

            if( entry == null ) entry = addressableAssetSettings.CreateOrMoveEntry( guid, _group );

            entry.SetAddress( _path );

            if( !string.IsNullOrEmpty( _label ) ) entry.SetLabel( _label, true, true );

            EditorUtility.SetDirty( _source );

            addressableAssetSettings.SetDirty( AddressableAssetSettings.ModificationEvent.EntryModified, entry, true );
        }

        /// <summary>
        /// AddressableAsset定義ファイルのAddressable関連設定の確認
        /// </summary>
        public void CheckAddressablesEntyForDeifneInfoFIle()
        {
            AddressableAssetGroup group = FindGroupAndCreate( AddressableDefine.ADRESSABLES_INFO_NAME );
            CheckAddressableEntry( OverwriteTarget, group, AddressableDefine.ADRESSABLES_INFO_NAME, out _ );
        }
    }

    [CustomEditor( typeof( AddressableInfoCreater ) )]
    public class AddressablesDefineInfoCreaterEditor : Editor
    {
        private AddressableInfoCreater m_Cretator = null;
        private ReorderableList m_AssetList;
        private SerializedProperty m_AssetDefineListProperty;

        private void OnEnable()
        {
            m_Cretator = target as AddressableInfoCreater;

            m_AssetDefineListProperty = serializedObject.FindProperty( "TargetAssetList" );
            m_AssetList = new ReorderableList( serializedObject, m_AssetDefineListProperty );

            m_AssetList.drawHeaderCallback = ( rect ) =>
            {
                DropAreaGUI( rect );
            };

            m_AssetList.elementHeightCallback = ( index ) =>
            {
                SerializedProperty property = m_AssetDefineListProperty.GetArrayElementAtIndex( index );

                return EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight( property, true );
            };

            m_AssetList.drawElementCallback = ( rect, index, isActive, isFocused ) =>
            {
                SerializedProperty property = m_AssetDefineListProperty.GetArrayElementAtIndex( index );

                rect.x += 10;
                rect.width -= 10;

                EditorGUI.PropertyField( rect, property, new GUIContent( "Index : " + index.ToString() ) );
            };

            m_AssetList.drawFooterCallback = ( rect ) =>
            {
                if( m_AssetList.count > 0 )
                {
                    Rect leftRect = new Rect( rect );
                    leftRect.size = new Vector2( rect.width * 0.4f, rect.height );

                    Rect rightRect = new Rect( rect );
                    rightRect.position = new Vector2( rect.width * 0.8f, rect.position.y );
                    rightRect.size = new Vector2( rect.width * 0.2f, rect.height );

                    if( m_AssetList.index >= 0 )
                    {
                        if( GUI.Button( leftRect, "選択している項目を削除" ) )
                        {
                            m_AssetDefineListProperty.DeleteArrayElementAtIndex( m_AssetList.index );
                        }
                    }

                    if( GUI.Button( rightRect, "全て削除" ) )
                    {
                        m_AssetDefineListProperty.ClearArray();
                    }
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //base.OnInspectorGUI();
            //GUILayout.Space( 100 );

            using( new GUILayout.VerticalScope( GUI.skin.box ) )
            {
                if( GUILayout.Button( "新規保存" ) )
                {
                    Save( false );
                }

                if( m_Cretator.OverwriteTarget != null )
                {
                    GUILayout.Space( 10 );
                    if( GUILayout.Button( "上書き保存" ) )
                    {
                        Save( true );
                    }

                    if( m_AssetDefineListProperty.arraySize > 0 )
                    {
                        GUILayout.Space( 20 );

                        if( GUILayout.Button( "Identifier定義クラスファイルを作成" ) ) CreateIdentifierClassFile();

                        GUILayout.Space( 10 );

                        if( GUILayout.Button( "Identifier定義クラスをクリップボードにコピー" ) ) CopyIdentifierClass();

                        GUILayout.Space( 10 );

                        if( GUILayout.Button( "Asset名一覧をクリップボードにコピー" ) ) CopyAssetNameList();
                    }
                }
            }

            GUILayout.Space( 20 );

            m_Cretator.OverwriteTarget = ( AddressableInfo )EditorGUILayout.ObjectField( "上書き対象のファイル", m_Cretator.OverwriteTarget, typeof( AddressableInfo ), false );

            GUILayout.Space( 10 );

            //AddressablesのカテゴリをPopupで選択
            m_Cretator.Category = ( AddressableCategory )EditorGUILayout.EnumPopup( "カテゴリ", m_Cretator.Category );

            GUILayout.Space( 10 );

            if( m_AssetList != null ) m_AssetList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty( m_Cretator );
        }

        /// <summary>
        /// AssetをドラッグアンドドロップでAssetを追加するエリア
        /// </summary>
        private void DropAreaGUI( Rect _rect )
        {
            GUIStyle style = new GUIStyle( GUI.skin.box );
            style.alignment = TextAnchor.MiddleLeft;

            //GUI.contentColor = Color.white;
            //GUI.backgroundColor = Color.white;
            GUI.Box( _rect, "対象のAsset一覧[" + m_AssetDefineListProperty.arraySize + "]\t(ここにドラッグ&ドロップでファイルを追加)", style );

            Event evt = Event.current;

            switch( evt.type )
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if( !_rect.Contains( evt.mousePosition ) ) return;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if( evt.type == EventType.DragPerform )
                        {
                            DragAndDrop.AcceptDrag();

                            List<Object> addList = new List<Object>();

                            foreach( Object obj in DragAndDrop.objectReferences )
                            {
                                if( obj == null )
                                {
                                    Debug.LogError( "Assetが見つかりません" );
                                    continue;
                                }

                                if( !AssetDatabase.IsMainAsset( obj ) )
                                {
                                    Debug.LogError( obj.name + " : MainAssetではありません" );
                                    continue;
                                }

                                string path = AssetDatabase.GetAssetPath( obj );

                                if( string.IsNullOrEmpty( path ) )
                                {
                                    Debug.LogError( obj.name + " : ファイルパスが取得できません" );
                                    continue;
                                }

                                addList.Add( obj );
                            }

                            if( addList.Count > 0 )
                            {//先頭に既存のリストを追加して差し替え
                                if( m_AssetDefineListProperty.arraySize > 0 ) addList.InsertRange( 0, m_Cretator.TargetAssetList );

                                m_Cretator.TargetAssetList = addList.ToArray();
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="_isOverwrite">trueなら上書き</param>
        private void Save( bool _isOverwrite )
        {
            if( !m_Cretator.CheckAssetStatus() ) return;

            if( _isOverwrite )
            {
                m_Cretator.CreateAddressablesDefineInfo( _isOverwrite );
            }
            else
            {
                string saveFilePath = EditorUtility.SaveFilePanelInProject( "AssetBundleDefineInfoの保存", "AssetBundleDefineInfo", "asset", "" );

                if( !string.IsNullOrEmpty( saveFilePath ) )
                {
                    m_Cretator.CreateAddressablesDefineInfo( _isOverwrite );
                    AssetDatabase.CreateAsset( m_Cretator.OverwriteTarget, saveFilePath );
                }
            }

            m_Cretator.CheckAddressablesEntyForDeifneInfoFIle();

            EditorUtility.SetDirty( m_Cretator.OverwriteTarget );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool CreateIdentifierClassFile()
        {
            m_Cretator.CreateAddressablesDefineInfo( true );

            AddressableInfo defineInfo = m_Cretator.OverwriteTarget;
            string className = defineInfo.name + "IdentifierDefine";

            string path = EditorUtility.SaveFilePanelInProject( "定義ファイルの保存", className, "cs", "" );

            if( !string.IsNullOrEmpty( path ) )
            {
                string outputText = CreateDefineFileContents();
                File.WriteAllText( path, outputText );

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return true;
            }

            return false;
        }

        private void CopyIdentifierClass()
        {
            EditorGUIUtility.systemCopyBuffer = CreateDefineFileContents();
        }

        private string CreateDefineFileContents()
        {
            AddressableInfo defineInfo = m_Cretator.OverwriteTarget;

            string categoryText = nameof( AddressableCategory ) + "." + defineInfo.Category.ToString();
            string className = defineInfo.name + "IdentifierDefine";
            string outputText = $"namespace CodeIcf.AssetManagement.Addressable\n{{\n\tpublic static class {className}\n\t{{\n";

            for( int i = 0; i < defineInfo.AdressList.Length; i++ )
            {
                AssetData info = defineInfo.AdressList[ i ];
                string assetName = info.AssetName.Replace( " ", "_" );
                outputText += $"\t\tpublic static readonly uint {assetName.ToUpperSnakeCase()} =  AddressableIdentifier.CreateIdentifier(  {categoryText}, {i} );\n";
            }

            outputText += "\t}\n}\n";

            return outputText;
        }

        private void CopyAssetNameList()
        {
            EditorGUIUtility.systemCopyBuffer = CreateAssetNameList();
        }


        private string CreateAssetNameList()
        {
            AddressableInfo defineInfo = m_Cretator.OverwriteTarget;
            string outputText = "";

            for( int i = 0; i < defineInfo.AdressList.Length; i ++ )
            {
                AssetData info = defineInfo.AdressList[ i ];
                string assetName = info.AssetName;
                outputText += $"\"{ assetName}\",\n";
            }

            return outputText;
        }
    }
}