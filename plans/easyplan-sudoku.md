# Easyplan: 스도쿠 게임 (IronRose UI)

## 개요
IronRose 엔진의 UI 시스템을 활용한 클래식 스도쿠 퍼즐 게임. 노트/종이 느낌의 아날로그 감성 UI로, 연필로 수첩에 풀어보는 듯한 분위기를 제공한다. 현재 열린 `sudoku` 씬에 구현.

## 대상 사용자
IronRose 엔진의 UI 시스템 데모 및 캐주얼 퍼즐 게임 사용자.

## 핵심 기능
1. **9x9 스도쿠 보드** — 화면 중앙 70% 차지, 3x3 박스 구분 가능한 그리드
2. **난이도 선택** — Easy / Medium / Hard 3단계
3. **숫자 입력 (듀얼)** — 하단 숫자 패널(1~9) 클릭 + 키보드 1~9 입력
4. **힌트 기능** — 선택한 빈 칸의 정답을 알려주는 버튼
5. **셀 하이라이트** — 선택한 셀, 같은 행/열/박스, 같은 숫자, 오류 표시
6. **New Game** — 새 게임 시작 버튼
7. **지우기** — 입력한 숫자를 삭제하는 기능

## 기술 스택
| 영역 | 선택 | 이유 |
|---|---|---|
| 엔진 | IronRose Engine | 프로젝트 엔진 |
| 언어 | C# | IronRose 스크립트 언어 |
| UI | IronRose UI System | Canvas, Panel, Text, Image, Button 등 |
| 스프라이트 | image-forge 스킬 | SVG → PNG 변환 |

## 아키텍처 개요

```
SudokuScene
├── Canvas (StretchAll)
│   ├── Background (전체 배경 - 노트 종이 텍스처)
│   ├── Header Panel
│   │   ├── Title Text ("Sudoku")
│   │   ├── Difficulty Buttons (Easy / Medium / Hard)
│   │   └── New Game Button
│   ├── Board Panel (중앙, 정사각형)
│   │   ├── Grid Background (격자 배경)
│   │   └── Cells[9x9] (각 셀 = Button + Text)
│   │       ├── Given Number (진한 펜 느낌)
│   │       └── User Input (연필 느낌)
│   ├── Number Pad Panel (하단)
│   │   ├── Number Buttons (1~9)
│   │   ├── Erase Button
│   │   └── Hint Button
│   └── Status Panel
│       └── Timer / Message Text
```

### 스크립트 구조
- **SudokuGame.cs** — 게임 로직 (퍼즐 생성, 정답 검증, 난이도별 빈칸 수)
- **SudokuBoard.cs** — 보드 UI 관리 (셀 생성, 하이라이트, 숫자 표시)
- **SudokuCell.cs** — 개별 셀 동작 (선택, 입력, 상태 관리)
- **SudokuGenerator.cs** — 스도쿠 퍼즐 생성 알고리즘 (backtracking)
- **NumberPad.cs** — 하단 숫자 패널 관리

## 데이터 모델
```
SudokuPuzzle:
  - solution[9,9]: int       // 정답 배열
  - puzzle[9,9]: int          // 문제 (0 = 빈칸)
  - userInput[9,9]: int       // 유저 입력
  - isGiven[9,9]: bool        // 고정 숫자 여부
  - difficulty: Easy|Medium|Hard

CellState:
  - Normal, Selected, SameGroup, SameNumber, Error
```

## UI/UX 방향

### 비주얼 테마: 노트/종이 느낌
- **배경**: 크림색/아이보리 노트 종이 텍스처, 미세한 줄무늬
- **셀**: 연한 격자 무늬, 선택 시 연필 동그라미 느낌의 하이라이트
- **고정 숫자**: 진한 잉크(펜) 느낌 — 볼드, 짙은 남색
- **유저 입력 숫자**: 연필 느낌 — 약간 연한 회색-갈색
- **오류 표시**: 빨간 밑줄 또는 연한 빨간 배경
- **버튼**: 둥근 모서리의 종이 스티커/태그 느낌

