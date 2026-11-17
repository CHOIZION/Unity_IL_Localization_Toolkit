// ============================================================================
// Unity IL Localization Toolkit
// Module: TranslationEngine.cs
// Description: High-performance $O(1)$ in-memory string translation dictionary.
// ============================================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace UnityLocalizationToolkit.Core
{
    /// <summary>
    /// 수만 개의 번역 항목을 메모리상에 상주시켜 1/60초(1프레임) 이내에 지연 없이
    /// 문자열을 조회·치환하는 초고속 인메모리 번역 딕셔너리 엔진입니다.
    /// </summary>
    public class TranslationEngine
    {
        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _translationsIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int Count { get { return _translations.Count; } }

        /// <summary>
        /// 탭 구분자(TSV: Key \t Value) 형식의 번역 텍스트 파일을 로드합니다.
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                    int tabIdx = line.IndexOf('\t');
                    if (tabIdx <= 0) continue;

                    string key = line.Substring(0, tabIdx).Trim();
                    string val = line.Substring(tabIdx + 1).Trim();

                    // 줄바꿈 이스케이프 복원
                    key = key.Replace("\\n", "\n").Replace("\\r", "\r");
                    val = val.Replace("\\n", "\n").Replace("\\r", "\r");

                    Add(key, val);

                    // 따옴표 변형(’ vs ') 자동 등록
                    if (key.Contains("'"))
                    {
                        Add(key.Replace("'", "’"), val);
                    }
                    else if (key.Contains("’"))
                    {
                        Add(key.Replace("’", "'"), val);
                    }
                }
            }
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;

            _translations[key] = value;
            if (!_translationsIgnoreCase.ContainsKey(key))
            {
                _translationsIgnoreCase[key] = value;
            }
        }

        /// <summary>
        /// 원문 문자열을 조회하여 번역된 문자열을 반환합니다.
        /// 대소문자 일치 -> 대소문자 무시 순으로 검색하며, 일치하는 항목이 없을 시 원문을 반환합니다.
        /// </summary>
        public string Translate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result;
            // 1. 정확한 일치 (Ordinal)
            if (_translations.TryGetValue(text, out result))
            {
                return result;
            }

            // 2. 대소문자 무시 일치 (OrdinalIgnoreCase)
            if (_translationsIgnoreCase.TryGetValue(text, out result))
            {
                return result;
            }

            // 3. 앞뒤 공백 제거 후 일치 검사
            string trimmed = text.Trim();
            if (trimmed != text && _translations.TryGetValue(trimmed, out result))
            {
                return result;
            }

            return text;
        }

        public bool TryGet(string key, out string value)
        {
            return _translations.TryGetValue(key, out value) || _translationsIgnoreCase.TryGetValue(key, out value);
        }
    }
}
