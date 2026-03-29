# AngryClawd 데모 게임 설계 계획서

## 배경
- IronRose 엔진의 데모용 Angry Birds 스타일 3D 물리 게임
- 엔진의 Rigidbody 물리, Prefab 시스템, 충돌 콜백, 입력 시스템을 활용하는 쇼케이스

## 목표
- slingshot으로 cannonball을 발사하여 pile(큐브 더미) 속의 pig를 모두 제거하는 게임
- 스테이지 시스템: 진행할수록 pile 수 증가
- 폭탄 큐브: 주변 3m 범위 큐브 일괄 삭제

## 현재 상태

### 기존 파일
| 파일 | 상태 | 설명 |
|------|------|------|
| `LiveCode/AngryClawd/AngryClawdGame.cs` | 빈 껍데기 | SimpleGameBase 상속, 빈 Start/Update |
| `LiveCode/AngryClawd/PileScript.cs` | 빈 껍데기 | MonoBehaviour 상속, 빈 Start/Update |
| `LiveCode/SimpleGameBase.cs` | 빈 껍데기 | MonoBehaviour 상속, 빈 Start/Update |

### 기존 에셋
| 에셋 | GUID | 설명 |
|------|------|------|
| `Assets/AngryClawdAssets/mat_block.mat` | `ae2c9f21-fb57-41b6-999e-38c4f9bded72` | 흰색 블록 머티리얼 |
| `Assets/AngryClawdAssets/mat_pig.mat` | `d80c9906-546b-4d30-a1ae-21ffd2221b0f` | 노란색(황금) 피그 머티리얼 |
| `Assets/AngryClawdAssets/mat_bomb.mat` | `08ba5c79-5dda-442a-9fc4-a223d45176df` | 빨간색 폭탄 머티리얼 |
| `Assets/AngryClawdAssets/pile.prefab` | `da096309-223c-488c-b39a-c5f62ba55fe0` | pile 프리팹 (PileScript 포함, Cube 자식 1개) |
| `Assets/AngryClawdAssets/cannonball.prefab` | `0804bc30-df11-4fd2-89a5-5265d7180ff2` | cannonball 프리팹 (Sphere + Clawd 캐릭터) |

### 기존 씬 (AngryClawd.scene)
- Main Camera: 위치 (0, 3, -6), 약간 아래를 바라봄
- ground: 5x5 스케일 Plane (BoxCollider 포함)
- Spot Light: 위에서 비추는 스팟 라이트
- pile: pile.prefab 인스턴스 (원점)
- game: SimpleGameBase + AngryClawdGame 컴포넌트
- shooter: 빈 GO, 위치 (-24.5, 0, 0) -- slingshot 발사 위치
- cannonball: cannonball.prefab 인스턴스, 위치 (-5.7, 1.7, 0)

### 엔진 API 현황
- `Camera.ScreenPointToRay` **구현 완료** -- 화면 좌표 → 월드 Ray 변환 가능
- `Matrix4x4.Inverse` **구현 완료** -- SIMD 역행렬 가능
- `Ray` struct **구현 완료** -- origin, direction, GetPoint()
- `Physics.Raycast` 사용 가능
- `Physics.OverlapSphere` 사용 가능
- `Collision.relativeVelocity` 사용 가능
- `PrefabUtility.InstantiatePrefab(guid, position, rotation)` 사용 가능
- `Resources.Load<Material>(path)` 사용 가능
- `Object.Destroy(obj)` 사용 가능
- `Random.Range`, `Random.value` 사용 가능

---

## 설계

### 개요

게임 흐름:
```
[게임 시작] -> [스테이지 생성] -> [발사 대기] -> [조준/발사] -> [충돌 판정]
     ^              |                                              |
     |              v                                              v
     +------[다음 스테이지]<---[모든 pig 제거됨?]<---[cannonball 정지/낙하]
```

slingshot 메커닉은 `Camera.ScreenPointToRay` + ground Raycast로 마우스 월드 좌표를 구하여 구현한다:
- 마우스 클릭: 조준 시작, ScreenPointToRay로 시작 월드 좌표 계산
- 마우스 드래그: 드래그 방향의 반대 방향이 발사 방향 (새총 당기는 느낌)
- 마우스 릴리즈: 월드 드래그 거리에 비례한 힘으로 Impulse 발사

### 상세 설계

