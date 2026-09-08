using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // จำตำแหน่งเริ่มต้นของกล้องไว้
        originalPos = transform.localPosition;
    }

    // เรียกฟังก์ชันนี้จากสคริปต์อื่นเพื่อสั่งให้กล้องสั่น
    public void TriggerShake(float duration, float magnitude)
    {
        StopAllCoroutines(); // หยุดการสั่นเก่า (ถ้ามี)
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // สุ่มตำแหน่งกล้องให้ขยับไปมา
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ดึงกล้องกลับมาตำแหน่งเดิมเป๊ะๆ เมื่อสั่นเสร็จ
        transform.localPosition = originalPos;
    }
}