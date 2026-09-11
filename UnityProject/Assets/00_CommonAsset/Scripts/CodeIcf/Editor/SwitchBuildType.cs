namespace CodeIcf.EditorScripts
{
    /// <summary>
    /// Nintendo Switch版でのバージョン判定用定義
    /// </summary>
    public enum SwitchBuildType : int
    {
        /// <summary>Nintendo Switch版ではない</summary>
        None_SwitchEdition = -1,
        /// <summary>Liica/ダウンロード版</summary>
        Liica_Download,
        /// <summary>Liica/物理エディション</summary>
        Liica_PhysicalEdition,
        /// <summary>Aksys Games/マルチアプリケーション</summary>
        AksysGames_Publish,
    }
}
