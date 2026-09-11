#if UNITY_SWITCH
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace CodeIcf.EditorScripts
{
    public class BuildChecker
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler( CheckBuildSetting );
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void CheckBuildSetting( BuildPlayerOptions options )
        {
            bool isCheckedJpDl = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMenu_Liica_Download );
            bool isCheckedJpPhysicalEdition = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMenu_Liica_PhysicalEdition );
            bool isCheckedNaMultoProgram = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMemu_AksysGames_Publish );

            if( !isCheckedJpDl && !isCheckedJpPhysicalEdition && !isCheckedNaMultoProgram )
            {
                throw new BuildFailedException( "メニューの BuildSwitcher から対応するバージョンを選択してください" );
            }
            else
            {
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer( options );
            }
        }

        private static void OnPlayModeStateChanged( PlayModeStateChange _playModeStateChange )
        {
            if( _playModeStateChange == PlayModeStateChange.ExitingEditMode )
            {
                bool isCheckedDL = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMenu_Liica_Download );
                bool isCheckedGameCard = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMenu_Liica_PhysicalEdition );
                bool isCheckedNaMultiProgram = UnityEditor.Menu.GetChecked( BuildSwithcer.PathMemu_AksysGames_Publish );

                if( !isCheckedDL && !isCheckedGameCard && !isCheckedNaMultiProgram )
                {
                    EditorApplication.isPlaying = false;
                    Debug.LogError( "メニューの BuildSwitcher から対応するバージョンを選択してください" );
                }
            }
        }
    }
}
#endif