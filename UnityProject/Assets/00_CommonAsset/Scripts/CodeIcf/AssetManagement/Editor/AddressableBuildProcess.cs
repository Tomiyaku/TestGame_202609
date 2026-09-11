using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace CodeIcf.AssetManagement.AdressableEditor
{
    public class AddressableBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild( BuildReport report )
        {
            AddressableBulkUpdataWindow.OverwriteAllAddressableInfo( EditorUserBuildSettings.activeBuildTarget );
        }
    }
}