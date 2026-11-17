// ============================================================================
// Unity IL Localization Toolkit
// Module: TagAndSpriteParser.cs
// Description: Dynamic rich-text & sprite tag phrase parser and converter.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityLocalizationToolkit.Core
{
    /// <summary>
    /// 복합 인라인 스프라이트 태그(<sprite=...>) 및 대괄호 용어 링크([[Tag]])를 실시간으로
    /// 보존·번역하고, 영어 접속사(" or ", " and ")를 자연스럽게 치환하는 구문 파서입니다.
    /// </summary>
    public class TagAndSpriteParser
    {
        private static readonly Regex _aspectSpriteRegex = new Regex(
            @"(<sprite=[^>]+>)?([A-Za-z0-9_-]+)", RegexOptions.Compiled);

        private static readonly Regex _doubleBracketRegex = new Regex(
            @"\[\[\s*(?<key>[^\[\]]+?)\s*\]\](?<space>\s*)(?<particle>\([이을은으과]\)[가를는로와]|[이가을를은는과와]\([가이을를은는와과]\)|[가-힣]+)?",
            RegexOptions.Compiled);

        private readonly Func<string, string> _termLookup;

        /// <summary>
        /// 용어 검색 대리자를 전달받아 파서를 초기화합니다.
        /// </summary>
        /// <param name="termLookup">영어 키를 한국어로 변환하는 검색 함수</param>
        public TagAndSpriteParser(Func<string, string> termLookup)
        {
            _termLookup = termLookup ?? (k => k);
        }

        /// <summary>
        /// "<sprite=1>Obscure, <sprite=2>Bewitch or <sprite=3>Trespass" 형태의 복합 스프라이트 구문을 파싱하여
        /// "<sprite=1>은폐, <sprite=2>홀림 또는 <sprite=3>침범" 형태로 번역합니다.
        /// </summary>
        public string ParseAndTranslateAspectPhrase(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return phrase;

            // 1. 접속사 선행 치환
            string res = phrase.Replace(" or ", " 또는 ").Replace(" and ", " 및 ");

            // 2. 각 스프라이트 뒤의 단어 토큰 매칭 및 번역
            res = _aspectSpriteRegex.Replace(res, m =>
            {
                string sprite = m.Groups[1].Value;
                string name = m.Groups[2].Value.Trim();

                if (string.IsNullOrEmpty(name)) return m.Value;
                if (name == "또는" || name == "및") return m.Value;

                string translated = _termLookup(name);
                if (!string.IsNullOrEmpty(translated) && translated != name)
                {
                    return sprite + translated;
                }

                return m.Value;
            });

            return res;
        }

        /// <summary>
        /// "[[Tag]]" 형태의 위키 링크 태그를 TextMeshPro의 <link="Tag">한글</link> 및 색상 태그로 변환하며,
        /// 뒤에 조사가 따라올 경우 종성 계산을 통해 자동으로 조사를 교정합니다.
        /// </summary>
        public string ParseAndExpandBracketLinks(string text, string linkColor = "#D24B00")
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf("[[", StringComparison.Ordinal) < 0)
                return text;

            return _doubleBracketRegex.Replace(text, m =>
            {
                string key = m.Groups["key"].Value.Trim();
                string label = _termLookup(key);
                string space = m.Groups["space"].Success ? m.Groups["space"].Value : "";

                string linkHtml = "<color=" + linkColor + "><link=\"" + key + "\">" + label + "</link></color>";

                if (m.Groups["particle"].Success && m.Groups["particle"].Length > 0)
                {
                    string rawParticle = m.Groups["particle"].Value;
                    string resolved = KoreanParticleResolver.ResolveParticle(label, rawParticle);
                    return linkHtml + resolved;
                }

                return linkHtml + space;
            });
        }
    }
}
