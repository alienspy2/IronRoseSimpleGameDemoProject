# Phase 02: 핵심 스크립트 구현

## 목표
- PileScript에서 큐브 더미를 동적으로 생성하는 로직을 구현한다.
- BlockScript, PigScript, BombScript, CannonballScript 4개의 새 스크립트를 생성한다.
- 빌드가 성공하는 상태로 만든다.

## 선행 조건
- Phase 01 완료 (씬 준비)
- 기존 빈 파일 존재:
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PileScript.cs`
  - `/home/alienspy/git/MyGame/LiveCode/AngryClawd/AngryClawdGame.cs`
  - `/home/alienspy/git/MyGame/LiveCode/SimpleGameBase.cs`

## 생성할 파일

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BlockScript.cs`
- **역할**: 일반 블록 큐브에 부착. 블록 간 고속 충돌 시 연쇄 파괴 처리.
- **클래스**: `BlockScript : MonoBehaviour`
- **주요 멤버**:
  - `private const float BREAK_SPEED = 8.0f` -- 블록끼리 부딪힐 때 파괴 속도 임계값
  - `public override void OnCollisionEnter(Collision collision)` -- 충돌 판정
- **의존**: `RoseEngine` 네임스페이스 (MonoBehaviour, Collision, Object)
- **구현 힌트**:
  - `collision.gameObject.CompareTag("Block")` 또는 `CompareTag("Bomb")`인 경우에만 처리
  - `collision.relativeVelocity.magnitude >= BREAK_SPEED`이면 `Object.Destroy(gameObject)` 호출
  - cannonball과의 충돌은 CannonballScript에서 처리하므로 여기서는 블록/폭탄 간 충돌만 처리

```csharp
using RoseEngine;

public class BlockScript : MonoBehaviour
{
    private const float BREAK_SPEED = 8.0f;

    public override void OnCollisionEnter(Collision collision)
    {
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

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PigScript.cs`
- **역할**: pig 큐브에 부착. 일정 속도 이상의 충돌로 사망 처리.
- **클래스**: `PigScript : MonoBehaviour`
- **주요 멤버**:
  - `private const float KILL_SPEED = 2.0f` -- pig 사망 최소 충돌 속도
  - `public override void OnCollisionEnter(Collision collision)` -- 충돌 판정
- **의존**: `RoseEngine`
- **구현 힌트**:
  - cannonball 직접 충돌은 CannonballScript에서 처리
  - 블록에 의한 간접 충돌로도 pig 사망 가능하므로, 모든 충돌에 대해 relativeVelocity 체크
  - `collision.relativeVelocity.magnitude >= KILL_SPEED`이면 `Object.Destroy(gameObject)`

```csharp
using RoseEngine;

public class PigScript : MonoBehaviour
{
    private const float KILL_SPEED = 2.0f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= KILL_SPEED)
        {
            Object.Destroy(gameObject);
        }
    }
}
```

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/BombScript.cs`
- **역할**: 폭탄 큐브에 부착. 충돌 시 또는 Explode() 호출 시 주변 3m 내 모든 오브젝트 삭제.
- **클래스**: `BombScript : MonoBehaviour`
- **주요 멤버**:
  - `private const float EXPLOSION_RADIUS = 3.0f` -- 폭발 범위
  - `private const float TRIGGER_SPEED = 2.0f` -- 폭발 트리거 최소 속도
  - `private bool hasExploded = false` -- 중복 폭발 방지
  - `public override void OnCollisionEnter(Collision collision)` -- 충돌로 폭발 트리거
  - `public void Explode()` -- 외부에서 호출 가능한 폭발 메서드 (CannonballScript에서 사용)
- **의존**: `RoseEngine` (MonoBehaviour, Collision, Physics, Object)
- **구현 힌트**:
  - `Physics.OverlapSphere(transform.position, EXPLOSION_RADIUS)` 반환 타입은 `Collider[]`
  - 각 collider의 `col.gameObject`로 접근하여 Destroy
  - 자기 자신은 제외 (`col.gameObject != gameObject`)
  - 마지막에 `Object.Destroy(gameObject)`로 자기 자신도 삭제

```csharp
using RoseEngine;