#### 1. SimpleGameBase (LiveCode/SimpleGameBase.cs)

게임 공통 베이스 클래스. AngryClawd 외 다른 게임에서도 재사용 가능.

```csharp
using RoseEngine;

public class SimpleGameBase : MonoBehaviour
{
    // 하위 클래스에서 override하여 사용
    // 기본 구현은 비어 있음
}
```

현재 상태 유지 (빈 클래스). AngryClawdGame이 게임 로직을 전담한다.

#### 2. AngryClawdGame (LiveCode/AngryClawd/AngryClawdGame.cs)

**역할**: 전체 게임 로직 관리 -- 스테이지 생성, 슈팅 메커닉, 승리/진행 판정

**주요 필드**:
```csharp
// === Inspector 필드 (에디터에서 설정) ===
public string pilePrefabGuid = "da096309-223c-488c-b39a-c5f62ba55fe0";
public string cannonballPrefabGuid = "0804bc30-df11-4fd2-89a5-5265d7180ff2";

// === 게임 상태 ===
private int currentStage = 1;
private bool isAiming = false;               // 조준 중 여부
private Vector2 aimStartPos;                  // 마우스 클릭 시작 위치 (screen)
private GameObject? currentCannonball;        // 현재 발사 대기 중인 cannonball
private List<GameObject> activePiles = new(); // 현재 스테이지의 pile 목록
private bool cannonballFired = false;         // cannonball이 발사되었는지
private float fireTime;                       // 발사 시각 (타임아웃용)

// === 상수 ===
private const float SHOOT_FORCE_MULTIPLIER = 0.3f; // 드래그 거리 -> 힘 변환 계수
private const float MAX_SHOOT_FORCE = 40f;          // 최대 발사 힘
private const float CANNONBALL_MASS = 2.0f;         // cannonball 질량
private const float CANNONBALL_TIMEOUT = 8.0f;      // 발사 후 n초 뒤 자동 정리
private const float STAGE_CLEAR_DELAY = 2.0f;       // 스테이지 클리어 후 대기 시간
private const float PILE_SPACING = 8.0f;            // pile 간 X 간격
private const float PILE_START_X = 0.0f;            // 첫 pile의 X 위치
```

**주요 메서드**:

```csharp
public override void Start()
// - shooter 위치에 첫 cannonball 생성 (SpawnCannonball)
// - 첫 스테이지 생성 (SetupStage)

public override void Update()
// - 조준/발사 입력 처리 (HandleAiming)
// - 발사된 cannonball 추적 (TrackCannonball)
// - 스테이지 클리어 판정 (CheckStageClear)

private void SetupStage(int stageNum)
// - 기존 pile 정리
// - stageNum에 따라 pile 수 결정: pileCount = 1 + (stageNum - 1) (최대 5)
// - 각 pile을 PrefabUtility.InstantiatePrefab으로 생성
// - pile 위치: (PILE_START_X + i * PILE_SPACING, 0, 0) + 약간의 Z 랜덤

private void ClearStage()
// - activePiles의 모든 pile과 자식 Destroy
// - 남아있는 cannonball Destroy
// - activePiles.Clear()

private void SpawnCannonball()
// - shooter GO 위치 참조
// - cannonball prefab 인스턴스화 (shooter 위치 + 약간 위)
// - Rigidbody 추가, isKinematic = true (발사 전 정지)
// - CannonballScript 추가

private void HandleAiming()
// - GetMouseButtonDown(0): isAiming = true, aimStartPos 기록
// - GetMouseButton(0) && isAiming: 드래그 시각적 피드백 (선택사항)
// - GetMouseButtonUp(0) && isAiming: 발사 (Fire)

private void Fire()
// - 드래그 벡터 계산: delta = aimStartPos - mousePosition (반대 방향)
// - 힘 크기: Mathf.Min(delta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE)
// - 발사 방향: (deltaX, deltaY, 0).normalized -- 화면 X/Y를 월드 X/Y에 매핑
// - Rigidbody.isKinematic = false, AddForce(direction * force, ForceMode.Impulse)
// - cannonballFired = true, fireTime = Time.time

private void TrackCannonball()
// - cannonball이 null이면 return
// - 타임아웃 체크: Time.time - fireTime > CANNONBALL_TIMEOUT -> 정리 & 새 cannonball
// - y < -10 (낙하) -> 정리 & 새 cannonball

private void CheckStageClear()
// - 모든 pig 태그 오브젝트 검색 (FindGameObjectsWithTag("Pig"))
// - pig가 0개면 스테이지 클리어
// - 다음 스테이지로 진행 (Invoke 또는 코루틴으로 딜레이)

private void NextStage()
// - currentStage++
// - ClearStage()
// - SetupStage(currentStage)
// - SpawnCannonball()
```

