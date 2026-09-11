using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BedInteraction : MonoBehaviour, Interactable
{
    [Header("主對話框")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("選項按鈕")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button btn1;  // 調查
    [SerializeField] private Button btn2;  // 結束

    [Header("結果對話框")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

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
        if (btn2) btn2.onClick.AddListener(OnClose);

        _buttons = new Button[] { btn1, btn2 };
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
            if (Input.GetKeyDown(KeyCode.F)) CloseAll();
            return;
        }
    }

    public void TriggerAction()
    {
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        dialogueText.text = "剛剛都還坐在上面的床。要做什麼呢？";
        if (dialoguePanel) dialoguePanel.SetActive(true);
        Player.CanMove = false;
    }

    private void ShowOptions()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
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
        resultText.text = "很大的床，藍色的。或許是有些老舊，布料褪色了。掀開棉被，棉被裡面什麼都沒有。";
        if (resultPanel) resultPanel.SetActive(true);
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