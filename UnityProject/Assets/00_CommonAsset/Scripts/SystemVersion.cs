namespace jp.co.liica.q2.Common
{
    public static class SystemVersion
    {
        /// <summary>メジャーバージョン</summary>
        public const int Major = 1;
        /// <summary>マイナーバージョン</summary>
        public const int Minor = 0;
        /// <summary>ビルド</summary>
        public const int Build = 5;

        public static string GetVersion()
        {
            return $"{Major}.{Minor}.{Build}";
        }
    }
}
