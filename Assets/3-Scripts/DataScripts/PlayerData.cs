using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    // Default stats
    [Header("Default Stats")]
    public float defaultPlayerSpeed = 5f;
    public int defaultPlayerDamage = 5;
    public float defaultProjectileSpeed = 10f;
    public float defaultProjectileRange = 2f;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 100;
    public int defaultShield = 0;
    public float defaultShootCooldown = 0.5f;
    public int defaultDefense = 0;

    // Current stats (프라이빗 필드 이름 변경)
    [Header("Current Stats")]
    [SerializeField]
    private float _currentPlayerSpeed;
    [SerializeField]
    private int _currentPlayerDamage;
    [SerializeField]
    private float _currentProjectileSpeed;
    [SerializeField]
    private float _currentProjectileRange;
    [SerializeField]
    private int _currentProjectileType;
    [SerializeField]
    private int _currentMaxHP;
    [SerializeField]
    private int _currentShield;
    [SerializeField]
    private float _currentShootCooldown;
    [SerializeField]
    private int _currentDefense;
    [SerializeField]
    private int _currentExperience;
    [SerializeField]
    private int _currentHP;
    [SerializeField]
    private int _currentCurrency;
    [SerializeField]
    private int _currentLevel;

    [Tooltip("경험치 테이블")]
    public int[] experienceThresholds = { 100, 200, 400, 800, 1600 };

    [Tooltip("경험치 획득량 배수 (기본값 1.0 = 100%)")]
    [SerializeField]
    private float _experienceMultiplier;

    [SerializeField]
    private float defaultExperienceMultiplier = 1.0f;

    [Header("Reverse Attack Stats")]
    [Tooltip("현재 반전 공격 데미지 퍼센트 (0.0f ~ 1.0f)")]
    [Range(0f, 1.0f)]
    [SerializeField]
    private float _reverseAttackDamagePercentage = 1.0f; // 기본값 100%

    // 스탯 변경 시 호출되는 이벤트
    public event UnityAction OnStatsChanged;

    /// <summary>
    /// 스탯을 초기화합니다.
    /// </summary>
    public void InitializeStats()
    {
        currentPlayerSpeed = defaultPlayerSpeed;
        currentPlayerDamage = 10;
        defaultPlayerDamage = 10;
        currentProjectileSpeed = defaultProjectileSpeed;
        currentProjectileRange = defaultProjectileRange;
        currentProjectileType = defaultProjectileType;
        currentShootCooldown = defaultShootCooldown;
        currentDefense = defaultDefense;
        currentMaxHP = defaultMaxHP;
        experienceMultiplier = defaultExperienceMultiplier;
        currentHP = currentMaxHP;
        currentExperience = 0;
        currentLevel = 1;
        reverseAttackDamagePercentage = 1.0f; // 초기화
        OnStatsChanged?.Invoke();
    }

    public float currentPlayerSpeed
    {
        get => _currentPlayerSpeed;
        set
        {
            if (_currentPlayerSpeed != value)
            {
                _currentPlayerSpeed = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentPlayerDamage
    {
        get => _currentPlayerDamage;
        set
        {
            if (_currentPlayerDamage != value)
            {
                _currentPlayerDamage = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public float currentProjectileSpeed
    {
        get => _currentProjectileSpeed;
        set
        {
            if (_currentProjectileSpeed != value)
            {
                _currentProjectileSpeed = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public float currentProjectileRange
    {
        get => _currentProjectileRange;
        set
        {
            if (_currentProjectileRange != value)
            {
                _currentProjectileRange = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentProjectileType
    {
        get => _currentProjectileType;
        set
        {
            if (_currentProjectileType != value)
            {
                _currentProjectileType = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentMaxHP
    {
        get => _currentMaxHP;
        set
        {
            if (_currentMaxHP != value)
            {
                _currentMaxHP = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentShield
    {
        get => _currentShield;
        set
        {
            if (_currentShield != value)
            {
                _currentShield = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public float currentShootCooldown
    {
        get => _currentShootCooldown;
        set
        {
            if (Mathf.Abs(_currentShootCooldown - value) > 0.0001f)
            {
                _currentShootCooldown = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentDefense
    {
        get => _currentDefense;
        set
        {
            if (_currentDefense != value)
            {
                _currentDefense = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentExperience
    {
        get => _currentExperience;
        set
        {
            if (_currentExperience != value)
            {
                _currentExperience = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentHP
    {
        get => _currentHP;
        set
        {
            if (_currentHP != value)
            {
                _currentHP = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentCurrency
    {
        get => _currentCurrency;
        set
        {
            if (_currentCurrency != value)
            {
                _currentCurrency = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public int currentLevel
    {
        get => _currentLevel;
        set
        {
            if (_currentLevel != value)
            {
                _currentLevel = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public float reverseAttackDamagePercentage
    {
        get => _reverseAttackDamagePercentage;
        set
        {
            if (Mathf.Abs(_reverseAttackDamagePercentage - value) > 0.0001f)
            {
                _reverseAttackDamagePercentage = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    public float experienceMultiplier
    {
        get => _experienceMultiplier;
        set
        {
            if (Mathf.Abs(_experienceMultiplier - value) > 0.0001f)
            {
                _experienceMultiplier = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// 데미지를 증가시킵니다.
    /// </summary>
    public void IncreaseDamage(int amount)
    {
        currentPlayerDamage += amount;
    }

    /// <summary>
    /// 데미지를 받습니다.
    /// </summary>
    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(0, damage - currentDefense);
        currentHP -= finalDamage;
    }

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > currentMaxHP)
        {
            currentHP = currentMaxHP;
        }
    }

    /// <summary>
    /// 경험치를 획득합니다.
    /// </summary>
    public bool GainExperience(int amount)
    {
        int adjustedAmount = Mathf.RoundToInt(amount * experienceMultiplier);
        currentExperience += adjustedAmount;
        return CheckLevelUp();
    }

    /// <summary>
    /// 레벨업을 체크하고 처리합니다.
    /// </summary>
    private bool CheckLevelUp()
    {
        bool leveledUp = false;

        while (currentLevel < experienceThresholds.Length && currentExperience >= experienceThresholds[currentLevel - 1])
        {
            currentExperience -= experienceThresholds[currentLevel - 1];
            currentLevel++;
            leveledUp = true;
        }

        return leveledUp;
    }
}
