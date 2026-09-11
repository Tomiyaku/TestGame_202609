using CodeIcf.Input;

namespace CodeIcf.Input.Touch
{
    /// <summary>
    /// タッチの入力履歴
    /// </summary>
    public class NSTouchHistory : AbstractNSInputHistory
    {
        /// <summary>入力データ</summary>
        public NSTouchData TouchData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_touchData"></param>
        public NSTouchHistory( float _time, NSTouchData _touchData )
        {
            IsEnable = true;
            GetDataTime = _time;
            TouchData = _touchData;
        }
    }
}