### 사용 흐름
1. 게임 시작 → 기본 난이도(Easy)로 퍼즐 자동 생성
2. 빈 셀 클릭 → 셀 선택 (하이라이트)
3. 하단 숫자 패널 클릭 또는 키보드 1~9 → 숫자 입력
4. 오류 시 → 셀 빨간 표시
5. 힌트 버튼 → 선택한 셀에 정답 표시
6. 모든 칸 채우면 → 클리어 메시지
7. New Game → 난이도 유지한 채 새 퍼즐

## 제약조건 및 고려사항
- IronRose UI 시스템의 기능 범위 내에서 구현
- 스프라이트는 image-forge(SVG→PNG)로 생성, 9-slice 지원 필요한 것은 별도 표기
- 키보드 입력은 IronRose의 Input 시스템 사용

## MVP 범위

### 포함
- 9x9 스도쿠 보드 UI
- 퍼즐 생성 (backtracking 알고리즘)
- Easy/Medium/Hard 난이도
- 하단 숫자 패널 + 키보드 입력
- 셀 하이라이트 (선택, 같은 그룹, 오류)
- 힌트 버튼
- New Game 버튼
- 지우기 버튼

### 제외 (향후 확장)
- 메모 기능 (후보 숫자 작게 적기)
- 타이머
- 실행취소(Undo)
- 기록 저장 / 리더보드
- 애니메이션 효과

---

## 스프라이트 목록 및 image-forge 프롬프트

모든 스프라이트는 `Assets/Sudoku/Sprites/` 폴더에 저장.
크기는 넉넉하게 크게 생성하여 축소 시에도 선명하게.

### 1. 배경 (Background)

| # | 파일명 | 크기 | 9-slice | 설명 |
|---|--------|------|---------|------|
| 1 | `bg_notebook.png` | 1024x1024 | X | 전체 화면 배경 — 노트 종이 텍스처 |

**image-forge 프롬프트:**
> 크림색/아이보리 노트 종이 텍스처. 미세한 가로줄이 은은하게 보이고, 종이 질감이 느껴지는 따뜻한 배경. 모서리 부근에 약간의 그림자/얼룩으로 빈티지 느낌. 타일링 가능하게. 크기: 1024x1024

### 2. 보드/그리드 (Board & Grid)

| # | 파일명 | 크기 | 9-slice | 설명 |
|---|--------|------|---------|------|
| 2 | `board_bg.png` | 512x512 | O (border 16) | 스도쿠 보드 전체 배경 — 약간 어두운 크림색 |
| 3 | `cell_normal.png` | 128x128 | O (border 8) | 일반 빈 셀 배경 |
| 4 | `cell_given.png` | 128x128 | O (border 8) | 고정 숫자(문제에 주어진) 셀 배경 — 약간 더 진한 종이색 |
| 5 | `cell_selected.png` | 128x128 | O (border 8) | 선택된 셀 — 연필 동그라미/하이라이트 |
| 6 | `cell_samegroup.png` | 128x128 | O (border 8) | 같은 행/열/박스 셀 — 연한 노란 하이라이트 |
| 7 | `cell_error.png` | 128x128 | O (border 8) | 오류 셀 — 연한 빨간 배경 |
| 8 | `grid_line_thin.png` | 4x128 | X | 셀 사이 얇은 격자선 |
| 9 | `grid_line_thick.png` | 8x128 | X | 3x3 박스 사이 두꺼운 격자선 |

**image-forge 프롬프트:**

> **board_bg**: 스도쿠 보드 배경. 약간 어두운 크림/베이지색 사각형. 미세한 종이 질감. 연한 그림자 테두리로 살짝 입체감. 9-slice용으로 모서리/테두리/중앙 영역 구분. 크기: 512x512

