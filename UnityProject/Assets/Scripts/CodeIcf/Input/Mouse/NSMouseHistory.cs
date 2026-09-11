using CodeIcf.Input;

namespace CodeIcf.Input.Mouse
{
    /// <summary>
    /// マウスの入力履歴
    /// </summary>
    public class NSMouseHistory : AbstractNSInputHistory
    {
        /// <summary>入力データ</summary>
        public NSMouseState InputData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_inputData"></param>
        public NSMouseHistory( float _time, NSMouseState _inputData )
        {
            IsEnable = true;
            GetDataTime = _time;
            InputData = _inputData;
        }
    }
}