# Phase E: 게임 컨트롤러 통합 (SudokuGame, NumberPad)

## 목표
- 메인 게임 컨트롤러 `SudokuGame`(MonoBehaviour)을 작성하여 모든 시스템을 통합한다.
- 하단 숫자 패드 `NumberPad`를 구현한다.
- 키보드 입력(1~9, Delete/Backspace)을 처리한다.
- 난이도 선택, 새 게임, 힌트, 지우기, 클리어 판정을 구현한다.
- 씬에 SudokuGame 컴포넌트를 연결한다.
- 완전히 플레이 가능한 스도쿠 게임이 된다.

## 선행 조건
- Phase B 완료 (SudokuPuzzle, SudokuGenerator, Difficulty)
- Phase C 완료 (씬 UI 구조)
- Phase D 완료 (SudokuBoard, SudokuCell, CellState)
- Phase A 완료 (스프라이트 에셋)

## 생성할 파일

### `Scripts/Sudoku/NumberPad.cs`
- **역할**: 하단 숫자 패드(1~9), 지우기, 힌트 버튼의 클릭 이벤트를 관리한다.
- **클래스**: `NumberPad` (일반 클래스)
- **주요 멤버**:
  - `Action<int>? OnNumberClicked` -- 숫자 버튼 클릭 콜백 (1~9)
  - `Action? OnEraseClicked` -- 지우기 버튼 클릭 콜백
  - `Action? OnHintClicked` -- 힌트 버튼 클릭 콜백
  - `void Initialize(GameObject numberPadPanel)` -- NumberPadPanel 아래의 버튼들에 onClick 핸들러를 연결한다.
- **의존**: `RoseEngine` (GameObject, UIButton)
- **구현 힌트**:
  - `Initialize()` 구현:
    ```csharp
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
    ```
  - `FindChildByName` 헬퍼: Transform의 자식을 순회하며 이름으로 찾는다.
    ```csharp
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
    ```

### `Scripts/Sudoku/SudokuGame.cs`
- **역할**: 스도쿠 게임의 메인 컨트롤러. MonoBehaviour로 씬에 부착된다. 퍼즐 생성, 입력 처리, UI 갱신, 게임 흐름을 관리한다.
- **클래스**: `SudokuGame : SimpleGameBase`
- **주요 멤버**:
  - `// 스프라이트 링크 (인스펙터에서 설정)`
  - `public Sprite? cellNormalSprite;`
  - `public Sprite? cellGivenSprite;`
  - `public Sprite? cellSelectedSprite;`
  - `public Sprite? cellSameGroupSprite;`
  - `public Sprite? cellErrorSprite;`
  - `public Sprite? btnDifficultySprite;`
  - `public Sprite? btnDifficultyActiveSprite;`
  - `// 내부 상태`
  - `private SudokuPuzzle? currentPuzzle;`
  - `private SudokuBoard? board;`
  - `private NumberPad? numberPad;`
  - `private Difficulty currentDifficulty = Difficulty.Easy;`
  - `// UI 참조`
  - `private GameObject? messagePanel;`
  - `private GameObject? messageText;`
  - `private UIButton? easyBtn, mediumBtn, hardBtn;`
  - `private UIImage? easyImg, mediumImg, hardImg;`
  - `override void Start()` -- 초기화: UI 요소 찾기, 보드 생성, 이벤트 연결, 첫 게임 시작
  - `override void Update()` -- 키보드 입력 처리
  - `private void StartNewGame()` -- 새 퍼즐 생성 및 보드 갱신
  - `private void OnCellClicked(int row, int col)` -- 셀 클릭 처리
  - `private void OnNumberInput(int number)` -- 숫자 입력 처리 (패드 클릭 또는 키보드)
  - `private void OnErase()` -- 선택 셀 지우기
  - `private void OnHint()` -- 선택 셀에 정답 표시
  - `private void CheckCompletion()` -- 퍼즐 완료 판정
  - `private void SetDifficulty(Difficulty diff)` -- 난이도 변경 및 UI 갱신
  - `private void UpdateDifficultyButtons()` -- 난이도 버튼 스프라이트 갱신
  - `private void ShowMessage(string msg)` -- 메시지 표시
  - `private void HideMessage()` -- 메시지 숨기기
