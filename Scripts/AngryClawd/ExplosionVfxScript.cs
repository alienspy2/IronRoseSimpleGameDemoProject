// ------------------------------------------------------------
// @file    ExplosionVfxScript.cs
// @brief   폭탄 폭발 시 반투명 스피어 VFX를 표시하는 스크립트.
//          폭발 반경 크기에서 시작하여 scale이 0까지 줄어드는 애니메이션을 수행한 뒤 자기 자신을 파괴한다.
//          프리팹으로 사용되며, BombScript에서 Instantiate하여 사용한다.
// @deps    RoseEngine (MonoBehaviour, Time, Mathf, Object)
// @exports
//   class ExplosionVfxScript : MonoBehaviour
//     Init(float): void  -- 폭발 반경을 설정하고 애니메이션을 시작
// @note    BombScript.Explode()에서 프리팹 인스턴스화하여 호출된다.
//          머티리얼은 프리팹의 MeshRenderer에 object link로 설정되어 있다.
// ------------------------------------------------------------
using RoseEngine;

public class ExplosionVfxScript : MonoBehaviour
{
    private const float SHRINK_DURATION = 0.3f;

    private float diameter;
    private float elapsed;

    /// <summary>
    /// 폭발 반경을 설정하고 애니메이션을 시작한다.
    /// </summary>
    public void Init(float explosionRadius)
    {
        diameter = explosionRadius * 2f;
        elapsed = 0f;
        transform.localScale = new Vector3(diameter, diameter, diameter);
    }

    public override void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / SHRINK_DURATION);
        float scale = Mathf.Lerp(diameter, 0f, t);
        transform.localScale = new Vector3(scale, scale, scale);

        if (t >= 1f)
        {
            RoseEngine.Object.Destroy(gameObject);
        }
    }
}
