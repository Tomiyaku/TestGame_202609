using UnityEngine;

namespace CodeIcf.VibrationManagement
{
    /// <summary>
    /// ビットが立っている場合に振動させるパターンデータ
    /// </summary>
    public class VibrationPattern
    {
        /// <summary>振動パターン</summary>
        private int m_Pattern = 0b01;
        /// <summary>パターン数</summary>
        private int m_PatternCount = 2;
        /// <summary>現在のビットの位置</summary>
        private int m_PatternIndex = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VibrationPattern()
        {
            m_Pattern = 0b01;
            m_PatternCount = 2;
            m_PatternIndex = 0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_pattern"></param>
        /// <param name="_count"></param>
        public VibrationPattern( int _pattern, int _count )
        {
            m_Pattern = _pattern;
            m_PatternCount = Mathf.Clamp( _count, 1, 32 );
            m_PatternIndex = 0;
        }

        /// <summary>
        /// ビットの位置を更新
        /// </summary>
        public void UpdatePattern()
        {
            m_PatternIndex = ( m_PatternIndex + 1 ) % m_PatternCount;
        }

        /// <summary>
        /// 現在のビットの位置でビットが立っているか
        /// </summary>
        /// <returns></returns>
        public bool IsEnableVibration()
        {
            int flag = 0b1 << m_PatternIndex;

            return ( m_Pattern & flag ) == flag;
        }

        /// <summary>
        /// 現在の位置のビットが立っていれば振動、そうでないなら停止
        /// </summary>
        /// <param name="_type">振動の種類</param>
        /// <param name="_deviceId">対象のコントローラーのデバイスID</param>
        public void PlayVibrationPattern( VibrationType _type, int _deviceId )
        {
            if( IsEnableVibration() ) VibrationManager.Instance.Play( _type, _deviceId );
            else VibrationManager.Instance.Stop( _deviceId );
        }

        /// <summary>
        /// 現在の位置のビットが立っていれば振動、そうでないなら停止
        /// </summary>
        /// <param name="_type">振動の種類</param>
        /// <param name="_deviceId">対象のコントローラーのデバイスID一覧</param>
        public void PlayVibrationPattern( VibrationType _type, int[] _deviceIdList )
        {
            if( _deviceIdList == null ) return;

            foreach( int deviceId in _deviceIdList ) PlayVibrationPattern( _type, deviceId );
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            m_PatternIndex = 0;
        }
    }
}