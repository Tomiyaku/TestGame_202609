#if UNITY_SWITCH
#if !UNITY_EDITOR
#define ENABLE_SWITCH_SAVE_SYSTEM
#endif // !UNITY_EDITOR
#endif // UNITY_SWITCH

#if UNITY_STANDALONE_WIN
#if !UNITY_EDITOR
#define ENABLE_AES_ENCRYPT
#endif // !UNITY_EDITOR
#endif // UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;

using CodeIcf.Extensions;
using CodeIcf.LanguageResoucrsManagement;
using CodeIcf.Switch;

using jp.co.liica.q2.Achievement;
using jp.co.liica.q2.Common;

namespace jp.co.liica.q2.SaveDataManagement
{
    /// <summary>
    /// <see cref="SaveDataManager.Save(WriteBufferData[])"/>で指定する書き込み範囲
    /// </summary>
    public struct WriteBufferData
    {
        /// <summary><see cref="SaveDataManager.m_SaveData"/>の開始インデックス</summary>
        public int Offset;
        /// <summary>書き込むデータサイズ</summary>
        public int Size;
        /// <summary>書き込むデータ</summary>
        public byte[] Buffer;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_offset"></param>
        /// <param name="_size"></param>
        public WriteBufferData( int _offset, int _size )
        {
            Offset = _offset;
            Size = _size;
            Buffer = new byte[ Size ];
        }
    }

    /// <summary>
    /// BGM再生設定の種類
    /// </summary>
    public enum BgmPlayType
    {
        /// <summary>決め打ち</summary>
        Once = 0,
        /// <summary>ランダム</summary>
        Random,
        /// <summary>ラジオ</summary>
        Radio
    }

    /// <summary>
    /// ステージの状態
    /// </summary>
    public enum StageCondition
    {
        /// <summary>ロック</summary>
        Lock = 0,
        /// <summary>解放済み</summary>
        Unlock = 0b1,
        /// <summary>プレイ済み</summary>
        Played = 0b11,
        /// <summary>クリア済み</summary>
        Cleared = 0b111,
        /// <summary>1手クリア済み</summary>
        OneMoveCleared = 0b1111,
    }

    /// <summary>
    /// セーブデータバージョン
    /// </summary>
    public static class SaveDataVersionDefine
    {
        /// <summary>セーブデータバージョンのローカルインデックス </summary>
        public const int LOCAL_INDEX_VERSION = 0;

        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = 0;
        /// <summary>セーブデータバージョンのインデックス</summary>
        public const int GLOBAL_INDEX_VERSION = GLOBAL_START_INDEX + LOCAL_INDEX_VERSION;

        /// <summary>データサイズ</summary>
        public const int SIZE = 1;
    }

    /// <summary>
    /// CRC
    /// </summary>
    public static class CrcDataDefine
    {
        /// <summary>セーブデータバージョンのローカルインデックス </summary>
        public const int LOCAL_INDEX_CRC = 0;

        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = SaveDataVersionDefine.GLOBAL_START_INDEX + SaveDataVersionDefine.SIZE;
        /// <summary>セーブデータバージョンのインデックス</summary>
        public const int GLOBAL_INDEX_CRC = GLOBAL_START_INDEX + LOCAL_INDEX_CRC;

        /// <summary>データサイズ</summary>
        public const int SIZE = 4;
    }

    /// <summary>
    /// 設定
    /// </summary>
    public static class ConfigDataDefine
    {
        /// <summary>BGMのローカルインデックス</summary>
        public const int LOCAL_INDEX_BGM = 0;
        /// <summary>SEのローカルインデックス</summary>
        public const int LOCAL_INDEX_SE = 1;
        /// <summary>振動のローカルインデックス</summary>
        public const int LOCAL_INDEX_VIBRATION = 2;
        /// <summary>ボタンガイドのローカルインデックス</summary>
        public const int LOCAL_INDEX_BUTTON_GUIDE = 3;
        /// <summary>言語のローカルインデックス</summary>
        public const int LOCAL_INDEX_LAUNGAGE = 4;
        /// <summary>BGM再生設定 : ステージセレクト</summary>
        public const int LOCAL_INDEX_BGM_PLAY_TYPE_STAGE_SELECT = 5;
        /// <summary>BGM再生設定 : ステージ中</summary>
        public const int LOCAL_INDEX_BGM_PLAY_TYPE_STAGE = 6;
        /// <summary>BGM再生設定 : キャラセレクト</summary>
        public const int LOCAL_INDEX_BGM_PLAY_TYPE_CHARA_SELECT = 7;
        /// <summary>BGM再生設定 : ラジオ1局</summary>
        public const int LOCAL_INDEX_BGM_PLAY_RADIO_ONCE = 8;
        /// <summary> キーボードでの操作を許可</summary>
        public const int LOCAL_INDEX_KEYBOARD_CONTROL = 9;
        /// <summary>ディスプレイモード</summary>
        public const int LOCAL_INDEX_DISPLAY_MODE = 10;
        /// <summary>解像度</summary>
        public const int LOCAL_INDEX_DISPLAY_RESOLUTION = 11;


        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = CrcDataDefine.GLOBAL_START_INDEX + CrcDataDefine.SIZE;
        /// <summary>BGMのインデックス</summary>
        public const int GLOBAL_INDEX_BGM = GLOBAL_START_INDEX + LOCAL_INDEX_BGM;
        /// <summary>SEのインデックス</summary>
        public const int GLOBAL_INDEX_SE = GLOBAL_START_INDEX + LOCAL_INDEX_SE;
        /// <summary>振動のインデックス</summary>
        public const int GLOBAL_INDEX_VIBRATION = GLOBAL_START_INDEX + LOCAL_INDEX_VIBRATION;
        /// <summary> ボタンガイドのグローバルインデックス</summary>
        public const int GLOBAL_INDEX_BUTTON_GUIDE = GLOBAL_START_INDEX + LOCAL_INDEX_BUTTON_GUIDE;
        /// <summary>言語のグローバルインデックス</summary>
        public const int GLOBAL_INDEX_LANGUAGE = GLOBAL_START_INDEX + LOCAL_INDEX_LAUNGAGE;
        /// <summary>グローバルインデックス BGM再生設定 : ステージセレクト</summary>
        public const int GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE_SELECT = GLOBAL_START_INDEX + LOCAL_INDEX_BGM_PLAY_TYPE_STAGE_SELECT;
        /// <summary>グローバルインデックス BGM再生設定 : ステージ中</summary>
        public const int GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE = GLOBAL_START_INDEX + LOCAL_INDEX_BGM_PLAY_TYPE_STAGE;
        /// <summary>グローバルインデックス BGM再生設定 : キャラセレクト</summary>
        public const int GLOBAL_INDEX_BGM_PLAY_TYPE_CHARA_SELECT = GLOBAL_START_INDEX + LOCAL_INDEX_BGM_PLAY_TYPE_CHARA_SELECT;
        /// <summary>グローバルインデックス BGM再生設定 : ラジオ1局</summary>
        public const int GLOBAL_INDEX_BGM_PLAY_RADIO_ONCE = GLOBAL_START_INDEX + LOCAL_INDEX_BGM_PLAY_RADIO_ONCE;
        /// <summary>グローバルインデックス キーボードでの操作を許可</summary>
        public const int GLOBAL_INDEX_KEYBOARD_CONTROL = GLOBAL_START_INDEX + LOCAL_INDEX_KEYBOARD_CONTROL;
        /// <summary>グローバルインデックス ディスプレイモード</summary>
        public const int GLOBAL_INDEX_DISPLAY_MODE = GLOBAL_START_INDEX + LOCAL_INDEX_DISPLAY_MODE;
        /// <summary>グローバルインデックス 解像度</summary>
        public const int GLOBAL_INDEX_DISPLAY_RESOLUTION = GLOBAL_START_INDEX + LOCAL_INDEX_DISPLAY_RESOLUTION;

        /// <summary>データサイズ</summary>
        public const int SIZE = 32;
    }

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public static class PlayerInfoDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE;

        /// <summary>ローカルインデックス : 最後に選択したキャラクターID</summary>
        public const int LOCAL_INDEX_SELECT_CHARA_ID = 0;
        /// <summary>ローカルインデックス : 最後に選択したキャラクターのスキンID</summary>
        public const int LOCAL_INDEX_SELECT_SKIN_ID = 1;
        /// <summary>ローカルインデックス : プレイヤー名の使用サイズ</summary>
        public const int LOCAL_INDEX_NAME_SIZE = 2;
        /// <summary>ローカルインデックス : プレイヤー表示名</summary>
        public const int LOCAL_INDEX_NAME = 3;
        /// <summary>プレイヤー表示名の最大サイズ</summary>
        public const int NAME_MAX_SIZE = 40;