#### 3. PileScript (LiveCode/AngryClawd/PileScript.cs)

**역할**: pile prefab에 부착. Start() 시 런타임으로 큐브 더미를 생성한다.

**주요 필드**:
```csharp
// === 빌딩 설정 ===
private const int MIN_WIDTH = 2;           // 최소 가로 큐브 수
private const int MAX_WIDTH = 4;           // 최대 가로 큐브 수
private const int MIN_HEIGHT = 3;          // 최소 세로 큐브 수
private const int MAX_HEIGHT = 6;          // 최대 세로 큐브 수
private const float CUBE_SIZE = 0.8f;      // 큐브 크기
private const float BOMB_CHANCE = 0.05f;   // 폭탄 생성 확률 (5%)

// === 머티리얼 GUID ===
private const string MAT_BLOCK_GUID = "ae2c9f21-fb57-41b6-999e-38c4f9bded72";
private const string MAT_PIG_GUID   = "d80c9906-546b-4d30-a1ae-21ffd2221b0f";
private const string MAT_BOMB_GUID  = "08ba5c79-5dda-442a-9fc4-a223d45176df";

private bool pigPlaced = false;  // 이 pile에 pig가 배치되었는지
```

**주요 메서드**:

```csharp
public override void Start()
// - 기존 자식 Cube 제거 (prefab에 포함된 placeholder)
// - 랜덤 width/height 결정
// - pig 위치 미리 결정: 랜덤 (x, y) 좌표 1개
// - 2중 루프로 큐브 생성 (BuildPile)

private void BuildPile()
// - int width = Random.Range(MIN_WIDTH, MAX_WIDTH + 1)
// - int height = Random.Range(MIN_HEIGHT, MAX_HEIGHT + 1)
// - int pigX = Random.Range(0, width), pigY = Random.Range(1, height) -- pig는 바닥이 아닌 곳
// - for y in [0, height), for x in [0, width):
//     - 큐브 생성: GameObject.CreatePrimitive(PrimitiveType.Cube)
//     - 크기 설정: transform.localScale = Vector3.one * CUBE_SIZE
//     - 위치 계산: pile의 localPosition + offset
//       offsetX = (x - width/2f + 0.5f) * CUBE_SIZE
//       offsetY = CUBE_SIZE / 2f + y * CUBE_SIZE  (바닥 위에 쌓기)
//     - 부모 설정: transform.SetParent(this.transform)
//     - Rigidbody 추가 (mass = 0.5f)
//     - 타입 결정:
//       if (x == pigX && y == pigY): pig 큐브
//         - 머티리얼 = mat_pig, tag = "Pig"
//         - PigScript 추가
//       else if (Random.value < BOMB_CHANCE && pigPlaced): bomb 큐브
//         - 머티리얼 = mat_bomb, tag = "Bomb"
//         - BombScript 추가
//       else: 일반 블록
//         - 머티리얼 = mat_block, tag = "Block"
//         - BlockScript 추가
```

#### 4. CannonballScript (새 파일: LiveCode/AngryClawd/CannonballScript.cs)

**역할**: cannonball에 부착. 충돌 시 대상 타입에 따라 판정 처리.

**주요 필드**:
```csharp
private const float MIN_DESTROY_SPEED = 3.0f; // 블록 파괴 최소 상대 속도
private bool hasHit = false;  // 첫 충돌만 처리 (다중 충돌 방지)
```

**주요 메서드**:
```csharp
public override void OnCollisionEnter(Collision collision)
// - 충돌 대상의 tag 확인
// - "Pig": Object.Destroy(collision.gameObject)
// - "Block":
//     if (collision.relativeVelocity.magnitude >= MIN_DESTROY_SPEED)
//       Object.Destroy(collision.gameObject)
// - "Bomb": 폭발 처리
//     var bombScript = collision.gameObject.GetComponent<BombScript>()
//     if (bombScript != null) bombScript.Explode()
// - 첫 의미있는 충돌 후 cannonball도 일정 시간 후 삭제 (선택)
```

