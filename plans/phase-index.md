# Phase Index

## AngryClawd 데모 게임 구현 Phase 목록

| Phase | 제목 | 파일 | 선행 | 유형 | 상태 |
|-------|------|------|------|------|------|
| 01 | 씬 준비 | phase01_scene-preparation.md | - | rose-cli | 미완료 |
| 02 | 핵심 스크립트 | phase02_core-scripts.md | 01 | 코드 | 미완료 |
| 03 | 게임 로직 | phase03_game-logic.md | 02 | 코드 | 미완료 |
| 04 | 테스트/밸런싱 | phase04_testing-balancing.md | 03 | rose-cli + 코드 | 미완료 |
| 05a | UI 에셋/Canvas 구성 | phase05a_ui-assets-and-canvas.md | 03 | rose-cli + image-forge | 미완료 |
| 05b | UI 스크립트 통합 | phase05b_ui-script-integration.md | 03, 05a | 코드 | 미완료 |
| 06 | 폴리싱 (선택) | phase06_polishing.md | 05b | 코드 + rose-cli | 미완료 |

## 의존 관계

```
Phase 01 (씬 준비)
  |
  v
Phase 02 (핵심 스크립트: PileScript, BlockScript, PigScript, BombScript, CannonballScript)
  |
  v
Phase 03 (게임 로직: AngryClawdGame)
  |
  +-------+------------------+
  |       |                  |
  v       v                  v
Phase 04  Phase 05a          (Phase 04와 05a는 병행 가능)
(테스트)  (UI 에셋/Canvas)
          |
          v
        Phase 05b (UI 스크립트 통합)
          |
          v
        Phase 06 (폴리싱, 선택)
```

## Phase 유형별 분류

### 코드 작업 (aca-coder-csharp)
- Phase 02: 핵심 스크립트 구현 (5개 파일)
- Phase 03: AngryClawdGame 게임 로직 (1개 파일)
- Phase 05b: UI 스크립트 통합 (1개 파일 수정)
- Phase 06: 폴리싱 (선택, 여러 파일 수정)

### 에디터 작업 (rose-cli)
- Phase 01: 씬 오브젝트 정리/조정
- Phase 05a: UI Canvas/요소 생성

### 혼합 작업
- Phase 04: Play 모드 테스트 + 코드 밸런싱

## 파일 목록

### 수정 파일
| 파일 | Phase |
|------|-------|
| `LiveCode/AngryClawd/AngryClawdGame.cs` | 03, 05b |
| `LiveCode/AngryClawd/PileScript.cs` | 02 |

### 새 파일
| 파일 | Phase |
|------|-------|
| `LiveCode/AngryClawd/BlockScript.cs` | 02 |
| `LiveCode/AngryClawd/PigScript.cs` | 02 |
| `LiveCode/AngryClawd/BombScript.cs` | 02 |
| `LiveCode/AngryClawd/CannonballScript.cs` | 02 |

### 에셋 (Phase 05a)
| 에셋 | 유형 |
|------|------|
| `Assets/AngryClawdAssets/UI/aim_arrow.png` | image-forge 생성 |
| `Assets/AngryClawdAssets/UI/pig_icon.png` | image-forge 생성 |
| `Assets/AngryClawdAssets/UI/shot_icon.png` | image-forge 생성 |
| `Assets/AngryClawdAssets/UI/panel_bg.png` | image-forge 생성 |
| `Assets/AngryClawdAssets/UI/GameCanvas.prefab` | rose-cli 생성 |
