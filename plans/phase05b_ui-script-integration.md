# Phase 05b: UI 스크립트 통합

## 목표
- AngryClawdGame.cs에 UI 로직을 추가한다.
- 스테이지 번호, pig 카운트, 발사 횟수를 HUD에 표시한다.
- 스테이지 클리어/게임오버 메시지를 표시한다.
- 재시작 버튼 기능을 연결한다.

## 선행 조건
- Phase 03 완료 (AngryClawdGame 게임 로직 구현)
- Phase 05a 완료 (Canvas + UI 요소가 씬에 존재)
- 씬에 다음 이름의 GO가 존재해야 함:
  - `StageText` -- UIText 컴포넌트 보유
  - `PigCountText` -- UIText 컴포넌트 보유
  - `ShotCountText` -- UIText 컴포넌트 보유
  - `CenterMessage` -- UIPanel 컴포넌트 보유 (비활성 상태)
  - `MessageText` -- UIText 컴포넌트 보유 (CenterMessage 자식)
  - `RestartButton` -- UIButton 컴포넌트 보유

## 수정할 파일

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/AngryClawdGame.cs`
- **변경 내용**: UI 참조 필드, 발사 횟수 시스템, UI 갱신/메시지 표시 로직 추가
- **이유**: HUD와 게임 상태 메시지를 연동하기 위해

**변경 사항 상세**:

#### 1. 추가할 필드 (게임 상태 영역)

```csharp
// === UI 참조 ===
private GameObject? stageText;
private GameObject? pigCountText;
private GameObject? shotCountText;
private GameObject? centerMessage;
private GameObject? messageText;
private GameObject? restartButton;

// === 발사 횟수 ===
private int shotsRemaining;
private const int SHOTS_PER_STAGE_BASE = 3;
private const int SHOTS_PER_PILE = 2;

// === 게임 오버 상태 ===
private bool gameOver = false;
```

#### 2. Start() 메서드 수정

기존 Start() 끝에 UI 초기화를 추가한다:

```csharp
public override void Start()
{
    SetupStage(currentStage);
    SpawnCannonball();
    InitUI();
}
```

#### 3. 새 메서드: InitUI()

```csharp
private void InitUI()
{
    stageText = GameObject.Find("StageText");
    pigCountText = GameObject.Find("PigCountText");
    shotCountText = GameObject.Find("ShotCountText");
    centerMessage = GameObject.Find("CenterMessage");
    messageText = GameObject.Find("MessageText");
    restartButton = GameObject.Find("RestartButton");

    // CenterMessage 숨기기
    if (centerMessage != null) centerMessage.SetActive(false);

    // Restart 버튼 onClick 연결
    if (restartButton != null)
    {
        var btn = restartButton.GetComponent<UIButton>();
        if (btn != null)
        {
            btn.onClick = OnRestartClicked;
        }
    }

    UpdateUI();
}
```

#### 4. Update() 메서드 수정

UpdateUI() 호출을 추가한다:

```csharp
public override void Update()
{
    if (gameOver) return;

    if (stageClearing)
    {
        if (Time.time - stageClearTime >= STAGE_CLEAR_DELAY)
        {
            stageClearing = false;
            HideMessage();
            NextStage();
        }
        return;
    }

    HandleAiming();
    TrackCannonball();
    CheckStageClear();
    UpdateUI();
}
```

#### 5. 새 메서드: UpdateUI()

```csharp
private void UpdateUI()
{
    if (stageText != null)
    {
        var text = stageText.GetComponent<UIText>();
        if (text != null) text.text = $"Stage {currentStage}";
    }

    if (pigCountText != null)
    {
        var pigs = GameObject.FindGameObjectsWithTag("Pig");
        var text = pigCountText.GetComponent<UIText>();
        if (text != null) text.text = $"Pigs: {pigs.Length}";
    }

    if (shotCountText != null)
    {
        var text = shotCountText.GetComponent<UIText>();
        if (text != null) text.text = $"Shots: {shotsRemaining}";
    }
}
```

#### 6. 새 메서드: ShowMessage(), HideMessage()

```csharp
private void ShowMessage(string msg)
{
    if (centerMessage != null) centerMessage.SetActive(true);
    if (messageText != null)
    {
        var text = messageText.GetComponent<UIText>();
        if (text != null) text.text = msg;
    }
}

