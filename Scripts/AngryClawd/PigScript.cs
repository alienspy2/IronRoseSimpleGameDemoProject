// ------------------------------------------------------------
// @file    PigScript.cs
// @brief   pig 큐브에 부착되는 스크립트. 일정 속도 이상의 충돌로 사망(파괴) 처리한다.
// @deps    RoseEngine (MonoBehaviour, Collision, Object)
// @exports
//   class PigScript : MonoBehaviour
//     OnCollisionEnter(Collision): void  -- KILL_SPEED 이상 충돌 시 자신 파괴
// @note    cannonball 직접 충돌은 CannonballScript에서 처리.
//          블록에 의한 간접 충돌로도 사망 가능하므로 모든 충돌에 대해 relativeVelocity 체크.
// ------------------------------------------------------------
using RoseEngine;

public class PigScript : MonoBehaviour
{
    private const float KILL_SPEED = 2.0f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= KILL_SPEED)
        {
            SpawnDebris();
            RoseEngine.Object.Destroy(gameObject);
        }
    }

    private void SpawnDebris()
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
