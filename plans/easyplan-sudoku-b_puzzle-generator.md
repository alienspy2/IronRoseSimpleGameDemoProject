# Phase B: 스도쿠 퍼즐 생성기 및 데이터 모델

## 목표
- 스도쿠 퍼즐 생성 알고리즘(backtracking)을 구현한다.
- 게임 데이터 모델(`SudokuPuzzle`)을 정의한다.
- 순수 로직 클래스로, UI 의존 없이 독립적으로 동작한다.
- `dotnet build` 성공을 보장한다.

## 선행 조건
- 없음 (UI와 독립적인 순수 로직)

## 생성할 파일

### `Scripts/Sudoku/SudokuGenerator.cs`
- **역할**: backtracking 알고리즘으로 유효한 스도쿠 퍼즐을 생성한다.
- **클래스**: `SudokuGenerator` (static 클래스)
- **주요 멤버**:
  - `static int[,] GenerateFullBoard()` -- 완전히 채워진 유효한 9x9 스도쿠 보드를 생성한다. backtracking + 랜덤 셔플로 매번 다른 보드를 생성.
  - `static int[,] CreatePuzzle(int[,] fullBoard, Difficulty difficulty)` -- 완성 보드에서 난이도에 따라 셀을 제거하여 퍼즐을 만든다. 빈 칸은 0으로 표시.
  - `static bool IsValid(int[,] board, int row, int col, int num)` -- 특정 위치에 숫자를 놓을 수 있는지 검증 (행/열/3x3 박스 체크).
  - `private static bool SolveBoard(int[,] board)` -- backtracking으로 보드를 풀어본다. GenerateFullBoard 내부에서 사용.
- **의존**: `RoseEngine` (없음, 순수 C#)
- **구현 힌트**:
  - `GenerateFullBoard()` 구현:
    1. 빈 9x9 배열 생성
    2. 각 빈 셀에 1~9을 랜덤 순서로 시도
    3. `IsValid()` 통과하면 배치 후 다음 셀로 재귀
    4. 실패 시 backtrack (셀을 0으로 되돌림)
    5. `System.Random`을 사용하여 숫자 순서를 셔플
  - `CreatePuzzle()` 구현:
    1. 완성 보드를 복사
    2. 난이도별 제거 셀 수: Easy=35, Medium=45, Hard=55
    3. 랜덤 위치의 셀을 0으로 설정
    4. 모든 위치를 랜덤 순서로 순회하며 제거
  - `IsValid()` 구현:
    1. 같은 행에 같은 숫자 있는지 체크
    2. 같은 열에 같은 숫자 있는지 체크
    3. 같은 3x3 박스에 같은 숫자 있는지 체크 (박스 시작: `row/3*3`, `col/3*3`)

### `Scripts/Sudoku/SudokuPuzzle.cs`
- **역할**: 스도쿠 게임의 전체 상태를 담는 데이터 모델이다.
- **클래스**: `SudokuPuzzle`
- **주요 멤버**:
  - `int[,] Solution` -- 정답 배열 (9x9)
  - `int[,] Puzzle` -- 문제 배열 (0 = 빈칸)
  - `int[,] UserInput` -- 유저가 입력한 숫자 배열 (0 = 미입력)
  - `bool[,] IsGiven` -- 고정 숫자 여부 (true면 수정 불가)
  - `Difficulty CurrentDifficulty` -- 현재 난이도
  - `bool IsComplete` (프로퍼티) -- 모든 칸이 정답으로 채워졌는지 검사
  - `void SetUserInput(int row, int col, int number)` -- 유저 입력을 설정한다. IsGiven인 셀은 무시.
  - `void ClearUserInput(int row, int col)` -- 유저 입력을 지운다. IsGiven인 셀은 무시.
  - `bool HasError(int row, int col)` -- 해당 셀의 유저 입력이 정답과 다른지 검사. 빈 칸이면 false.
  - `int GetDisplayNumber(int row, int col)` -- 해당 셀에 표시할 숫자를 반환. IsGiven이면 Puzzle값, 아니면 UserInput값. 0이면 빈칸.
  - `static SudokuPuzzle Generate(Difficulty difficulty)` -- SudokuGenerator를 사용하여 새 퍼즐을 생성한다.
- **의존**: `SudokuGenerator`
- **구현 힌트**:
  - `Generate()` 팩토리 메서드에서:
    1. `SudokuGenerator.GenerateFullBoard()`로 완성 보드 생성
    2. `SudokuGenerator.CreatePuzzle()`로 문제 생성
    3. `IsGiven[r,c] = (puzzle[r,c] != 0)`으로 고정 셀 표시
    4. UserInput은 모두 0으로 초기화
  - `IsComplete` 프로퍼티: 모든 81칸을 순회하며 `GetDisplayNumber(r,c) == Solution[r,c]`인지 확인

### `Scripts/Sudoku/Difficulty.cs`
- **역할**: 난이도 열거형 정의
- **열거형**: `Difficulty`
- **값**: `Easy`, `Medium`, `Hard`
- **의존**: 없음

## 검증 기준
- [ ] `dotnet build` 성공 (경로: `/home/alienspy/git/IronRoseSimpleGameDemoProject/Scripts/`)
- [ ] `SudokuGenerator.GenerateFullBoard()`가 유효한 9x9 스도쿠 보드를 반환
- [ ] `SudokuPuzzle.Generate(Difficulty.Easy)`로 퍼즐 생성 시 빈칸이 약 35개

## 참고
- `using RoseEngine;`은 이 phase에서 불필요하지만, 추후 phase에서 MonoBehaviour 기반 스크립트와 같은 프로젝트에 있으므로 네임스페이스 충돌에 주의한다.
- `Scripts/Scripts.csproj`가 SDK-style이므로 `Scripts/` 하위의 모든 `.cs` 파일이 자동으로 컴파일 대상에 포함된다.
- `Scripts/Sudoku/` 폴더를 새로 생성해야 한다.
