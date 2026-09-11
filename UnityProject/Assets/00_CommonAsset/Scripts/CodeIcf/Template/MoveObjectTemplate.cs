using UnityEngine;

using CodeIcf.Extensions.Easing;

namespace CodeIcf.Template
{
    /// <summary>
    /// オブジェクトの移動演出のテンプレート
    /// </summary>
    [System.Serializable]
    public class MoveObjectTemplate
    {
        /// <summary>対象のオブジェクト</summary>
        public Transform TargetTransform;
        /// <summary>開始座標</summary>
        public Vector3 StartPosition;
        /// <summary>終了座標</summary>
        public Vector3 EndPosition;
        /// <summary>演出時間</summary>
        public float MoveTime;
        /// <summary>通常時の変化率の処理の種類</summary>
        public EasingType NormalEasingType = EasingType.QuintOut;
        /// <summary>逆再生の変化率の処理の種類</summary>
        public EasingType ReverseEasingType = EasingType.QuintIn;
        /// <summary>演出開始時間</summary>
        public float StartTime { get; set; } = -1;

        /// <summary>
        /// 対象オブジェクトの座標を開始座標に設定
        /// </summary>
        private void SetStartPosition()
        {
            TargetTransform.position = StartPosition;
        }

        /// <summary>
        /// 対象オブジェクトの座標を終了座標に設定
        /// </summary>
        private void SetEndPosition()
        {
            TargetTransform.position = EndPosition;
        }

        /// <summary>
        /// 移動開始
        /// </summary>
        public void StartMoving()
        {
            SetStartPosition();
            StartTime = Time.unscaledTime;
        }

        public void StartMovingToReverse()
        {
            SetEndPosition();
            StartTime = Time.unscaledTime;
        }

        /// <summary>
        /// 移動の処理と終了判定
        /// </summary>
        /// <returns>移動処理を終了していればtrue</returns>
        public bool MovingObject()
        {
            if( StartTime < 0 ) return false;

            float time = Time.unscaledTime - StartTime;

            TargetTransform.position = EasingVector3D.GetNowValue( NormalEasingType, time, MoveTime, StartPosition, EndPosition );

            return time >= MoveTime;
        }

        /// <summary>
        /// 移動の処理と終了判定の逆再生
        /// </summary>
        /// <returns>移動処理を終了していればtrue</returns>
        public bool MovingObjectToReverse()
        {
            if( StartTime < 0 ) return false;

            float time = Time.unscaledTime - StartTime;

            TargetTransform.position = EasingVector3D.GetNowValue( ReverseEasingType, time, MoveTime, EndPosition, StartPosition );

            return time >= MoveTime;
        }

        /// <summary>
        /// 移動の終了
        /// </summary>
        public void EndMoving()
        {
            SetEndPosition();
            StartTime = -1;
        }

        public void EndMovingToReverse()
        {
            SetStartPosition();
            StartTime = -1;
        }
    }
}