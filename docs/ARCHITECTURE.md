# Unity IL Localization Toolkit: Architecture & IL Injection Deep Dive

본 문서는 **Unity IL Localization Toolkit (UIL-Toolkit)**의 내부 아키텍처와 `Mono.Cecil`을 이용한 IL(Intermediate Language) 바이트코드 주입 원리를 상세히 기술합니다.

---

## 1. 유니티 텍스트 파이프라인의 취약점과 후킹 지점

유니티 엔진(특히 TextMeshPro 기반)에서 문자열이 화면에 렌더링되는 전형적인 수명 주기는 다음과 같습니다:

```
[게임 로직] (대사 선택 / 퀘스트 진행)
    │
    ▼
[문자열 포맷팅] (Loc.FormatWithArgs / string.Format)
    │
    ▼
[UI 텍스트 할당] (TMP_Text.set_text(string value) / SetText(string))
    │
    ▼
[글리프 파싱 & 레이아웃] (TMP_Text.GenerateTextMesh)
    │
    ▼
[그래픽스 렌더러] (메시 지오메트리 빌드 -> GPU 셰이더)
```

### 왜 TMP_Text.set_text인가?
1. **단일 수렴점 (Chokepoint)**: 게임 내의 대화창, 인벤토리 툴팁, 퀘스트 알림, 시스템 메뉴 등 모든 UI 요소는 화면에 글자를 표시하기 위해 필연적으로 `TMP_Text.set_text(string)` 또는 `TMP_Text.text = ...` 프로퍼티 세터를 호출합니다.
2. **독립성 (Game-Agnostic)**: 게임 고유의 클래스(`DialogueManager`, `QuestController`)에 의존하지 않고, 유니티 공용 라이브러리인 `Unity.TextMeshPro.dll`을 가로채므로 특정 게임에 종속되지 않는 범용 프레임워크가 됩니다.

---

## 2. Mono.Cecil IL 인젝션 메커니즘

`Mono.Cecil`은 .NET 어셈블리를 실행하지 않고도 정적 디스크 파일 상태에서 바이트코드 명령어를 탐색, 수정, 재작성할 수 있는 고성능 라이브러리입니다.

### IL 명령어 삽입 상세

`AssemblyPatcher.cs`의 주입 루틴은 다음과 같습니다:

```csharp
var setTextMethod = tmpTextType.Methods.FirstOrDefault(m => m.Name == "set_text" && m.Parameters.Count == 1);
var il = setTextMethod.Body.GetILProcessor();
var first = setTextMethod.Body.Instructions[0];

// 1. ldarg.1 : 첫 번째 인자(value: 입력된 영어 문자열)를 평가 스택에 로드
il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));

// 2. call : UnityTextHookPatch.ProcessText(string) 메서드 호출
il.InsertBefore(first, il.Create(OpCodes.Call, processTextRef));

// 3. starg.s : 스택에 반환된 번역 문자열로 원래 value 인자를 덮어씀
il.InsertBefore(first, il.Create(OpCodes.Starg_S, setTextMethod.Parameters[0]));
```

### 평가 스택(Evaluation Stack)의 변화 추이

| 단계 | 실행 IL 명령어 | 스택 상태 (Top of Stack) | 설명 |
|:---:|:---|:---|:---|
| 0 | *진입 시점* | `[empty]` | `set_text(string value)` 호출됨 |
| 1 | `ldarg.1` | `[value ("Hello")]` | 영문 인자 `value`를 스택에 푸시 |
| 2 | `call ProcessText` | `[returnValue ("안녕하세요")]` | 패치 엔진이 번역 후 결과를 스택에 푸시 |
| 3 | `starg.s 1` | `[empty]` | 스택의 결과값을 `value` 인자 변수에 덮어씀 |
| 4 | *기존 첫 명령* (`ldarg.0`) | `[this]` | 이제 게임 엔진의 원래 로직이 실행됨 |
| 5 | `ldarg.1` | `[this, value ("안녕하세요")]` | 덮어써진 한국어가 필드에 기록됨! |

---

## 3. TextMeshPro 폰트 폴백(Fallback Font) 구조

유니티의 `TMP_FontAsset` 클래스는 다국어 지원을 위해 `fallbackFontAssetTable` 리스트를 내장하고 있습니다.

```csharp
public class TMP_FontAsset : TMP_Asset
{
    // 누락된 글리프(Missing Glyphs) 발생 시 순차적으로 탐색할 보조 폰트 목록
    public List<TMP_FontAsset> fallbackFontAssetTable;
}
```

### 주입 시점 및 순서의 중요성:
인디 게임 개발자가 기본 영문 폰트에 다른 폴백 폰트를 이미 등록해 두었더라도, 한글 글리프가 없는 영문 폰트일 가능성이 높습니다.  
따라서 우리의 `FontFallbackManager`는 항상 **`Insert(0, koreanFallbackFont)`**를 호출하여 최우선 0순위로 한글 폰트를 등록합니다:

```
[TextMeshPro 렌더러]
       │
       ▼
[글리프 탐색: '스'] ──(없음)──> [fallbackFontAssetTable[0]: NanumGothic] ──(발견!)──> [화면 렌더링]
```

이 방식을 통해 원본 게임의 영문 폰트 스타일(크기, 굵기, 텍스처 아틀라스)은 그대로 유지하면서, 한글 글자만 선명한 나눔고딕으로 완벽하게 하이브리드 렌더링됩니다.
