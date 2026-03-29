using RoseEngine;

public class HueRotator : MonoBehaviour
{
    private MeshRenderer? meshRenderer;
    private Rigidbody? rb;
    private float hue;
    private float saturation;
    private float value;

    public float speed = 0.2f;
    public float jumpForce = 8f;
    public float torqueStrength = 5f;

    public override void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

        if (meshRenderer?.material != null)
        {
            Color.RGBToHSV(meshRenderer.material.color, out hue, out saturation, out value);
            if (saturation < 0.5f) saturation = 1f;
            if (value < 0.5f) value = 1f;
        }
    }

    public override void Update()
    {
        if (meshRenderer?.material != null)
        {
            hue += Time.deltaTime * speed;
            if (hue > 1f) hue -= 1f;
            meshRenderer.material.color = Color.HSVToRGB(hue, saturation, value);
        }

        if (rb != null && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            var randomTorque = new Vector3(
                RoseEngine.Random.Range(-1f, 1f),
                RoseEngine.Random.Range(-1f, 1f),
                RoseEngine.Random.Range(-1f, 1f)
            ) * torqueStrength;
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }
}
