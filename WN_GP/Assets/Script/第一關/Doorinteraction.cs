using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour, Interactable
{
    [Header("主對話框")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("選項按鈕")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button btn1;  // 調查
    [SerializeField] private Button btn2;  // 使用
    [SerializeField] private Button btn3;  // 結束

    [Header("結果對話框")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("物品選擇面板（選項2用）")]
    [SerializeField] private GameObject itemPanel;

    private Canvas _canvas;
    private int _selectedIndex = 0;
    private Button[] _buttons;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>(true);
        if (_canvas) _canvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (btn1) btn1.onClick.AddListener(OnInvestigate);
        if (btn2) btn2.onClick.AddListener(OnUse);
        if (btn3) btn3.onClick.AddListener(OnClose);

        _buttons = new Button[] { btn1, btn2, btn3 };
    }

    private void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F)) { ShowOptions(); return; }
            return;
        }

        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            HandleOptionInput();
            return;
        }

        if (resultPanel != null && resultPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F)) { ShowOptions(); return; }
            return;
        }

        if (itemPanel != null && itemPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F)) { CloseItemPanel(); return; }
            return;
        }
    }

    public void TriggerAction()
    {
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (itemPanel) itemPanel.SetActive(false);
        dialogueText.text = "牢房房門，牢牢鎖上了。";
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
    }

    private void ShowOptions()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
        _selectedIndex = 0;
        UpdateOptionHighlight();
    }

    private void HandleOptionInput()
    {
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_buttons[_selectedIndex] != null)
                _buttons[_selectedIndex].onClick.Invoke();
        }
    }

    private void UpdateOptionHighlight()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null) continue;
            var colors = _buttons[i].colors;
            colors.normalColor = i == _selectedIndex
                ? new Color(1f, 0.88f, 0.2f)
                : Color.white;
            _buttons[i].colors = colors;
        }
    }

    private void OnInvestigate()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        resultText.text = "沒有任何縫隙，中央的窗戶是霧面的，能微微看到外面的人影。\n圓頭鎖不論如何轉動都打不開。";
        if (resultPanel) resultPanel.SetActive(true);
    }

    private void OnUse()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        // 若有物品面板就顯示，否則顯示文字
        if (itemPanel != null)
        {
            itemPanel.SetActive(true);
        }
        else
        {
            resultText.text = "請選擇使用的物品。";
            if (resultPanel) resultPanel.SetActive(true);
        }
    }

    private void CloseItemPanel()
    {
        if (itemPanel) itemPanel.SetActive(false);
        ShowOptions();
    }

    private void OnClose()
    {
        CloseAll();
    }

    private void CloseAll()
    {
        if (_canvas) _canvas.gameObject.SetActive(false);
        Player.CanMove = true;
    }
}
