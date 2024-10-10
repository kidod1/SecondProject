using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    // Default stats
    public float defaultPlayerSpeed = 5;
    public int defaultPlayerDamage = 5;
    public float defaultProjectileSpeed = 10;
    public float defaultProjectileRange = 2;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 100;
    public int defaultShield = 0;
    public float defalutShotCooldown = 0.5f;
    public int defalutDefense = 0;

    // Current stats
    public float currentPlayerSpeed;
    public int currentPlayerDamage;
    public float currentProjectileSpeed;
    public float currentProjectileRange;
    public int currentProjectileType;
    public int currentMaxHP;
    public int currentShield;
    public float currentShootCooldown;
    public int currentDefense;
    public int currentExperience;
    public int currentHP;
    public int currentCurrency;

    [Tooltip("경험치 테이블")]
    public int[] experienceThresholds = { 100, 200, 400, 800, 1600 };

    [Tooltip("경험치 획득량 배수 (기본값 1.0 = 100%)")]
    public float experienceMultiplier;

    public float defalutExperienceMultiplier = 1.0f;
    public int currentLevel;

    [Header("Reverse Attack Stats")]
    [Tooltip("현재 반전 공격 데미지 퍼센트 (0.0f ~ 1.0f)")]
    [Range(0f, 1.0f)]
    public float reverseAttackDamagePercentage = 1.0f; // 기본값 100%

    public void InitializeStats()
    {
        currentPlayerSpeed = defaultPlayerSpeed;
        currentPlayerDamage = defaultPlayerDamage;
        currentProjectileSpeed = defaultProjectileSpeed;
        currentProjectileRange = defaultProjectileRange;
        currentProjectileType = defaultProjectileType;
        currentShootCooldown = defalutShotCooldown;
        currentDefense = defalutDefense;
        currentMaxHP = defaultMaxHP;
        experienceMultiplier = defalutExperienceMultiplier;
        currentHP = currentMaxHP;
        currentExperience = 0;
        currentLevel = 1;
        reverseAttackDamagePercentage = 1.0f; // 초기화
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(0, damage - currentDefense);
        currentHP -= finalDamage;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > currentMaxHP)
        {
            currentHP = currentMaxHP;
        }
    }

    public bool GainExperience(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * experienceMultiplier);
        currentExperience += adjustedAmount;
        return CheckLevelUp();
    }

    private bool CheckLevelUp()
    {
        bool leveledUp = false;

        while (currentLevel < experienceThresholds.Length && currentExperience >= experienceThresholds[currentLevel])
        {
            currentExperience -= experienceThresholds[currentLevel];
            currentLevel++;
            leveledUp = true;
        }

        return leveledUp;
    }
}
