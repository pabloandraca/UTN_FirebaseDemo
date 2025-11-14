using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public bool FirebaseReady { get; private set; }
    public FirebaseFirestore DB { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            DB = FirebaseFirestore.DefaultInstance;
            FirebaseReady = true;
            Debug.Log("Firebase Lista!");
        }
        else
        {
            Debug.LogError("Firebase no disponible: " + status);
            FirebaseReady = false;
        }
    }

    public void AddOrUpdatePlayerInfo(PlayerInfo playerInfo)
    {
        if (!FirebaseReady)
        {
            Debug.LogWarning("Firebase no está lista!");
            return;
        }

        DocumentReference doc = DB.Collection("playerData").Document(playerInfo.playerName);
        Dictionary<string, object> data = new()
        {
            ["playerName"] = playerInfo.playerName,
            ["playerHP"] = playerInfo.playerHP,
            ["playerMana"] = playerInfo.playerMana,
            ["playerSpeed"] = playerInfo.playerSpeed
        };

        doc.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) Debug.LogError(task.Exception);
            else Debug.Log($"Player {playerInfo.playerName} guardado!");
        });
    }

    public void GetPlayerInfo(string playerName, Action<PlayerInfo?> onDone)
    {
        if (!FirebaseReady)
        {
            Debug.LogWarning("Firebase no está lista!");
            onDone?.Invoke(null); return;
        }

        DocumentReference doc = DB.Collection("playerData").Document(playerName);
        doc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                onDone?.Invoke(null); return;
            }

            var snap = task.Result;

            if (!snap.Exists)
            {
                Debug.LogWarning($"Jugador {playerName} no encontrado");
                onDone?.Invoke(null); return;
            }

            var player = new PlayerInfo(
                snap.GetValue<string>("playerName"),
                snap.GetValue<int>("playerHP"),
                snap.GetValue<int>("playerMana"),
                snap.GetValue<float>("playerSpeed")
            );
            onDone?.Invoke(player);
        });
    }
}