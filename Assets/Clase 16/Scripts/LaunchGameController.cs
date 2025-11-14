using UnityEngine;
using UnityEngine.UI;

public class LaunchGameController : MonoBehaviour
{
    public enum State
    {
        AngleSelection,
        PowerSelection,
        InFlight,
        Finished
    }

    [Header("Referencias")]
    public Rigidbody projectileRb;
    public Transform angleArrow;
    public Slider powerSlider;
    public UIScoreboardManager scoreboard;

    [Header("Ángulo")]
    public float minAngle = 10f;
    public float maxAngle = 80f;
    public float angleSpeed = 60f;

    [Header("Fuerza")]
    public float minForce = 5f;
    public float maxForce = 25f;
    public float powerSpeed = 1.5f;

    [Header("Detección fin de vuelo")]
    public float stopSpeedThreshold = 0.05f;
    public float stopDelay = 1f;

    public State currentState = State.AngleSelection;

    float currentAngle;
    int angleDirection = 1;

    float power01;
    int powerDirection = 1;

    Vector3 startPos;
    Quaternion startRot;

    float maxDistance;
    float maxSpeed;
    float stoppedTime;

    void Start()
    {
        if (!projectileRb)
            projectileRb = GetComponent<Rigidbody>();

        startPos = projectileRb.position;
        startRot = projectileRb.rotation;

        ResetLauncherState();

        if (FirebaseManager.I != null)
            FirebaseManager.I.OnLogout += HandleLogout;
    }

    void OnDestroy()
    {
        if (FirebaseManager.I != null)
            FirebaseManager.I.OnLogout -= HandleLogout;
    }

    void HandleLogout()
    {
        ResetLauncherState();
    }

    bool UserLoggedIn()
    {
        return FirebaseManager.I != null
               && FirebaseManager.I.FirebaseReady
               && FirebaseManager.I.CurrentUser != null;
    }

    void Update()
    {
        if (!UserLoggedIn())
            return;

        switch (currentState)
        {
            case State.AngleSelection:
                UpdateAngleSelection();
                if (Input.GetKeyDown(KeyCode.Space))
                    ConfirmAngle();
                break;

            case State.PowerSelection:
                UpdatePowerSelection();
                if (Input.GetKeyDown(KeyCode.Space))
                    ConfirmPowerAndLaunch();
                break;

            case State.InFlight:
                UpdateFlight();
                break;

            case State.Finished:
                break;
        }
    }

    void ResetLauncherState()
    {
        currentState = State.AngleSelection;

        currentAngle = minAngle;
        angleDirection = 1;
        UpdateArrowVisual();

        power01 = 0f;
        powerDirection = 1;
        if (powerSlider != null)
            powerSlider.value = power01;

        maxDistance = 0f;
        maxSpeed = 0f;
        stoppedTime = 0f;


        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;
        projectileRb.isKinematic = true;
        projectileRb.position = startPos;
        projectileRb.rotation = startRot;


        if (scoreboard != null)
            scoreboard.HideScoreboard();
    }

    public void RetryThrow()
    {
        if (!UserLoggedIn())
            return;

        ResetLauncherState();
    }

    // ---------- FASE 1: ÁNGULO ----------

    void UpdateAngleSelection()
    {
        currentAngle += angleDirection * angleSpeed * Time.deltaTime;

        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            angleDirection = -1;
        }
        else if (currentAngle <= minAngle)
        {
            currentAngle = minAngle;
            angleDirection = 1;
        }

        UpdateArrowVisual();
    }

    void UpdateArrowVisual()
    {
        if (!angleArrow) return;
        angleArrow.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
    }

    public void ConfirmAngle()
    {
        if (currentState != State.AngleSelection) return;
        Debug.Log($"Ángulo elegido: {currentAngle:F1}°");
        currentState = State.PowerSelection;
    }

    // ---------- FASE 2: FUERZA ----------

    void UpdatePowerSelection()
    {
        power01 += powerDirection * powerSpeed * Time.deltaTime;
        if (power01 >= 1f)
        {
            power01 = 1f;
            powerDirection = -1;
        }
        else if (power01 <= 0f)
        {
            power01 = 0f;
            powerDirection = 1;
        }

        if (powerSlider != null)
            powerSlider.value = power01;
    }

    public void ConfirmPowerAndLaunch()
    {
        if (currentState != State.PowerSelection) return;

        float force = Mathf.Lerp(minForce, maxForce, power01);
        Debug.Log($"Fuerza elegida: {force:F1} (power01={power01:F2})");

        LaunchProjectile(force);
        currentState = State.InFlight;
    }

    void LaunchProjectile(float force)
    {
        projectileRb.isKinematic = false;
        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;

        Vector3 dir = Quaternion.Euler(-currentAngle, 0f, 0f) * Vector3.forward;
        projectileRb.AddForce(dir * force, ForceMode.Impulse);

        startPos = projectileRb.position;
        maxDistance = 0f;
        maxSpeed = 0f;
        stoppedTime = 0f;
    }

    // ---------- FASE 3: VUELO ----------

    void UpdateFlight()
    {
        float speed = projectileRb.linearVelocity.magnitude;
        maxSpeed = Mathf.Max(maxSpeed, speed);

        float dist = Vector3.Distance(startPos, projectileRb.position);
        maxDistance = Mathf.Max(maxDistance, dist);

        if (speed < stopSpeedThreshold)
        {
            stoppedTime += Time.deltaTime;
            if (stoppedTime >= stopDelay)
            {
                EndRun();
            }
        }
        else
        {
            stoppedTime = 0f;
        }
    }

    void EndRun()
    {
        currentState = State.Finished;
        Debug.Log($"Tiro terminado → Distancia máx: {maxDistance:F2} | Velocidad máx: {maxSpeed:F2}");

        if (scoreboard != null)
        {
            scoreboard.ShowScoreboard(maxDistance, maxSpeed);
        }
        else
        {
            Debug.LogWarning("No hay UIScoreboardManager asignado en LaunchGameController.");
        }
    }

    // ---------- Métodos para botones de UI ----------

    public void OnAngleButtonPressed()
    {
        ConfirmAngle();
    }

    public void OnPowerButtonPressed()
    {
        ConfirmPowerAndLaunch();
    }
}
