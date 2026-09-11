using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.Extensions
{
#if UNITY_EDITOR
    /// <summary>
    /// EditorでのSafeAreaのシミュレート用パラメータ
    /// </summary>
    public class SafeAreaEditParam : ScriptableSingleton<SafeAreaEditParam>
    {
        /// <summary>SafeAreのシミュレート実行フラグ</summary>
        public bool IsSimurate = true;
    }
#endif

    /// <summary>
    /// iOSでSafeAreへの自動対応とEditor上でのシミュレート
    /// </summary>
    [RequireComponent( typeof( RectTransform ) )]
    public class SafeArea : MonoBehaviour
    {
#pragma warning disable CS0414
        /// <summary>ステータスバーの表示領域を確保するかのフラグ</summary>
        [SerializeField]
        private bool m_IsViewStatusBar = false;
        /// <summary>ステータスバーの領域</summary>
        [SerializeField]
        private int m_StatusBarHeight = 40;
#pragma warning restore CS0414
        /// <summary>画面上方向への調整フラグ</summary>
        [SerializeField]
        private bool m_IsAdjustTop = true;
        /// <summary>画面下方向への調整フラグ</summary>
        [SerializeField]
        private bool m_IsAdjustBottom = true;
        /// <summary>画面左方向への調整フラグ</summary>
        [SerializeField]
        private bool m_IsAdjustLeft = true;
        /// <summary>画面右方向への調整フラグ</summary>
        [SerializeField]
        private bool m_IsAdjustRight = true;

        void Start()
        {
            SettingSafeArea();
        }

        /// <summary>
        /// SafeAreaをRectTransformに反映させる
        /// </summary>
        public void SettingSafeArea()
        {
            Rect area = Screen.safeArea;
            RectTransform rt = ( RectTransform )transform;

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2( 0.5f, 0.5f );

            //Debug.Log( "ScreenSize:" + Screen.width + "x" + Screen.height + " SafeArea:" + area.width + "x" + area.height );

#if UNITY_IOS
#if UNITY_EDITOR
        SafeAreaEditParam param = SafeAreaEditParam.instance;
        if( param.IsSimurate )
        {
            if( !GetSafeArea( out area ) ) area = Screen.safeArea;
        }
#endif
        area = GetAdjustRect( area, Screen.width, Screen.height );

        if( m_IsViewStatusBar )
        {
            float top = Screen.height - area.yMax;
            if( top < m_StatusBarHeight )
            {
                area.yMax = Screen.height - m_StatusBarHeight;
            }
        }
#endif
            Vector2 anchorMin = area.position;
            Vector2 anchorMax = area.position + area.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor上でのみのSafeAreaの設定
        /// </summary>
        /// <param name="_w">画面幅</param>
        /// <param name="_h">画面高</param>
        public void SetSafeAreaToEditor( int _w, int _h, bool _isUseSafeArea )
        {
            Rect area = Screen.safeArea;
            RectTransform rt = ( RectTransform )transform;

            if( _isUseSafeArea )
            {
                if( !GetSafeArea( _w, _h, out area ) ) area = Screen.safeArea;

                area = GetAdjustRect( area, _w, _h );

                if( m_IsViewStatusBar )
                {
                    float top = Screen.height - area.yMax;
                    if( top < m_StatusBarHeight )
                    {
                        area.yMax = Screen.height - m_StatusBarHeight;
                    }
                }

                Vector2 anchorMin = area.position;
                Vector2 anchorMax = area.position + area.size;
                anchorMin.x /= _w;
                anchorMin.y /= _h;
                anchorMax.x /= _w;
                anchorMax.y /= _h;
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
            }
            else
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
            }
        }
#endif

        /// <summary>
        /// 各画面方向のフラグからSafeAreaを適応するかを判定しRectを返す
        /// </summary>
        /// <returns>フラグから調整を行なったSafeArea</returns>
        /// <param name="_saveArea">SafeArea</param>
        /// <param name="_width">画面幅</param>
        /// <param name="_height">画面高</param>
        private Rect GetAdjustRect( Rect _saveArea, int _width, int _height )
        {
            float x = _saveArea.x;
            float y = _saveArea.y;
            float w = _saveArea.width;
            float h = _saveArea.height;

            if( !m_IsAdjustTop )
            {//上方向のSafeAreaを適応させない
                h = _height - y; ;
            }

            if( !m_IsAdjustBottom )
            {//下方向のSafeAreaを適応させない
                h += y;
                y = 0;
            }

            if( !m_IsAdjustLeft )
            {//左方向のSafeAreaを適応させない
                w += x;
                x = 0;
            }

            if( !m_IsAdjustRight )
            {//右方向のSafeAreaを適応させない
                w = _width - x;
            }

            return new Rect( x, y, w, h );
        }

#if UNITY_EDITOR
        /// <summary>
        /// SafeAreaが存在する解像度一覧
        /// </summary>
        public static Vector2Int[] Resolutions = {
      new Vector2Int(1125, 2436), // iPhone X or Xs
      new Vector2Int( 828, 1792), // iPhone XR
      new Vector2Int(1242, 2688), // iPhone Xs Max
      new Vector2Int(1668, 2388), // iPad Pro 11
      new Vector2Int(2048, 2732)  // iPad Pro 12.9 3rd
    };

        /// <summary>
        /// 解像度毎のSafeArea適応後の表示領域
        /// </summary>
        public static Rect[,] SafeAreaResolutions = {
      { new Rect(0, 102, 1125, 2202), new Rect(132, 63, 2172, 1062) },  // iPhone X or Xs
      { new Rect(0,  68,  828, 1636), new Rect( 88, 42, 1616,  786) },  // iPhon XR
      { new Rect(0, 102, 1242, 2454), new Rect(132, 63, 2424, 1179) },  // iPhone X Max
      { new Rect(0,  40, 1668, 2348), new Rect(  0, 40, 2388, 1628) },  // iPad Pro 11
      { new Rect(0,  40, 2048, 2692), new Rect(  0, 40, 2732, 2008) }   // iPad Pro 12.9 3rd
    };

        /// <summary>
        /// 現在の画面が特定の解像度の場合にSafeAreaを取得する
        /// </summary>
        /// <returns><c>true</c>SafeAreaの対象解像度<c>false</c>それ以外</returns>
        /// <param name="_outSafeArea">SafeAreaに対応済みのRect</param>
        private bool GetSafeArea( out Rect _outSafeArea )
        {
            _outSafeArea = Rect.zero;

            string[] list = UnityStats.screenRes.Split( 'x' );

            // Gameの表示サイズを取得
            int width = int.Parse( list[ 0 ] );
            int height = int.Parse( list[ 1 ] );

            //SafeAreaが存在する解像度に一致する場合にSafeAre対応のRectを返す
            for( int i = 0; i < Resolutions.Length; i++ )
            {
                Vector2Int size = Resolutions[ i ];

                if( width == size.x && height == size.y )
                {
                    _outSafeArea = GetAdjustRect( SafeAreaResolutions[ i, 0 ], size.x, size.y );
                    return true;
                }
                else if( height == size.x && width == size.y )
                {
                    _outSafeArea = GetAdjustRect( SafeAreaResolutions[ i, 1 ], size.x, size.y );
                    return true;
                }
            }
            //SafeAreaがない
            return false;
        }

        /// <summary>
        /// 現在の画面が特定の解像度の場合にSafeAreaを取得する
        /// </summary>
        /// <returns><c>true</c>SafeAreaの対象解像度<c>false</c>それ以外</returns>
        /// <param name="_w">画面幅</param>
        /// <param name="_h">画面高</param>
        /// <param name="_outSafeArea">SafeAreaに対応済みのRect</param>
        private bool GetSafeArea( int _w, int _h, out Rect _outSafeArea )
        {
            _outSafeArea = Rect.zero;

            //SafeAreaが存在する解像度に一致する場合にSafeAre対応のRectを返す
            for( int i = 0; i < Resolutions.Length; i++ )
            {
                Vector2Int size = Resolutions[ i ];

                if( _w == size.x && _h == size.y )
                {
                    _outSafeArea = GetAdjustRect( SafeAreaResolutions[ i, 0 ], size.x, size.y );
                    return true;
                }
                else if( _h == size.x && _w == size.y )
                {
                    _outSafeArea = GetAdjustRect( SafeAreaResolutions[ i, 1 ], size.x, size.y );
                    return true;
                }
            }
            return false;
        }

        [CustomEditor( typeof( SafeArea ) )]
        public class SafeAreaEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

#if UNITY_IOS
            SafeAreaEditParam param = SafeAreaEditParam.instance;

            EditorGUILayout.BeginVertical( GUI.skin.box );
            {
                EditorGUILayout.LabelField( "iOS Editor設定" );

                EditorGUI.indentLevel++;

                using( new EditorGUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField( "SafeAreaのシミュレート" );
                    param.IsSimurate = EditorGUILayout.Toggle( param.IsSimurate );
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
#endif
            }
        }
#endif

    }
}