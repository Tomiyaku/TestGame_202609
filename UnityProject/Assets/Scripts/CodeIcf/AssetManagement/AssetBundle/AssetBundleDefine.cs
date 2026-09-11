using UnityEngine;

namespace CodeIcf.AssetManagement.AssetBundleManagement
{
    /// <summary>
    /// Assetの読み込み処理を呼び出した結果
    /// </summary>
    public enum AssetLoadResultCode
    {
        /// <summary>読み込み済み</summary>
        Complited,
        /// <summary>読み込み中</summary>
        NowLoading,

        /// <summary>定義データが存在しない</summary>
        Error_MissingAssetBundleInfo,
        /// <summary>Asset情報が存在しない</summary>
        Error_MissingAssetData,

        /// <summary>何らかのエラー</summary>
        Error,
    }

    /// <summary>
    ///  AssetBundleの種類
    /// </summary>
    /// <remarks><see cref="AssetBundleCategory.AssetBundleInfo"/>以外はゲーム毎に設定</remarks>
    public enum AssetBundleCategory : int
    {
        /// <summary>AssetBundle定義ファイル</summary>
        AssetBundleInfo = 0,
    }

    /// <summary>
    /// Assetに関する定義
    /// </summary>
    public static class AssetBundleDefine
    {
        /// <summary>AssetBundle定義ファイルのAssetBundleName</summary>
        public const string ASSETBUNDLE_INFO_NAME = "assetbundleinfo";
        /// <summary>AssetBundle定義ファイルのAssetBundleへのパス</summary>
        public static readonly string ASSETBUNDLE_DEFINITAION_INFO_PATH = Application.streamingAssetsPath + "/" + ASSETBUNDLE_INFO_NAME;

        /// <summary>Assetの配置をしているディレクトリのパス</summary>
        public const string ASSET_FILE_DIRECTRY_PATH = "Assets/AssetBundleResources/";
        /// <summary>Assetの配置パスベーステキスト</summary>
        public const string ASSET_FILE_PATH_BASE = ASSET_FILE_DIRECTRY_PATH + "{0}{1}{2}";

        #region AddOnContents

        /// <summary>追加コンテンツへアクセスするため、ファイルシステムを利用するためにマウントした際の識別子</summary>
        /// <remarks>マウント名のサイズは終端のNULL含めて2byte以上 nn::fs::MountNameLengthMax(現時点で15) + 1以下</remarks>
        public const string AOC_MOUNT_NAME = "Aoc_Root";
        /// <summary>追加コンテンツへアクセスするためのパスのベーステキスト</summary>
        /// <remarks>「マウント名 + :」以降のパスのサイズは終端のNULL含めて2byte以上 nn::fs::PathSizeMax(現時点で768) + 1以下</remarks>
        public const string AOC_PAHT_BASE = AOC_MOUNT_NAME + ":/{0}";
        /// <summary>インストール済みで権利を保有する追加コンテンツの追加コンテンツインデックスのリストを取得する際の上限数</summary>
        public const int AOC_LISTUP_COUNT_MAX = 32;

        #endregion //#region AddOnContents

#if UNITY_EDITOR
        /// <summary>AssetBundle定義ファイルが格納されているフォルダへのパス</summary>
        public const string ASSET_BUNDLE_INFO_DIRECTORY_PATH = ASSET_FILE_DIRECTRY_PATH + "AssetBundleInfo";
        /// <summary>UnityEditor上でAssetBundleを読み込まない場合に、AssetBundleを読み込んでいるふりをしている時間</summary>
        public const float ASSET_BUNDLE_LOAD_PSEUDO_DELAY = 0.25f;
        /// <summary>AssetBundleを読み込んでいるふりをしている時間を、同時に読み込んでいるふりをしている数だけ増価させるかのフラグ</summary>
        public const bool IS_PSEUDO_DELAY_ADDITION_COUNT = true;
#endif

        //これ以降はプロジェクト固有の定義
    }
}