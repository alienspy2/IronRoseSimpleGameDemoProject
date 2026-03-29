# Phase 05a: UI 에셋 생성 및 Canvas 구성 (rose-cli / image-forge)

## 목표
- image-forge 스킬로 UI 에셋(아이콘, 패널 배경)을 생성한다.
- rose-cli로 Canvas와 기본 UI 구조를 씬에 구성한다.
- UI 프리팹으로 저장한다.

## 선행 조건
- Phase 01 ~ 03 완료 (게임 기본 동작 구현)
- Phase 04 완료 또는 병행 가능 (UI는 게임 로직과 독립)
- 에디터가 실행 중이어야 한다

## 작업 (image-forge + rose-cli)

이 Phase는 **에셋 생성 + 씬 UI 구조 설정** 작업이다.

### 5a-1. UI 에셋 생성 (image-forge)

다음 이미지를 `/home/alienspy/git/MyGame/Assets/AngryClawdAssets/UI/` 디렉토리에 생성한다.

| 에셋 | 크기 | 설명 |
|------|------|------|
| `aim_arrow.png` | 64x64 | 조준 방향 화살표 아이콘 (흰색, 투명 배경) |
| `pig_icon.png` | 32x32 | pig 카운터 옆 아이콘 (노란색 사각형) |
| `shot_icon.png` | 32x32 | 발사 횟수 옆 아이콘 (원형 탄환) |
| `panel_bg.png` | 64x64 | 반투명 패널 배경 (9-slice용, border 16,16,16,16) |

스프라이트 임포트 설정 (rose-cli):
```
sprite.set_type Assets/AngryClawdAssets/UI/aim_arrow.png Sprite
sprite.set_filter Assets/AngryClawdAssets/UI/aim_arrow.png Point

sprite.set_type Assets/AngryClawdAssets/UI/pig_icon.png Sprite
sprite.set_filter Assets/AngryClawdAssets/UI/pig_icon.png Point

sprite.set_type Assets/AngryClawdAssets/UI/shot_icon.png Sprite
sprite.set_filter Assets/AngryClawdAssets/UI/shot_icon.png Point

sprite.set_type Assets/AngryClawdAssets/UI/panel_bg.png Sprite
sprite.set_filter Assets/AngryClawdAssets/UI/panel_bg.png Point
sprite.set_border Assets/AngryClawdAssets/UI/panel_bg.png 16,16,16,16
```

### 5a-2. Canvas 생성
```
ui.create_canvas GameCanvas
```
- Canvas ID를 기록한다 (이후 명령에서 사용)

```
ui.canvas.set_scale_mode <canvasId> ScaleWithScreenSize
ui.canvas.set_reference_resolution <canvasId> 1280,720
```

### 5a-3. 상단 정보 바 구성

**TopPanel 생성**:
```
ui.create_panel <canvasId> 0.1,0.1,0.1,0.7
```
- 반환된 ID로 TopPanel 설정:
```
ui.rect.set_preset <topPanelId> TopStretch
ui.rect.set_size <topPanelId> 0,50
```

**StageText 생성**:
```
ui.create_text <topPanelId> "Stage 1" 28
```
```
ui.rect.set_preset <stageTextId> MiddleLeft
ui.rect.set_position <stageTextId> 20,0
```

**PigCountText 생성**:
```
ui.create_text <topPanelId> "Pigs: 0" 24
```
```
ui.rect.set_preset <pigCountTextId> MiddleCenter
```

**ShotCountText 생성**:
```
ui.create_text <topPanelId> "Shots: 5" 24
```
```
ui.rect.set_preset <shotCountTextId> MiddleRight
ui.rect.set_position <shotCountTextId> -20,0
```

### 5a-4. 중앙 메시지 (스테이지 클리어/실패)

**CenterMessage Panel 생성**:
```
ui.create_panel <canvasId> 0,0,0,0.8
```
```
ui.rect.set_preset <centerMsgId> MiddleCenter
ui.rect.set_size <centerMsgId> 500,120
```

**MessageText 생성**:
```
ui.create_text <centerMsgId> "Stage Clear!" 48
```
```
ui.rect.set_preset <msgTextId> StretchAll
ui.text.set_alignment <msgTextId> MiddleCenter
ui.text.set_color <msgTextId> 1,1,0,1
```

**기본 상태: 비활성화**:
```
go.set_active <centerMsgId> false
```

### 5a-5. 재시작 버튼

```
ui.create_button <canvasId> "Restart"
```
```
ui.rect.set_preset <restartBtnId> BottomRight
ui.rect.set_position <restartBtnId> -20,20
ui.rect.set_size <restartBtnId> 120,40
```

### 5a-6. GO 이름 변경 (코드에서 Find하기 위해)

각 UI 요소의 이름을 코드에서 찾을 수 있도록 설정한다:
```
go.rename <topPanelId> TopPanel
go.rename <stageTextId> StageText
go.rename <pigCountTextId> PigCountText
go.rename <shotCountTextId> ShotCountText
go.rename <centerMsgId> CenterMessage
go.rename <msgTextId> MessageText
go.rename <restartBtnId> RestartButton
```

### 5a-7. UI 프리팹으로 저장
```
ui.prefab.save <canvasId> Assets/AngryClawdAssets/UI/GameCanvas.prefab
```

### 5a-8. 씬 저장
```
scene.save
```

## 검증 기준
- [ ] UI 에셋 4개가 `Assets/AngryClawdAssets/UI/`에 생성됨
- [ ] Canvas가 씬에 존재하고 ScaleWithScreenSize 1280x720으로 설정됨
- [ ] TopPanel에 StageText, PigCountText, ShotCountText가 포함됨
- [ ] CenterMessage가 비활성 상태로 존재함
- [ ] RestartButton이 우하단에 배치됨
- [ ] 각 UI 요소의 GO 이름이 코드에서 Find 가능하도록 설정됨
- [ ] UI 프리팹이 저장됨
- [ ] 씬이 저장됨

## 참고
- image-forge 스킬이 사용 가능한 환경이어야 한다.
- rose-cli UI 명령의 정확한 인자 형식은 `command-reference.md`를 참조한다.
- `ui.create_text`, `ui.create_panel`, `ui.create_button`의 반환값에서 생성된 GO ID를 기록하여 이후 명령에서 사용해야 한다.
- 이 Phase는 코드 빌드가 필요 없다 (에디터 씬/에셋 작업만).
- Phase 05b에서 AngryClawdGame.cs에 UI 로직을 추가한다.