        /// <summary>1プレイヤー毎の割り当てサイズ</summary>
        public const int SIZE_PER_ONE_PLAYER = 24 + NAME_MAX_SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = SIZE_PER_ONE_PLAYER * ApplicationDefine.PLAYER_MAX_COUNT;
    }

    /// <summary>
    /// キャラクターの取得状態定義
    /// </summary>
    public static class CharacterUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = PlayerInfoDefine.GLOBAL_START_INDEX + PlayerInfoDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 32;
    }

    /// <summary>
    /// 鍵の取得状態定義
    /// </summary>
    public static class KeyUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = CharacterUnlockDefine.GLOBAL_START_INDEX + CharacterUnlockDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 32;
    }

    /// <summary>
    /// メダルの取得状態定義
    /// </summary>
    public static class MedalUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = KeyUnlockDefine.GLOBAL_START_INDEX + KeyUnlockDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 256;
    }

    /// <summary>
    /// スキンを開放するコインの取得状態定義
    /// </summary>
    public static class SkinCoinUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = MedalUnlockDefine.GLOBAL_START_INDEX + MedalUnlockDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 256;
    }

    /// <summary>
    /// メダル・コインの消費量の定義
    /// </summary>
    public static class ConsumptionDefine
    {
        /// <summary>ローカルインデックス : メダルの消費量の開始位置</summary>
        public const int LOCAL_INDEX_MEDAL_CONSUMPTION = 0;
        /// <summary>メダルの最大消費量</summary>
        /// <remarks>この値はメダルの最大数と同じ</remarks>
        public const int MAX_MEDAL_CONSUMPTION = MedalUnlockDefine.SIZE * SaveDataManager.BYTE_SIZE;
        /// <summary>ローカルインデックス : スキンコインの消費量の開始位置</summary>
        public const int LOCAL_INDEX_SKIN_COIN_CONSUMPTION = 4;
        /// <summary>スキンコインの最大消費量</summary>
        /// <remarks>この値はスキンコインの最大数と同じ</remarks>
        public const int MAX_SKIN_COIN_CONSUMPTION = SkinCoinUnlockDefine.SIZE * SaveDataManager.BYTE_SIZE;

        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = SkinCoinUnlockDefine.GLOBAL_START_INDEX + SkinCoinUnlockDefine.SIZE;
        /// <summary>グローバルインデックス : メダルの消費量の開始位置</summary>
        public const int GLOBAL_INDEX_MEDAL_CONSUMPTION = GLOBAL_START_INDEX + LOCAL_INDEX_MEDAL_CONSUMPTION;
        /// <summary>グローバルインデックス : スキンコインの消費量の開始位置</summary>
        public const int GLOBAL_INDEX_SKIN_COIN_CONSUMPTION = GLOBAL_START_INDEX + LOCAL_INDEX_SKIN_COIN_CONSUMPTION;

        /// <summary>データサイズ</summary>
        public const int SIZE = 32;
    }

    /// <summary>
    /// 追加ステージエリア解放状態定義
    /// </summary>
    public static class ExtraStageAreaUnlockData
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = ConsumptionDefine.GLOBAL_START_INDEX + ConsumptionDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 8;
    }

    /// <summary>
    /// BGMの取得状態定義
    /// </summary>
    public static class BGMUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = ExtraStageAreaUnlockData.GLOBAL_START_INDEX + ExtraStageAreaUnlockData.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 56;
    }

    /// <summary>
    /// スキンの獲得状態定義
    /// </summary>
    public static class SkinUnlockDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = BGMUnlockDefine.GLOBAL_START_INDEX + BGMUnlockDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 32;
    }

    /// <summary>
    /// 演出の状態定義
    /// </summary>
    public static class PerformanceEndStateDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = SkinUnlockDefine.GLOBAL_START_INDEX + SkinUnlockDefine.SIZE;

        /// <summary> ステージ開放演出の開始インデックス/summary>
        public const int LOCAL_START_INDEX_STAGE_UNLOCK = 0;
        /// <summary> セーブデータ全体でのステージ開放演出の開始インデックス</summary>
        public const int GLOBAL_START_INDEX_STAGE_UNLOCK = GLOBAL_START_INDEX + LOCAL_START_INDEX_STAGE_UNLOCK;
        /// <summary> ステージ開放演出のデータサイズ</summary>
        public const int LOCAL_SIZE_STAGE_UNLOCK = 64;

        /// <summary> メッセージ演出の開始インデックス</summary>
        public const int LOCAL_START_INDEX_MESSAGE = LOCAL_START_INDEX_STAGE_UNLOCK + LOCAL_SIZE_STAGE_UNLOCK;
        /// <summary> セーブデータ全体でのメッセージ演出の開始インデックス</summary>
        public const int GLOBAL_START_INDEX_MESSAGE = GLOBAL_START_INDEX + LOCAL_START_INDEX_MESSAGE;
        /// <summary> メッセージ演出のデータサイズ</summary>
        public const int LOCAL_SIZE_MESSAGE = 64;

        /// <summary>データサイズ</summary>
        public const int SIZE = 256;
    }

    /// <summary>
    /// 実績カウント
    /// </summary>
    public static class AchievementCountDataDeifne
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = PerformanceEndStateDefine.GLOBAL_START_INDEX + PerformanceEndStateDefine.SIZE;

        /// <summary>描いた線の長さの開始インデックス</summary>
        public const int LOCAL_START_INDEX_DRAW_LINE_LENGTH = 0;
        /// <summary>描いた線の長さのデータサイズ</summary>
        public const int LOCAL_SIZE_DRAW_LINE_LENGTH = 4;
        /// <summary>グローバルインデックス : 描いた線の長さの開始インデックス</summary>
        public const int GLOBAL_START_INDEX_DRAW_LINE_LENGTH = GLOBAL_START_INDEX + LOCAL_START_INDEX_DRAW_LINE_LENGTH;
        /// <summary>描いた線の長さの上限</summary>
        public const int LIMIT_DRAW_LINE_LENGTH = 500000;


        /// <summary>リトライ回数の開始インデックス</summary>
        public const int LOCAL_START_INDEX_RETRY_COUNT = LOCAL_START_INDEX_DRAW_LINE_LENGTH + LOCAL_SIZE_DRAW_LINE_LENGTH;
        /// <summary>リトライ回数のデータサイズ</summary>
        public const int LOCAL_SIZE_RETRY_COUNT = 4;
        /// <summary>グローバルインデックス : リトライ回数の開始インデックス</summary>
        public const int GLOBAL_START_INDEX_RETRY_COUNT = GLOBAL_START_INDEX + LOCAL_START_INDEX_RETRY_COUNT;
        /// <summary>リトライ回数の上限</summary>
        public const int LIMIT_RETRY_COUNT = 999;

        /// <summary>データサイズ</summary>
        public const int SIZE = 256;
    }

    /// <summary>
    /// ステージの状態
    /// </summary>
    public static class StageConditionDataDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = AchievementCountDataDeifne.GLOBAL_START_INDEX + AchievementCountDataDeifne.SIZE;
        /// <summary>1ステージのデータサイズ(byte)</summary>
        public const int STAGE_DATA_SIZE = 1;
        /// <summary>全ステージ分のデータサイズ</summary>
        public const int SIZE = 2048;
    }

    /// <summary>
    /// セーブデータ予約枠
    /// </summary>
    public static class ReserveDataDefine
    {
        /// <summary>セーブデータ全体での開始インデックス</summary>
        public const int GLOBAL_START_INDEX = StageConditionDataDefine.GLOBAL_START_INDEX + StageConditionDataDefine.SIZE;
        /// <summary>データサイズ</summary>
        public const int SIZE = 1024;
    }

    /// <summary>
    /// セーブデータ管理
    /// </summary>
    public class SaveDataManager
    {
        /// <summary>最新のセーブデータバージョン</summary>
        private const int LEAD_SAVE_DATA_VERSION = 1;
        /// <summary>1Byte辺りのビット数</summary>
        public const int BYTE_SIZE = 8;
        /// <summary>セーブデータ全体のサイズ</summary>
        public const int SAVE_DATA_ALL_SIZE = ReserveDataDefine.GLOBAL_START_INDEX + ReserveDataDefine.SIZE;
        /// <summary>セーブデータファイル名</summary>
        public const string SAVE_DATA_FILE_NAME = "SaveData.bin";

        /// <summary>インスタンス</summary>
        private static SaveDataManager m_Instance = null;
        /// <summary>インスタンス</summary>
        public static SaveDataManager Instance
        {
            get
            {
                if( m_Instance == null )
                {
                    m_Instance = new SaveDataManager();
#if UNITY_EDITOR
                    m_Instance.Initialize();
                    m_Instance.Load();
#endif
                }

                return m_Instance;
            }
        }

        /// <summary>セーブデータ</summary>
        private byte[] m_SaveData = null;
        /// <summary>ゲストとしてオンラインプレイに参加している際に参照するホスト側のセーブデータ</summary>
        private byte[] m_OnlineHostSaveData = null;
        /// <summary>ゲストとしてオンラインプレイに参加しているかのフラグ</summary>
        /// <remarks>ホスト側のセーブデータを保持していればtrue</remarks>
        public bool IsOnlineGuestMode => m_OnlineHostSaveData != null;
        /// <summary>現在の状態で参照するセーブデータ</summary>
        /// <remarks>
        /// ゲストとしてオンラインプレイに参加している場合は<see cref="m_OnlineHostSaveData"/>を参照<br></br>
        /// それ以外(ホストとしてオンラインプレイ・ローカルプレイ)の場合は<see cref="m_SaveData"/>を参照<br></br>
        /// どの状態でもこのクライアント自身のセーブデータを参照する場合は直接<see cref="m_SaveData"/>を参照する
        /// </remarks>
        private byte[] CurrentSaveData
        {
            get
            {
                if( IsOnlineGuestMode ) return m_OnlineHostSaveData;

                return m_SaveData;
            }
        }

        /// <summary>
        /// セーブデータを複製
        /// </summary>
        /// <param name="_outCopyData">複製されたセーブデータの格納先</param>
        public void CopyData( out byte[] _outCopyData )
        {
            _outCopyData = null;

            if( m_SaveData == null ) return;

            _outCopyData = new byte[ m_SaveData.Length ];

            Array.Copy( m_SaveData, _outCopyData, m_SaveData.Length );
        }

        /// <summary>
        /// セーブデータを全て更新
        /// </summary>
        /// <param name="_buff">セーブデータのバイト入れる</param>
        /// <returns>trueなら成功</returns>
        public bool UpdateSaveData( byte[] _buff )
        {
            if( m_SaveData == null ) return false;
            if( m_SaveData.Length != _buff.Length ) return false;

            m_SaveData = _buff;
            //Array.Copy(_buff, m_SaveData, m_SaveData.Length);

            return true;
        }

        /// <summary>
        /// セーブデータを全て保存
        /// </summary>
        public void SaveAllBuffer()
        {
            if( m_SaveData == null ) return;

            Save( 0, m_SaveData.LongLength );
        }

#if ENABLE_SWITCH_SAVE_SYSTEM
        /// <summary>ユーザー識別子</summary>
        private nn.account.Uid m_Uid;
        public nn.account.Uid Uid => m_Uid;
        /// <summary>ユーザーハンドル</summary>
        private nn.account.UserHandle m_UserHandle;
        public nn.account.UserHandle UserHandle => m_UserHandle;
        /// <summary>ファイルを扱うためのハンドル</summary>
        private nn.fs.FileHandle m_FileHandle;
#else
        private string m_SaveFilePath = "";
#endif //ENABLE_SWITCH_SAVE_SYSTEM

        /// <summary>
        /// キャラ・メダル・スキンコイン・鍵・スキン・BGMを取得した際に自動で保存するかのフラグ
        /// </summary>
        /// <remarks>UnlockContentsToで始まるメソッドが対象</remarks>
        public bool IsAutoSaveToUnlockContents { get; set; } = true;
        /// <summary>初期化済みフラグ</summary>
        private bool m_IsInitialized = false;
        /// <summary>初期化済みフラグ</summary>
        public static bool IsInitialized => m_Instance != null ? m_Instance.m_IsInitialized : false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private SaveDataManager() { }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>セーブデータファイルの確認・ファイルがない場合には新規作成</remarks>
        public void Initialize()
        {
            if( m_IsInitialized ) return;

            m_OnlineHostSaveData = null;

#if ENABLE_SWITCH_SAVE_SYSTEM
            nn.account.Account.Initialize();
#if SWITCH_LIICA_PHYSICAL_EDITION
            int outCount = 0;
            nn.account.UserHandle[] userHandles = new nn.account.UserHandle[ 1 ];

            NSMultProgramApplication.PopOpenUsers( ref outCount, ref userHandles, 1 );

            nn.account.UserHandle userHandle = userHandles[ 0 ];
#else // SWITCH_LIICA_PHYSICAL_EDITION
            nn.account.UserHandle userHandle = new nn.account.UserHandle();

            if( !nn.account.Account.TryOpenPreselectedUser( ref userHandle ) )
            {
                nn.Nn.Abort( "Failed to open preselected user." );
            }
#endif // SWITCH_LIICA_PHYSICAL_EDITION

            m_UserHandle = userHandle;
            nn.Result result = nn.account.Account.GetUserId( ref m_Uid, userHandle );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to get user id." );
            }
            result = nn.fs.SaveData.Mount( SwitchDefine.SAVE_MOUNT_NAME, m_Uid );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to mount save data." );
            }

            nn.fs.EntryType entryType = 0;
            result = nn.fs.FileSystem.GetEntryType( ref entryType, SwitchDefine.SAVE_FILE_PATH );

            m_IsInitialized = true;

            //セーブデータファイルがあるのでここで終わり
            if( result.IsSuccess() )
            {
                CheckSaveDataVersion();
            }
            else
            {
                Debug.Log( "Not Found SaveData!" );
            }
