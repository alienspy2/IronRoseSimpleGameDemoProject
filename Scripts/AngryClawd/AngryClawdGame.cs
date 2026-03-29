// ------------------------------------------------------------
// @file    AngryClawdGame.cs
// @brief   AngryClawd 게임의 메인 컨트롤러. 스테이지 생성, slingshot 발사,
//          cannonball 추적, 스테이지 클리어 판정, HUD/메시지 UI를 담당한다.
// ------------------------------------------------------------
using RoseEngine;
using System.Collections;
using System.Collections.Generic;

public class AngryClawdGame : SimpleGameBase
{
    // === 프리팹 링크 ===
    public GameObject? pilePrefab;
    public GameObject? cannonballPrefab;

    // === 게임 상태 ===
    /// <summary>슬링샷이 한 번이라도 발사되었는지 여부. BombScript에서 참조하여 발사 전 폭발을 방지한다.</summary>
    public static bool HasFired { get; private set; }

    private int currentStage = 1;
    private bool isAiming = false;
    private Vector2 aimStartPos;
    private Vector3 cannonballRestPos;         // cannonball 대기 위치 (슬링샷 복귀용)
    private GameObject? currentCannonball;
    private List<GameObject> activePiles = new();
    private bool cannonballFired = false;
    private float fireTime;
    private float stageClearTime = -1f;
    private bool stageClearing = false;
    private int stageSetupFrame = -1;             // SetupStage가 호출된 프레임 (PileScript.Start 대기용)

    // === UI 참조 ===
    private GameObject? stageText;
    private GameObject? pigCountText;
    private GameObject? shotCountText;
    private GameObject? centerMessage;
    private GameObject? messageText;
    private GameObject? restartButton;
    private GameObject? exitDemoButton;

    // === 아이콘 UI ===
    private Sprite? pigIconSprite;
    private Sprite? shotIconSprite;
    private List<GameObject> pigIconGOs = new();
    private List<GameObject> shotIconGOs = new();
    private int lastPigCount = -1;
    private int lastShotCount = -1;
    private const float ICON_SIZE = 32f;
    private const float ICON_SPACING = 4f;

    // === 돼지 아이콘 죽음 애니메이션 ===
    private const float PIG_DEATH_DELAY = 0.5f;           // X 표시 후 축소 시작까지 대기
    private const float PIG_DEATH_SHRINK_DURATION = 0.3f;  // scale 축소 애니메이션 시간
    private int dyingPigIconCount;                          // 현재 죽음 애니메이션 중인 아이콘 수

    // 아이콘 스프라이트 링크
    public Sprite? pigIconSpritePrefab;
    public Sprite? shotIconSpritePrefab;

    // === 발사 횟수 ===
    private int shotsRemaining;
    private const int SHOTS_PER_STAGE_BASE = 3;
    private const int SHOTS_PER_PILE = 2;

    // === 게임 오버 상태 ===
    private bool gameOver = false;

    // === 카메라 자동 줌 ===
    private float cameraZVelocity;              // SmoothDamp 내부 상태
    private float cameraXVelocity;              // SmoothDamp 내부 상태
    private float cameraYVelocity;              // SmoothDamp 내부 상태
    private const float CAMERA_SMOOTH_TIME = 0.6f;   // SmoothDamp 평활 시간
    private const float CAMERA_PADDING = 4.0f;       // 바운딩 영역 여백 (월드 단위)
    private const float CAMERA_MIN_Z = -20.0f;       // 최소 후퇴 거리 (너무 가까이 오지 않도록)
    private const float CAMERA_Y_PADDING = 3.0f;     // Y축 상단 여백 (pile 높이 고려)
    private bool cameraYInitialized = false;    // 카메라 Y 고정 초기화 여부
    private float cameraFixedY;                 // 고정된 카메라 Y 위치

    // === 상수 ===
    private const float SHOOT_FORCE_MULTIPLIER = 0.5f;
    private const float MAX_SHOOT_FORCE = 50f;
    private const float SLING_PULL_SCALE = 0.02f;  // 드래그 픽셀 → 월드 이동 비율
    private const float MAX_PULL_DISTANCE = 3.0f;   // 최대 당기기 거리
    private const float CANNONBALL_MASS = 2.0f;
    private const float CANNONBALL_TIMEOUT = 8.0f;
    private const float STAGE_CLEAR_DELAY = 2.0f;
    private const float PILE_SPACING = 8.0f;
    private const float PILE_START_X = 0.0f;
    private const int MAX_PILES = 5;