private void HideMessage()
{
    if (centerMessage != null) centerMessage.SetActive(false);
}
```

#### 7. 새 메서드: OnRestartClicked()

```csharp
private void OnRestartClicked()
{
    gameOver = false;
    currentStage = 1;
    HideMessage();
    SetupStage(currentStage);
    SpawnCannonball();
}
```

#### 8. SetupStage() 수정

발사 횟수 초기화를 추가한다:

```csharp
private void SetupStage(int stageNum)
{
    ClearStage();

    int pileCount = Mathf.Min(1 + (stageNum - 1), MAX_PILES);

    // 발사 횟수 설정
    shotsRemaining = SHOTS_PER_STAGE_BASE + pileCount * SHOTS_PER_PILE;

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

    Debug.Log($"[AngryClawd] Stage {stageNum} setup: {pileCount} piles, {shotsRemaining} shots");
}
```

#### 9. Fire() 수정

발사 횟수 차감을 추가한다:

```csharp
private void Fire()
{
    if (currentCannonball == null) return;

    Vector2 mouseEndPos = Input.mousePosition;
    Vector2 delta = aimStartPos - mouseEndPos;

    if (delta.magnitude < 10f) return;

    float force = Mathf.Min(delta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE);
    var direction = new Vector3(delta.x, -delta.y, 0f).normalized;

    var rb = currentCannonball.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    cannonballFired = true;
    fireTime = Time.time;
    shotsRemaining--;

    Debug.Log($"[AngryClawd] Fired! dir={direction}, force={force:F1}, shotsRemaining={shotsRemaining}");
}
```

#### 10. CheckStageClear() 수정

스테이지 클리어 메시지 표시를 추가한다:

```csharp
private void CheckStageClear()
{
    if (!cannonballFired) return;
    if (stageClearing) return;

    var pigs = GameObject.FindGameObjectsWithTag("Pig");
    if (pigs.Length == 0)
    {
        Debug.Log($"[AngryClawd] Stage {currentStage} clear!");
        stageClearing = true;
        stageClearTime = Time.time;
        ShowMessage("Stage Clear!");
    }
}
```

#### 11. TrackCannonball() 수정

cannonball 정리 후 발사 횟수 체크를 추가한다:

```csharp
private void TrackCannonball()
{
    if (!cannonballFired) return;
    if (currentCannonball == null)
    {
        HandleCannonballDone();
        return;
    }

    if (Time.time - fireTime > CANNONBALL_TIMEOUT)
    {
        Debug.Log("[AngryClawd] Cannonball timeout, respawning");
        Object.Destroy(currentCannonball);
        currentCannonball = null;
        HandleCannonballDone();
        return;
    }

    if (currentCannonball.transform.position.y < -10f)
    {
        Debug.Log("[AngryClawd] Cannonball fell off, respawning");
        Object.Destroy(currentCannonball);
        currentCannonball = null;
        HandleCannonballDone();
        return;
    }
}
```

#### 12. 새 메서드: HandleCannonballDone()

```csharp
private void HandleCannonballDone()
{
    // 발사 횟수 소진 체크
    if (shotsRemaining <= 0)
    {
        // pig가 남아있는지 확인
        var pigs = GameObject.FindGameObjectsWithTag("Pig");
        if (pigs.Length > 0)
        {
            Debug.Log("[AngryClawd] Game Over! No shots remaining.");
            gameOver = true;
            ShowMessage("Game Over");
            return;
        }
    }

    SpawnCannonball();
}
```

#### 13. 전체 AngryClawdGame.cs 최종 코드

위의 모든 변경을 통합한 전체 파일:

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

    // === UI 참조 ===
    private GameObject? stageText;
    private GameObject? pigCountText;
    private GameObject? shotCountText;
    private GameObject? centerMessage;
    private GameObject? messageText;
    private GameObject? restartButton;

    // === 발사 횟수 ===
    private int shotsRemaining;
    private const int SHOTS_PER_STAGE_BASE = 3;
    private const int SHOTS_PER_PILE = 2;

    // === 게임 오버 상태 ===
    private bool gameOver = false;

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
        InitUI();
    }

    public override void Update()
    {
        if (gameOver) return;

        if (stageClearing)
        {
            if (Time.time - stageClearTime >= STAGE_CLEAR_DELAY)
            {
                stageClearing = false;
                HideMessage();
                NextStage();
            }
            return;
        }

        HandleAiming();
        TrackCannonball();
        CheckStageClear();
        UpdateUI();
    }

    private void SetupStage(int stageNum)
    {
        ClearStage();

        int pileCount = Mathf.Min(1 + (stageNum - 1), MAX_PILES);
        shotsRemaining = SHOTS_PER_STAGE_BASE + pileCount * SHOTS_PER_PILE;

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

        Debug.Log($"[AngryClawd] Stage {stageNum} setup: {pileCount} piles, {shotsRemaining} shots");
    }

    private void ClearStage()
    {
        foreach (var pile in activePiles)
        {
            if (pile != null)
            {
                Object.Destroy(pile);
            }
        }
        activePiles.Clear();

        if (currentCannonball != null)
        {
            Object.Destroy(currentCannonball);
            currentCannonball = null;
        }

        cannonballFired = false;
    }

    private void SpawnCannonball()
    {
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
            var rb = currentCannonball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentCannonball.AddComponent<Rigidbody>();
            }
            rb.mass = CANNONBALL_MASS;
            rb.isKinematic = true;

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

        Vector2 mouseEndPos = Input.mousePosition;
        Vector2 delta = aimStartPos - mouseEndPos;

        if (delta.magnitude < 10f) return;

        float force = Mathf.Min(delta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE);
        var direction = new Vector3(delta.x, -delta.y, 0f).normalized;

        var rb = currentCannonball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        cannonballFired = true;
        fireTime = Time.time;
        shotsRemaining--;

        Debug.Log($"[AngryClawd] Fired! dir={direction}, force={force:F1}, shotsRemaining={shotsRemaining}");
    }

    private void TrackCannonball()
    {
        if (!cannonballFired) return;
        if (currentCannonball == null)
        {
            HandleCannonballDone();
            return;
        }

        if (Time.time - fireTime > CANNONBALL_TIMEOUT)
        {
            Debug.Log("[AngryClawd] Cannonball timeout, respawning");
            Object.Destroy(currentCannonball);
            currentCannonball = null;
            HandleCannonballDone();
            return;
        }

        if (currentCannonball.transform.position.y < -10f)
        {
            Debug.Log("[AngryClawd] Cannonball fell off, respawning");
            Object.Destroy(currentCannonball);
            currentCannonball = null;
            HandleCannonballDone();
            return;
        }
    }

    private void HandleCannonballDone()
    {
        if (shotsRemaining <= 0)
        {
            var pigs = GameObject.FindGameObjectsWithTag("Pig");
            if (pigs.Length > 0)
            {
                Debug.Log("[AngryClawd] Game Over! No shots remaining.");
                gameOver = true;
                ShowMessage("Game Over");
                return;
            }
        }

        SpawnCannonball();
    }

    private void CheckStageClear()
    {
        if (!cannonballFired) return;
        if (stageClearing) return;

        var pigs = GameObject.FindGameObjectsWithTag("Pig");
        if (pigs.Length == 0)
        {
            Debug.Log($"[AngryClawd] Stage {currentStage} clear!");
            stageClearing = true;
            stageClearTime = Time.time;
            ShowMessage("Stage Clear!");
        }
    }

    private void NextStage()
    {
        currentStage++;
        Debug.Log($"[AngryClawd] Moving to stage {currentStage}");
        SetupStage(currentStage);
        SpawnCannonball();
    }

    // === UI 메서드 ===

    private void InitUI()
    {
        stageText = GameObject.Find("StageText");
        pigCountText = GameObject.Find("PigCountText");
        shotCountText = GameObject.Find("ShotCountText");
        centerMessage = GameObject.Find("CenterMessage");
        messageText = GameObject.Find("MessageText");
        restartButton = GameObject.Find("RestartButton");

        if (centerMessage != null) centerMessage.SetActive(false);

        if (restartButton != null)
        {
            var btn = restartButton.GetComponent<UIButton>();
            if (btn != null)
            {
                btn.onClick = OnRestartClicked;
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (stageText != null)
        {
            var text = stageText.GetComponent<UIText>();
            if (text != null) text.text = $"Stage {currentStage}";
        }

        if (pigCountText != null)
        {
            var pigs = GameObject.FindGameObjectsWithTag("Pig");
            var text = pigCountText.GetComponent<UIText>();
            if (text != null) text.text = $"Pigs: {pigs.Length}";
        }

        if (shotCountText != null)
        {
            var text = shotCountText.GetComponent<UIText>();
            if (text != null) text.text = $"Shots: {shotsRemaining}";
        }
    }

    private void ShowMessage(string msg)
    {
        if (centerMessage != null) centerMessage.SetActive(true);
        if (messageText != null)
        {
            var text = messageText.GetComponent<UIText>();
            if (text != null) text.text = msg;
        }
    }

    private void HideMessage()
    {
        if (centerMessage != null) centerMessage.SetActive(false);
    }

    private void OnRestartClicked()
    {
        gameOver = false;
        currentStage = 1;
        HideMessage();
        SetupStage(currentStage);
        SpawnCannonball();
    }
}
```

