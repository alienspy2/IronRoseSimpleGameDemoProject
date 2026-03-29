// ------------------------------------------------------------
// @file    BlockScript.cs
// @brief   일반 블록 큐브에 부착되는 스크립트. 블록/폭탄 간 고속 충돌 시 자기 자신을 파괴한다.
// @deps    RoseEngine (MonoBehaviour, Collision, Object)
// @exports
//   class BlockScript : MonoBehaviour
//     OnCollisionEnter(Collision): void  -- 블록/폭탄과 BREAK_SPEED 이상 충돌 시 자신 파괴
// @note    cannonball과의 충돌은 CannonballScript에서 처리하므로 여기서는 "Block"/"Bomb" 태그만 판정.
//          파괴 시 DebrisVfxScript로 부스러기 VFX를 생성한다.
// ------------------------------------------------------------
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
                SpawnDebris();
                RoseEngine.Object.Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 블록 위치에 부스러기 VFX를 생성한다.
    /// MeshRenderer에서 머티리얼 색상을 읽어 부스러기 색상으로 사용한다.
    /// </summary>
    public void SpawnDebris()
    {
        var color = Color.white;
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            color = renderer.material.color;
        }
        DebrisVfxScript.SpawnAt(transform.position, color);
    }
}
