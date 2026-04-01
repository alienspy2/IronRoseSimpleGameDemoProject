// ------------------------------------------------------------
// @file    CellState.cs
// @brief   스도쿠 셀의 시각적 상태를 나타내는 열거형. SudokuCell과 SudokuBoard에서
//          셀 배경 스프라이트 결정에 사용된다.
// @deps    없음
// @exports
//   enum CellState
//     Normal     -- 기본 상태
//     Selected   -- 현재 선택된 셀
//     SameGroup  -- 선택된 셀과 같은 행/열/박스
//     SameNumber -- 선택된 셀과 같은 숫자 (SameGroup과 동일 스프라이트 사용)
//     Error      -- 유저 입력이 정답과 다른 셀
// ------------------------------------------------------------

public enum CellState
{
    Normal,
    Selected,
    SameGroup,
    SameNumber,
    Error
}