#### 5. BlockScript (새 파일: LiveCode/AngryClawd/BlockScript.cs)

**역할**: 일반 블록 큐브에 부착. 블록 간 물리 충돌 시 속도 기반 파괴 판정.

```csharp
using RoseEngine;

public class BlockScript : MonoBehaviour
{
    private const float BREAK_SPEED = 8.0f; // 블록끼리 부딪힐 때 파괴 속도 임계값

    public override void OnCollisionEnter(Collision collision)
    {
        // cannonball과의 충돌은 CannonballScript에서 처리하므로 여기서는 블록 간 연쇄만 처리
        // 블록끼리 빠른 속도로 충돌 시 자기 자신 파괴 (연쇄 효과)
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Bomb"))
        {
            if (collision.relativeVelocity.magnitude >= BREAK_SPEED)
            {
                Object.Destroy(gameObject);
            }
        }
    }
}
```

#### 6. PigScript (새 파일: LiveCode/AngryClawd/PigScript.cs)

**역할**: pig 큐브에 부착. 피격 판정 + 파괴.

```csharp
using RoseEngine;

public class PigScript : MonoBehaviour
{
    private const float KILL_SPEED = 2.0f; // pig 사망 최소 충돌 속도

    public override void OnCollisionEnter(Collision collision)
    {
        // cannonball이 직접 맞으면 CannonballScript에서 처리
        // 블록에 의한 간접 충돌로도 pig 사망 가능
        if (collision.relativeVelocity.magnitude >= KILL_SPEED)
        {
            Object.Destroy(gameObject);
        }
    }
}
```

#### 7. BombScript (새 파일: LiveCode/AngryClawd/BombScript.cs)

**역할**: 폭탄 큐브에 부착. 충돌 시 주변 3m 내 모든 큐브 삭제.

```csharp
using RoseEngine;

public class BombScript : MonoBehaviour
{
    private const float EXPLOSION_RADIUS = 3.0f;
    private const float TRIGGER_SPEED = 2.0f; // 폭발 트리거 최소 속도
    private bool hasExploded = false;

    public override void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        if (collision.relativeVelocity.magnitude >= TRIGGER_SPEED)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Physics.OverlapSphere로 주변 콜라이더 검색
        var colliders = Physics.OverlapSphere(transform.position, EXPLOSION_RADIUS);
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject) // 자기 자신 제외
            {
                Object.Destroy(col.gameObject);
            }
        }

        // 자기 자신도 삭제
        Object.Destroy(gameObject);
    }
}
```

### 머티리얼 로딩 전략

PileScript에서 큐브 생성 시 머티리얼을 적용해야 한다. `Resources.Load<Material>(path)` 사용:

```csharp
// PileScript.Start() 또는 별도 초기화에서
var matBlock = Resources.Load<Material>("Assets/AngryClawdAssets/mat_block.mat");
var matPig   = Resources.Load<Material>("Assets/AngryClawdAssets/mat_pig.mat");
var matBomb  = Resources.Load<Material>("Assets/AngryClawdAssets/mat_bomb.mat");

// 큐브 생성 후 적용
var renderer = cube.GetComponent<MeshRenderer>();
renderer.material = matBlock; // 또는 matPig, matBomb
```

### Tag 시스템

`FindGameObjectsWithTag`로 pig 생존 여부를 확인하려면 tag를 설정해야 한다.
IronRose의 `GameObject.tag`는 임의의 문자열을 할당 가능:

```csharp
cube.tag = "Pig";    // pig 큐브
cube.tag = "Block";  // 일반 블록
cube.tag = "Bomb";   // 폭탄 큐브
```

### Slingshot 메커닉 상세

`Camera.ScreenPointToRay`가 구현되었으므로, 마우스 드래그 + 레이캐스트 기반으로 구현한다:

```
마우스 드래그:
  1. MouseButtonDown(0):
     - aimStartPos = Input.mousePosition
     - ScreenPointToRay로 클릭 지점의 월드 좌표 계산 (ground raycast)
  2. MouseButton(0):
     - 실시간 드래그 — 현재 마우스 위치로 ScreenPointToRay
     - 시작점과 현재점 사이의 월드 벡터가 발사 방향의 반대 (새총 당기는 느낌)
  3. MouseButtonUp(0):
     - dragDelta = aimStartWorldPos - currentWorldPos (반대 방향 = 당긴 방향)
     - direction = dragDelta.normalized
     - 힘 크기: force = Mathf.Min(dragDelta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE)
     - Rigidbody.AddForce(direction * force, ForceMode.Impulse)

대안 (간단 버전):
  - ScreenPointToRay 없이 화면 좌표 X/Y를 월드 X/Y에 직접 매핑하는 방식도 가능
  - 측면 고정 카메라에서는 두 방식의 결과가 거의 동일
```

