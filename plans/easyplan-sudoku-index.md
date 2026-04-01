# Easyplan Sudoku - Phase 구현 명세서 인덱스

## 개요
IronRose 엔진의 UI 시스템을 사용한 스도쿠 퍼즐 게임. 노트/종이 느낌의 아날로그 감성 UI.

## Phase 목록

| Phase | 파일 | 제목 | 의존 | 주요 산출물 |
|-------|------|------|------|------------|
| A | `easyplan-sudoku-a_sprite-assets.md` | 스프라이트 에셋 생성 | 없음 | 19개 PNG 스프라이트 |
| B | `easyplan-sudoku-b_puzzle-generator.md` | 퍼즐 생성기 및 데이터 모델 | 없음 | SudokuGenerator, SudokuPuzzle, Difficulty |
| C | `easyplan-sudoku-c_scene-ui-setup.md` | 씬 UI 계층 구조 구성 | A | sudoku.scene Canvas UI |
| D | `easyplan-sudoku-d_board-ui-scripts.md` | 보드 UI 스크립트 | B, C | SudokuBoard, SudokuCell, CellState |
| E | `easyplan-sudoku-e_game-controller.md` | 게임 컨트롤러 통합 | A, B, C, D | SudokuGame, NumberPad |

## 의존 관계 다이어그램

```
Phase A (스프라이트)     Phase B (로직)
     │                      │
     └──────┬───────────────┘
            │
     Phase C (씬 UI)
            │
     Phase D (보드 UI)
            │
     Phase E (통합)
```

## 실행 순서

1. Phase A와 Phase B는 병렬 실행 가능
2. Phase C는 Phase A 완료 후 실행
3. Phase D는 Phase B, C 완료 후 실행
4. Phase E는 Phase D 완료 후 실행

## 파일 구조 (최종)

```
IronRoseSimpleGameDemoProject/
├── Assets/
│   └── Sudoku/
│       └── Sprites/
│           ├── bg_notebook.png
│           ├── board_bg.png
│           ├── cell_normal.png
│           ├── cell_given.png
│           ├── cell_selected.png
│           ├── cell_samegroup.png
│           ├── cell_error.png
│           ├── grid_line_thin.png
│           ├── grid_line_thick.png
│           ├── btn_number.png
│           ├── btn_number_pressed.png
│           ├── btn_action.png
│           ├── btn_action_pressed.png
│           ├── btn_difficulty.png
│           ├── btn_difficulty_active.png
│           ├── icon_hint.png
│           ├── icon_erase.png
│           ├── icon_newgame.png
│           └── title_sudoku.png
├── Scripts/
│   └── Sudoku/
│       ├── CellState.cs
│       ├── Difficulty.cs
│       ├── NumberPad.cs
│       ├── SudokuBoard.cs
│       ├── SudokuCell.cs
│       ├── SudokuGame.cs
│       ├── SudokuGenerator.cs
│       └── SudokuPuzzle.cs
└── Assets/Scenes/SimpleGameDemo/
    └── sudoku.scene (수정됨)
```
