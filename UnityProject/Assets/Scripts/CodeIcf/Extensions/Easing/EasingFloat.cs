/**
 * イージング関数について : https://easings.net/ja
 * 参考:https://qiita.com/pixelflag/items/e5ddf0160781170b671b
 */

using UnityEngine;

namespace CodeIcf.Extensions.Easing
{
    /// <summary>
    /// float用イージング関数
    /// </summa
    public static class EasingFloat
    {
        public static float GetNowValue( EasingType _type, float _nowTime, float _totalTime, float _min, float _max )
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
                case EasingType.ElasticIn: return ElasticIn( _nowTime, _totalTime, _min, _max );
                case EasingType.ElasticInOut: return ElasticInOut( _nowTime, _totalTime, _min, _max );
                case EasingType.ElasticOut: return ElasticOut( _nowTime, _totalTime, _min, _max );
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


        public static float QuadIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return _max * _time * _time + _min;
        }

        public static float QuadOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return -_max * _time * ( _time - 2 ) + _min;
        }

        public static float QuadInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time + _min;

            _time = _time - 1;

            return -_max / 2 * ( _time * ( _time - 2 ) - 1 ) + _min;
        }

        public static float CubicIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return _max * _time * _time * _time + _min;
        }

        public static float CubicOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time = _time / _totalTime - 1;

            return _max * ( _time * _time * _time + 1 ) + _min;
        }

        public static float CubicInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time + _min;

            _time = _time - 2;

            return _max / 2 * ( _time * _time * _time + 2 ) + _min;
        }

        public static float QuartIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return _max * _time * _time * _time * _time + _min;
        }

        public static float QuartOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time = _time / _totalTime - 1;

            return -_max * ( _time * _time * _time * _time - 1 ) + _min;
        }

        public static float QuartInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time * _time + _min;

            _time = _time - 2;

            return -_max / 2 * ( _time * _time * _time * _time - 2 ) + _min;
        }

        public static float QuintIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return _max * _time * _time * _time * _time * _time + _min;
        }

        public static float QuintOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time = _time / _totalTime - 1;

            return _max * ( _time * _time * _time * _time * _time + 1 ) + _min;
        }

        public static float QuintInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return _max / 2 * _time * _time * _time * _time * _time + _min;

            _time = _time - 2;

            return _max / 2 * ( _time * _time * _time * _time * _time + 2 ) + _min;
        }

        public static float SineIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return -_max * Mathf.Cos( _time * ( Mathf.PI * 90 / 180 ) / _totalTime ) + _max + _min;
        }

        public static float SineOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return _max * Mathf.Sin( _time * ( Mathf.PI * 90 / 180 ) / _totalTime ) + _min;
        }

        public static float SineInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return -_max / 2 * ( Mathf.Cos( _time * Mathf.PI / _totalTime ) - 1 ) + _min;
        }

        public static float ExpIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return _time == 0.0 ? _min : _max * Mathf.Pow( 2, 10 * ( _time / _totalTime - 1 ) ) + _min;
        }

        public static float ExpOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return _time == _totalTime ? _max + _min : _max * ( -Mathf.Pow( 2, -10 * _time / _totalTime ) + 1 ) + _min;
        }

        public static float ExpInOut( float _time, float _totalTime, float _min, float _max )
        {
            if( _time == 0.0f ) return _min;
            if( _time == _totalTime ) return _max;

            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return _max / 2 * Mathf.Pow( 2, 10 * ( _time - 1 ) ) + _min;

            _time = _time - 1;

            return _max / 2 * ( -Mathf.Pow( 2, -10 * _time ) + 2 ) + _min;

        }

        public static float CircIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            return -_max * ( Mathf.Sqrt( 1 - _time * _time ) - 1 ) + _min;
        }

        public static float CircOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time = _time / _totalTime - 1;

            return _max * Mathf.Sqrt( 1 - _time * _time ) + _min;
        }

        public static float CircInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            if( _time < 1 ) return -_max / 2 * ( Mathf.Sqrt( 1 - _time * _time ) - 1 ) + _min;

            _time = _time - 2;

            return _max / 2 * ( Mathf.Sqrt( 1 - _time * _time ) + 1 ) + _min;
        }

        public static float ElasticIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            float s = 1.70158f;
            float p = _totalTime * 0.3f;
            float a = _max;

            if( _time == 0 ) return _min;
            if( _time == 1 ) return _min + _max;

            if( a < Mathf.Abs( _max ) )
            {
                a = _max;
                s = p / 4;
            }
            else
            {
                s = p / ( 2 * Mathf.PI ) * Mathf.Asin( _max / a );
            }

            _time = _time - 1;

            return -( a * Mathf.Pow( 2, 10 * _time ) * Mathf.Sin( ( _time * _totalTime - s ) * ( 2 * Mathf.PI ) / p ) ) + _min;
        }

        public static float ElasticOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

            float s = 1.70158f;
            float p = _totalTime * 0.3f; ;
            float a = _max;

            if( _time == 0 ) return _min;
            if( _time == 1 ) return _min + _max;

            if( a < Mathf.Abs( _max ) )
            {
                a = _max;
                s = p / 4;
            }
            else
            {
                s = p / ( 2 * Mathf.PI ) * Mathf.Asin( _max / a );
            }

            return a * Mathf.Pow( 2, -10 * _time ) * Mathf.Sin( ( _time * _totalTime - s ) * ( 2 * Mathf.PI ) / p ) + _max + _min;
        }

        public static float ElasticInOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime / 2;

            float s = 1.70158f;
            float p = _totalTime * ( 0.3f * 1.5f );
            float a = _max;

            if( _time == 0 ) return _min;
            if( _time == 2 ) return _min + _max;

            if( a < Mathf.Abs( _max ) )
            {
                a = _max;
                s = p / 4;
            }
            else
            {
                s = p / ( 2 * Mathf.PI ) * Mathf.Asin( _max / a );
            }

            if( _time < 1 )
            {
                return -0.5f * ( a * Mathf.Pow( 2, 10 * ( _time -= 1 ) ) * Mathf.Sin( ( _time * _totalTime - s ) * ( 2 * Mathf.PI ) / p ) ) + _min;
            }

            _time = _time - 1;

            return a * Mathf.Pow( 2, -10 * _time ) * Mathf.Sin( ( _time * _totalTime - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + _max + _min;
        }

        //public static float BackIn( float _time, float _totalTime, float _min, float _max, float _s)
        //{
        //    _max -= _min;
        //    _time /= _totalTime;

        //    return _max * _time * _time * ( ( _s + 1 ) * _time - _s ) + _min;
        //}

        //public static float BackOut( float _time, float _totalTime, float _min, float _max, float _s )
        //{
        //    _max -= _min;
        //    _time = _time / _totalTime - 1;

        //    return _max * ( _time * _time * ( ( _s + 1 ) * _time + _s ) + 1 ) + _min;
        //}

        //public static float BackInOut( float _time, float _totalTime, float _min, float _max, float _s )
        //{
        //    _max -= _min;
        //    _s *= 1.525f;
        //    _time /= _totalTime / 2;

        //    if( _time < 1 ) return _max / 2 * ( _time * _time * ( ( _s + 1 ) * _time - _s ) ) + _min;

        //    _time = _time - 2;

        //    return _max / 2 * ( _time * _time * ( ( _s + 1 ) * _time + _s ) + 2 ) + _min;
        //}

        public static float BounceIn( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;

            return _max - BounceOut( _totalTime - _time, _totalTime, 0, _max ) + _min;
        }

        public static float BounceOut( float _time, float _totalTime, float _min, float _max )
        {
            _max -= _min;
            _time /= _totalTime;

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

        public static float BounceInOut( float _time, float _totalTime, float _min, float _max )
        {
            if( _time < _totalTime / 2 )
            {
                return BounceIn( _time * 2, _totalTime, 0, _max - _min ) * 0.5f + _min;
            }
            else
            {
                return BounceOut( _time * 2 - _totalTime, _totalTime, 0, _max - _min ) * 0.5f + _min + ( _max - _min ) * 0.5f;
            }
        }

        public static float Linear( float _time, float _totalTime, float _min, float _max )
        {
            return ( _max - _min ) * _time / _totalTime + _min;
        }
    }
}