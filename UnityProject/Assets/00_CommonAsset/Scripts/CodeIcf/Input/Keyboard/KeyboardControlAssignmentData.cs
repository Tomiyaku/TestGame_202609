using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeIcf.Input.Keyboard
{
    /// <summary>
    /// キーボードの入力アサイン情報
    /// </summary>
    [CreateAssetMenu( fileName = "KeyboardControlAssignmentData", menuName = "ScriptableObjects/Create KeyboardControlAssignmentData" )]
    public class KeyboardControlAssignmentData : ScriptableObject
    {
        /// <summary>OK</summary>
        public Key[] OKKeys;
        /// <summary>キャンセル</summary>
        public Key[] CancelKeys;
        /// <summary>メニュー</summary>
        public Key[] MenuKeys;
        /// <summary>左スティック上</summary>
        public Key[] LeftArrowUpKeys;
        /// <summary>左スティック下</summary>
        public Key[] LeftArrowDownKeys;
        /// <summary>左スティック左</summary>
        public Key[] LeftArrowLeftKeys;
        /// <summary>左スティック右</summary>
        public Key[] LeftArrowRightKeys;
        /// <summary>右スティック上</summary>
        public Key[] RightArrowUpKeys;
        /// <summary>右スティック下</summary>
        public Key[] RighArrowDownKeys;
        /// <summary>右スティック左</summary>
        public Key[] RighArrowLeftKeys;
        /// <summary>右スティック右</summary>
        public Key[] RighArrowRightKeys;
        /// <summary>ジャンプ</summary>
        public Key[] JumpKeys;
        /// <summary>パンチ・キック</summary>
        public Key[] PunchKeys;
        /// <summary>掴む・投げる</summary>
        public Key[] CatchKeys;
        /// <summary>特殊能力</summary>
        public Key[] SpecialKeys;
        /// <summary>エモーション 1</summary>
        public Key[] Emote_1Keys;
        /// <summary>エモーション 2</summary>
        public Key[] Emote_2Keys;
        /// <summary>エモーション 3</summary>
        public Key[] Emote_3Keys;
        /// <summary>エモーション 4</summary>
        public Key[] Emote_4Keys;
    }
}