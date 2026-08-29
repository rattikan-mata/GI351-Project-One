using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentCombo { get; private set; }
    public int TotalMisses { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetAll();
    }

    public void RegisterHit()
    {
        TotalMisses = 0;
        CurrentCombo++;
        Debug.Log($"Combo: {CurrentCombo}");
    }

    public void RegisterMiss()
    {
        CurrentCombo = 0;
        TotalMisses++;
        Debug.Log($"Miss: {TotalMisses}");
    }

    public void ResetAll()
    {
        CurrentCombo = 0;
        TotalMisses = 0;
        Debug.Log($"Combo: {CurrentCombo} | Miss: {TotalMisses}");
    }
}