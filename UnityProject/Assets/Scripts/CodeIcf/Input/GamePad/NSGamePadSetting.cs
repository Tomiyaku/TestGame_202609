using System;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// ゲームがサポートするコントローラーの持ち方・数の設定
    /// </summary>
    public struct NSGamePadSetting
    {
        /// <summary>プロコンが使用可能か</summary>
        public bool IsFullKey;
        /// <summary>Joyコン２本持ちが使用可能か</summary>
        public bool IsJoyDuel;
        /// <summary>携帯モードが使用可能か</summary>
        public bool IsHandheld;
        /// <summary>Joyコン(L)が使用可能か</summary>
        public bool IsJoyLeft;
        /// <summary>Joyコン(R)が使用可能か</summary>
        public bool IsJoyRight;
        /// <summary>コントローラー接続最小数</summary>
        public byte MinGamePadCount;
        /// <summary>コントローラー接続最大数</summary>
        public byte MaxGamePadCount;
        /// <summary>Joyコンの持ち方が横持ちか</summary>
        public bool IsHoldHorizontalType;
        /// <summary>コントローラーの接続が切れた場合に自動でサポートアプレットを呼び出すか</summary>
        public bool IsAutoCheckDeviceConnect;
        /// <summary>コントローラーの最小接続数未満の場合、一定時間後にサポートアプレットを表示するか</summary>
        public bool IsAutoCheckMinimumDeviceCount;

        /// <summary>
        /// デフォルト設定
        /// </summary>
        /// <returns></returns>
        public static NSGamePadSetting DefaultSetting()
        {
            return new NSGamePadSetting
            {
                IsFullKey = true,
                IsJoyDuel = true,
                IsHandheld = true,
                IsJoyLeft = true,
                IsJoyRight = true,
                MinGamePadCount = 1,
                MaxGamePadCount = 4,
                IsHoldHorizontalType = true,
                IsAutoCheckDeviceConnect  = true,
                IsAutoCheckMinimumDeviceCount = true,
            };
        }
    }
}
