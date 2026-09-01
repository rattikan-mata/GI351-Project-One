using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SecretCharacter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[YOU WIN!] You saved Zenpai!");
            GameManager.Instance.TriggerGameWin();
        }
    }
}