- **의존**: `RoseEngine`, `SudokuPuzzle`, `SudokuBoard`, `NumberPad`, `SudokuCell`, `CellState`, `Difficulty`
- **구현 힌트**:

  - **Start() 구현**:
    ```csharp
    public override void Start()
    {
        // 1. UI 참조 찾기
        var boardPanel = GameObject.Find("BoardPanel");
        var numberPadPanel = GameObject.Find("NumberPadPanel");
        messagePanel = GameObject.Find("MessagePanel");
        messageText = GameObject.Find("MessageText");

        // 2. 스프라이트 배열 구성
        var cellSprites = new Sprite?[] {
            cellNormalSprite, cellGivenSprite, cellSelectedSprite,
            cellSameGroupSprite, cellErrorSprite
        };

        // 3. SudokuBoard 초기화
        board = new SudokuBoard();
        currentPuzzle = SudokuPuzzle.Generate(currentDifficulty);
        board.OnCellClicked = OnCellClicked;
        board.Initialize(boardPanel, currentPuzzle, cellSprites);

        // 4. NumberPad 초기화
        numberPad = new NumberPad();
        numberPad.OnNumberClicked = OnNumberInput;
        numberPad.OnEraseClicked = OnErase;
        numberPad.OnHintClicked = OnHint;
        numberPad.Initialize(numberPadPanel);

        // 5. 헤더 버튼 연결
        SetupHeaderButtons();

        // 6. 난이도 버튼 초기 상태
        UpdateDifficultyButtons();

        // 7. 메시지 패널 숨기기
        if (messagePanel != null) messagePanel.SetActive(false);
    }
    ```

  - **Update() -- 키보드 입력 처리**:
    ```csharp
    public override void Update()
    {
        // 숫자 키 1~9 (Alpha1~Alpha9)
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnNumberInput(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) OnNumberInput(2);
        // ... Alpha3~Alpha9 동일 패턴
        else if (Input.GetKeyDown(KeyCode.Alpha9)) OnNumberInput(9);

        // 키패드 숫자 1~9
        else if (Input.GetKeyDown(KeyCode.Keypad1)) OnNumberInput(1);
        // ... Keypad2~Keypad9 동일 패턴

        // 지우기 (Delete, Backspace)
        else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            OnErase();
    }
    ```

  - **SetupHeaderButtons() 구현**:
    ```csharp
    private void SetupHeaderButtons()
    {
        // 난이도 버튼
        var easyGO = GameObject.Find("EasyButton");
        var mediumGO = GameObject.Find("MediumButton");
        var hardGO = GameObject.Find("HardButton");

        if (easyGO != null)
        {
            easyBtn = easyGO.GetComponent<UIButton>();
            easyImg = easyGO.GetComponent<UIImage>();
            if (easyBtn != null) easyBtn.onClick = () => SetDifficulty(Difficulty.Easy);
        }
        // mediumBtn, hardBtn 동일 패턴

        // NewGame 버튼
        var newGameGO = GameObject.Find("NewGameButton");
        if (newGameGO != null)
        {
            var btn = newGameGO.GetComponent<UIButton>();
            if (btn != null) btn.onClick = StartNewGame;
        }
    }
    ```

  - **OnCellClicked(row, col) 구현**:
    ```csharp
    private void OnCellClicked(int row, int col)
    {
        if (board == null || currentPuzzle == null) return;
        board.SelectCell(row, col);
        board.UpdateHighlights(currentPuzzle);
    }
    ```

  - **OnNumberInput(number) 구현**:
    ```csharp
    private void OnNumberInput(int number)
    {
        if (board == null || currentPuzzle == null) return;
        if (board.SelectedRow < 0 || board.SelectedCol < 0) return;

        int row = board.SelectedRow;
        int col = board.SelectedCol;

        // 고정 숫자 셀은 입력 불가
        if (currentPuzzle.IsGiven[row, col]) return;

        currentPuzzle.SetUserInput(row, col, number);
        board.UpdateDisplay(currentPuzzle);
        board.UpdateHighlights(currentPuzzle);
        CheckCompletion();
    }
    ```

  - **OnErase() 구현**:
    ```csharp
    private void OnErase()
    {
        if (board == null || currentPuzzle == null) return;
        if (board.SelectedRow < 0) return;
        currentPuzzle.ClearUserInput(board.SelectedRow, board.SelectedCol);
        board.UpdateDisplay(currentPuzzle);
        board.UpdateHighlights(currentPuzzle);
    }
    ```

  - **OnHint() 구현**:
    ```csharp
    private void OnHint()
    {
        if (board == null || currentPuzzle == null) return;
        if (board.SelectedRow < 0) return;
        int row = board.SelectedRow;
        int col = board.SelectedCol;
        if (currentPuzzle.IsGiven[row, col]) return;

        // 정답을 유저 입력에 설정
        int answer = currentPuzzle.Solution[row, col];
        currentPuzzle.SetUserInput(row, col, answer);
        board.UpdateDisplay(currentPuzzle);
        board.UpdateHighlights(currentPuzzle);
        CheckCompletion();
    }
    ```

  - **SetDifficulty(diff) 구현**:
    ```csharp
    private void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        UpdateDifficultyButtons();
        StartNewGame();
    }
    ```

  - **UpdateDifficultyButtons() 구현**:
    ```csharp
    private void UpdateDifficultyButtons()
    {
        // 각 난이도 버튼의 UIImage sprite를 active/inactive로 전환
        if (easyImg != null)
            easyImg.sprite = currentDifficulty == Difficulty.Easy
                ? btnDifficultyActiveSprite : btnDifficultySprite;
        // mediumImg, hardImg 동일 패턴
    }
    ```

  - **StartNewGame() 구현**:
    ```csharp
    private void StartNewGame()
    {
        HideMessage();
        board?.DestroyBoard();

        currentPuzzle = SudokuPuzzle.Generate(currentDifficulty);

        var cellSprites = new Sprite?[] {
            cellNormalSprite, cellGivenSprite, cellSelectedSprite,
            cellSameGroupSprite, cellErrorSprite
        };

        var boardPanel = GameObject.Find("BoardPanel");
        board?.Initialize(boardPanel, currentPuzzle, cellSprites);
    }
    ```

  - **CheckCompletion() 구현**:
    ```csharp
    private void CheckCompletion()
    {
        if (currentPuzzle != null && currentPuzzle.IsComplete)
        {
            ShowMessage("Congratulations!");
            Debug.Log("[Sudoku] Puzzle completed!");
        }
    }
    ```

  - **ShowMessage / HideMessage 구현**:
    ```csharp
    private void ShowMessage(string msg)
    {
        if (messagePanel != null) messagePanel.SetActive(true);
        if (messageText != null)
        {
            var text = messageText.GetComponent<UIText>();
            if (text != null) text.text = msg;
        }
    }

    private void HideMessage()
    {
        if (messagePanel != null) messagePanel.SetActive(false);
    }
    ```

