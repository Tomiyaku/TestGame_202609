using UnityEditor;
using UnityEngine;

using jp.co.liica.q2.Common;
using CodeIcf.AssetManagement.AdressableEditor;

namespace CodeIcf.EditorScripts
{
    public class BuildSwithcer : EditorWindow
    {
        /// <summary>Liica/ダウンロード版を選択するためのメニュー</summary>
        public const string PathMenu_Liica_Download = "BuildSwitcher/Liica/ダウンロード版";
        /// <summary>Liica/物理エディションを選択するためのメニュー</summary>
        public const string PathMenu_Liica_PhysicalEdition = "BuildSwitcher/Liica/物理エディション";
        /// <summary>Aksys Games/米・欧・豪を選択するためのメニュー</summary>
        public const string PathMemu_AksysGames_Publish = "BuildSwitcher/Aksys Games/米・欧・豪";

        /// <summary>Liica/ダウンロード版のnmetaファイルのパス</summary>
        private const string NmetaFilePath_Liica_Download = "../Docs/nmeta/Application.aarch64.lp64.nmeta";
        /// <summary>Liica/物理エディションのnmetaファイルのフルパス</summary>
        private const string NmetaFilePath_Liica_PhysicalEdition = "C:/Users/PC-190_User/workspace/q_q2_launcherapp/Docs/nmeta/Q2_rom.nmeta";
        /// <summary>Aksys Games/米・欧・豪のnmetaファイルのフルパス</summary>
        private const string NmetaFilePath_AksysGames_Publish = "../Docs/nmeta_AksysGames/q2_AksysGames_Application.aarch64.lp64.nmeta";

        public static void ResetMark()
        {
            UnityEditor.Menu.SetChecked( PathMenu_Liica_Download, false );
            UnityEditor.Menu.SetChecked( PathMenu_Liica_PhysicalEdition, false );
            UnityEditor.Menu.SetChecked( PathMemu_AksysGames_Publish, false );
        }

#if UNITY_SWITCH
        [MenuItem( PathMenu_Liica_Download, false, 1 )]
        private static void CheckMark_JP_Download()
        {
            if( EditorApplication.isPlaying ) return;

            UnityEditor.Menu.SetChecked( PathMenu_Liica_Download, true );
            UnityEditor.Menu.SetChecked( PathMenu_Liica_PhysicalEdition, false );
            UnityEditor.Menu.SetChecked( PathMemu_AksysGames_Publish, false );
            SetScriptingDefine( SwitchBuildType.Liica_Download );

            AddressableBulkUpdataWindow.SwitchPlatformAllAddressableInfo( EditorUserBuildSettings.activeBuildTarget, SwitchBuildType.Liica_Download );
        }

        [MenuItem( PathMenu_Liica_PhysicalEdition, false, 2 )]
        private static void CheckMark_JP_PhysicalEdition()
        {
            if( EditorApplication.isPlaying ) return;

            UnityEditor.Menu.SetChecked( PathMenu_Liica_Download, false );
            UnityEditor.Menu.SetChecked( PathMenu_Liica_PhysicalEdition, true );
            UnityEditor.Menu.SetChecked( PathMemu_AksysGames_Publish, false );
            SetScriptingDefine( SwitchBuildType.Liica_PhysicalEdition );

            AddressableBulkUpdataWindow.SwitchPlatformAllAddressableInfo( EditorUserBuildSettings.activeBuildTarget, SwitchBuildType.Liica_PhysicalEdition );
        }

        [MenuItem( PathMemu_AksysGames_Publish, false, 3 )]
        private static void ChackMark_NA_MultiProgram()
        {
            if( EditorApplication.isPlaying ) return;

            UnityEditor.Menu.SetChecked( PathMenu_Liica_Download, false );
            UnityEditor.Menu.SetChecked( PathMenu_Liica_PhysicalEdition, false );
            UnityEditor.Menu.SetChecked( PathMemu_AksysGames_Publish, true );
            SetScriptingDefine( SwitchBuildType.AksysGames_Publish );

            AddressableBulkUpdataWindow.SwitchPlatformAllAddressableInfo( EditorUserBuildSettings.activeBuildTarget, SwitchBuildType.AksysGames_Publish );
        }


        /// <summary>
        /// Scripting Define Symbolsの変更
        /// </summary>
        /// <param name="_switchBuildType"></param>
        private static void SetScriptingDefine( SwitchBuildType _switchBuildType )
        {
            string oldDefineSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Switch );
            string newDefineSymbol = oldDefineSymbol;
            Debug.LogFormat( "Define変更前:[{0}]", oldDefineSymbol );

