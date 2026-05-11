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
    public TextAsset secondTextFile;   // 第二次用
    public TextAsset thirdTextFile;    // 第三次用（觸發其他文本）
    public int index;

    [Header("開啟次數")]
    public int openCount = 0;          // 記錄開啟次數

    List<string> textList = new List<string>();


    // 每次被 SetActive(true) 時自動呼叫
    void OnEnable()
    {
        openCount++;  // 開啟次數+1

        // 根據次數載入不同文本
        if (openCount == 1)
            GetTextFromFile(textFile);
        else if (openCount == 2)
            GetTextFromFile(secondTextFile);
        else if (openCount >= 3)
            GetTextFromFile(thirdTextFile);  // 第三次起觸發其他文本

        index = 0;
        textlabel.text = textList[index];
        index++;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && index == textList.Count)
        {
            gameObject.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
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