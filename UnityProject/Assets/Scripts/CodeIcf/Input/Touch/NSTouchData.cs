#if UNITY_SWITCH && !UNITY_EDITOR
#define SWITCH_INPUT_ENABLE
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

#if SWITCH_INPUT_ENABLE
using nn.hid;
#endif

namespace CodeIcf.Input.Touch
{
    /// <summary>
    /// タッチでの入力データ
    /// </summary>
    /// <remarks>UnityEditorで実行している場合、マウスの入力データが格納</remarks>    
    public struct NSTouchData
    {
        /// <summary>この構造体のデータが有効か</summary>
        public bool IsEnable { get; private set; }
        /// <summary>タッチID</summary>
        public int TouchId { get; private set; }
        /// <summary>タッチX座標</summary>
        public float TouchPositionX { get; private set; }
        /// <summary>タッチＹ座標</summary>
        public float TouchPositionY { get; private set; }
        /// <summary>タッチの座標</summary>
        public Vector2 Position => new Vector2( TouchPositionX, TouchPositionY );
        /// <summary>タッチの状態</summary>
        public TouchPhase Phase { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_pos"></param>
        /// <param name="_phase"></param>
        private NSTouchData( int _id, Vector2 _pos, TouchPhase _phase )
        {
            IsEnable = true;
            TouchId = _id;
            TouchPositionX = _pos.x ;
            TouchPositionY = _pos.y;
            Phase = _phase;
        }

        /// <summary>
        /// タッチ入力データの作成
        /// </summary>
        /// <param name="_device"></param>
        /// <returns></returns>
        public static List<NSTouchData> CreateDataList( InputDevice _device )
        {
            List<NSTouchData> list = new List<NSTouchData>();

#if SWITCH_INPUT_ENABLE
            Touchscreen touchscreen = _device as Touchscreen;

            if( touchscreen == null ) return null;

            foreach( TouchControl touchControl in touchscreen.touches )
            {
                NSTouchData touchData = new NSTouchData( touchControl.touchId.ReadValue(), touchControl.position.ReadValue(), touchControl.phase.ReadValue() );
                list.Add( touchData );
            }
#else //SWITCH_INPUT_ENABLE

            if( !( _device is UnityEngine.InputSystem.Mouse mouse ) ) return null;

            int id = 0;
            Vector2 pos = mouse.position.ReadValue();
            TouchPhase phase = TouchPhase.None;

            if( mouse.leftButton.IsPressed() )
            {
                phase = mouse.leftButton.wasPressedThisFrame ? TouchPhase.Began : TouchPhase.Moved;
            }
            else
            {
                if( mouse.leftButton.wasReleasedThisFrame ) phase = TouchPhase.Ended;
            }

            if( phase != TouchPhase.None )
            {
                NSTouchData touchData = new NSTouchData( id, pos, phase );
                list.Add( touchData );
            }
#endif //SWITCH_INPUT_ENABLE

            return list;
        }

        /// <summary>
        /// 画面をタッチしているか
        /// </summary>
        /// <returns></returns>
        public bool IsToch()
        {
            if( !IsEnable ) return false;

            return Phase == TouchPhase.Began || Phase == TouchPhase.Moved || Phase == TouchPhase.Stationary;
        }
    }
}