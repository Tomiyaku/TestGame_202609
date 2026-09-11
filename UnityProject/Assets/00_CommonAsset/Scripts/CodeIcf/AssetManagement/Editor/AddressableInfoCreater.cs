using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;

using CodeIcf.AssetManagement.AdressableManagement;
using CodeIcf.EditorScripts;
using CodeIcf.Extensions;

using AssetData = CodeIcf.AssetManagement.AdressableManagement.AddressableInfo.AssetData;

namespace CodeIcf.AssetManagement.AdressableEditor
{
    /// <summary>
    /// Asset情報
    /// </summary>
    [System.Serializable]
    public class AssetInfo
    {
        /// <summary>対象のAsset</summary>
        public Object Asset;
        /// <summary>このAssetを使用することができるPlatform</summary>
        public UsePlatformType UsePlatform;
        /// <summary>このAssetが使用できる条件</summary>
        public UseContidionType UseContidion;
        /// <summary><see cref="Asset"/>へのファイルパス ソート用</summary>
        public string AssetPath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AssetInfo()
        {
            Asset = null;
            UsePlatform = EnumExtensions.Everything<UsePlatformType>();
            UseContidion = EnumExtensions.Nothing<UseContidionType>();
            AssetPath = "";
        }
    }

    /// <summary>
    /// <see cref="AssetInfo"/>のInspectorカスタム表示
    /// </summary>
    [CustomPropertyDrawer( typeof( AssetInfo ) )]
    public class AssetIndoDrawer : PropertyDrawer
    {
        private readonly string[] USE_PLATFORM_NAME_LIST = { "PC", "Switch", };
        private readonly string[] CONDITON_NAME_LIST = { "Development Only", "Switch 物理エディションのみ", "Switch Aksys Games販売版のみ", "Switch Aksys Games販売版には含めない" };

        public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
        {
            EditorGUI.BeginProperty( _position, _label, _property );

            using( new EditorGUILayout.VerticalScope() )
            {
                SerializedProperty assetProperty = _property.FindPropertyRelative( "Asset" );
                SerializedProperty platformProperty = _property.FindPropertyRelative( "UsePlatform" );
                SerializedProperty conditionPropety = _property.FindPropertyRelative( "UseContidion" );

                float defaultLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50f;

                _position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField( _position, _label );

                _position.x = EditorGUIUtility.currentViewWidth * 0.2f;
                _position.width = EditorGUIUtility.currentViewWidth * 0.35f;

                EditorGUI.PropertyField( _position, assetProperty, new GUIContent( "" ) );

                _position.x = EditorGUIUtility.currentViewWidth * 0.56f;
                _position.width = EditorGUIUtility.currentViewWidth * 0.2f;
                EditorGUIUtility.labelWidth = 50f;

                platformProperty.intValue = EditorGUI.MaskField( _position, platformProperty.intValue, USE_PLATFORM_NAME_LIST );

                _position.x = EditorGUIUtility.currentViewWidth * 0.77f;
                _position.width = EditorGUIUtility.currentViewWidth * 0.2f;

                conditionPropety.intValue = EditorGUI.MaskField( _position, conditionPropety.intValue, CONDITON_NAME_LIST );

                EditorGUIUtility.labelWidth = defaultLabelWidth;
            }

            EditorGUI.EndProperty();
        }
    }

    [CreateAssetMenu( fileName = "AdressableInfoCreater", menuName = "ScriptableObjects/Create AdressableInfoCreater File" )]
    public class AddressableInfoCreater : ScriptableObject
    {
        /// <summary>更新時に上書きの対象になる<see cref="AddressableInfo"/></summary>
        public AddressableInfo OverwriteTarget;
        /// <summary>種類識別</summary>
        public AddressableCategory Category;
        /// <summary>対象のAssetを含めた情報一覧</summary>
        public AssetInfo[] AssetInfoList;

