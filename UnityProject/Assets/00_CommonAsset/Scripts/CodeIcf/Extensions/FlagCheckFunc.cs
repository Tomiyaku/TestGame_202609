using System;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// 特定のフラグを確認するための<see cref="Func{T, TResult}"/>を保持するクラス
    /// </summary>
    public class FlagCheckFunc
    {
        /// <summary>フラグ確認用処理</summary>
        private Func<bool> m_FlagCheckProcess;

        public FlagCheckFunc()
        {
            m_FlagCheckProcess = null;
        }

        /// <summary>
        /// フラグ状態確認
        /// </summary>
        /// <returns><see cref="m_FlagCheckProcess"/>がnullの場合は常にfalse</returns>
        public bool IsFlag()
        {
            if( m_FlagCheckProcess == null ) return false;

            return m_FlagCheckProcess();
        }

        /// <summary>
        /// フラグ確認処理を設定
        /// </summary>
        /// <param name="_process"></param>
        public void SetProcess( Func<bool> _process )
        {
            m_FlagCheckProcess = _process;
        }

        /// <summary>
        /// フラグ確認処理を解放
        /// </summary>
        public void ReleaseProcess()
        {
            m_FlagCheckProcess = null;
        }
    }
}