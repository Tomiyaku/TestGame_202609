#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeIcf.Extensions
{
    /// <summary>
    /// 列挙型拡張処理
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 列挙型をInt型に変換する
        /// </summary>
        /// <param name="_self">列挙型</param>
        /// <returns>変換後のInt値</returns>
        public static int ToInt( this System.Enum _self ) => System.Convert.ToInt32( _self );

        /// <summary>
        /// 列挙型をByte型に変換する
        /// </summary>
        /// <param name="_self">列挙型</param>
        /// <returns>変換後のByte値</returns>
        public static byte ToByte( this System.Enum _self ) => System.Convert.ToByte( _self );

        /// <summary>
        /// 列挙型をuint型に変換する
        /// </summary>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static uint ToUint( this System.Enum _self ) => System.Convert.ToUInt32( _self );

        /// <summary>
        /// int型を指定の列挙型に変換する
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="_self">int値</param>
        /// <returns>変換後の列挙型</returns>
        public static T ToEnum<T>( this int _self ) where T : System.Enum => ( T )System.Enum.ToObject( typeof( T ), _self );

        /// <summary>
        /// Byte型を指定の列挙型に変換する
        /// </summary>
        /// <typeparam name="T">列挙型の型</typeparam>
        /// <param name="_self">int値</param>
        /// <returns>変換後の列挙型</returns>
        public static T ToEnum<T>( this byte _self ) where T : System.Enum => ( T )System.Enum.ToObject( typeof( T ), _self );

        /// <summary>
        /// uint型を指定の列挙型に変換する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static T ToEnum<T>( this uint _self ) where T : System.Enum => ( T )System.Enum.ToObject( typeof( T ), _self );

#if UNITY_EDITOR
        public static System.Enum EnumParamField( string _label, System.Enum _value )
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField( _label );
                _value = EditorGUILayout.EnumPopup( _value );
            }

            return _value;
        }
#endif
    }
}