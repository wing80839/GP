using UnityEngine;
using System.Collections;

public class RhythmSpawner : MonoBehaviour
{
    [Header("音符 Prefab（上下各一）")]
    [SerializeField] private GameObject notePrefabJ;
    [SerializeField] private GameObject notePrefabK;

    [Header("戰鬥攝影機（音符位置以此為基準）")]
    [SerializeField] private Camera battleCamera;

    [Header("生成位置（相對攝影機的偏移）")]
    [SerializeField] private float spawnOffsetX = 8f;
    [SerializeField] private float laneJY = 1f;
    [SerializeField] private float laneKY = -1f;
    [SerializeField] private float laneZ = 0f;

    [Header("音符設定")]
    [SerializeField] private float noteSpeed = 8f;
    [SerializeField] private float missOffsetX = -8f;

    [Header("生成節奏")]
    [SerializeField] private float spawnInterval = 1.4f;

    private RhythmNote.Lane[] _pattern = {
        RhythmNote.Lane.J, RhythmNote.Lane.K,
        RhythmNote.Lane.J, RhythmNote.Lane.J,
        RhythmNote.Lane.K, RhythmNote.Lane.K,
        RhythmNote.Lane.J, RhythmNote.Lane.K,
    };

    private int _patternIdx = 0;
    private Coroutine _spawnCoroutine = null;

    private void Start()
    {
        if (battleCamera == null)
            battleCamera = Camera.main;

        // 不自動開始，等 BattleController 呼叫 StartSpawning()
    }

    /// <summary>進入戰鬥時由 BattleController 呼叫</summary>
    public void StartSpawning()
    {
        _patternIdx = 0;
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    /// <summary>離開戰鬥時由 BattleController 呼叫，停止並清除音符</summary>
    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        // 清除所有還在場景上的音符
        foreach (var note in FindObjectsByType<RhythmNote>(FindObjectsSortMode.None))
            Destroy(note.gameObject);
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