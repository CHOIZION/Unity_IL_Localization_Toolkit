// ============================================================================
// Unity IL Localization Toolkit
// Example: SampleGameHook.cs
// Description: Demonstrates how to write custom game-specific hooks using Mono.Cecil.
// ============================================================================

using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UnityLocalizationToolkit.Examples
{
    /// <summary>
    /// 게임 고유의 대사 시스템(예: DialogueManager, QuestSystem)을 후킹하는 예제 코드입니다.
    /// </summary>
    public static class SampleGameHook
    {
        public static void PatchCustomDialogueSystem(string scriptsDllPath, MethodReference processTextMethod)
        {
            var asm = AssemblyDefinition.ReadAssembly(scriptsDllPath, new ReaderParameters { ReadWrite = true });
            var mod = asm.MainModule;

            // 예시: 게임 내 "DialogueManager" 클래스의 "ShowSubtitle(string speaker, string message)" 메서드 후킹
            var dialogueManager = mod.Types.FirstOrDefault(t => t.Name == "DialogueManager");
            if (dialogueManager != null)
            {
                var showSubtitle = dialogueManager.Methods.FirstOrDefault(m => m.Name == "ShowSubtitle");
                if (showSubtitle != null && showSubtitle.HasBody)
                {
                    var il = showSubtitle.Body.GetILProcessor();
                    var first = showSubtitle.Body.Instructions[0];

                    // message 인자(index 2)를 ProcessText로 변환하여 덮어쓰기
                    // ldarg.2 -> call ProcessText -> starg.s 2
                    il.InsertBefore(first, il.Create(OpCodes.Ldarg_2));
                    il.InsertBefore(first, il.Create(OpCodes.Call, mod.ImportReference(processTextMethod)));
                    il.InsertBefore(first, il.Create(OpCodes.Starg_S, showSubtitle.Parameters[1]));

                    Console.WriteLine("[SampleHook] DialogueManager::ShowSubtitle 후킹 완료!");
                }
            }

            asm.Write();
        }
    }
}