#else //ENABLE_SWITCH_SAVE_SYSTEM       
#if UNITY_EDITOR
            m_SaveFilePath = Directory.GetCurrentDirectory();
            m_SaveFilePath += "/SaveData/";
#else //UNITY_EDITOR
            m_SaveFilePath = Application.persistentDataPath + "/";
#endif //UNITY_EDITOR
            if( !Directory.Exists( m_SaveFilePath ) ) Directory.CreateDirectory( m_SaveFilePath );

            m_SaveFilePath += SAVE_DATA_FILE_NAME;

            m_IsInitialized = true;

            if( File.Exists( m_SaveFilePath ) )
            {
                CheckSaveDataVersion();
            }
#endif //ENABLE_SWITCH_SAVE_SYSTEM
        }

        /// <summary>
        /// セーブデータバージョンのチェックとセーブデータの更新対応
        /// </summary>
        private void CheckSaveDataVersion()
        {
            int savedataVersion = -1;
            long savedataSize = 0;
            byte[] saveData = null;
            byte[] fileBuffer = null;

#if ENABLE_SWITCH_SAVE_SYSTEM
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType( ref entryType, SwitchDefine.SAVE_FILE_PATH );

            if( !nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                result.abortUnlessSuccess();

                result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Read );
                if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
                {
                    // 対象ファイルが存在しません。
                    return;
                }

                if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
                {
                    // 対象ファイルが既にオープンされてます。
                    return;
                }

                result = nn.fs.File.GetSize( ref savedataSize, m_FileHandle );
                if( !result.IsSuccess() )
                {
                    nn.err.Error.Show( result );
                    nn.Nn.Abort( "Failed to get file size." );
                }

                fileBuffer = new byte[ savedataSize ];
                result = nn.fs.File.Read( m_FileHandle, 0, fileBuffer, savedataSize );
                if( !result.IsSuccess() )
                {
                    nn.err.Error.Show( result );
                    nn.Nn.Abort( "Failed to read save data." );
                }

                nn.fs.File.Close( m_FileHandle );

                saveData = fileBuffer;
                savedataVersion = saveData[ 0 ];
            }
#else
            try
            {
                using FileStream fs = new FileStream( m_SaveFilePath, FileMode.Open, FileAccess.Read );
                fileBuffer = new byte[ fs.Length ];
                fs.Read( fileBuffer, 0, fileBuffer.Length );
                fs.Close();
            }
            catch
            {
            }

            if( fileBuffer != null && fileBuffer.Length > 0 )
            {
#if ENABLE_AES_ENCRYPT
                saveData = AesManager.DecryptToBytes( fileBuffer );
                savedataSize = saveData.Length;
                savedataVersion = saveData[ 0 ];
#else  // ENABLE_AES_ENCRYPT
                saveData = fileBuffer;
                savedataSize = saveData.Length;
                savedataVersion = saveData[ 0 ];
#endif // ENABLE_AES_ENCRYPT
            }
#endif // ENABLE_SWITCH_SAVE_SYSTEM
            Debug.Log( "SaveData Size = " + saveData.Length + " DataVersion = " + savedataVersion );

            if( savedataVersion < LEAD_SAVE_DATA_VERSION )
            {
                if( savedataSize == FIrstVersionSavedataDefine.SAVEDATA_SIZE )
                {
                    ConvertSavedata0To1( saveData );
#if ENABLE_SWITCH_SAVE_SYSTEM
                    UpdateSaveDataFromNS();
#else
                    SaveAllBuffer();
#endif
                }
                else
                {
                    Delete();

                    CreateAndSave();
                }
            }
            else
            {
                //セーブデータサイズが設定サイズと違う or セーブデータバージョンが最新未満の場合、セーブデータファイルを削除して新規作成
                if( savedataSize != SAVE_DATA_ALL_SIZE || savedataVersion < LEAD_SAVE_DATA_VERSION )
                {
                    Delete();

                    CreateAndSave();
                }
            }
        }

        /// <summary>
        /// データバージョン0から1への変換
        /// </summary>
        /// <param name="_oldSaveBuffer"></param>
        private void ConvertSavedata0To1( byte[] _oldSaveBuffer )
        {
#if !ENABLE_SWITCH_SAVE_SYSTEM && ( DEVELOPMENT_BUILD || UNITY_EDITOR )
            string fileName = "_Old_" + DateTime.Now.ToString( "yyyyMMddHHmmssfff" ) + "_" + SAVE_DATA_FILE_NAME;
            string writeFilePath = Path.GetDirectoryName( m_SaveFilePath ) + "/" + fileName;

#if ENABLE_AES_ENCRYPT
            byte[] writebuffer = AesManager.EncryptToBytes( _oldSaveBuffer );
#else // ENABLE_AES_ENCRYPT
            byte[] writebuffer = new byte[ _oldSaveBuffer.Length ];
            System.Array.Copy( _oldSaveBuffer, writebuffer, _oldSaveBuffer.Length );
#endif // ENABLE_AES_ENCRYPT

            System.IO.File.WriteAllBytes( writeFilePath, writebuffer );
#endif // !ENABLE_SWITCH_SAVE_SYSTEM && ( DEVELOPMENT_BUILD || UNITY_EDITOR )
            CreateSaveData();

            Debug.Log( "Old SaveData Size = " + _oldSaveBuffer.Length + " new SaveDataSIze  = " + m_SaveData.Length );

            //データバージョン
            m_SaveData[ SaveDataVersionDefine.GLOBAL_START_INDEX ] = LEAD_SAVE_DATA_VERSION;

            // CRC 未使用なのとインデックスのミスで元からコピーしない
            //System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_CRC, m_SaveData, CrcDataDefine.GLOBAL_INDEX_CRC, FIrstVersionSavedataDefine.SIZE_CRC );

            // 設定
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_CONFIG, m_SaveData, ConfigDataDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_CONFIG );

            // プレイヤー状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_PLAYER_INFO, m_SaveData, PlayerInfoDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_PLAYER_INFO );

            // キャラクター開放状態 1キャラ1bitから1キャラ1byteへ変更
            byte[] newCharaUnlockData = new byte[ CharacterUnlockDefine.SIZE ];

            for( int i = 0; i < newCharaUnlockData.Length; i++ )
            {
                int index = i / BYTE_SIZE;
                int sift = i % BYTE_SIZE;
                byte bitFlag = ( byte )( 0b1 << sift );

                if( ( _oldSaveBuffer[ FIrstVersionSavedataDefine.INDEX_CHARA + index ] & bitFlag ) == bitFlag )
                {
                    newCharaUnlockData[ i ] = 0b1;
                }
            }

            System.Array.Copy( newCharaUnlockData, 0, m_SaveData, CharacterUnlockDefine.GLOBAL_START_INDEX, CharacterUnlockDefine.SIZE );

            // カギの取得状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_KEY, m_SaveData, KeyUnlockDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_KEY );

            //メダルの取得状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_MEDAL, m_SaveData, MedalUnlockDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_MEDAL );

            //スキンコインの取得状態 新規追加枠
            //アイテムの消費状態 新規追加枠

            // BGMの開放状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_BGM, m_SaveData, BGMUnlockDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_BGM );

            // スキンの開放状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_SKIN, m_SaveData, SkinUnlockDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_SKIN );

            // 演出終了フラグ
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_PERFORMANCE, m_SaveData, PerformanceEndStateDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_PERFORMANCE );

            // 実績カウント
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_ACHIVEMENT, m_SaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_ACHIVEMENT );

            // ステージ状態
            System.Array.Copy( _oldSaveBuffer, FIrstVersionSavedataDefine.INDEX_STAGE, m_SaveData, StageConditionDataDefine.GLOBAL_START_INDEX, FIrstVersionSavedataDefine.SIZE_STAGE );
        }

        /// <summary>
        /// セーブデータファイルを削除する
        /// </summary>
        private void Delete()
        {
            Debug.Log( "SaveDataManager.Delete()" );
#if ENABLE_SWITCH_SAVE_SYSTEM
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType( ref entryType, SwitchDefine.SAVE_FILE_PATH );

            if( !nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                result = nn.fs.File.Delete( SwitchDefine.SAVE_FILE_PATH );
                Debug.Log( "Delete SaveData Result : " + result.ToString() );
                result.abortUnlessSuccess();
            }
#else //ENABLE_SWITCH_SAVE_SYSTEM
            if( File.Exists( m_SaveFilePath ) ) File.Delete( m_SaveFilePath );
#endif //ENABLE_SWITCH_SAVE_SYSTEM
        }

        /// <summary>
        /// セーブデータを新規作成してファイル保存
        /// </summary>
        private void CreateAndSave()
        {
            CreateSaveData();

            Debug.Log( "SaveDataManager.CreateAndSave()" );

#if ENABLE_SWITCH_SAVE_SYSTEM
            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            nn.Result result = nn.fs.File.Create( SwitchDefine.SAVE_FILE_PATH, SAVE_DATA_ALL_SIZE );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルの親ディレクトリが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultPathAlreadyExists.Includes( result ) || nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既に存在しています。
                return;
            }

            if( nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes( result ) )
            {
                // セーブデータのデータ保存領域が不足しています。
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Usable space not enough." );
                return;
            }

            result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Write );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既にオープンされています。
                return;
            }

            result = nn.fs.File.Write( m_FileHandle, 0, m_SaveData, SAVE_DATA_ALL_SIZE, nn.fs.WriteOption.Flush );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to write save data." );
            }

            nn.fs.File.Close( m_FileHandle );
            result = nn.fs.FileSystem.Commit( SwitchDefine.SAVE_MOUNT_NAME );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to close file system." );
            }

            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();

