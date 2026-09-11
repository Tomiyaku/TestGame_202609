#if UNITY_SWITCH && !UNITY_EDITOR
#define ENABLE_NS_NGWORD_CHECK
#endif

using System;

using CodeIcf.Extensions;

#if ENABLE_NS_NGWORD_CHECK
using nn.ngc;
#endif

namespace CodeIcf.Switch
{
    /// <summary>
    /// Nintendo SwitchのNGワードチェックを実行するためのクラス
    /// </summary>
    public class NSNgWordChecker
    {
        /// <summary><see cref="MaskProfanityWordsInText(string, out string, PatternList)"/>の第3引数に渡すデフォルトの値</summary>
        public const PatternList DefaultPattern = PatternList.Japanese | PatternList.AmericanEnglish | PatternList.BritishEnglish;
        /// <summary>インスタンス</summary>
        private static NSNgWordChecker s_instance = null;

        /// <summary>
        /// 言語毎に不正文字列パターンのリスト
        /// </summary>
        /// <remarks>内容は nn.ngc.ProfanityFilter.PatternListと同一</remarks>
        [Flags]
        public enum PatternList
        {
            Japanese = 1 << 0,
            AmericanEnglish = 1 << 1,
            CanadianFrench = 1 << 2,
            LatinAmericanSpanish = 1 << 3,
            BritishEnglish = 1 << 4,
            French = 1 << 5,
            German = 1 << 6,
            Italian = 1 << 7,
            Spanish = 1 << 8,
            Dutch = 1 << 9,
            Korean = 1 << 10,
            SimplifiedChinese = 1 << 11,
            Portuguese = 1 << 12,
            Russian = 1 << 13,
            BrazilianPortuguese = 1 << 14,

            TraditionalChinese = 1 << 15,

            Max = 16
        }

#if ENABLE_NS_NGWORD_CHECK
        /// <summary>NG ワードフィルタリングを行うためのクラス</summary>
        private ProfanityFilter _profanityFilter = null;
#endif

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private NSNgWordChecker()
        {
#if ENABLE_NS_NGWORD_CHECK
            if( _profanityFilter == null )
            {
                _profanityFilter = new ProfanityFilter();
            }
#endif
        }

        /// <summary>
        /// インスタンス生成
        /// </summary>
        public static void CreateInstance()
        {
            if( s_instance == null )
            {
                s_instance = new NSNgWordChecker();
            }
        }

        /// <summary>
        /// インスタンス破棄
        /// </summary>
        public static void ReleaseInstance()
        {
            if( s_instance == null ) return;
#if ENABLE_NS_NGWORD_CHECK
            s_instance._profanityFilter.Dispose();
            s_instance._profanityFilter = null;
#endif
            s_instance = null;
        }

        /// <summary>
        /// 指定した文字列に対してNGワードがある場合に*に変換した文字列を返す
        /// </summary>
        /// <param name="baseStr">NGワードチェックを行う文字列</param>
        /// <param name="outMaskedStr">返還後の文字列 何らかの理由でNGワードチェックが行われなかった場合は_inTextと同じ文字列</param>
        /// <param name="pattern">NGワードチェックを行う言語リスト</param>
        /// <returns></returns>
        public static bool MaskProfanityWordsInText( string baseStr, out string outMaskedStr, PatternList pattern = DefaultPattern )
        {
            outMaskedStr = baseStr;

            if( s_instance == null ) return false;

            bool result = false;

#if ENABLE_NS_NGWORD_CHECK
            int profanityWordCount = 0;
            int patternInt = pattern.ToInt();
            ProfanityFilter.PatternList patternList = patternInt.ToEnum<ProfanityFilter.PatternList>();

            nn.Result ngwordCheckResult = s_instance._profanityFilter.MaskProfanityWordsInText( ref profanityWordCount, baseStr, out outMaskedStr, patternList );
            ngwordCheckResult.abortUnlessSuccess();

            result = ngwordCheckResult.IsSuccess();
#endif

            return result;
        }
    }
}