        /// <summary>
        /// <see cref="AddressableInfo"/>を作成
        /// </summary>
        /// <param name="_isOverride">trueの場合は上書き</param>
        /// <param name="_buildTarget">ビルド対象のプラットフォーム</param>
        public void CreateAddressablesDefineInfo( bool _isOverride, BuildTarget _buildTarget )
        {
            if( OverwriteTarget == null || !_isOverride )
            {//AssetBundleDefineInfoがまだない or 新規作成の場合は作成
                OverwriteTarget = CreateInstance<AddressableInfo>();
            }

            OverwriteTarget.Category = Category;

            AssetDatabase.Refresh();

            AddressableAssetGroup group = FindGroupAndCreate( Category.ToString() );
            AssetData[] adressList = new AssetData[ AssetInfoList.Length ];

            //対象のAssetからAssetDefineInfoを作成
            if( AssetInfoList != null && AssetInfoList.Length > 0 )
            {
                for( int i = 0; i < AssetInfoList.Length; i++ )
                {
                    string path = "";
                    bool isDevelop = true;
                    bool isPlatform = false;

                    if( !EditorUserBuildSettings.development )
                    {
                        // Developのフラグが立っていて、DevelopmentBuildではない場合は対象にしない
                        if( AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.DevelopmentOnly ) ) isDevelop = false;
                    }

                    if( _buildTarget == BuildTarget.Switch && AssetInfoList[ i ].UsePlatform.HasFlag( UsePlatformType.Switch ) )
                    {//PlatformがSwitchの状態であり、Swich版で読み込むAssetであればエントリに追加
                        bool isPhysicalEditonOnlty = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly );
                        bool isAksysGamesOnly = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_Only );
                        bool isAksysGamesNotInclude = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_NotIncluded );
#if SWITCH_LIICA_PHYSICAL_EDITION
                        isPlatform = !isAksysGamesOnly;
#elif SWITCH_AKSYSGAMES_PUBLISH
                        isPlatform = !isPhysicalEditonOnlty && !isAksysGamesNotInclude;
#else // SWITCH_LIICA_PHYSICAL_EDITION SWITCH == false && SWITCH_AKSYSGAMES_PUBLISH == false
                        isPlatform = !isPhysicalEditonOnlty && !isAksysGamesOnly;
#endif  // SWITCH_LIICA_PHYSICAL_EDITION SWITCH_AKSYSGAMES_PUBLISH
                    }
                    else if( ( _buildTarget == BuildTarget.StandaloneWindows || _buildTarget == BuildTarget.StandaloneWindows64 || _buildTarget == BuildTarget.StandaloneLinux64 || _buildTarget == BuildTarget.StandaloneOSX )
                        && AssetInfoList[ i ].UsePlatform.HasFlag( UsePlatformType.PC ) )
                    {//PlatformがPCの状態であり、PC版で読み込むAssetであればエントリに追加
                        isPlatform = true;
                    }

                    if( isDevelop && isPlatform )
                    {// Develop Platformのどちらの条件を満たしているのでエントリに追加
                        CheckAddressableEntry( AssetInfoList[ i ].Asset, group, null, out path );
                    }
                    else
                    {// それ以外場合はAddressabbleAssetのエントリを削除
                        RemoveAddressableAssetEntry( i );
                        path = AssetDatabase.GetAssetPath( AssetInfoList[ i ].Asset );
                    }

                    string assetName = Path.GetFileNameWithoutExtension( path );
                    string extention = Path.GetExtension( path );
                    bool isScene = extention == ExtensionDefine.SCENE;

