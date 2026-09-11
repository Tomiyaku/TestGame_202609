using System;

using CodeIcf.Extensions;

namespace jp.co.liica.q2.Common
{
    /// <summary>
    /// ステージ特殊設定
    /// </summary>
    public class StageSpecialConfig
    {
        /// <summary>
        /// ステージ特殊設定フラグ
        /// </summary>
        [Flags]
        public enum SpecialStageSettingFlags
        {
            /// <summary>ステージで使用可能キャラを設定</summary>
            UsageAvailableCharacter = 1 << 0,
            /// <summary>キャラクターのスキンをロボに強制的に変更</summary>
            ChengeRoboSkin = 1 << 1,
            /// <summary>オンラインプレイ専用</summary>
            OnlinePlayOnly = 1 << 2,
        }

        /// <summary>
        /// 全てのフラグを立てた<see cref="SpecialStageSettingFlags"/>を返す
        /// </summary>
        /// <returns></returns>
        public static SpecialStageSettingFlags FlagEverything => ( ~( -1 << Enum.GetNames( typeof( SpecialStageSettingFlags ) ).Length ) ).ToEnum<SpecialStageSettingFlags>();

        /// <summary>
        /// 全てのフラグを下した<see cref="SpecialStageSettingFlags"/>を返す
        /// </summary>
        /// <returns></returns>
        public static SpecialStageSettingFlags FlagNothing => 0.ToEnum<SpecialStageSettingFlags>();
    }
}