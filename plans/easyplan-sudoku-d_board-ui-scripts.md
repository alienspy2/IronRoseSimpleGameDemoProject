# Phase D: 보드 UI 스크립트 (SudokuBoard, SudokuCell)

## 목표
- 9x9 스도쿠 보드의 셀을 동적으로 생성하고 관리하는 스크립트를 작성한다.
- 셀 선택, 하이라이트(같은 행/열/박스, 같은 숫자), 오류 표시 기능을 구현한다.
- `dotnet build` 성공을 보장한다.

## 선행 조건
- Phase B 완료 (SudokuPuzzle, SudokuGenerator, Difficulty 존재)
- Phase C 완료 (씬에 BoardPanel이 존재)

## 생성할 파일

### `Scripts/Sudoku/CellState.cs`
- **역할**: 셀의 시각적 상태를 나타내는 열거형
- **열거형**: `CellState`
- **값**: `Normal`, `Selected`, `SameGroup`, `SameNumber`, `Error`
- **의존**: 없음

### `Scripts/Sudoku/SudokuCell.cs`
- **역할**: 개별 셀 하나의 시각적 상태와 클릭 동작을 관리한다.
- **클래스**: `SudokuCell` (MonoBehaviour 아님, 일반 클래스)
- **주요 멤버**:
  - `int Row` (readonly) -- 0~8
  - `int Col` (readonly) -- 0~8
  - `GameObject CellObject` -- 셀의 루트 GameObject
  - `CellState State` -- 현재 시각적 상태
  - `void SetNumber(int number, bool isGiven)` -- 셀에 표시할 숫자를 설정한다. 0이면 빈칸. isGiven이면 진한 잉크 색상, 아니면 연필 색상.
  - `void SetState(CellState state)` -- 셀의 시각적 상태(배경 스프라이트)를 변경한다.
  - `void Clear()` -- 숫자를 지우고 Normal 상태로 되돌린다.
- **의존**: `RoseEngine` (GameObject, RectTransform, UIImage, UIText, UIButton)
- **구현 힌트**:
  - 생성자에서 받을 것: `(int row, int col, GameObject cellGO, Sprite[] cellSprites)`
    - cellSprites 배열: [0]=normal, [1]=given, [2]=selected, [3]=samegroup, [4]=error
  - CellObject 내부 구조 (SudokuBoard.CreateCell에서 생성):
    - `CellObject` (RectTransform + UIImage + UIButton)
      - `NumberText` (자식, RectTransform + UIText)
  - `SetNumber()` 구현:
    ```
    var text = CellObject 자식의 UIText 컴포넌트
    text.text = number == 0 ? "" : number.ToString()
    text.color = isGiven ? new Color(0.1f, 0.1f, 0.3f, 1f) : new Color(0.4f, 0.35f, 0.3f, 1f)
    ```
    - isGiven 색상: 짙은 남색 (펜 느낌) `(0.1, 0.1, 0.3, 1)`
    - userInput 색상: 연한 회색-갈색 (연필 느낌) `(0.4, 0.35, 0.3, 1)`
  - `SetState()` 구현: UIImage의 sprite를 상태에 맞는 스프라이트로 교체
    - Normal -> cell_normal, Selected -> cell_selected, SameGroup -> cell_samegroup
    - Error -> cell_error, 그리고 isGiven인 셀의 Normal 상태에서는 cell_given 사용

### `Scripts/Sudoku/SudokuBoard.cs`
- **역할**: 9x9 보드 전체의 UI를 생성하고 관리한다. 셀 생성, 하이라이트 업데이트, 숫자 표시를 담당.
- **클래스**: `SudokuBoard` (일반 클래스, MonoBehaviour 아님)
- **주요 멤버**:
  - `SudokuCell[,] Cells` -- 9x9 셀 배열
  - `int SelectedRow` -- 현재 선택된 셀의 행 (-1이면 미선택)
  - `int SelectedCol` -- 현재 선택된 셀의 열 (-1이면 미선택)
  - `Action<int, int>? OnCellClicked` -- 셀 클릭 콜백 (row, col)
  - `void Initialize(GameObject boardPanel, SudokuPuzzle puzzle, Sprite[] cellSprites)` -- 보드 패널 아래에 81개 셀 GO를 동적 생성하고 퍼즐 데이터로 초기화한다.
  - `void UpdateDisplay(SudokuPuzzle puzzle)` -- 퍼즐 데이터를 기반으로 모든 셀의 숫자와 상태를 갱신한다.
  - `void SelectCell(int row, int col)` -- 셀을 선택하고 하이라이트를 갱신한다.
  - `void ClearSelection()` -- 선택 해제.
  - `void UpdateHighlights(SudokuPuzzle puzzle)` -- 선택된 셀 기준으로 같은 행/열/박스, 같은 숫자 하이라이트를 적용한다.
  - `void DestroyBoard()` -- 모든 셀 GO를 파괴한다 (새 게임 시).