**카메라 위치 조정**: 현재 카메라는 (0, 3, -6)에 있어 약간 위에서 내려다보는 시점이다.
shooter 위치(-24.5, 0, 0)와 pile 위치(0, 0, 0)를 모두 볼 수 있도록 카메라를 조정해야 한다.

제안 카메라 위치: (약 -12, 8, -25) 정도로, 전체 필드를 측면에서 조망.
FOV와 정확한 위치는 rose-cli로 조정하면서 시각적으로 결정.

### 씬 구조 변경

현재 씬의 pile prefab 인스턴스와 cannonball prefab 인스턴스는 **제거**해야 한다.
게임이 런타임에 동적으로 생성하기 때문이다.

변경 후 씬 구성:
```
AngryClawd.scene
  Main Camera    -- 위치/회전 조정 필요
  ground         -- 기존 유지 (5x5 Plane) + 스케일 확대 고려 (shooter~pile 거리 커버)
  Spot Light     -- 기존 유지
  game           -- AngryClawdGame 컴포넌트 (SimpleGameBase 제거 가능)
  shooter        -- 기존 유지 (위치 참조용 빈 GO)
```

### 영향 범위

**수정 파일**:
| 파일 | 변경 내용 |
|------|-----------|
| `LiveCode/SimpleGameBase.cs` | 변경 없음 (현재 상태 유지) |
| `LiveCode/AngryClawd/AngryClawdGame.cs` | 전체 게임 로직 구현 |
| `LiveCode/AngryClawd/PileScript.cs` | 큐브 더미 생성 로직 구현 |

**새 파일**:
| 파일 | 역할 |
|------|------|
| `LiveCode/AngryClawd/CannonballScript.cs` | cannonball 충돌 판정 |
| `LiveCode/AngryClawd/BlockScript.cs` | 일반 블록 연쇄 파괴 |
| `LiveCode/AngryClawd/PigScript.cs` | pig 충돌 사망 처리 |
| `LiveCode/AngryClawd/BombScript.cs` | 폭탄 범위 폭발 |

**rose-cli 씬 편집 작업**:
| 작업 | 명령 |
|------|------|
| pile prefab 인스턴스 제거 | `go.destroy <pile_id>` |
| cannonball prefab 인스턴스 제거 | `go.destroy <cannonball_id>` |
| game GO에서 SimpleGameBase 제거 | `component.remove <game_id> SimpleGameBase` |
| 카메라 위치 조정 | `transform.set_position <cam_id> -12,8,-25` |
| 카메라 회전 조정 | `transform.set_rotation <cam_id> 15,0,0` (시각적 확인 후 결정) |
| ground 스케일 확대 | `transform.set_scale <ground_id> 10,10,10` (shooter~pile 거리 커버) |
| 씬 저장 | `scene.save` |

**rose-cli UI 작업** (Phase 5):
| 작업 | 명령 |
|------|------|
| Canvas 생성 | `ui.create_canvas GameCanvas` |
| Canvas 스케일 모드 설정 | `ui.canvas.set_scale_mode`, `ui.canvas.set_reference_resolution` |
| 상단 정보 바 (Panel + Text x3) | `ui.create_panel`, `ui.create_text` x3 |
| 중앙 메시지 (Panel + Text) | `ui.create_panel`, `ui.create_text` |
| 재시작 버튼 | `ui.create_button` |
| RectTransform 배치 | `ui.rect.set_preset`, `ui.rect.set_position`, `ui.rect.set_size` |
| UI 프리팹 저장 | `ui.prefab.save` |

**image-forge 에셋 작업** (Phase 5):
| 에셋 | 용도 |
|------|------|
| `Assets/AngryClawdAssets/UI/aim_arrow.png` (64x64) | 조준 방향 화살표 |
| `Assets/AngryClawdAssets/UI/pig_icon.png` (32x32) | pig 카운터 아이콘 |
| `Assets/AngryClawdAssets/UI/shot_icon.png` (32x32) | 발사 횟수 아이콘 |
| `Assets/AngryClawdAssets/UI/panel_bg.png` (64x64, 9-slice) | 패널 배경 |

