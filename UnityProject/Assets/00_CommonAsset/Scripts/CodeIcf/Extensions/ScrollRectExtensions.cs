using UnityEngine;
using UnityEngine.UI;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// ScrollRectの拡張機能を提供します
    /// </summary>
    public static class ScrollRectExtensions
    {
        /// <summary>
        /// ScrollRectの上端にGameObjectをあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_obj"></param>
        public static bool ScrollToBottom( this ScrollRect _scrollRect, GameObject _obj, out float _outScrollValue )
        {
            return ScrollToCore( _scrollRect, _obj, 0f, out _outScrollValue );
        }

        /// <summary>
        /// ScrollRectの下端にGameObjectをあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_obj"></param>
        public static bool ScrollToTop( this ScrollRect _scrollRect, GameObject _obj, out float _outScrollValue )
        {
            return ScrollToCore( _scrollRect, _obj, 1f, out _outScrollValue );
        }

        /// <summary>
        /// ScrollRectの縦中央にGameObjectをあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_obj"></param>
        public static bool ScrollToCentering( this ScrollRect _scrollRect, GameObject _obj, out float _outScrollValue )
        {
            return ScrollToCore( _scrollRect, _obj, 0.5f, out _outScrollValue );
        }

        /// <summary>
        /// ScrollRectの縦中央にGameObjectをあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_targetRect"></param>
        public static bool ScrollToCentering( this ScrollRect _scrollRect, RectTransform _targetRect, out float _outScrollValue )
        {
            return ScrollToCore( _scrollRect, _targetRect, 0.5f, out _outScrollValue );
        }

        /// <summary>
        /// ScrollRectのスクロール位置をGameObjectにあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_obj"></param>
        /// <param name="_align">0:下、0.5:中央、1:上</param>
        /// <returns></returns>
        public static bool ScrollToCore( this ScrollRect _scrollRect, GameObject _obj, float _align, out float _outScrollValue )
        {
            return ScrollToCore( _scrollRect, _obj.GetComponent<RectTransform>(), _align, out _outScrollValue );
        }

        /// <summary>
        /// ScrollRectのスクロール位置をGameObjectにあわせる
        /// </summary>
        /// <param name="_scrollRect"></param>
        /// <param name="_rectTransform"></param>
        /// <param name="_align">0:下、0.5:中央、1:上</param>
        /// <returns></returns>
        public static bool ScrollToCore( this ScrollRect _scrollRect, RectTransform _targetRect, float _align, out float _outScrollValue )
        {
            _outScrollValue = 0;

            float contentHeight = _scrollRect.content.rect.height;
            float viewportHeight = _scrollRect.viewport.rect.height;

            // スクロール不要
            if( contentHeight < viewportHeight )
            {
                return true;
            }

            // ローカル座標が contentHeight の上辺を0として負の値で格納されてる
            // これは現在のレイアウト特有なのかもしれないので、要確認
            float targetPos = contentHeight + GetPosY( _targetRect ) + _targetRect.rect.height * _align;
            float gap = viewportHeight * _align; // 上端〜下端あわせのための調整量
            float normalizedPos = ( targetPos - gap ) / ( contentHeight - viewportHeight );

            _outScrollValue = Mathf.Clamp01( normalizedPos );

            return false;
        }

        public static bool ScrollHorizontalToCore( this ScrollRect _scrollRect, RectTransform _targetRect, float _align, out float _outScrollValue )
        {
            _outScrollValue = 0;

            if( !_scrollRect.horizontal ) return true;

            float contentWidth = _scrollRect.content.rect.width;
            float viewportWidth = _scrollRect.viewport.rect.width;

            // スクロール不要
            if( contentWidth < viewportWidth )
            {
                return true;
            }

            // ローカル座標が contentHeight の上辺を0として負の値で格納されてる
            // これは現在のレイアウト特有なのかもしれないので、要確認
            float targetPos = contentWidth + GetPosX( _targetRect ) + _targetRect.rect.width * _align;
            float gap = viewportWidth * _align; // 上端〜下端あわせのための調整量
            float normalizedPos = ( targetPos - gap ) / ( contentWidth - viewportWidth );

            _outScrollValue = Mathf.Clamp01( normalizedPos );

            return false;

        }

        private static float GetPosX( RectTransform _transform )
        {
            return _transform.localPosition.x + _transform.rect.x; //pivotによるズレをrect.yで補正
        }

        private static float GetPosY( RectTransform _transform )
        {
            return _transform.localPosition.y + _transform.rect.y; //pivotによるズレをrect.yで補正
        }
    }
}