    public override void Start()
    {
        HasFired = false;
        SetupStage(currentStage);
        SpawnCannonball();
        InitUI();
    }

    public override void Update()
    {
        if (gameOver) return;

        if (stageClearing)
        {
            UpdateCameraZoom();
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
        UpdateCameraZoom();
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
            var position = new Vector3(xPos, 0f, 0f);

            var pile = pilePrefab != null ? PrefabUtility.InstantiatePrefab(pilePrefab, position, Quaternion.identity) : null;
            if (pile != null)
            {
                activePiles.Add(pile);
            }
        }

        stageSetupFrame = Time.frameCount;

        Debug.Log($"[AngryClawd] Stage {stageNum} setup: {pileCount} piles, {shotsRemaining} shots");
    }

    private void ClearStage()
    {
        foreach (var pile in activePiles)
        {
            if (pile != null)
            {
                RoseEngine.Object.Destroy(pile);
            }
        }
        activePiles.Clear();

        if (currentCannonball != null)
        {
            RoseEngine.Object.Destroy(currentCannonball);
            currentCannonball = null;
        }

        cannonballFired = false;
        HasFired = false;

        // 아이콘 카운트 캐시 리셋 (UpdateUI에서 재생성됨)
        lastPigCount = -1;
        lastShotCount = -1;
        dyingPigIconCount = 0;
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
        currentCannonball = cannonballPrefab != null ? PrefabUtility.InstantiatePrefab(cannonballPrefab, spawnPos, Quaternion.identity) : null;

        if (currentCannonball != null)
        {
            // Rigidbody를 미리 추가해서 physics body가 등록되도록 함
            var rb = currentCannonball.GetComponent<Rigidbody>();
            if (rb == null)
                rb = currentCannonball.AddComponent<Rigidbody>();
            rb.mass = CANNONBALL_MASS;
            rb.useGravity = false;

            if (currentCannonball.GetComponent<CannonballScript>() == null)
                currentCannonball.AddComponent<CannonballScript>();

            cannonballRestPos = currentCannonball.transform.position;
            cannonballFired = false;
        }

        Debug.Log("[AngryClawd] Cannonball spawned (HOTRELOAD_TEST)");
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

        // 드래그 중: cannonball을 당긴 방향으로 이동 (슬링샷 시각 피드백)
        // 발사 전에는 velocity를 매 프레임 0으로 고정 (physics drift 방지)
        if (!cannonballFired)
        {
            var rb = currentCannonball.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;
        }

        if (Input.GetMouseButton(0) && isAiming)
        {
            Vector2 mouseCur = Input.mousePosition;
            Vector2 screenDelta = mouseCur - aimStartPos;
            var pullOffset = new Vector3(screenDelta.x, -screenDelta.y, 0f) * SLING_PULL_SCALE;
            if (pullOffset.magnitude > MAX_PULL_DISTANCE)
                pullOffset = pullOffset.normalized * MAX_PULL_DISTANCE;
            currentCannonball.transform.position = cannonballRestPos + pullOffset;
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
        // 발사 방향 = 당긴 반대 방향 (시작점 ← 끝점의 반대)
        Vector2 screenDelta = aimStartPos - mouseEndPos;

        if (screenDelta.magnitude < 10f)
        {
            // 드래그 너무 짧으면 cannonball을 원위치로 복귀
            currentCannonball.transform.position = cannonballRestPos;
            return;
        }

        float force = Mathf.Min(screenDelta.magnitude * SHOOT_FORCE_MULTIPLIER, MAX_SHOOT_FORCE);
        // screenDelta = aimStart - mouseEnd = 당긴 반대 = 발사 방향
        // 화면 Y는 아래가 +이므로 월드 Y만 반전
        var direction = new Vector3(screenDelta.x, -screenDelta.y, 0f).normalized;

        // cannonball을 원위치로 복귀 후 발사
        currentCannonball.transform.position = cannonballRestPos;

        var rb = currentCannonball.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.useGravity = true;
        rb.velocity = direction * (force / rb.mass);

        cannonballFired = true;
        HasFired = true;
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
            RoseEngine.Object.Destroy(currentCannonball);
            currentCannonball = null;
            HandleCannonballDone();
            return;
        }

        if (currentCannonball.transform.position.y < -10f)
        {
            Debug.Log("[AngryClawd] Cannonball fell off, respawning");
            RoseEngine.Object.Destroy(currentCannonball);
            currentCannonball = null;
            HandleCannonballDone();
            return;
        }

        // 카메라 뷰포트 밖으로 나간 포탄은 즉시 다음 샷으로 전환
        if (IsCannonballOutOfView())
        {
            Debug.Log("[AngryClawd] Cannonball out of camera view, respawning");
            RoseEngine.Object.Destroy(currentCannonball);
            currentCannonball = null;
            HandleCannonballDone();
            return;
        }
    }

