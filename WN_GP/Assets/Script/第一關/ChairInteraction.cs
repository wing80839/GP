using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChairInteraction : MonoBehaviour, Interactable
{
    [Header("主對話框")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("選項按鈕")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button btn1;  // 調查
    [SerializeField] private Button btn2;  // 帶走
    [SerializeField] private Button btn3;  // 結束

    [Header("結果對話框")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("椅子跟隨設定")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 followOffset = new Vector3(0.5f, 0f, 0f);

    private bool isFollowing = false;
    private Canvas _canvas;

    // WS 選項控制
    private int _selectedIndex = 0;  // 0=調查 1=帶走 2=結束
    private Button[] _buttons;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>(true);
        Debug.Log("找到 Canvas: " + (_canvas != null ? _canvas.name : "null"));
        if (_canvas) _canvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (btn1) btn1.onClick.AddListener(OnInvestigate);
        if (btn2) btn2.onClick.AddListener(OnTakeAway);
        if (btn3) btn3.onClick.AddListener(OnClose);

        _buttons = new Button[] { btn1, btn2, btn3 };
    }

    private void Update()
    {
        if (isFollowing && playerTransform != null)
            transform.position = playerTransform.position + followOffset;

        // 對話框顯示時按 F 進入選項
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ShowOptions();
                return;
            }
            return;
        }

        // 選項面板顯示時用 WS 控制、F 確認
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            HandleOptionInput();
            return;
        }

        // 結果面板顯示時按 F 關閉
        if (resultPanel != null && resultPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F))
                CloseAll();
            return;
        }
    }

    private void HandleOptionInput()
    {
        // W 往上、S 往下
        if (Input.GetKeyDown(KeyCode.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _buttons.Length) % _buttons.Length;
            UpdateOptionHighlight();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _buttons.Length;
            UpdateOptionHighlight();
        }

        // F 確認選項
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_buttons[_selectedIndex] != null)
                _buttons[_selectedIndex].onClick.Invoke();
        }
    }

    // 高亮顯示當前選中的按鈕
    private void UpdateOptionHighlight()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null) continue;
            var colors = _buttons[i].colors;
            colors.normalColor = i == _selectedIndex
                ? new Color(1f, 0.88f, 0.2f)   // 黃色 = 選中
                : Color.white;
            _buttons[i].colors = colors;
        }
    }

    public void TriggerAction()
    {
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

        Debug.Log("dialogueText name: " + dialogueText.name);  // ← 改成這行

        dialogueText.text = "剛剛都還坐在上面的床。要做什麼呢？";
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
    }

    private void ShowOptions()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);

        // 預設選第一個選項
        _selectedIndex = 0;
        UpdateOptionHighlight();
    }

    private void OnInvestigate()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        resultText.text = "就是很普通的小椅子，但感覺光是看到就能安心不少。";
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void OnTakeAway()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        isFollowing = true;
        resultText.text = "把椅子帶走了。";
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void OnClose()
    {
        CloseAll();
    }

    private void CloseAll()
    {
        if (_canvas) _canvas.gameObject.SetActive(false);

        // 恢復玩家移動
        Player.CanMove = true;
    }
}