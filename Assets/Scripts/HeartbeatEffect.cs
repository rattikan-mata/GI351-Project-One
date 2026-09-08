using UnityEngine;
using UnityEngine.UI;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ใส่ Image ของ Pink Frame")]
    public Image pinkFrame;
    [Tooltip("ใส่ Image ของ Heart Frame")]
    public Image heartFrame;

    [Header("Pink Frame Opacity Settings (0.0 - 1.0)")]
    [Range(0f, 1f)] public float phase1Opacity = 0.3f;
    [Range(0f, 1f)] public float phase2Opacity = 0.6f;
    [Range(0f, 1f)] public float phase3Opacity = 1.0f;

    void Start()
    {
        // เริ่มต้นด้วยการซ่อน Effect ไว้ก่อน (เฟส 0)
        SetHeartbeatPhase(0);
    }

    // เรียกฟังก์ชันนี้และใส่ตัวเลขเฟส 1, 2 หรือ 3 เมื่อต้องการเปลี่ยนเฟส
    public void SetHeartbeatPhase(int phase)
    {
        Color pinkColor = pinkFrame.color;

        switch (phase)
        {
            case 1:
                pinkColor.a = phase1Opacity;     // ตั้งค่า Opacity ตาม Inspector
                pinkFrame.gameObject.SetActive(true);
                heartFrame.gameObject.SetActive(false); // ซ่อนกรอบหัวใจ
                break;

            case 2:
                pinkColor.a = phase2Opacity;     // ตั้งค่า Opacity ตาม Inspector
                pinkFrame.gameObject.SetActive(true);
                heartFrame.gameObject.SetActive(false); // ซ่อนกรอบหัวใจ
                break;

            case 3:
                pinkColor.a = phase3Opacity;     // ตั้งค่า Opacity ตาม Inspector
                pinkFrame.gameObject.SetActive(true);
                heartFrame.gameObject.SetActive(true);  // โชว์กรอบหัวใจเฉพาะเฟส 3
                break;

            default:
                // เฟส 0 หรืออื่นๆ (ปิด Effect)
                pinkColor.a = 0f;
                pinkFrame.gameObject.SetActive(false);
                heartFrame.gameObject.SetActive(false);
                break;
        }

        // อัปเดตสีกลับไปที่ UI
        pinkFrame.color = pinkColor;
    }
}