            switch( _switchBuildType )
            {
                case SwitchBuildType.Liica_Download:
                case SwitchBuildType.Liica_PhysicalEdition:
                    newDefineSymbol = SetScriptingDefine_Liica( _switchBuildType, oldDefineSymbol );
                    break;
                case SwitchBuildType.AksysGames_Publish:
                    newDefineSymbol = SetScriptingDefine_AksysGames( _switchBuildType, oldDefineSymbol );
                    break;
            }

            if( newDefineSymbol.Equals( oldDefineSymbol ) )
            {
                Debug.LogFormat( "Define変更なし:[{0}]", newDefineSymbol );
            }
            else
            {
                Debug.LogFormat( "Define変更あり:[{0}]", newDefineSymbol );
                PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Switch, newDefineSymbol );

                string modi = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Switch );
                Debug.LogFormat( "Define変更後取得:[{0}]", modi );
            }
        }

        /// <summary>
        /// 対象のシンボルを追加する
        /// </summary>
        /// <param name="_refDefimeSymbol"></param>
        /// <param name="_addTargetSymbol"></param>
        private static void AddTargetSymbol( ref string _refDefimeSymbol, string _addTargetSymbol )
        {
            if( string.IsNullOrEmpty( _refDefimeSymbol ) )
            {
                _refDefimeSymbol = _addTargetSymbol;
            }
            else if( !_refDefimeSymbol.Contains( _addTargetSymbol ) )
            {
                _refDefimeSymbol += ";" + _addTargetSymbol;
            }
        }

        /// <summary>
        /// 対象のシンボルを削除する
        /// </summary>
        /// <param name="_refDefimeSymbol"></param>
        /// <param name="_removeTargetSymbol"></param>
        private static void RemoveTargetSymbol( ref string _refDefimeSymbol, string _removeTargetSymbol )
        {
            if( _refDefimeSymbol.Equals( _removeTargetSymbol ) )
            {// 対象のシンボルのみ
                _refDefimeSymbol = "";
            }
            else
            {
                string beforeColon = ";" + _removeTargetSymbol;
                string afterColon = _removeTargetSymbol + ";";

                if( _refDefimeSymbol.Contains( beforeColon ) )
                {// 2つ目以降
                    _refDefimeSymbol = _refDefimeSymbol.Replace( beforeColon, "" );
                }
                else if( _refDefimeSymbol.Contains( afterColon ) )
                {// 先頭
                    _refDefimeSymbol = _refDefimeSymbol.Replace( afterColon, "" );
                }
            }
        }

        private static string SetScriptingDefine_Liica( SwitchBuildType _switchBuildType, string _oldDefineSymbol )
        {
            string defineSymbol = _oldDefineSymbol;
            RemoveTargetSymbol( ref defineSymbol, SwitchDefine.SWITCH_AKSYSGAMES_PUBLISH_SYMBOL_NAME );

            if( _switchBuildType == SwitchBuildType.Liica_PhysicalEdition )
            {
                // SWITCH_LIICA_PHYSICAL_EDITIONのdefineを追加
                AddTargetSymbol( ref defineSymbol, SwitchDefine.SWITCH_LIICA_PHYSICAL_EDITION_SYMBOLE_NAME );

                PlayerSettings.Switch.NMETAOverride = NmetaFilePath_Liica_PhysicalEdition;
            }
            else if( _switchBuildType == SwitchBuildType.Liica_Download )
            {
                // SWITCH_LIICA_PHYSICAL_EDITIONのdefineを除去
                RemoveTargetSymbol( ref defineSymbol, SwitchDefine.SWITCH_LIICA_PHYSICAL_EDITION_SYMBOLE_NAME );

                PlayerSettings.Switch.NMETAOverride = NmetaFilePath_Liica_Download;
            }

            return defineSymbol;
        }

        private static string SetScriptingDefine_AksysGames( SwitchBuildType _switchBuildType, string _oldDefineSymbol )
        {
            string defineSymbol = _oldDefineSymbol;
            RemoveTargetSymbol( ref defineSymbol, SwitchDefine.SWITCH_LIICA_PHYSICAL_EDITION_SYMBOLE_NAME );

            if( _switchBuildType == SwitchBuildType.AksysGames_Publish )
            {
                AddTargetSymbol( ref defineSymbol, SwitchDefine.SWITCH_AKSYSGAMES_PUBLISH_SYMBOL_NAME );
                PlayerSettings.Switch.NMETAOverride = NmetaFilePath_AksysGames_Publish;
            }

            return defineSymbol;
        }
#endif
    }
}