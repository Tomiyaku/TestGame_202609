using UnityEngine;

using CodeIcf.Extensions;

namespace CodeIcf.Input.GamePad
{
    /// <summary>
    /// ゲームパッドのキーのリピート入力判定
    /// </summary>
    public class NSGamePadKeyRepeatChecker
    {
        /// <summary>
        /// キー毎のリピート入力判定
        /// </summary>
        public class KeyRepeatData
        {
            /// <summary>押下時間</summary>
            public float PressedTime { get; private set; } = -1;
            /// <summary>押し続けている時間</summary>
            public float HoldingTime { get; private set; } = -1;
            /// <summary>離した時間</summary>
            public float ReleaseTime { get; private set; } = -1;
            /// <summary>リピート入力の判定開始までの時間 </summary>
            public float RepeatCheckInterval { get ; private set; } = 0.5f;
            /// <summary>リピート間隔</summary>
            public float RepeatInterval { get; private set; } = -1;
            /// <summary>リピート入力がされたか</summary>
            public bool IsRepeat { get; private set; } = false;

            /// <summary>
            /// リピート入力が有効か
            /// </summary>
            public bool IsEnableRepeat => RepeatInterval > 0;

            /// <summary>
            /// 押下時間を更新
            /// </summary>
            public void DownKey()
            {
                PressedTime = Time.unscaledTime;
                HoldingTime = -1;
                ReleaseTime = -1;

                IsRepeat = false;
            }

            /// <summary>
            /// 押し続けている時間を更新
            /// </summary>
            public void HoldKey()
            {
                IsRepeat = false;

                if( RepeatInterval <= 0 ) return;
                if( PressedTime < 0 ) return;
                if( ReleaseTime >= 0 ) return;

                float now = Time.unscaledTime;

                if( now - PressedTime > RepeatCheckInterval )
                {
                    //押し続けている時間が未設定なら設定
                    if( HoldingTime < 0 ) HoldingTime = now;

                    if( now - HoldingTime >= RepeatInterval )
                    {//リピート入力判定
                        IsRepeat = true;
                        HoldingTime = now;
                    }
                }
            }

            /// <summary>
            /// 離した時間を更新
            /// </summary>
            public void UpKey()
            {
                PressedTime = -1;
                HoldingTime = -1;
                IsRepeat = false;

                ReleaseTime = Time.unscaledTime;
            }

            /// <summary>
            /// リピート間隔を設定
            /// </summary>
            /// <param name="_interval"></param>
            public void SetRepeat( float _interval )
            {
                RepeatInterval = _interval;
                IsRepeat = false;
            }
        }

        ///<summary>
        /// ボタンリスト
        /// </summary>
        /// <remarks>
        ///<see cref="GamepadKeyId"/>の<see cref="GamepadKeyId.None"/>以降と数と並びは同じ
        /// </remarks>
        public enum KeyList : int
        {
            A = 0,
            B,
            X,
            Y,
            L,
            R,
            ZL,
            ZR,
            D_Pad_Up,
            D_Pad_Down,
            D_Pad_Left,
            D_Pad_Right,
            LS,
            RS,
            Plus,
            Minus,

            KEY_COUNT,
        }

        /// <summary>リピート判定一覧</summary>
        private KeyRepeatData[] m_KeyRepeatDataList = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NSGamePadKeyRepeatChecker()
        {
            m_KeyRepeatDataList = new KeyRepeatData[ KeyList.KEY_COUNT.ToInt() ];

            for( int i = 0; i < m_KeyRepeatDataList.Length; i++ ) m_KeyRepeatDataList[ i ] = new KeyRepeatData();
        }