---

## 구현 단계

### Phase 1: 에셋/씬 준비 (rose-cli)

씬에서 불필요한 오브젝트를 제거하고, 카메라/ground를 조정한다.
이 단계는 에디터가 실행 중인 상태에서 rose-cli로 수행한다.

- [ ] 1-1. 에디터 실행 확인 (`ping`)
- [ ] 1-2. `scene.tree`로 현재 씬 구조 확인, 각 GO의 ID 파악
- [ ] 1-3. pile prefab 인스턴스 제거 (`go.destroy`)
- [ ] 1-4. cannonball prefab 인스턴스 제거 (`go.destroy`)
- [ ] 1-5. game GO에서 SimpleGameBase 컴포넌트 제거 (`component.remove`)
- [ ] 1-6. ground 스케일 확대 -- shooter(-24.5)부터 pile 영역(~40)까지 커버
- [ ] 1-7. 카메라 위치/회전 조정 -- 전체 필드가 보이도록
- [ ] 1-8. 씬 저장 (`scene.save`)
- [ ] 1-9. 스크린샷으로 결과 확인

### Phase 2: 핵심 스크립트 구현

PileScript와 기본 게임 루프를 먼저 구현한다.

- [ ] 2-1. `PileScript.cs` 구현 -- 큐브 더미 동적 생성
  - 랜덤 width/height 결정
  - 큐브 생성 + Rigidbody 부착
  - pig/bomb/block 머티리얼 및 태그 설정
  - PigScript, BombScript, BlockScript 컴포넌트 부착
- [ ] 2-2. `BlockScript.cs` 구현 -- 블록 연쇄 파괴
- [ ] 2-3. `PigScript.cs` 구현 -- pig 충돌 사망
- [ ] 2-4. `BombScript.cs` 구현 -- 폭탄 범위 폭발
- [ ] 2-5. `CannonballScript.cs` 구현 -- cannonball 충돌 판정
- [ ] 2-6. 빌드 확인 (`dotnet build`)

### Phase 3: AngryClawdGame 게임 로직

전체 게임 흐름을 구현한다.

- [ ] 3-1. `AngryClawdGame.cs` 구현
  - Start(): 스테이지 생성 + cannonball 스폰
  - Update(): 조준/발사/추적/클리어 판정
  - SetupStage(): pile 배치 로직
  - SpawnCannonball(): cannonball 생성 + Rigidbody 설정
  - HandleAiming(): 마우스 드래그 slingshot
  - Fire(): Rigidbody Impulse 발사
  - TrackCannonball(): 타임아웃/낙하 체크
  - CheckStageClear(): pig 전멸 확인 + 다음 스테이지
- [ ] 3-2. 빌드 확인 (`dotnet build`)

### Phase 4: 테스트 및 밸런싱

에디터에서 플레이하며 밸런스를 조정한다.

- [ ] 4-1. 에디터에서 Play 모드로 테스트 (`play.enter`)
- [ ] 4-2. pile 크기/간격 밸런싱
- [ ] 4-3. 발사 힘/방향 감도 조정
- [ ] 4-4. 카메라 위치 미세 조정
- [ ] 4-5. 블록 파괴 속도 임계값 조정
- [ ] 4-6. 폭탄 확률/범위 조정
- [ ] 4-7. 스테이지 진행 밸런스 확인

### Phase 5: UI 구현

게임 HUD와 상태 표시 UI를 구현한다. rose-cli의 UI 시스템과 image-forge 스킬을 활용한다.

#### UI 구조 설계

```
Canvas (ScreenSpaceOverlay, ScaleWithScreenSize 1280x720)
├── TopPanel (상단 정보 바)
│   ├── StageText         -- "Stage 1" 표시
│   ├── PigCountText      -- 남은 pig 수 "Pigs: 3"
│   └── ShotCountText     -- 남은 발사 횟수 "Shots: 5"
├── CenterMessage         -- 스테이지 클리어/실패 메시지 (숨김 상태)
│   └── MessageText       -- "Stage Clear!" / "Game Over"
├── AimIndicator          -- 조준 UI (드래그 시 표시)
│   └── AimArrowImage     -- 발사 방향/힘 표시 화살표
└── BottomPanel           -- 하단 컨트롤
    └── RestartButton     -- 재시작 버튼
```

