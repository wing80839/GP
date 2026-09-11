using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [Header("UI組件")]
    public TextMeshProUGUI textlabel;
    public Image faceImage;

    [Header("文本文件")]
    public TextAsset textFile;
    public int index;

    List<string> textList = new List<string>();

    void OnEnable()
    {
        GetTextFromFile(textFile);
        index = 0;
        textlabel.text = textList[index];
        index++;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && index == textList.Count)
        {
            gameObject.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            textlabel.text = textList[index];
            index++;
        }
    }

    void GetTextFromFile(TextAsset file)
    {
        textList.Clear();
        index = 0;

        var lineData = file.text.Split('\n');

        foreach (var line in lineData)
        {
            textList.Add(line);
        }
    }
}