#else //ENABLE_SWITCH_SAVE_SYSTEM
            Save( 0, SAVE_DATA_ALL_SIZE );
#endif //ENABLE_SWITCH_SAVE_SYSTEM
        }

        public void Load()
        {
            if( !IsInitialized ) return;

#if ENABLE_SWITCH_SAVE_SYSTEM
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType( ref entryType, SwitchDefine.SAVE_FILE_PATH );

            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                //セーブデータ無し
                Debug.Log( "SaveDataManager.Load() Not found SaveData!" );
                CreateAndSave();
                return;
            }

            result.abortUnlessSuccess();

            result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Read );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既にオープンされています。
                return;
            }

            long fileSize = 0;
            result = nn.fs.File.GetSize( ref fileSize, m_FileHandle );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to get file size." );
            }

            m_SaveData = new byte[ fileSize ];
            result = nn.fs.File.Read( m_FileHandle, 0, m_SaveData, fileSize );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to read save data." );
            }

            nn.fs.File.Close( m_FileHandle );
#else // ENABLE_SWITCH_SAVE_SYSTEM
            byte[] saveBuffer = null;

            try
            {
                using FileStream fs = new FileStream( m_SaveFilePath, FileMode.Open, FileAccess.Read );
                saveBuffer = new byte[ fs.Length ];
                fs.Read( saveBuffer, 0, saveBuffer.Length );
                fs.Close();
            }
            catch
            {
                CreateSaveData();
                Save( 0, m_SaveData.LongLength );
            }

            if( saveBuffer != null && saveBuffer.Length > 0 )
            {
#if ENABLE_AES_ENCRYPT
                m_SaveData = AesManager.DecryptToBytes( saveBuffer );
#else  // ENABLE_AES_ENCRYPT
                m_SaveData = saveBuffer;
#endif // ENABLE_AES_ENCRYPT
            }
#endif // ENABLE_SWITCH_SAVE_SYSTEM
        }

        /// <summary>
        /// セーブデータを初期化して保存
        /// </summary>
        /// <param name="_prevConfig"></param>
        public void ResetSaveData( byte[] _prevConfig )
        {
            // 設定前の実績カウントがあれば設定
            AchievementManager.SetCountData();

            // キーボードの設定だけはリセットしても引き継ぐ
            bool iskeyboard = KeyboardControl;

            // オプション領域の内容をコピー
            byte[] configBuff = new byte[ ConfigDataDefine.SIZE ];

            if( _prevConfig == null ) Array.Copy( m_SaveData, ConfigDataDefine.GLOBAL_START_INDEX, configBuff, 0, ConfigDataDefine.SIZE );
            else Array.Copy( _prevConfig, 0, configBuff, 0, ConfigDataDefine.SIZE );

            // 実績用カウント領域をコピー
            byte[] achievementCountBuff = new byte[ AchievementCountDataDeifne.SIZE ];
            Array.Copy( m_SaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX, achievementCountBuff, 0, AchievementCountDataDeifne.SIZE );

            CreateSaveData();

            // オプション領域をリセットされたセーブデータにコピー
            Array.Copy( configBuff, 0, m_SaveData, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.SIZE );
            // 実績用カウント領域をリセットされたセーブデータにコピー
            Array.Copy( achievementCountBuff, 0, m_SaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX, AchievementCountDataDeifne.SIZE );

            KeyboardControl = iskeyboard;

            Save( 0, SAVE_DATA_ALL_SIZE );
        }

        /// <summary>
        /// セーブデータの作成と初期設定
        /// </summary>
        private void CreateSaveData()
        {
            Debug.Log( "SaveDataManager.CreateSaveData()" );

            m_SaveData = new byte[ SAVE_DATA_ALL_SIZE ];

            for( int i = 0; i < m_SaveData.Length; i++ ) m_SaveData[ i ] = 0;

            m_SaveData[ SaveDataVersionDefine.GLOBAL_INDEX_VERSION ] = LEAD_SAVE_DATA_VERSION;
            m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM ] = ApplicationDefine.BGM_VOLUME_MAX / 2;
            m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_SE ] = ApplicationDefine.SE_VOLUME_MAX / 2;
            m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_VIBRATION ] = 1;
            m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BUTTON_GUIDE ] = 1;

#if !ENABLE_SWITCH_SAVE_SYSTEM
            Resolution resolution = Screen.currentResolution;
            Vector2Int defaultSIze = ApplicationDefine.DISPLAY_RESOLUTION_LIST[ 0 ];

            if( resolution.width <= defaultSIze.x || resolution.height <= defaultSIze.y )
            {//ディスプレイの画面サイズが1920x1080以下の場合は1280x720に設定
                m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_DISPLAY_RESOLUTION ] = 1;
            }
#endif //!ENABLE_SWITCH_SAVE_SYSTEM

#if ENABLE_SWITCH_SAVE_SYSTEM
            string language = nn.oe.Language.GetDesired();
#else //ENABLE_SWITCH_SAVE_SYSTEM
            string language = Application.systemLanguage == SystemLanguage.Japanese ? LanguageDefine.JP_CODE : "";
#endif //ENABLE_SWITCH_SAVE_SYSTEM

            Debug.Log( language );

            m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_LANGUAGE ] = ( byte )( language == LanguageDefine.JP_CODE ? LanguageDefine.LANGUAGE_TYPE_JP : LanguageDefine.LANGUAGE_TYPE_EN );

            //プレイヤー関連の初期値
            for( int i = 0; i < ApplicationDefine.PLAYER_MAX_COUNT; i++ )
            {
                int index = PlayerInfoDefine.GLOBAL_START_INDEX + ( PlayerInfoDefine.SIZE_PER_ONE_PLAYER * i );

                m_SaveData[ index ] = 0; //初期キャラを選択したことにする
                m_SaveData[ index + 1 ] = 0; //初期スキン

                UpdatePlayerName( i, string.Format( ApplicationDefine.DEFAULT_PLAYER_NAME, ( i + 1 ).ToString() ) );
            }

            m_SaveData[ StageConditionDataDefine.GLOBAL_START_INDEX ] = 0b1; //最初のステージを解放済みにする
        }

        /// <summary>
        /// Nintendo Switch専用でセーブデータのファイルサイズを変更してすべて保存する
        /// </summary>
        private void UpdateSaveDataFromNS()
        {
            if( !IsInitialized ) return;
#if ENABLE_SWITCH_SAVE_SYSTEM
            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            nn.Result result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Write | nn.fs.OpenFileMode.AllowAppend );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既にオープンされています。
                return;
            }

            result = nn.fs.File.SetSize( m_FileHandle, SAVE_DATA_ALL_SIZE );

            //if( !result.IsSuccess() ) Debug.Log( "1 " + result.ToString() );
            result.abortUnlessSuccess();

            result = nn.fs.File.Write( m_FileHandle, 0, m_SaveData, SAVE_DATA_ALL_SIZE, nn.fs.WriteOption.Flush );

            //if( !result.IsSuccess() ) Debug.Log( "2 " + result.ToString() );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to write save data." );
            }

            nn.fs.File.Close( m_FileHandle );
            result = nn.fs.FileSystem.Commit( SwitchDefine.SAVE_MOUNT_NAME );

            //if( !result.IsSuccess() ) Debug.Log( "3 " + result.ToString() );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to close file system." );
            }

            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
        }

        /// <summary>
        /// 保存
        /// </summary>        
        /// <param name="_offset">セーブファイルの書き込み開始位置</param>
        /// <param name="_size">書き込みするデータサイズ(<see cref="_data"/>のサイズ)</param>
        private void Save( int _offset, long _size )
        {
            if( !IsInitialized ) return;

            if( isNetworkMode )
            {
                var start = _offset;
                var end = _offset + _size;

                if( end < ConfigDataDefine.GLOBAL_START_INDEX ) return;
                if( start >= ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE ) return;

                start = Math.Clamp( start, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE );
                end = Math.Clamp( end, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE );

                _offset = start;
                _size = end - start;
            }

            byte[] wrtieBuffer = new byte[ _size ];
            Array.Copy( m_SaveData, _offset, wrtieBuffer, 0, _size );

#if ENABLE_SWITCH_SAVE_SYSTEM
            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            nn.Result result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Write );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既にオープンされています。
                return;
            }

            result = nn.fs.File.Write( m_FileHandle, _offset, wrtieBuffer, _size, nn.fs.WriteOption.Flush );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to write save data." );
            }

            nn.fs.File.Close( m_FileHandle );
            result = nn.fs.FileSystem.Commit( SwitchDefine.SAVE_MOUNT_NAME );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to close file system." );
            }

            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#elif ENABLE_AES_ENCRYPT
            // 暗号化して保存
            byte[] encryptBuff = AesManager.EncryptToBytes( m_SaveData );
            using FileStream fs = new FileStream( m_SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );
            fs.Position = 0;
            fs.Write( encryptBuff, 0, encryptBuff.Length );
            fs.Close();
