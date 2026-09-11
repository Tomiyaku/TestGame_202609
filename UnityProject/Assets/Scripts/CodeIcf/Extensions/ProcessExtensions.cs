using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.Extensions
{
    /// <summary>
    /// 汎用拡張処理
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// 画面比率によるカメラサイズの変更するための値の取得
        /// </summary>
        /// <returns>変更後のカメラサイズ</returns>
        public static float GetCameraOrthographicUISize()
        {
#if UNITY_EDITOR
            string[] list = UnityEditor.UnityStats.screenRes.Split( 'x' );

            float width = float.Parse( list[ 0 ] );
            float height = float.Parse( list[ 1 ] );

            float size = height / width;
#else
        float size = ( float )Screen.currentResolution.height / ( float )Screen.currentResolution.width;
#endif
            size = size.FloatRoundDown( 2 );// Mathf.Floor( size * 100f ) / 100f;

            return Mathf.Max( 6.67f, size * 3.75f );
        }

        public const float CAMERA_BASE_SIZE = 34f;

        /// <summary>
        /// 画面比率によるカメラサイズの変更するための値の取得
        /// </summary>
        /// <returns>変更後のカメラサイズ</returns>
        public static float GetCameraOrthographicSize()
        {
#if UNITY_EDITOR
            string[] list = UnityEditor.UnityStats.screenRes.Split( 'x' );

            float width = float.Parse( list[ 0 ] );
            float height = float.Parse( list[ 1 ] );

            float size = height / width;
#else
        float size = ( float )Screen.currentResolution.height / ( float )Screen.currentResolution.width;
#endif
            size = size.FloatRoundDown( 2 );// Mathf.Floor( size * 100f ) / 100f;

            //Debug.Log( size * 20.5f );

            return Mathf.Max( CAMERA_BASE_SIZE, size * 20.5f );
        }

        private const int DEFAULT_FIELD_OF_VIEW = 56;

        public static float GetCameraFieldOfView()
        {
#if UNITY_EDITOR
            string[] list = UnityEditor.UnityStats.screenRes.Split( 'x' );

            float width = float.Parse( list[ 0 ] );
            float height = float.Parse( list[ 1 ] );

            float size = width / height;
#else
        float size = ( float )Screen.currentResolution.width / ( float )Screen.currentResolution.height;
#endif
            int s = DEFAULT_FIELD_OF_VIEW - Mathf.FloorToInt( size * 100f );

            return Mathf.Max( DEFAULT_FIELD_OF_VIEW, DEFAULT_FIELD_OF_VIEW + s );
        }

        /// <summary>
        /// float型の小数点以下を指定の桁数で切り捨て
        /// </summary>
        /// <returns>計算結果</returns>
        /// <param name="_self">元の値</param>
        /// <param name="_digit">小数点以下の桁数</param>
        public static float FloatRoundDown( this float _self, int _digit )
        {
            float magni = Mathf.Pow( 10, _digit );

            float result = Mathf.Floor( _self * magni );

            return result / magni;
        }

        /// <summary>単位をアルファベット1文字で表した文字列</summary>
        public static readonly string[] UNIT_STR_LIST = { "", "K", "M", "G", "T", "P", "Z", "Y" };

        /// <summary>
        /// 引数を10の3乗毎に単位を追加した文字列に変換して返す
        /// </summary>
        /// <returns>単位の追加した文字列</returns>
        /// <param name="_self">元の値</param>
        public static string GetUnitValueStr( this int _self )
        {
            int digit = 0;
            float result;

            for( result = _self; result >= 1000f; result /= 1000f )
            {
                digit++;
            }

            return result.ToString( digit < 1 ? "F0" : "F1" ) + UNIT_STR_LIST[ digit ];
        }

        /// <summary>
        /// 引数を10の3乗毎に単位を追加した文字列に変換して返す
        /// </summary>
        /// <returns>単位の追加した文字列</returns>
        /// <param name="_self">元の値</param>
        public static string GetUnitValueStr( this double _self )
        {
            int digit = 0;

            double result = 0;

            for( result = _self; result >= 1000; result /= 1000 )
            {
                digit++;
            }

            return result.ToString( digit < 1 ? "F0" : "F1" ) + UNIT_STR_LIST[ digit ];
        }


        /// <summary>
        /// Colorのアルファ値の変更
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_a">変更後のアルファ値</param>
        /// <returns>アルファ値を変更したColor</returns>
        public static Color SetA( this Color _self, float _a )
        {
            Color ret = _self;
            ret.a = _a;
            return ret;
        }        

#if UNITY_EDITOR
        public static int IntParamField( string _label, int _value )
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField( _label );
                _value = EditorGUILayout.IntField( _value );
            }

            return _value;
        }

        public static float FloatParamField( string _label, float _value )
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField( _label );
                _value = EditorGUILayout.FloatField( _value );
            }

            return _value;
        }

        public static double DoubleParamField( string _label, double _value )
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField( _label );
                _value = EditorGUILayout.DoubleField( _value );
            }

            return _value;
        }
#endif
    }
}