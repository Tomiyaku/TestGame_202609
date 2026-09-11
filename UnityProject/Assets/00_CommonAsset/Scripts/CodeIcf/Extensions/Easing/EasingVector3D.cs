/**
 * イージング関数について : https://easings.net/ja
 * 参考:https://qiita.com/pixelflag/items/e5ddf0160781170b671b
 */

using UnityEngine;

namespace CodeIcf.Extensions.Easing
{
    /// <summary>
    /// Vector3用イージング関数
    /// </summary>
    public static class EasingVector3D
    {
        public static Vector3 GetNowValue( EasingType _type, float _nowTime, float _totalTime, Vector3 _min, Vector3 _max )
        {
            switch( _type )
            {
                case EasingType.BounceIn: return BounceIn( _nowTime, _totalTime, _min, _max );
                case EasingType.BounceInOut: return BounceInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.BounceOut: return BounceOut( _nowTime, _totalTime, _min, _max );
                case EasingType.CircIn: return CircIn( _nowTime, _totalTime, _min, _max );
                case EasingType.CircInOut: return CircInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.CircOut: return CircOut( _nowTime, _totalTime, _min, _max );
                case EasingType.CubicIn: return CubicIn( _nowTime, _totalTime, _min, _max );
                case EasingType.CubicInOut: return CubicInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.CubicOut: return CubicOut( _nowTime, _totalTime, _min, _max );
                case EasingType.ExpIn: return ExpIn( _nowTime, _totalTime, _min, _max );
                case EasingType.ExpInOut: return ExpInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.ExpOut: return ExpOut( _nowTime, _totalTime, _min, _max );
                case EasingType.Linear: return Linear( _nowTime, _totalTime, _min, _max );
                case EasingType.QuadIn: return QuadIn( _nowTime, _totalTime, _min, _max );
                case EasingType.QuadInOut: return QuadInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.QuadOut: return QuadOut( _nowTime, _totalTime, _min, _max );
                case EasingType.QuartIn: return QuartIn( _nowTime, _totalTime, _min, _max );
                case EasingType.QuartInOut: return QuartInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.QuartOut: return QuartOut( _nowTime, _totalTime, _min, _max );
                case EasingType.QuintIn: return QuintIn( _nowTime, _totalTime, _min, _max );
                case EasingType.QuintInOut: return QuintInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.QuintOut: return QuintOut( _nowTime, _totalTime, _min, _max );
                case EasingType.SineIn: return SineIn( _nowTime, _totalTime, _min, _max );
                case EasingType.SineInOut: return SineInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.SineOut: return SineOut( _nowTime, _totalTime, _min, _max );
            }

            return _max;
        }

        public static Vector3 QuadIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return _max * _time * _time + _min;
        }

