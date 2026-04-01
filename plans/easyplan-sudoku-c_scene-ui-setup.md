# Phase C: 씬 UI 계층 구조 구성 (rose-cli)

## 목표
- `sudoku.scene`에 스도쿠 게임의 전체 UI 계층 구조를 rose-cli로 구성한다.
- Canvas, 패널, 텍스트, 버튼 등 모든 UI 요소를 씬에 배치한다.
- 이 phase에서는 씬 데이터만 편집하며 스크립트는 작성하지 않는다.

## 선행 조건
- Phase A 완료 (스프라이트 에셋이 `Assets/Sudoku/Sprites/`에 존재해야 함)

## 작업 대상 씬
- `/home/alienspy/git/IronRoseSimpleGameDemoProject/Assets/Scenes/SimpleGameDemo/sudoku.scene`

## 씬 편집 규칙
- **반드시 rose-cli 스킬을 사용**하여 씬을 편집한다. `.scene` 파일을 직접 편집하지 않는다.
- 기존 씬의 Main Camera, Cube, Plane, Spot Light 등 기본 오브젝트는 그대로 둔다 (UI 전용 Canvas를 추가).

## 생성할 UI 계층 구조

씬 파일 형식은 TOML이며, 기존 AngryClawd.scene의 Canvas 구성을 참고한다. 좌표계는 Y-down (0,0 = 좌상단).

### 최상위 구조

```
SudokuCanvas (Canvas, StretchAll)
  ├── Background (UIImage, StretchAll) -- bg_notebook.png
  ├── HeaderPanel (RectTransform, TopStretch) -- 상단 영역
  │   ├── TitleImage (UIImage) -- title_sudoku.png
  │   ├── DifficultyPanel (RectTransform) -- 난이도 버튼 그룹
  │   │   ├── EasyButton (UIButton + UIImage + UIText)
  │   │   ├── MediumButton (UIButton + UIImage + UIText)
  │   │   └── HardButton (UIButton + UIImage + UIText)
  │   └── NewGameButton (UIButton + UIImage + UIText)
  ├── BoardPanel (RectTransform, MiddleCenter) -- 중앙 보드 영역
  │   └── BoardBackground (UIImage) -- board_bg.png
  ├── NumberPadPanel (RectTransform, BottomStretch) -- 하단 숫자 패드
  │   ├── Num1 ~ Num9 (각각 UIButton + UIImage + UIText)
  │   ├── EraseButton (UIButton + UIImage + UIText)
  │   └── HintButton (UIButton + UIImage + UIText)
  └── MessagePanel (RectTransform, MiddleCenter) -- 클리어 메시지 (기본 비활성)
      └── MessageText (UIText)
```

### 상세 설정

#### 1. SudokuCanvas
- **GameObject 이름**: `SudokuCanvas`
- **컴포넌트**: `RoseEngine.Canvas`
  - `renderMode`: `ScreenSpaceOverlay`
  - `sortingOrder`: 0
  - `referenceResolution`: [1280, 720]
  - `scaleMode`: `ScaleWithScreenSize`
  - `matchWidthOrHeight`: 0.5
- **RectTransform**: anchorPreset StretchAll, sizeDelta [0, 0]

#### 2. Background
- **부모**: SudokuCanvas
- **컴포넌트**: `RoseEngine.UIImage`
  - `sprite`: `Assets/Sudoku/Sprites/bg_notebook.png` (assetGuid로 참조)
  - `color`: [1, 1, 1, 1]
  - `imageType`: Simple
- **RectTransform**: anchorPreset StretchAll, sizeDelta [0, 0]

#### 3. HeaderPanel
- **부모**: SudokuCanvas
- **RectTransform**:
  - anchorMin: [0, 0], anchorMax: [1, 0] (TopStretch)
  - sizeDelta: [-40, 80] (좌우 20px 패딩, 높이 80px)
  - anchoredPosition: [0, 10]
  - pivot: [0.5, 0]

