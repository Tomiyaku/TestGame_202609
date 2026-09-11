using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

using CodeIcf.AssetManagement.AdressableEditor;

namespace CodeIcf.EditorScripts
{
    /// <summary>
    /// プラットフォーム切り替え時に切り替え後のプラットフォームで使用するAssetのみをバイナリに含めるための処理を実行するクラス
    /// </summary>
    public class PlatformChangedAddressableResetting : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged( BuildTarget _previousTarget, BuildTarget _newTarget )
        {
            if( !AddressableBulkUpdataWindow.CheckTargetAssetNull() )
            {
                Debug.LogError( "不正なAddressableInfoCreaterが存在します" );
                throw new System.NotImplementedException();
            }

            if( _newTarget == BuildTarget.StandaloneWindows || _newTarget == BuildTarget.StandaloneWindows64 || _newTarget == BuildTarget.StandaloneLinux64 || _newTarget == BuildTarget.StandaloneOSX )
            {
                AddressableBulkUpdataWindow.SwitchPlatformAllAddressableInfo( _newTarget, SwitchBuildType.None_SwitchEdition);
            }
            else if( _newTarget == BuildTarget.Switch )
            {
                BuildSwithcer.ResetMark();
            }
        }
    }
}
