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


    /// <summary>
    /// AdressablesAssetの種類
    /// </summary>
    /// <remarks>各値の表示名がAdressableのグループ名になります</remarks>
    public enum AddressableCategory : int
    {
        Menu = 0,
        Title,
        StageSelect,
        CharacterSelect,
        NewCharacterPerformance,
        ButtonGuide,
        LoadingView,
        Stage,
        Actor,
        GameScene,
        StageIconThumbnail,
        StageUnlockPerformanceData,
        StageSelect_BgParticle,
        MessageWindow,
        LanguageResources_EN,
        LanguageResources_JP,
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
    }

    public static class CharacterSelectIdentifierDefine
    {
        public static readonly uint CHARACTER_SELECT = AddressableIdentifier.CreateIdentifier( AddressableCategory.CharacterSelect, 0 );
    }

    public static class MenuIdentifierDefine
    {
        public static readonly uint MENU_CANVAS = AddressableIdentifier.CreateIdentifier( AddressableCategory.Menu, 0 );
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
    }
}