// ------------------------------------------------------------
// @file    SudokuBoard.cs
// @brief   9x9 스도쿠 보드 전체의 UI를 동적 생성하고 관리하는 일반 클래스.
//          셀 생성, 하이라이트 업데이트, 숫자 표시, 셀 선택을 담당한다.
//          MonoBehaviour가 아니며, 게임 컨트롤러(Phase E의 SudokuGame)에서 생성/관리된다.
// @deps    SudokuCell, SudokuPuzzle, CellState,
//          RoseEngine (GameObject, RectTransform, UIImage, UIText, UIButton, Sprite, Color,
//                      ImageType, TextAnchor, Vector2, Object)
// @exports
//   class SudokuBoard
//     Cells: SudokuCell[,]                                          -- 9x9 셀 배열
//     SelectedRow: int                                              -- 현재 선택된 셀의 행 (-1이면 미선택)
//     SelectedCol: int                                              -- 현재 선택된 셀의 열 (-1이면 미선택)
//     OnCellClicked: Action<int, int>?                              -- 셀 클릭 콜백 (row, col)
//     Initialize(GameObject, SudokuPuzzle, Sprite[]): void          -- 81개 셀 GO 동적 생성 및 초기화
//     UpdateDisplay(SudokuPuzzle): void                             -- 퍼즐 데이터로 전체 셀 갱신
//     SelectCell(int row, int col): void                            -- 셀 선택 및 하이라이트 갱신
//     ClearSelection(): void                                        -- 선택 해제
//     UpdateHighlights(SudokuPuzzle): void                          -- 선택 기준 하이라이트 적용
//     DestroyBoard(): void                                          -- 모든 셀 GO 파괴
// @note    RectTransform 좌표계는 Y-down. anchoredPosition의 Y가 증가하면 아래로 이동.
//          cellSprites 배열: [0]=normal, [1]=given, [2]=selected, [3]=samegroup, [4]=error
//          같은 3x3 박스 판별: (r/3 == selectedRow/3) && (c/3 == selectedCol/3)
//          하이라이트 우선순위: Normal -> SameGroup -> SameNumber -> Error -> Selected
// ------------------------------------------------------------

using System;
using RoseEngine;

public class SudokuBoard
{
    private const int SIZE = 9;

    public SudokuCell[,] Cells { get; private set; } = new SudokuCell[SIZE, SIZE];
    public int SelectedRow { get; private set; } = -1;
    public int SelectedCol { get; private set; } = -1;
    public Action<int, int>? OnCellClicked;
    private readonly System.Collections.Generic.List<GameObject> _gridLines = new();

    /// <summary>
    /// boardPanel 아래에 81개 셀 GameObject를 동적 생성하고 퍼즐 데이터로 초기화한다.
    /// </summary>
    public void Initialize(GameObject boardPanel, SudokuPuzzle puzzle, Sprite[] cellSprites, Font? font = null, Sprite? lineThin = null, Sprite? lineThick = null)
    {
        var boardRT = boardPanel.GetComponent<RectTransform>();
        float boardSize = boardRT != null ? boardRT.sizeDelta.x : 450f;
        float cellSize = boardSize / SIZE;

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                // 셀 루트 GO 생성
                var cellGO = new GameObject($"Cell_{row}_{col}");
                cellGO.transform.SetParent(boardPanel.transform);

                // RectTransform 설정 (좌상단 기준, Y-down)
                var rt = cellGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0, 0);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(col * cellSize, row * cellSize);

                // UIImage (배경 스프라이트, Sliced 모드)
                var img = cellGO.AddComponent<UIImage>();
                img.sprite = cellSprites[0];
                img.imageType = ImageType.Sliced;

                // UIButton (클릭 핸들러)
                var btn = cellGO.AddComponent<UIButton>();
                int capturedRow = row;
                int capturedCol = col;
                btn.onClick = () => HandleCellClick(capturedRow, capturedCol);

                // 숫자 텍스트용 자식 GO
                var textGO = new GameObject("NumberText");
                textGO.transform.SetParent(cellGO.transform);

                var textRT = textGO.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = new Vector2(1, 1);
                textRT.sizeDelta = Vector2.zero;
                textRT.anchoredPosition = Vector2.zero;

                var uiText = textGO.AddComponent<UIText>();
                uiText.font = font;
                uiText.fontSize = cellSize * 0.54f;
                uiText.alignment = TextAnchor.MiddleCenter;

