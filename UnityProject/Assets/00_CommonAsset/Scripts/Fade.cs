using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using CodeIcf.Extensions;
using CodeIcf.Extensions.Easing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace jp.co.liica.q2.Common
{
    public enum FadeEasingType
    {
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        SineIn,
        SineOut,
        SineInOut,
        ExpIn,
        ExpOut,
        ExpInOut,
        CircIn,
        CircOut,
        CircInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        Linear,
    }

    /// <summary>
    /// フェード
    /// </summary>    
    public class Fade : MonoBehaviour
    {
        /// <summary>インスタンス</summary>
        public static Fade m_Instance = null;
        /// <summary>インスタンス</summary>
        public static Fade Instance => m_Instance;
        /// <summary>フェード処理実行中フラグ</summary>
        private bool m_IsFading = false;
        /// <summary>フェード処理実行中フラグ</summary>
        public bool IsFading => m_IsFading;
        /// <summary>フェードの開始時間</summary>
        private float m_FadeStartTime = -1;
        /// <summary>フェードにかける時間</summary>
        private float m_FadeProcessTime = -1;
        /// <summary>フェードの色</summary>
        private Color m_FadeColor = Color.black;
        /// <summary>フェード終了時に実行する処理</summary>
        private System.Action m_FadeEndProcess = null;
        /// <summary>フェードの処理を行うコルーチン</summary>
        private Coroutine m_FadeCoroutine = null;
        /// <summary><see cref="UnityEngine.UI.Image"/></summary>
        [SerializeField]
        private Image m_FadeImage;

        private void Awake()
        {
            if( m_Instance != null && m_Instance != this )
            {
                Destroy( gameObject );
                return;
            }

            m_Instance = this;

            DontDestroyOnLoad( m_Instance );
        }

        public static void CreateInstanceToEditor()
        {
#if UNITY_EDITOR
            if( m_Instance != null ) return;

            GameObject baseObj = AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/00_CommonAsset/Prefab/FadeCanvas.prefab" );
            GameObject obj = Instantiate( baseObj );
#endif
        }

        /// <summary>
        /// フェードインの開始
        /// </summary>
        /// <param name="_color">フェードの色</param>
        /// <param name="_time">フェードにかける時間</param>
        /// <param name="_endProcess">フェード終了時に実行する処理</param>
        /// <param name="_type">フェードの際の処理の種類</param>
        public void StartFadeIn( Color _color, float _time, System.Action _endProcess, FadeEasingType _type = FadeEasingType.Linear )
        {
            if( m_FadeCoroutine != null )
            {
                StopCoroutine( m_FadeCoroutine );
                m_FadeCoroutine = null;
            }

            m_FadeColor = _color;
            m_FadeStartTime = Time.unscaledTime;
            m_FadeProcessTime = _time;
            m_FadeEndProcess = _endProcess;
            m_FadeImage.color = new Color( m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 1 );

            m_IsFading = true;
            m_FadeCoroutine = StartCoroutine( FadeIn( _type ) );

        }

        private IEnumerator FadeIn( FadeEasingType _type )
        {
            while( true )
            {
                yield return null;

                float time = Time.unscaledTime - m_FadeStartTime;
                float alpha = EasingFloat.GetNowValue( ( EasingType )_type, time, m_FadeProcessTime, 0, 1 );

                m_FadeImage.color = new Color( m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 1 - alpha );

                if( time >= m_FadeProcessTime ) break;
            }

            m_FadeImage.color = m_FadeColor.SetA( 0 );
            m_FadeCoroutine = null;
            m_FadeEndProcess?.Invoke();

            m_IsFading = false;
        }

        /// <summary>
        /// フェードアウトの開始
        /// </summary>
        /// <param name="_color">フェードの色</param>
        /// <param name="_time">フェードにかける時間</param>
        /// <param name="_endProcess">フェード終了時に実行する処理</param>
        /// <param name="_style">フェードの際の処理の種類</param>
        public void StartFadeOut( Color _color, float _time, System.Action _endProcess, FadeEasingType _style = FadeEasingType.Linear )
        {
            if( m_FadeCoroutine != null )
            {
                StopCoroutine( m_FadeCoroutine );
                m_FadeCoroutine = null;
            }

            m_FadeColor = _color;
            m_FadeStartTime = Time.unscaledTime;
            m_FadeProcessTime = _time;
            m_FadeEndProcess = _endProcess;
            m_FadeImage.color = new Color( m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 0 );

            m_IsFading = true;
            m_FadeCoroutine = StartCoroutine( FadeOut( _style ) );
        }

        private IEnumerator FadeOut( FadeEasingType _type )
        {
            while( true )
            {
                yield return null;

                float time = Time.unscaledTime - m_FadeStartTime;
                float alpha = EasingFloat.GetNowValue( ( EasingType )_type, time, m_FadeProcessTime, 0, 1 );

                m_FadeImage.color = new Color( m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, alpha );

                if( time >= m_FadeProcessTime )
                {
                    yield return null;
                    break;
                }
            }

            m_FadeImage.color = m_FadeColor.SetA( 1 );
            m_FadeCoroutine = null;
            m_FadeEndProcess?.Invoke();

            m_IsFading = false;
        }

        /// <summary>
        /// 画面全体を特定の1色で表示
        /// </summary>
        /// <param name="_color"></param>
        public void Show( Color _color )
        {
            if( m_FadeCoroutine != null )
            {
                StopCoroutine( m_FadeCoroutine );
                m_FadeCoroutine = null;
            }

            m_FadeColor = _color;
            m_FadeImage.color = new Color( m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 1 );            
        }

        /// <summary>
        /// フェード演出無しで即時に非表示
        /// </summary>
        public void Hide()
        {
            m_FadeImage.color = m_FadeColor.SetA( 0 );
            m_FadeCoroutine = null;
            m_FadeEndProcess = null;

            m_IsFading = false;
        }
    }
}