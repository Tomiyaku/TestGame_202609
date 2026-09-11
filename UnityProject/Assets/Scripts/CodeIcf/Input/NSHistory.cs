using System.Collections.Generic;

using CodeIcf.Extensions;
using CodeIcf.Input.GamePad;
using CodeIcf.Input.Mouse;
using CodeIcf.Input.Touch;

namespace CodeIcf.Input
{
    /// <summary>
    /// 入力履歴データ
    /// </summary>
    public class NSHistory
    {
        /// <summary>このデータを取得した時間</summary>
        public float GetDataTime { get; private set; } = 0;
        /// <summary>ゲームパッド入力情報</summary>
        public List<NSGamePadInputData> InputDataList { get; private set; } = null;
        /// <summary>タッチ入力情報</summary>
        public List<NSTouchData> TouchDataList { get; private set; } = null;
        /// <summary>マウス入力情報</summary>
        public NSMouseState MouseState { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_inputList"></param>
        /// <param name="_touchList"></param>
        public NSHistory( float _time, List<NSGamePadDevice> _deviceList, List<NSTouchData> _touchList, NSMouseState _mouseState )
        {
            GetDataTime = _time;

            InputDataList = new List<NSGamePadInputData>();

            foreach( NSGamePadDevice device in _deviceList )
            {
                InputDataList.Add( device.InputData );
            }

            TouchDataList = new List<NSTouchData>( _touchList );

            MouseState = _mouseState;
        }
    }




}