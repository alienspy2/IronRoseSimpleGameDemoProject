// ------------------------------------------------------------
// @file    DebrisVfxScript.cs
// @brief   블록 파괴 시 작은 큐브 부스러기를 흩뿌리는 VFX 스크립트.
//          물리(Rigidbody) 적용 없이 방향과 속도로 이동하며,
//          scale 축소 애니메이션으로 사라진 뒤 자동 제거된다.
// @deps    RoseEngine (MonoBehaviour, GameObject, MeshFilter, MeshRenderer,
//          Material, PrimitiveGenerator, Color, Time, Mathf, Vector3, Random, Object)
// @exports
//   class DebrisVfxScript : MonoBehaviour
//     SpawnAt(Vector3, Color): void  -- 지정 위치에 부스러기 큐브들을 생성 (static)
// @note    블록 파괴 시 호출된다. 폭탄블럭에는 적용하지 않는다.
// ------------------------------------------------------------
using RoseEngine;

public class DebrisVfxScript : MonoBehaviour
{
    private const int DEBRIS_COUNT = 6;
    private const float DEBRIS_SIZE = 0.80f;
    private const float SPREAD_SPEED_MIN = 1.0f;
    private const float SPREAD_SPEED_MAX = 3.0f;
    private const float SHRINK_DURATION = 0.5f;

    private Vector3 velocity;
    private float elapsed;

    /// <summary>
    /// 지정 위치에 부스러기 큐브들을 생성한다.
    /// </summary>
    /// <param name="position">블록 파괴 위치 (월드 좌표)</param>
    /// <param name="color">부스러기 색상 (원본 블록의 머티리얼 색상)</param>
    public static void SpawnAt(Vector3 position, Color color)
    {
        for (int i = 0; i < DEBRIS_COUNT; i++)
        {
            var go = new GameObject("DebrisVFX");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * DEBRIS_SIZE;

            // Mesh 설정 (collider 없이 시각적 큐브만 생성)
            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = PrimitiveGenerator.CreateCube();

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.material = new Material(color);

            // VFX 스크립트 부착 + 랜덤 방향/속도 설정
            var vfx = go.AddComponent<DebrisVfxScript>();
            float speed = RoseEngine.Random.Range(SPREAD_SPEED_MIN, SPREAD_SPEED_MAX);
            vfx.velocity = RoseEngine.Random.insideUnitSphere.normalized * speed;
            vfx.elapsed = 0f;
        }
    }

    public override void Update()
    {
        elapsed += Time.deltaTime;

        // 위치 이동 (물리 없이 단순 이동)
        transform.position += velocity * Time.deltaTime;

        // Scale 축소 애니메이션
        float t = Mathf.Clamp01(elapsed / SHRINK_DURATION);
        float scale = Mathf.Lerp(DEBRIS_SIZE, 0f, t);
        transform.localScale = new Vector3(scale, scale, scale);

        if (t >= 1f)
        {
            RoseEngine.Object.Destroy(gameObject);
        }
    }
}
