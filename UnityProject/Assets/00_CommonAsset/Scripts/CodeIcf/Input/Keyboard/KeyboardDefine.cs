namespace CodeIcf.Input.Keyboard
{
    /// <summary>
    /// キーボードの入力をコントローラー入力に変換する際の種類
    /// </summary>
    public enum KeyboardInputConvertType
    {
        /// <summary>いづれかのキーを押下</summary>
        AnyKey,
        /// <summary>UI</summary>
        UI,
        /// <summary>ステージ中のプレイヤー操作</summary>
        Player
    }

    /// <summary>
    /// キーボードのキー状態
    /// </summary>
    public enum KeyboardKeyState
    {
        None = 0,
        Down,
        Hold,
        Up,
    }

    public class KeyboardDefine
    {
        public const int KEY_STATES_INDEX_OK = 0;
        public const int KEY_STATES_INDEX_CANCEL = 1;
        public const int KEY_STATES_INDEX_MENU = 2;
        public const int KEY_STATES_INDEX_LEFT_ARROW_UP = 3;
        public const int KEY_STATES_INDEX_LEFT_ARROW_DOWN = 4;
        public const int KEY_STATES_INDEX_LEFT_ARROW_LEFT = 5;
        public const int KEY_STATES_INDEX_LEFT_ARROW_RIGHT = 6;
        public const int KEY_STATES_INDEX_RIGHT_ARROW_UP = 7;
        public const int KEY_STATES_INDEX_RIGHT_ARROW_DOWN = 8;
        public const int KEY_STATES_INDEX_RIGHT_ARROW_LEFT = 9;
        public const int KEY_STATES_INDEX_RIGHT_ARROW_RIGHT = 10;
        public const int KEY_STATES_INDEX_JUMP = 11;
        public const int KEY_STATES_INDEX_PUNCH = 12;
        public const int KEY_STATES_INDEX_CATCH = 13;
        public const int KEY_STATES_INDEX_SPACIAL = 14;
        public const int KEY_STATES_INDEX_EMOTE_1 = 15;
        public const int KEY_STATES_INDEX_EMOTE_2 = 16;
        public const int KEY_STATES_INDEX_EMOTE_3 = 17;
        public const int KEY_STATES_INDEX_EMOTE_4 = 18;
        public const int KEY_STATE_COUNT = 19;
    }
}