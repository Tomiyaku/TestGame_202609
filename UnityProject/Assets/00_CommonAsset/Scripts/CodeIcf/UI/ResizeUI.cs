using UnityEngine;
using UnityEngine.UI;

namespace CodeIcf.UI
{
    /// <summary>
    /// Canvasn直下 に配置するUI表示領域のサイズ調整
    /// </summary>
    public class ResizeUI : MonoBehaviour
    {
        /// <summary>CanvasのRectTransform</summary>
        [SerializeField]
        private RectTransform m_CanvasRectTransform = null;
        /// <summary>RectTransform</summary>
        private RectTransform RectTransform { get; set; } = null;
        /// <summary>想定のCanvasの幅</summary>
        [SerializeField]
        private int m_TargetCanvasWidth = 1920;
        /// <summary>想定のCanvasの高さ</summary>
        [SerializeField]
        private int m_TargetCanvasHeight = 1080;
        /// <summary>最後に確認したScreenの幅</summary>
        private int m_LastCheckCanvasWidth = 0;
        /// <summary>最後に確認したScreenの高さ</summary>
        private int m_LastCheckCanvaseight = 0;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            CanvasScaler canvasScaler = m_CanvasRectTransform.GetComponent<CanvasScaler>();

            if( canvasScaler != null )
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2( m_TargetCanvasWidth, m_TargetCanvasHeight );
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                canvasScaler.referencePixelsPerUnit = 100;
            }
        }

        private void Update()
        {
            if( m_CanvasRectTransform == null ) return;

            int canvasWidth = ( int )m_CanvasRectTransform.sizeDelta.x;
            int canvasHeight = ( int )m_CanvasRectTransform.sizeDelta.y;

            if( m_LastCheckCanvasWidth != canvasWidth || m_LastCheckCanvaseight != canvasHeight )
            {
                m_LastCheckCanvasWidth = canvasWidth;
                m_LastCheckCanvaseight = canvasHeight;

                Resize( m_TargetCanvasWidth, m_TargetCanvasHeight );
            }
        }

        /// <summary>
        /// サイズ変更
        /// </summary>
        /// <param name="_width"></param>
        /// <param name="_hright"></param>
        public void Resize( int _width, int _hright )
        {
            if( m_CanvasRectTransform == null ) return;

            float ws = Mathf.Max( 0, m_CanvasRectTransform.sizeDelta.x - _width );
            float hs = Mathf.Max( 0, m_CanvasRectTransform.sizeDelta.y - _hright );
            ws = Mathf.Abs( ws / 2f );
            hs = Mathf.Abs( hs / 2f );

            RectTransform.offsetMin = new Vector2( ws, hs );
            RectTransform.offsetMax = new Vector2( -ws, -hs );
        }

#if UNITY_EDITOR
        private void Reset()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
#endif
    }
}