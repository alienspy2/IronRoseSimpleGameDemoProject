using System.IO;
using RoseEngine;

public class DemoLauncherScript : MonoBehaviour
{
    private readonly string[] sceneNames = { "AngryClawd" };
    private readonly string sceneFolderRelative = "Scenes/SimpleGameDemo";

    public UIText? descriptionText;
    public TextAsset? readme;

    public override void Start()
    {
        var buttons = FindObjectsOfType<UIButton>();
        for (int i = 0; i < buttons.Length && i < sceneNames.Length; i++)
        {
            var sceneName = sceneNames[i];
            buttons[i].onClick = () => LoadScene(sceneName);
        }

        LoadDescription();
    }

    private void LoadDescription()
    {
        if (descriptionText == null || readme == null) return;
        descriptionText.text = readme.text;
    }

    private void LoadScene(string sceneName)
    {
        var scenePath = Path.Combine(Application.dataPath, sceneFolderRelative, sceneName + ".scene");
        Debug.Log($"[DemoLauncher] Loading scene: {scenePath}");
        SceneManager.LoadScene(scenePath);
    }
}
