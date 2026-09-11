namespace CodeIcf.Language
{
    /// <summary>
    /// 言語関連定義
    /// </summary>
    public static class LanguageDefine
    {
        /// <summary>nn.oe.Language.GetDesired()で日本語の場合に帰ってくる文字列</summary>
        public const string JP_CODE = "ja";
        /// <summary>オプション画面のテクスチャファイルで言語別にファイル名に追加されている文字列</summary>
        public static readonly string[] ADD_TEXT_LIST = { "_EN", "_JP" };
        /// <summary>言語別AssetBundleの配置フォルダ名</summary>
        public static readonly string[] SYMBOL_LIST = { "EN", "JP" };

        /// <summary>言語の種類:英語(汎用)</summary>
        public const int LANGUAGE_TYPE_EN = 0;
        /// <summary>言語の種類:日本語</summary>
        public const int LANGUAGE_TYPE_JP = 1;
        /// <summary>このアプリが対応する言語の数</summary>
        public const int LANGUAGE_TYPE_SIZE = 2;
    }
}