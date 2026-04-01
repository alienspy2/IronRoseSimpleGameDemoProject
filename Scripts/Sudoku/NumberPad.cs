// ------------------------------------------------------------
// @file    NumberPad.cs
// @brief   하단 숫자 패드(1~9), 지우기, 힌트 버튼의 클릭 이벤트를 관리하는 일반 클래스.
//          MonoBehaviour가 아니며, SudokuGame에서 생성/관리된다.
// @deps    RoseEngine (GameObject, UIButton, Transform)
// @exports
//   class NumberPad
//     OnNumberClicked: Action<int>?     -- 숫자 버튼 클릭 콜백 (1~9)
//     OnEraseClicked: Action?           -- 지우기 버튼 클릭 콜백
//     OnHintClicked: Action?            -- 힌트 버튼 클릭 콜백
//     Initialize(GameObject numberPadPanel): void  -- NumberPadPanel 아래 버튼에 onClick 연결
// @note    "Num1"~"Num9", "EraseButton", "HintButton" 이름으로 자식을 찾아 연결한다.
//          FindChildByName은 직계 자식만 검색한다.
// ------------------------------------------------------------

using System;
using RoseEngine;

public class NumberPad
{
    private const int NUM_COUNT = 9;

    public Action<int>? OnNumberClicked;
    public Action? OnEraseClicked;
    public Action? OnHintClicked;

    // 숫자 버튼 참조 (인덱스 0 = 숫자 1, ... 인덱스 8 = 숫자 9)
    private readonly UIButton?[] _numButtons = new UIButton?[NUM_COUNT];
    private readonly UIText?[] _numTexts = new UIText?[NUM_COUNT];
    private readonly Color[] _originalTextColors = new Color[NUM_COUNT];

    /// <summary>
    /// NumberPadPanel 아래의 숫자/지우기/힌트 버튼에 onClick 핸들러를 연결한다.
    /// </summary>
    public void Initialize(GameObject numberPadPanel)
    {
        // 숫자 버튼: 이름이 "Num1" ~ "Num9"
        for (int i = 1; i <= 9; i++)
        {
            var btnGO = FindChildByName(numberPadPanel, $"Num{i}");
            if (btnGO != null)
            {
                var btn = btnGO.GetComponent<UIButton>();
                if (btn != null)
                {
                    int num = i; // 캡처용 로컬 변수
                    btn.onClick = () => OnNumberClicked?.Invoke(num);
                }
                _numButtons[i - 1] = btn;
                var text = btnGO.GetComponentInChildren<UIText>();
                _numTexts[i - 1] = text;
                if (text != null) _originalTextColors[i - 1] = text.color;
            }
        }

        // 지우기 버튼
        var eraseGO = FindChildByName(numberPadPanel, "EraseButton");
        if (eraseGO != null)
        {
            var btn = eraseGO.GetComponent<UIButton>();
            if (btn != null) btn.onClick = () => OnEraseClicked?.Invoke();
        }

        // 힌트 버튼
        var hintGO = FindChildByName(numberPadPanel, "HintButton");
        if (hintGO != null)
        {
            var btn = hintGO.GetComponent<UIButton>();
            if (btn != null) btn.onClick = () => OnHintClicked?.Invoke();
        }
    }

    /// <summary>
    /// 보드 상태에 따라 숫자 버튼의 활성/비활성을 갱신한다.
    /// 9개 모두 배치된 숫자의 버튼은 비활성화하고 텍스트도 어둡게 한다.
    /// </summary>
    public void UpdateButtonStates(SudokuPuzzle puzzle)
    {
        var disabledTextColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        for (int i = 0; i < NUM_COUNT; i++)
        {
            int number = i + 1;
            bool allPlaced = puzzle.CountDisplayed(number) >= NUM_COUNT;

            if (_numButtons[i] != null)
                _numButtons[i]!.interactable = !allPlaced;

            if (_numTexts[i] != null)
                _numTexts[i]!.color = allPlaced ? disabledTextColor : _originalTextColors[i];
        }
    }

    private static GameObject? FindChildByName(GameObject parent, string name)
    {
        var t = parent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (child.gameObject.name == name) return child.gameObject;
        }
        return null;
    }
}
