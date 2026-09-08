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
    [SerializeField] private AudioClip monster1Dead;

    [Tooltip("ปล่อยว่างไว้ได้เลย จะได้ไม่มีเสียงตอนตาย")]
    [SerializeField] private AudioClip elithMonsterDead;

    [Tooltip("ใส่เสียงตีตุ๊บๆ ตอนโดนตีรัวๆ ที่นี่")]
    [SerializeField] private AudioClip eliteMonsterHit;
    private float lastEliteHitTime = -999f;

    // ---------------- Potion ----------------
    [Header("Potion SFX")]
    [SerializeField] private AudioClip potionPickup;

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
    [Tooltip("ลาก AudioSource แยกต่างหาก ไว้เล่นเสียง Stage 3 โดยเฉพาะ")]
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

    private void PlayRandomSFX(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;
        AudioClip chosen = clips[Random.Range(0, clips.Length)];
        if (chosen != null) sfxSource.PlayOneShot(chosen);
    }

    private void PlayRandomSFXNoRepeat(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;

        int index;
        if (clips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, clips.Length);
            } while (index == lastIndex);
        }

        lastIndex = index;
        AudioClip chosen = clips[index];
        if (chosen != null) sfxSource.PlayOneShot(chosen);
    }

    // ================= Ambient =================
    public void PlayAmbientLoop() => PlayBGM(ambientLoop);

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

    public void PlayEliteMonsterHit()
    {
        // ป้องกันเสียงซ้อนกันถี่เกินไป (หน่วง 0.08 วินาที = กดรัวสุด 12 ครั้ง/วิ เสียงก็จะไม่แตก)
        if (Time.time - lastEliteHitTime < 0.08f) return;

        lastEliteHitTime = Time.time;
        PlaySFX(eliteMonsterHit);
    }

    // ================= Potion =================
    public void PlayPotionPickup() => PlaySFX(potionPickup);

    // ================= Menu =================
    public void PlayMenuStart() => PlaySFX(menuStart);
    public void PlayUIClick() => PlaySFX(uiClick);
    public void PlayFadeInMenu() => PlaySFX(fadeInMenu);
    public void PlayFadeInCutscene() => PlaySFX(fadeInCutscene);

    // ================= Gameplay (Rage / Goal) =================
    public void PlayRageUp() => PlaySFX(rageUp);
    public void PlayGoal() => PlaySFX(goalSound);

    public void PlayRageStage3Loop()
    {
        if (rageStage3Source == null || rageStage3Clip == null) return;
        if (rageStage3Source.isPlaying && rageStage3Source.clip == rageStage3Clip) return;

        rageStage3Source.clip = rageStage3Clip;
        rageStage3Source.loop = true;
        rageStage3Source.Play();
    }

    public void StopRageStage3Loop()
    {
        if (rageStage3Source == null) return;
        rageStage3Source.Stop();
    }
}