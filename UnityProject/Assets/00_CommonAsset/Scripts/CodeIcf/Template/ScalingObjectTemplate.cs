using UnityEngine;

using CodeIcf.Extensions.Easing;

namespace CodeIcf.Template
{
    /// <summary>
    /// オブジェクトの拡大・縮小演出のテンプレート
    /// </summary>
    [System.Serializable]
    public class ScalingObjectTemplate
    {
        /// <summary>対象のオブジェクト</summary>
        public Transform TargetTransform;
        /// <summary>開始時のScale</summary>
        public Vector3 StartScale;
        /// <summary>終了時のScale</summary>
        public Vector3 EndScale;
        /// <summary>演出時間</summary>
        public float ScaleTime;
        /// <summary>通常時の変化率の処理の種類</summary>
        public EasingType NormalEasingType = EasingType.QuintOut;
        /// <summary>逆再生の変化率の処理の種類</summary>
        public EasingType ReverseEasingType = EasingType.QuintIn;
        /// <summary>演出開始時間</summary>
        public float StartTime { get; set; } = -1;

        /// <summary>
        /// 対象オブジェクトのScaleを開始時の値に設定
        /// </summary>
        private void SetStartScale()
        {
            TargetTransform.localScale = StartScale;
        }

        /// <summary>
        /// 対象オブジェクトのScaleを終了時の値に設定
        /// </summary>
        private void SetEndScale()
        {
            TargetTransform.localScale = EndScale;
        }

        /// <summary>
        /// 演出の開始
        /// </summary>
        public void StartScaling()
        {
            SetStartScale();
            StartTime = Time.unscaledTime;
        }

        /// <summary>
        /// 拡大・縮小の処理と終了判定
        /// </summary>
        /// <returns>移動処理を終了していればtrue</returns>
        public bool ScalingObject()
        {
            if( StartTime < 0 ) return false;

            float time = Time.unscaledTime - StartTime;

            Vector3 newScale = EasingVector3D.GetNowValue( NormalEasingType, time, ScaleTime, StartScale, EndScale );

            float x = float.IsNaN( newScale.x ) ? EndScale.x : newScale.x;
            float y = float.IsNaN( newScale.y ) ? EndScale.y : newScale.y;
            float z = float.IsNaN( newScale.z ) ? EndScale.z : newScale.z;

            x = float.IsInfinity( x ) ? TargetTransform.localScale.x : x;
            y = float.IsInfinity( y ) ? TargetTransform.localScale.y : y;
            z = float.IsInfinity( z ) ? TargetTransform.localScale.z : z;

            TargetTransform.localScale = new Vector3( x, y, z );

            return time >= ScaleTime;
        }

        /// <summary>
        /// 拡大・縮小の処理と終了判定の逆再生
        /// </summary>
        /// <returns>移動処理を終了していればtrue</returns>
        public bool ScalingObjectToReverse()
        {
            if( StartTime < 0 ) return false;

            float time = Time.unscaledTime - StartTime;

            Vector3 newScale = EasingVector3D.GetNowValue( ReverseEasingType, time, ScaleTime, EndScale, StartScale );

            float x = float.IsNaN( newScale.x ) ? StartScale.x : newScale.x;
            float y = float.IsNaN( newScale.y ) ? StartScale.y : newScale.y;
            float z = float.IsNaN( newScale.z ) ? StartScale.z : newScale.z;

            x = float.IsInfinity( x ) ? TargetTransform.localScale.x : x;
            y = float.IsInfinity( y ) ? TargetTransform.localScale.y : y;
            z = float.IsInfinity( z ) ? TargetTransform.localScale.z : z;

            TargetTransform.localScale = new Vector3( x, y, z );

            return time >= ScaleTime;
        }

        /// <summary>
        /// 演出の終了
        /// </summary>
        public void EndScaling()
        {
            StartTime = -1;
            SetEndScale();
        }

        /// <summary>
        /// 現在の処理の状態を0~1で取得
        /// </summary>
        public float Ratio
        {
            get
            {
                if( StartTime < 0 ) return 0;

                return Mathf.Clamp( ( Time.unscaledTime - StartTime ) / ScaleTime, 0, 1 );
            }
        }
    }
}