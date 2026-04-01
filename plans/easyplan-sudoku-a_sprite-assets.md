# Phase A: 스프라이트 에셋 생성

## 목표
- image-forge 스킬을 사용하여 스도쿠 게임에 필요한 19개 스프라이트를 모두 생성한다.
- 모든 스프라이트는 `Assets/Sudoku/Sprites/` 폴더에 저장한다.
- 9-slice가 필요한 스프라이트는 border 값을 설정한다.

## 선행 조건
- 없음 (첫 번째 phase)

## 작업 내용

### 폴더 생성
- `Assets/Sudoku/Sprites/` 디렉토리를 생성한다.

### 스프라이트 생성 (image-forge 스킬 사용)

image-forge 스킬로 각 스프라이트를 생성한다. 생성 경로는 모두 `Assets/Sudoku/Sprites/` 하위이다.

#### 1. 배경

| 파일명 | 크기 | 9-slice |
|--------|------|---------|
| `bg_notebook.png` | 1024x1024 | X |

**프롬프트:** 크림색/아이보리 노트 종이 텍스처. 미세한 가로줄이 은은하게 보이고, 종이 질감이 느껴지는 따뜻한 배경. 모서리 부근에 약간의 그림자/얼룩으로 빈티지 느낌. 타일링 가능하게. 크기: 1024x1024

#### 2. 보드/그리드

| 파일명 | 크기 | 9-slice border |
|--------|------|----------------|
| `board_bg.png` | 512x512 | border 16 |
| `cell_normal.png` | 128x128 | border 8 |
| `cell_given.png` | 128x128 | border 8 |
| `cell_selected.png` | 128x128 | border 8 |
| `cell_samegroup.png` | 128x128 | border 8 |
| `cell_error.png` | 128x128 | border 8 |
| `grid_line_thin.png` | 4x128 | X |
| `grid_line_thick.png` | 8x128 | X |

**프롬프트:**