                // SudokuCell 인스턴스 생성
                var cell = new SudokuCell(row, col, cellGO, cellSprites);
                Cells[row, col] = cell;
            }
        }

        // 격자선 생성 (3x3 박스 구분)
        CreateGridLines(boardPanel, boardSize, cellSize, lineThin, lineThick);

        // 퍼즐 데이터로 초기 표시
        UpdateDisplay(puzzle);
    }

    /// <summary>
    /// 퍼즐 데이터를 기반으로 모든 셀의 숫자와 상태를 갱신한다.
    /// </summary>
    public void UpdateDisplay(SudokuPuzzle puzzle)
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                var cell = Cells[row, col];
                int displayNumber = puzzle.GetDisplayNumber(row, col);
                bool isGiven = puzzle.IsGiven[row, col];
                cell.SetNumber(displayNumber, isGiven);
            }
        }

        UpdateHighlights(puzzle);
    }

    /// <summary>
    /// 셀을 선택하고 하이라이트를 갱신한다.
    /// </summary>
    public void SelectCell(int row, int col)
    {
        SelectedRow = row;
        SelectedCol = col;
    }

    /// <summary>
    /// 선택을 해제한다.
    /// </summary>
    public void ClearSelection()
    {
        SelectedRow = -1;
        SelectedCol = -1;
    }

    /// <summary>
    /// 선택된 셀 기준으로 같은 행/열/박스, 같은 숫자 하이라이트를 적용한다.
    /// 우선순위: Normal -> SameGroup -> SameNumber -> Error -> Selected
    /// </summary>
    public void UpdateHighlights(SudokuPuzzle puzzle)
    {
        // 1. 모든 셀을 Normal로 리셋
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                Cells[r, c].SetState(CellState.Normal);
            }
        }

        // 2. 선택된 셀이 없으면 리턴
        if (SelectedRow < 0 || SelectedCol < 0)
            return;

        // 3. 같은 행/열/박스의 셀에 SameGroup 적용
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                bool sameRow = r == SelectedRow;
                bool sameCol = c == SelectedCol;
                bool sameBox = (r / 3 == SelectedRow / 3) && (c / 3 == SelectedCol / 3);

                if (sameRow || sameCol || sameBox)
                {
                    Cells[r, c].SetState(CellState.SameGroup);
                }
            }
        }

        // 4. 같은 숫자 하이라이트
        int selectedNumber = puzzle.GetDisplayNumber(SelectedRow, SelectedCol);
        if (selectedNumber != 0)
        {
            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    if (puzzle.GetDisplayNumber(r, c) == selectedNumber)
                    {
                        Cells[r, c].SetState(CellState.SameNumber);
                    }
                }
            }
        }

        // 5. 오류 셀에 Error 적용
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (puzzle.HasError(r, c))
                {
                    Cells[r, c].SetState(CellState.Error);
                }
            }
        }

        // 6. 선택된 셀 자체에 Selected 적용 (마지막에 덮어쓰기)
        Cells[SelectedRow, SelectedCol].SetState(CellState.Selected);
    }

    /// <summary>
    /// 모든 셀 GameObject를 파괴한다.
    /// </summary>
    public void DestroyBoard()
    {
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                var cell = Cells[r, c];
                if (cell?.CellObject != null)
                {
                    RoseEngine.Object.Destroy(cell.CellObject);
                }
            }
        }

        foreach (var line in _gridLines)
        {
            if (line != null) RoseEngine.Object.Destroy(line);
        }
        _gridLines.Clear();

        Cells = new SudokuCell[SIZE, SIZE];
        SelectedRow = -1;
        SelectedCol = -1;
    }

    private void CreateGridLines(GameObject boardPanel, float boardSize, float cellSize, Sprite? lineThin, Sprite? lineThick)
    {
        float thinWidth = 2f;
        float thickWidth = 4f;

        // 세로선 (col 1~8)
        for (int col = 1; col < SIZE; col++)
        {
            bool isThick = col % 3 == 0;
            var sprite = isThick ? lineThick : lineThin;
            if (sprite == null) continue;

            float width = isThick ? thickWidth : thinWidth;
            var go = new GameObject($"VLine_{col}");
            go.transform.SetParent(boardPanel.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(width, boardSize);
            rt.anchoredPosition = new Vector2(col * cellSize, 0f);
            var img = go.AddComponent<UIImage>();
            img.sprite = sprite;
            _gridLines.Add(go);
        }

        // 가로선 (row 1~8)
        for (int row = 1; row < SIZE; row++)
        {
            bool isThick = row % 3 == 0;
            var sprite = isThick ? lineThick : lineThin;
            if (sprite == null) continue;

            float height = isThick ? thickWidth : thinWidth;
            var go = new GameObject($"HLine_{row}");
            go.transform.SetParent(boardPanel.transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(boardSize, height);
            rt.anchoredPosition = new Vector2(0f, row * cellSize);
            var img = go.AddComponent<UIImage>();
            img.sprite = sprite;
            _gridLines.Add(go);
        }
    }

    private void HandleCellClick(int row, int col)
    {
        OnCellClicked?.Invoke(row, col);
    }
}
