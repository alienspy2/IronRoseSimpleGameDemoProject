# Phase 03: AngryClawdGame 게임 로직

## 목표
- AngryClawdGame.cs에 전체 게임 흐름을 구현한다.
- 스테이지 생성, 슈팅 메커닉(마우스 드래그 slingshot), cannonball 추적, 스테이지 클리어 판정을 모두 포함한다.

## 선행 조건
- Phase 01 완료 (씬에 pile/cannonball 인스턴스 제거 완료)
- Phase 02 완료 (PileScript, BlockScript, PigScript, BombScript, CannonballScript 구현 완료)
- 존재해야 하는 파일:
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PileScript.cs` (구현됨)
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/CannonballScript.cs` (구현됨)
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BlockScript.cs` (구현됨)
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PigScript.cs` (구현됨)
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BombScript.cs` (구현됨)
- 씬에 존재해야 하는 GO:
  - `shooter` -- 위치 (-24.5, 0, 0), cannonball 스폰 위치 참조용

## 수정할 파일

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/AngryClawdGame.cs`
- **변경 내용**: 빈 클래스에서 전체 게임 로직으로 전면 교체
- **이유**: 게임의 메인 컨트롤러. 스테이지 관리, 입력 처리, 승리 판정을 담당.

**전체 구현**:

