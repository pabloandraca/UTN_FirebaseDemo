using UnityEngine;
using TMPro;
using Firebase.Auth;

public class UIAuthManager : MonoBehaviour
{
    [Header("Campos de Login / Registro")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;

    [Header("Paneles")]
    public GameObject authPanel;
    public GameObject gamePanel;

    void Start()
    {
        FirebaseManager.I.OnLogin += HandleLogin;
        FirebaseManager.I.OnLogout += HandleLogout;

        if (FirebaseManager.I.CurrentUser != null)
            HandleLogin(FirebaseManager.I.CurrentUser);
        else
            HandleLogout();
    }

    void OnDestroy()
    {
        if (FirebaseManager.I == null) return;
        FirebaseManager.I.OnLogin -= HandleLogin;
        FirebaseManager.I.OnLogout -= HandleLogout;
    }

    // Botón "Registrar"
    public void AttemptRegistration()
    {
        string email = emailField.text;
        string password = passwordField.text;
        FirebaseManager.I.RegisterNewUser(email, password);
    }

    // Botón "Login"
    public void AttemptLogin()
    {
        string email = emailField.text;
        string password = passwordField.text;
        FirebaseManager.I.LoginUser(email, password);
    }

    // Botón "Logout"
    public void AttemptLogout()
    {
        FirebaseManager.I.LogoutUser();
    }

    void HandleLogin(FirebaseUser user)
    {
        Debug.Log("[UI] Login detectado: " + user.Email);
        authPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    void HandleLogout()
    {
        Debug.Log("[UI] Logout detectado");
        authPanel.SetActive(true);
        gamePanel.SetActive(false);
    }
}