#### UI 에셋 (image-forge로 생성)

| 에셋 | 크기 | 설명 |
|------|------|------|
| `Assets/AngryClawdAssets/UI/aim_arrow.png` | 64x64 | 조준 방향 화살표 아이콘 (흰색, 투명 배경) |
| `Assets/AngryClawdAssets/UI/pig_icon.png` | 32x32 | pig 카운터 옆 아이콘 (노란색 사각형) |
| `Assets/AngryClawdAssets/UI/shot_icon.png` | 32x32 | 발사 횟수 옆 아이콘 (원형 탄환) |
| `Assets/AngryClawdAssets/UI/panel_bg.png` | 64x64 | 반투명 패널 배경 (9-slice용, border 16,16,16,16) |

#### UI 구현 단계

- [ ] 5-1. **image-forge로 UI 에셋 생성**
  - aim_arrow.png, pig_icon.png, shot_icon.png, panel_bg.png
  - 스프라이트 임포트 설정: `sprite.set_type <path> Sprite`, `sprite.set_filter <path> Point`
  - panel_bg는 9-slice 설정: `sprite.set_border <path> 16,16,16,16`

- [ ] 5-2. **rose-cli로 Canvas + 기본 UI 구조 생성**
  ```
  ui.create_canvas GameCanvas
  ui.canvas.set_scale_mode <canvasId> ScaleWithScreenSize
  ui.canvas.set_reference_resolution <canvasId> 1280,720
  ```

- [ ] 5-3. **상단 정보 바 구성** (rose-cli)
  ```
  ui.create_panel <canvasId> 0.1,0.1,0.1,0.7      → TopPanel
  ui.rect.set_preset <topPanelId> TopStretch
  ui.rect.set_size <topPanelId> 0,50

  ui.create_text <topPanelId> "Stage 1" 28          → StageText
  ui.rect.set_preset <stageTextId> MiddleLeft
  ui.rect.set_position <stageTextId> 20,0

  ui.create_text <topPanelId> "Pigs: 0" 24          → PigCountText
  ui.rect.set_preset <pigTextId> MiddleCenter

  ui.create_text <topPanelId> "Shots: 5" 24         → ShotCountText
  ui.rect.set_preset <shotTextId> MiddleRight
  ui.rect.set_position <shotTextId> -20,0
  ```

- [ ] 5-4. **중앙 메시지 (클리어/실패)**
  ```
  ui.create_panel <canvasId> 0,0,0,0.8              → CenterMessage
  ui.rect.set_preset <msgPanelId> MiddleCenter
  ui.rect.set_size <msgPanelId> 500,120

  ui.create_text <msgPanelId> "Stage Clear!" 48      → MessageText
  ui.rect.set_preset <msgTextId> StretchAll
  ui.text.set_alignment <msgTextId> MiddleCenter
  ui.text.set_color <msgTextId> 1,1,0,1
  ```
  - 기본 상태: `go.set_active <msgPanelId> false`

- [ ] 5-5. **재시작 버튼**
  ```
  ui.create_button <canvasId> "Restart"              → RestartButton
  ui.rect.set_preset <btnId> BottomRight
  ui.rect.set_position <btnId> -20,20
  ui.rect.set_size <btnId> 120,40
  ```

- [ ] 5-6. **UI 프리팹으로 저장**
  ```
  ui.prefab.save <canvasId> Assets/AngryClawdAssets/UI/GameCanvas.prefab
  ```

- [ ] 5-7. **AngryClawdGame에 UI 로직 추가**
  - UIText 참조 필드 추가 (GameObject.Find로 찾기)
  - UpdateUI() 메서드: 매 프레임 pig 카운트, shot 카운트, 스테이지 번호 갱신
  - ShowMessage(string msg): 중앙 메시지 표시 (SetActive + 텍스트 설정)
  - HideMessage(): 중앙 메시지 숨기기
  - 스테이지 클리어 시 "Stage Clear!" 표시 → 2초 후 다음 스테이지
  - 발사 횟수 소진 시 "Game Over" 표시 + 재시작 버튼 활성화

#### UI 스크립트 변경 (AngryClawdGame.cs에 추가)

