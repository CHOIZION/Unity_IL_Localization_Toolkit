// ============================================================================
// Unity IL Localization Toolkit
// Module: InteractiveDemo.cs
// Description: Interactive CLI showcase & benchmark for the 5 core technologies.
// ============================================================================

using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityLocalizationToolkit.Core;

namespace UnityLocalizationToolkit.Demo
{
    public class InteractiveDemo
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                PrintBanner();
                Console.WriteLine("  [1] 기술 1: IL 바이트코드 인젝션 원리 (\"잉크젯 노즐 가로채기\")");
                Console.WriteLine("  [2] 기술 2: O(1) 초고속 인메모리 딕셔너리 벤치마크");
                Console.WriteLine("  [3] 기술 3: 한글 유니코드 음절 분해 조사 수학 계산기");
                Console.WriteLine("  [4] 기술 4: 복합 스프라이트 및 문법 파서 (Tag & Sprite Parser)");
                Console.WriteLine("  [5] 기술 5: TextMeshPro 런타임 폰트 폴백(Fallback Font) 주입 원리");
                Console.WriteLine("  [6] 라이브 대화형 번역 파이프라인 샌드박스 (직접 입력 테스트)");
                Console.WriteLine("  [7] 전체 종합 테스트 및 검증 스위트 실행");
                Console.WriteLine("  [0] 프로그램 종료");
                Console.WriteLine("================================================================================");
                Console.Write("  선택할 번호를 입력하세요 (0-7): ");

                string choice = Console.ReadLine();
                if (choice == null || choice.Trim() == "0")
                {
                    Console.WriteLine("\n  프로그램을 종료합니다. 감사합니다!\n");
                    break;
                }

                Console.WriteLine();
                switch (choice.Trim())
                {
                    case "1":
                        ShowPillar1_ILInjection();
                        break;
                    case "2":
                        ShowPillar2_O1Benchmark();
                        break;
                    case "3":
                        ShowPillar3_KoreanParticleMath();
                        break;
                    case "4":
                        ShowPillar4_SpriteAndGrammarParser();
                        break;
                    case "5":
                        ShowPillar5_FontFallbackArchitecture();
                        break;
                    case "6":
                        RunInteractiveSandbox();
                        break;
                    case "7":
                        RunFullTestSuite();
                        break;
                    default:
                        Console.WriteLine("  [알림] 올바른 번호를 입력해 주세요.");
                        break;
                }

