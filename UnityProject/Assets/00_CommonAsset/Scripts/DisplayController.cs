using UnityEngine;

using jp.co.liica.q2.SaveDataManagement;

namespace jp.co.liica.q2.Common
{
    /// <summary>
    /// ディスプレイモードの切り替え対応クラス
    /// </summary>
    public static class DisplayController
    {
        /// <summary>
        /// ディスプレイモードの種類
        /// </summary>
        public enum DisplayMode : int
        {
            Window,
            Borderless_Window,
            FullScreen
        }

        /// <summary>
        /// ディスプレイモードの変更
        /// </summary>
        public static void ChangeDisplayMode()
        {
#if !UNITY_SWITCH
            DisplayMode displayMode = ( DisplayMode )SaveDataManager.Instance.DisplayMode;
            Vector2Int currentDisplayResoluton = ApplicationDefine.DISPLAY_RESOLUTION_LIST[ SaveDataManager.Instance.DIsplayResolution ];

            switch( displayMode )
            {
                case DisplayMode.Window: Screen.SetResolution( currentDisplayResoluton.x, currentDisplayResoluton.y, FullScreenMode.Windowed ); break;
                case DisplayMode.Borderless_Window: Screen.SetResolution( currentDisplayResoluton.x, currentDisplayResoluton.y, FullScreenMode.FullScreenWindow ); break;
                case DisplayMode.FullScreen: Screen.SetResolution( currentDisplayResoluton.x, currentDisplayResoluton.y, FullScreenMode.ExclusiveFullScreen ); break;
            }
#endif
        }
    }
}