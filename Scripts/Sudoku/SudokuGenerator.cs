// ------------------------------------------------------------
// @file    SudokuGenerator.cs
// @brief   Backtracking 알고리즘으로 유효한 9x9 스도쿠 퍼즐을 생성하는 static 유틸리티 클래스.
//          완전한 보드를 생성한 뒤 난이도에 따라 셀을 제거하여 퍼즐을 만든다.
// @deps    Difficulty
// @exports
//   static class SudokuGenerator
//     static GenerateFullBoard(): int[,]                          — 완전히 채워진 유효한 9x9 보드 생성
//     static CreatePuzzle(int[,] fullBoard, Difficulty): int[,]   — 난이도별 셀 제거로 퍼즐 생성
//     static IsValid(int[,] board, int row, int col, int num): bool — 행/열/3x3 박스 유효성 검증
// @note    GenerateFullBoard()는 System.Random으로 숫자 순서를 셔플하여 매번 다른 보드를 생성한다.
//          제거 셀 수: Easy=35, Medium=45, Hard=55.
// ------------------------------------------------------------

using System;

public static class SudokuGenerator
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    private static readonly Random _random = new();

    /// <summary>
    /// 완전히 채워진 유효한 9x9 스도쿠 보드를 생성한다.
    /// backtracking + 랜덤 셔플로 매번 다른 보드를 생성한다.
    /// </summary>
    public static int[,] GenerateFullBoard()
    {
        var board = new int[SIZE, SIZE];
        SolveBoard(board);
        return board;
    }

    /// <summary>
    /// 완성 보드에서 난이도에 따라 셀을 제거하여 퍼즐을 만든다.
    /// 빈 칸은 0으로 표시된다.
    /// </summary>
    public static int[,] CreatePuzzle(int[,] fullBoard, Difficulty difficulty)
    {
        var puzzle = new int[SIZE, SIZE];
        Array.Copy(fullBoard, puzzle, fullBoard.Length);

        int cellsToRemove = difficulty switch
        {
            Difficulty.Easy => 35,
            Difficulty.Medium => 45,
            Difficulty.Hard => 55,
            _ => 35
        };

        // 모든 셀 위치를 랜덤 순서로 순회
        var positions = new int[SIZE * SIZE];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = i;
        }
        Shuffle(positions);

        int removed = 0;
        for (int i = 0; i < positions.Length && removed < cellsToRemove; i++)
        {
            int row = positions[i] / SIZE;
            int col = positions[i] % SIZE;

            if (puzzle[row, col] != 0)
            {
                puzzle[row, col] = 0;
                removed++;
            }
        }

        return puzzle;
    }

    /// <summary>
    /// 특정 위치에 숫자를 놓을 수 있는지 검증한다 (행/열/3x3 박스 체크).
    /// </summary>
    public static bool IsValid(int[,] board, int row, int col, int num)
    {
        // 같은 행에 같은 숫자가 있는지 체크
        for (int c = 0; c < SIZE; c++)
        {
            if (board[row, c] == num)
                return false;
        }

        // 같은 열에 같은 숫자가 있는지 체크
        for (int r = 0; r < SIZE; r++)
        {
            if (board[r, col] == num)
                return false;
        }

        // 같은 3x3 박스에 같은 숫자가 있는지 체크
        int boxRowStart = row / BOX_SIZE * BOX_SIZE;
        int boxColStart = col / BOX_SIZE * BOX_SIZE;
        for (int r = boxRowStart; r < boxRowStart + BOX_SIZE; r++)
        {
            for (int c = boxColStart; c < boxColStart + BOX_SIZE; c++)
            {
                if (board[r, c] == num)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Backtracking으로 보드를 풀어본다. GenerateFullBoard 내부에서 사용.
    /// </summary>
    private static bool SolveBoard(int[,] board)
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (board[row, col] != 0)
                    continue;

                // 1~9를 랜덤 순서로 시도
                var numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                Shuffle(numbers);

                foreach (int num in numbers)
                {
                    if (IsValid(board, row, col, num))
                    {
                        board[row, col] = num;

                        if (SolveBoard(board))
                            return true;

                        // 실패 시 backtrack
                        board[row, col] = 0;
                    }
                }

                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Fisher-Yates 셔플 알고리즘으로 배열을 랜덤하게 섞는다.
    /// </summary>
    private static void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
