using CodeIcf.Input;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// コントローラーからの入力履歴
    /// </summary>
    public class GamePadHistory : AbstractInputHistory
    {
        /// <summary>入力データ</summary>
        public GamePadInputData InputData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_inputData"></param>
        public GamePadHistory( float _time, GamePadInputData _inputData )
        {
            IsEnable = true;
            GetDataTime = _time;
            InputData = _inputData;
        }
    }
}