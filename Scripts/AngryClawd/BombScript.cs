// ------------------------------------------------------------
// @file    BombScript.cs
// @brief   폭탄 큐브에 부착되는 스크립트. 충돌 시 또는 외부 Explode() 호출 시 주변 반경 내 Block을 파괴하고 다른 Bomb을 연쇄 폭발시킨다.
// @deps    RoseEngine (MonoBehaviour, Collision, Physics, Object, Collider)
// @exports
//   class BombScript : MonoBehaviour
//     OnCollisionEnter(Collision): void  -- TRIGGER_SPEED 이상 충돌 시 Explode() 호출
//     Explode(): void                    -- 반경 EXPLOSION_RADIUS 내 Block 파괴 + 다른 Bomb 연쇄 폭발 (외부 호출 가능)
// @note    hasExploded 플래그로 중복 폭발 방지. CannonballScript에서 Explode()를 직접 호출할 수 있다.
// ------------------------------------------------------------
using RoseEngine;

public class BombScript : MonoBehaviour
{
    private const float EXPLOSION_RADIUS = 3.0f;
    private const float TRIGGER_SPEED = 2.0f;

    /// <summary>폭발 VFX 프리팹 (PileScript에서 주입).</summary>
    public GameObject? explosionVfxPrefab;

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
            if (col.gameObject == gameObject) continue;

            if (col.gameObject.CompareTag("Block"))
            {
                var blockScript = col.gameObject.GetComponent<BlockScript>();
                if (blockScript != null)
                {
                    blockScript.SpawnDebris();
                }
                RoseEngine.Object.Destroy(col.gameObject);
            }
            else if (col.gameObject.CompareTag("Bomb"))
            {
                var otherBomb = col.gameObject.GetComponent<BombScript>();
                if (otherBomb != null)
                {
                    otherBomb.Explode();
                }
            }
        }

        ExplosionVfxScript.SpawnAt(transform.position, EXPLOSION_RADIUS);

        RoseEngine.Object.Destroy(gameObject);
    }
}
