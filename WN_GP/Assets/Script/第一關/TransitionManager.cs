using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 全螢幕橢圓淡入淡出轉場管理器。
/// 掛在一個常駐物件上(建議放在啟動場景，並 DontDestroyOnLoad)。
/// 場景名稱符合 targetSceneName 時會自動播放淡入。
/// 也可手動呼叫 PlayFadeIn / PlayFadeOut / FadeOutThenLoadScene。
/// </summary>
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("鋪滿全螢幕、套用 EllipseFadeTransition Shader 的 UI Image")]
    [SerializeField] private Image fadeImage;

    [Header("Trigger Settings")]
    [Tooltip("進入此名稱的場景時，自動播放淡入效果")]
    [SerializeField] private string targetSceneName = "第一關";

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.2f;
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Material fadeMaterial;
    private Coroutine currentRoutine;

    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int CenterID = Shader.PropertyToID("_Center");
    private static readonly int EllipseScaleID = Shader.PropertyToID("_EllipseScale");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 每個 Image 用自己的 material 實例，避免共用到同一份材質造成互相影響
        fadeMaterial = fadeImage.material;
        UpdateAspectRatio();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void UpdateAspectRatio()
    {
        // 讓橢圓的橫向拉伸比例配合目前螢幕解析度，視覺上呈現正圓/橢圓可自行調整
        float aspect = (float)Screen.width / Screen.height;
        fadeMaterial.SetVector(EllipseScaleID, new Vector4(aspect, 1f, 0f, 0f));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetSceneName)
        {
            PlayFadeIn();
        }
    }

    /// <summary>橢圓由中心向外擴散，畫面由暗轉亮(淡入)。</summary>
    public void PlayFadeIn(Vector2? centerViewport = null)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, maxRadius, centerViewport ?? new Vector2(0.5f, 0.5f)));
    }

    /// <summary>橢圓由外向中心收合，畫面由亮轉暗(淡出)。</summary>
    public void PlayFadeOut(Vector2? centerViewport = null)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(maxRadius, 0f, centerViewport ?? new Vector2(0.5f, 0.5f)));
    }

    /// <summary>先淡出蓋黑畫面，再載入指定場景(常用於換關卡)。</summary>
    public IEnumerator FadeOutThenLoadScene(string sceneName, Vector2? centerViewport = null)
    {
        yield return FadeRoutine(maxRadius, 0f, centerViewport ?? new Vector2(0.5f, 0.5f));
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeRoutine(float fromRadius, float toRadius, Vector2 center)
    {
        fadeImage.enabled = true;
        UpdateAspectRatio();
        fadeMaterial.SetVector(CenterID, center);

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float p = easeCurve.Evaluate(Mathf.Clamp01(t / transitionDuration));
            float radius = Mathf.Lerp(fromRadius, toRadius, p);
            fadeMaterial.SetFloat(RadiusID, radius);
            yield return null;
        }

        fadeMaterial.SetFloat(RadiusID, toRadius);

        // 淡入完成(洞已擴大到蓋滿螢幕) -> 可以關閉 Image 省效能
        if (Mathf.Approximately(toRadius, maxRadius))
        {
            fadeImage.enabled = false;
        }
    }
}
