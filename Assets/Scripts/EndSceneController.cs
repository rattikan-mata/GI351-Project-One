using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndSceneController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ลาก TextMeshProUGUI ที่เขียนว่า Press Space to Main Menu มาใส่")]
    [SerializeField] private TextMeshProUGUI pressSpaceText;

    private void Start()
    {
        // ป้องกันกรณีที่หน้าเล่นเกมค้าง timeScale ไว้ (เช่นตายแล้วหยุดเวลา)
        Time.timeScale = 1f;

        // เริ่มอนิเมชันข้อความกะพริบ
        if (pressSpaceText != null)
        {
            StartCoroutine(BlinkPromptRoutine());
        }
    }

    private void Update()
    {
        // เมื่อกด Spacebar ที่ซีนนี้ จะพาผู้เล่นกลับ Main Menu ทันที
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
                AudioManager.Instance.StopRageStage3Loop();
            }

            // โหลดฉาก Main Menu (สมมติว่า Main Menu อยู่ Index ที่ 0 หรือเปลี่ยนตามลำดับใน Build Settings ของคุณ)
            SceneManager.LoadScene(0);
        }
    }

    // ฟังก์ชันทำข้อความกะพริบ (แบบเดียวกับ Cutscene)
    private IEnumerator BlinkPromptRoutine()
    {
        CanvasGroup canvasGroup = pressSpaceText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = pressSpaceText.gameObject.AddComponent<CanvasGroup>();
        }

        while (true)
        {
            float elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0.2f, elapsed / 0.6f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0.2f, 1f, elapsed / 0.6f);
                yield return null;
            }
        }
    }
}