- **의존**: `RoseEngine`, `SudokuCell`, `SudokuPuzzle`, `CellState`
- **구현 힌트**:
  - `Initialize()` 구현:
    1. boardPanel의 RectTransform 크기를 가져온다 (sizeDelta로 450x450 예상)
    2. 셀 크기 계산: `cellSize = boardSize / 9` (약 50x50)
    3. 이중 루프 (row 0~8, col 0~8):
       a. 새 GameObject 생성: `new GameObject($"Cell_{row}_{col}")`
       b. `cellGO.transform.SetParent(boardPanel.transform)`
       c. RectTransform 추가 및 설정:
          ```csharp
          var rt = cellGO.AddComponent<RectTransform>();
          rt.anchorMin = Vector2.zero;
          rt.anchorMax = Vector2.zero;
          rt.pivot = new Vector2(0, 0);
          rt.sizeDelta = new Vector2(cellSize, cellSize);
          rt.anchoredPosition = new Vector2(col * cellSize, row * cellSize);
          ```
       d. UIImage 추가: `var img = cellGO.AddComponent<UIImage>(); img.sprite = cellSprites[0]; img.imageType = ImageType.Sliced;`
       e. UIButton 추가: `var btn = cellGO.AddComponent<UIButton>();`
       f. btn.onClick에 셀 클릭 핸들러 연결 (row, col을 캡처)
       g. 숫자 텍스트용 자식 GO 생성:
          ```csharp
          var textGO = new GameObject("NumberText");
          textGO.transform.SetParent(cellGO.transform);
          var textRT = textGO.AddComponent<RectTransform>();
          // StretchAll 설정
          textRT.anchorMin = Vector2.zero;
          textRT.anchorMax = new Vector2(1, 1);
          textRT.sizeDelta = Vector2.zero;
          textRT.anchoredPosition = Vector2.zero;
          var uiText = textGO.AddComponent<UIText>();
          uiText.fontSize = cellSize * 0.6f;
          uiText.alignment = TextAnchor.MiddleCenter;
          ```
       h. SudokuCell 인스턴스 생성 및 Cells[row, col]에 저장
    4. 퍼즐 데이터로 초기 표시: `UpdateDisplay(puzzle)` 호출

  - `UpdateHighlights()` 구현:
    1. 모든 셀을 Normal 상태로 리셋 (isGiven인 셀은 cell_given 스프라이트)
    2. 선택된 셀이 없으면 (-1) 리턴
    3. 같은 행/열/박스의 셀에 SameGroup 상태 적용
    4. 선택된 셀과 같은 숫자가 표시된 셀에 SameNumber 배경 적용 (SameGroup과 동일 스프라이트 사용)
    5. 오류가 있는 셀에 Error 상태 적용 (HasError 기준)
    6. 선택된 셀 자체에 Selected 상태 적용 (마지막에 덮어쓰기)

  - 같은 3x3 박스 판별: `(r / 3 == selectedRow / 3) && (c / 3 == selectedCol / 3)`

## 수정할 파일
없음

## 검증 기준
- [ ] `dotnet build` 성공
- [ ] SudokuBoard.Initialize() 호출 시 boardPanel 아래에 81개의 셀 GameObject가 생성됨
- [ ] 셀 클릭 시 OnCellClicked 콜백이 올바른 (row, col)로 호출됨
- [ ] UpdateHighlights() 호출 시 선택 셀, 같은 그룹, 오류 셀의 배경이 변경됨

## 참고
- SudokuBoard와 SudokuCell은 MonoBehaviour가 아닌 일반 C# 클래스이다. MonoBehaviour는 SudokuGame만 해당한다 (Phase E).
- IronRose의 UIButton은 클릭 시 onClick Action을 호출한다. ImGui의 hit test를 사용하므로 별도의 raycast 설정이 불필요하다.
- UIImage의 imageType을 Sliced로 설정하면 9-slice 렌더링이 적용된다. sprite의 border 값이 필요하다.
- RectTransform의 좌표계는 Y-down이다. anchoredPosition의 Y가 증가하면 아래로 이동한다.
- 셀 크기는 BoardPanel의 sizeDelta(450)를 9로 나눈 50px이다. 격자선은 별도 GO로 만들지 않고 셀의 배경 스프라이트(cell_normal 등)의 테두리로 표현한다.