#### 4. TitleImage
- **부모**: HeaderPanel
- **컴포넌트**: `RoseEngine.UIImage`
  - `sprite`: `Assets/Sudoku/Sprites/title_sudoku.png`
  - `imageType`: Simple
  - `preserveAspect`: true
- **RectTransform**:
  - anchorMin: [0, 0], anchorMax: [0, 0] (TopLeft)
  - sizeDelta: [200, 50]
  - anchoredPosition: [10, 15]
  - pivot: [0, 0]

#### 5. DifficultyPanel
- **부모**: HeaderPanel
- **RectTransform**:
  - anchorMin: [0.5, 0.5], anchorMax: [0.5, 0.5] (MiddleCenter)
  - sizeDelta: [360, 40]
  - anchoredPosition: [0, 0]
  - pivot: [0.5, 0.5]

#### 6. EasyButton / MediumButton / HardButton
- **부모**: DifficultyPanel
- **컴포넌트들**: `RoseEngine.UIButton` + `RoseEngine.UIImage` + `RoseEngine.UIText`
  - UIImage.sprite: `btn_difficulty.png` (비선택) / `btn_difficulty_active.png` (선택)
  - UIImage.imageType: Sliced
  - UIText.text: "Easy" / "Medium" / "Hard"
  - UIText.fontSize: 18
  - UIText.color: [0.3, 0.25, 0.2, 1] (짙은 갈색)
  - UIText.alignment: MiddleCenter
  - UIButton.transition: SpriteSwap
- **RectTransform** (각 버튼, 가로 배치):
  - anchorMin/Max: [0, 0] (TopLeft 기준, 코드에서 배치)
  - sizeDelta: [100, 36]
  - EasyButton anchoredPosition: [10, 2]
  - MediumButton anchoredPosition: [120, 2]
  - HardButton anchoredPosition: [230, 2]
  - pivot: [0, 0]

#### 7. NewGameButton
- **부모**: HeaderPanel
- **컴포넌트들**: `RoseEngine.UIButton` + `RoseEngine.UIImage` + `RoseEngine.UIText`
  - UIImage.sprite: `btn_action.png`
  - UIImage.imageType: Sliced
  - UIText.text: "New Game"
  - UIText.fontSize: 18
  - UIText.color: [0.3, 0.25, 0.2, 1]
  - UIText.alignment: MiddleCenter
- **RectTransform**:
  - anchorMin: [1, 0], anchorMax: [1, 0] (TopRight)
  - sizeDelta: [140, 40]
  - anchoredPosition: [-10, 20]
  - pivot: [1, 0]

#### 8. BoardPanel
- **부모**: SudokuCanvas
- **용도**: 9x9 셀을 코드에서 동적으로 생성할 컨테이너
- **RectTransform**:
  - anchorMin: [0.5, 0.5], anchorMax: [0.5, 0.5] (MiddleCenter)
  - sizeDelta: [450, 450] (정사각형, 화면 높이의 약 62%)
  - anchoredPosition: [0, -10]
  - pivot: [0.5, 0.5]

#### 9. BoardBackground
- **부모**: BoardPanel
- **컴포넌트**: `RoseEngine.UIImage`
  - `sprite`: `Assets/Sudoku/Sprites/board_bg.png`
  - `imageType`: Sliced
  - `color`: [1, 1, 1, 1]
- **RectTransform**: anchorPreset StretchAll, sizeDelta [10, 10] (보드보다 약간 크게, 패딩 효과)
  - anchoredPosition: [0, 0]

#### 10. NumberPadPanel
- **부모**: SudokuCanvas
- **RectTransform**:
  - anchorMin: [0, 1], anchorMax: [1, 1] (BottomStretch)
  - sizeDelta: [-40, 70] (좌우 20px 패딩, 높이 70px)
  - anchoredPosition: [0, -10]
  - pivot: [0.5, 1]

