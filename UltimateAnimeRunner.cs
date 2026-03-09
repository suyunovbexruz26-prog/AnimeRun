using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UltimateAnimeRunner : MonoBehaviour
{
    // --- O'zgaruvchilar ---
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed = 12f;
    public float maxSpeed = 30f;

    private int desiredLane = 1; // 0:Chap, 1:O'rta, 2:O'ng
    public float laneDistance = 4f;

    public float jumpForce = 12f;
    public float gravity = -30f;

    // Swipe (Siltash) boshqaruvi
    private Vector2 startTouchPosition;
    private float swipeThreshold = 50f;

    // Holatlar va Bonuslar
    private bool isSliding = false;
    private bool isFlying = false;
    private bool isInvincible = false; // Bonus vaqtida o'lmaslik

    // UI elementlari (Inspektordan ulanadi)
    public Text coinsDisplay;
    public GameObject gameOverPanel;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Time.timeScale = 1; // O'yinni boshlashda vaqtni tiklash
        UpdateCoinUI();
    }

    void Update()
    {
        // 1. Tezlikni oshirish
        if (forwardSpeed < maxSpeed)
            forwardSpeed += 0.15f * Time.deltaTime;

        direction.z = forwardSpeed;

        // 2. Boshqaruv (Touch & Mouse)
        HandleInput();

        // 3. Gravitatsiya va Uchish
        if (controller.isGrounded && !isFlying)
        {
            direction.y = -1;
        }
        else if (!isFlying)
        {
            direction.y += gravity * Time.deltaTime;
        }

        // 4. Yo'laklar orasida harakatlanish
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;

        if (desiredLane == 0) targetPosition += Vector3.left * laneDistance;
        else if (desiredLane == 2) targetPosition += Vector3.right * laneDistance;

        // Silliq ko'chish (Lerp o'rniga aniqroq Move ishlatamiz)
        if (transform.position != targetPosition)
        {
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 30 * Time.deltaTime;
            if (moveDir.sqrMagnitude < diff.sqrMagnitude)
                controller.Move(moveDir);
            else
                controller.Move(diff);
        }

        // Doimiy oldinga harakat
        controller.Move(direction * Time.deltaTime);
    }

    void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) startTouchPosition = touch.position;
            else if (touch.phase == TouchPhase.Ended) AnalyzeSwipe(touch.position);
        }
        else if (Input.GetMouseButtonDown(0)) startTouchPosition = Input.mousePosition;
        else if (Input.GetMouseButtonUp(0)) AnalyzeSwipe(Input.mousePosition);
    }

    void AnalyzeSwipe(Vector2 endPosition)
    {
        Vector2 delta = endPosition - startTouchPosition;
        if (delta.magnitude > swipeThreshold)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0 && desiredLane < 2) desiredLane++;
                else if (delta.x < 0 && desiredLane > 0) desiredLane--;
            }
            else
            {
                if (delta.y > 0 && controller.isGrounded) Jump();
                else if (delta.y < 0 && !isSliding) StartCoroutine(Slide());
            }
        }
    }

    void Jump()
    {
        direction.y = jumpForce;
    }

    IEnumerator Slide()
    {
        isSliding = true;
        controller.height = 0.6f;
        controller.center = new Vector3(0, -0.7f, 0);
        // Animator.SetTrigger("Slide"); // Animatsiyangiz bo'lsa yoqing

        yield return new WaitForSeconds(1.2f);

        controller.height = 2f;
        controller.center = Vector3.zero;
        isSliding = false;
    }

    // --- TO'QNASHUVLAR ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && !isInvincible)
        {
            GameOver();
        }

        if (other.CompareTag("Coin"))
        {
            int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            PlayerPrefs.SetInt("TotalCoins", currentCoins + 1);
            UpdateCoinUI();
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Jetpack")) StartCoroutine(PowerUpJetpack());
        if (other.CompareTag("Sneakers")) StartCoroutine(PowerUpSneakers());
    }

    IEnumerator PowerUpJetpack()
    {
        isFlying = true;
        isInvincible = true;
        float originalY = transform.position.y;
        transform.position = new Vector3(transform.position.x, 10f, transform.position.z);
        direction.y = 0;
        
        yield return new WaitForSeconds(10f); // 10 soniya uchish
        
        isFlying = false;
        isInvincible = false;
    }

    IEnumerator PowerUpSneakers()
    {
        jumpForce = 22f; // Balandroq sakrash
        yield return new WaitForSeconds(8f);
        jumpForce = 12f;
    }

    void UpdateCoinUI()
    {
        if(coinsDisplay != null)
            coinsDisplay.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
    }

    void GameOver()
    {
        Time.timeScale = 0; // O'yinni to'xtatish
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
public AudioSource coinSound; // Inspektor orqali ulanadi
// OnTriggerEnter ichiga qo'shiladi:
if (other.CompareTag("Coin")) {
    coinSound.Play();
}