public class BombScript : MonoBehaviour
{
    private const float EXPLOSION_RADIUS = 3.0f;
    private const float TRIGGER_SPEED = 2.0f;
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

        var colliders = Physics.OverlapSphere(transform.position, EXPLOSION_RADIUS);
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                Object.Destroy(col.gameObject);
            }
        }

        Object.Destroy(gameObject);
    }
}
```

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/CannonballScript.cs`
- **역할**: cannonball에 부착. 충돌 시 대상 타입(tag)에 따라 판정 처리.
- **클래스**: `CannonballScript : MonoBehaviour`
- **주요 멤버**:
  - `private const float MIN_DESTROY_SPEED = 3.0f` -- 블록 파괴 최소 상대 속도
  - `public override void OnCollisionEnter(Collision collision)` -- 충돌 판정
- **의존**: `RoseEngine`
- **구현 힌트**:
  - `collision.gameObject.tag`로 대상 타입 확인
  - "Pig": 무조건 `Object.Destroy(collision.gameObject)`
  - "Block": `collision.relativeVelocity.magnitude >= MIN_DESTROY_SPEED`이면 Destroy
  - "Bomb": `collision.gameObject.GetComponent<BombScript>()?.Explode()` 호출

```csharp
using RoseEngine;

public class CannonballScript : MonoBehaviour
{
    private const float MIN_DESTROY_SPEED = 3.0f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pig"))
        {
            Object.Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Block"))
        {
            if (collision.relativeVelocity.magnitude >= MIN_DESTROY_SPEED)
            {
                Object.Destroy(collision.gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            var bombScript = collision.gameObject.GetComponent<BombScript>();
            if (bombScript != null)
            {
                bombScript.Explode();
            }
        }
    }
}
```

## 수정할 파일

### `/home/alienspy/git/MyGame/LiveCode/AngryClawd/PileScript.cs`
- **변경 내용**: 빈 클래스에서 큐브 더미 동적 생성 로직으로 전면 교체
- **이유**: pile prefab에 부착되어 Start() 시 런타임으로 큐브 더미를 생성해야 함

**전체 구현**:

```csharp
using RoseEngine;

public class PileScript : MonoBehaviour
{
    // === 빌딩 설정 ===
    private const int MIN_WIDTH = 2;
    private const int MAX_WIDTH = 4;
    private const int MIN_HEIGHT = 3;
    private const int MAX_HEIGHT = 6;
    private const float CUBE_SIZE = 0.8f;
    private const float BOMB_CHANCE = 0.05f;

    // === 머티리얼 경로 ===
    private const string MAT_BLOCK_PATH = "Assets/AngryClawdAssets/mat_block.mat";
    private const string MAT_PIG_PATH = "Assets/AngryClawdAssets/mat_pig.mat";
    private const string MAT_BOMB_PATH = "Assets/AngryClawdAssets/mat_bomb.mat";

    private bool pigPlaced = false;

    public override void Start()
    {
        // 기존 자식 Cube 제거 (prefab에 포함된 placeholder)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

        BuildPile();
    }

    private void BuildPile()
    {
        // 머티리얼 로드
        var matBlock = Resources.Load<Material>(MAT_BLOCK_PATH);
        var matPig = Resources.Load<Material>(MAT_PIG_PATH);
        var matBomb = Resources.Load<Material>(MAT_BOMB_PATH);

        int width = Random.Range(MIN_WIDTH, MAX_WIDTH + 1);
        int height = Random.Range(MIN_HEIGHT, MAX_HEIGHT + 1);

        // pig 위치 미리 결정 (바닥이 아닌 곳)
        int pigX = Random.Range(0, width);
        int pigY = Random.Range(1, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 큐브 생성
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = Vector3.one * CUBE_SIZE;

                // 위치 계산: pile의 위치 기준 오프셋
                float offsetX = (x - width / 2f + 0.5f) * CUBE_SIZE;
                float offsetY = CUBE_SIZE / 2f + y * CUBE_SIZE;
                cube.transform.position = transform.position + new Vector3(offsetX, offsetY, 0f);

                // 부모 설정
                cube.transform.SetParent(transform);

                // Rigidbody 추가
                var rb = cube.AddComponent<Rigidbody>();
                rb.mass = 0.5f;

                // 타입 결정 및 머티리얼/태그/스크립트 설정
                if (x == pigX && y == pigY)
                {
                    // pig 큐브
                    cube.tag = "Pig";
                    cube.AddComponent<PigScript>();
                    if (matPig != null)
                    {
                        var renderer = cube.GetComponent<MeshRenderer>();
                        if (renderer != null) renderer.material = matPig;
                    }
                    pigPlaced = true;
                }
                else if (pigPlaced && Random.value < BOMB_CHANCE)
                {
                    // bomb 큐브 (pig 배치 후에만 생성 가능)
                    cube.tag = "Bomb";
                    cube.AddComponent<BombScript>();
                    if (matBomb != null)
                    {
                        var renderer = cube.GetComponent<MeshRenderer>();
                        if (renderer != null) renderer.material = matBomb;
                    }
                }
                else
                {
                    // 일반 블록
                    cube.tag = "Block";
                    cube.AddComponent<BlockScript>();
                    if (matBlock != null)
                    {
                        var renderer = cube.GetComponent<MeshRenderer>();
                        if (renderer != null) renderer.material = matBlock;
                    }
                }
            }
        }
    }
}
```

