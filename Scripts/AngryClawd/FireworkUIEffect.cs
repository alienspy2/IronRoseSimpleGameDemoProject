// ------------------------------------------------------------
// @file    FireworkUIEffect.cs
// @brief   UI 기반 불꽃놀이 이펙트. Stage Clear 시 Canvas 위에
//          파티클 UIImage를 동적 생성하여 불꽃놀이를 연출한다.
// ------------------------------------------------------------
using RoseEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Random = RoseEngine.Random;
using Object = RoseEngine.Object;

public class FireworkUIEffect : MonoBehaviour
{
    public Sprite? circleSprite;
    public Sprite? starSprite;
    public Sprite? sparkSprite;

    private readonly List<GameObject> activeParticles = new();

    public override void Start()
    {
        // 스프라이트가 인스펙터에서 연결되지 않은 경우 Resources.Load로 로드
        string basePath = Path.Combine(Application.dataPath, "AngryClawdAssets", "UI");
        if (circleSprite == null)
            circleSprite = Resources.Load<Sprite>(Path.Combine(basePath, "firework_particle_circle.png"));
        if (starSprite == null)
            starSprite = Resources.Load<Sprite>(Path.Combine(basePath, "firework_particle_star.png"));
        if (sparkSprite == null)
            sparkSprite = Resources.Load<Sprite>(Path.Combine(basePath, "firework_spark.png"));
    }

    // 불꽃놀이 색상 팔레트
    private static readonly Color[] FireworkColors = new[]
    {
        new Color(1f, 0.3f, 0.3f, 1f),   // 빨강
        new Color(1f, 0.8f, 0.2f, 1f),   // 노랑
        new Color(0.3f, 1f, 0.5f, 1f),   // 초록
        new Color(0.3f, 0.6f, 1f, 1f),   // 파랑
        new Color(1f, 0.5f, 0.9f, 1f),   // 분홍
        new Color(0.9f, 0.5f, 1f, 1f),   // 보라
        new Color(1f, 0.6f, 0.2f, 1f),   // 주황
        new Color(0.5f, 1f, 1f, 1f),     // 시안
    };

    private const int PARTICLES_PER_BURST = 16;
    private const float PARTICLE_SIZE = 24f;
    private const float SPARK_WIDTH = 12f;
    private const float SPARK_HEIGHT = 40f;
    private const float SPREAD_RADIUS = 300f;
    private const float PARTICLE_LIFETIME = 1.0f;
    private const float BURST_COUNT = 5;
    private const float BURST_INTERVAL = 0.4f;

    /// <summary>
    /// 불꽃놀이 이펙트를 시작한다. 여러 발의 불꽃을 시간차로 발사한다.
    /// </summary>
    public void Play()
    {
        StartCoroutine(FireworkSequence());
    }

    /// <summary>
    /// 진행 중인 모든 이펙트를 즉시 정리한다.
    /// </summary>
    public void Stop()
    {
        StopAllCoroutines();
        foreach (var p in activeParticles)
        {
            if (p != null) Object.Destroy(p);
        }
        activeParticles.Clear();
    }

    private IEnumerator FireworkSequence()
    {
        for (int i = 0; i < BURST_COUNT; i++)
        {
            // 화면 내 랜덤 위치에서 폭발
            float x = Random.Range(-300f, 300f);
            float y = Random.Range(-100f, 150f);
            var center = new Vector2(x, y);

            // 랜덤 색상 선택 (1~2색 조합)
            var mainColor = FireworkColors[Random.Range(0, FireworkColors.Length)];
            var subColor = FireworkColors[Random.Range(0, FireworkColors.Length)];

            SpawnBurst(center, mainColor, subColor);

            yield return new WaitForSeconds(BURST_INTERVAL);
        }
    }

    private void SpawnBurst(Vector2 center, Color mainColor, Color subColor)
    {
        for (int i = 0; i < PARTICLES_PER_BURST; i++)
        {
            // 방사형 방향 계산
            float angle = (float)i / PARTICLES_PER_BURST * Mathf.PI * 2f;
            angle += Random.Range(-0.2f, 0.2f); // 약간의 랜덤 오프셋

            float speed = Random.Range(0.6f, 1.0f);
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            // 파티클 종류 결정: 70% 원형, 20% 별, 10% 스파크
            float roll = Random.Range(0f, 1f);
            Sprite? sprite;
            Vector2 size;

            if (roll < 0.7f)
            {
                sprite = circleSprite;
                float s = Random.Range(PARTICLE_SIZE * 0.6f, PARTICLE_SIZE * 1.4f);
                size = new Vector2(s, s);
            }
            else if (roll < 0.9f)
            {
                sprite = starSprite;
                float s = Random.Range(PARTICLE_SIZE * 0.8f, PARTICLE_SIZE * 1.6f);
                size = new Vector2(s, s);
            }
            else
            {
                sprite = sparkSprite;
                size = new Vector2(SPARK_WIDTH, SPARK_HEIGHT);
            }

            if (sprite == null) sprite = circleSprite;
            if (sprite == null) continue;

            var color = (i % 3 == 0) ? subColor : mainColor;

            StartCoroutine(AnimateParticle(center, direction, sprite, size, color));
        }
    }

    private IEnumerator AnimateParticle(Vector2 center, Vector2 direction, Sprite sprite, Vector2 size, Color color)
    {
        // 파티클 GO 생성
        var go = new GameObject("FireworkParticle");
        go.transform.SetParent(gameObject.transform);
        activeParticles.Add(go);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = center;

        var img = go.AddComponent<UIImage>();
        img.sprite = sprite;
        img.color = color;

        float elapsed = 0f;
        float lifetime = PARTICLE_LIFETIME + Random.Range(-0.2f, 0.2f);

        while (elapsed < lifetime)
        {
            if (go == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            // 위치: 방사형으로 퍼져나감 + 중력으로 약간 아래로
            float gravity = 80f * t * t;
            float posX = center.x + direction.x * SPREAD_RADIUS * t;
            float posY = center.y + direction.y * SPREAD_RADIUS * t - gravity;
            rt.anchoredPosition = new Vector2(posX, posY);

            // 스케일: 초반에 커지다가 후반에 줄어듦
            float scale;
            if (t < 0.1f)
                scale = Mathf.Lerp(0f, 1f, t / 0.1f);  // 팝인
            else
                scale = Mathf.Lerp(1f, 0f, (t - 0.1f) / 0.9f);  // 서서히 소멸
            go.transform.localScale = new Vector3(scale, scale, 1f);

            // 알파: 후반부에 페이드아웃
            float alpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            img.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }

        // 정리
        if (go != null)
        {
            activeParticles.Remove(go);
            Object.Destroy(go);
        }
    }
}