    /// <summary>
    /// 포탄이 카메라 뷰포트 밖에 있는지 판정한다.
    /// perspective 카메라의 FOV와 aspect로 XY 평면상의 가시 영역을 계산하고,
    /// 여유 마진을 두어 화면 밖으로 충분히 벗어났을 때 true를 반환한다.
    /// </summary>
    private bool IsCannonballOutOfView()
    {
        if (currentCannonball == null) return false;

        var cam = Camera.main;
        if (cam == null) return false;

        var camPos = cam.transform.position;
        var ballPos = currentCannonball.transform.position;

        // 카메라에서 포탄까지의 Z 거리 (카메라는 -Z 방향을 바라봄)
        float zDist = Mathf.Abs(camPos.z - ballPos.z);
        if (zDist < 0.01f) return false;

        // perspective frustum의 가시 영역 계산
        float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float aspect = (float)Screen.width / Screen.height;
        float halfHeight = zDist * tanHalfFov;
        float halfWidth = halfHeight * aspect;

        // 여유 마진 (가시 영역의 10% 추가)
        const float OUT_OF_VIEW_MARGIN = 1.1f;
        float maxHalfWidth = halfWidth * OUT_OF_VIEW_MARGIN;
        float maxHalfHeight = halfHeight * OUT_OF_VIEW_MARGIN;

        // 카메라 중심 기준 포탄의 상대 위치
        float dx = Mathf.Abs(ballPos.x - camPos.x);
        float dy = Mathf.Abs(ballPos.y - camPos.y);

        return dx > maxHalfWidth || dy > maxHalfHeight;
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
        if (stageClearing) return;

        // PileScript.Start()는 다음 프레임에 호출되므로,
        // SetupStage 직후 프레임에서는 Pig가 아직 생성 안 됨 → 스킵
        if (Time.frameCount <= stageSetupFrame + 1) return;

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

    // === 카메라 자동 줌 ===

    private void UpdateCameraZoom()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // shooter 위치 (항상 포함)
        var shooter = GameObject.Find("shooter");
        float minX = shooter != null ? shooter.transform.position.x : -24.5f;
        float maxX = minX;
        float maxY = 5f; // 기본 최소 높이

        // 모든 활성 pile의 자식 블록 위치를 기반으로 바운딩 영역 계산
        foreach (var pile in activePiles)
        {
            if (pile == null) continue;
            var pt = pile.transform;
            for (int i = 0; i < pt.childCount; i++)
            {
                var childPos = pt.GetChild(i).position;
                if (childPos.x < minX) minX = childPos.x;
                if (childPos.x > maxX) maxX = childPos.x;
                if (childPos.y > maxY) maxY = childPos.y;
            }
            // pile 자체 위치도 포함 (자식이 없는 경우 대비)
            var pilePos = pt.position;
            if (pilePos.x < minX) minX = pilePos.x;
            if (pilePos.x > maxX) maxX = pilePos.x;
        }

        // 여백 추가
        minX -= CAMERA_PADDING;
        maxX += CAMERA_PADDING;
        maxY += CAMERA_Y_PADDING;

        // 바운딩 영역의 중심과 크기
        float centerX = (minX + maxX) * 0.5f;
        float halfWidth = (maxX - minX) * 0.5f;
        float halfHeight = maxY * 0.5f; // Y는 바닥(0)부터 maxY까지

        // 카메라 FOV 기반으로 필요한 Z 거리 계산
        // perspective 카메라: 뷰 높이 = 2 * |Z| * tan(fov/2)
        //                    뷰 너비 = 뷰 높이 * aspect
        float aspect = (float)Screen.width / Screen.height;
        float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // 높이 기준 필요 거리
        float requiredZForHeight = halfHeight / tanHalfFov;
        // 너비 기준 필요 거리
        float requiredZForWidth = halfWidth / (tanHalfFov * aspect);

        // 더 큰 값 사용 (양쪽 모두 화면에 들어오도록)
        float requiredZ = Mathf.Max(requiredZForHeight, requiredZForWidth);
        float targetZ = -Mathf.Max(requiredZ, -CAMERA_MIN_Z); // 음의 Z 방향, 최소 거리 보장

        // 카메라 Y: 포탄/지면(Y≈0)이 화면 하단 1/3에 보이도록 초기 계산 후 고정.
        // camY = halfViewHeight / 3 이면 지면(y=0)이 화면 하단 1/3에 위치.
        if (!cameraYInitialized)
        {
            float initZDist = Mathf.Abs(targetZ);
            float initHalfViewHeight = initZDist * tanHalfFov;
            cameraFixedY = initHalfViewHeight / 3f;
            cameraYInitialized = true;
        }

        // SmoothDamp로 부드럽게 이동 (X, Z만 추적, Y는 고정값으로 수렴)
        var camPos = cam.transform.position;
        float newX = Mathf.SmoothDamp(camPos.x, centerX, ref cameraXVelocity, CAMERA_SMOOTH_TIME);
        float newZ = Mathf.SmoothDamp(camPos.z, targetZ, ref cameraZVelocity, CAMERA_SMOOTH_TIME);
        float newY = Mathf.SmoothDamp(camPos.y, cameraFixedY, ref cameraYVelocity, CAMERA_SMOOTH_TIME);
        cam.transform.position = new Vector3(newX, newY, newZ);
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

        exitDemoButton = GameObject.Find("ExitDemoButton");
        if (exitDemoButton != null)
        {
            var btn = exitDemoButton.GetComponent<UIButton>();
            if (btn != null)
            {
                btn.onClick = OnExitDemoClicked;
            }
        }

        // 아이콘 스프라이트 로드 (인스펙터 링크)
        pigIconSprite = pigIconSpritePrefab;
        shotIconSprite = shotIconSpritePrefab;

        // 기존 UIText 숨기기 (텍스트를 빈 문자열로)
        if (pigCountText != null)
        {
            var text = pigCountText.GetComponent<UIText>();
            if (text != null) text.text = "";
        }
        if (shotCountText != null)
        {
            var text = shotCountText.GetComponent<UIText>();
            if (text != null) text.text = "";
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

        // 돼지 아이콘 갱신
        var pigs = GameObject.FindGameObjectsWithTag("Pig");
        int pigCount = pigs.Length;
        if (pigCount != lastPigCount)
        {
            int prevCount = lastPigCount;
            lastPigCount = pigCount;

            // 아이콘이 줄어드는 경우: 죽음 애니메이션 적용
            // prevCount == -1은 초기화/스테이지 전환이므로 즉시 재생성
            int aliveIconCount = pigIconGOs.Count - dyingPigIconCount;
            if (prevCount > 0 && pigCount < aliveIconCount)
            {
                int deathCount = aliveIconCount - pigCount;
                AnimatePigIconDeath(deathCount);
            }
            else
            {
                // 초기화, 증가, 또는 스테이지 전환: 전체 재생성
                dyingPigIconCount = 0;
                UpdateIconRow(pigCountText, pigIconSprite, pigIconGOs, pigCount);
            }
        }

        // 포탄 아이콘 갱신
        if (shotsRemaining != lastShotCount)
        {
            lastShotCount = shotsRemaining;
            UpdateIconRow(shotCountText, shotIconSprite, shotIconGOs, shotsRemaining);
        }
    }

    /// <summary>
    /// 부모 GO 아래에 아이콘 UIImage GO를 count개만큼 생성한다.
    /// 기존 아이콘은 모두 삭제 후 재생성한다.
    /// </summary>
    private void UpdateIconRow(GameObject? parent, Sprite? iconSprite, List<GameObject> iconGOs, int count)
    {
        // 기존 아이콘 삭제
        foreach (var go in iconGOs)
        {
            if (go != null) RoseEngine.Object.Destroy(go);
        }
        iconGOs.Clear();

        if (parent == null || iconSprite == null || count <= 0) return;

        // 총 폭 계산: count * ICON_SIZE + (count - 1) * ICON_SPACING
        float totalWidth = count * ICON_SIZE + (count - 1) * ICON_SPACING;

        for (int i = 0; i < count; i++)
        {
            var iconGO = new GameObject($"Icon_{i}");
            iconGO.transform.SetParent(parent.transform);

            var rt = iconGO.AddComponent<RectTransform>();
            // 부모의 중심 기준으로 아이콘을 가로로 배치
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            // X 좌표: 전체 폭의 왼쪽 끝 + i번째 아이콘 중심
            float x = -totalWidth * 0.5f + ICON_SIZE * 0.5f + i * (ICON_SIZE + ICON_SPACING);
            rt.anchoredPosition = new Vector2(x, 0f);

            var img = iconGO.AddComponent<UIImage>();
            img.sprite = iconSprite;
            img.color = Color.white;

            iconGOs.Add(iconGO);
        }
    }

    /// <summary>
    /// 돼지 아이콘에 죽음 애니메이션을 적용한다.
    /// 뒤쪽 아이콘부터 deathCount개를 대상으로:
    /// 1) "X" 텍스트 오버레이 추가 + 아이콘 빨간색 틴트
    /// 2) 0.5초 후 scale 축소 애니메이션
    /// 3) scale 0 도달 시 GO 제거
    /// </summary>
    private void AnimatePigIconDeath(int deathCount)
    {
        // 살아있는 아이콘 중 뒤쪽부터 선택 (dying 중인 것은 건너뜀)
        int aliveStartIndex = pigIconGOs.Count - dyingPigIconCount;
        for (int i = 0; i < deathCount; i++)
        {
            int targetIndex = aliveStartIndex - 1 - i;
            if (targetIndex < 0 || targetIndex >= pigIconGOs.Count) continue;

            var iconGO = pigIconGOs[targetIndex];
            if (iconGO == null) continue;

            // 아이콘을 빨간색으로 틴트
            var img = iconGO.GetComponent<UIImage>();
            if (img != null) img.color = new Color(1f, 0.3f, 0.3f, 1f);

            // "X" 오버레이 텍스트 추가
            var xGO = new GameObject("DeathX");
            xGO.transform.SetParent(iconGO.transform);

            var xRT = xGO.AddComponent<RectTransform>();
            xRT.anchorMin = new Vector2(0.5f, 0.5f);
            xRT.anchorMax = new Vector2(0.5f, 0.5f);
            xRT.pivot = new Vector2(0.5f, 0.5f);
            xRT.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);
            xRT.anchoredPosition = Vector2.zero;

            var xText = xGO.AddComponent<UIText>();
            xText.text = "X";
            xText.fontSize = ICON_SIZE * 0.8f;
            xText.color = Color.red;

            dyingPigIconCount++;
            StartCoroutine(PigIconDeathCoroutine(iconGO, targetIndex));
        }
    }