- **board_bg**: 스도쿠 보드 배경. 약간 어두운 크림/베이지색 사각형. 미세한 종이 질감. 연한 그림자 테두리로 살짝 입체감. 9-slice용으로 모서리/테두리/중앙 영역 구분. 크기: 512x512
- **cell_normal**: 빈 셀 배경. 밝은 아이보리/흰색 정사각형. 아주 얇은 연회색 테두리. 깨끗한 종이 느낌. 9-slice 가능. 크기: 128x128
- **cell_given**: 고정 숫자 셀 배경. cell_normal보다 약간 진한 크림/연갈색. 잉크가 스며든 듯한 느낌. 미세한 종이 질감. 9-slice 가능. 크기: 128x128
- **cell_selected**: 선택 셀 하이라이트. 연한 하늘색/파란색 배경. 마치 연필로 살짝 칠한 듯한 부드러운 색감. 중앙이 약간 더 진하고 가장자리가 페이드. 9-slice 가능. 크기: 128x128
- **cell_samegroup**: 같은 그룹 셀 하이라이트. 매우 연한 노란색/레몬색 배경. 형광펜으로 살짝 칠한 느낌. 투명감 있게. 9-slice 가능. 크기: 128x128
- **cell_error**: 오류 셀 배경. 연한 빨간색/살몬색 배경. 빨간 펜으로 밑줄 친 듯한 느낌. 9-slice 가능. 크기: 128x128
- **grid_line_thin**: 셀 사이 얇은 격자선. 연한 회색(#CCCCCC). 연필 선처럼 약간 불규칙한 느낌. 세로 방향. 크기: 4x128
- **grid_line_thick**: 3x3 박스 경계 두꺼운 격자선. 진한 갈색/다크그레이(#555555). 펜으로 그은 듯한 진한 선. 세로 방향. 크기: 8x128

#### 3. 버튼

| 파일명 | 크기 | 9-slice border |
|--------|------|----------------|
| `btn_number.png` | 128x128 | border 12 |
| `btn_number_pressed.png` | 128x128 | border 12 |
| `btn_action.png` | 256x96 | border 16 |
| `btn_action_pressed.png` | 256x96 | border 16 |
| `btn_difficulty.png` | 192x64 | border 12 |
| `btn_difficulty_active.png` | 192x64 | border 12 |

**프롬프트:**

- **btn_number**: 숫자 패드용 정사각형 버튼. 둥근 모서리(radius 12). 밝은 크림색 배경에 얇은 갈색 테두리. 종이 스티커/태그 느낌. 약간의 그림자로 입체감. 9-slice 가능. 크기: 128x128
- **btn_number_pressed**: 숫자 패드 눌림 버튼. btn_number와 같지만 배경이 약간 어두운 갈색/탄색. 그림자 없이 평평한 느낌 (눌려 들어간 효과). 9-slice 가능. 크기: 128x128
- **btn_action**: 가로로 긴 직사각형 액션 버튼. 둥근 모서리(radius 16). 따뜻한 베이지/카키색 배경. 갈색 테두리. 마스킹 테이프/스티커 느낌. 9-slice 가능. 크기: 256x96
- **btn_action_pressed**: btn_action의 눌림 상태. 약간 더 어두운 톤. 그림자 제거로 평평한 느낌. 9-slice 가능. 크기: 256x96
- **btn_difficulty**: 난이도 선택 버튼 (비선택). 둥근 모서리의 작은 태그. 밝은 크림색 배경, 연한 회색 테두리. 탭 느낌. 9-slice 가능. 크기: 192x64
- **btn_difficulty_active**: 난이도 선택 버튼 (선택됨). btn_difficulty와 같지만 배경이 따뜻한 갈색/커피색. 흰 텍스트용으로 어두운 배경. 약간의 그림자. 9-slice 가능. 크기: 192x64

#### 4. 아이콘

| 파일명 | 크기 | 9-slice |
|--------|------|---------|
| `icon_hint.png` | 96x96 | X |
| `icon_erase.png` | 96x96 | X |
| `icon_newgame.png` | 96x96 | X |

**프롬프트:**

- **icon_hint**: 전구 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 심플한 전구. 종이 위에 연필로 그린 느낌. 투명 배경. 크기: 96x96
- **icon_erase**: 지우개 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 직사각형 지우개. 지우개 가루가 약간 흩날리는 느낌. 투명 배경. 크기: 96x96
- **icon_newgame**: 새 게임 아이콘. 손그림/스케치 스타일. 갈색 선으로 그린 리프레시/회전 화살표. 노트에 볼펜으로 그린 느낌. 투명 배경. 크기: 96x96

#### 5. 타이틀

| 파일명 | 크기 | 9-slice |
|--------|------|---------|
| `title_sudoku.png` | 512x128 | X |

**프롬프트:** "SUDOKU" 타이틀 텍스트. 손글씨/캘리그래피 스타일. 진한 갈색/다크브라운 색상. 약간 기울어지고 자연스러운 필기체. 아래에 가벼운 밑줄 장식. 투명 배경. 크기: 512x128

### 9-slice border 설정

스프라이트 PNG 생성 후, 에디터의 FileSystemWatcher가 자동으로 `.rose` 메타파일을 생성한다. 메타파일이 생성된 뒤 **rose-cli**로 border를 설정한다. `.rose` 메타파일을 직접 편집하지 않는다.

**순서:**
1. image-forge로 PNG 파일 생성
2. 에디터가 자동으로 `.rose` 메타파일 생성 (FileSystemWatcher)
3. `sprite.set_border`로 9-slice border 설정 (rose-cli)

```bash
rose-cli sprite.set_border Assets/Sudoku/Sprites/<파일명>.png <left,bottom,right,top>
```

- border 8인 경우: `sprite.set_border <path> 8,8,8,8`
- border 12인 경우: `sprite.set_border <path> 12,12,12,12`
- border 16인 경우: `sprite.set_border <path> 16,16,16,16`

PPU(pixelsPerUnit) 변경이 필요한 경우:
```bash
rose-cli sprite.set_ppu Assets/Sudoku/Sprites/<파일명>.png 100
```

> **주의**: PNG 생성 직후 `sprite.set_border` 호출 시, 에디터가 아직 메타파일을 생성하지 않았을 수 있다. 메타파일 존재를 확인하거나 잠시 대기 후 호출한다.

## 검증 기준
- [ ] `Assets/Sudoku/Sprites/` 폴더에 19개 PNG 파일이 모두 존재
- [ ] 9-slice가 필요한 스프라이트(총 10개)의 `.rose` 메타파일에 border 값이 올바르게 설정됨
- [ ] 에디터에서 스프라이트가 정상적으로 로드되어 미리보기 가능

## 참고
- image-forge 스킬의 사용법에 따라 SVG를 생성하고 PNG로 변환한다.
- `.rose` 메타파일은 에디터가 FileSystemWatcher로 자동 생성하며, border/PPU 등은 rose-cli로 설정한다. 직접 편집하지 않는다.
- 이 phase는 C# 코드 작성이 없으므로 `dotnet build`와는 무관하다.
