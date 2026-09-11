using UnityEngine;

namespace jp.co.liica.q2.Common
{
#if UNITY_SWITCH
    /// <summary>
    /// Nintendo Switchのみで使用する定数定義
    /// </summary>
    public static class SwitchDefine
    {
        /// <summary>Nintendo Switchで物理エディションのバイナリかを判断するためのScripting Define Symbolsに追加されるシンボル名</summary>
        public const string SWITCH_LIICA_PHYSICAL_EDITION_SYMBOLE_NAME = "SWITCH_LIICA_PHYSICAL_EDITION";
        /// <summary>Nintendo SwitchでAksys Games配信版のバイナリかを判断するためのScripting Define Symbolsに追加されるシンボル名</summary>
        public const string SWITCH_AKSYSGAMES_PUBLISH_SYMBOL_NAME = "SWITCH_AKSYSGAMES_PUBLISH";
#if  SWITCH_LIICA_PHYSICAL_EDITION
        /// <summary>Qを起動するためのアプリインデックス</summary>
        public const int MULTI_PROGRAM_INDEX_Q = 1;
#endif

        /// <summary>Nintendo Switchの携帯モードでの解像度 幅</summary>
        public const float SCREEN_WIDTH = 1280f;
        /// <summary>Nintendo Switchの携帯モードでの解像度 高さ</summary>
        public const float SCREEN_HEIGHT = 720f;

        /// <summary>セーブデータマウント名</summary>
        public const string SAVE_MOUNT_NAME = "Q2";
        /// <summary>セーブデータファイル名</summary>
        public const string SAVE_FILE_NAME = "MySaveData";
        /// <summary>セーブデータファイルパス</summary>
        public static readonly string SAVE_FILE_PATH = string.Format( "{0}:/{1}", SAVE_MOUNT_NAME, SAVE_FILE_NAME );
    }
#endif // UNITY_SWITCH

    /// <summary>
    /// ゲームに関する共通定義
    /// </summary>
    public static class ApplicationDefine
    {
        /// <summary>プレイヤー最小人数</summary>
        public const int PLAYER_MIN_COUNT = 1;
        /// <summary>プレイヤー最大人数</summary>
        public const int PLAYER_MAX_COUNT = 4;

        /// <summary>言語ID : 英語</summary>
        public const int LANGUAGE_ID_EN = 0;
        /// <summary>言語ID : 日本語</summary>
        public const int LANGUAGE_ID_JP = 1;

        /// <summary>プレイヤー表示名のデフォルト</summary>
        public const string DEFAULT_PLAYER_NAME = "{0}P";

        /// <summary>プレイヤー表示名上限文字数</summary>
        public const int PLAYER_NAME_LENGTH_MAX = 10;

        /// <summary>ステージ・メインの数</summary>
        public const int SURFACE_STAGE_MAIN_COUNT = 81;
        /// <summary>ステージ・ブランチの数</summary>
        public const int SURFACE_STAGE_BRANCH_COUNT = 112;
        /// <summary>表面ステージ全体の数</summary>
        public const int SURFACE_STAGE_ALL_COUNT = SURFACE_STAGE_MAIN_COUNT + SURFACE_STAGE_BRANCH_COUNT;

        /// <summary>裏面のメインステージの数</summary>
        public const int REVERSE_STAGE_MAIN_COUNT = 81;
        /// <summary>裏面のブランチステージの数</summary>
        public const int REVERSE_STAGE_BRANCH_COUNT = 112;
        /// <summary>裏面のステージ全体の数</summary>
        public const int REVERSE_STAGE_ALL_COUNT = REVERSE_STAGE_MAIN_COUNT + REVERSE_STAGE_BRANCH_COUNT;

        /// <summary>表・裏のステージ数</summary>
        public const int ALL_STAGE_COUNT = SURFACE_STAGE_ALL_COUNT + REVERSE_STAGE_ALL_COUNT;

        /// <summary>エクストラステージの数</summary>
        public const int EXTRA_STAGE_COUNT = 80;

        /// <summary>表・裏・エクストラのステージ数</summary>
        public const int ALL_STAGE_COUNT_WITH_EXTRA = ALL_STAGE_COUNT + EXTRA_STAGE_COUNT;

        /// <summary>メダルの総数</summary>
        /// <remarks>最終ステージにメダルはないので総数ステージ数 - 1</remarks>
        public const int ALL_MEDAL_COUNT = ALL_STAGE_COUNT - 1;

        /// <summary>キャラクターの数</summary>
        public const int CHARACTER_COUNT = 18;

        /// <summary>有効な解像度一覧</summary>
        public readonly static Vector2Int[] DISPLAY_RESOLUTION_LIST =
        {
            new Vector2Int(1920, 1080),
            new Vector2Int(1280, 720)
        };

        /// <summary>BGM音量最低値</summary>
        public const int BGM_VOLUME_MIN = 0;
        /// <summary>BGM音量最大値</summary>
        public const int BGM_VOLUME_MAX = 10;
        /// <summary>BGMのAudioSources.volumeに設定する際の倍率</summary>
        public const float BGM_VOLUME_MAGNIFICATE = 1f;
        /// <summary>SE音量最低値</summary>
        public const int SE_VOLUME_MIN = 0;
        /// <summary>SE音量最大値</summary>
        public const int SE_VOLUME_MAX = 10;
        /// <summary>BGMのAudioSources.volumeに設定する際の倍率</summary>
        public const float SE_VOLUME_MAGNIFICATE = 1f;
        /// <summary>プレイヤー毎のカラーコード</summary>
        public readonly static string[] PLAYER_COLOR_CODE_LIST = { "#DE003D", "#008BC8", "#F39800", "#729C12" };

#if !UNITY_SWITCH
        /// <summary> Q2のRemotePalyToghtherのWeb説明書のページURL<summary>
        public const string URL_HOW_TO_REMOTE_PLAY = "https://www.liica.co.jp/q2/remoteplay/";
        /// <summary>SteamのRemotePlayのページURL</summary>
        public const string URL_STEAM_REMOTE_PLAY = "https://store.steampowered.com/remoteplay";
#endif

#if UNITY_EDITOR
        /// <summary>PhotonAppSettingsファイルのパス</summary>
        public const string PHOTON_APP_SETTING_FILE_PATH = "Assets/Photon/Fusion/Resources/PhotonAppSettings.asset";
        /// <summary>通信に使用するAppID</summary>
        public const string PHOTON_APP_ID = "f16cccec-5776-4837-918f-eb2487a0cb39";
#endif // UNITY_EDITOR
    }
}