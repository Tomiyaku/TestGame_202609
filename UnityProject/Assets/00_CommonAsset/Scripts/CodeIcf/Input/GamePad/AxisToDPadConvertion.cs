using UnityEngine;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// ジョイスティック(アナログ入力)を十字キー(デジタル入力)の入力に変換するクラス
    /// </summary>
    public class AxisToDPadConvertion
    {
        /// <summary>水平方向入力値</summary>
        private float m_Horizonatal = 0;
        /// <summary>垂直方向入力値</summary>
        private float m_Vartical = 0;
        /// <summary>水平方向の閾値</summary>
        private float m_HorizonatalThreshold = 0.75f;
        /// <summary>垂直方向の閾値</summary>
        private float m_VarticalThreshold = 0.75f;
        /// <summary>押し続けたときにホールド判定が有効になるまでの時間</summary>
        private float m_HoldTime = 0.05f;
        /// <summary>ホールド判定が有効の場合、繰り返し入力までの時間</summary>
        private float m_RepeatTime = 0.5f;

        /// <summary>前回の押下判定の結果</summary>
        private DPad m_PrevDownResult = DPad.None;
        /// <summary>押下判定の結果</summary>
        private DPad m_DownResult = DPad.None;
        /// <summary>ホールド(長押し)判定の結果</summary>
        private DPad m_HoldResult = DPad.None;
        /// <summary>rリピート(一定時間長押しで繰り返し入力)判定の結果</summary>
        private DPad m_RepeatResult = DPad.None;
        /// <summary>押下判定の時間</summary>
        private float m_DownStartTime = -1;
        /// <summary>ホールド開始時間</summary>
        private float m_HoldStartTime = -1;

        /// <summary>水平方向の閾値</summary>
        public float HorizonatalThreshold
        {
            get => m_HorizonatalThreshold;
            set => m_HorizonatalThreshold = Mathf.Max( 0, value );
        }

        /// <summary>垂直方向の閾値</summary>
        public float VarticalThreshold
        {
            get => m_VarticalThreshold;
            set => m_VarticalThreshold = Mathf.Max( 0, value );
        }

        /// <summary>押し続けたときにホールド判定が有効になるまでの時間</summary>
        public float HoldTime
        {
            get => m_HoldTime;
            set => m_HoldTime = Mathf.Max( 0.05f, value );
        }

        /// <summary>ホールド判定が有効の場合、繰り返し入力までの時間</summary>
        public float RepeatTime
        {
            get => m_RepeatTime;
            set => m_RepeatTime = Mathf.Max( 0.05f, value );
        }

        /// <summary>
        /// 入力を更新
        /// </summary>
        /// <remarks>アナログ入力のデータを更新し、押下・ホールド・リピートの判定をここで行う</remarks>
        /// <param name="_horizonatal">アナログスティック水平方向入力値</param>
        /// <param name="_vartical">アナログスティック垂直方向入力値</param>
        public void Update( float _horizonatal, float _vartical )
        {
            DPad result = DPad.None;

            if( _horizonatal > m_HorizonatalThreshold ) result |= DPad.Right;
            else if( _horizonatal < -m_HorizonatalThreshold ) result |= DPad.Left;

            if( _vartical > m_VarticalThreshold ) result |= DPad.Up;
            else if( _vartical < -m_VarticalThreshold ) result |= DPad.Down;

            if( m_DownResult != result )
            {//結果を更新し、ホールド状態を解除
                m_DownStartTime = result == DPad.None ? -1 : Time.unscaledTime;
                m_HoldStartTime = -1;

                m_HoldResult = DPad.None;
                m_RepeatResult = DPad.None;               
            }
            else if( m_DownStartTime >= 0 )
            {//ホールド状態
                if( m_HoldStartTime < 0 )
                {
                    if( Time.unscaledTime - m_DownStartTime >= m_HoldTime )
                    {
                        m_HoldStartTime = Time.unscaledTime;
                        m_HoldResult = result;
                    }
                }
                else
                {
                    if( m_RepeatTime > 0 )
                    {
                        if( Time.unscaledTime - m_HoldStartTime >= m_RepeatTime )
                        {//一定時間ホールドしていればリピート
                            m_RepeatResult = result;
                            m_HoldStartTime = Time.unscaledTime;
                        }
                        else
                        {//リピート無し
                            m_RepeatResult = DPad.None;
                        }
                    }
                }
            }

            m_Horizonatal = _horizonatal;
            m_Vartical = _vartical;
            m_PrevDownResult = m_DownResult;
            m_DownResult = result;
        }

        /// <summary>
        /// 押下判定
        /// </summary>
        /// <param name="_dpadKey">方向キーの対応ビット 論理OR演算子で斜め判定も可能</param>
        /// <returns>押下直後ならtrue</returns>
        public bool IsDown( DPad _dpadKey )
        {
            return ( m_DownResult & _dpadKey ) == _dpadKey && ( m_PrevDownResult & _dpadKey ) != _dpadKey;
        }

        /// <summary>
        /// ホールド判定
        /// </summary>
        /// <param name="_dpadKey">方向キーの対応ビット 論理OR演算子で斜め判定も可能</param>
        /// <returns>押し続けていればtrue</returns>
        public bool IsHold( DPad _dpadKey )
        {
            return ( m_HoldResult & _dpadKey ) == _dpadKey;
        }

        /// <summary>
        /// リピート判定
        /// </summary>
        /// <param name="_dpadKey">方向キーの対応ビット 論理OR演算子で斜め判定も可能</param>
        /// <returns></returns>
        public bool IsRepeat( DPad _dpadKey )
        {
            return ( m_RepeatResult & _dpadKey ) == _dpadKey;
        }

        /// <summary>
        /// 押下orリピート判定
        /// </summary>
        /// <param name="_dpadKey">方向キーの対応ビット 論理OR演算子で斜め判定も可能</param>
        /// <returns></returns>
        public bool IsDownOrRepeat( DPad _dpadKey )
        {
            return IsDown( _dpadKey ) || IsRepeat( _dpadKey );
        }

        /// <summary>
        /// 離した判定
        /// </summary>
        /// <param name="_dpadKey">方向キーの対応ビット 論理OR演算子で斜め判定も可能</param>
        /// <returns></returns>
        public bool IsUp( DPad _dpadKey )
        {
            return ( m_DownResult & _dpadKey ) != _dpadKey && ( m_PrevDownResult & _dpadKey ) == _dpadKey;
        }

        /// <summary>
        /// 入力のリセット
        /// </summary>
        public void Reset()
        {
            m_Horizonatal = 0;
            m_Vartical = 0;
            m_DownResult = DPad.None;
            m_HoldResult = DPad.None;
            m_RepeatResult = DPad.None;

            m_DownStartTime = -1;
        }
    }
}