```csharp
// === UI 참조 ===
private GameObject? uiCanvas;
private GameObject? stageText;
private GameObject? pigCountText;
private GameObject? shotCountText;
private GameObject? centerMessage;
private GameObject? messageText;

// === 발사 횟수 ===
private int shotsRemaining;
private const int SHOTS_PER_STAGE_BASE = 3;  // 기본 발사 횟수
private const int SHOTS_PER_PILE = 2;        // pile당 추가 발사 횟수

// Start()에서 UI 초기화
private void InitUI()
// - GameObject.Find("StageText") 등으로 UI 요소 찾기
// - 또는 Canvas prefab을 인스턴스화 후 transform.Find으로 검색

// Update()에서 UI 갱신
private void UpdateUI()
// - stageText: "Stage {currentStage}"
// - pigCountText: "Pigs: {remainingPigs}"
// - shotCountText: "Shots: {shotsRemaining}"
// UIText 컴포넌트 접근: go.GetComponent<UIText>().text = "..."

private void ShowMessage(string msg)
// - centerMessage.SetActive(true)
// - messageText.GetComponent<UIText>().text = msg

private void HideMessage()
// - centerMessage.SetActive(false)
```

### Phase 6: 폴리싱 (선택)

게임 느낌을 개선하는 추가 작업.

- [ ] 6-1. cannonball 재장전 시 shooter 위치로 부드럽게 이동
- [ ] 6-2. 큐브 파괴 시 간단한 이펙트 (스케일 축소 + Destroy)
- [ ] 6-3. ground 머티리얼 변경 (시각적 구분)
- [ ] 6-4. 조준 시 드래그 방향에 따라 AimArrow 회전/크기 변경
- [ ] 6-5. 폭탄 폭발 시 카메라 흔들림 효과 (선택)

---

## 대안 검토

### Slingshot 입력 방식

| 방안 | 장점 | 단점 | 선택 |
|------|------|------|------|
| **A. 마우스 드래그 (화면 좌표 -> 월드 XY 매핑)** | 구현 간단, 직관적 | 3D 깊이감 없음 | 선택 |
| B. Physics.Raycast 기반 (ground 클릭) | 3D 좌표 정확 | 조준 UX 복잡, slingshot 느낌 약함 | - |
| C. Camera.ScreenPointToRay | 가장 정확 | ~~엔진 미구현~~ **구현 완료** | 대안 (필요시 전환 가능) |

### Pig 생존 확인 방식

| 방안 | 장점 | 단점 | 선택 |
|------|------|------|------|
| **A. FindGameObjectsWithTag("Pig")** | 간단, 확실 | 매 프레임 검색은 비효율 | 선택 (주기적 체크) |
| B. pig 카운터 관리 (PigScript.OnDestroy에서 감소) | 효율적 | OnDestroy 타이밍 복잡 | 대안 |

### 큐브 머티리얼 적용 방식

| 방안 | 장점 | 단점 | 선택 |
|------|------|------|------|
| **A. Resources.Load<Material>(path)** | 간단, 경로 기반 | - | 선택 |
| B. AssetDatabase.LoadByGuid<Material>(guid) | GUID 기반, 리네임에 강함 | Resources.GetAssetDatabase() 호출 필요 | 대안 |

---

## 미결 사항

1. **카메라 정확한 위치/회전**: shooter(-24.5, 0, 0)부터 pile 영역(~30, 0, 0)까지 모두 보이는 카메라 위치는 에디터에서 시각적으로 결정해야 한다. Phase 1에서 rose-cli로 반복 조정한다.

2. **ground 크기**: 현재 ground는 5x5 스케일(실제 50x50 단위)이지만, shooter 위치(-24.5)와 pile 영역(~30 이상)을 모두 커버하려면 스케일 확대가 필요할 수 있다.

3. **cannonball prefab 재사용 여부**: 현재 cannonball.prefab에는 Clawd 캐릭터가 자식으로 포함되어 있다. PrefabUtility.InstantiatePrefab으로 매번 인스턴스화할 것이므로 그대로 사용 가능하지만, Rigidbody는 런타임에 AddComponent로 추가해야 한다.

4. **SimpleGameBase 컴포넌트**: 씬의 game GO에 SimpleGameBase와 AngryClawdGame이 모두 붙어 있다. AngryClawdGame이 SimpleGameBase를 상속하므로, 별도의 SimpleGameBase 컴포넌트는 제거해야 한다 (Start/Update 이중 호출 방지).