#else
            using FileStream fs = new FileStream( m_SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );
            fs.Position = _offset;
            fs.Write( wrtieBuffer, 0, wrtieBuffer.Length );
            fs.Close();
#endif
        }

        /// <summary>
        ///  複数のデータ領域の保存
        /// </summary>
        /// <param name="_writeData">保存するセーブデータ領域一覧</param>
        public void Save( params WriteBufferData[] _writeData )
        {
            if( !IsInitialized ) return;

            for( int i = 0; i < _writeData.Length; i++ )
            {
                var data = _writeData[ i ];

                if( isNetworkMode )
                {
                    var start = data.Offset;
                    var end = data.Offset + data.Size;

                    if( ( end < ConfigDataDefine.GLOBAL_START_INDEX )
                        || ( start >= ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE ) )
                    {
                        data.Size = 0;
                        _writeData[ i ] = data;
                        continue;
                    }

                    start = Math.Clamp( start, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE );
                    end = Math.Clamp( end, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.GLOBAL_START_INDEX + ConfigDataDefine.SIZE );

                    data.Offset = start;
                    data.Size = end - start;
                    _writeData[ i ] = data;
                }

                if( data.Size > 0 )
                {
                    Array.Copy( m_SaveData, data.Offset, data.Buffer, 0, data.Size );
                }
            }
            if( _writeData.All( data => data.Size <= 0 ) ) return;

#if ENABLE_SWITCH_SAVE_SYSTEM
            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            nn.Result result = nn.fs.File.Open( ref m_FileHandle, SwitchDefine.SAVE_FILE_PATH, nn.fs.OpenFileMode.Write );
            if( nn.fs.FileSystem.ResultPathNotFound.Includes( result ) )
            {
                // 対象ファイルが存在しません。
                return;
            }

            if( nn.fs.FileSystem.ResultTargetLocked.Includes( result ) )
            {
                // 対象ファイルが既にオープンされています。
                return;
            }

            foreach( WriteBufferData data in _writeData )
            {
                if( data.Size <= 0 ) continue;
                result = nn.fs.File.Write( m_FileHandle, data.Offset, data.Buffer, data.Size, nn.fs.WriteOption.Flush );
                if( !result.IsSuccess() )
                {
                    nn.err.Error.Show( result );
                    nn.Nn.Abort( "Failed to write save data." );
                }
            }

            nn.fs.File.Close( m_FileHandle );
            result = nn.fs.FileSystem.Commit( SwitchDefine.SAVE_MOUNT_NAME );
            if( !result.IsSuccess() )
            {
                nn.err.Error.Show( result );
                nn.Nn.Abort( "Failed to close file system." );
            }

            // Nintendo Switch Guideline 0080
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#elif ENABLE_AES_ENCRYPT
            // 暗号化して保存
            byte[] encryptBuff = AesManager.EncryptToBytes( m_SaveData );
            using FileStream fs = new FileStream( m_SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );
            fs.Position = 0;
            fs.Write( encryptBuff, 0, encryptBuff.Length );
            fs.Close();
#else
            using FileStream fs = new FileStream( m_SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );

            foreach( WriteBufferData data in _writeData )
            {
                if( data.Size <= 0 ) continue;
                fs.Position = data.Offset;
                fs.Write( data.Buffer, 0, data.Size );
            }

            fs.Close();
#endif
        }

        /// <summary>
        /// セーブデータの指定の1byteだけ保存する
        /// </summary>
        /// <param name="_index">m_SaveDataのインデックス</param>
        private void SaveByte( int _index )
        {
            Save( _index, 1 );
        }

        /// <summary>
        /// BGMの音量の取得
        /// </summary>
        /// <value><c>true</c>BGM再生<c>false</c>BGM停止</value>
        public int BgmVolume
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM ];
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM ] = ( byte )Mathf.Clamp( value, ApplicationDefine.BGM_VOLUME_MIN, ApplicationDefine.BGM_VOLUME_MAX );
        }

        /// <summary>
        /// SEの音量の取得
        /// </summary>
        /// <value><c>true</c>BGM再生<c>false</c>BGM停止</value>
        public int SeVolume
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_SE ];
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_SE ] = ( byte )Mathf.Clamp( value, ApplicationDefine.SE_VOLUME_MIN, ApplicationDefine.SE_VOLUME_MAX );
        }

        /// <summary>
        /// 振動の再生状態の読込・保存
        /// </summary>
        /// <value><c>true</c>SE再生 <c>false</c>SE停止</value>
        public bool VibrationFlag
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_VIBRATION ] == 1;
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_VIBRATION ] = ( byte )( value ? 1 : 0 );
        }

        /// <summary>
        /// ボタンガイドの設定の読み込み・保存
        /// </summary>
        public bool ButtonGuideFlag
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BUTTON_GUIDE ] == 1;
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BUTTON_GUIDE ] = ( byte )( value ? 1 : 0 );
        }

        /// <summary>
        /// 言語設定
        /// </summary>
        public int LaungageID
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_LANGUAGE ];
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_LANGUAGE ] = ( byte )value;
        }

        /// <summary>
        /// BGM再生設定 : ステージセレクトの設定の取得・保存
        /// </summary>
        public BgmPlayType BgmPlayType_StageSelect
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE_SELECT ].ToEnum<BgmPlayType>();
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE_SELECT ] = value.ToByte();
        }

        /// <summary>
        /// BGM再生設定 : ステージ中の設定の取得・保存
        /// </summary>
        public BgmPlayType BgmPlayType_Stage
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE ].ToEnum<BgmPlayType>();
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_STAGE ] = value.ToByte();
        }

        /// <summary>
        /// BGM再生設定 : キャラセレクトの設定の取得・保存
        /// </summary>
        public BgmPlayType BgmPlayType_CharaSelect
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_CHARA_SELECT ].ToEnum<BgmPlayType>();
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_TYPE_CHARA_SELECT ] = value.ToByte();
        }

        /// <summary>
        /// BGM再生設定 : ラジオ1曲の設定の取得・保存
        /// </summary>
        public bool BgmPlayRadioOnce
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_RADIO_ONCE ] == 1;
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_BGM_PLAY_RADIO_ONCE ] = ( byte )( value ? 1 : 0 );
        }

        /// <summary>
        /// キーボードでの入力を許可
        /// </summary>
        public bool KeyboardControl
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_KEYBOARD_CONTROL ] == 1;
            set
            {
                m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_KEYBOARD_CONTROL ] = ( byte )( value ? 1 : 0 );
                // 村上：一旦、値変わったら保存されるように変更
                Save( ConfigDataDefine.GLOBAL_INDEX_KEYBOARD_CONTROL, ConfigDataDefine.LOCAL_INDEX_KEYBOARD_CONTROL );
            }
        }

        /// <summary>
        /// ディスプレイモード
        /// </summary>
        public int DisplayMode
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_DISPLAY_MODE ];
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_DISPLAY_MODE ] = ( byte )value;
        }

        /// <summary>
        /// 解像度
        /// </summary>
        public int DIsplayResolution
        {
            get => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_DISPLAY_RESOLUTION ];
            set => m_SaveData[ ConfigDataDefine.GLOBAL_INDEX_DISPLAY_RESOLUTION ] = ( byte )value;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void SaveConfig()
        {
            Save( ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.SIZE );
        }

        /// <summary>
        /// プレイヤーが選択しているキャラID
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <returns></returns>
        public int GetPlayerUseCharaID( int _playerId )
        {
            return CurrentSaveData[ PlayerInfoDefine.GLOBAL_START_INDEX + ( _playerId * PlayerInfoDefine.SIZE_PER_ONE_PLAYER ) ];
        }

        /// <summary>
        /// プレイヤーが選択しているキャラのスキンID
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <returns></returns>
        public int GetPlayerUseCharaSkinID( int _playerId )
        {
            return CurrentSaveData[ PlayerInfoDefine.GLOBAL_START_INDEX + ( _playerId * PlayerInfoDefine.SIZE_PER_ONE_PLAYER ) + 1 ];
        }

        /// <summary>
        /// プレイヤーが選択しているキャラの更新
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <param name="_charaId">キャラクターID</param>
        /// <param name="_skinId">キャラクターのスキンID</param>
        public void SetPlayerUseChara( int _playerId, int _charaId, int _skinId = 0 )
        {
            int index = PlayerInfoDefine.GLOBAL_START_INDEX + ( _playerId * PlayerInfoDefine.SIZE_PER_ONE_PLAYER );

            CurrentSaveData[ index ] = ( byte )_charaId;
            CurrentSaveData[ index + 1 ] = ( byte )_skinId;
        }

        /// <summary>
        /// プレイヤー名を取得
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <returns></returns>
        public string GetPlayerName( int _playerId )
        {
            int index = PlayerInfoDefine.GLOBAL_START_INDEX + ( PlayerInfoDefine.SIZE_PER_ONE_PLAYER * _playerId ) + 7;
            int size = CurrentSaveData[ index ];
            byte[] resultBuff = new byte[ size ];

            Array.Copy( CurrentSaveData, index + 1, resultBuff, 0, size );

            NSNgWordChecker.MaskProfanityWordsInText( Encoding.UTF8.GetString( resultBuff, 0, resultBuff.Length ), out string maskedPlayerName );

            return maskedPlayerName;
        }

        /// <summary>
        /// プレイヤー名の更新(セーブはしない)
        /// </summary>
        /// <param name="_playerId"></param>
        /// <param name="_name"></param>
        public void UpdatePlayerName( int _playerId, string _name )
        {
            NSNgWordChecker.MaskProfanityWordsInText( _name, out string maskedPlayerName );

            byte[] list = Encoding.UTF8.GetBytes( maskedPlayerName );
            byte size = ( byte )list.Length;

            byte[] result = new byte[ list.Length + 1 ];
            result[ 0 ] = size;

            Array.Copy( list, 0, result, 1, list.Length );
            Array.Copy( result, 0, CurrentSaveData, PlayerInfoDefine.GLOBAL_START_INDEX + 7 + ( PlayerInfoDefine.SIZE_PER_ONE_PLAYER * _playerId ), result.Length );
        }

        /// <summary>
        /// プレイヤー名の保存
        /// </summary>
        /// <param name="_playerId">プレイヤーID</param>
        /// <param name="_name">プレイヤー名</param>
        public void SavePlayerNameByteArray( int _playerId, string _name )
        {
            UpdatePlayerName( _playerId, _name );

            if( IsOnlineGuestMode ) return;

            Save( PlayerInfoDefine.GLOBAL_START_INDEX + ( PlayerInfoDefine.SIZE_PER_ONE_PLAYER * _playerId ), PlayerInfoDefine.SIZE_PER_ONE_PLAYER );
        }

        /// <summary>
        /// プレイヤー情報の保存
        /// </summary>
        public void SavePlayerInfo()
        {
            if( IsOnlineGuestMode ) return;

            Save( PlayerInfoDefine.GLOBAL_START_INDEX, PlayerInfoDefine.SIZE );
        }

        /// <summary>
        /// 指定のIDの項目を取得済みにする
        /// </summary>
        /// <remarks>鍵・メダル・スキンコイン・BGM・スキンの取得の際の共通処理</remarks>
        /// <param name="_id">ID</param>
        /// <param name="_globalStartIndex">セーブデータの開始インデックス</param>
        private void UnlockContents( int _id, int _globalStartIndex )
        {
            int index = _globalStartIndex + ( _id / BYTE_SIZE );
            int sift = _id % BYTE_SIZE;

            CurrentSaveData[ index ] |= ( byte )( 0x1 << sift );

            if( IsOnlineGuestMode ) return;
            if( IsAutoSaveToUnlockContents ) SaveByte( index );
        }

        /// <summary>
        /// 指定のIDの項目が取得済みか
        /// </summary>
        /// <remarks>鍵・メダル・スキンコイン・BGM・スキンの取得の際の共通処理</remarks>
        /// <param name="_id">ID</param>
        /// <param name="_globalStartIndex">セーブデータの開始インデックス</param>
        /// <returns>trueなら取得済み</returns>
        private bool IsUnlockContens( int _id, int _globalStartIndex )
        {
            if( _id < 0 ) return false;

            int index = _id / BYTE_SIZE;
            int sift = _id % BYTE_SIZE;
            byte bitFlag = ( byte )( 0b1 << sift );

            return ( CurrentSaveData[ _globalStartIndex + index ] & bitFlag ) == bitFlag;
        }

        /// <summary>
        /// 特定の取得済みの数を取得
        /// </summary>
        /// <remarks>鍵・メダル・スキンコイン・BGM・スキンの取得数の確認の際の共通処理</remarks>
        /// <param name="_globalStartIndex">セーブデータの開始インデックス</param>
        /// <param name="_dataSize">データサイズ(byte)</param>
        /// <returns></returns>
        private int GetUnlockCounts( int _globalStartIndex, int _dataSize )
        {
            int result = 0;

            for( int i = 0; i < _dataSize; i++ )
            {
                result += CurrentSaveData[ _globalStartIndex + i ].GetBitCount();
            }

            return result;
        }

        /// <summary>
        /// 指定のキャラを取得済みにする
        /// </summary>
        /// <param name="_charaId"></param>
        public void UnlockContentsToCharacter( int _charaId )
        {
            CurrentSaveData[ CharacterUnlockDefine.GLOBAL_START_INDEX + _charaId ] |= 0b1;

            if( IsOnlineGuestMode ) return;

            if( IsAutoSaveToUnlockContents ) Save( CharacterUnlockDefine.GLOBAL_START_INDEX, CharacterUnlockDefine.SIZE );
        }

        /// <summary>
        /// 指定のキャラが取得済みか
        /// </summary>
        /// <param name="_charaId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToCharacter( int _charaId )
        {
            return CurrentSaveData[ CharacterUnlockDefine.GLOBAL_START_INDEX + _charaId ].EqualLogicAnd( 0b1 );
        }

        /// <summary>
        /// アンロック済みのキャラクター数の取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToCharacter()
        {
            int result = 0;

            for( int i = 0; i < ApplicationDefine.CHARACTER_COUNT; i++ )
            {
                if( IsUnlockContentsToCharacter( i ) ) result++;
            }

            return result;
        }

        /// <summary>
        /// 指定の鍵を取得済みにする
        /// </summary>
        /// <param name="_keyId"></param>
        public void UnlockContentsToKey( int _keyId )
        {
            UnlockContents( _keyId, KeyUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// 指定の鍵が取得済みか
        /// </summary>
        /// <param name="_keyId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToKey( int _keyId )
        {
            return IsUnlockContens( _keyId, KeyUnlockDefine.GLOBAL_START_INDEX );
        }

        /// アンロック済みのメダルの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToKey()
        {
            return GetUnlockCounts( KeyUnlockDefine.GLOBAL_START_INDEX, KeyUnlockDefine.SIZE );
        }

        /// <summary>
        /// 指定のメダルを取得済みにする
        /// </summary>
        /// <param name="_medalId"></param>
        public void UnlockContentsToMedal( int _medalId )
        {
            UnlockContents( _medalId, MedalUnlockDefine.GLOBAL_START_INDEX );

            if( IsOnlineGuestMode ) return;

            AchievementManager.CheckAchievementGetMedal();
        }

        /// <summary>
        /// 指定のメダルが取得済みか
        /// </summary>
        /// <param name="_medalId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToMedal( int _medalId )
        {
            return IsUnlockContens( _medalId, MedalUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// アンロック済みのメダルの数を取得
        /// </summary>
        /// <remarks>実績の関係するID0000～ID0385が対象</remarks>
        /// <returns></returns>
        public int GetUnlockCountsToStoryModeMedal()
        {
            return GetUnlockCounts( MedalUnlockDefine.GLOBAL_START_INDEX, ApplicationDefine.ALL_MEDAL_COUNT );
        }

        /// <summary>
        /// アンロック済み全てのメダルの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToAllMedal()
        {
            return GetUnlockCounts( MedalUnlockDefine.GLOBAL_START_INDEX, MedalUnlockDefine.SIZE );
        }

        /// <summary>
        /// 表面に配置されている全てのメダルを取得しているかの判定
        /// </summary>
        /// <param name="_outUnlockMedalCount">開放済みの表面に配置されているメダルの数</param>
        /// <returns></returns>
        public bool IsAllUnlockMedalToSurfaceStage( out int _outUnlockMedalCount )
        {
            _outUnlockMedalCount = 0;
            bool isAllUnlockMedal = true;

            for( int i = 0; i < ApplicationDefine.SURFACE_STAGE_ALL_COUNT; i++ )
            {
                if( IsUnlockContentsToMedal( i ) ) _outUnlockMedalCount++;
                else isAllUnlockMedal = false;
            }

            return isAllUnlockMedal;
        }

        /// <summary>
        /// 裏面に配置されている全てのメダルを取得しているかの判定
        /// /// <param name="_outUnlockMedalCount">開放済みの裏面に配置されているメダルの数</param>
        /// </summary>
        /// <returns></returns>
        public bool IsAllUnlockMedalToReverseStage( out int _outUnlockMedalCount )
        {
            _outUnlockMedalCount = 0;
            bool isAllUnlockMedal = true;

            // 最終ステージ(ID385にはメダルはないので判定しない
            for( int i = 0; i < ApplicationDefine.REVERSE_STAGE_ALL_COUNT - 1; i++ )
            {
                if( IsUnlockContentsToMedal( ApplicationDefine.SURFACE_STAGE_ALL_COUNT + i ) ) _outUnlockMedalCount++;
                else isAllUnlockMedal = false;
            }

            return isAllUnlockMedal;
        }

        /// <summary>
        /// 表・裏に配置されている全てのメダルを取得しているかの判定
        /// </summary>
        /// <returns></returns>
        public bool IsAllUnlockMedal()
        {
            // 最終ステージ(ID385にはメダルはないので判定しない
            for( int i = 0; i < ApplicationDefine.ALL_STAGE_COUNT - 1; i++ )
            {
                if( !IsUnlockContentsToMedal( i ) ) return false;
            }

            return true;
        }

        /// <summary>
        /// スキンコインを取得済みにする
        /// </summary>
        /// <param name="_coinId"></param>
        public void UnlockContentsToSkinCoin( int _coinId )
        {
            UnlockContents( _coinId, SkinCoinUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// 指定のスキンコインが取得済みか
        /// </summary>
        /// <param name="_medalId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToSkinCoin( int _coinId )
        {
            return IsUnlockContens( _coinId, SkinCoinUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// アンロック済みのスキンコインの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToSkinCoin()
        {
            return GetUnlockCounts( SkinCoinUnlockDefine.GLOBAL_START_INDEX, SkinCoinUnlockDefine.SIZE );
        }

        /// <summary>
        /// メダルの消費量を取得
        /// </summary>
        /// <returns></returns>
        public int GetMedalConsumption()
        {
            return BitConverter.ToInt32( CurrentSaveData, ConsumptionDefine.GLOBAL_INDEX_MEDAL_CONSUMPTION ); ;
        }

        /// <summary>
        /// メダルの消費量を追加
        /// </summary>
        /// <param name="_add"></param>
        public void AddMedalConsumption( int _add )
        {
            int nextCount = Mathf.Clamp( GetMedalConsumption() + _add, 0, ConsumptionDefine.MAX_MEDAL_CONSUMPTION );
            byte[] buffer = BitConverter.GetBytes( nextCount );

            System.Array.Copy( buffer, 0, CurrentSaveData, ConsumptionDefine.GLOBAL_INDEX_MEDAL_CONSUMPTION, buffer.Length );
        }

        /// <summary>
        /// スキンコインの消費量を取得
        /// </summary>
        /// <returns></returns>
        public int GetSkinCoinConsumption()
        {
            return BitConverter.ToInt32( CurrentSaveData, ConsumptionDefine.GLOBAL_INDEX_SKIN_COIN_CONSUMPTION );
        }

        /// <summary>
        /// スキンコインの消費量を追加
        /// </summary>
        /// <param name="_add"></param>
        public void AddSkinCoinConsumption( int _add )
        {
            int nextCount = Mathf.Clamp( GetSkinCoinConsumption() + _add, 0, ConsumptionDefine.MAX_SKIN_COIN_CONSUMPTION );
            byte[] buffer = BitConverter.GetBytes( nextCount );

            System.Array.Copy( buffer, 0, CurrentSaveData, ConsumptionDefine.GLOBAL_INDEX_SKIN_COIN_CONSUMPTION, buffer.Length );
        }

        /// <summary>
        /// 指定の追加ステージエリアを解放済みにする
        /// </summary>
        /// <param name="_extraStageAreaId"></param>
        public void UnlockContentsToExtraStageArea( int _extraStageAreaId )
        {
            UnlockContents( _extraStageAreaId, ExtraStageAreaUnlockData.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// 指定の追加ステージエリアが解放済みか
        /// </summary>
        /// <param name="_bgmId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToExtraStageArea( int _extraStageAreaId )
        {
            return IsUnlockContens( _extraStageAreaId, ExtraStageAreaUnlockData.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// アンロック済みの追加ステージエリアの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToExtraStageArea()
        {
            return GetUnlockCounts( ExtraStageAreaUnlockData.GLOBAL_START_INDEX, ExtraStageAreaUnlockData.SIZE );
        }

        /// <summary>
        /// 指定のBGMを取得済みにする
        /// </summary>
        /// <param name="_bgmId"></param>
        public void UnlockContentsToBGM( int _bgmId )
        {
            UnlockContents( _bgmId, BGMUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// 指定のBGMが取得済みか
        /// </summary>
        /// <param name="_bgmId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToBGM( int _bgmId )
        {
            return IsUnlockContens( _bgmId, BGMUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// アンロック済みのBGMの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountsToBGM()
        {
            return GetUnlockCounts( BGMUnlockDefine.GLOBAL_START_INDEX, BGMUnlockDefine.SIZE );
        }

        /// <summary>
        /// 指定のスキンを取得済みにする
        /// </summary>
        /// <param name="_skinId"></param>
        public void UnlockContentsToSkin( int _skinId )
        {
            UnlockContents( _skinId, SkinUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// 指定のスキンが取得済みか
        /// </summary>
        /// <param name="_skinId"></param>
        /// <returns></returns>
        public bool IsUnlockContentsToSkin( int _skinId )
        {
            return IsUnlockContens( _skinId, SkinUnlockDefine.GLOBAL_START_INDEX );
        }

        /// <summary>
        /// アンロック済みのスキンの数を取得
        /// </summary>
        /// <returns></returns>
        public int GetUnlockCountToSkin()
        {
            return GetUnlockCounts( SkinUnlockDefine.GLOBAL_START_INDEX, SkinUnlockDefine.SIZE );
        }


        ///// <summary>
        ///// 指定のステージ開放演出が終了済みか
        ///// </summary>
        ///// <param name="_id"></param>
        ///// <returns></returns>
        public bool IsEndStageUnlockPerformance( int _id )
        {
            if( _id < 0 ) return false;

            int index = PerformanceEndStateDefine.GLOBAL_START_INDEX_STAGE_UNLOCK + ( _id / BYTE_SIZE );
            int sift = _id % BYTE_SIZE;
            byte bitFlag = ( byte )( 0b1 << sift );

            return ( CurrentSaveData[ index ] & bitFlag ) == bitFlag;
        }

        ///// <summary>
        ///// ステージ開放演出を終了
        ///// </summary>
        ///// <remarks>設定のみ行い保存はしない</remarks>
        ///// <param name="_id"></param>
        public void EndStageUnlockPerformance( int _id )
        {
            if( _id < 0 ) return;

            int index = PerformanceEndStateDefine.GLOBAL_START_INDEX_STAGE_UNLOCK + ( _id / BYTE_SIZE );
            int sift = _id % BYTE_SIZE;

            CurrentSaveData[ index ] |= ( byte )( 0x1 << sift );
        }

        ///// <summary>
        ///// 指定のメッセージ表示演出が終了済みか
        ///// </summary>
        ///// <param name="_id"></param>
        ///// <returns></returns>
        public bool IsEndMessagePerformance( int _id )
        {
            if( _id < 0 ) return false;

            int index = PerformanceEndStateDefine.GLOBAL_START_INDEX_MESSAGE + ( _id / BYTE_SIZE );
            int sift = _id % BYTE_SIZE;
            byte bitFlag = ( byte )( 0b1 << sift );

            return ( CurrentSaveData[ index ] & bitFlag ) == bitFlag;
        }

        ///// <summary>
        ///// メッセージ表示演出を終了
        ///// </summary>
        ///// <remarks>設定のみ行い保存はしない</remarks>
        ///// <param name="_id"></param>
        public void EndMessagePerformance( int _id )
        {
            if( _id < 0 ) return;

            int index = PerformanceEndStateDefine.GLOBAL_START_INDEX_MESSAGE + ( _id / BYTE_SIZE );
            int sift = _id % BYTE_SIZE;

            CurrentSaveData[ index ] |= ( byte )( 0x1 << sift );
        }

        /// <summary>
        /// ステージ開放演出の状態を保存
        /// </summary>
        public void SaveStageUnlockPerformanceState()
        {
            if( IsOnlineGuestMode ) return;

            Save( PerformanceEndStateDefine.GLOBAL_START_INDEX_STAGE_UNLOCK, PerformanceEndStateDefine.LOCAL_SIZE_STAGE_UNLOCK );
        }

        /// <summary>
        /// メッセージ表示演出の状態を保存
        /// </summary>
        public void SaveMessagePerformanceState()
        {
            if( IsOnlineGuestMode ) return;

            Save( PerformanceEndStateDefine.GLOBAL_START_INDEX_MESSAGE, PerformanceEndStateDefine.LOCAL_SIZE_MESSAGE );
        }

        /// <summary>
        /// 開放終了フラグを保存
        /// </summary>
        public void SavePerformanceState()
        {
            if( IsOnlineGuestMode ) return;

            Save( PerformanceEndStateDefine.GLOBAL_START_INDEX, PerformanceEndStateDefine.SIZE );
        }

        /// <summary>
        /// 描いた線の長さを取得
        /// </summary>
        /// <returns></returns>
        public uint GetDrawLineLength()
        {
            byte[] buff = new byte[ AchievementCountDataDeifne.LOCAL_SIZE_DRAW_LINE_LENGTH ];

            Array.Copy( CurrentSaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX_DRAW_LINE_LENGTH, buff, 0, AchievementCountDataDeifne.LOCAL_SIZE_DRAW_LINE_LENGTH );

            return BitConverter.ToUInt32( buff );
        }

        /// <summary>
        ///  描いた線の長さを設定(セーブはしない)
        /// </summary>
        /// <param name="_length"></param>
        public void SetDrawLineLength( uint _length )
        {
            if( _length < 0 ) _length = 0;
            if( _length > AchievementCountDataDeifne.LIMIT_DRAW_LINE_LENGTH ) _length = AchievementCountDataDeifne.LIMIT_DRAW_LINE_LENGTH;

            byte[] buff = BitConverter.GetBytes( _length );

            Array.Copy( buff, 0, CurrentSaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX_DRAW_LINE_LENGTH, AchievementCountDataDeifne.LOCAL_SIZE_DRAW_LINE_LENGTH );
        }

        /// <summary>
        /// リトライ回数の取得
        /// </summary>
        /// <returns></returns>
        public uint GetRetryCount()
        {
            byte[] buff = new byte[ AchievementCountDataDeifne.LOCAL_SIZE_RETRY_COUNT ];

            Array.Copy( CurrentSaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX_RETRY_COUNT, buff, 0, AchievementCountDataDeifne.LOCAL_SIZE_RETRY_COUNT );

            return BitConverter.ToUInt32( buff );
        }

        /// <summary>
        /// リトライ回数の設定
        /// </summary>
        /// <param name="_count"></param>
        public void SetRetryCount( uint _count )
        {
            if( _count < 0 ) _count = 0;
            if( _count > AchievementCountDataDeifne.LIMIT_RETRY_COUNT ) _count = AchievementCountDataDeifne.LIMIT_RETRY_COUNT;

            byte[] buff = BitConverter.GetBytes( _count );

            Array.Copy( buff, 0, CurrentSaveData, AchievementCountDataDeifne.GLOBAL_START_INDEX_RETRY_COUNT, AchievementCountDataDeifne.LOCAL_SIZE_RETRY_COUNT );
        }

        /// <summary>
        /// 実績カウントの保存
        /// </summary>
        public void SaveAchievementCount()
        {
            if( IsOnlineGuestMode ) return;

            Save( AchievementCountDataDeifne.GLOBAL_START_INDEX, AchievementCountDataDeifne.SIZE );
        }

        /// <summary>
        /// 指定のステージのアンロック(セーブはしない)
        /// </summary>
        /// <param name="_stageId">ステージID</param>
        public void UnlockStage_NotSave( int _stageId )
        {
            if( GetStageCondition( _stageId ) >= StageCondition.Unlock ) return;

            CurrentSaveData[ StageConditionDataDefine.GLOBAL_START_INDEX + _stageId ] |= StageCondition.Unlock.ToByte();
        }

        /// <summary>
        /// 指定のステージのアンロック
        /// </summary>
        /// <param name="_stageId">ステージID</param>
        public void UnlockStage( int _stageId )
        {
            if( GetStageCondition( _stageId ) >= StageCondition.Unlock ) return;

            int index = StageConditionDataDefine.GLOBAL_START_INDEX + _stageId;

            CurrentSaveData[ index ] |= StageCondition.Unlock.ToByte();

            SaveStageCondition( index );
        }

        /// <summary>
        /// 指定のステージのプレイ済み状態へ変更
        /// </summary>
        /// <param name="_stageId"></param>
        public void PlayedStage( int _stageId )
        {
            if( GetStageCondition( _stageId ) >= StageCondition.Played ) return;

            int index = StageConditionDataDefine.GLOBAL_START_INDEX + _stageId;

            CurrentSaveData[ index ] |= StageCondition.Played.ToByte();

            SaveStageCondition( index );
        }

        /// <summary>
        /// 指定のステージクリア
        /// </summary>
        /// <param name="_stageId">ステージID</param>
        /// <param name="_isOneMove">1手でクリアしたか</param>
        public void ClearStage( int _stageId, bool _isOneMove )
        {
            if( _stageId < 0 ) return;

            StageCondition condition = GetStageCondition( _stageId );

            if( _isOneMove )
            {
                //既に1手でクリアしている場合は更新しない
                if( condition == StageCondition.OneMoveCleared ) return;
            }
            else
            {
                //既にクリア済みの場合は更新しない
                if( condition >= StageCondition.Cleared ) return;
            }

            int index = StageConditionDataDefine.GLOBAL_START_INDEX + _stageId;

            CurrentSaveData[ index ] |= ( _isOneMove ? StageCondition.Cleared : StageCondition.OneMoveCleared ).ToByte();

            if( IsOnlineGuestMode ) return;

            SaveStageCondition( index );
            AchievementManager.CheckAchievementStageClear( _stageId );
        }

        /// <summary>
        /// ステージの状態更新
        /// </summary>
        /// <param name="_index">セーブデータのインデックス</param>
        private void SaveStageCondition( int _index )
        {
            if( IsOnlineGuestMode ) return;

            Save( _index, StageConditionDataDefine.STAGE_DATA_SIZE );
        }

        /// <summary>
        /// 全ステージの状態を保存する
        /// </summary>
        public void SaveAllStageCondition()
        {
            if( IsOnlineGuestMode ) return;

            Save( StageConditionDataDefine.GLOBAL_START_INDEX, StageConditionDataDefine.SIZE );
        }

        /// <summary>
        /// 指定のステージの状態を取得
        /// </summary>
        /// <param name="_stageID">ステージID</param>
        /// <returns></returns>
        public StageCondition GetStageCondition( int _stageID )
        {
            int index = StageConditionDataDefine.GLOBAL_START_INDEX + _stageID;

            return CurrentSaveData[ index ].ToEnum<StageCondition>();
        }

        /// <summary>
        /// クリア済みのステージ数を取得
        /// </summary>
        /// <remarks>追加ステージのクリア数は含まれない</remarks>
        /// <returns>クリアしているステージの数</returns>
        public int GetClearedStoryModeStageCount()
        {
            int result = 0;

            for( int i = 0; i < ApplicationDefine.ALL_STAGE_COUNT; i++ )
            {
                if( CurrentSaveData[ i + StageConditionDataDefine.GLOBAL_START_INDEX ] >= ( byte )StageCondition.Cleared ) result++;
            }

            return result;
        }

        /// <summary>
        /// 全てのクリア済みのステージ数を取得
        /// </summary>
        /// <returns>クリアしているステージの数</returns>
        public int GetClearedStageCount()
        {
            int result = 0;

            for( int i = 0; i < StageConditionDataDefine.SIZE; i++ )
            {
                if( CurrentSaveData[ i + StageConditionDataDefine.GLOBAL_START_INDEX ] >= ( byte )StageCondition.Cleared ) result++;
            }

            return result;
        }

        /// <summary>
        ///  表面のステージを全てクリアしたかの判定
        /// </summary>
        /// <param name="_outClearedStateCount">現在の表面のステージクリア数</param>
        /// <returns> trueなら全てクリア</returns>
        public bool IsAllClearedSurfaceStage( out int _outClearedStateCount )
        {
            _outClearedStateCount = 0;
            bool isAllCleared = true;

            for( int i = 0; i < ApplicationDefine.SURFACE_STAGE_ALL_COUNT; i++ )
            {
                if( CurrentSaveData[ StageConditionDataDefine.GLOBAL_START_INDEX + i ] >= ( byte )StageCondition.Cleared ) _outClearedStateCount++;
                else isAllCleared = false;
            }

            return isAllCleared;
        }

        /// <summary>
        /// 裏面のステージを全てクリアしたかの判定
        /// </summary>
        /// <param name="_outClearedStateCount">現在の裏面のステージクリア数</param>
        /// <returns> trueなら全てクリア</returns>
        public bool IsAllClearedReverseStage( out int _outClearedStateCount )
        {
            _outClearedStateCount = 0;
            bool isAllCleared = true;

            for( int i = 0; i < ApplicationDefine.REVERSE_STAGE_ALL_COUNT; i++ )
            {
                if( CurrentSaveData[ StageConditionDataDefine.GLOBAL_START_INDEX + ApplicationDefine.SURFACE_STAGE_ALL_COUNT + i ] >= ( byte )StageCondition.Cleared ) _outClearedStateCount++;
                else isAllCleared = false;
            }

            return isAllCleared;
        }

        /// <summary>
        /// 表・裏すべてのステージをクリアしたかの判定
        /// </summary>
        /// <returns>trueならすべてクリア</returns>
        public bool IsAllClearedStage()
        {
            bool isAllClearedSurface = IsAllClearedSurfaceStage( out int surfaceClearedCount );
            bool isAllClearedReverse = IsAllClearedSurfaceStage( out int reverseClearedCount );

            return isAllClearedSurface && isAllClearedReverse && ( surfaceClearedCount + reverseClearedCount == ApplicationDefine.ALL_STAGE_COUNT );
        }

        /// <summary>
        /// ゲーム終了時に実行する処理
        /// </summary>
        public void ApplicationQuit()
        {
#if ENABLE_SWITCH_SAVE_SYSTEM
            nn.fs.FileSystem.Unmount( SwitchDefine.SAVE_MOUNT_NAME );
#endif
        }

        /// <summary>
        /// 設定のセーブデータを複製して取得
        /// </summary>
        /// <returns></returns>
        public byte[] CopyConfig()
        {
            byte[] config = new byte[ ConfigDataDefine.SIZE ];

            Array.Copy( m_SaveData, ConfigDataDefine.GLOBAL_START_INDEX, config, 0, ConfigDataDefine.SIZE );

            return config;
        }

        /// <summary>
        /// 設定のセーブデータを上書き
        /// </summary>
        /// <param name="_configData"></param>
        /// <returns></returns>
        public bool UpdateConfig( byte[] _configData )
        {
            if( _configData == null || _configData.Length < ConfigDataDefine.SIZE ) return false;

            Array.Copy( _configData, 0, m_SaveData, ConfigDataDefine.GLOBAL_START_INDEX, ConfigDataDefine.SIZE );

            return true;
        }

        /// <summary>
        /// ステージ状態のセーブデータを上書きする
        /// </summary>
        /// <param name="_transferData">ステージ状態のセーブデータ</param>
        //        public void TransferSaveData( byte[] _transferData )
        //        {
        //#if ENABLE_SWITCH_SAVE_SYSTEM && !TRIAL_MODE_ON
        //            Array.Copy( _transferData, 0, m_SaveData, CharacterUnlockDefine.GLOBAL_START_INDEX, _transferData.Length );
        //            Save( _transferData, CharacterUnlockDefine.GLOBAL_START_INDEX, _transferData.Length );
        //#endif
        //        }

        private bool isNetworkMode;

        /// <summary>オンラインでゲストとしてセーブデータをホストから同期状態かのフラグ</summary>
        public bool IsNetworkMode => isNetworkMode;

        public bool StartNetworkMode( byte[] hostSaveData )
        {
            Debug.Log( "StartNetworkMode : " + isNetworkMode );

            var config = CopyConfig();
            if( UpdateSaveData( hostSaveData ) )
            {
                UpdateConfig( config );
                isNetworkMode = true;
                Debug.Log( "StartNetworkMode Success" );
                return true;
            }

            Debug.Log( "StartNetworkMode Failed" );
            return false;
        }

        public void StopNetworkMode()
        {
            Debug.Log( "StopNetworkMode : " + isNetworkMode );
            isNetworkMode = false;

            Load();
        }
    }
}