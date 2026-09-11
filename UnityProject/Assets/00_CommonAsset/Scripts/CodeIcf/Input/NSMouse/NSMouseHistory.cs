namespace CodeIcf.Input.NSMouse
{
    /// <summary>
    /// マウスの入力履歴
    /// </summary>
    public class NSMouseHistory : AbstractInputHistory
    {
        /// <summary>入力データ</summary>
        public NSMouseData InputData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_inputData"></param>
        public NSMouseHistory( float _time, NSMouseData _inputData )
        {
            IsEnable = true;
            GetDataTime = _time;
            InputData = _inputData;
        }
    }
}