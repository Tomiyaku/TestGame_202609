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

        /// <summary>
        /// この列挙型の変数と一致するものが1つでも存在するか
        /// </summary>
        /// <typeparam name="T">列挙型</typeparam>
        /// <param name="_self">対象の列挙型</param>
        /// <param name="_targetList">判定の列挙型一覧</param>
        /// <returns></returns>
        public static bool IsMatchAny<T>( this T _self, params T[] _targetList ) where T : System.Enum
        {
            foreach( T target in _targetList )
            {
                if( _self.Equals( target ) ) return true;
            }

            return false;
        }

        /// <summary>　　　
        /// FlagAtribueのついたEnumに対してシフト数を指定して値が一致するかの判定
        /// </summary>
        /// <typeparam name="T">列挙型</typeparam>
        /// <param name="_self">対象の列挙型</param>
        /// <param name="_bitSift">ID(シフト数)</param>
        /// <returns></returns>
        public static bool HashFlagFromID<T>( this T _self, int _bitSift ) where T : System.Enum
        {
            T target = ( 1 << _bitSift ).ToEnum<T>();

            return _self.HasFlag( target );
        }

        /// <summary>
        /// FlagAtribueのついたEnumに対して全て有効な状態を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static T Everything<T>() where T : System.Enum
        {
            return ( ~( -1 << System.Enum.GetNames( typeof( T ) ).Length ) ).ToEnum<T>();
        }

        /// <summary>
        /// FlagAtribueのついたEnumに対して全て無効な状態を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static T Nothing<T>() where T : System.Enum
        {
            return 0.ToEnum<T>();
        }

        /// <summary>
        /// FlagAtribueのついたEnumのフラグが全て立っている状態の判定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static bool IsEverything<T>( this T _self ) where T : System.Enum
        {
            return _self.ToInt() == ~( -1 << System.Enum.GetNames( typeof( T ) ).Length );
        }

        /// <summary>
        /// FlagAtribueのついたEnumのフラグが全て降りている状態の判定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static bool IsNothing<T>( this T _self ) where T : System.Enum
        {
            return _self.ToInt() == 0;
        }

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