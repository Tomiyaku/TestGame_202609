using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.EditorScripts;

namespace CodeIcf.AssetManagement.AdressableEditor
{
    /// <summary>
    /// <see cref="AddressableInfoCreater"/>への一斉チェックと一斉更新を行うためのウインドウ
    /// </summary>
    public class AddressableBulkUpdataWindow : EditorWindow
    {
        /// <summary><see cref="AddressableInfoCreater"/>のファイルが配置されているデフォルトのパス</summary>
        private const string DEFAULT_DIRECTORY_PATH = "Assets/98_Editor/ScriptableObject/Editor/DefinitionFileCreater";
        /// <summary><see cref="AddressableInfoCreater"/>のファイルパス</summary>
        private static string m_DirectoryPath = DEFAULT_DIRECTORY_PATH;

        private uint m_AssetIdentifer = 0;
        private AddressableIdentifier m_AddressableIdentifier = null;
        private AddressableInfoCreater m_TargetCreater = null;

        [MenuItem( "Tools/AddressableInfoCreater一括更新Windowを表示" )]
        static void OpenWindow()
        {
            GetWindow<AddressableBulkUpdataWindow>( "AddressableInfo一括更新" );
        }

        private void OnGUI()
        {
            using( new EditorGUILayout.HorizontalScope( GUI.skin.box ) )
            {
                GUILayout.Label( "配置パス : " + m_DirectoryPath );

                if( GUILayout.Button( "変更" ) )
                {
                    string newPath = EditorUtility.OpenFolderPanel( "配置ディレクトリの指定", m_DirectoryPath, "" );

                    if( !string.IsNullOrEmpty( newPath ) )
                    {
                        string currentDirectoryPath = Directory.GetCurrentDirectory();
                        currentDirectoryPath = currentDirectoryPath.Replace( "\\", "/" );

                        if( newPath.StartsWith( currentDirectoryPath ) )
                        {
                            int index = newPath.IndexOf( "Assets/" );
                            m_DirectoryPath = newPath.Substring( index );
                        }
                        else
                        {
                            Debug.LogError( "プロジェクトフォルダと異なるパスです" );
                        }
                    }
                }
            }

            GUILayout.Space( 20 );

            if( GUILayout.Button( "全てのAddressableIndoCreaterの対象AssetへのNullチェック" ) )
            {
                if( CheckTargetAssetNull() )
                {
                    Debug.Log( "全ての対象Assetが設定されています" );
                }
            }

            GUILayout.Space( 20 );

            if( GUILayout.Button( "全てのAddressableIndoCreaterからAddressableInfoへの上書き更新" ) )
            {
                if( CheckTargetAssetNull() )
                {
                    OverwriteAllAddressableInfo( EditorUserBuildSettings.activeBuildTarget );
                }
            }

            GUILayout.Space( 50 );

            using( new EditorGUILayout.VerticalScope( GUI.skin.box ) )
            {
                uint newAssetIdentifer = ( uint )EditorGUILayout.LongField( "Asset識別子", ( long )m_AssetIdentifer );

                if( newAssetIdentifer != m_AssetIdentifer )
                {
                    m_AddressableIdentifier = null;
                    m_TargetCreater = null;
                    m_AssetIdentifer = newAssetIdentifer;
                }

                if( GUILayout.Button( "Asset識別子のAssetを検索" ) )
                {
                    m_TargetCreater = null;
                    m_AddressableIdentifier = new AddressableIdentifier( m_AssetIdentifer );
                }

                GUILayout.Space( 10 );

                if( m_AddressableIdentifier != null )
                {
                    if( m_AddressableIdentifier.ResultState == AddressableIdentifier.ResultCode.Success )
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField( "カテゴリ  : " + m_AddressableIdentifier.Category.ToString() );
                        EditorGUILayout.LabelField( "インデックス : " + m_AddressableIdentifier.Index );

                        if( m_TargetCreater == null || m_TargetCreater.Category != m_AddressableIdentifier.Category )
                        {
                            List<AddressableInfoCreater> fileList = GetAllCreater();

                            if( fileList.Count > 0 ) m_TargetCreater = fileList.Find( ( c ) => c.Category == m_AddressableIdentifier.Category );
                        }
                    }
                    else
                    {
                        m_TargetCreater = null;
                        EditorGUILayout.LabelField( "AddressablesAssetを対象にしたAsset識別子ではない" );
                    }
                }

                if( m_TargetCreater != null )
                {
                    if( m_TargetCreater.AssetInfoList.Length > m_AddressableIdentifier.Index )
                    {
                        EditorGUILayout.LabelField( "Asset名 : " + m_TargetCreater.AssetInfoList[ m_AddressableIdentifier.Index ].Asset.name );
                        EditorGUILayout.LabelField( "ファイルパス : " + AssetDatabase.GetAssetPath( m_TargetCreater.AssetInfoList[ m_AddressableIdentifier.Index ].Asset ) );
                        GUILayout.Space( 10 );

                        using( new EditorGUILayout.HorizontalScope() )
                        {
                            if( GUILayout.Button( "Assetを選択" ) ) Selection.activeObject = m_TargetCreater.AssetInfoList[ m_AddressableIdentifier.Index ].Asset;

                            GUILayout.Space( 10 );

                            if( GUILayout.Button( "このAssetが対象に含まれている\nAddressableInfoCreaterを選択" ) ) Selection.activeObject = m_TargetCreater;
                            if( GUILayout.Button( "このAssetが対象に含まれている\nAddressableInfoを選択" ) ) Selection.activeObject = m_TargetCreater.OverwriteTarget;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// 指定のパス直下にある全ての<see cref="AddressableInfoCreater"/>の取得
        /// </summary>
        /// <returns></returns>
        public static List<AddressableInfoCreater> GetAllCreater()
        {
            List<AddressableInfoCreater> resultLIst = new List<AddressableInfoCreater>();
            string[] fileList = Directory.GetFiles( m_DirectoryPath, "*" + ExtensionDefine.ASSET, SearchOption.TopDirectoryOnly );

            for( int i = 0; i < fileList.Length; i++ )
            {
                AddressableInfoCreater creater = AssetDatabase.LoadAssetAtPath<AddressableInfoCreater>( fileList[ i ] );

                if( creater != null ) resultLIst.Add( creater );
            }

            return resultLIst;
        }

        /// <summary>
        /// <see cref="AddressableInfoCreater"/>の<see cref="AddressableInfoCreater.TargetAssetList"/>のnullチェック
        /// </summary>
        /// <returns>trueであればnullの項目なし</returns>
        public static bool CheckTargetAssetNull()
        {
            List<AddressableInfoCreater> fileList = GetAllCreater();

            if( fileList.Count <= 0 )
            {
                Debug.LogError( "AddressableInfoCreaterが見つかりません" );
                return false;
            }

            bool IsNotNullFile = true;

            foreach( AddressableInfoCreater creater in fileList )
            {
                if( creater == null ) continue;
                if( creater.AssetInfoList == null ) continue;

                for( int i = 0; i < creater.AssetInfoList.Length; i++ )
                {
                    if( creater.AssetInfoList[ i ] == null )
                    {
                        Debug.LogError( "対象のAsset情報が存在しません ファイル名 : " + creater.name + " インデックス : " + i );
                        IsNotNullFile = false;
                    }

                    if( creater.AssetInfoList[i].Asset == null )
                    {
                        Debug.LogError( "対象のAssetが設定されていません ファイル名 : " + creater.name + " インデックス : " + i );
                        IsNotNullFile = false;
                    }
                }
            }

            return IsNotNullFile;
        }

        /// <summary>
        /// <see cref="AddressableInfoCreater"/>の<see cref="AddressableInfoCreater.OverwriteTarget"/>への上書き
        /// </summary>
        /// <param name="_buildTarget">対象のPlatoform</param>
        public static bool OverwriteAllAddressableInfo( BuildTarget _buildTarget )
        {
            List<AddressableInfoCreater> fileList = GetAllCreater();

            if( fileList.Count <= 0 )
            {
                Debug.LogError( "AddressableInfoCreaterが見つかりません" );
                return false;
            }

            bool isOverwrite = true;

            foreach( AddressableInfoCreater creater in fileList )
            {
                if( creater.OverwriteTarget == null )
                {
                    Debug.LogError( "上書き対象が設定されていません  ファイル名 : " + creater.name );
                    isOverwrite = false;
                }
            }

            if( !isOverwrite ) return false ;

            foreach( AddressableInfoCreater creater in fileList )
            {
                if( creater.CheckAssetStatus() )
                {
                    creater.CreateAddressablesDefineInfo( true, _buildTarget );
                    EditorUtility.SetDirty( creater.OverwriteTarget );
                }
            }

            AssetDatabase.SaveAssets();

            return false;
        }

        /// <summary>
        /// <see cref="AddressableInfoCreater"/>の<see cref="AddressableInfoCreater.OverwriteTarget"/>への上書き
        /// </summary>
        /// <param name="_buildTarget">対象のPlatoform</param>
        /// <param name="_switchBuildType">PlatformがSwitchの場合でのビルドの種類</param>
        public static bool SwitchPlatformAllAddressableInfo( BuildTarget _buildTarget, SwitchBuildType _switchBuildType )
        {
            List<AddressableInfoCreater> fileList = GetAllCreater();

            if( fileList.Count <= 0 )
            {
                Debug.LogError( "AddressableInfoCreaterが見つかりません" );
                return false;
            }

            bool isOverwrite = true;

            foreach( AddressableInfoCreater creater in fileList )
            {
                if( creater.OverwriteTarget == null )
                {
                    Debug.LogError( "上書き対象が設定されていません  ファイル名 : " + creater.name );
                    isOverwrite = false;
                }
            }

            if( !isOverwrite ) return false;

            foreach (AddressableInfoCreater creater in fileList)
            {
                if (creater.CheckAssetStatus())
                {
                    creater.SwitchPlatformAddressablesDefineInfo(_buildTarget, _switchBuildType);
                    EditorUtility.SetDirty(creater.OverwriteTarget);
                }
            }

            AssetDatabase.SaveAssets();

            return false;
        }
    }
}