## 수정할 씬 (rose-cli 사용)

### `Assets/Scenes/SimpleGameDemo/sudoku.scene`

SudokuGame 컴포넌트를 씬에 추가해야 한다:

1. 새 GameObject "SudokuGameController"를 씬 루트에 추가
2. `SudokuGame` 스크립트 컴포넌트를 부착
3. 스프라이트 필드를 에셋 guid로 연결:
   - `cellNormalSprite` -> `Assets/Sudoku/Sprites/cell_normal.png`
   - `cellGivenSprite` -> `Assets/Sudoku/Sprites/cell_given.png`
   - `cellSelectedSprite` -> `Assets/Sudoku/Sprites/cell_selected.png`
   - `cellSameGroupSprite` -> `Assets/Sudoku/Sprites/cell_samegroup.png`
   - `cellErrorSprite` -> `Assets/Sudoku/Sprites/cell_error.png`
   - `btnDifficultySprite` -> `Assets/Sudoku/Sprites/btn_difficulty.png`
   - `btnDifficultyActiveSprite` -> `Assets/Sudoku/Sprites/btn_difficulty_active.png`

## 검증 기준
- [ ] `dotnet build` 성공
- [ ] 게임 실행 시 9x9 보드가 화면 중앙에 표시됨
- [ ] 셀 클릭 시 해당 셀이 파란색으로 하이라이트됨
- [ ] 같은 행/열/박스의 셀이 노란색으로 하이라이트됨
- [ ] 숫자 패드 클릭 또는 키보드 1~9로 숫자가 입력됨
- [ ] 고정 숫자(문제에 주어진)는 진한 남색, 유저 입력은 연한 갈색으로 표시
- [ ] 오답 입력 시 셀이 빨간 배경으로 표시됨
- [ ] 지우기 버튼으로 입력한 숫자가 삭제됨
- [ ] 힌트 버튼으로 정답이 자동 입력됨
- [ ] Easy/Medium/Hard 버튼으로 난이도 변경 및 새 게임 시작
- [ ] New Game 버튼으로 새 퍼즐 시작
- [ ] 모든 칸을 올바르게 채우면 "Congratulations!" 메시지 표시

## 참고
- `GameObject.Find()`는 씬 전체에서 이름으로 검색한다. 이름이 유일해야 한다. Phase C에서 고유 이름을 부여했으므로 충돌 없어야 한다.
- UIButton의 transition이 `ColorTint`일 경우 UIImage의 color를 직접 조작하므로, 스프라이트 교체 로직과 충돌할 수 있다. 난이도 버튼은 `SpriteSwap` transition을 사용하거나, transition을 `ColorTint`로 두고 스프라이트 교체는 onClick 핸들러에서 직접 처리한다.
- IronRose의 Input 클래스에서 키코드: `KeyCode.Alpha1`~`KeyCode.Alpha9`, `KeyCode.Keypad1`~`KeyCode.Keypad9`, `KeyCode.Delete`, `KeyCode.Backspace`를 사용한다.
- SudokuGame의 public Sprite 필드들은 씬 파일에서 에셋 guid로 직렬화된다. 인스펙터 링크 패턴은 AngryClawd의 `pigIconSpritePrefab` 참조 방식과 동일하다.
- `SudokuGame`은 `SimpleGameBase`를 상속하며 `Start()`와 `Update()`를 `override`한다 (`SimpleGameBase`는 `MonoBehaviour`를 상속한 공통 베이스 클래스이다, `Scripts/SimpleGameBase.cs` 참고).
