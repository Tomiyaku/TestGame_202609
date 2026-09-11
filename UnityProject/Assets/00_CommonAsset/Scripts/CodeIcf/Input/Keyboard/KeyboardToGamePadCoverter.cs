using UnityEngine.InputSystem;

using CodeIcf.Extensions;

namespace CodeIcf.Input.Keyboard
{
    public static class KeyboardToGamePadCoverter
    {
        public static KeyboardKeyState[] ConvertKeyboatdInputToGamePadKeyState( KeyboardControlAssignmentData _assignmentData )
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            if( keyboard == null ) return null;

            KeyboardKeyState[] result = new KeyboardKeyState[ KeyboardDefine.KEY_STATE_COUNT ];

            result[ KeyboardDefine.KEY_STATES_INDEX_OK ] = GetKeyState( _assignmentData.OKKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_CANCEL ] = GetKeyState( _assignmentData.CancelKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_MENU ] = GetKeyState( _assignmentData.MenuKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_UP ] = GetKeyState( _assignmentData.LeftArrowUpKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_DOWN ] = GetKeyState( _assignmentData.LeftArrowDownKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_LEFT ] = GetKeyState( _assignmentData.LeftArrowLeftKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_LEFT_ARROW_RIGHT ] = GetKeyState( _assignmentData.LeftArrowRightKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_UP ] = GetKeyState( _assignmentData.RightArrowUpKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_DOWN ] = GetKeyState( _assignmentData.RighArrowDownKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_LEFT ] = GetKeyState( _assignmentData.RighArrowLeftKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_RIGHT_ARROW_RIGHT ] = GetKeyState( _assignmentData.RighArrowRightKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_JUMP ] = GetKeyState( _assignmentData.JumpKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_PUNCH ] = GetKeyState( _assignmentData.PunchKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_CATCH ] = GetKeyState( _assignmentData.CatchKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_SPACIAL ] = GetKeyState( _assignmentData.SpecialKeys );
            result[ KeyboardDefine.KEY_STATES_INDEX_EMOTE_1 ] = GetKeyState( _assignmentData.Emote_1Keys );
            result[ KeyboardDefine.KEY_STATES_INDEX_EMOTE_2 ] = GetKeyState( _assignmentData.Emote_2Keys );
            result[ KeyboardDefine.KEY_STATES_INDEX_EMOTE_3 ] = GetKeyState( _assignmentData.Emote_3Keys );
            result[ KeyboardDefine.KEY_STATES_INDEX_EMOTE_4 ] = GetKeyState( _assignmentData.Emote_4Keys );

            return result;
        }

        /// <summary>
        /// キー状態の取得
        /// </summary>
        /// <param name="_keyList"></param>
        /// <returns></returns>
        private static KeyboardKeyState GetKeyState( params Key[] _keyList )
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            KeyboardKeyState result = KeyboardKeyState.None;

            foreach( Key key in _keyList )
            {
                if( keyboard[ key ].isPressed )
                {
                    if( keyboard[ key ].wasPressedThisFrame ) result = KeyboardKeyState.Down;
                    else return KeyboardKeyState.Hold;
                }
                else if( keyboard[ key ].wasReleasedThisFrame && result == KeyboardKeyState.None )
                {
                    result = KeyboardKeyState.Up;
                }
            }

            return result;
        }

        public static KeyboardKeyState GetAnyKeyState()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            if( keyboard == null ) return KeyboardKeyState.None;

            KeyboardKeyState result = KeyboardKeyState.None;

            for( int i = 1; i < Key.F1.ToInt(); i++ )
            {
                Key key = i.ToEnum<Key>();

                if( keyboard[ key ].isPressed )
                {
                    if( keyboard[ key ].wasPressedThisFrame ) result = KeyboardKeyState.Down;
                    else return KeyboardKeyState.Hold;
                }
                else if( keyboard[ key ].wasReleasedThisFrame && result == KeyboardKeyState.None )
                {
                    result = KeyboardKeyState.Up;
                }
            }

            return result;
        }
    }
}