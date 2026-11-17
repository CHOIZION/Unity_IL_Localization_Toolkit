// ============================================================================
// Unity IL Localization Toolkit
// Module: KoreanParticleResolver.cs
// Description: Dynamic Korean particle (조사) conjugation calculator based on
//              Unicode Hangul syllable decomposition.
// ============================================================================

using System;
using System.Text.RegularExpressions;

namespace UnityLocalizationToolkit.Core
{
    /// <summary>
    /// 한국어 유니코드 음절 분해 공식에 기반한 실시간 조사 자동 보정 엔진입니다.
    /// 앞 단어의 종성(받침) 유무 및 'ㄹ' 받침 특수 규칙을 계산하여 어색하지 않은 한국어 조사를 합성합니다.
    /// </summary>
    public static class KoreanParticleResolver
    {
        // 정규식: 앞 단어 + 닫는 태그(</color>, </link>, ] 등) + 괄호형/이중형 조사 패턴
        private static readonly Regex _particlePattern = new Regex(
            @"(?<word>[가-힣0-9A-Za-z]+)(?<tags>(?:</[^>]+>|\])*)(?:\((?<p1>이|을|은|으|과|와)\)(?<p2>가|를|는|로|와|과)|(?<alt1>이|가|을|를|은|는|과|와|으로|로)\((?<alt2>가|이|를|을|는|은|와|과|로|으로)\))",
            RegexOptions.Compiled);

        /// <summary>
        /// 텍스트 전체에서 "단어(이)가", "단어(을)를", "단어(은)는", "단어(과)와", "단어(으)로" 등의 패턴을 탐색하여
        /// 올바른 단일 조사로 치환합니다. 서식 태그(HTML/TMP 태그)가 단어와 조사 사이에 끼어 있어도 정상 판별합니다.
        /// </summary>
        public static string ResolveAllParticles(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return _particlePattern.Replace(input, m =>
            {
                string word = m.Groups["word"].Value;
                string tags = m.Groups["tags"].Value;

                // 목표 조사 유형 판별
                string p1 = m.Groups["p1"].Success ? m.Groups["p1"].Value : m.Groups["alt1"].Value;
                string p2 = m.Groups["p2"].Success ? m.Groups["p2"].Value : m.Groups["alt2"].Value;

                string particleType = p1 + "/" + p2;
                string resolved = SelectParticle(word, particleType);

                return word + tags + resolved;
            });
        }

        /// <summary>
        /// 주어진 단어 뒤에 올 올바른 조사를 계산하여 반환합니다.
        /// </summary>
        /// <param name="word">앞 단어 (예: "스펜서", "두드림", "칼")</param>
        /// <param name="particle">부착하고자 하는 조사 또는 조사쌍 (예: "이/가", "을/를", "은/는", "과/와", "으로/로")</param>
        /// <returns>올바르게 선택된 단일 조사</returns>
        public static string ResolveParticle(string word, string particle)
        {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(particle)) return particle;

            // 단어 뒤쪽에 붙은 서식 태그 제거 후 마지막 글자 추출
            string clean = Regex.Replace(word, @"<[^>]+>", "").Trim();
            if (string.IsNullOrEmpty(clean)) return particle;

            char lastChar = clean[clean.Length - 1];

            // 1. 한글 음절 판별 (가: 0xAC00 ~ 힣: 0xD7A3)
            if (lastChar >= 0xAC00 && lastChar <= 0xD7A3)
            {
                int syllableIndex = lastChar - 0xAC00;
                int coda = syllableIndex % 28; // 종성(받침) 인덱스: 0이면 받침 없음, 1~27이면 받침 있음
                bool hasCoda = (coda != 0);
                bool isRieul = (coda == 8); // 'ㄹ' 받침: 인덱스 8

                return SelectByCoda(particle, hasCoda, isRieul);
            }

            // 2. 숫자 판별 (0~9의 한국어 독음 기준 받침 유무)
            if (char.IsDigit(lastChar))
            {
                // 0(영), 1(일), 3(삼), 6(육), 7(칠), 8(팔): 받침 있음
                // 2(이), 4(사), 5(오), 9(구): 받침 없음
                bool numHasCoda = (lastChar == '0' || lastChar == '1' || lastChar == '3' ||
                                   lastChar == '6' || lastChar == '7' || lastChar == '8');
                bool numIsRieul = (lastChar == '1' || lastChar == '7' || lastChar == '8'); // 일, 칠, 팔

                return SelectByCoda(particle, numHasCoda, numIsRieul);
            }

            // 한글이나 숫자가 아닌 경우(영문 고유명사 등) 기본형 반환
            return NormalizeDefault(particle);
        }

        private static string SelectByCoda(string particle, bool hasCoda, bool isRieul)
        {
            // 이 / 가
            if (particle.Contains("이") && particle.Contains("가"))
                return hasCoda ? "이" : "가";
            if (particle == "이" || particle == "가")
                return hasCoda ? "이" : "가";

            // 은 / 는
            if (particle.Contains("은") && particle.Contains("는"))
                return hasCoda ? "은" : "는";
            if (particle == "은" || particle == "는")
                return hasCoda ? "은" : "는";

            // 을 / 를
            if (particle.Contains("을") && particle.Contains("를"))
                return hasCoda ? "을" : "를";
            if (particle == "을" || particle == "를")
                return hasCoda ? "을" : "를";

            // 과 / 와
            if (particle.Contains("과") && particle.Contains("와"))
                return hasCoda ? "과" : "와";
            if (particle == "과" || particle == "와")
                return hasCoda ? "과" : "와";

            // 으로 / 로 ('ㄹ' 받침인 경우 받침이 있어도 "로"를 사용함: 예: "칼로", "물로", "서울로")
            if (particle.Contains("으로") || particle.Contains("로"))
            {
                if (hasCoda && !isRieul) return "으로";
                return "로";
            }

            return particle;
        }

        private static string SelectParticle(string word, string particlePair)
        {
            return ResolveParticle(word, particlePair);
        }

        private static string NormalizeDefault(string particle)
        {
            if (particle.Contains("/"))
            {
                var parts = particle.Split('/');
                return parts[0];
            }
            return particle.Replace("(", "").Replace(")", "");
        }
    }
}
