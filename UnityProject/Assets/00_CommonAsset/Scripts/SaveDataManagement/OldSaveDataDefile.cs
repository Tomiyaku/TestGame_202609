namespace jp.co.liica.q2.SaveDataManagement
{
    /// <summary>
    /// 初回バージョンのセーブデータ定義
    /// </summary>
    public static class FIrstVersionSavedataDefine
    {
        /// <summary>データバージョンのインデックス</summary>
        public const int INDEX_VERSION = 0;
        /// <summary>データバージョンのサイズ</summary>
        public const int SIZE_VERSION = 1;

        /// <summary>CRCのインデックス</summary>
        public const int INDEX_CRC = INDEX_VERSION + SIZE_VERSION;
        /// <summary>CRCのサイズ</summary>
        /// <remarks>CRCは本来4byteだがインデックスのミスがあったので3byteとして扱う　未使用なので影響はない</remarks>
        public const int SIZE_CRC = 3;

        /// <summary>設定のインデックス</summary>
        public const int INDEX_CONFIG = INDEX_CRC + SIZE_CRC;
        /// <summary>設定のサイズ</summary>
        public const int SIZE_CONFIG = 32;

        /// <summary>プレイヤー状態のインデックス</summary>
        public const int INDEX_PLAYER_INFO = INDEX_CONFIG + SIZE_CONFIG;
        /// <summary>プレイヤー状態のサイズ</summary>
        public const int SIZE_PLAYER_INFO = 256;

        /// <summary>キャラ解放状態のインデックス</summary>
        public const int INDEX_CHARA = INDEX_PLAYER_INFO + SIZE_PLAYER_INFO;
        /// <summary>キャラ解放状態のサイズ</summary>
        public const int SIZE_CHARA = 4;

        /// <summary>カギの取得状態のインデックス</summary>
        public const int INDEX_KEY = INDEX_CHARA + SIZE_CHARA;
        /// <summary>カギの取得状態のサイズ</summary>
        public const int SIZE_KEY = 32;

        /// <summary>メダルの取得状態のインデックス</summary>
        public const int INDEX_MEDAL = INDEX_KEY + SIZE_KEY;
        /// <summary>メダルの取得状態のサイズ</summary>
        public const int SIZE_MEDAL = 128;

        /// <summary>BGMの開放状態のインデックス</summary>
        public const int INDEX_BGM = INDEX_MEDAL + SIZE_MEDAL;
        /// <summary>BGMの解放状態のサイズ</summary>
        public const int SIZE_BGM = 64;

        /// <summary>スキンの開放状態のインデックス</summary>
        public const int INDEX_SKIN = INDEX_BGM + SIZE_BGM;
        /// <summary>スキンの開放状態のサイズ</summary>
        public const int SIZE_SKIN = 32;

        /// <summary>演出終了フラグのインデックス</summary>
        public const int INDEX_PERFORMANCE = INDEX_SKIN + SIZE_SKIN;
        /// <summary>演出終了フラグのサイズ</summary>
        public const int SIZE_PERFORMANCE = 256;

        public const int INDEX_PERFORMANCE_STAGE_OPEN = INDEX_PERFORMANCE;
        public const int SIZE_PERFORMANCE_STAGE_OPEN = 64;

        public const int INDEX_PERFORMANCE_MESSAGE = INDEX_PERFORMANCE_STAGE_OPEN + SIZE_PERFORMANCE_STAGE_OPEN;
        public const int SIZE_PERFORMANCE_MESSAGE = 64;

        /// <summary>実績カウント枠のインデックス</summary>
        public const int INDEX_ACHIVEMENT = INDEX_PERFORMANCE + SIZE_PERFORMANCE;
        public const int INDEX_ACHIVEMENT_DRAW_LENGTH = INDEX_ACHIVEMENT;
        public const int INDEX_ACHIVEMENT_RETRY_COUNT = INDEX_ACHIVEMENT + 4;
        /// <summary>実績カウント枠のサイズ</summary>
        public const int SIZE_ACHIVEMENT = 256;

        /// <summary>ステージの状態のインデックス</summary>
        public const int INDEX_STAGE = INDEX_ACHIVEMENT + SIZE_ACHIVEMENT;
        /// <summary>ステージの状態のサイズ</summary>
        public const int SIZE_STAGE = 1024;

        /// <summary>予約枠のインデックス</summary>
        public const int INDEX_RESERVE = INDEX_STAGE + SIZE_STAGE;
        /// <summary>予約枠のサイズ</summary>
        public const int SIZE_RESERVE = 1024;

        /// <summary>データバージョン0でのセーブデータサイズ</summary>
        public const int SAVEDATA_SIZE = SIZE_VERSION + SIZE_CRC + SIZE_CONFIG + SIZE_PLAYER_INFO + SIZE_CHARA + SIZE_KEY + SIZE_MEDAL + SIZE_BGM + SIZE_SKIN + SIZE_PERFORMANCE + SIZE_ACHIVEMENT + SIZE_STAGE + SIZE_RESERVE;

    }
}