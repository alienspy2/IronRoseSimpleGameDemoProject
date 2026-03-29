using RoseEngine;

public class PlayerPrefsTest : MonoBehaviour
{
    private bool _tested = false;

    public override void Start()
    {
        Debug.Log("[PlayerPrefsTest] Start — PlayerPrefs 테스트 시작");

        // 기존 데이터 클리어
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Int 테스트
        PlayerPrefs.SetInt("score", 100);
        int score = PlayerPrefs.GetInt("score", 0);
        Debug.Log($"[PlayerPrefsTest] SetInt/GetInt: score = {score} (expected 100)");

        // Float 테스트
        PlayerPrefs.SetFloat("volume", 0.75f);
        float volume = PlayerPrefs.GetFloat("volume", 0f);
        Debug.Log($"[PlayerPrefsTest] SetFloat/GetFloat: volume = {volume} (expected 0.75)");

        // String 테스트
        PlayerPrefs.SetString("playerName", "TestPlayer");
        string playerName = PlayerPrefs.GetString("playerName", "");
        Debug.Log($"[PlayerPrefsTest] SetString/GetString: playerName = {playerName} (expected TestPlayer)");

        // HasKey 테스트
        Debug.Log($"[PlayerPrefsTest] HasKey(\"score\") = {PlayerPrefs.HasKey("score")} (expected True)");
        Debug.Log($"[PlayerPrefsTest] HasKey(\"missing\") = {PlayerPrefs.HasKey("missing")} (expected False)");

        // 기본값 테스트
        int missing = PlayerPrefs.GetInt("missing", -1);
        Debug.Log($"[PlayerPrefsTest] GetInt missing key = {missing} (expected -1)");

        // DeleteKey 테스트
        PlayerPrefs.DeleteKey("score");
        Debug.Log($"[PlayerPrefsTest] After DeleteKey: HasKey(\"score\") = {PlayerPrefs.HasKey("score")} (expected False)");

        // Save
        PlayerPrefs.Save();
        Debug.Log("[PlayerPrefsTest] Save() 완료");

        // Application 경로 확인
        Debug.Log($"[PlayerPrefsTest] Application.companyName = {Application.companyName}");
        Debug.Log($"[PlayerPrefsTest] Application.productName = {Application.productName}");
        Debug.Log($"[PlayerPrefsTest] Application.persistentDataPath = {Application.persistentDataPath}");
        Debug.Log($"[PlayerPrefsTest] Application.dataPath = {Application.dataPath}");

        Debug.Log("[PlayerPrefsTest] 테스트 완료!");
    }

    public override void Update()
    {
        // 1프레임 후 저장된 값 재확인
        if (!_tested)
        {
            _tested = true;
            string name = PlayerPrefs.GetString("playerName", "");
            float vol = PlayerPrefs.GetFloat("volume", 0f);
            Debug.Log($"[PlayerPrefsTest] Update 재확인 — playerName={name}, volume={vol}");
        }
    }
}
