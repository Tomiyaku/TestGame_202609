namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// ゲームがサポートするコントローラーの持ち方・数の設定
    /// </summary>
    public struct GamePadSetting
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
        /// <summary>コントローラーサポートアプレット表示の際、接続済みのコントローラーの接続を維持するかのフラグ</summary>
        public bool IsEnableTakeOverConnection;

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="_source"></param>
        public GamePadSetting( GamePadSetting _source )
        {
            IsFullKey = _source.IsFullKey;
            IsJoyDuel = _source.IsJoyDuel;
            IsHandheld = _source.IsHandheld;
            IsJoyLeft = _source.IsJoyLeft;
            IsJoyRight = _source.IsJoyRight;
            MinGamePadCount = _source.MinGamePadCount;
            MaxGamePadCount = _source.MaxGamePadCount;
            IsHoldHorizontalType = _source.IsHoldHorizontalType;
            IsAutoCheckDeviceConnect = _source.IsAutoCheckDeviceConnect;
            IsAutoCheckMinimumDeviceCount = _source.IsAutoCheckMinimumDeviceCount;
            IsEnableTakeOverConnection = _source.IsEnableTakeOverConnection;
        }

        /// <summary>
        /// デフォルト設定
        /// </summary>
        /// <returns></returns>
        public static GamePadSetting DefaultSetting()
        {
            return new GamePadSetting
            {
                IsFullKey = true,
                IsJoyDuel = true,
                IsHandheld = true,
                IsJoyLeft = false,
                IsJoyRight = false,
                MinGamePadCount = 1,
                MaxGamePadCount = 4,
                IsHoldHorizontalType = true, //コントローラーサポートアプレットを表示する際JoyLeft、JoyRight以外を有効にする場合にIsHoldHorizontalTypeをtrueに設定しておく必要がある
                IsAutoCheckDeviceConnect = true,
                IsAutoCheckMinimumDeviceCount = true,
                IsEnableTakeOverConnection = true,
            };
        }
    }
}
