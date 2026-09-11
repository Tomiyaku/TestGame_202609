using System;

using CodeIcf.Extensions;

namespace jp.co.liica.q2.Common
{
    /// <summary>
    /// ステージ中の使用可能キャラクター設定関連
    /// </summary>
    public static class UsageAvailableCharacter
    {
        /// <summary>ステージ中で使用できるキャラクターを設定するためのフラグ</summary>
        /// <remarks>ビットが立っているキャラのみ使用可能 </remarks>
        [Flags]
        public enum UsageAvailableFlag : int
        {
            Normal = 1 << 0,
            Pitcher = 1 << 1,
            Jump = 1 << 2,
            Heavy = 1 << 3,
            Balloon = 1 << 4,
            Bomber = 1 << 5,
            Electric = 1 << 6,
            Magnet = 1 << 7,
            Oustretched = 1 << 8,
            Diver = 1 << 9,
            Ball = 1 << 10,
            Fire = 1 << 11,
            Glue = 1 << 12,
            Gravity = 1 << 13,
            Friction = 1 << 14,
            Brawler = 1 << 15,
            Eraser = 1 << 16,
            Time = 1 << 17,
        }

        /// <summary>
        /// 全てのフラグを立てた<see cref="UsageAvailableFlag"/>を返す
        /// </summary>
        /// <returns></returns>
        public static UsageAvailableFlag FlagEverything => ( ~( -1 << Enum.GetNames( typeof( UsageAvailableFlag ) ).Length ) ).ToEnum<UsageAvailableFlag>();

        /// <summary>
        /// 全てのフラグを下した<see cref="UsageAvailableFlag"/>を返す
        /// </summary>
        /// <returns></returns>
        public static UsageAvailableFlag FlagNothing =>0.ToEnum<UsageAvailableFlag>();
    }
}