```csharp
using RoseEngine;
using System.Collections.Generic;

public class AngryClawdGame : SimpleGameBase
{
    // === 프리팹 GUID ===
    public string pilePrefabGuid = "da096309-223c-488c-b39a-c5f62ba55fe0";
    public string cannonballPrefabGuid = "0804bc30-df11-4fd2-89a5-5265d7180ff2";

    // === 게임 상태 ===
    private int currentStage = 1;
    private bool isAiming = false;
    private Vector2 aimStartPos;
    private GameObject? currentCannonball;
    private List<GameObject> activePiles = new();
    private bool cannonballFired = false;
    private float fireTime;
    private float stageClearTime = -1f;
    private bool stageClearing = false;

    // === 상수 ===
    private const float SHOOT_FORCE_MULTIPLIER = 0.3f;
    private const float MAX_SHOOT_FORCE = 40f;
    private const float CANNONBALL_MASS = 2.0f;
    private const float CANNONBALL_TIMEOUT = 8.0f;
    private const float STAGE_CLEAR_DELAY = 2.0f;
    private const float PILE_SPACING = 8.0f;
    private const float PILE_START_X = 0.0f;
    private const int MAX_PILES = 5;

    public override void Start()
    {
        SetupStage(currentStage);
        SpawnCannonball();
    }

    public override void Update()
    {
        // 스테이지 클리어 딜레이 처리
        if (stageClearing)
        {
            if (Time.time - stageClearTime >= STAGE_CLEAR_DELAY)
            {
                stageClearing = false;
                NextStage();
            }
            return;
        }

        HandleAiming();
        TrackCannonball();
        CheckStageClear();
    }

    private void SetupStage(int stageNum)
    {
        // 기존 pile 정리
        ClearStage();

        // stageNum에 따라 pile 수 결정 (최대 MAX_PILES)
        int pileCount = Mathf.Min(1 + (stageNum - 1), MAX_PILES);

        for (int i = 0; i < pileCount; i++)
        {
            float xPos = PILE_START_X + i * PILE_SPACING;
            float zOffset = Random.Range(-1.0f, 1.0f);
            var position = new Vector3(xPos, 0f, zOffset);

            var pile = PrefabUtility.InstantiatePrefab(pilePrefabGuid, position, Quaternion.identity);
            if (pile != null)
            {
                activePiles.Add(pile);
            }
        }

        Debug.Log($"[AngryClawd] Stage {stageNum} setup: {pileCount} piles");
    }

    private void ClearStage()
    {
        // 활성 pile과 자식 모두 Destroy
        foreach (var pile in activePiles)
        {
            if (pile != null)
            {
                Object.Destroy(pile);
            }
        }
        activePiles.Clear();

        // 남아있는 cannonball Destroy
        if (currentCannonball != null)
        {
            Object.Destroy(currentCannonball);
            currentCannonball = null;
        }

        cannonballFired = false;
    }

    private void SpawnCannonball()
    {
        // shooter GO 위치 참조
        var shooter = GameObject.Find("shooter");
        if (shooter == null)
        {
            Debug.Log("[AngryClawd] shooter GO not found!");
            return;
        }

        var spawnPos = shooter.transform.position + new Vector3(0f, 1.5f, 0f);
        currentCannonball = PrefabUtility.InstantiatePrefab(cannonballPrefabGuid, spawnPos, Quaternion.identity);

        if (currentCannonball != null)
        {
            // Rigidbody 추가, 발사 전에는 kinematic으로 정지
            var rb = currentCannonball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentCannonball.AddComponent<Rigidbody>();
            }
            rb.mass = CANNONBALL_MASS;
            rb.isKinematic = true;

            // CannonballScript 추가
            if (currentCannonball.GetComponent<CannonballScript>() == null)
            {
                currentCannonball.AddComponent<CannonballScript>();
            }

            cannonballFired = false;
        }

        Debug.Log("[AngryClawd] Cannonball spawned");
    }

    private void HandleAiming()
    {
        if (cannonballFired) return;
        if (currentCannonball == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
            aimStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            isAiming = false;
            Fire();
        }
    }

    private void Fire()
    {
        if (currentCannonball == null) return;

        // 드래그 벡터 계산: 시작점 - 끝점 (반대 방향 = 새총 당기는 느낌)
        Vector2 mouseEndPos = Input.mousePosition;
        Vector2 delta = aimStartPos - mouseEndPos;

        // 최소 드래그 거리 체크
        if (delta.magnitude < 10f) return;

        // 힘 크기 계산
        float force = Mathf.Min(delta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE);

        // 발사 방향: 화면 X/Y를 월드 X/Y에 매핑 (측면 고정 카메라)
        // 화면 좌표계: Y는 위가 양수 방향이 아닐 수 있으므로 주의
        // IronRose의 mousePosition은 Silk.NET 기반 -- 좌상단 원점, Y 아래로 증가
        // 따라서 deltaY의 부호를 반전하여 월드 Y에 매핑
        var direction = new Vector3(delta.x, -delta.y, 0f).normalized;

        // Rigidbody isKinematic 해제 후 Impulse 발사
        var rb = currentCannonball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        cannonballFired = true;
        fireTime = Time.time;

        Debug.Log($"[AngryClawd] Fired! dir={direction}, force={force:F1}");
    }

    private void TrackCannonball()
    {
        if (!cannonballFired) return;
        if (currentCannonball == null)
        {
            // cannonball이 이미 파괴됨 (폭탄 등에 의해)
            SpawnCannonball();
            return;
        }

        // 타임아웃 체크
        if (Time.time - fireTime > CANNONBALL_TIMEOUT)
        {
            Debug.Log("[AngryClawd] Cannonball timeout, respawning");
            Object.Destroy(currentCannonball);
            currentCannonball = null;
            SpawnCannonball();
            return;
        }

        // 낙하 체크 (y < -10)
        if (currentCannonball.transform.position.y < -10f)
        {
            Debug.Log("[AngryClawd] Cannonball fell off, respawning");
            Object.Destroy(currentCannonball);
            currentCannonball = null;
            SpawnCannonball();
            return;
        }
    }

    private void CheckStageClear()
    {
        if (!cannonballFired) return;
        if (stageClearing) return;

        // 모든 pig 태그 오브젝트 검색
        var pigs = GameObject.FindGameObjectsWithTag("Pig");
        if (pigs.Length == 0)
        {
            Debug.Log($"[AngryClawd] Stage {currentStage} clear!");
            stageClearing = true;
            stageClearTime = Time.time;
        }
    }

    private void NextStage()
    {
        currentStage++;
        Debug.Log($"[AngryClawd] Moving to stage {currentStage}");
        SetupStage(currentStage);
        SpawnCannonball();
    }
}
```

