using UnityEngine;

/// <summary>
/// จุดรวมเสียงทั้งหมดของเกม จัดหมวดตาม mind map:
/// BGM / Ambient Sound / Player / Enemy / Menu / Gameplay (Rage/Goal)
/// สคริปต์อื่นเรียกใช้ผ่าน AudioManager.Instance.PlayXxx()
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("ลากค่า AudioSource ที่ตั้ง Loop = true ไว้สำหรับเล่นเพลงพื้นหลัง")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("ลากค่า AudioSource เปล่าๆ ไว้สำหรับเล่น SFX แบบ PlayOneShot (ซ้อนกันได้)")]
    [SerializeField] private AudioSource sfxSource;

    // ---------------- BGM ----------------
    [Header("BGM")]
    [SerializeField] private AudioClip bgmMainMenu;
    [SerializeField] private AudioClip bgmInGame;

    // ---------------- Ambient Sound ----------------
    [Header("Ambient Sound")]
    [Tooltip("ยังไม่มีไฟล์จริง (Type something ในผัง) เว้นว่างไว้ก่อนได้")]
    [SerializeField] private AudioClip ambientLoop;

    // ---------------- Player ----------------
    [Header("Player SFX")]
    [SerializeField] private AudioClip playerAttack;

    [Tooltip("ระยะห่างขั้นต่ำระหว่างเสียงตี (วินาที) กันเสียงซ้อนกันถี่เกินไปตอน spam กด")]
    [SerializeField] private float attackSfxCooldown = 0.1f;
    private float lastAttackSfxTime = -999f;

    [Tooltip("ใส่ได้หลายเสียง จะสุ่มเล่น 1 เสียงทุกครั้งที่โดนตี กันไม่ให้ฟังซ้ำจำเจ")]
    [SerializeField] private AudioClip[] playerHurtClips;
    private int lastHurtClipIndex = -1; // จำ index ล่าสุดไว้ กันสุ่มได้ตัวเดิมซ้ำติดกัน

    [SerializeField] private AudioClip playerDead;

    // ---------------- Enemy ----------------
    [Header("Enemy SFX")]
    [Tooltip("ยังไม่มีไฟล์จริง (Type something ในผัง) เว้นว่างไว้ก่อนได้")]
    [SerializeField] private AudioClip monster1Dead;
    [Tooltip("ยังไม่มีไฟล์จริง (Type something ในผัง) เว้นว่างไว้ก่อนได้")]
    [SerializeField] private AudioClip elithMonsterDead;

    // ---------------- Menu ----------------
    [Header("Menu SFX")]
    [SerializeField] private AudioClip menuStart;
    [SerializeField] private AudioClip uiClick;
    [Tooltip("ยังไม่มีไฟล์จริง (Type something ในผัง) เว้นว่างไว้ก่อนได้")]
    [SerializeField] private AudioClip fadeInMenu;
    [Tooltip("ยังไม่มีไฟล์จริง (Type something ในผัง) เว้นว่างไว้ก่อนได้")]
    [SerializeField] private AudioClip fadeInCutscene;

    // ---------------- Gameplay (Rage / Goal) ----------------
    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip rageUp;
    [SerializeField] private AudioClip goalSound;

    [Header("Rage Stage 3 (Looping)")]
    [Tooltip("ลาก AudioSource แยกต่างหาก (ตั้ง Loop = true, Play On Awake = false) ไว้เล่นเสียง Stage 3 โดยเฉพาะ ไม่ใช้ sfxSource ร่วมกับเสียงอื่น จะได้ Start/Stop ตามสถานะได้แม่นยำ")]
    [SerializeField] private AudioSource rageStage3Source;
    [SerializeField] private AudioClip rageStage3Clip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ให้คงอยู่ข้ามซีน (เมนู <-> เกม)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ================= BGM =================

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // กันเล่นซ้ำเพลงเดิม

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayBGMMainMenu() => PlayBGM(bgmMainMenu);
    public void PlayBGMInGame() => PlayBGM(bgmInGame);
    public void StopBGM() { if (bgmSource != null) bgmSource.Stop(); }

    // ================= SFX (ใช้ร่วม) =================

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>สุ่มเล่นเสียงหนึ่งตัวจาก array ที่ให้มา ใช้กับเสียงที่อยากมีหลายแบบ (เช่น Player Hurt)</summary>
    private void PlayRandomSFX(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;
        AudioClip chosen = clips[Random.Range(0, clips.Length)];
        if (chosen != null) sfxSource.PlayOneShot(chosen);
    }

    /// <summary>
    /// เหมือน PlayRandomSFX ปกติ แต่กันไม่ให้สุ่มได้ตัวเดิมซ้ำติดกัน 2 รอบ
    /// (ใช้ lastIndex เป็น ref เก็บ index ล่าสุดที่เล่นไป)
    /// </summary>
    private void PlayRandomSFXNoRepeat(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;

        int index;
        if (clips.Length == 1)
        {
            index = 0; // มีเสียงเดียวก็เล่นเสียงนั้นแหละ ไม่มีให้เลี่ยง
        }
        else
        {
            do
            {
                index = Random.Range(0, clips.Length);
            } while (index == lastIndex); // สุ่มใหม่จนกว่าจะไม่ตรงกับตัวล่าสุด
        }

        lastIndex = index;
        AudioClip chosen = clips[index];
        if (chosen != null) sfxSource.PlayOneShot(chosen);
    }

    // ================= Ambient =================
    public void PlayAmbientLoop() => PlayBGM(ambientLoop); // ถ้าอยากให้ ambient วนลูปเหมือน BGM

    // ================= Player =================
    public void PlayPlayerAttack()
    {
        // กันเสียงซ้อนถี่เกินไปตอน spam กดตี
        if (Time.time - lastAttackSfxTime < attackSfxCooldown) return;
        lastAttackSfxTime = Time.time;
        PlaySFX(playerAttack);
    }

    public void PlayPlayerHurt() => PlayRandomSFXNoRepeat(playerHurtClips, ref lastHurtClipIndex);
    public void PlayPlayerDead() => PlaySFX(playerDead);

    // ================= Enemy =================
    public void PlayMonster1Dead() => PlaySFX(monster1Dead);
    public void PlayElithMonsterDead() => PlaySFX(elithMonsterDead);

    // ================= Menu =================
    public void PlayMenuStart() => PlaySFX(menuStart);
    public void PlayUIClick() => PlaySFX(uiClick);
    public void PlayFadeInMenu() => PlaySFX(fadeInMenu);
    public void PlayFadeInCutscene() => PlaySFX(fadeInCutscene);

    // ================= Gameplay (Rage / Goal) =================
    public void PlayRageUp() => PlaySFX(rageUp);
    public void PlayGoal() => PlaySFX(goalSound);

    /// <summary>เริ่มเล่นเสียง Stage 3 แบบ loop ถ้ากำลังเล่นอยู่แล้วจะไม่เล่นซ้อน</summary>
    public void PlayRageStage3Loop()
    {
        if (rageStage3Source == null || rageStage3Clip == null) return;
        if (rageStage3Source.isPlaying && rageStage3Source.clip == rageStage3Clip) return; // กันเล่นซ้อนตัวเอง

        rageStage3Source.clip = rageStage3Clip;
        rageStage3Source.loop = true;
        rageStage3Source.Play();
    }

    /// <summary>หยุดเสียง Stage 3 ทันที เรียกตอนหลุดออกจาก stage 3</summary>
    public void StopRageStage3Loop()
    {
        if (rageStage3Source == null) return;
        rageStage3Source.Stop();
    }
}