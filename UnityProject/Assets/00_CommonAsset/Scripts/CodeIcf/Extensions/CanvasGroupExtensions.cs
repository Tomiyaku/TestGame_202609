using UnityEngine;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// <see cref="CanvasGroup"/>拡張クラス
    /// </summary>
    public static class CanvasGroupExtensions
    {
        /// <summary>
        /// CanvagGroupの表示
        /// </summary>
        /// <param name="_self"></param>
        public static void ShowCanvas( this CanvasGroup _self )
        {
            _self.alpha = 1;
            _self.blocksRaycasts = true;
            _self.interactable = true;
        }

        /// <summary>
        /// CanvagGroupの非表示
        /// </summary>
        /// <param name="_self"></param>
        public static void HideCanvas( this CanvasGroup _self )
        {
            _self.alpha = 0;
            _self.blocksRaycasts = false;
            _self.interactable = false;
        }
    }
}
