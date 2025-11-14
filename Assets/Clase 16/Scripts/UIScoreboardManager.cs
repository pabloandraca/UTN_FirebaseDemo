using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreboardManager : MonoBehaviour
{
    [Header("UI Scoreboard")]
    public GameObject scoreboardRoot;
    public Transform scoresParent;
    public GameObject scoreRowPrefab;

    private readonly List<GameObject> spawnedRows = new();

    void Start()
    {
        if (scoreboardRoot != null)
            scoreboardRoot.SetActive(false);
    }

    public void ShowScoreboard(float distance, float maxSpeed)
    {
        FirebaseManager.I.UploadScore(distance, maxSpeed);

        FirebaseManager.I.GetScoreList(OnScoresReceived);
    }

    void OnScoresReceived(List<ScoreEntry> list)
    {
        if (list == null)
        {
            Debug.LogWarning("[UI Score] Lista nula");
            return;
        }

        if (scoreboardRoot != null)
            scoreboardRoot.SetActive(true);

        foreach (var go in spawnedRows)
            Destroy(go);
        spawnedRows.Clear();

        list.Sort((a, b) => b.bestDistance.CompareTo(a.bestDistance));

        string currentUid = FirebaseManager.I.CurrentUser != null
            ? FirebaseManager.I.CurrentUser.UserId
            : null;

        foreach (var entry in list)
        {
            var row = Instantiate(scoreRowPrefab, scoresParent);
            spawnedRows.Add(row);

            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length < 3)
            {
                Debug.LogWarning("ScoreRowPrefab: se esperan al menos 3 Texts hijos.");
                continue;
            }

            texts[0].text = string.IsNullOrEmpty(entry.email) ? entry.userId : entry.email;
            texts[1].text = entry.bestDistance.ToString("0.00");
            texts[2].text = entry.bestMaxSpeed.ToString("0.00");

            if (entry.userId == currentUid)
            {
                texts[0].fontStyle = (FontStyles)FontStyle.Bold;
                texts[1].fontStyle = (FontStyles)FontStyle.Bold;
                texts[2].fontStyle = (FontStyles)FontStyle.Bold;
            }
        }
    }

    public void HideScoreboard()
    {
        if (scoreboardRoot != null)
            scoreboardRoot.SetActive(false);
    }
}