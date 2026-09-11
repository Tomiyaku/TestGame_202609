using System;

namespace CodeIcf.AssetManagement.AdressableManagement
{
    /// <summary>
    /// Assetの読み込み処理を呼び出した結果
    /// </summary>
    public enum AddressableLoadResult
    {
        /// <summary>読み込み済み</summary>
        Complited = 0x01,
        /// <summary>読み込み開始</summary>
        StartLoading = 0x02,
        /// <summary>読み込み中</summary>
        NowLoading = 0x03,

        /// <summary>何らかのエラー</summary>
        Error = 0x10,
        /// <summary>定義データが存在しない</summary>
        Error_MissingAddressableInfo = 0x11,
        /// <summary>Asset情報が存在しない</summary>
        Error_MissingAssetData = 0x12,
        /// <summary>シーンを読み込もうとしている</summary>
        Error_SceneAsset = 0x13,
        /// <summary>シーンファイルではないAssetを読み込もうとしている</summary>
        Error_NotSceneAsset = 0x14,
        /// <summary>不正なAsset識別子</summary>
        Error_BadIdentifier = 0x15,
        /// <summary>現在のPlatformでは読み込めないAssetを読み込もうとした</summary>
        Error_NotAvailablePlatform = 0x16,
        /// <summary>使用条件を見たいしていないAssetを読み込もうとした</summary>
        Error_NotAvailableCondition = 0x17
    }

    /// <summary>
    /// データの検索結果
    /// </summary>
    public enum AddressableSerachResult
    {
        /// <summary>成功</summary>
        Success,
        /// <summary>AddressableAsset定義データが存在しない</summary>
        Nothing_InfoList,
        /// <summary>AddressableAsset定義データが見つからない</summary>
        NotFind_Info,
        /// <summary>Assetが見つからない</summary>
        NotFind_AssetData,
        /// <summary>Identiferの種類が異なる</summary>
        TypeError,
    }

    public enum LoadSceneResult
    {
        None = 0,
        Success_LoadSceneAndSetActive = 1,
        Success_StartLoadScene = 2,
        Success_LoadedScene = 3,
        Success_StartActiveScene = 4,
        Success_SetActiveScene = 5,

        /// <summary>何らかのエラー</summary>
        Error = -1,
        /// <summary>定義データが存在しない</summary>
        Error_MissingAddressableInfo = -2,
        /// <summary>Asset情報が存在しない</summary>
        Error_MissingAssetData = -3,
        /// <summary>シーンを読み込もうとしている</summary>
        Error_SceneAsset = -4,
        /// <summary>シーンファイルではないAssetを読み込もうとしている</summary>
        Error_NotSceneAsset = -5,
        /// <summary>不正なAsset識別子</summary>
        Error_BadIdentifier = -6,
        Error_NotLoadedScene = -7,
    }

    /// <summary>
    /// Assetを使用することができるPlatform
    /// </summary>
    [Flags]
    public enum UsePlatformType
    {
        PC = 1,
        Switch = 1 << 1,
    }

    /// <summary>
    /// Assetの使用条件
    /// </summary>
    [Flags]
    public enum UseContidionType
    {
        /// <summary>Development BuildかEditorで実行中の場合だけ有効なリソース</summary>
        DevelopmentOnly = 1,
        /// <summary>Scripting Define Symbolsに「SWITCH_LIICA_PHYSICAL_EDITION」がある場合のみ有効なリソース</summary>
        PhysicalEditionOnly = 1 << 1,
        /// <summary>Scripting Define Symbolsに「SWITCH_AKSYSGAMES_PUBLISH」がある場合のみ有効なリソース</summary>
        AksysGames_Publish_Only = 1 << 2,
        /// <summary>Scripting Define Symbolsに「SWITCH_AKSYSGAMES_PUBLISH」がある場合に無効なリソース</summary>
        AksysGames_Publish_NotIncluded = 1<< 3,
    }

    /// <summary>
    /// AddressableAsset関連の定義
    /// </summary>
    public static class AddressableDefine
    {
        /// <summary>.AddressableAsset定義ファイルに設定するグループ名とラベル</summary>
        public const string ADRESSABLES_INFO_NAME = "AddressableInfo";
    }

    public static class TitleIdentifierDefine
    {
        public static readonly uint TITLE = AddressableIdentifier.CreateIdentifier( AddressableCategory.Title, 0 );
    }

