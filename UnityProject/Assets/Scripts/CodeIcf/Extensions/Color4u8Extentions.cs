#if UNITY_SWITCH

using UnityEngine;
using nn.util;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// <see cref="Color4u8"/>の拡張クラス
    /// </summary>
    public static class Color4u8Extentions
    {
        /// <summary>色の値の上限値</summary>
        private const float COLOR_VALUE_MAX = 255f;

        /// <summary>
        /// <see cref="Color4u8"/>を<see cref="Color"/>に変換
        /// </summary>
        /// <param name="_base">元の<see cref="Color4u8"/>型の値</param>
        /// <returns>引数を<see cref="Color"/>型に変換した値</returns>
        public static Color ConvertColor( this Color4u8 _self )
        {
            return new Color( _self.RedRatio(), _self.GreenRatio(), _self.BuleRatio(), _self.AlphaRatio() );
        }

        /// <summary>
        /// <see cref="Color4u8.r"/>の値を0~1の割合として返す
        /// </summary>
        /// <param name="_base">元の<see cref="Color4u8"/>型の値</param>
        /// <returns> <see cref="Color4u8.r"/>の値を0~1に変換した値</returns>
        public static float RedRatio( this Color4u8 _self )
        {
            return _self.r / COLOR_VALUE_MAX;
        }

        /// <summary>
        /// <see cref="Color4u8.g"/>の値を0~1の割合として返す
        /// </summary>
        /// <param name="_base">元の<see cref="Color4u8"/>型の値</param>
        /// <returns> <see cref="Color4u8.g"/>の値を0~1に変換した値</returns>
        public static float GreenRatio( this Color4u8 _self )
        {
            return _self.g / COLOR_VALUE_MAX;
        }

        /// <summary>
        /// <see cref="Color4u8.b"/>の値を0~1の割合として返す
        /// </summary>
        /// <param name="_base">元の<see cref="Color4u8"/>型の値</param>
        /// <returns> <see cref="Color4u8.b"/>の値を0~1に変換した値</returns>
        public static float BuleRatio( this Color4u8 _self )
        {
            return _self.b / COLOR_VALUE_MAX;
        }

        /// <summary>
        /// <see cref="Color4u8.a"/>の値を0~1の割合として返す
        /// </summary>
        /// <param name="_self">元の<see cref="Color4u8"/>型の値</param>
        /// <returns> <see cref="Color4u8.a"/>の値を0~1に変換した値</returns>
        public static float AlphaRatio( this Color4u8 _self )
        {
            return _self.a / COLOR_VALUE_MAX;
        }
    }
}

#endif