**주요 API 시그니처 참조**:
- `GameObject.CreatePrimitive(PrimitiveType.Cube)` -- Cube GO 생성 (MeshFilter + MeshRenderer + BoxCollider 자동 부착)
- `cube.AddComponent<Rigidbody>()` -- 반환 타입 `Rigidbody`
- `cube.AddComponent<T>()` where T : Component, new() -- 제네릭 컴포넌트 추가
- `cube.GetComponent<MeshRenderer>()` -- nullable 반환
- `Resources.Load<Material>(string path)` -- 에셋 경로 기반 로드, nullable 반환
- `Random.Range(int min, int max)` -- min 이상 max 미만 정수 반환
- `Random.value` -- 0.0f ~ 1.0f 실수 반환
- `transform.childCount` -- int, 자식 수
- `transform.GetChild(int index)` -- Transform 반환
- `transform.SetParent(Transform parent)` -- 부모 설정 (worldPositionStays=true 기본)
- `Vector3.one` -- (1,1,1)
- `new Vector3(float x, float y, float z)` -- 생성자

## NuGet 패키지
- 없음 (IronRose.Engine 프로젝트 참조만 사용)

## 빌드 명령
```bash
cd /home/alienspy/git/MyGame && dotnet build LiveCode/LiveCode.csproj
```

## 검증 기준
- [ ] `dotnet build` 성공 (에러 없음)
- [ ] BlockScript.cs, PigScript.cs, BombScript.cs, CannonballScript.cs 4개 새 파일 생성됨
- [ ] PileScript.cs가 BuildPile() 로직을 포함함
- [ ] 모든 스크립트가 `using RoseEngine` 사용
- [ ] 모든 스크립트가 `MonoBehaviour` 상속

## 참고
- **파일 인코딩**: UTF-8 with BOM
- **네이밍**: 클래스/메서드 PascalCase, 필드/변수 camelCase, 상수 UPPER_CASE
- AngryClawdGame.cs는 이 Phase에서 수정하지 않는다 (Phase 03에서 구현).
- SimpleGameBase.cs는 변경하지 않는다 (빈 클래스 유지).
- PileScript의 BuildPile() 내에서 큐브 순회 순서: y가 바깥 루프 (아래에서 위로), x가 안쪽 루프 (왼쪽에서 오른쪽으로). pig 위치는 pigY >= 1이므로 바닥층에는 pig가 배치되지 않는다.
- bomb은 pig가 배치된 이후에만 생성될 수 있다 (`pigPlaced` 플래그). 이는 y=0, x < pigX인 큐브에는 bomb이 나오지 않음을 의미한다. 이는 설계 의도이다.