        /// <summary>
        /// <see cref="GamepadKeyId"/>をインデックスに変換
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        /// <returns></returns>
        private int ConvertIndex( GamepadKeyId _keyId )
        {
            switch( _keyId )
            {
                case GamepadKeyId.A: return KeyList.A.ToInt();
                case GamepadKeyId.B: return KeyList.B.ToInt();
                case GamepadKeyId.X: return KeyList.X.ToInt();
                case GamepadKeyId.Y: return KeyList.Y.ToInt();
                case GamepadKeyId.L: return KeyList.L.ToInt();
                case GamepadKeyId.R: return KeyList.R.ToInt();
                case GamepadKeyId.ZL: return KeyList.ZL.ToInt();
                case GamepadKeyId.ZR: return KeyList.ZR.ToInt();
                case GamepadKeyId.D_Pad_Up: return KeyList.D_Pad_Up.ToInt();
                case GamepadKeyId.D_Pad_Down: return KeyList.D_Pad_Down.ToInt();
                case GamepadKeyId.D_Pad_Left: return KeyList.D_Pad_Left.ToInt();
                case GamepadKeyId.D_Pad_Right: return KeyList.D_Pad_Right.ToInt();
                case GamepadKeyId.LS: return KeyList.LS.ToInt();
                case GamepadKeyId.RS: return KeyList.RS.ToInt();
                case GamepadKeyId.Plus: return KeyList.Plus.ToInt();
                case GamepadKeyId.Minus: return KeyList.Minus.ToInt();
            }

            return -1;
        }

        /// <summary>
        /// 指定のキーのリピート入力が有効かの判定
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        /// <returns>有効になっていればtrue</returns>
        public bool IsEnableRepeat( GamepadKeyId _keyId )
        {
            int index = ConvertIndex( _keyId );

            return m_KeyRepeatDataList[ index ].IsEnableRepeat;
        }

        /// <summary>
        /// 指定のキーのリピート入力を有効にする
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        /// <param name="_interval">リピート間隔</param>
        /// <returns></returns>
        public bool EnableRepaet( GamepadKeyId _keyId, float _interval )
        {
            if( _interval <= 0 ) return false;
            if( _keyId == GamepadKeyId.None ) return false;

            int index = ConvertIndex( _keyId );

            m_KeyRepeatDataList[ index ].SetRepeat( _interval );

            //Debug.Log( "Enable Repeat Key : " + _keyId.ToString() + " Interval : " + _interval );

            return true;
        }

        /// <summary>
        /// リピート入力を停止する
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        public void StopRepeat( GamepadKeyId _keyId )
        {
            if( _keyId == GamepadKeyId.None ) return;

            int index = ConvertIndex( _keyId );

            m_KeyRepeatDataList[ index ].SetRepeat( -1 );
        }

        /// <summary>
        /// 全てのリピート入力を停止する
        /// </summary>
        public void StopAllRepeat()
        {
            foreach( KeyRepeatData repeatData in m_KeyRepeatDataList ) repeatData.SetRepeat( -1 );
        }

        /// <summary>
        /// 指定のキーの押下時間を更新
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        public void SetKeyDownTime( GamepadKeyId _keyId )
        {
            if( _keyId == GamepadKeyId.None ) return;

            int index = ConvertIndex( _keyId );

            m_KeyRepeatDataList[ index ].DownKey();
        }

        /// <summary>
        /// 指定のキーの押し続けている時間を更新
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        public void CheckHoldRepeat( GamepadKeyId _keyId )
        {
            if( _keyId == GamepadKeyId.None ) return;

            int index = ConvertIndex( _keyId );

            m_KeyRepeatDataList[ index ].HoldKey();
        }

        /// <summary>
        /// 指定のキーの離した時間を更新
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        public void SetKeyUpTime( GamepadKeyId _keyId )
        {
            if( _keyId == GamepadKeyId.None ) return;

            int index = ConvertIndex( _keyId );

            m_KeyRepeatDataList[ index ].UpKey();
        }

        /// <summary>
        /// 指定のキーがリピート入力されたか
        /// </summary>
        /// <param name="_keyId">指定のキー</param>
        /// <returns></returns>
        public bool IsRepeat( GamepadKeyId _keyId )
        {
            if( _keyId == GamepadKeyId.None ) return false;

            int index = ConvertIndex( _keyId );

            return m_KeyRepeatDataList[ index ].IsRepeat;
        }
    }
}