// ------------------------------------------------------------
// @file    BlockScript.cs
// @brief   일반 블록 큐브에 부착되는 스크립트. 일정 속도 이상의 충돌로 파괴 처리한다.
// @deps    RoseEngine (MonoBehaviour, Collision, Object)
// @exports
//   class BlockScript : MonoBehaviour
//     OnCollisionEnter(Collision): void  -- BREAK_SPEED 이상 충돌 시 자신 파괴
// @note    모든 충돌에 대해 relativeVelocity 체크. 파괴 시 DebrisVfxScript로 부스러기 VFX를 생성한다.
// ------------------------------------------------------------
using RoseEngine;

public class BlockScript : MonoBehaviour
{
    private const float BREAK_SPEED = 2.0f;
    private const float FALL_OFF_Y = -10f;

    public override void Update()
    {
        if (transform.position.y < FALL_OFF_Y)
        {
            SpawnDebris();
            RoseEngine.Object.Destroy(gameObject);
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= BREAK_SPEED)
        {
            SpawnDebris();
            RoseEngine.Object.Destroy(gameObject);
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
