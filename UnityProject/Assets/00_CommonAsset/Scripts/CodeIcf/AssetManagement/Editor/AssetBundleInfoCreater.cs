using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using CodeIcf.AssetManagement.AssetBundleManagement;
using AssetData = CodeIcf.AssetManagement.AssetBundleManagement.AssetBundleInfo.AssetData;

namespace CodeIcf.AssetManagement.AssetBundleManagementEditor
{
    /// <summary>
    /// <see cref="AssetBundleInfo"/>を作成するためのScriptableObject
    /// </summary>
    [CreateAssetMenu( fileName = "AssetBundleInfoCreater", menuName = "ScriptableObjects/Create AssetBundleInfoCreater File" )]
    public class AssetBundleInfoCreater : ScriptableObject
    {
        /// <summary>更新時に上書きの対象になる<see cref="AssetBundleInfo"/></summary>
        public AssetBundleInfo OverwriteTarget;
        /// <summary>種類識別</summary>
        public AssetBundleCategory Category;
        /// <summary>AssetBundle名</summary>
        public string AssetBundleName;
        /// <summary>このAssetBundleに含まれるAssetを使用する場合に読み込みが必要なAssetBundleのカテゴリ</summary>
        public AssetBundleCategory[] RequiredCategoryList;
        /// <summary>体験版のみに使用するAssetBundleかのフラグ</summary>
        public bool IsTrialAssetBundle;
        /// <summary>trueの場合にゲーム終了以外でAssetの破棄を行わなくなる</summary>
        public bool IsNotRelease;
        /// <summary>追加コンテンツ用のAssetBundleかのフラグ</summary>
        public bool IsAddOnContent;
        /// <summary>追加コンテンツ名</summary>
        public string AocName;
        /// <summary>追加コンテンツのインデックス</summary>
        public int AocIndex;
        /// <summary>対象のAssetファイル一覧</summary>
        public Object[] TargetAssetList;

        /// <summary>
        /// <see cref="AssetBundleInfo"/>を作成
        /// </summary>
        /// <param name="_isOverride">trueの場合は上書き</param>
        public void CreateAssetBundleDefineInfo( bool _isOverride )
        {
            if( OverwriteTarget == null || !_isOverride )
            {//AssetBundleDefineInfoがまだない or 新規作成の場合は作成
                OverwriteTarget = CreateInstance<AssetBundleInfo>();
            }

            OverwriteTarget.Category = Category;
            OverwriteTarget.AssetBundleName = AssetBundleName;
            OverwriteTarget.RequiredCategoryList = RequiredCategoryList;
            OverwriteTarget.IsTrialAssetBundle = IsTrialAssetBundle;
            OverwriteTarget.IsNotRelease = IsNotRelease;
            OverwriteTarget.IsAddOnContent = IsAddOnContent;
            OverwriteTarget.AocName = AocName;
            OverwriteTarget.AocIndex = AocIndex;

            //対象のAssetからAssetDefineInfoを作成
            if( TargetAssetList != null && TargetAssetList.Length > 0 )
            {
                AssetData[] assetInfoList = new AssetData[ TargetAssetList.Length ];

                for( int i = 0; i < TargetAssetList.Length; i++ )
                {
                    string path = AssetDatabase.GetAssetPath( TargetAssetList[ i ] );
                    string assetName = Path.GetFileNameWithoutExtension( path );
                    string extention = Path.GetExtension( path );

                    path = path.Remove( 0, AssetBundleDefine.ASSET_FILE_DIRECTRY_PATH.Length );

                    int index = path.IndexOf( "/" + assetName + extention );

                    if( index > 0 ) path = path.Remove( index );

                    assetInfoList[ i ] = new AssetData() { AssetName = assetName, Path = path, Extention = extention };
                }

                OverwriteTarget.AssetDataList = assetInfoList;
            }
        }

        public bool CheckAssetStatus()
        {
            bool result = true;

            if( TargetAssetList == null ) return true;

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
                    continue;
                }
            }

