using CodeIcf.Input;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// コントローラーからの入力履歴
    /// </summary>
    public class NSGamePadHistory : AbstractNSInputHistory
    {
        /// <summary>入力データ</summary>
        public NSGamePadInputData InputData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_inputData"></param>
        public NSGamePadHistory( float _time, NSGamePadInputData _inputData )
        {
            IsEnable = true;
            GetDataTime = _time;
            InputData = _inputData;
        }
    }
}