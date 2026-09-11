namespace CodeIcf.Input.PointerDevice
{
    /// <summary>
    /// ポインティングデバイスの入力履歴
    /// </summary>
    public class PointerHistory : AbstractInputHistory
    {
        /// <summary>入力データ</summary>
        public PointerData TouchData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_touchData"></param>
        public PointerHistory( float _time, PointerData _touchData )
        {
            IsEnable = true;
            GetDataTime = _time;
            TouchData = _touchData;
        }
    }
}