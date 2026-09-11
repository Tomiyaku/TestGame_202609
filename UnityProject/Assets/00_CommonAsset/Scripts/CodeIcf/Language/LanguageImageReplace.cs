using UnityEngine;
using UnityEngine.UI;

namespace CodeIcf.LanguageResoucrsManagement
{
    /// <summary>
    /// 言語別画像リソースの差し替え処理
    /// </summary>
    public class LanguageImageReplace : MonoBehaviour
    {
        /// <summary>対象の言語別リソース</summary>
        [SerializeField]
        protected LanguageAssetAssignment m_AssetAssignment;
        /// <summary>差し替えの対象のImage</summary>
        protected Image Image { get; set; } = null;

        protected virtual void Awake()
        {
            Image = GetComponent<Image>();

            LanguageResourcesManager.AddImageRepace( this );            
        }

        protected virtual void Start()
        {
            ReplaceImage();
        }

        protected virtual void OnDestroy()
        {
            Image.sprite = null;
            LanguageResourcesManager.RemoveImageRepralce( this );
        }

        /// <summary>
        /// 画像の差し替え
        /// </summary>
        public virtual void ReplaceImage()
        {
            Image.sprite = LanguageResourcesManager.Instance.GetSprite( m_AssetAssignment.PackCategory, m_AssetAssignment.AssetIndex );
            Image.SetNativeSize();
        }        
    }
}