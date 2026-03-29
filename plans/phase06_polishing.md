# Phase 06: 폴리싱 (선택)

## 목표
- 게임 느낌을 개선하는 추가 작업을 수행한다.
- 이 Phase는 선택 사항이며, 기본 게임이 완성된 후 시간 여유가 있을 때 수행한다.

## 선행 조건
- Phase 01 ~ 05b 완료 (게임 기본 + UI 완성)

## 작업 항목

### 6-1. Ground 머티리얼 변경 (rose-cli)

ground의 기본 흰색 머티리얼을 시각적으로 구분 가능한 색상으로 변경한다.

```
material.set_color <ground_id> 0.4,0.6,0.3,1.0
```
- 초록빛 잔디 느낌의 색상

### 6-2. Cannonball 재장전 시 부드러운 이동 (코드)

현재는 SpawnCannonball()에서 즉시 shooter 위치에 생성하지만, 시각적으로 부드러운 이동 효과를 줄 수 있다.

**AngryClawdGame.cs 수정**:

```csharp
// === 추가 필드 ===
private bool isReloading = false;
private Vector3 reloadStartPos;
private Vector3 reloadEndPos;
private float reloadStartTime;
private const float RELOAD_DURATION = 0.5f;
```

Update()에 리로딩 로직 추가:
```csharp
if (isReloading && currentCannonball != null)
{
    float t = (Time.time - reloadStartTime) / RELOAD_DURATION;
    if (t >= 1.0f)
    {
        currentCannonball.transform.position = reloadEndPos;
        isReloading = false;
    }
    else
    {
        currentCannonball.transform.position = Vector3.Lerp(reloadStartPos, reloadEndPos, t);
    }
}
```

SpawnCannonball()에서 리로딩 시작:
```csharp
// cannonball을 화면 밖에서 생성 후 목표 위치로 이동
var offscreenPos = spawnPos + new Vector3(-5f, 5f, 0f);
currentCannonball = PrefabUtility.InstantiatePrefab(cannonballPrefabGuid, offscreenPos, Quaternion.identity);
// ...
isReloading = true;
reloadStartPos = offscreenPos;
reloadEndPos = spawnPos;
reloadStartTime = Time.time;
```

### 6-3. 조준 시 AimArrow 회전/크기 변경 (코드 + UI)

드래그 중에 AimArrowImage의 방향과 크기를 조준 방향에 맞춰 변경한다.

**필요 사항**:
- Phase 05a에서 AimIndicator UI 요소가 생성되어야 함
- AngryClawdGame에서 AimArrowImage 참조 + 드래그 중 업데이트 로직

이 항목은 UI 이미지 회전이 필요하므로, RectTransform 회전 지원 여부를 확인해야 한다.

### 6-4. 큐브 파괴 시 스케일 축소 효과 (코드)

블록이 파괴될 때 즉시 Destroy 대신, 스케일을 축소한 후 Destroy한다.

**BlockScript.cs 수정 예시**:
```csharp
// Object.Destroy(gameObject) 대신:
StartCoroutine(DestroyWithShrink());

private System.Collections.IEnumerator DestroyWithShrink()
{
    float duration = 0.2f;
    float elapsed = 0f;
    Vector3 originalScale = transform.localScale;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
        yield return null;
    }

    Object.Destroy(gameObject);
}
```

PigScript, BombScript에도 동일 패턴 적용 가능.

**주요 API 참조**:
- `MonoBehaviour.StartCoroutine(IEnumerator routine)` -- `Coroutine` 반환
- `yield return null` -- 다음 프레임까지 대기
- `Vector3.Lerp(Vector3 a, Vector3 b, float t)` -- 선형 보간 (IronRose에서 지원 확인 필요)
- `Vector3.zero` -- (0,0,0)
- `Time.deltaTime` -- 프레임 간 시간

### 6-5. 폭탄 폭발 시 카메라 흔들림 (코드)

카메라 shake 효과를 추가한다.

**AngryClawdGame.cs에 추가**:
```csharp
private bool cameraShaking = false;
private float shakeEndTime;
private Vector3 cameraOriginalPos;
private const float SHAKE_DURATION = 0.3f;
private const float SHAKE_INTENSITY = 0.3f;
```

폭발 시 shake 트리거, Update에서 shake 처리.
이 기능은 BombScript에서 AngryClawdGame으로 이벤트를 전달하는 방법이 필요하다.
가장 간단한 방법: BombScript.Explode()에서 static 이벤트 발생, 또는 AngryClawdGame에서 폭발을 감지.

## 빌드 명령
```bash
cd /home/alienspy/git/MyGame && dotnet build LiveCode/LiveCode.csproj
```

## 검증 기준
- [ ] `dotnet build` 성공
- [ ] 적용한 폴리싱 항목이 Play 모드에서 동작함
- [ ] 기존 게임 로직이 깨지지 않음

## 참고
- 이 Phase의 각 항목은 독립적이므로, 원하는 항목만 선택적으로 구현할 수 있다.
- `Vector3.Lerp` 지원 여부를 IronRose 엔진에서 확인해야 한다. 없으면 수동 보간(`a + (b - a) * t`)으로 대체.
- 카메라 shake는 에디터 카메라와 게임 카메라가 다를 수 있으므로, `Camera.main`의 transform을 직접 수정하는 방식으로 구현한다.
- 코루틴은 IronRose에서 지원된다 (`MonoBehaviour.StartCoroutine`).
