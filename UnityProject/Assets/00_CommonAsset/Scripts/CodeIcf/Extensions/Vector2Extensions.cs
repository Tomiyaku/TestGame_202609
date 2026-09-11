using UnityEngine;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// Vector2型拡張処理
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Vector2をXZに値を入れたVector3へ変換する
        /// </summary>
        /// <returns>The xz.</returns>
        /// <param name="_self">Self.</param>
        /// <param name="_y">Y軸の値 デフォルトは0</param>
        public static Vector3 ToXZ( this Vector2 _self, float _y = 0 ) => new Vector3( _self.x, _y, _self.y );

        /// <summary>
        /// Vector2をXYに値を入れたVector3へ変換する
        /// </summary>
        /// <returns>The xy.</returns>
        /// <param name="_self">Self.</param>
        /// <param name="_z">Z軸の値 デフォルトは0</param>
        public static Vector3 ToXY( this Vector2 _self, float _z = 0 ) => new Vector3( _self.x, _self.y, _z );

        /// <summary>
        /// Vector2のX値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector2 SetX( this Vector2 _self, float _x )
        {
            Vector2 ret = _self;
            ret.x = _x;
            return ret;
        }

        /// <summary>
        /// Vector2のY値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public static Vector2 SetY( this Vector2 _self, float _y )
        {
            Vector2 ret = _self;
            ret.y = _y;
            return ret;
        }

        /// <summary>
        /// Vector2のX値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector2 AddX( this Vector2 _self, float _x )
        {
            Vector2 ret = _self;
            ret.x += _x;
            return ret;
        }

        /// <summary>
        /// Vector2のY値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public static Vector2 AddY( this Vector2 _self, float _y )
        {
            Vector2 ret = _self;
            ret.y += _y;
            return ret;
        }

        /// <summary>
        /// Vector2の各値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public static Vector2 Add( this Vector2 _self, float _x, float _y )
        {
            Vector2 ret = _self;
            ret.x += _x;
            ret.y += _y;

            return ret;
        }

        /// <summary>
        /// Vector2の各値に乗算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_m"></param>
        /// <returns></returns>
        public static Vector2 Multiplication( this Vector2 _self, Vector2 _m )
        {
            return Multiplication( _self, _m.x, _m.y );
        }

        /// <summary>
        /// Vector2の各値に乗算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_mx"></param>
        /// <param name="_my"></param>
        /// <returns></returns>
        public static Vector2 Multiplication( this Vector2 _self, float _mx, float _my )
        {
            Vector2 ret = _self;
            ret.x *= _mx;
            ret.y *= _my;

            return ret;
        }
    }
}