    public static class StageSelectIdentifierDefine
    {
        public static readonly uint STAGE_SELECT = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 0 );
        public static readonly uint IMG_STAGE_THUMB_UNKNOWN = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 1 );
        public static readonly uint ICO_CURSOR_STAGESELECT = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 2 );
        public static readonly uint STAGE_LINK = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 3 );
        public static readonly uint SPHERES_PACKAGE = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 4 );
        public static readonly uint STAGE_GROUP_PAKCAGE = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 5 );
        public static readonly uint DEBUG_DATA_EDIT = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 6 );
        public static readonly uint FLIP_FIRST_PERFORMANCE = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 7 );
        public static readonly uint PREPARING_VIEW = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageSelect, 8 );
    }

    public static class CharacterSelectIdentifierDefine
    {
        public static readonly uint CHARACTER_SELECT = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 0 );
        public static readonly uint SKILL_RING = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 1 );
        public static readonly uint CHARA_ICON_IMAGE = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 2 );
        public static readonly uint IMG_UI_CHARASELECT_CHANGESKIN_KEYBOARD = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 3 );
        public static readonly uint IMG_UI_CHARASELECT_CHANGESKIN_PAD = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 4 );
        public static readonly uint IMG_UI_CHARASELECT_CHANGESKIN_SWITCH = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 5 );
        public static readonly uint CHARA_00 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 6 );
        public static readonly uint CHARA_01 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 7 );
        public static readonly uint CHARA_02 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 8 );
        public static readonly uint CHARA_03 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 9 );
        public static readonly uint CHARA_04 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 10 );
        public static readonly uint CHARA_05 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 11 );
        public static readonly uint CHARA_06 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 12 );
        public static readonly uint CHARA_07 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 13 );
        public static readonly uint CHARA_08 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 14 );
        public static readonly uint CHARA_09 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 15 );
        public static readonly uint CHARA_10 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 16 );
        public static readonly uint CHARA_11 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 17 );
        public static readonly uint CHARA_12 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 18 );
        public static readonly uint CHARA_13 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 19 );
        public static readonly uint CHARA_14 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 20 );
        public static readonly uint CHARA_15 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 21 );
        public static readonly uint CHARA_16 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 22 );
        public static readonly uint CHARA_17 = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 23 );
    }

    public static class MenuIdentifierDefine
    {
        public static readonly uint MENU_CANVAS = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 0 );
        public static readonly uint CONTENTS_CHARA_PC = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 1 );
        public static readonly uint CONTENTS_CONTROLLER_PC = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 2 );
        public static readonly uint CONTENTS_COOP_PC = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 3 );
        public static readonly uint CONTENTS_CHARA_SWITCH = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 4 );
        public static readonly uint CONTENTS_CONTROLLER_SWITCH = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 5 );
        public static readonly uint CONTENTS_COOP_SWICH = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 6 );
        public static readonly uint CONTENTS_COOP_SWICH_NA = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 7 );
        public static readonly uint SKIN_ITEM = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 8 );
        public static readonly uint SKIN_LINEUP = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 9 );
        public static readonly uint IMG_UI_MENU_BTN_TOP_CONTROLLER_PC = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 10 );
        public static readonly uint IMG_UI_MENU_BTN_TOP_CONTROLLER_SWITCH = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 11 );
    }

    public static class NewCharacterPerformanceIdentifierDefine
    {
        public static readonly uint NEW_CHARACER_PERFORMANCE = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 0 );
        public static readonly uint PERFORMANCE00_CHARA00 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 1 );
        public static readonly uint PERFORMANCE01_CHARA01 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 2 );
        public static readonly uint PERFORMANCE02_CHARA02 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 3 );
        public static readonly uint PERFORMANCE03_CHARA03 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 4 );
        public static readonly uint PERFORMANCE04_CHARA04 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 5 );
        public static readonly uint PERFORMANCE05_CHARA05 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 6 );
        public static readonly uint PERFORMANCE06_CHARA06 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 7 );
        public static readonly uint PERFORMANCE07_CHARA07 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 8 );
        public static readonly uint PERFORMANCE08_CHARA08 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 9 );
        public static readonly uint PERFORMANCE09_CHARA09 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 10 );
        public static readonly uint PERFORMANCE10_CHARA10 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 11 );
        public static readonly uint PERFORMANCE11_CHARA11 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 12 );
        public static readonly uint PERFORMANCE12_CHARA12 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 13 );
        public static readonly uint PERFORMANCE13_CHARA13 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 14 );
        public static readonly uint PERFORMANCE14_CHARA14 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 15 );
        public static readonly uint PERFORMANCE15_CHARA15 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 16 );
        public static readonly uint PERFORMANCE16_CHARA16 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 17 );
        public static readonly uint PERFORMANCE17_CHARA17 = AddressableIdentifier.CreateIdentifier( AddressableCategory.NewCharacterPerformance, 18 );
    }

    public static class ButtonGuideIdentifierDefine
    {
        public static readonly uint BUTTON_GUIDE_MANAGER = AddressableIdentifier.CreateIdentifier( AddressableCategory.ButtonGuide, 0 );
        public static readonly uint KEY_IMAGE_PC_CONTROLLER = AddressableIdentifier.CreateIdentifier( AddressableCategory.ButtonGuide, 1 );
        public static readonly uint KEY_IMAGE_KEYBOARD = AddressableIdentifier.CreateIdentifier( AddressableCategory.ButtonGuide, 2 );
        public static readonly uint KEY_IMAGE_SW = AddressableIdentifier.CreateIdentifier( AddressableCategory.ButtonGuide, 3 );
    }

    public static class LoadingViewIdentifierDefine
    {
        public static readonly uint LOADING_VIEW = AddressableIdentifier.CreateIdentifier( AddressableCategory.LoadingView, 0 );
    }

    public static class GameSceneIdentifierDefine
    {
        public static readonly uint GAME_SCENE = AddressableIdentifier.CreateIdentifier( AddressableCategory.GameScene, 0 );
    }

    public static class MessageWindowIdentifierDefine
    {
        public static readonly uint MESSAGE_WINDOW = AddressableIdentifier.CreateIdentifier( AddressableCategory.MessageWindow, 0 );
        public static readonly uint MESSAGE_DISPLAY_CONDITION_SETTING = AddressableIdentifier.CreateIdentifier( AddressableCategory.MessageWindow, 1 );
    }

    public static class AudioManagementIdentifierDefine
    {
        public static readonly uint AUDIO_MANAGER = AddressableIdentifier.CreateIdentifier( AddressableCategory.AudioManagement, 0 );
        public static readonly uint BGM_ASSET_SPECIFY = AddressableIdentifier.CreateIdentifier( AddressableCategory.AudioManagement, 1 );
        public static readonly uint SE_ASSET_SPECIFY = AddressableIdentifier.CreateIdentifier( AddressableCategory.AudioManagement, 2 );
    }

    public static class InputSystemIdentifierDefine
    {
        public static readonly uint UIINPUT_ACTIONS = AddressableIdentifier.CreateIdentifier( AddressableCategory.InputSystem, 0 );
        public static readonly uint KEYBOARD_CONTROL_ASSIGNMENT_DATA = AddressableIdentifier.CreateIdentifier( AddressableCategory.InputSystem, 1 );
        public static readonly uint SWITCH_KEYCODE_CONVERT_DATA = AddressableIdentifier.CreateIdentifier( AddressableCategory.InputSystem, 2 );
    }

    public static class EventSceneIdentifierDefine
    {
        public static readonly uint EVENT_SCENE = AddressableIdentifier.CreateIdentifier( AddressableCategory.EventScene, 0 );
    }

    public static class HowTo_CharaIdentifierDefine
    {
        public static readonly uint HOWTO_CHARA_99 = AddressableIdentifier.CreateIdentifier( AddressableCategory.HowTo_Chara, 0 );
        public static readonly uint HOWTO_CHARA_BG = AddressableIdentifier.CreateIdentifier( AddressableCategory.HowTo_Chara, 1 );
    }

    public static class ExtraStageIcon_ThumbnailIdentifierDefine
    {
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0000 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 0 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0001 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 1 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0002 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 2 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0003 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 3 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0004 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 4 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0005 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 5 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0006 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 6 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0007 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 7 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0008 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 8 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0009 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 9 );
        public static readonly uint EXTRA_STAGE_THUMBNAIL_E0010 = AddressableIdentifier.CreateIdentifier( AddressableCategory.ExtraStageIcon_Thumbnail, 10 );
    }
    public static class StageIconThumbnail_ExceptionIdentifierDefine
    {
        public static readonly uint EXCEPTION_STAGE_ICON_THUMBNAIL_DATA = AddressableIdentifier.CreateIdentifier( AddressableCategory.StageIconThumbnail_Exception, 0 );
    }

    public static class AchievementIdentifierDefine
    {
        public static readonly uint STEAM_ACHIEVEMENT_DATA = AddressableIdentifier.CreateIdentifier( AddressableCategory.Achievement, 0 );
        public static readonly uint CLEAR_ACHIEVEMENT_EDITOR = AddressableIdentifier.CreateIdentifier( AddressableCategory.Achievement, 1 );
    }
}