                Console.WriteLine("\n  엔터(Enter) 키를 누르면 메인 메뉴로 돌아갑니다...");
                Console.ReadLine();
            }
        }

        private static void PrintBanner()
        {
            try { Console.Clear(); } catch { }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine("          Unity IL Localization Toolkit (UIL-Toolkit) Showcase                  ");
            Console.WriteLine("        유니티 실시간 번역 및 바이트코드 후킹 5대 핵심 기술 데모                ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
        }

        private static void ShowPillar1_ILInjection()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [기술 1] IL 바이트코드 인젝션: \"잉크젯 노즐 바로 앞에서 가로채기\" ===");
            Console.ResetColor();
            Console.WriteLine(@"
[개념 설명]
유니티(Unity) 게임은 C# 언어로 작성되어 .dll 파일(Unity.TextMeshPro.dll 등) 형태로 컴파일됩니다.
우리의 인젝터(AssemblyPatcher)는 Mono.Cecil 라이브러리를 사용해 컴파일된 DLL 파일들의 기계어
(IL: Intermediate Language) 코드를 열고 수술을 진행합니다.

게임 엔진이 화면에 글자를 띄우려고 할 때 호출하는 메서드(TMP_Text::set_text)의 맨 첫 줄에
'우리가 만든 번역 함수(ProcessText)를 먼저 거쳐가라'는 IL 명령어를 강제로 끼워 넣습니다.
");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("--- [원본 IL 코드 (Before Injection)] ---");
            Console.WriteLine(".method public hidebysig specialname instance void set_text(string 'value') cil managed");
            Console.WriteLine("{");
            Console.WriteLine("    IL_0000: ldarg.0         // this 포인터 로드");
            Console.WriteLine("    IL_0001: ldarg.1         // value 인자 (영어 문자열) 로드");
            Console.WriteLine("    IL_0002: stfld           string TMPro.TMP_Text::m_text");
            Console.WriteLine("    ...");
            Console.WriteLine("}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- [수술 후 주입된 IL 코드 (After Injection)] ---");
            Console.WriteLine(".method public hidebysig specialname instance void set_text(string 'value') cil managed");
            Console.WriteLine("{");
            Console.WriteLine("    // >>> [INJECTED HOOK START: 잉크젯 노즐 가로채기] <<<");
            Console.WriteLine("    IL_0000: ldarg.1         // 1. 입력받은 영어 문자열을 평가 스택에 올림");
            Console.WriteLine("    IL_0001: call            string UnityTextHookPatch::ProcessText(string) // 2. 번역 함수 호출");
            Console.WriteLine("    IL_0006: starg.s 'value' // 3. 반환된 한국어 문자열로 원래 value 인자를 덮어씀!");
            Console.WriteLine("    // >>> [INJECTED HOOK END] <<<");
            Console.WriteLine("");
            Console.WriteLine("    IL_0008: ldarg.0         // 원래 게임 엔진 로직이 시작됨 (자신이 한국어를");
            Console.WriteLine("    IL_0009: ldarg.1         // 출력하고 있는지도 모른 채 덮어써진 한국어를 렌더링)");
            Console.WriteLine("    IL_000a: stfld           string TMPro.TMP_Text::m_text");
            Console.WriteLine("    ...");
            Console.WriteLine("}");
            Console.ResetColor();

            Console.WriteLine(@"
[핵심 이점]
1. BepInEx, MelonLoader 등 무거운 외부 모드 프레임워크 불필요 (단일 EXE 파일로 0.1초 만에 패치)
2. 게임 소스 코드가 없어도 컴파일된 바이너리 상태에서 완벽하게 텍스트 가로채기 성공
3. 게임 코드가 변경되어도 텍스트 렌더링 진입점만 가로채므로 버전 호환성 극대화");
        }

        private static void ShowPillar2_O1Benchmark()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [기술 2] O(1) 초고속 인메모리 딕셔너리 벤치마크 ===");
            Console.ResetColor();
            Console.WriteLine(@"
[개념 설명]
가로챈 문자열을 매번 디스크(하드/SSD) 파일에서 찾으면 게임 프레임이 뚝뚝 끊기는 스터터링이 발생합니다.
패치 엔진은 게임 시작 시 모든 번역 데이터를 메모리상에 '해시 테이블(Hash Table Dictionary)'로 로드합니다.
아래에서 10,000개의 번역 데이터를 메모리에 올리고 100,000회 연속 조회 속도를 직접 측정합니다:
");

            var engine = new TranslationEngine();
            // 더미 데이터 10,000개 로드
            for (int i = 0; i < 10000; i++)
            {
                engine.Add("Dialogue line sample #" + i + " with some text", "한국어 대사 샘플 #" + i + " 번역 텍스트입니다");
            }
            engine.Add("Why?", "어째서입니까?");
            engine.Add("Invisible Bag", "보이지 않는 가방");
            engine.Add("Knock", "두드림");

            int testIterations = 100000;
            Console.WriteLine("  * 메모리에 적재된 번역 항목 수: " + engine.Count.ToString("N0") + " 개");
            Console.WriteLine("  * 벤치마크 수행 횟수: " + testIterations.ToString("N0") + " 회 연속 조회\n");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < testIterations; i++)
            {
                string r = engine.Translate("Invisible Bag");
            }
            sw.Stop();

            double totalMs = sw.Elapsed.TotalMilliseconds;
            double avgUs = (totalMs / testIterations) * 1000.0; // 마이크로초
            double avgNs = avgUs * 1000.0; // 나노초

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [벤치마크 결과 측정 완료]");
            Console.WriteLine("  - 총 소요 시간: {0:F3} ms", totalMs);
            Console.WriteLine("  - 1회 조회당 평균 소요 시간: {0:F3} µs ({1:F1} 나노초)", avgUs, avgNs);
            Console.WriteLine("  - 60fps 1프레임(16.6ms) 동안 처리 가능한 텍스트 수: 약 {0:N0} 개", (int)(16.6 / (totalMs / testIterations)));
            Console.ResetColor();

            Console.WriteLine(@"
[결론]
1회 조회에 1초의 수십만 분의 1(마이크로초)도 걸리지 않으므로, 화면에 수백 줄의 대사가
동시에 출력되더라도 0.001초도 지연되지 않고 60fps / 144fps 방어율 100%를 달성합니다.");
        }

        private static void ShowPillar3_KoreanParticleMath()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [기술 3] 한글 유니코드 음절 분해 조사 수학 계산기 ===");
            Console.ResetColor();
            Console.WriteLine(@"
[개념 설명]
영어는 단어 뒤에 붙는 조사가 없습니다:
  - Spencer is / Nina is / Knock is
반면 한국어는 앞 글자의 받침(종성) 유무에 따라 조사가 달라집니다:
  - 스펜서는 (받침 없음) / 니나는 (받침 없음) / 두드림은 (받침 있음)

게임 대사 안에는 [[Knock]], [[Spencer]]처럼 변수가 런타임에 동적으로 주입되므로
사전에 번역문에 조사를 확정해 둘 수 없습니다.
우리는 한글 유니코드 음절 분해 수학 공식을 패치 엔진에 내장하여 이를 실시간으로 해결했습니다.
");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"[수학 공식]");
            Console.WriteLine(@"  음절 번호 = (문자 유니코드 - 0xAC00)");
            Console.WriteLine(@"  종성(받침) 번호 = 음절 번호 % 28");
            Console.WriteLine(@"");
            Console.WriteLine(@"  * 종성 번호 == 0 : 받침 없음 -> 는, 가, 를, 와, 로");
            Console.WriteLine(@"  * 종성 번호 != 0 : 받침 있음 -> 은, 이, 을, 과, 으로");
            Console.WriteLine(@"    (특수 예외: 종성 번호 == 8 ('ㄹ' 받침, 예: 칼, 물) -> 받침이 있어도 '로' 선택!)");
            Console.ResetColor();

            Console.WriteLine("\n[실제 단어 계산 검증 시뮬레이션]");
            string[] testWords = new string[] { "스펜서", "두드림", "칼", "물", "가방", "책", "100", "7" };
            foreach (var w in testWords)
            {
                char last = w[w.Length - 1];
                if (last >= 0xAC00 && last <= 0xD7A3)
                {
                    int syl = last - 0xAC00;
                    int coda = syl % 28;
                    string codaInfo = (coda == 0) ? "받침 없음" : (coda == 8 ? "받침 있음('ㄹ' 특수음)" : "받침 있음(종성=" + coda + ")");
                    string topic = KoreanParticleResolver.ResolveParticle(w, "은/는");
                    string subject = KoreanParticleResolver.ResolveParticle(w, "이/가");
                    string objectP = KoreanParticleResolver.ResolveParticle(w, "을/를");
                    string inst = KoreanParticleResolver.ResolveParticle(w, "으로/로");

                    Console.WriteLine("  '{0}' (0x{1:X4}) -> {2}", w, (int)last, codaInfo);
                    Console.WriteLine("    결과: {0}{1}, {0}{2}, {0}{3}, {0}{4}", w, topic, subject, objectP, inst);
                }
                else
                {
                    string topic = KoreanParticleResolver.ResolveParticle(w, "은/는");
                    string inst = KoreanParticleResolver.ResolveParticle(w, "으로/로");
                    Console.WriteLine("  '{0}' (숫자/기호) -> 결과: {0}{1}, {0}{2}", w, topic, inst);
                }
            }
        }

        private static void ShowPillar4_SpriteAndGrammarParser()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [기술 4] 복합 스프라이트 및 문법 파서 (Tag & Sprite Parser) ===");
            Console.ResetColor();
            Console.WriteLine(@"
[개념 설명]
게임 코드 내부에서 <sprite=5>Obscure, <sprite=2>Bewitch or <sprite=8>Trespass 처럼
아이콘 태그와 영문 단어를 쉼표와 'or'로 결합하여 하나의 문자열로 동적 생성하는 경우,
단순 텍스트 매칭 방식으로는 절대 번역할 수 없습니다.

TagAndSpriteParser는 정규표현식 파이프라인을 가동하여:
1. <sprite=...> 이미지 태그는 훼손되지 않도록 격리 보호
2. 그 뒤에 따라오는 영문 키워드(Obscure, Bewitch, Trespass)만 추출
3. 태그 사전에서 한국어(은폐, 홀림, 침범)로 각각 번역
4. 접속사(' or ', ' and ')를 감지하여 한국어 문맥(' 또는 ', ' 및 ')으로 치환
5. 다시 하나의 완성된 리치 텍스트 문자열로 실시간 재조립합니다.
");

            var termDict = new Dictionary<string, string>
            {
                { "Obscure", "은폐" },
                { "Bewitch", "홀림" },
                { "Trespass", "침범" },
                { "Expel", "추방" },
                { "Knock", "두드림" }
            };

            var parser = new TagAndSpriteParser(k => termDict.ContainsKey(k) ? termDict[k] : k);

            string sample1 = "<sprite=5>Obscure, <sprite=2>Bewitch or <sprite=8>Trespass";
            string sample2 = "<sprite=1>Expel";
            string sample3 = "대화 선택지에서 {0}을(를) 사용할 수 있습니다.";

            string res1 = parser.ParseAndTranslateAspectPhrase(sample1);
            string res2 = parser.ParseAndTranslateAspectPhrase(sample2);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[파서 변환 시뮬레이션 결과]");
            Console.WriteLine("  원본 1 : " + sample1);
            Console.WriteLine("  변환 1 : " + res1);
            Console.WriteLine();
            Console.WriteLine("  원본 2 : " + sample2);
            Console.WriteLine("  변환 2 : " + res2);
            Console.WriteLine();
            Console.WriteLine("  최종 포맷 결합: " + string.Format(sample3, res1));
            Console.ResetColor();
        }

        private static void ShowPillar5_FontFallbackArchitecture()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [기술 5] TextMeshPro 런타임 폰트 폴백(Fallback Font) 강제 주입 ===");
            Console.ResetColor();
            Console.WriteLine(@"
[개념 설명]
외국 인디 게임에는 영문 폰트만 들어있고 한글 글리프(글자 모양 데이터)가 0개입니다.
따라서 아무리 번역 텍스트를 제대로 넘겨주어도 화면에는 네모 상자(ㅁㅁㅁ)나 공백으로 깨져 나옵니다.

유니티의 TextMeshPro 엔진은 글리프를 찾지 못했을 때 우회할 '폴백 폰트 목록(fallbackFontAssetTable)'을
지원하지만, 인디 게임 개발자들은 이 목록을 비워둡니다.

[해결책: 런타임 폴백 주입]
1. 게임 시작 시 FontFallbackManager가 외장 NanumGothic.ttf 또는 윈도우 OS 폰트(맑은 고딕)로부터
   동적 유니티 폰트 에셋(TMP_FontAsset)을 메모리에 실시간으로 생성합니다.
2. 어셈블리 훅이 UI 텍스트 컴포넌트가 활성화될 때(Awake / OnEnable):
   baseFont.fallbackFontAssetTable.Insert(0, koreanFallbackFont);
   명령어를 실행하여 우리의 한글 폰트를 '폴백 목록 0순위'로 강제 등록합니다.
3. 유니티 텍스트 렌더러는 영문 폰트를 그리다가 한글을 만나는 순간, 0순위 폴백 폰트로 이동해
   나눔고딕 글리프를 즉시 꺼내와서 선명하고 깨짐 없이 화면에 렌더링합니다!
");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[폰트 폴백 작동 구조도]");
            Console.WriteLine(@"
  [게임 텍스트 출력 요청] -> TMP_Text.text = '두드림 (Knock)'
                                |
                                v
  [1단계: 영문 기본 폰트] -> ' (Knock)'는 있음!
                                '두드림' 글리프 없음! (Missing Glyph)
                                |
                                v (Fallback 트리거)
  [2단계: fallbackFontAssetTable[0]] -> [주입된 한글 폰트 (NanumGothic)]
                                '두드림' 글리프 발견!
                                |
                                v
  [최종 화면 렌더링] -> 영문은 원래 게임 폰트로, 한글은 선명한 나눔고딕으로 완벽 혼합 출력!
");
            Console.ResetColor();
        }

        private static void RunInteractiveSandbox()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [라이브 대화형 번역 파이프라인 샌드박스] ===");
            Console.ResetColor();
            Console.WriteLine(@"
직접 원하는 텍스트를 입력해 보세요.
대괄호 태그([[Spencer]], [[Knock]]), 조사 괄호((은)는, (이)가, (으)로),
인라인 스프라이트 태그(<sprite=1>Expel)를 자유롭게 조합하여 입력할 수 있습니다.
(종료하려면 'exit' 입력)
");

            var engine = new TranslationEngine();
            engine.Add("Spencer", "스펜서");
            engine.Add("Nina", "니나");
            engine.Add("Knock", "두드림");
            engine.Add("Invisible Bag", "보이지 않는 가방");
            engine.Add("Obscure", "은폐");
            engine.Add("Bewitch", "홀림");
            engine.Add("Trespass", "침범");
            engine.Add("Expel", "추방");
            engine.Add("Sword", "칼");
            engine.Add("Water", "물");

            var parser = new TagAndSpriteParser(k =>
            {
                string tr;
                return engine.TryGet(k, out tr) ? tr : k;
            });

            while (true)
            {
                Console.Write("\n입력 > ");
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line) || line.Trim().ToLower() == "exit")
                    break;

                var sw = Stopwatch.StartNew();

                // 파이프라인 실행
                string step1 = parser.ParseAndTranslateAspectPhrase(line);
                string step2 = parser.ParseAndExpandBracketLinks(step1);
                string step3 = KoreanParticleResolver.ResolveAllParticles(step2);

                sw.Stop();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("출력 > " + step3);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("       (처리 소요 시간: {0:F4} ms)", sw.Elapsed.TotalMilliseconds);
                Console.ResetColor();
            }
        }

        private static void RunFullTestSuite()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== [전체 종합 테스트 및 검증 스위트 실행] ===");
            Console.ResetColor();

            int passed = 0;
            int total = 0;

            Action<string, bool> assert = (desc, cond) =>
            {
                total++;
                if (cond)
                {
                    passed++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  [PASS] " + desc);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  [FAIL] " + desc);
                }
                Console.ResetColor();
            };

            // Test 1: Korean Particles
            assert("스펜서 + 은/는 -> 스펜서는", KoreanParticleResolver.ResolveParticle("스펜서", "은/는") == "는");
            assert("두드림 + 은/는 -> 두드림은", KoreanParticleResolver.ResolveParticle("두드림", "은/는") == "은");
            assert("스펜서 + 이/가 -> 스펜서가", KoreanParticleResolver.ResolveParticle("스펜서", "이/가") == "가");
            assert("두드림 + 이/가 -> 두드림이", KoreanParticleResolver.ResolveParticle("두드림", "이/가") == "이");
            assert("가방 + 을/를 -> 가방을", KoreanParticleResolver.ResolveParticle("가방", "을/를") == "을");
            assert("니나 + 을/를 -> 니나를", KoreanParticleResolver.ResolveParticle("니나", "을/를") == "를");
            assert("칼 + 으로/로 -> 칼로 ('ㄹ' 받침 특수 규칙)", KoreanParticleResolver.ResolveParticle("칼", "으로/로") == "로");
            assert("두드림 + 으로/로 -> 두드림으로 (일반 자음 받침)", KoreanParticleResolver.ResolveParticle("두드림", "으로/로") == "으로");
            assert("100 + 은/는 -> 100은 (숫자 0 영 받침)", KoreanParticleResolver.ResolveParticle("100", "은/는") == "은");
            assert("7 + 으로/로 -> 7로 (숫자 7 칠 'ㄹ' 받침)", KoreanParticleResolver.ResolveParticle("7", "으로/로") == "로");

            // Test 2: Tag & Sprite Parser
            var parser = new TagAndSpriteParser(k =>
            {
                if (k == "Obscure") return "은폐";
                if (k == "Bewitch") return "홀림";
                if (k == "Trespass") return "침범";
                if (k == "Expel") return "추방";
                return k;
            });

            string sIn = "<sprite=5>Obscure, <sprite=2>Bewitch or <sprite=8>Trespass";
            string sOut = parser.ParseAndTranslateAspectPhrase(sIn);
            assert("복합 스프라이트 파서 치환 검증", sOut == "<sprite=5>은폐, <sprite=2>홀림 또는 <sprite=8>침범");

            // Test 3: Bracket Links & Particles
            string bIn = "[[스펜서]](이)가 [[가방]](을)를 찾았다.";
            string bOut = KoreanParticleResolver.ResolveAllParticles(bIn);
            assert("인라인 대괄호 조사 합성 검증", bOut == "[[스펜서]]가 [[가방]]을 찾았다.");

            Console.WriteLine("\n=======================================================");
            Console.ForegroundColor = (passed == total) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("  테스트 결과: {0} / {1} 통과 ({2:F1}%)", passed, total, (double)passed / total * 100.0);
            Console.ResetColor();
            Console.WriteLine("=======================================================");
        }
    }
}
