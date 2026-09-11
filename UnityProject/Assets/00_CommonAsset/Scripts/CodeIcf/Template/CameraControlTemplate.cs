using UnityEngine;

using CodeIcf.Extensions.Easing;

namespace CodeIcf.Template
{
    public enum PerformanceEasingType
    {
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        SineIn,
        SineOut,
        SineInOut,
        ExpIn,
        ExpOut,
        ExpInOut,
        CircIn,
        CircOut,
        CircInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        Linear,
    }

    /// <summary>
    /// カメラ移動演出のデータ
    /// </summary>
    [System.Serializable]
    public class ControlData
    {
        /// <summary>座標</summary>
        public Vector3 Position;
        /// <summary>FIeldOdView</summary>
        /// <remarks>カメラのProjecttionがPerspectiveの場合のみ有効</remarks>
        public float FieldOfView;
        /// <summary>OrthographicSize</summary>
        /// <remarks>カメラのProjecttionがOrthographicの場合のみ有効</remarks>
        public float OrthographicSize;
    }

    /// <summary>
    /// カメラ移動演出テンプレート
    /// </summary>
    [System.Serializable]
    public class CameraControlTemplate
    {
        /// <summary>対象のカメラ</summary>
        public Camera TargetCamera;
        /// <summary>演出開始時の情報</summary>
        public ControlData Start;
        /// <summary>演出終了時の情報</summary>
        public ControlData End;
        /// <summary>演出時間</summary>
        public float MoveTime;
        /// <summary>演出開始時間</summary>
        public float StartTime { get; set; } = -1;
        /// <summary>演出の経過時間</summary>
        public float ProgressTime { get;private set; } = -1;

        /// <summary>
        /// 演出開始
        /// </summary>
        public void StartPerformance()
        {
            StartTime = Time.unscaledTime;
        }

        /// <summary>
        /// 演出中の処理と終了判定
        /// </summary>
        /// <param name="_posStyle">座標移動の変化率の処理の種類</param>
        /// <param name="_viewStyle">FieldOfViewの変化率の処理の種類</param>
        /// <param name="_sizeStype">Sizeの変化率の処理の種類</param>
        /// <returns>演出が終了していればtrue</returns>
        public bool PerformanceObject( PerformanceEasingType _posStyle = PerformanceEasingType.QuintOut, PerformanceEasingType _viewStyle = PerformanceEasingType.ExpIn, PerformanceEasingType _sizeStype = PerformanceEasingType.ExpIn )
        {
            if( StartTime < 0 ) return true;

            ProgressTime = Mathf.Min( Time.unscaledTime - StartTime, MoveTime );

            if( Start.Position != End.Position ) TargetCamera.transform.position = EasingVector3D.GetNowValue( ( EasingType )_posStyle, ProgressTime, MoveTime, Start.Position, End.Position );
            if( Start.FieldOfView != End.FieldOfView ) TargetCamera.fieldOfView = EasingFloat.GetNowValue( ( EasingType )_viewStyle, ProgressTime, MoveTime, Start.FieldOfView, End.FieldOfView );
            if( Start.OrthographicSize != End.OrthographicSize ) TargetCamera.orthographicSize = EasingFloat.GetNowValue( ( EasingType )_sizeStype, ProgressTime, MoveTime, Start.OrthographicSize, End.OrthographicSize );

            return ProgressTime >= MoveTime;
        }

        /// <summary>
        /// 演出中の処理と終了判定を逆再生で行う
        /// </summary>
        /// <param name="_posStyle">座標移動の変化率の処理の種類</param>
        /// <param name="_viewStyle">FieldOfViewの変化率の処理の種類</param>
        /// <param name="_sizeStype">Sizeの変化率の処理の種類</param>
        /// <returns>演出が終了していればtrue</returns>
        public bool PerformanceObjectToReverse( PerformanceEasingType _posStyle = PerformanceEasingType.QuintOut, PerformanceEasingType _viewStyle = PerformanceEasingType.ElasticOut, PerformanceEasingType _sizeStype = PerformanceEasingType.ExpOut )
        {
            if( StartTime < 0 ) return true;

            ProgressTime = Time.unscaledTime - StartTime;

            if( Start.Position != End.Position ) TargetCamera.transform.position = EasingVector3D.GetNowValue( ( EasingType )_posStyle, ProgressTime, MoveTime, End.Position, Start.Position );
            if( Start.FieldOfView != End.FieldOfView ) TargetCamera.fieldOfView = EasingFloat.GetNowValue( ( EasingType )_viewStyle, ProgressTime, MoveTime, End.FieldOfView, Start.FieldOfView );
            if( Start.OrthographicSize != End.OrthographicSize ) TargetCamera.orthographicSize = EasingFloat.GetNowValue( ( EasingType )_sizeStype, ProgressTime, MoveTime, End.OrthographicSize, Start.OrthographicSize );

            return ProgressTime >= MoveTime;
        }

        /// <summary>
        /// 演出の終了あ
        /// </summary>
        public void EndPoerformance()
        {
            StartTime = -1;
            ProgressTime = 0;
        }
    }
}