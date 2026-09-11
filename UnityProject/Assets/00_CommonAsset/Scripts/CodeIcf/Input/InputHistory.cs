using System.Collections.Generic;

using CodeIcf.Input.GamePad;
using CodeIcf.Input.Keyboard;
using CodeIcf.Input.PointerDevice;

namespace CodeIcf.Input
{
    /// <summary>
    /// 入力履歴データ
    /// </summary>
    public class InputHistory
    {
        /// <summary>このデータを取得した時間</summary>
        public float GetDataTime { get; private set; } = 0;
        /// <summary>ゲームパッド入力情報</summary>
        public List<GamePadInputData> InputDataList { get; private set; } = null;
        /// <summary>タッチ入力情報</summary>
        public List<PointerData> PointerDataList { get; private set; } = null;
        /// <summary>マウス入力情報</summary>
        public KeyboardInputData KeyboardData { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_deviceList"></param>
        /// <param name="_PointerList"></param>
        /// <param name="_keyboardData"></param>
        public InputHistory( float _time, List<GamePadDevice> _deviceList, List<PointerData> _PointerList, KeyboardInputData _keyboardData )
        {
            GetDataTime = _time;

            InputDataList = new List<GamePadInputData>();

            foreach( GamePadDevice device in _deviceList )
            {
                InputDataList.Add( device.InputData );
            }

            PointerDataList = new List<PointerData>( _PointerList );

            KeyboardData = _keyboardData;
        }
    }




}