using UnityEngine;

namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語別リソースをまとめるためのScriptableObject
    /// </summary>
    [CreateAssetMenu( fileName = "LanguageAssetPack", menuName = "ScriptableObjects/Create LanguageAssetPack" )]
    public class LanguageAssetPack : ScriptableObject
    {
        /// <summary>言語別の種類</summary>
        public LanguagePackCategory PackCategory;
        /// <summary>Asset一覧</summary>
        public Object[] AssetList;
    }
}