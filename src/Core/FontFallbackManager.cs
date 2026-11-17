// ============================================================================
// Unity IL Localization Toolkit
// Module: FontFallbackManager.cs
// Description: Dynamic TextMeshPro fallback font asset injection at runtime.
// ============================================================================

using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityLocalizationToolkit.Core
{
    /// <summary>
    /// 해외 인디 게임의 기본 영문 폰트에서 한글 유니코드가 깨지는 현상(ㅁ자 또는 공백)을 방지하기 위해,
    /// 게임 실행 중 동적으로 한글 폰트(TTF)를 생성하여 TextMeshPro의 Fallback Font 목록에 주입하는 관리자입니다.
    /// </summary>
    public static class FontFallbackManager
    {
        private static TMP_FontAsset _fallbackFontAsset;
        private static bool _initialized = false;

        public static TMP_FontAsset FallbackFont { get { return _fallbackFontAsset; } }

        /// <summary>
        /// 지정된 TTF 파일 경로 또는 운영체제 기본 폰트(맑은 고딕 등)로부터 
        /// TextMeshPro용 동적 폰트 에셋을 생성하고 초기화합니다.
        /// </summary>
        /// <param name="ttfPath">한글 TTF 파일의 절대 경로 (없을 경우 OS 폰트 자동 탐색)</param>
        public static void Initialize(string ttfPath = null)
        {
            if (_initialized && _fallbackFontAsset != null) return;

            try
            {
                Font dynamicFont = null;

                // 1. 전달된 TTF 파일 경로 검사
                if (!string.IsNullOrEmpty(ttfPath) && File.Exists(ttfPath))
                {
                    dynamicFont = new Font(ttfPath);
                }

                // 2. 파일이 없거나 로드 실패 시 Windows 기본 폰트 폴백
                if (dynamicFont == null)
                {
                    string[] osFonts = new string[] { "Malgun Gothic", "NanumGothic", "AppleGothic" };
                    dynamicFont = Font.CreateDynamicFontFromOSFont(osFonts, 24);
                }

                if (dynamicFont == null)
                {
                    Debug.LogWarning("[FontFallbackManager] 한글 폰트를 로드할 수 없습니다.");
                    return;
                }

                // 3. Unity TextMeshPro 폰트 에셋 생성
                _fallbackFontAsset = TMP_FontAsset.CreateFontAsset(dynamicFont);
                if (_fallbackFontAsset != null)
                {
                    _fallbackFontAsset.name = "Korean_Dynamic_Fallback_Font";
                    _initialized = true;
                    Debug.Log("[FontFallbackManager] TextMeshPro 한글 폴백 폰트 에셋이 성공적으로 생성되었습니다.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[FontFallbackManager] 폰트 에셋 초기화 예외: " + ex.Message);
            }
        }

        /// <summary>
        /// 지정된 TMP_Text 콤포넌트의 폰트에 한글 폴백 폰트가 등록되어 있는지 검사하고,
        /// 없으면 FallbackFontAssets 목록의 0순위로 주입합니다.
        /// </summary>
        public static void EnsureFontFallback(TMP_Text tmpText)
        {
            if (tmpText == null) return;
            if (!_initialized) Initialize();
            if (_fallbackFontAsset == null) return;

            var baseFont = tmpText.font;
            if (baseFont == null || baseFont == _fallbackFontAsset) return;

            if (baseFont.fallbackFontAssetTable == null)
            {
                baseFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }

            // 폴백 목록에 없으면 0순위(최우선)로 삽입
            if (!baseFont.fallbackFontAssetTable.Contains(_fallbackFontAsset))
            {
                baseFont.fallbackFontAssetTable.Insert(0, _fallbackFontAsset);
            }
        }
    }
}