        public static Vector3 QuadOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return -_max * _time * ( _time - 2 ) + _min;
        }

        public static Vector3 QuadInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time + _min;

            _time = _time - 1;

            return -_max / 2 * ( _time * ( _time - 2 ) - 1 ) + _min;
        }

        public static Vector3 CubicIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return _max * _time * _time * _time + _min;
        }

        public static Vector3 CubicOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time = _time / _totaltime - 1;

            return _max * ( _time * _time * _time + 1 ) + _min;
        }

        public static Vector3 CubicInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time + _min;

            _time = _time - 2;

            return _max / 2 * ( _time * _time * _time + 2 ) + _min;
        }

        public static Vector3 QuartIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return _max * _time * _time * _time * _time + _min;
        }

        public static Vector3 QuartOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time = _time / _totaltime - 1;

            return -_max * ( _time * _time * _time * _time - 1 ) + _min;
        }

        public static Vector3 QuartInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time * _time + _min;

            _time = _time - 2;

            return -_max / 2 * ( _time * _time * _time * _time - 2 ) + _min;
        }

        public static Vector3 QuintIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return _max * _time * _time * _time * _time * _time + _min;
        }

        public static Vector3 QuintOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time = _time / _totaltime - 1;

            return _max * ( _time * _time * _time * _time * _time + 1 ) + _min;
        }

        public static Vector3 QuintInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time * _time * _time + _min;

            _time = _time - 2;

            return _max / 2 * ( _time * _time * _time * _time * _time + 2 ) + _min;
        }

        public static Vector3 SineIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return -_max * Mathf.Cos( _time * ( Mathf.PI * 90 / 180 ) / _totaltime ) + _max + _min;
        }

        public static Vector3 SineOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return _max * Mathf.Sin( _time * ( Mathf.PI * 90 / 180 ) / _totaltime ) + _min;
        }

        public static Vector3 SineInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return -_max / 2 * ( Mathf.Cos( _time * Mathf.PI / _totaltime ) - 1 ) + _min;
        }

        public static Vector3 ExpIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return _time == 0.0 ? _min : _max * Mathf.Pow( 2, 10 * ( _time / _totaltime - 1 ) ) + _min;
        }

        public static Vector3 ExpOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return _time == _totaltime ? _max + _min : _max * ( -Mathf.Pow( 2, -10 * _time / _totaltime ) + 1 ) + _min;
        }

        public static Vector3 ExpInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            if( _time == 0.0f ) return _min;
            if( _time == _totaltime ) return _max;

            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return _max / 2 * Mathf.Pow( 2, 10 * ( _time - 1 ) ) + _min;

            _time = _time - 1;

            return _max / 2 * ( -Mathf.Pow( 2, -10 * _time ) + 2 ) + _min;

        }

        public static Vector3 CircIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            return -_max * ( Mathf.Sqrt( 1 - _time * _time ) - 1 ) + _min;
        }

        public static Vector3 CircOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time = _time / _totaltime - 1;

            return _max * Mathf.Sqrt( 1 - _time * _time ) + _min;
        }

        public static Vector3 CircInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime / 2;

            if( _time < 1 ) return -_max / 2 * ( Mathf.Sqrt( 1 - _time * _time ) - 1 ) + _min;

            _time = _time - 2;

            return _max / 2 * ( Mathf.Sqrt( 1 - _time * _time ) + 1 ) + _min;
        }

        //public static Vector3 BackIn( float _time, float _totaltime, Vector3 _min, Vector3 _max, float _s )
        //{
        //    _max -= _min;
        //    _time /= _totaltime;

        //    return _max * _time * _time * ( ( _s + 1 ) * _time - _s ) + _min;
        //}

        //public static Vector3 BackOut( float _time, float _totaltime, Vector3 _min, Vector3 _max, float _s )
        //{
        //    _max -= _min;
        //    _time = _time / _totaltime - 1;

        //    return _max * ( _time * _time * ( ( _s + 1 ) * _time + _s ) + 1 ) + _min;
        //}

        //public static Vector3 BackInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max, float _s )
        //{
        //    _max -= _min;
        //    _s *= 1.525f;
        //    _time /= _totaltime / 2;

        //    if( _time < 1 ) return _max / 2 * ( _time * _time * ( ( _s + 1 ) * _time - _s ) ) + _min;

        //    _time = _time - 2;

        //    return _max / 2 * ( _time * _time * ( ( _s + 1 ) * _time + _s ) + 2 ) + _min;
        //}

        public static Vector3 BounceIn( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;

            return _max - BounceOut( _totaltime - _time, _totaltime, new Vector3(), _max ) + _min;
        }

        public static Vector3 BounceOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            _max -= _min;
            _time /= _totaltime;

            if( _time < 1.0f / 2.75f )
            {
                return _max * ( 7.5625f * _time * _time ) + _min;
            }
            else if( _time < 2.0f / 2.75f )
            {
                _time -= 1.5f / 2.75f;

                return _max * ( 7.5625f * _time * _time + 0.75f ) + _min;
            }
            else if( _time < 2.5f / 2.75f )
            {
                _time -= 2.25f / 2.75f;

                return _max * ( 7.5625f * _time * _time + 0.9375f ) + _min;
            }
            else
            {
                _time -= 2.625f / 2.75f;

                return _max * ( 7.5625f * _time * _time + 0.984375f ) + _min;
            }
        }

        public static Vector3 BounceInOut( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            if( _time < _totaltime / 2 )
            {
                return BounceIn( _time * 2, _totaltime, new Vector3(), _max - _min ) * 0.5f + _min;
            }
            else
            {
                return BounceOut( _time * 2 - _totaltime, _totaltime, new Vector3(), _max - _min ) * 0.5f + _min + ( _max - _min ) * 0.5f;
            }
        }

        public static Vector3 Linear( float _time, float _totaltime, Vector3 _min, Vector3 _max )
        {
            return ( ( _max - _min ) * ( _time / _totaltime ) ) + _min;
        }
    }
}