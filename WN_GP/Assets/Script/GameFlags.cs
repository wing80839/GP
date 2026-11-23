using System.Collections.Generic;
using UnityEngine;

// 整個遊戲共用的「記號」，用來記住劇情進度（例如「見過病友B」「阿狸的眼鏡不見了」）。
// 對話文本裡用 [SET:記號] 記下、[IF:記號 -> 標籤] 判斷。換場景也會記得；重新開始遊戲會清空。
public static class GameFlags
{
    private static readonly HashSet<string> _flags = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAll() => _flags.Clear();

    public static bool Has(string flag) => !string.IsNullOrEmpty(flag) && _flags.Contains(flag);

    public static void Set(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        if (_flags.Add(flag)) Debug.Log($"[記號] 記下「{flag}」");
    }

    public static void Clear(string flag)
    {
        if (_flags.Remove(flag)) Debug.Log($"[記號] 清除「{flag}」");
    }
}