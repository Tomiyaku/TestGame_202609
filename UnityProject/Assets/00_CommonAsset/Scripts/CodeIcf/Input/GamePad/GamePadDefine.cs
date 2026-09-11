namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// ボタン入力のビットフラグ
    /// </summary>
    /// <remarks>
    /// SLとSRは持ち方を変えた際に自動でLとRが割り当てられるためここでは定義しない 
    /// </remarks>
    public enum GamepadKeyId
    {
        None = 0,
        /// <summary>Xbox:A PS:× SW:A</summary>
        A = 0x1,
        /// <summary>Xbox:A PS:○ SW:B</summary>
        B = 0x1 << 1,
        /// <summary>Xbox:X PS:□ SW:X</summary>
        X = 0x1 << 2,
        /// <summary>Xbox:Y PS:△ SW:Y</summary>
        Y = 0x1 << 3,
        /// <summary>Xbox:LB PS:L1 SW:L</summary>
        L = 0x1 << 4,
        /// <summary>Xbox:RB PS:R1 SW:R</summary>
        R = 0x1 << 5,
        /// <summary>Xbox:LT PS:L2 SW:ZL</summary>
        LT = 0x1 << 6,
        /// <summary>Xbox:RT PS:R2 SW:ZR</summary>
        RT = 0x1 << 7,
        /// <summary>十字キー上</summary>
        D_Pad_Up = 0x1 << 8,
        /// <summary>十字キー下</summary>
        D_Pad_Down = 0x1 << 9,
        /// <summary>十字キー左</summary>
        D_Pad_Left = 0x1 << 10,
        /// <summary>十字キー右</summary>
        D_Pad_Right = 0x1 << 11,
        /// <summary>左スティックボタン</summary>
        LS = 0x1 << 12,
        /// <summary>右スティックボタン</summary>
        RS = 0x1 << 13,
        /// <summary>Xbox:Menu PS:Option SW:+</summary>
        Start = 0x1 << 14,
        /// <summary>Xbox:View PS:Share SW:-</summary>
        Select = 0x1 << 15,
    }

    /// <summary>
    /// コントローラーの持ち方
    /// </summary>
    public enum ControllerStyle
    {
        None,
        FullKey = 0x1 << 0,
        Handheld = 0x1 << 1,
        JoyDual = 0x1 << 2,
        JoyLeft = 0x1 << 3,
        JoyRight = 0x1 << 4,
        Keyboard = 0x1 << 5,
        Invalid = 0x1 << 15,
    }

    /// <summary>十字キーの入力を示すビットフラグ</summary>
    public enum DPad
    {
        /// <summary>入力無し</summary>
        None = 0,
        /// <summary>上</summary>
        Up = 0x1,
        /// <summary>下</summary>
        Down = 0x1 << 1,
        /// <summary>左</summary>
        Left = 0x1 << 2,
        /// <summary>右</summary>
        Right = 0x1 << 3,
    }

    /// <summary>
    /// 十字キー or 左スティックをデジタル変換 or 右スティックをデジタル変換での入力状態の取得する際の対象
    /// </summary>
    public enum SelectConfirmInput
    {
        /// <summary>無し</summary>
        None = 0,
        /// <summary>十字キー</summary>
        Dpad = 0x1,
        /// <summary>左スティック入力をデジタルに変換</summary>
        LeftStick_Digital = 0x1 << 1,
        //// <summary>右スティック入力をデジタルに変換</summary>
        RightStick_Digital = 0x1 << 2,
    }

    /// <summary>
    /// <see cref="GamePadInputData.IsMultipleDown(GamepadKeyId, float)"/>の結果
    /// </summary>
    public enum MultipleDownResult : int
    {
        /// <summary>同時押しが成功</summary>
        Success = 1,
        /// <summary>同時押しが失敗</summary>
        Failure = -1,
        /// <summary>キーが不足</summary>
        MissingKey = -2,
        /// <summary>入力履歴が見つからなかった</summary>
        MissingHistory = -3,
    }

    /// <summary>
    /// <see cref="InputDeviceManager.IsHoldingKeyForKeepTime(int, GamepadKeyId, float)"/>の結果
    /// </summary>
    /// <remarks><see cref="Success"/>か<see cref="HistoryEnd"/>であれば指定時間以上押されていると判断して良い</remarks>
    public enum HoldingResult : int
    {
        /// <summary>指定時間以上押し続けている</summary>
        Success = 1,
        /// <summary>入力履歴の上限まで押し続けている(指定時間が入力履歴の一番古いデータより前の場合)</summary>
        HistoryEnd = 0,
        /// <summary>指定のキーを一定時間以上押し続けていない</summary>
        Failure = -1,
        /// <summary>無効なデバイスID</summary>
        InvalidDeviceId = -2,
        /// <summary>指定のキーを現在押し続けていない</summary>
        NotNowHold = -3,
    }

    /// <summary>
    /// <see cref="InputDeviceManager.GetGamePadDeviceID(UnityEngine.InputSystem.InputDevice)の結果"/>
    /// </summary>
    public enum GetGamePadDeviceIDResult : int
    {
        /// <summary>成功</summary>
        Success = 0,
        /// <summary>失敗 : コントローラー以外のデバイス</summary>
        Failed_NotGamePad = -1,
        /// <summary>失敗 : デバッグパッド</summary>
        Failed_DebugPad = -2,
        /// <summary>失敗 : Joyコン２本持ちの場合で左側が未接続</summary>
        Failed_JoyDuel_Missing_Left = -3,
        /// <summary>失敗 : Joyコン２本持ちの場合で右側が未接続</summary>
        Failed_JoyDuel_Missing_Right = -4,
        /// <summary>失敗 : Joyコン２本持ちの場合で両側が未接続</summary>
        Failed_JoyDuel_Missing_Both = -5,
        /// <summary>その他のエラー</summary>
        Failed = -6,
    }

    /// <summary>
    /// コントローラー関連定義
    /// </summary>
    public static class GamePadDefine
    {
        /// <summary>コントローラーが未接続の場合にサポートアプレットを呼び出すまでの時間(秒)</summary>
#if UNITY_SWITCH
        public const float WAIT_TIME_SHOW_CONTROLLER_SUPPOERT = 5f;
#else
        public const float WAIT_TIME_SHOW_CONTROLLER_SUPPOERT = 1f;
#endif
        /// <summary>TVモード/携帯モード切り替え時に追加でコントローラーが未接続の場合にサポートアプレットを呼び出すまでの時間(秒)</summary>
        public const float ADD_WAIT_TIME_CONTROLLER_SUPPORT = 3.5f;
        /// <summary>コントローラーの最小数</summary>
        public const int MIN_CONTROLLER_NUM = 0;
        /// <summary>コントローラーの最大数</summary>
        public const int MAX_CONTROLLER_NUM = 8;
        /// <summary>無効なデバイスID</summary>
        public const int INVALID_DEVICE_ID = -1;
        /// <summary>プレイヤーランプの数</summary>
        public const int PLAYER_LED_COUNT = 4;
        /// <summary>ボタン同時押し判定の有効時間</summary>
        public const float MULTIPLE_DOWN_TIME = 0.1f;

        /// <summary>
        /// コントローラーの色のデータ
        /// </summary>
        public static class ControllerColorIndex
        {
            /// <summary>データの数</summary>
            public const int DATA_SIZE = 4;
            /// <summary>右メインのインデックス</summary>
            public const int INDEX_LM = 0;
            /// <summary>右サブのインデックス</summary>
            public const int INDEX_LS = 1;
            /// <summary>左メインのインデックス</summary>
            public const int INDEX_RM = 2;
            /// <summary>左サブのインデックス</summary>
            public const int INDEX_RS = 3;
        }
    }
}