**주요 API 시그니처 참조**:

- `PrefabUtility.InstantiatePrefab(string prefabGuid, Vector3 position, Quaternion rotation)` -- `GameObject?` 반환
- `GameObject.Find(string name)` -- `GameObject?` 반환 (이름 정확 매칭)
- `GameObject.FindGameObjectsWithTag(string tag)` -- `GameObject[]` 반환
- `Input.GetMouseButtonDown(int button)` -- 마우스 버튼 눌림 (0=Left)
- `Input.GetMouseButtonUp(int button)` -- 마우스 버튼 뗌
- `Input.GetMouseButton(int button)` -- 마우스 버튼 누르고 있는 중
- `Input.mousePosition` -- `Vector2` (IronRose에서는 좌상단 원점, Y 아래로 증가)
- `Time.time` -- `float` (게임 시작 후 경과 시간)
- `Rigidbody.isKinematic` -- `bool` (set 가능)
- `Rigidbody.mass` -- `float` (set 가능)
- `Rigidbody.AddForce(Vector3 force, ForceMode mode)` -- ForceMode.Impulse 사용
- `Object.Destroy(Object obj, float t = 0f)` -- 즉시 또는 t초 후 파괴
- `Quaternion.identity` -- (0,0,0,1)
- `Vector3.normalized` -- 정규화된 벡터 반환
- `Vector2.magnitude` -- 벡터 크기
- `Random.Range(float min, float max)` -- 실수 범위 랜덤
- `Mathf.Min(float a, float b)` -- 작은 값 반환
- `Debug.Log(string message)` -- 콘솔 로그 (using RoseEngine)
- `MonoBehaviour.Invoke(string methodName, float time)` -- 딜레이 호출 (사용하지 않고 수동 타이머 사용)

## NuGet 패키지
- 없음

## 빌드 명령
```bash
cd /home/alienspy/git/MyGame && dotnet build LiveCode/LiveCode.csproj
```

## 검증 기준
- [ ] `dotnet build` 성공 (에러 없음)
- [ ] AngryClawdGame.cs에 다음 메서드가 모두 구현됨:
  - Start(), Update()
  - SetupStage(int), ClearStage()
  - SpawnCannonball()
  - HandleAiming(), Fire()
  - TrackCannonball()
  - CheckStageClear(), NextStage()
- [ ] 에디터에서 Play 모드 진입 시:
  - pile이 동적으로 생성됨
  - cannonball이 shooter 위치에 스폰됨
  - 마우스 드래그로 cannonball 발사 가능
  - pig 전멸 시 다음 스테이지로 진행

## 참고
- **파일 인코딩**: UTF-8 with BOM
- **마우스 좌표계 주의**: IronRose의 Input.mousePosition은 Silk.NET 기반으로 **좌상단 원점, Y 아래로 증가**한다. 따라서 드래그 delta의 Y축 부호를 반전하여 월드 Y(위가 양수)에 매핑해야 한다.
- **측면 고정 카메라**: 카메라가 Z축 뒤쪽에서 바라보므로, 화면 X는 월드 X, 화면 Y는 월드 Y에 거의 직접 매핑된다. ScreenPointToRay 기반 정밀 조준은 필요하지 않다.
- **스테이지 클리어 딜레이**: 즉시 다음 스테이지로 넘기지 않고 STAGE_CLEAR_DELAY(2초) 대기 후 NextStage() 호출. Invoke 대신 수동 타이머(stageClearing, stageClearTime)를 사용하여 구현이 단순하다.
- **cannonball 파괴 감지**: TrackCannonball()에서 `currentCannonball == null` 체크로 Object.Destroy 후 fake null 패턴이 동작한다 (IronRose는 Unity와 동일한 fake null 패턴 지원).
- **`using System.Collections.Generic`**: List<GameObject>를 사용하므로 필요하다. `ImplicitUsings`이 enable이므로 실제로는 자동 포함되지만, 명시적으로 적는 것이 안전하다.
