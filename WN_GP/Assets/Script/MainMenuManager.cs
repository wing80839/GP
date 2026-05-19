using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button btnNew;

    private void Start()
    {
        btnNew.onClick.AddListener(() => LoadLevel());
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene("第一關");
    }
}
