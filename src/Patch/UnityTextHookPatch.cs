// ============================================================================
// Unity IL Localization Toolkit
// Module: UnityTextHookPatch.cs
// Description: Standalone C# runtime hook engine injected into Unity games.
// ============================================================================

using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityLocalizationToolkit.Core;

namespace UnityLocalizationToolkit.Patch
{
    /// <summary>
    /// 게임 실행 중 호출되어 실시간 번역, 폰트 폴백 적용, 구문 파싱을 지휘하는 중앙 패치 클래스입니다.
    /// IL 인젝터(AssemblyPatcher)에 의해 게임의 텍스트 렌더링 경로에서 직접 호출됩니다.
    /// </summary>
    public static class UnityTextHookPatch
    {
        private static bool _initialized = false;
        private static TranslationEngine _engine;
        private static TagAndSpriteParser _parser;

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                _engine = new TranslationEngine();
                _parser = new TagAndSpriteParser(k => _engine.Translate(k));

                // 번역 파일 탐색 (게임 데이터 폴더)
                string dataPath = Application.dataPath;
                string transPath = Path.Combine(dataPath, "translations.txt");
                if (!File.Exists(transPath))
                {
                    transPath = Path.Combine(dataPath, "Korean", "translations.txt");
                }

                if (File.Exists(transPath))
                {
                    _engine.LoadFromFile(transPath);
                    Debug.Log(string.Format("[UnityTextHookPatch] 번역 데이터 로드 완료 ({0}개 항목)", _engine.Count));
                }

                // 폰트 폴백 초기화
                string fontPath = Path.Combine(dataPath, "font.ttf");
                if (!File.Exists(fontPath))
                {
                    fontPath = Path.Combine(dataPath, "Korean", "font.ttf");
                }
                FontFallbackManager.Initialize(fontPath);

                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[UnityTextHookPatch] 초기화 중 예외 발생: " + ex.Message);
            }
        }

        /// <summary>
        /// [IL 훅 진입점] 게임의 TMP_Text.set_text, SetText 등 텍스트 지정 메서드 호출 시 가로챕니다.
        /// </summary>
        public static string ProcessText(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return rawText;
            if (!_initialized) Initialize();

            // 1. 단순 사전 번역 시도
            string translated = _engine.Translate(rawText);

            // 2. 대괄호 위키 링크 및 조사 자동 교정
            translated = _parser.ParseAndExpandBracketLinks(translated);

            // 3. 인라인 조사 최종 합성
            translated = KoreanParticleResolver.ResolveAllParticles(translated);

            return translated;
        }

        /// <summary>
        /// [IL 훅 진입점] TextMeshProUGUI.Awake, OnEnable 시점에 호출되어 폰트 폴백을 주입합니다.
        /// </summary>
        public static void EnsureTMPText(TMP_Text tmpText)
        {
            if (tmpText == null) return;
            if (!_initialized) Initialize();

            FontFallbackManager.EnsureFontFallback(tmpText);
        }

        /// <summary>
        /// [IL 훅 진입점] 게임의 Loc.Format(template, args) 또는 String.Format 호출 시 가로챕니다.
        /// </summary>
        public static void ProcessFormatArgs(ref string template, string[] args)
        {
            if (!_initialized) Initialize();

            if (!string.IsNullOrEmpty(template))
            {
                template = _engine.Translate(template);
            }

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (string.IsNullOrEmpty(arg)) continue;

                    // 복합 스프라이트 구문인 경우 파서 호출
                    if (arg.IndexOf("<sprite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        arg.IndexOf(" or ", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        args[i] = _parser.ParseAndTranslateAspectPhrase(arg);
                    }
                    else
                    {
                        args[i] = _engine.Translate(arg);
                    }
                }
            }
        }
    }
}