#### 11. 숫자 버튼 Num1~Num9
- **부모**: NumberPadPanel
- **컴포넌트들**: `RoseEngine.UIButton` + `RoseEngine.UIImage` + `RoseEngine.UIText`
  - UIImage.sprite: `btn_number.png`
  - UIImage.imageType: Sliced
  - UIText.text: "1" ~ "9" (각각)
  - UIText.fontSize: 28
  - UIText.color: [0.2, 0.15, 0.1, 1] (진한 갈색, 펜 느낌)
  - UIText.alignment: MiddleCenter
- **RectTransform** (가로 배치, 등간격):
  - 각 버튼 sizeDelta: [50, 50]
  - anchorMin/Max: [0.5, 0.5] (중앙 기준)
  - 가로 배치: 9개 버튼을 중앙 정렬로 배치
  - 간격 계산: 총 폭 = 9*50 + 8*6 = 498px, 시작 X = -249
  - Num1 anchoredPosition: [-224, 0], Num2: [-168, 0], ..., Num9: [224, 0] (56px 간격)
  - pivot: [0.5, 0.5]

#### 12. EraseButton
- **부모**: NumberPadPanel
- **컴포넌트들**: `RoseEngine.UIButton` + `RoseEngine.UIImage` + `RoseEngine.UIText`
  - UIImage.sprite: `btn_action.png`
  - UIImage.imageType: Sliced
  - UIText.text: "Erase"
  - UIText.fontSize: 16
  - UIText.alignment: MiddleCenter
- **RectTransform**:
  - anchorMin: [0, 0.5], anchorMax: [0, 0.5] (MiddleLeft)
  - sizeDelta: [80, 40]
  - anchoredPosition: [10, 0]
  - pivot: [0, 0.5]

#### 13. HintButton
- **부모**: NumberPadPanel
- **컴포넌트들**: `RoseEngine.UIButton` + `RoseEngine.UIImage` + `RoseEngine.UIText`
  - UIImage.sprite: `btn_action.png`
  - UIImage.imageType: Sliced
  - UIText.text: "Hint"
  - UIText.fontSize: 16
  - UIText.alignment: MiddleCenter
- **RectTransform**:
  - anchorMin: [1, 0.5], anchorMax: [1, 0.5] (MiddleRight)
  - sizeDelta: [80, 40]
  - anchoredPosition: [-10, 0]
  - pivot: [1, 0.5]

#### 14. MessagePanel
- **부모**: SudokuCanvas
- **activeSelf**: false (기본 비활성)
- **컴포넌트**: `RoseEngine.UIPanel`
  - `color`: [0, 0, 0, 0.5] (반투명 검은 배경)
- **RectTransform**: anchorPreset StretchAll, sizeDelta [0, 0]

#### 15. MessageText
- **부모**: MessagePanel
- **컴포넌트**: `RoseEngine.UIText`
  - `text`: ""
  - `fontSize`: 48
  - `color`: [1, 1, 1, 1]
  - `alignment`: MiddleCenter
- **RectTransform**: anchorPreset MiddleCenter, sizeDelta [400, 100]

## 검증 기준
- [ ] `sudoku.scene` 파일이 유효한 TOML 형식
- [ ] 에디터에서 씬을 열면 Canvas UI 계층이 보임
- [ ] Background에 bg_notebook.png 텍스처가 표시됨
- [ ] HeaderPanel, BoardPanel, NumberPadPanel이 올바른 위치에 배치됨
- [ ] 모든 버튼에 텍스트가 표시됨

## 참고
- 씬 파일은 TOML 형식이며, 부모-자식 관계는 `parentIndex`로 표현된다 (0-based, -1은 루트).
- 스프라이트 참조는 `_assetGuid`와 `_assetType`으로 한다. rose-cli가 에셋 경로를 guid로 자동 변환해준다.
- rose-cli 사용 시 에셋 파일이 이미 존재해야 guid 참조가 가능하므로, Phase A가 먼저 완료되어야 한다.
- 9x9 셀(81개)은 이 phase에서 씬에 넣지 않는다. Phase D에서 스크립트로 동적 생성한다.
- 숫자 버튼의 정확한 위치는 스크립트에서 조정할 수 있으므로 대략적인 위치로 배치한다.
