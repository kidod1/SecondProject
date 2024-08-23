using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    public float defaultPlayerSpeed = 5;
    public int defaultPlayerDamage = 5;
    public float defaultProjectileSpeed = 10;
    public float defaultProjectileRange = 2;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 100;
    public int defaultShield = 0;
    public float defalutShotCooldown = 0.5f;
    public int defalutDefense = 0;

    public float currentPlayerSpeed;
    public int currentPlayerDamage;
    public float currentProjectileSpeed;
    public float currentProjectileRange;
    public int currentProjectileType;
    public int currentMaxHP;
    public int currentShield;
    public float currentShotCooldown;
    public int currentDefense;
    public int currentExperience;
    public int currentHP;
    public int currentCurrency;

    [Tooltip("경험치 테이블")]
    public int[] experienceThresholds = { 100, 200, 400, 800, 1600 };

    [Tooltip("경험치 획득량 배수 (기본값 1.0 = 100%)")]
    public float experienceMultiplier;

    public float defalutExperienceMultiplier = 1.0f;

    // 플레이어의 현재 레벨
    public int currentLevel;

    public void InitializeStats()
    {
        currentPlayerSpeed = defaultPlayerSpeed;
        currentPlayerDamage = defaultPlayerDamage;
        currentProjectileSpeed = defaultProjectileSpeed;
        currentProjectileRange = defaultProjectileRange;
        currentProjectileType = defaultProjectileType;
        currentShotCooldown = defalutShotCooldown;
        currentDefense = defalutDefense;
        currentMaxHP = defaultMaxHP;
        experienceMultiplier = defalutExperienceMultiplier;
        currentHP = currentMaxHP;
        currentExperience = 0;
        currentLevel = 1;
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
    public void GainExperience(int amount)
    {
        // 경험치 획득량 배수 적용
        int adjustedAmount = Mathf.RoundToInt(amount * experienceMultiplier);
        currentExperience += adjustedAmount;

        // 레벨업 체크
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (currentLevel < experienceThresholds.Length && currentExperience >= experienceThresholds[currentLevel])
        {
            currentExperience -= experienceThresholds[currentLevel];
            currentLevel++;
        }
    }
}
