using System;
using UnityEngine;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// ビット演算拡張処理
    /// </summary>
    public static class LogicalExtensions
    {
        #region BYTE
        /// <summary>
        /// ビットフラグを反転する
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <returns>元の値からビットを反転した値</returns>
        public static byte FLip( this byte _self )
        {
            return ( byte )~_self;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this byte _self, byte _logic )
        {
            return ( _self & _logic ) == _logic;
        }
        #endregion //BYTE

        #region SHORT
        /// <summary>
        /// ビットフラグを反転する
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <returns>元の値からビットを反転した値</returns>
        public static short FLip( this short _self )
        {
            return ( short )~_self;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this short _self, byte _logic )
        {
            return ( _self & _logic ) == _logic;
        }
        #endregion //SHORT

        #region USHORT
        /// <summary>
        /// ビットフラグを反転する
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <returns>元の値からビットを反転した値</returns>
        public static ushort FLip( this ushort _self )
        {
            return ( ushort )~_self;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this ushort _self, byte _logic )
        {
            return ( _self & _logic ) == _logic;
        }

        #endregion //USHORT

        #region INT
        /// <summary>
        /// ビットフラグを反転する
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <returns>元の値からビットを反転した値</returns>
        public static ushort FLip( this int _self )
        {
            return ( ushort )~_self;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this int _self, byte _logic )
        {
            return ( _self & _logic ) == _logic;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this int _self, int _logic )
        {
            return ( _self & _logic ) == _logic;
        }
        #endregion //INT

        #region UINT
        /// <summary>
        /// ビットフラグを反転する
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <returns>元の値からビットを反転した値</returns>
        public static ushort FLip( this uint _self )
        {
            return ( ushort )~_self;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this uint _self, byte _logic )
        {
            return ( _self & _logic ) == _logic;
        }

        /// <summary>
        /// 指定の値と同じビットフラグが立っているか
        /// </summary>
        /// <param name="_self">自身の値</param>
        /// <param name="_logic">比較対象の値</param>
        /// <returns>比較対象と同じビットフラグが立っている値であればtrue</returns>
        public static bool EqualLogicAnd( this uint _self, uint _logic )
        {
            return ( _self & _logic ) == _logic;
        }
        #endregion //UINT
    }
}