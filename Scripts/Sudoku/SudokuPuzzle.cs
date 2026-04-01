// ------------------------------------------------------------
// @file    SudokuPuzzle.cs
// @brief   스도쿠 게임의 전체 상태를 담는 데이터 모델. 정답, 퍼즐, 유저 입력을 관리하며
//          셀 검증, 표시 숫자 조회, 완성 여부 판정 기능을 제공한다.
// @deps    SudokuGenerator, Difficulty
// @exports
//   class SudokuPuzzle
//     Solution: int[,]                           — 정답 배열 (9x9)
//     Puzzle: int[,]                             — 문제 배열 (0 = 빈칸)
//     UserInput: int[,]                          — 유저 입력 숫자 배열 (0 = 미입력)
//     IsGiven: bool[,]                           — 고정 숫자 여부 (true면 수정 불가)
//     CurrentDifficulty: Difficulty               — 현재 난이도
//     IsComplete: bool                           — 모든 칸이 정답으로 채워졌는지 여부
//     SetUserInput(int row, int col, int number)  — 유저 입력 설정 (IsGiven이면 무시)
//     ClearUserInput(int row, int col)            — 유저 입력 제거 (IsGiven이면 무시)
//     HasError(int row, int col): bool            — 해당 셀 유저 입력이 정답과 다른지 검사
//     GetDisplayNumber(int row, int col): int     — 표시할 숫자 반환 (0이면 빈칸)
//     static Generate(Difficulty): SudokuPuzzle   — 팩토리: 새 퍼즐 생성
// @note    Generate() 팩토리에서 SudokuGenerator로 보드를 생성하고 IsGiven을 설정한다.
//          IsComplete는 81칸 모두 GetDisplayNumber == Solution인지 확인한다.
// ------------------------------------------------------------

public class SudokuPuzzle
{
    private const int SIZE = 9;

    /// <summary>정답 배열 (9x9)</summary>
    public int[,] Solution { get; private set; }

    /// <summary>문제 배열 (0 = 빈칸)</summary>
    public int[,] Puzzle { get; private set; }

    /// <summary>유저가 입력한 숫자 배열 (0 = 미입력)</summary>
    public int[,] UserInput { get; private set; }

    /// <summary>고정 숫자 여부 (true면 수정 불가)</summary>
    public bool[,] IsGiven { get; private set; }

    /// <summary>현재 난이도</summary>
    public Difficulty CurrentDifficulty { get; private set; }

    /// <summary>모든 칸이 정답으로 채워졌는지 검사</summary>
    public bool IsComplete
    {
        get
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (GetDisplayNumber(row, col) != Solution[row, col])
                        return false;
                }
            }
            return true;
        }
    }

    private SudokuPuzzle(int[,] solution, int[,] puzzle, Difficulty difficulty)
    {
        Solution = solution;
        Puzzle = puzzle;
        CurrentDifficulty = difficulty;
        UserInput = new int[SIZE, SIZE];
        IsGiven = new bool[SIZE, SIZE];

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                IsGiven[row, col] = puzzle[row, col] != 0;
            }
        }
    }

    /// <summary>
    /// 유저 입력을 설정한다. IsGiven인 셀은 무시한다.
    /// </summary>
    public void SetUserInput(int row, int col, int number)
    {
        if (IsGiven[row, col])
            return;

        UserInput[row, col] = number;
    }

    /// <summary>
    /// 유저 입력을 지운다. IsGiven인 셀은 무시한다.
    /// </summary>
    public void ClearUserInput(int row, int col)
    {
        if (IsGiven[row, col])
            return;

        UserInput[row, col] = 0;
    }

    /// <summary>
    /// 해당 셀의 유저 입력이 정답과 다른지 검사한다.
    /// 빈 칸(0)이면 false를 반환한다.
    /// </summary>
    public bool HasError(int row, int col)
    {
        if (IsGiven[row, col])
            return false;

        int userValue = UserInput[row, col];
        if (userValue == 0)
            return false;

        return userValue != Solution[row, col];
    }

    /// <summary>
    /// 해당 셀에 표시할 숫자를 반환한다.
    /// IsGiven이면 Puzzle값, 아니면 UserInput값. 0이면 빈칸.
    /// </summary>
    public int GetDisplayNumber(int row, int col)
    {
        if (IsGiven[row, col])
            return Puzzle[row, col];

        return UserInput[row, col];
    }

    /// <summary>
    /// 보드에 표시된 특정 숫자의 개수를 반환한다 (given + user input).
    /// </summary>
    public int CountDisplayed(int number)
    {
        int count = 0;
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (GetDisplayNumber(row, col) == number)
                    count++;
            }
        }
        return count;
    }

    /// <summary>
    /// SudokuGenerator를 사용하여 새 퍼즐을 생성한다.
    /// </summary>
    public static SudokuPuzzle Generate(Difficulty difficulty)
    {
        var fullBoard = SudokuGenerator.GenerateFullBoard();
        var puzzle = SudokuGenerator.CreatePuzzle(fullBoard, difficulty);
        return new SudokuPuzzle(fullBoard, puzzle, difficulty);
    }
}