**주요 API 시그니처 참조** (Phase 03 추가분):
- `GameObject.Find(string name)` -- `GameObject?` 반환
- `go.SetActive(bool value)` -- GO 활성/비활성
- `go.GetComponent<UIText>()` -- `UIText?` 반환
- `go.GetComponent<UIButton>()` -- `UIButton?` 반환
- `UIText.text` -- `string` (읽기/쓰기 가능)
- `UIButton.onClick` -- `Action?` (delegate 할당)

## NuGet 패키지
- 없음

## 빌드 명령
```bash
cd /home/alienspy/git/MyGame && dotnet build LiveCode/LiveCode.csproj
```

## 검증 기준
- [ ] `dotnet build` 성공 (에러 없음)
- [ ] AngryClawdGame.cs에 UI 관련 메서드가 모두 구현됨:
  - InitUI(), UpdateUI(), ShowMessage(), HideMessage(), OnRestartClicked(), HandleCannonballDone()
- [ ] 에디터에서 Play 모드 진입 시:
  - 상단 HUD에 Stage, Pigs, Shots가 표시됨
  - 스테이지 클리어 시 "Stage Clear!" 메시지 표시
  - 발사 횟수 소진 시 "Game Over" 메시지 표시
  - Restart 버튼 클릭 시 스테이지 1부터 재시작

## 참고
- **파일 인코딩**: UTF-8 with BOM
- Phase 05a에서 UI 요소의 GO 이름을 정확히 설정해야 `GameObject.Find()`로 찾을 수 있다.
- `UIButton.onClick`은 `Action?` 타입이므로 메서드 그룹(OnRestartClicked)을 직접 할당할 수 있다.
- `UpdateUI()`에서 `FindGameObjectsWithTag("Pig")`를 매 프레임 호출한다. 성능이 문제가 되면 주기적 체크로 변경할 수 있지만, 데모 게임 규모에서는 무시할 수 있다.
- 이 Phase가 완료되면 Phase 03의 AngryClawdGame.cs는 이 Phase의 전체 코드로 **완전히 교체**된다 (Phase 03의 내용을 포함하며 확장).
