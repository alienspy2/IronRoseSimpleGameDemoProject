// ------------------------------------------------------------
// @file    SudokuGame.cs
// @brief   스도쿠 게임의 메인 컨트롤러. SimpleGameBase를 상속하여 씬에 부착된다.
//          퍼즐 생성, 보드/넘버패드 초기화, 키보드/버튼 입력 처리, 난이도 선택,
//          힌트/지우기, 클리어 판정 등 전체 게임 흐름을 관리한다.
// @deps    SimpleGameBase, SudokuPuzzle, SudokuBoard, NumberPad, SudokuCell, CellState, Difficulty,
//          RoseEngine (GameObject, Sprite, UIButton, UIImage, UIText, Input, KeyCode, Debug)
// @exports
//   class SudokuGame : SimpleGameBase
//     cellNormalSprite: Sprite?              -- 일반 셀 배경 스프라이트
//     cellGivenSprite: Sprite?               -- 고정 숫자 셀 배경 스프라이트
//     cellSelectedSprite: Sprite?            -- 선택된 셀 배경 스프라이트
//     cellSameGroupSprite: Sprite?           -- 같은 그룹 셀 배경 스프라이트
//     cellErrorSprite: Sprite?               -- 오류 셀 배경 스프라이트
//     btnDifficultySprite: Sprite?           -- 난이도 버튼 기본 스프라이트
//     btnDifficultyActiveSprite: Sprite?     -- 난이도 버튼 활성 스프라이트
//     override Start(): void                 -- 초기화: UI 요소 찾기, 보드/패드 생성, 이벤트 연결
//     override Update(): void                -- 키보드 입력 처리 (숫자 1~9, Delete, Backspace)
// @note    cellSprites 배열 순서: [0]=normal, [1]=given, [2]=selected, [3]=samegroup, [4]=error
//          SelectCell 후 반드시 UpdateHighlights를 별도 호출해야 한다.
//          GameObject.Find()로 씬 요소를 이름으로 검색한다 (Phase C에서 고유 이름 부여됨).
// ------------------------------------------------------------

using RoseEngine;

public class SudokuGame : SimpleGameBase
{
    // 스프라이트 링크 (인스펙터에서 설정)
    public Sprite? cellNormalSprite;
    public Sprite? cellGivenSprite;
    public Sprite? cellSelectedSprite;
    public Sprite? cellSameGroupSprite;
    public Sprite? cellErrorSprite;
    public Sprite? btnDifficultySprite;
    public Sprite? btnDifficultyActiveSprite;

    // 내부 상태
    private SudokuPuzzle? currentPuzzle;
    private SudokuBoard? board;
    private NumberPad? numberPad;
    private Difficulty currentDifficulty = Difficulty.Easy;

    // UI 참조
    private GameObject? messagePanel;
    private GameObject? messageText;
    private UIButton? easyBtn, mediumBtn, hardBtn;
    private UIImage? easyImg, mediumImg, hardImg;

    public override void Start()
    {
        // 1. UI 참조 찾기
        var boardPanel = GameObject.Find("BoardPanel");
        var numberPadPanel = GameObject.Find("NumberPadPanel");
        messagePanel = GameObject.Find("MessagePanel");
        messageText = GameObject.Find("MessageText");

        // 2. 스프라이트 배열 구성
        var cellSprites = new Sprite?[]
        {
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

    public override void Update()
    {
        // 숫자 키 1~9 (Alpha1~Alpha9)
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnNumberInput(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) OnNumberInput(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) OnNumberInput(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) OnNumberInput(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) OnNumberInput(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) OnNumberInput(6);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) OnNumberInput(7);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) OnNumberInput(8);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) OnNumberInput(9);

        // 키패드 숫자 1~9
        else if (Input.GetKeyDown(KeyCode.Keypad1)) OnNumberInput(1);
        else if (Input.GetKeyDown(KeyCode.Keypad2)) OnNumberInput(2);
        else if (Input.GetKeyDown(KeyCode.Keypad3)) OnNumberInput(3);
        else if (Input.GetKeyDown(KeyCode.Keypad4)) OnNumberInput(4);
        else if (Input.GetKeyDown(KeyCode.Keypad5)) OnNumberInput(5);
        else if (Input.GetKeyDown(KeyCode.Keypad6)) OnNumberInput(6);
        else if (Input.GetKeyDown(KeyCode.Keypad7)) OnNumberInput(7);
        else if (Input.GetKeyDown(KeyCode.Keypad8)) OnNumberInput(8);
        else if (Input.GetKeyDown(KeyCode.Keypad9)) OnNumberInput(9);

        // 지우기 (Delete, Backspace)
        else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            OnErase();
    }

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

        if (mediumGO != null)
        {
            mediumBtn = mediumGO.GetComponent<UIButton>();
            mediumImg = mediumGO.GetComponent<UIImage>();
            if (mediumBtn != null) mediumBtn.onClick = () => SetDifficulty(Difficulty.Medium);
        }

        if (hardGO != null)
        {
            hardBtn = hardGO.GetComponent<UIButton>();
            hardImg = hardGO.GetComponent<UIImage>();
            if (hardBtn != null) hardBtn.onClick = () => SetDifficulty(Difficulty.Hard);
        }

        // NewGame 버튼
        var newGameGO = GameObject.Find("NewGameButton");
        if (newGameGO != null)
        {
            var btn = newGameGO.GetComponent<UIButton>();
            if (btn != null) btn.onClick = StartNewGame;
        }
    }

    private void OnCellClicked(int row, int col)
    {
        if (board == null || currentPuzzle == null) return;
        board.SelectCell(row, col);
        board.UpdateHighlights(currentPuzzle);
    }

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

    private void OnErase()
    {
        if (board == null || currentPuzzle == null) return;
        if (board.SelectedRow < 0) return;
        currentPuzzle.ClearUserInput(board.SelectedRow, board.SelectedCol);
        board.UpdateDisplay(currentPuzzle);
        board.UpdateHighlights(currentPuzzle);
    }

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

    private void CheckCompletion()
    {
        if (currentPuzzle != null && currentPuzzle.IsComplete)
        {
            ShowMessage("Congratulations!");
            Debug.Log("[Sudoku] Puzzle completed!");
        }
    }

    private void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        UpdateDifficultyButtons();
        StartNewGame();
    }

    private void UpdateDifficultyButtons()
    {
        // 각 난이도 버튼의 UIImage sprite를 active/inactive로 전환
        if (easyImg != null)
            easyImg.sprite = currentDifficulty == Difficulty.Easy
                ? btnDifficultyActiveSprite : btnDifficultySprite;
        if (mediumImg != null)
            mediumImg.sprite = currentDifficulty == Difficulty.Medium
                ? btnDifficultyActiveSprite : btnDifficultySprite;
        if (hardImg != null)
            hardImg.sprite = currentDifficulty == Difficulty.Hard
                ? btnDifficultyActiveSprite : btnDifficultySprite;
    }

    private void StartNewGame()
    {
        HideMessage();
        board?.DestroyBoard();

        currentPuzzle = SudokuPuzzle.Generate(currentDifficulty);

        var cellSprites = new Sprite?[]
        {
            cellNormalSprite, cellGivenSprite, cellSelectedSprite,
            cellSameGroupSprite, cellErrorSprite
        };

        var boardPanel = GameObject.Find("BoardPanel");
        board?.Initialize(boardPanel, currentPuzzle, cellSprites);
    }

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
}
