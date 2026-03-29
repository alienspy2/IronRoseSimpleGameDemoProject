// ------------------------------------------------------------
// @file    CannonballScript.cs
// @brief   cannonball에 부착되는 스크립트. 충돌 시 대상 타입(tag)에 따라 파괴/폭발 판정을 처리한다.
// @deps    RoseEngine (MonoBehaviour, Collision, Object), BombScript
// @exports
//   class CannonballScript : MonoBehaviour
//     OnCollisionEnter(Collision): void  -- Pig 무조건 파괴, Block 속도 조건부 파괴, Bomb Explode() 호출
// @note    Pig는 무조건 파괴, Block은 MIN_DESTROY_SPEED 이상일 때만, Bomb은 Explode() 위임.
//          Block 파괴 시 BlockScript.SpawnDebris()로 부스러기 VFX를 생성한다.
// ------------------------------------------------------------
using RoseEngine;

public class CannonballScript : MonoBehaviour
{
    private const float MIN_DESTROY_SPEED = 3.0f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pig"))
        {
            RoseEngine.Object.Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Block"))
        {
            if (collision.relativeVelocity.magnitude >= MIN_DESTROY_SPEED)
            {
                var blockScript = collision.gameObject.GetComponent<BlockScript>();
                if (blockScript != null)
                {
                    blockScript.SpawnDebris();
                }
                RoseEngine.Object.Destroy(collision.gameObject);
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
