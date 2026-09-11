using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Jobs;

namespace CodeIcf.Extensions
{
    /// <summary>
    /// String型拡張クラス
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>ダブルクォーテーション</summary>
        private const char SINGLE_DOUBLE_QUOTATION = '"';
        /// <summary>２重のダブルクォーテーション</summary>
        private const string DOUBLE_DOUBLE_QUOTATION = "\"\"";
        /// <summary>3重のダブルクォーテーション</summary>
        private const string TRIPLE_DOUBLE_QUOTATION = "\"\"\"";
        /// <summary>カンマ</summary>
        private const char COMMA = ',';
        /// <summary>ダブルクォーテーションとカンマ/// </summary>
        private const string SINGLE_DOUBLE_QUOTATION_AND_COMMA = "\",";

        /// <summary>
        /// csvファイルを改行とカンマで分割してString配列に変換する
        /// </summary>
        /// <remarks>
        /// ダブルクォーテーションで囲まれた改行、カンマは分割しないように対応<br></br>
        /// ダブルクォーテーションで囲まれた２重のダブルクォーテーションは文字列中に１つのダブルクォーテーションに変換
        /// </remarks>
        /// <returns>改行とカンマで分割したString配列</returns>
        /// <param name="_self">CSV形式の文字列</param>
        /// <param name="_isFirstDataRemove">１件目の情報を無視するかのフラグ</param>
        public static string[][] SplitCsv( this string _self, bool _isFirstDataRemove = false )
        {
            List<string[]> result = new List<string[]>();
            List<string> lineStrList = new List<string>();

            //改行毎に分割
            using( StringReader reader = new StringReader( _self ) )
            {
                if( _isFirstDataRemove ) reader.ReadLine();

                while( reader.Peek() != -1 )
                {
                    lineStrList.Add( reader.ReadLine() );
                }
            }

            CombineStringInDoubleQuotes( ref lineStrList, Environment.NewLine );

            ///カンマ区切りで分割
            foreach( string lineStr in lineStrList )
            {
                result.Add( lineStr.Split( COMMA ) );
            }

            lineStrList.Clear();

            //ダブルクォーテーション中のカンマを文字列とみなして結合
            for( int i = 0; i < result.Count; i++ )
            {
                lineStrList.AddRange( result[ i ] );

                CombineStringInDoubleQuotes( ref lineStrList, COMMA.ToString() );

                result[ i ] = lineStrList.ToArray();
                lineStrList.Clear();
            }

            for( int i = 0; i < result.Count; i++ )
            {
                for( int j = 0; j < result[ i ].Length; j++ )
                {
                    //文字列の先頭と末尾のダブルクォーテーションを削除
                    int start = result[ i ][ j ].StartsWith( SINGLE_DOUBLE_QUOTATION ) ? 1 : 0;
                    int end = result[ i ][ j ].EndsWith( SINGLE_DOUBLE_QUOTATION ) ? 1 : 0;

                    result[ i ][ j ] = result[ i ][ j ].Substring( start, result[ i ][ j ].Length - start - end );

                    //２重のダブルクォーテーションを1つのダブルクォーテーションに変換
                    result[ i ][ j ] = result[ i ][ j ].Replace( DOUBLE_DOUBLE_QUOTATION, SINGLE_DOUBLE_QUOTATION.ToString() );
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 指定の文字列リストでダブルクォーテーション中で分割されている文字列を結合する
        /// </summary>
        /// <param name="_refTargets">結合対象の文字列リスト</param>
        /// <param name="_separeta">結合時に間に加える文字列</param>
        private static void CombineStringInDoubleQuotes( ref List<string> _refTargets, string _separeta )
        {
            //ダブルクォーテーション中フラグ
            bool isDqFlag = false;

            for( int i = 0; i < _refTargets.Count; i++ )
            {
                int dpIndex = _refTargets[ i ].IndexOf( SINGLE_DOUBLE_QUOTATION );

                if( isDqFlag )
                {//ダブルクォーテーション中であれば１つ前の行と結合し削除
                    _refTargets[ i - 1 ] += _separeta + _refTargets[ i ];

                    if( _refTargets[ i ].EndsWith( SINGLE_DOUBLE_QUOTATION ) )
                    {//末尾にダブルクォーテーションがあり、ダブルクォーテーションが単独か3つ連続の場合はフラグを下す
                        if( _refTargets[ i ].EndsWith( TRIPLE_DOUBLE_QUOTATION ) ) isDqFlag = false;
                        else if( !_refTargets[ i ].EndsWith( DOUBLE_DOUBLE_QUOTATION ) ) isDqFlag = false;
                    }
                    else if( _refTargets[ i ].EndsWith( COMMA ) )
                    {//末尾がカンマの場合、その手前にダブルクォーテーションがある場合にはフラグを下す
                        for( int j = _refTargets[ i ].Length - 1; j >= 0; j-- )
                        {
                            if( _refTargets[ i ][ j ] == COMMA ) continue;
                            if( _refTargets[ i ][ j ] == SINGLE_DOUBLE_QUOTATION ) isDqFlag = false;

                            break;
                        }
                    }

                    _refTargets.RemoveAt( i );
                    i--;
                }
                else
                {
                    //文字列にダブルクォーテーション無し
                    if( dpIndex < 0 ) continue;

                    isDqFlag = true;

                    //文字列中にダブルクォーテーションで囲まれていない場合はフラグを立てる
                    while( dpIndex < _refTargets[ i ].Length )
                    {
                        dpIndex = _refTargets[ i ].IndexOf( SINGLE_DOUBLE_QUOTATION, dpIndex + 1 );

                        if( dpIndex < 0 )
                        {//次の文字列を結合させるのでループを抜ける
                            break;
                        }
                        else
                        {
                            if( dpIndex < _refTargets[ i ].Length - 1 && _refTargets[ i ][ dpIndex + 1 ] == SINGLE_DOUBLE_QUOTATION )
                            {//２重のダブルクォーテーションはスキップ
                                dpIndex++;
                            }
                            else
                            {//ダブルクォーテーションが閉じている or 新規にに見つかったのでフラグを反転
                                isDqFlag = !isDqFlag;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 文字列を改行毎に分割してString配列に変換する
        /// </summary>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static string[] SplitNewLine( this string _self )
        {
            List<string> result = new List<string>();

            using( StringReader reader = new StringReader( _self ) )
            {
                while( reader.Peek() != -1 )
                {
                    result.Add( reader.ReadLine() );
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 文字列をスネークケースにしてすべて大文字で返す
        /// </summary>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static string ToUpperSnakeCase( this string _self )
        {
            Regex regex = new Regex( "[a-z][A-Z]" );

            return regex.Replace( _self, s => $"{s.Groups[ 0 ].Value[ 0 ]}_{s.Groups[ 0 ].Value[ 1 ]}" ).ToUpper();
        }

        /// <summary>
        /// 文字列をスネークケースにしてすべて小文字で返す
        /// </summary>
        /// <param name="_self"></param>
        /// <returns></returns>
        public static string ToLowerSnakeCase( this string _self )
        {
            Regex regex = new Regex( "[a-z][A-Z]" );

            return regex.Replace( _self, s => $"{s.Groups[ 0 ].Value[ 0 ]}_{s.Groups[ 0 ].Value[ 1 ]}" ).ToLower();
        }
    }
}