> **cell_normal**: 빈 셀 배경. 밝은 아이보리/흰색 정사각형. 아주 얇은 연회색 테두리. 깨끗한 종이 느낌. 9-slice 가능. 크기: 128x128

> **cell_given**: 고정 숫자 셀 배경. cell_normal보다 약간 진한 크림/연갈색. 잉크가 스며든 듯한 느낌. 미세한 종이 질감. 9-slice 가능. 크기: 128x128

> **cell_selected**: 선택 셀 하이라이트. 연한 하늘색/파란색 배경. 마치 연필로 살짝 칠한 듯한 부드러운 색감. 중앙이 약간 더 진하고 가장자리가 페이드. 9-slice 가능. 크기: 128x128

> **cell_samegroup**: 같은 그룹 셀 하이라이트. 매우 연한 노란색/레몬색 배경. 형광펜으로 살짝 칠한 느낌. 투명감 있게. 9-slice 가능. 크기: 128x128

> **cell_error**: 오류 셀 배경. 연한 빨간색/살몬색 배경. 빨간 펜으로 밑줄 친 듯한 느낌. 9-slice 가능. 크기: 128x128

> **grid_line_thin**: 셀 사이 얇은 격자선. 연한 회색(#CCCCCC). 연필 선처럼 약간 불규칙한 느낌. 세로 방향. 크기: 4x128

> **grid_line_thick**: 3x3 박스 경계 두꺼운 격자선. 진한 갈색/다크그레이(#555555). 펜으로 그은 듯한 진한 선. 세로 방향. 크기: 8x128

### 3. 버튼 (Buttons)

| # | 파일명 | 크기 | 9-slice | 설명 |
|---|--------|------|---------|------|
| 10 | `btn_number.png` | 128x128 | O (border 12) | 숫자 패드 버튼 (1~9) 배경 |
| 11 | `btn_number_pressed.png` | 128x128 | O (border 12) | 숫자 패드 버튼 눌림 상태 |
| 12 | `btn_action.png` | 256x96 | O (border 16) | 액션 버튼 (New Game, Hint, Erase) 배경 |
| 13 | `btn_action_pressed.png` | 256x96 | O (border 16) | 액션 버튼 눌림 상태 |
| 14 | `btn_difficulty.png` | 192x64 | O (border 12) | 난이도 버튼 (비선택) |
| 15 | `btn_difficulty_active.png` | 192x64 | O (border 12) | 난이도 버튼 (선택됨) |

**image-forge 프롬프트:**

> **btn_number**: 숫자 패드용 정사각형 버튼. 둥근 모서리(radius 12). 밝은 크림색 배경에 얇은 갈색 테두리. 종이 스티커/태그 느낌. 약간의 그림자로 입체감. 9-slice 가능. 크기: 128x128

> **btn_number_pressed**: 숫자 패드 눌림 버튼. btn_number와 같지만 배경이 약간 어두운 갈색/탄색. 그림자 없이 평평한 느낌 (눌려 들어간 효과). 9-slice 가능. 크기: 128x128

> **btn_action**: 가로로 긴 직사각형 액션 버튼. 둥근 모서리(radius 16). 따뜻한 베이지/카키색 배경. 갈색 테두리. 마스킹 테이프/스티커 느낌. 9-slice 가능. 크기: 256x96

> **btn_action_pressed**: btn_action의 눌림 상태. 약간 더 어두운 톤. 그림자 제거로 평평한 느낌. 9-slice 가능. 크기: 256x96

> **btn_difficulty**: 난이도 선택 버튼 (비선택). 둥근 모서리의 작은 태그. 밝은 크림색 배경, 연한 회색 테두리. 탭 느낌. 9-slice 가능. 크기: 192x64

> **btn_difficulty_active**: 난이도 선택 버튼 (선택됨). btn_difficulty와 같지만 배경이 따뜻한 갈색/커피색. 흰 텍스트용으로 어두운 배경. 약간의 그림자. 9-slice 가능. 크기: 192x64

### 4. 아이콘 (Icons)

| # | 파일명 | 크기 | 9-slice | 설명 |
|---|--------|------|---------|------|
| 16 | `icon_hint.png` | 96x96 | X | 힌트 아이콘 — 전구 |
| 17 | `icon_erase.png` | 96x96 | X | 지우기 아이콘 — 지우개 |
| 18 | `icon_newgame.png` | 96x96 | X | 새 게임 아이콘 — 새로고침/리프레시 |

**image-forge 프롬프트:**

> **icon_hint**: 전구 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 심플한 전구. 종이 위에 연필로 그린 느낌. 투명 배경. 크기: 96x96

> **icon_erase**: 지우개 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 직사각형 지우개. 지우개 가루가 약간 흩날리는 느낌. 투명 배경. 크기: 96x96

> **icon_newgame**: 새 게임 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 리프레시/회전 화살표. 노트에 볼펜으로 그린 느낌. 투명 배경. 크기: 96x96

### 5. 타이틀 (Title)

| # | 파일명 | 크기 | 9-slice | 설명 |
|---|--------|------|---------|------|
| 19 | `title_sudoku.png` | 512x128 | X | "SUDOKU" 타이틀 로고 |

**image-forge 프롬프트:**

> **title_sudoku**: "SUDOKU" 타이틀 텍스트. 손글씨/캘리그래피 스타일. 진한 갈색/다크브라운 색상. 약간 기울어지고 자연스러운 필기체. 아래에 가벼운 밑줄 장식. 투명 배경. 크기: 512x128

---

### 스프라이트 총 목록 요약

| # | 파일명 | 크기 | 용도 |
|---|--------|------|------|
| 1 | bg_notebook.png | 1024x1024 | 전체 배경 |
| 2 | board_bg.png | 512x512 | 보드 배경 (9-slice) |
| 3 | cell_normal.png | 128x128 | 일반 셀 (9-slice) |
| 4 | cell_given.png | 128x128 | 고정 숫자 셀 (9-slice) |
| 5 | cell_selected.png | 128x128 | 선택 셀 (9-slice) |
| 6 | cell_samegroup.png | 128x128 | 같은 그룹 셀 (9-slice) |
| 7 | cell_error.png | 128x128 | 오류 셀 (9-slice) |
| 8 | grid_line_thin.png | 4x128 | 얇은 격자선 |
| 9 | grid_line_thick.png | 8x128 | 두꺼운 격자선 |
| 10 | btn_number.png | 128x128 | 숫자 버튼 (9-slice) |
| 11 | btn_number_pressed.png | 128x128 | 숫자 버튼 눌림 (9-slice) |
| 12 | btn_action.png | 256x96 | 액션 버튼 (9-slice) |
| 13 | btn_action_pressed.png | 256x96 | 액션 버튼 눌림 (9-slice) |
| 14 | btn_difficulty.png | 192x64 | 난이도 버튼 (9-slice) |
| 15 | btn_difficulty_active.png | 192x64 | 난이도 버튼 활성 (9-slice) |
| 16 | icon_hint.png | 96x96 | 힌트 아이콘 |
| 17 | icon_erase.png | 96x96 | 지우기 아이콘 |
| 18 | icon_newgame.png | 96x96 | 새 게임 아이콘 |
| 19 | title_sudoku.png | 512x128 | 타이틀 로고 |

**총 19개 스프라이트**

## 향후 확장
- 메모 기능 (후보 숫자 표시) — 추가 스프라이트 필요: `cell_memo_bg.png`
- 타이머 표시
- 실행취소 (Undo/Redo) 스택
- 클리어 시 축하 애니메이션
- 기록 저장 및 베스트 타임 표시
- 사운드 효과 (숫자 입력, 오류, 클리어)
