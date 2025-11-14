using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Net.WebSockets;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager I { get; private set; }

    public bool FirebaseReady { get; private set; }
    public FirebaseFirestore DB { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser CurrentUser { get; private set; }

    public event Action<FirebaseUser> OnLogin;
    public event Action OnLogout;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    async void Start()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            DB = FirebaseFirestore.DefaultInstance;

            Auth = FirebaseAuth.DefaultInstance;
            Auth.StateChanged += HandleAuthStateChanged;
            HandleAuthStateChanged(this, null);

            FirebaseReady = true;
            Debug.Log("[Firebase] Ready");
        }
        else
        {
            FirebaseReady = false;
            Debug.LogError("[Firebase] Dependencias no disponibles: " + status);
        }
    }

    void OnDestroy()
    {
        if (Auth != null)
            Auth.StateChanged -= HandleAuthStateChanged;
    }

    void HandleAuthStateChanged(object sender, EventArgs e)
    {
        var newUser = Auth.CurrentUser;
        if (newUser != CurrentUser)
        {
            bool wasLoggedIn = CurrentUser != null;
            CurrentUser = newUser;

            if (CurrentUser != null)
            {
                Debug.Log("[Auth] Login: " + CurrentUser.Email);
                OnLogin?.Invoke(CurrentUser);
            }
            else if (wasLoggedIn)
            {
                Debug.Log("[Auth] Logout");
                OnLogout?.Invoke();
            }
        }
    }

    public void RegisterNewUser(string email, string password)
    {
        if (Auth == null)
        {
            Debug.LogError("[Auth] No inicializado");
            return;
        }

        Auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsCanceled)
                {
                    Debug.LogWarning("[Auth] Registro cancelado");
                    return;
                }
                if (t.IsFaulted)
                {
                    Debug.LogError("[Auth] Error en registro: " + t.Exception);
                    return;
                }

                FirebaseUser newUser = t.Result.User;
                Debug.Log("[Auth] Usuario creado: " + newUser.UserId);
            });
    }

    public void LoginUser(string email, string password)
    {
        if (Auth == null)
        {
            Debug.LogError("[Auth] No inicializado");
            return;
        }

        Auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsCanceled)
                {
                    Debug.LogWarning("[Auth] Login cancelado");
                    return;
                }
                if (t.IsFaulted)
                {
                    Debug.LogError("[Auth] Error en login: " + t.Exception);
                    return;
                }

                FirebaseUser user = t.Result.User;
                Debug.Log("[Auth] Login OK: " + user.Email);
            });
    }

    public void LogoutUser()
    {
        if (Auth == null) return;
        Auth.SignOut();
    }

    public void UploadScore(float distance, float maxSpeed)
    {
        if (!FirebaseReady)
        {
            Debug.LogWarning("[Score] Firebase no listo");
            return;
        }
        if (CurrentUser == null)
        {
            Debug.LogWarning("[Score] No hay usuario logeado");
            return;
        }

        string uid = CurrentUser.UserId;
        string email = CurrentUser.Email ?? "";

        DocumentReference doc = DB.Collection("scoreboard").Document(uid);

        var data = new Dictionary<string, object>
        {
            ["userId"] = uid,
            ["email"] = email,
            ["bestDistance"] = distance,
            ["bestMaxSpeed"] = maxSpeed
        };

        doc.SetAsync(data, SetOptions.MergeAll)
           .ContinueWithOnMainThread(t =>
           {
               if (t.IsFaulted)
               {
                   Debug.LogError("[Score] Error al subir score: " + t.Exception);
               }
               else
               {
                   Debug.Log("[Score] Score subido para " + email);
               }
           });
    }

    public void GetScoreList(Action<List<ScoreEntry>> onDone)
    {
        if (!FirebaseReady)
        {
            Debug.LogWarning("[Score] Firebase no listo");
            onDone?.Invoke(null);
            return;
        }

        DB.Collection("scoreboard").GetSnapshotAsync()
          .ContinueWithOnMainThread(t =>
          {
              if (t.IsFaulted)
              {
                  Debug.LogError("[Score] Error al obtener lista: " + t.Exception);
                  onDone?.Invoke(null);
                  return;
              }

              var list = new List<ScoreEntry>();
              foreach (var doc in t.Result.Documents)
              {
                  string uid = doc.TryGetValue<string>("userId", out var u) ? u : doc.Id;
                  string email = doc.TryGetValue<string>("email", out var e) ? e : "";
                  float dist = doc.TryGetValue<float>("bestDistance", out var d) ? d : 0f;
                  float speed = doc.TryGetValue<float>("bestMaxSpeed", out var s) ? s : 0f;

                  list.Add(new ScoreEntry(uid, email, dist, speed));
              }

              onDone?.Invoke(list);
          });
    }
}