                    adressList[ i ] = new AssetData()
                    {
                        AssetName = assetName,
                        Address = path,
                        IsSceneAsset = isScene,
                        UsePlatform = AssetInfoList[ i ].UsePlatform,
                        UseContidion = AssetInfoList[ i ].UseContidion
                    };
                }
            }

            OverwriteTarget.AdressList = adressList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_buildTarget"></param>
        /// <param name="_isGameCard">PlatformがSwitchの場合に、ゲームカード向けの設定かのフラグ</param>
        public void SwitchPlatformAddressablesDefineInfo( BuildTarget _buildTarget, SwitchBuildType _switchBuildType )
        {
            if( AssetInfoList == null || AssetInfoList.Length <= 0 )
            {
                return;
            }

            if( OverwriteTarget == null )
            {
                //AssetBundleDefineInfoがまだないので新規作成
                CreateAddressablesDefineInfo( true, _buildTarget );
                return;
            }

            AssetDatabase.Refresh();

            AddressableAssetGroup group = FindGroupAndCreate( Category.ToString() );

            for( int i = 0; i < AssetInfoList.Length; i++ )
            {
                string path = "";
                bool isDevelop = true;
                bool isPlatform = false;
                bool isSwitching = true;

                if( !EditorUserBuildSettings.development )
                {
                    // Developのフラグが立っていて、DevelopmentBuildではない場合は対象にしない
                    if( AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.DevelopmentOnly ) ) isDevelop = false;
                }

                if( AssetInfoList[ i ].UsePlatform.IsEverything() )
                {
                    isSwitching = false;
                }
                else
                {
                    if( _buildTarget == BuildTarget.Switch && AssetInfoList[ i ].UsePlatform.HasFlag( UsePlatformType.Switch ) )
                    {//PlatformがSwitchの状態であり、Swich版で読み込むAssetであればエントリに追加
                        // 物理エディションのみに含まれるリソースか
                        bool isPhysicalEditionOnly = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.PhysicalEditionOnly );
                        // Aksys Games販売版のみにふくまれるリソースか
                        bool isAksysGamesOnly = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_Only );
                        // Aksys Games販売版には含まれないリソースか
                        bool isAksysGamesNotInclude = AssetInfoList[ i ].UseContidion.HasFlag( UseContidionType.AksysGames_Publish_NotIncluded );

                        if( _switchBuildType == SwitchBuildType.Liica_PhysicalEdition )
                        {// Liica/物理エディション
                            isPlatform = !isAksysGamesOnly;
                        }
                        else if( _switchBuildType == SwitchBuildType.Liica_Download )
                        {// Liica/ダウンロード版
                            isPlatform = !isPhysicalEditionOnly && !isAksysGamesOnly;
                        }
                        else if( _switchBuildType == SwitchBuildType.AksysGames_Publish )
                        {// Aksys Games/米・欧・豪
                            isPlatform = !isPhysicalEditionOnly && !isAksysGamesNotInclude;
                        }
                        else
                        {// その他(ここに入ることはないはず
                            isPlatform = !isPhysicalEditionOnly && !isAksysGamesOnly;
                        }
                    }
                    else if( ( _buildTarget == BuildTarget.StandaloneWindows || _buildTarget == BuildTarget.StandaloneWindows64 || _buildTarget == BuildTarget.StandaloneLinux64 || _buildTarget == BuildTarget.StandaloneOSX ) && AssetInfoList[ i ].UsePlatform.HasFlag( UsePlatformType.PC ) )
                    {//PlatformがPCの状態であり、PC版で読み込むAssetであればエントリに追加
                        isPlatform = true;
                    }
                }

                if( isSwitching )
                {
                    if( isDevelop && isPlatform )
                    {// Develop Platformのどちらの条件を満たしているのでエントリに追加
                        CheckAddressableEntry( AssetInfoList[ i ].Asset, group, null, out path );
                    }
                    else
                    {// それ以外場合はAddressabbleAssetのエントリを削除
                        RemoveAddressableAssetEntry( i );
                        path = AssetDatabase.GetAssetPath( AssetInfoList[ i ].Asset );
                    }

                    string assetName = Path.GetFileNameWithoutExtension( path );
                    string extention = Path.GetExtension( path );
                    bool isScene = extention == ExtensionDefine.SCENE;

                    OverwriteTarget.AdressList[ i ].Updata( assetName, path, isScene, AssetInfoList[ i ].UsePlatform, AssetInfoList[ i ].UseContidion );
                }
            }
        }


        public bool CheckAssetStatus()
        {
            bool result = true;

            for( int i = 0; i < AssetInfoList.Length; i++ )
            {
                if( AssetInfoList[ i ].Asset == null )
                {
                    Debug.LogError( i + " : Assetがありません" );
                    result = false;
                    continue;
                }

                if( !AssetDatabase.IsMainAsset( AssetInfoList[ i ].Asset ) )
                {
                    Debug.LogError( i + " : 対象のAssetがMainAssetではありません " + AssetInfoList[ i ].Asset.name );
                    result = false;
                    continue;
                }

                string path = AssetDatabase.GetAssetPath( AssetInfoList[ i ].Asset );

                if( string.IsNullOrEmpty( path ) )
                {
                    Debug.LogError( i + " : 対象のAssetのファイルパスが取得できません " + AssetInfoList[ i ].Asset.name );
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
        public static AddressableAssetGroup FindGroupAndCreate( string _groupName )
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
        /// <param name="_outPath">_sourceへのファイルパス 読み込みの際のアドレスに設定される</param>
        public static void CheckAddressableEntry( Object _source, AddressableAssetGroup _group, string _label, out string _outPath )
        {
            _outPath = null;

            if( _source == null || !AssetDatabase.Contains( _source ) ) return;

            _outPath = AssetDatabase.GetAssetPath( _source );
            string guid = AssetDatabase.AssetPathToGUID( _outPath );

            AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = addressableAssetSettings.FindAssetEntry( guid );

            bool isUpdate = false;

            if( entry == null )
            {// AddressablesAssetとしてと設定されていないので設定
                entry = addressableAssetSettings.CreateOrMoveEntry( guid, _group );
                isUpdate = true;
            }

            if( !entry.address.Equals( _outPath ) )
            {// addressがファイルパスと異なるのでファイルパスを設定
                entry.SetAddress( _outPath );
                isUpdate = true;
            }

            if( !string.IsNullOrEmpty( _label ) && !entry.labels.Contains( _label ) )
            {// labelを変更する場合に既存のlabelと異なる場合は変更
                entry.SetLabel( _label, true, true );
                isUpdate = true;
            }

            if( isUpdate )
            {// 更新する必要がある場合にはAssetを更新
                EditorUtility.SetDirty( _source );

                addressableAssetSettings.SetDirty( AddressableAssetSettings.ModificationEvent.EntryModified, entry, true );
            }
        }

        /// <summary>
        /// 指定のAssetのAddressabbleAssetのエントリを削除
        /// </summary>
        /// <param name="_source"></param>
        public void RemoveAddressableAssetEntry( int _index )
        {
            if( _index < 0 || _index >= AssetInfoList.Length ) return;

            string path = AssetDatabase.GetAssetPath( AssetInfoList[ _index ].Asset );
            string guid = AssetDatabase.AssetPathToGUID( path );

            AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
            addressableAssetSettings.RemoveAssetEntry( guid );
        }

        /// <summary>
        /// <see cref="AssetInfoList"/>に含まれる全てのAssetのAddressabbleAssetのエントリを削除
        /// </summary>
        public void RemoveAddressableAssetEntryAll()
        {
            for( int i = 0; i < AssetInfoList.Length; i++ )
            {
                RemoveAddressableAssetEntry( i );
            }
        }

        /// <summary>
        /// AddressableAsset定義ファイルのAddressable関連設定の確認
        /// </summary>
        public void CheckAddressablesEntyForDeifneInfoFIle()
        {
            AddressableAssetGroup group = FindGroupAndCreate( AddressableDefine.ADRESSABLES_INFO_NAME );
            CheckAddressableEntry( OverwriteTarget, group, AddressableDefine.ADRESSABLES_INFO_NAME, out _ );
        }

        /// <summary>
        /// Platform、IsDevelopmentBuildOnlyの設定をまとめてcsvで出力
        /// </summary>
        public void CreatePlatformSettingCsv()
        {
            string csvStr = "Index,AssetPath,UsePlatform(0:None 1:PC 2 :Switch 255:ALL,UseCondition( 0: None 1:DevelopmentBuild Only 2:SwitchGameCard Only\n";

            for( int i = 0; i < AssetInfoList.Length; i++ )
            {
                csvStr += i.ToString() + ",";
                csvStr += '"' + ( AssetInfoList[ i ].Asset != null ? AssetDatabase.GetAssetPath( AssetInfoList[ i ].Asset ) + '"' : "" ) + ",";
                csvStr += AssetInfoList[ i ].UsePlatform.ToInt().ToString() + ",";
                csvStr += AssetInfoList[ i ].UseContidion.ToInt().ToString();

                if( i + 1 < AssetInfoList.Length ) csvStr += "\n";
            }

            string writeFIlePath = EditorUtility.SaveFilePanel( "AddresableInfoCreaterのPlatform設定のCsv出力", Directory.GetCurrentDirectory(), "", "csv" );

            if( !string.IsNullOrEmpty( writeFIlePath ) )
            {
                File.WriteAllText( writeFIlePath, csvStr, Encoding.UTF8 );
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// csvからPlatform、IsDevelopmentBuildOnlyの設定を上書き
        /// </summary>
        public void OverwritePlatformSettingToCsv()
        {
            string filePath = EditorUtility.OpenFilePanel( "Platform設定のCsvを読み込み", Directory.GetCurrentDirectory(), "csv" );

            if( string.IsNullOrEmpty( filePath ) ) return;

            string csvStr = File.ReadAllText( filePath );

            if( string.IsNullOrEmpty( csvStr ) ) return;

            string[][] csv = csvStr.SplitCsv();

            for( int i = 0; i < csv.Length; i++ )
            {
                if( i >= AssetInfoList.Length ) return;

                AssetInfoList[ i ].UsePlatform = int.TryParse( csv[ i + 1 ][ 2 ], out int platform ) ? platform.ToEnum<UsePlatformType>() : EnumExtensions.Nothing<UsePlatformType>();
                AssetInfoList[ i ].UseContidion = int.TryParse( csv[ i + 1 ][ 3 ], out int condition ) ? condition.ToEnum<UseContidionType>() : EnumExtensions.Nothing<UseContidionType>();
            }
        }
    }

    [CustomEditor( typeof( AddressableInfoCreater ) )]
    public class AddressablesDefineInfoCreaterEditor : Editor
    {
        private AddressableInfoCreater m_Creatator = null;
        private ReorderableList m_AssetList;
        private SerializedProperty m_AssetDefineListProperty;

        private void OnEnable()
        {
            m_Creatator = target as AddressableInfoCreater;

            m_AssetDefineListProperty = serializedObject.FindProperty( "AssetInfoList" );
            m_AssetList = new ReorderableList( serializedObject, m_AssetDefineListProperty, true, true, false, false );

            m_AssetList.drawHeaderCallback = ( rect ) =>
            {
                DropAreaGUI( rect );
            };

            m_AssetList.elementHeightCallback = ( index ) =>
            {
                SerializedProperty property = m_AssetDefineListProperty.GetArrayElementAtIndex( index );

                return EditorGUIUtility.singleLineHeight;//EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight( property, true );
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
                            m_Creatator.RemoveAddressableAssetEntry( m_AssetList.index );
                            m_AssetDefineListProperty.DeleteArrayElementAtIndex( m_AssetList.index );
                        }
                    }

                    if( GUI.Button( rightRect, "全て削除" ) )
                    {
                        m_Creatator.RemoveAddressableAssetEntryAll();
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

                if( m_Creatator.OverwriteTarget != null )
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

                        GUILayout.Space( 20 );

                        if( GUILayout.Button( "Platform設定をCSVで出力" ) ) m_Creatator.CreatePlatformSettingCsv();

                        GUILayout.Space( 10 );

                        if( GUILayout.Button( "CSVからPlatform設定を上書き" ) ) m_Creatator.OverwritePlatformSettingToCsv();
                    }
                }
            }

            GUILayout.Space( 20 );

            m_Creatator.OverwriteTarget = ( AddressableInfo )EditorGUILayout.ObjectField( "上書き対象のファイル", m_Creatator.OverwriteTarget, typeof( AddressableInfo ), false );

            GUILayout.Space( 10 );

            //AddressablesのカテゴリをPopupで選択
            m_Creatator.Category = ( AddressableCategory )EditorGUILayout.EnumPopup( "カテゴリ", m_Creatator.Category );

            GUILayout.Space( 10 );

            if( m_AssetList != null ) m_AssetList.DoLayoutList();

            GUILayout.Space( 30 );

            if( m_AssetList != null && m_AssetList.count > 1 )
            {
                if( GUILayout.Button( "ファイルパスでソート" ) )
                {
                    if( EditorUtility.DisplayDialog( "Sort", "ファイルパスで対象のAsset一覧をソートします", "Yes", "No" ) ) SortToFilePath();
                }
            }

            GUILayout.Space( 10 );

            if( GUILayout.Button( "クリップボードに全てのAssetのファイルパスをコピー" ) )
            {
                GetAssetPathListWithClipboard();
            }

            GUILayout.Space( 30 );

            if( GUILayout.Button( "クリップボードのファイルパスリストからAssetを追加" ) )
            {
                SetAssetInfoToFilePathList( EditorGUIUtility.systemCopyBuffer );
            }

            if( GUILayout.Button( "ファイルパスリストを読み込んでAssetを追加" ) )
            {
                string path = EditorUtility.OpenFilePanel( "ファイルパスリストのテキストを読み込む", Directory.GetCurrentDirectory(), "csv" );

                if( !string.IsNullOrEmpty( path ) )
                {
                    SetAssetInfoToFilePathList( File.ReadAllText( path ) );
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty( m_Creatator );
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

                            List<AssetInfo> addList = new List<AssetInfo>();

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

                                addList.Add( new AssetInfo() { Asset = obj } );
                            }

                            if( addList.Count > 0 && m_AssetDefineListProperty.arraySize > 0 )
                            {//既存リストで既に存在するAssetを削除
                                for( int i = 0; i < addList.Count; i++ )
                                {
                                    if( System.Array.FindIndex( m_Creatator.AssetInfoList, ( assetObject ) => assetObject.Asset == addList[ i ].Asset ) >= 0 )
                                    {
                                        Debug.Log( "Add Target Asset Auplication! : " + addList[ i ].Asset.name );
                                        addList.RemoveAt( i );
                                        i--;
                                    }
                                }
                            }

                            if( addList.Count > 0 )
                            {
                                //先頭に既存のリストを追加して差し替え
                                if( m_AssetDefineListProperty.arraySize > 0 ) addList.InsertRange( 0, m_Creatator.AssetInfoList );

                                m_Creatator.AssetInfoList = addList.ToArray();
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
            if( !m_Creatator.CheckAssetStatus() ) return;

            if( _isOverwrite )
            {
                m_Creatator.CreateAddressablesDefineInfo( _isOverwrite, EditorUserBuildSettings.activeBuildTarget );
            }
            else
            {
                string saveFilePath = EditorUtility.SaveFilePanelInProject( "AssetBundleDefineInfoの保存", m_Creatator.name, "asset", "", "Assets/00_CommonAsset/AddressableDefinition/DefinitionFile" );

                if( !string.IsNullOrEmpty( saveFilePath ) )
                {
                    m_Creatator.CreateAddressablesDefineInfo( _isOverwrite, EditorUserBuildSettings.activeBuildTarget );
                    AssetDatabase.CreateAsset( m_Creatator.OverwriteTarget, saveFilePath );
                }
            }

            m_Creatator.CheckAddressablesEntyForDeifneInfoFIle();

            EditorUtility.SetDirty( m_Creatator.OverwriteTarget );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool CreateIdentifierClassFile()
        {
            m_Creatator.CreateAddressablesDefineInfo( true, EditorUserBuildSettings.activeBuildTarget );

            AddressableInfo defineInfo = m_Creatator.OverwriteTarget;
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
            AddressableInfo defineInfo = m_Creatator.OverwriteTarget;

            string categoryText = nameof( AddressableCategory ) + "." + defineInfo.Category.ToString();
            string className = defineInfo.name + "IdentifierDefine";
            string outputText = $"namespace CodeIcf.AssetManagement.AdressableManagement\n{{\n\tpublic static class {className}\n\t{{\n";

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
            AddressableInfo defineInfo = m_Creatator.OverwriteTarget;
            string outputText = "";

            for( int i = 0; i < defineInfo.AdressList.Length; i++ )
            {
                AssetData info = defineInfo.AdressList[ i ];
                string assetName = info.AssetName;
                outputText += $"\"{assetName}\",\n";
            }

            return outputText;
        }

        private void SortToFilePath()
        {
            foreach( AssetInfo info in m_Creatator.AssetInfoList )
            {
                info.AssetPath = AssetDatabase.GetAssetPath( info.Asset );
            }

            System.Array.Sort( m_Creatator.AssetInfoList, ( a, b ) => a.AssetPath.CompareTo( b.AssetPath ) );
        }

        /// <summary>
        /// Assetのファイルパスを元に<see cref="AssetInfo"/>を追加
        /// </summary>
        /// <param name="_str">1行につき1ファイルのパスが記載されている文字列</param>
        private void SetAssetInfoToFilePathList( string _str )
        {
            if( string.IsNullOrEmpty( _str ) ) return;

            string[] filePathList = _str.SplitNewLine();

            if( filePathList == null || filePathList.Length <= 0 ) return;

            List<AssetInfo> addItemList = new List<AssetInfo>();

            foreach( string filePath in filePathList )
            {
                string assetPath = filePath;

                if( !assetPath.Contains( "Assets/" ) && !assetPath.Contains( "Assets\\" ) ) continue;

                if( !assetPath.StartsWith( "Assets" ) )
                {
                    var index = assetPath.IndexOf( "Assets" );
                    assetPath = assetPath.Substring( index );
                }

                assetPath = assetPath.Replace( "\\", "/" );
                Object asset = AssetDatabase.LoadAssetAtPath<Object>( assetPath );

                if( asset == null ) return;

                if( m_Creatator.AssetInfoList != null && m_Creatator.AssetInfoList.Length > 0 )
                {
                    if( System.Array.FindIndex( m_Creatator.AssetInfoList, ( a ) => a.Asset == asset ) >= 0 ) return;
                }

                AssetInfo assetInfo = new AssetInfo { Asset = asset };

                addItemList.Add( assetInfo );
            }

            if( addItemList.Count <= 0 ) return;

            addItemList.InsertRange( 0, m_Creatator.AssetInfoList );
            m_Creatator.AssetInfoList = addItemList.ToArray();
        }

        /// <summary>
        /// 登録しているAssetのファイルパスを全てクリップボードにコピーする
        /// </summary>
        public void GetAssetPathListWithClipboard()
        {
            if( m_Creatator.AssetInfoList == null || m_Creatator.AssetInfoList.Length <= 0 ) return;

            string result = "";

            foreach( AssetInfo assetInfo in m_Creatator.AssetInfoList )
            {
                if( assetInfo.Asset == null ) continue;

                result += AssetDatabase.GetAssetPath( assetInfo.Asset );
                result += "\n";
            }

            EditorGUIUtility.systemCopyBuffer = result;
        }
    }
}