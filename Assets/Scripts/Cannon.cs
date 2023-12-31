using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {
    [SerializeField] private float lagSpeed = 2.0f;
    private Quaternion targetRotation;

    [SerializeField] private GameObject cannonball;
    [SerializeField] private float cannonballOffset;
    [SerializeField] private float cannonballSpeed;
    [SerializeField] private float defaultAnimDelay = 0.1f;
    [SerializeField] private float fastAnimDelay = 0.1f;
    [SerializeField] private float animDelay = 0.1f;

    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float fastAttackDelay = 0.5f;
    [SerializeField] private float defaultAttackDelay = 0.5f;
    [SerializeField] private Coroutine refreshAttackCoro;

    [SerializeField] private bool isTimeSlowed = false;
    [SerializeField] private float timeSlow;
    [SerializeField] public float timeSlowAvailable = 100;
    [SerializeField] private float maxTimeSlow = 100;
    [SerializeField] private float timeSlowDecrease = 2;
    [SerializeField] private GameObject timeSlowBar;
    [SerializeField] private float regenDelay = 0.5f;
    [SerializeField] private float regenAmount = 0.1f;
    private float lastRealTime;

    private void Start() {
        lastRealTime = Time.realtimeSinceStartup;
        StartCoroutine(RegenerateTimeSlow());
    }

    private IEnumerator RegenerateTimeSlow() {
        while (true) {
            yield return new WaitForSeconds(regenDelay);
            if (!isTimeSlowed) {
                timeSlowAvailable += regenAmount;
                if (timeSlowAvailable > maxTimeSlow) {
                    timeSlowAvailable = maxTimeSlow;
                }
                SetTimeBarScale();
            }
        }
    }

    public void SetTimeBarScale() {
        timeSlowBar.transform.localScale = new Vector3(
            timeSlowBar.transform.localScale.x,
            timeSlowAvailable / maxTimeSlow * 10,
            timeSlowBar.transform.localScale.z
        );
    }

    private void Update() {
        float deltaTime = Time.realtimeSinceStartup - lastRealTime;
        lastRealTime = Time.realtimeSinceStartup;


        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPos.z = transform.position.z;
        Vector3 direction = cursorPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * lagSpeed);

        if (Input.GetMouseButton(0)) {
            if (canAttack) {
                canAttack = false;
                refreshAttackCoro = StartCoroutine(RefreshAttack());
                StartCoroutine(FireProjectile());
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && timeSlowAvailable > 0) {
            StopCoroutine(refreshAttackCoro);
            canAttack = true;
            attackDelay = fastAttackDelay;
            animDelay = fastAnimDelay;
            Time.timeScale = timeSlow;
            isTimeSlowed = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space)) {
            ResetTimeSpeed();
        }

        if (isTimeSlowed) {
            timeSlowAvailable -= timeSlowDecrease * deltaTime;
            timeSlowBar.transform.localScale = new Vector3(
                timeSlowBar.transform.localScale.x,
                timeSlowAvailable / maxTimeSlow * 10,
                timeSlowBar.transform.localScale.z
            );
            if (timeSlowAvailable < 0) {
                timeSlowAvailable = 0;
                ResetTimeSpeed();
            }
        }
    }

    private void FixedUpdate() {

    }

    private void ResetTimeSpeed() {
        attackDelay = defaultAttackDelay;
        animDelay = defaultAnimDelay;
        Time.timeScale = 1;
        isTimeSlowed = false;
        StopCoroutine(refreshAttackCoro);
        canAttack = true;
    }

    private IEnumerator RefreshAttack() {
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }

    private IEnumerator FireProjectile() {
        yield return new WaitForSeconds(animDelay);
        // squash, then unsquash the cannon for added visual effect
        transform.localScale = new Vector3(0.85f, 1.05f, 1.0f);
        yield return new WaitForSeconds(animDelay);
        transform.localScale = new Vector3(0.65f, 1.2f, 1.0f);
        yield return new WaitForSeconds(animDelay * 2);
        transform.localScale = new Vector3(0.85f, 1.05f, 1.0f);
        GameObject shot = Instantiate(
            cannonball, 
            transform.position + transform.right * cannonballOffset,
            transform.rotation
        );
        yield return new WaitForSeconds(animDelay / 2);
        transform.localScale = new Vector3(1.1f, 0.9f, 1.0f);
        shot.GetComponent<Rigidbody2D>().velocity = shot.transform.right * cannonballSpeed;
        yield return new WaitForSeconds(animDelay / 2);
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
