namespace CodeIcf.AssetManagement
{
    public static class AssetCommonDefine
    {
        /// <summary>Asset識別子のうち、種類(現時点ではAssetbundleかAddressableAssetか)を識別するためのビットフラグの範囲</summary>
        /// <remarks>Identiferの構造としては [識別用ビットフラグ(8bit)][カテゴリ(12bit)][カテゴリ毎のAssetのインデックス(12bit)]の計32bit(uint型)を想定</remarks>        
        public const uint IDENTIFIER_FLAG_RANGE = 0xFF000000;
        /// <summary>Asset識別子のうち、カテゴリを識別するためのビットフラグの範囲</summary>
        /// <remarks>Identiferの構造としては [識別用ビットフラグ(8bit)][カテゴリ(12bit)][カテゴリ毎のAssetのインデックス(12bit)]の計32bit(uint型)を想定</remarks>        
        public const uint IDENTIFER_CATEGORY_RANGE = 0x00FFF000;
        /// <summary>Asset識別子のうち、Assetのインデックスを識別するためのビットフラグの範囲</summary>
        /// <remarks>Identiferの構造としては [識別用ビットフラグ(8bit)][カテゴリ(12bit)][カテゴリ毎のAssetのインデックス(12bit)]の計32bit(uint型)を想定</remarks>        
        public const uint IDENTIFER_INDEX_RANGE = 0x00000FFF;

        /// <summary>各カテゴリの値をuint型に変換した場合に、<see cref="AddressablesCategory"/>かを判別するためのビットフラグ</summary>        
        public const uint CATEGORY_FLAG_ADRESSABLES = 0x1 << ( FLAG_BIT_SIZE - 1 );
        /// <summary>Identifer(uint型)の状態でIdentifierがAssetBunldeのものかAdressableAssetのものか判別する溜めのフラグ</summary>
        /// <remarks>Identifierの最上位ビットが立っている場合はAdressableAssetのIdentifier</remarks>
        public static readonly uint IDENTIFIER_FLAG_ADRESSABLES = CATEGORY_FLAG_ADRESSABLES << ( CATEGORY_BIT_SIZE + INDEX_BIT_SIZE );

        /// <summary>Asset識別子のうち、種類を識別するためのフラグのサイズ</summary>
        public const int FLAG_BIT_SIZE = 8;
        /// <summary>Asset識別子のうち、カテゴリのサイズ</summary>
        public const int CATEGORY_BIT_SIZE = 12;
        /// <summaryAsset識別子のうち、カテゴリ毎のインデックスのサイズ</summary>        
        public const int INDEX_BIT_SIZE = 12;
    }
}