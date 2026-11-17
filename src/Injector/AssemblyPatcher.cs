// ============================================================================
// Unity IL Localization Toolkit
// Module: AssemblyPatcher.cs
// Description: Mono.Cecil-based IL assembly injector for Unity games.
// ============================================================================

using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UnityLocalizationToolkit.Injector
{
    /// <summary>
    /// Mono.Cecil을 사용하여 컴파일된 Unity 게임 어셈블리(DLL)에 
    /// 텍스트 가로채기 및 폰트 폴백 훅 명령어를 직접 주입하는 CLI 도구입니다.
    /// </summary>
    public class AssemblyPatcher
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("  Unity IL Assembly Patcher - Bytecode Injection Engine   ");
            Console.WriteLine("==========================================================");

            if (args.Length < 2)
            {
                Console.WriteLine("사용법: AssemblyPatcher.exe <TargetManagedDir> <PatchDllPath>");
                Console.WriteLine("예시: AssemblyPatcher.exe \"C:\\Game\\game_Data\\Managed\" \"UnityTextHookPatch.dll\"");
                return 1;
            }

            string managedDir = args[0];
            string patchDllPath = args[1];

            if (!Directory.Exists(managedDir))
            {
                Console.WriteLine("[ERROR] 대상 Managed 디렉터리를 찾을 수 없습니다: " + managedDir);
                return 1;
            }

            if (!File.Exists(patchDllPath))
            {
                Console.WriteLine("[ERROR] 패치 DLL 파일을 찾을 수 없습니다: " + patchDllPath);
                return 1;
            }

            try
            {
                var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(managedDir);
                resolver.AddSearchDirectory(Path.GetDirectoryName(patchDllPath));

                var readerParams = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = false };

                // 1. 패치 DLL에서 훅 메서드 참조 읽기
                var patchAsm = AssemblyDefinition.ReadAssembly(patchDllPath, readerParams);
                var patchType = patchAsm.MainModule.Types.FirstOrDefault(t => t.Name == "UnityTextHookPatch");
                if (patchType == null)
                {
                    Console.WriteLine("[ERROR] UnityTextHookPatch 클래스를 찾을 수 없습니다.");
                    return 1;
                }

                var processTextMethod = patchType.Methods.FirstOrDefault(m => m.Name == "ProcessText");
                var ensureTMPTextMethod = patchType.Methods.FirstOrDefault(m => m.Name == "EnsureTMPText");

                // 2. Unity.TextMeshPro.dll 패치
                string tmproDllPath = Path.Combine(managedDir, "Unity.TextMeshPro.dll");
                if (File.Exists(tmproDllPath))
                {
                    PatchTextMeshPro(tmproDllPath, processTextMethod, ensureTMPTextMethod, readerParams);
                }
                else
                {
                    Console.WriteLine("[WARN] Unity.TextMeshPro.dll을 찾을 수 없습니다 (생략).");
                }

                Console.WriteLine("\n[SUCCESS] 모든 대상 어셈블리 패치 및 바이트코드 주입이 완료되었습니다!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[FATAL ERROR] 어셈블리 패치 중 치명적 오류 발생:\n" + ex);
                return 1;
            }
        }

        private static void PatchTextMeshPro(string dllPath, MethodDefinition processTextDef, MethodDefinition ensureTMPTextDef, ReaderParameters readerParams)
        {
            Console.WriteLine("\n--- Patching Unity.TextMeshPro.dll ---");

            // 원본 백업
            string bakPath = dllPath + ".bak";
            if (!File.Exists(bakPath))
            {
                File.Copy(dllPath, bakPath, true);
                Console.WriteLine("원본 백업 생성 완료: " + Path.GetFileName(bakPath));
            }

            var asm = AssemblyDefinition.ReadAssembly(bakPath, readerParams);
            var mod = asm.MainModule;

            var processTextRef = mod.ImportReference(processTextDef);
            var ensureTMPTextRef = mod.ImportReference(ensureTMPTextDef);

            // 1. TMP_Text.set_text(string value) 가로채기
            var tmpTextType = mod.Types.FirstOrDefault(t => t.Name == "TMP_Text");
            if (tmpTextType != null)
            {
                var setTextMethod = tmpTextType.Methods.FirstOrDefault(m => m.Name == "set_text" && m.Parameters.Count == 1);
                if (setTextMethod != null && setTextMethod.HasBody)
                {
                    var il = setTextMethod.Body.GetILProcessor();
                    var first = setTextMethod.Body.Instructions[0];

                    // ldarg.1 (입력 문자열 인자) -> call ProcessText -> starg.s 1 (인자 교체)
                    il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
                    il.InsertBefore(first, il.Create(OpCodes.Call, processTextRef));
                    il.InsertBefore(first, il.Create(OpCodes.Starg_S, setTextMethod.Parameters[0]));

                    Console.WriteLine("[HOOKED] TMP_Text::set_text(string) 인자 변환 주입 완료");
                }
            }

            // 2. TextMeshProUGUI.Awake() 폰트 폴백 주입
            var uguiType = mod.Types.FirstOrDefault(t => t.Name == "TextMeshProUGUI");
            if (uguiType != null)
            {
                var awakeMethod = uguiType.Methods.FirstOrDefault(m => m.Name == "Awake");
                if (awakeMethod != null && awakeMethod.HasBody)
                {
                    var il = awakeMethod.Body.GetILProcessor();
                    var first = awakeMethod.Body.Instructions[0];

                    // ldarg.0 (this) -> call EnsureTMPText
                    il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(first, il.Create(OpCodes.Call, ensureTMPTextRef));

                    Console.WriteLine("[HOOKED] TextMeshProUGUI::Awake() 폰트 폴백 검사 주입 완료");
                }
            }

            // 저장
            asm.Write(dllPath);
            Console.WriteLine("패치된 어셈블리 저장 완료: " + dllPath);
        }
    }
}