    /// <summary>
    /// 개별 돼지 아이콘의 죽음 애니메이션 코루틴.
    /// 대기 → 축소 → 제거 → 남은 아이콘 재정렬.
    /// </summary>
    private IEnumerator PigIconDeathCoroutine(GameObject iconGO, int listIndex)
    {
        // 1) X 표시 상태로 대기
        yield return new WaitForSeconds(PIG_DEATH_DELAY);

        // 2) scale 축소 애니메이션
        float elapsed = 0f;
        while (elapsed < PIG_DEATH_SHRINK_DURATION)
        {
            if (iconGO == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / PIG_DEATH_SHRINK_DURATION);
            float scale = Mathf.Lerp(1f, 0f, t);
            iconGO.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // 3) GO 제거
        if (iconGO != null)
        {
            pigIconGOs.Remove(iconGO);
            RoseEngine.Object.Destroy(iconGO);
        }

        dyingPigIconCount--;

        // 4) 남은 살아있는 아이콘 위치 재정렬
        RepositionPigIcons();
    }

    /// <summary>
    /// 살아있는 돼지 아이콘의 위치를 재계산하여 중앙 정렬한다.
    /// dying 중인 아이콘은 제자리를 유지하고, 살아있는 아이콘만 재정렬한다.
    /// </summary>
    private void RepositionPigIcons()
    {
        // 살아있는(dying이 아닌) 아이콘만 카운트
        int aliveCount = pigIconGOs.Count - dyingPigIconCount;
        if (aliveCount <= 0) return;

        float totalWidth = aliveCount * ICON_SIZE + (aliveCount - 1) * ICON_SPACING;
        int aliveIndex = 0;
        for (int i = 0; i < pigIconGOs.Count - dyingPigIconCount; i++)
        {
            var go = pigIconGOs[i];
            if (go == null) continue;

            var rt = go.GetComponent<RectTransform>();
            if (rt == null) continue;

            float x = -totalWidth * 0.5f + ICON_SIZE * 0.5f + aliveIndex * (ICON_SIZE + ICON_SPACING);
            rt.anchoredPosition = new Vector2(x, 0f);
            aliveIndex++;
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

    private void OnExitDemoClicked()
    {
        var scenePath = Path.Combine(Application.dataPath, "Scenes", "DemoLauncher.scene");
        SceneManager.LoadScene(scenePath);
    }

    private void OnRestartClicked()
    {
        gameOver = false;
        HasFired = false;
        currentStage = 1;
        lastPigCount = -1;
        lastShotCount = -1;
        dyingPigIconCount = 0;
        HideMessage();
        SetupStage(currentStage);
        SpawnCannonball();
    }
}
