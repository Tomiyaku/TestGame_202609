using System.Security.Cryptography;
using System.Text;

namespace jp.co.liica.q2.SaveDataManagement
{
    /// <summary>
    /// 暗号化・複合化を行うクラス
    /// </summary>
    public static class AesManager
    {
        /// <summary>初期化ベクトル</summary>
        private const string INITIALIZATION_VECTOR = "O)lkai>qTw1IF~(p";
        /// <summary>共通キー</summary>
        private const string SECRET_KEY = "*YmzGr5YFk#&g79'LVb_Lty.Ln*@J2ql";

        /// <summary>
        /// バイト配列を暗号化する
        /// </summary>
        /// <param name="_buff"></param>
        /// <returns></returns>
        public static byte[] EncryptToBytes( byte[] _buff )
        {
            byte[] result = null;

            using( Aes aes = Aes.Create() )
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = Encoding.UTF8.GetBytes( INITIALIZATION_VECTOR );
                aes.Key = Encoding.UTF8.GetBytes( SECRET_KEY );

                using( ICryptoTransform encryptor = aes.CreateEncryptor() )
                {
                    result = encryptor.TransformFinalBlock( _buff, 0, _buff.Length );
                }
            }

            return result;
        }

        /// <summary>
        /// バイト配列を復号化する
        /// </summary>
        /// <param name="_buff"></param>
        /// <returns></returns>
        public static byte[] DecryptToBytes( byte[] _buff )
        {
            byte[] result = null;

            using( Aes aes = Aes.Create() )
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = Encoding.UTF8.GetBytes( INITIALIZATION_VECTOR );
                aes.Key = Encoding.UTF8.GetBytes( SECRET_KEY );

                using( ICryptoTransform decryptor = aes.CreateDecryptor() )
                {
                    result = decryptor.TransformFinalBlock( _buff, 0, _buff.Length );
                }
            }

            return result;
        }
    }
}