            return result;
        }
    }

    [CustomEditor( typeof( AssetBundleInfoCreater ) )]
    public class AssetBundleDefineInfoCreaterEditor : Editor
    {
        private AssetBundleInfoCreater m_Cretator = null;
        private ReorderableList m_AssetList;
        private SerializedProperty m_AssetDefineListProperty;

        private static string[] m_AssetBundleNameList = null;
        private static int[] m_AssetBundleNameIndexList = null;

        private void OnEnable()
        {
            m_Cretator = target as AssetBundleInfoCreater;

            if( m_AssetBundleNameList == null ) UpdateAssetBundleNameList();

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

        /// <summary>
        /// AssetBundleName一覧を更新
        /// </summary>
        private void UpdateAssetBundleNameList()
        {
            m_AssetBundleNameList = AssetDatabase.GetAllAssetBundleNames();
            m_AssetBundleNameIndexList = new int[ m_AssetBundleNameList.Length ];

            for( int i = 0; i < m_AssetBundleNameIndexList.Length; i++ ) m_AssetBundleNameIndexList[ i ] = i;
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
                    }
                }
            }

            GUILayout.Space( 20 );

            m_Cretator.OverwriteTarget = ( AssetBundleInfo )EditorGUILayout.ObjectField( "上書き対象のファイル", m_Cretator.OverwriteTarget, typeof( AssetBundleInfo ), false );

            GUILayout.Space( 10 );

            //AssetBundleのカテゴリをPopupで選択
            m_Cretator.Category = ( AssetBundleCategory )EditorGUILayout.EnumPopup( "カテゴリ", m_Cretator.Category );

            //Assetbundle名を現在設定されている全てのAssetbundle nameからPopupで選択
            int nameIndex = Mathf.Clamp( System.Array.FindIndex( m_AssetBundleNameList, ( string abn ) => abn == m_Cretator.AssetBundleName ), 0, m_AssetBundleNameList.Length - 1 );
            nameIndex = EditorGUILayout.Popup( "AssetBundle名", nameIndex, m_AssetBundleNameList );
            m_Cretator.AssetBundleName = m_AssetBundleNameList[ nameIndex ];

            if( GUILayout.Button( "AssetBundleName一覧を更新" ) )
            {//Assetbundle nameのリストを更新
                UpdateAssetBundleNameList();
            }

            SerializedProperty requiredListProperty = serializedObject.FindProperty( "RequiredCategoryList" );
            EditorGUILayout.PropertyField( requiredListProperty, new GUIContent( "必須カテゴリ" ) );

            m_Cretator.IsTrialAssetBundle = EditorGUILayout.Toggle( "体験版専用AssetBundle", m_Cretator.IsTrialAssetBundle );
            m_Cretator.IsNotRelease = EditorGUILayout.Toggle( "Assetを解放しない", m_Cretator.IsNotRelease );

            m_Cretator.IsAddOnContent = EditorGUILayout.Toggle( "追加コンテンツ用AssetBundle", m_Cretator.IsAddOnContent );

            if( m_Cretator.IsAddOnContent )
            {
                EditorGUI.indentLevel++;
                m_Cretator.AocName = EditorGUILayout.TextField( "追加コンテンツ名", m_Cretator.AocName );
                m_Cretator.AocIndex = EditorGUILayout.IntField( "追加コンテンツのインデックス", m_Cretator.AocIndex );
                EditorGUI.indentLevel--;
            }

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

            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
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

                                string assetName = Path.GetFileNameWithoutExtension( path );
                                string extention = Path.GetExtension( path );

                                //パスがAssetBundleの配置パスから開始していれば、ファイル名と拡張子を
                                if( !path.StartsWith( AssetBundleDefine.ASSET_FILE_DIRECTRY_PATH ) )
                                {
                                    Debug.LogError( "AssetのパスがAssetBundleの指定のパスと異なります!  \nASSET_FILE_DIRECTRY_PATH:" + AssetBundleDefine.ASSET_FILE_DIRECTRY_PATH + "\nAsset Path:" + path );
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
        /// <param name="_isOverride">trueなら上書き</param>
        private void Save( bool _isOverride )
        {
            if( !m_Cretator.CheckAssetStatus() ) return;

            if( _isOverride )
            {
                m_Cretator.CreateAssetBundleDefineInfo( _isOverride );
            }
            else
            {
                string saveFilePath = EditorUtility.SaveFilePanelInProject( "AssetBundleDefineInfoの保存", "AssetBundleDefineInfo", "asset", "" );

                if( !string.IsNullOrEmpty( saveFilePath ) )
                {
                    m_Cretator.CreateAssetBundleDefineInfo( _isOverride );
                    AssetDatabase.CreateAsset( m_Cretator.OverwriteTarget, saveFilePath );
                    AssetImporter.GetAtPath( saveFilePath ).SetAssetBundleNameAndVariant( AssetBundleDefine.ASSETBUNDLE_INFO_NAME, "" );
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool CreateIdentifierClassFile()
        {
            m_Cretator.CreateAssetBundleDefineInfo( true );

            AssetBundleInfo defineInfo = m_Cretator.OverwriteTarget;
            string className = defineInfo.name + "IdentifierDefine";

            string path = EditorUtility.SaveFilePanelInProject( "定義ファイルの保存", className, "cs", "" );

            if( !string.IsNullOrEmpty( path ) )
            {
                string outputText = CreateDefineFileContents();
                File.WriteAllText( path, outputText );
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
            AssetBundleInfo defineInfo = m_Cretator.OverwriteTarget;

            string categoryText = nameof( AssetBundleCategory ) + "." + defineInfo.Category.ToString();
            string className = defineInfo.name + "IdentifierDefine";
            string outputText = $"namespace CodeIcf.AssetManagement\n{{\n\tpublic static class {className}\n\t{{\n";

            for( int i = 0; i < defineInfo.AssetDataList.Length; i++ )
            {
                AssetData info = defineInfo.AssetDataList[ i ];
                string assetName = info.AssetName.Replace( " ", "_" );
                outputText += $"\t\tpublic static readonly uint {assetName.ToUpper()} =  AssetIdentifier.CreateIdentifier(  {categoryText}, {i} );\n";
            }

            outputText += "\t}\n}\n";

            return outputText;
        }
    }
}