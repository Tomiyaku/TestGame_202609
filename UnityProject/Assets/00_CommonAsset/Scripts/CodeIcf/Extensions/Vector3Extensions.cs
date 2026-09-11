using UnityEngine;

namespace CodeIcf.Extensions
{
    /// <summary>
    ///  Vector3型拡張処理
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Vector3のX値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector3 SetX(this Vector3 _self, float _x)
        {
            Vector3 ret = _self;
            ret.x = _x;
            return ret;
        }

        /// <summary>
        /// Vector3のY値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public static Vector3 SetY(this Vector3 _self, float _y)
        {
            Vector3 ret = _self;
            ret.y = _y;
            return ret;
        }

        /// <summary>
        /// Vector3のZ値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public static Vector3 SetZ(this Vector3 _self, float _z)
        {
            Vector3 ret = _self;
            ret.z = _z;
            return ret;
        }

        /// <summary>
        /// Vector3の各値を設定して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public static Vector3 Set(this Vector3 _self, float _x, float _y, float _z)
        {
            Vector3 ret = _self;
            ret.x = _x;
            ret.y = _y;
            ret.z = _z;

            return ret;
        }

        /// <summary>
        /// Vector3のX値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector3 AddX(this Vector3 _self, float _x)
        {
            return Add(_self, _x, 0, 0);
        }

        /// <summary>
        /// Vector3のY値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public static Vector3 AddY(this Vector3 _self, float _y)
        {
            return Add(_self, 0, _y, 0);
        }

        /// <summary>
        /// Vector3のZ値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public static Vector3 AddZ(this Vector3 _self, float _z)
        {
            return Add(_self, 0, 0, _z);
        }

        /// <summary>
        /// Vector3のY値に加算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public static Vector3 Add(this Vector3 _self, float _x, float _y, float _z)
        {
            Vector3 ret = _self;
            ret.x += _x;
            ret.y += _y;
            ret.z += _z;

            return ret;
        }

        /// <summary>
        /// Vector3の各値に乗算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_m"></param>
        /// <returns></returns>
        public static Vector3 Multiplication(this Vector3 _self, Vector3 _m)
        {
            return Multiplication(_self, _m.x, _m.y, _m.z);
        }

        /// <summary>
        /// Vector3の各値に乗算して返す
        /// </summary>
        /// <param name="_self"></param>
        /// <param name="_mx"></param>
        /// <param name="_my"></param>
        /// <returns></returns>
        public static Vector3 Multiplication(this Vector3 _self, float _mx, float _my, float _mz)
        {
            Vector3 ret = _self;
            ret.x *= _mx;
            ret.y *= _my;
            ret.z *= _mz;

            return ret;
        }

        public static Vector2 ToVector2(this Vector3 _self)
        {
            return _self;
        }
    }
}