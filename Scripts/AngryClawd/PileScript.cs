// ------------------------------------------------------------
// @file    PileScript.cs
// @brief   pile prefab에 부착. Start() 시 큐브 더미를 동적으로 생성하여 블록/pig/bomb을 배치한다.
// @deps    RoseEngine (MonoBehaviour, GameObject, Object, Resources, Material,
//          MeshRenderer, Rigidbody, Vector3, Random, PrimitiveType, Transform),
//          BlockScript, PigScript, BombScript
// @exports
//   class PileScript : MonoBehaviour
//     Start(): void       -- 기존 자식 제거 후 BuildPile()로 큐브 더미 생성
//     BuildPile(): void   -- 랜덤 크기/배치로 블록/pig/bomb 큐브 격자 생성
// @note    pig는 바닥층(y=0)에는 배치되지 않음. bomb은 pig 배치 이후에만 생성 가능.
//          머티리얼은 Assets/AngryClawdAssets/ 경로에서 Resources.Load로 로드.
// ------------------------------------------------------------
using RoseEngine;

public class PileScript : MonoBehaviour
{
    // === 빌딩 설정 ===
    private const int MIN_WIDTH = 2;
    private const int MAX_WIDTH = 4;
    private const int MIN_HEIGHT = 3;
    private const int MAX_HEIGHT = 6;
    private const float CUBE_SIZE = 0.8f;
    private const float CUBE_SPACING = CUBE_SIZE + 0.02f; // 블록 간 미세 간격 (물리 겹침 방지)
    private const float BOMB_CHANCE = 0.05f;

    // === 머티리얼 GUID ===
    private const string MAT_BLOCK_GUID = "ae2c9f21-fb57-41b6-999e-38c4f9bded72";
    private const string MAT_PIG_GUID = "d80c9906-546b-4d30-a1ae-21ffd2221b0f";
    private const string MAT_BOMB_GUID = "08ba5c79-5dda-442a-9fc4-a223d45176df";

    public GameObject? explosionVfxPrefab;

    private bool pigPlaced = false;

    public override void Start()
    {
        Debug.Log($"[PileScript] Start() called. explosionVfxPrefab null={explosionVfxPrefab == null}");

        // 기존 자식 Cube 제거 (prefab에 포함된 placeholder)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            RoseEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }

        BuildPile();
    }

    private void BuildPile()
    {
        // 머티리얼 로드 (GUID 기반)
        var db = Resources.GetAssetDatabase();
        var matBlock = db?.LoadByGuid<Material>(MAT_BLOCK_GUID);
        var matPig = db?.LoadByGuid<Material>(MAT_PIG_GUID);
        var matBomb = db?.LoadByGuid<Material>(MAT_BOMB_GUID);

        int width = RoseEngine.Random.Range(MIN_WIDTH, MAX_WIDTH + 1);
        int height = RoseEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT + 1);

        // pig 위치 미리 결정 (바닥이 아닌 곳)
        int pigX = RoseEngine.Random.Range(0, width);
        int pigY = RoseEngine.Random.Range(1, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 큐브 생성
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = Vector3.one * CUBE_SIZE;

                // 위치 계산: pile의 위치 기준 오프셋 (CUBE_SPACING으로 간격 확보)
                float offsetX = (x - width / 2f + 0.5f) * CUBE_SPACING;
                float offsetY = CUBE_SIZE / 2f + y * CUBE_SPACING;
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
                else if (pigPlaced && RoseEngine.Random.value < BOMB_CHANCE)
                {
                    // bomb 큐브 (pig 배치 후에만 생성 가능)
                    cube.tag = "Bomb";
                    var bomb = cube.AddComponent<BombScript>();
                    bomb.explosionVfxPrefab = explosionVfxPrefab;
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
