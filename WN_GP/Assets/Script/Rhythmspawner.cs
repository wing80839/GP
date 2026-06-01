using UnityEngine;
using System.Collections;

/// <summary>
/// 自動在右側生成音符，往左移動到判定線。
/// 掛在場景中的 GameManager 或 BattleCanvas 物件上。
/// </summary>
public class RhythmSpawner : MonoBehaviour
{
    [Header("音符 Prefab（上下各一）")]
    [SerializeField] private GameObject notePrefabJ;  // J 軌道音符
    [SerializeField] private GameObject notePrefabK;  // K 軌道音符

    [Header("戰鬥攝影機（音符位置以此為基準）")]
    [SerializeField] private Camera battleCamera;       // 拖入戰鬥攝影機

    [Header("生成位置（相對攝影機的偏移）")]
    [SerializeField] private float spawnOffsetX = 8f;  // 攝影機右側幾個單位生成
    [SerializeField] private float laneJY = 1f;  // J 軌道 Y 偏移
    [SerializeField] private float laneKY = -1f;  // K 軌道 Y 偏移
    [SerializeField] private float laneZ = 0f;

    [Header("音符設定")]
    [SerializeField] private float noteSpeed = 8f;  // 移動速度（世界單位/秒）
    [SerializeField] private float missOffsetX = -8f;  // 攝影機左側幾個單位算 Miss

    [Header("生成節奏")]
    [SerializeField] private float spawnInterval = 1.4f;  // 每隔幾秒生成一顆

    // 固定的出現順序（可改成讀譜面檔）
    private RhythmNote.Lane[] _pattern = {
        RhythmNote.Lane.J,
        RhythmNote.Lane.K,
        RhythmNote.Lane.J,
        RhythmNote.Lane.J,
        RhythmNote.Lane.K,
        RhythmNote.Lane.K,
        RhythmNote.Lane.J,
        RhythmNote.Lane.K,
    };
    private int _patternIdx = 0;

    private void Start()
    {
        // 若沒有指定戰鬥攝影機，退回使用 Main Camera
        if (battleCamera == null)
            battleCamera = Camera.main;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnNote(_pattern[_patternIdx % _pattern.Length]);
            _patternIdx++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnNote(RhythmNote.Lane lane)
    {
        GameObject prefab = lane == RhythmNote.Lane.J ? notePrefabJ : notePrefabK;
        if (prefab == null) return;

        // 以戰鬥攝影機的世界座標為基準計算生成點
        float camX = battleCamera.transform.position.x;
        float camY = battleCamera.transform.position.y;
        float spawnX = camX + spawnOffsetX;
        float missX = camX + missOffsetX;
        float y = camY + (lane == RhythmNote.Lane.J ? laneJY : laneKY);

        Vector3 pos = new Vector3(spawnX, y, laneZ);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        RhythmNote note = obj.GetComponent<RhythmNote>();
        if (note == null) note = obj.AddComponent<RhythmNote>();

        note.Init(lane, noteSpeed, missX);
    }
}