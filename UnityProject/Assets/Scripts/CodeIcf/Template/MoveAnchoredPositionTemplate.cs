using UnityEngine;

using CodeIcf.Extensions.Easing;

namespace CodeIcf.Template
{
    /// <summary>
    /// <see cref="RectTransform.anchoredPosition3D"/>での移動演出のテンプレート
    /// </summary>
    [System.Serializable]
    public class MoveAnchoredPositionTemplate
    {
        /// <summary>対象のオブジェクト</summary>
        public RectTransform TargetRectTransform;
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
            TargetRectTransform.anchoredPosition3D = StartPosition;
        }

        /// <summary>
        /// 対象オブジェクトの座標を終了座標に設定
        /// </summary>
        private void SetEndPosition()
        {
            TargetRectTransform.anchoredPosition3D = EndPosition;
        }

        /// <summary>
        /// 移動開始
        /// </summary>
        public void StartMoving()
        {
            SetStartPosition();
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

            TargetRectTransform.anchoredPosition3D = EasingVector3D.GetNowValue( NormalEasingType, time, MoveTime, StartPosition, EndPosition );

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

            TargetRectTransform.anchoredPosition3D = EasingVector3D.GetNowValue( ReverseEasingType, time, MoveTime, EndPosition, StartPosition );

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
    }
}