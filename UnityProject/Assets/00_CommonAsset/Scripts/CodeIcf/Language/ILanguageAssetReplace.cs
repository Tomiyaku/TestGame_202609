namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語切り替えを行った後にリソース切り替えを行うためのインターフェイス
    /// </summary>
    public interface ILanguageAssetReplace
    {
        /// <summary>
        /// 言語を切り替えた後に実行するリソース切り替え処理
        /// </summary>
        public void ReplaceAsset();
    }
}