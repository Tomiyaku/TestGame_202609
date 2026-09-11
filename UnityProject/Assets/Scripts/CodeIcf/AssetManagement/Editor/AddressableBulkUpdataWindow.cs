using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
                    OverwriteAllAddressableInfo();
                }
            }
        }

        /// <summary>
        /// 指定のパス直下にある全ての<see cref="AddressableInfoCreater"/>の取得
        /// </summary>
        /// <returns></returns>
        private List<AddressableInfoCreater> GetAllCretaer()
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
        private bool CheckTargetAssetNull()
        {
            List<AddressableInfoCreater> fileList = GetAllCretaer();

            if( fileList.Count <= 0 )
            {
                Debug.LogError( "AddressableInfoCreaterが見つかりません" );
                return false;
            }

            bool IsNotNullFile = true;

            foreach( AddressableInfoCreater creater in fileList )
            {
                for( int i = 0; i < creater.TargetAssetList.Length; i++ )
                {
                    if( creater.TargetAssetList[ i ] == null )
                    {
                        Debug.LogError( "対象のAssetがありません ファイル名 : " + creater.name + " インデックス : " + i );
                        IsNotNullFile = false;
                    }
                }
            }

            return IsNotNullFile;
        }

        /// <summary>
        /// <see cref="AddressableInfoCreater"/>の<see cref="AddressableInfoCreater.OverwriteTarget"/>への上書き
        /// </summary>
        private void OverwriteAllAddressableInfo()
        {
            List<AddressableInfoCreater> fileList = GetAllCretaer();

            if( fileList.Count <= 0 )
            {
                Debug.LogError( "AddressableInfoCreaterが見つかりません" );
                return;
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

            if( !isOverwrite ) return;

            foreach( AddressableInfoCreater creater in fileList )
            {
                if( creater.CheckAssetStatus() )
                {
                    creater.CreateAddressablesDefineInfo( true );
                    EditorUtility.SetDirty( creater.OverwriteTarget );
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
