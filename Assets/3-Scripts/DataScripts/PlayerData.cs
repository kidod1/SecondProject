using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 버프 타입을 정의하는 열거형
/// </summary>
public enum BuffType
{
    AttackDamage,   // 공격력 버프
    AttackSpeed,    // 공격 속도 버프
    MovementSpeed   // 이동 속도 버프
}

[CreateAssetMenu(menuName = "Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    // 기본 스탯
    [Header("Default Stats")]
    public int DataPlayerDamage = 10;
    public float DataPlayerAttackSpeed = 2f;
    public float DataPlayerMoveSpeed = 5f;
    public int DataPlayerDefense = 0;

    public float defaultPlayerSpeed = 5f;
    public int defaultPlayerDamage = 10;
    public float defaultProjectileSpeed = 10f;
    public float defaultProjectileRange = 2f;
    public int defaultProjectileType = 0;
    public int defaultMaxHP = 100;
    public int defaultShield = 0;
    public float defaultAttackSpeed = 2.0f;
    public int defaultDefense = 0;

    [SerializeField]
    private float defaultExperienceMultiplier = 1.0f;

    // 현재 스탯
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
    private int _currentDefense;
    [SerializeField]
    private int _currentExperience;
    [SerializeField]
    private int _currentHP;
    [SerializeField]
    private int _currentCurrency;
    [SerializeField]
    private int _currentLevel;
    [SerializeField]
    private float _currentAttackSpeed;

    [Tooltip("경험치 테이블")]
    public int[] experienceThresholds = { 100, 200, 400, 800, 1600 };

    [Tooltip("경험치 획득량 배수 (기본값 1.0 = 100%)")]
    [SerializeField]
    private float _experienceMultiplier;

    // 버프 관리용 딕셔너리
    private Dictionary<string, float> attackDamageBuffs = new Dictionary<string, float>();
    private Dictionary<string, float> attackSpeedBuffs = new Dictionary<string, float>();
    private Dictionary<string, float> movementSpeedBuffs = new Dictionary<string, float>();

    // 스탯 변경 시 호출되는 이벤트
    public event UnityAction OnStatsChanged;

    /// <summary>
    /// 스탯을 초기화합니다.
    /// </summary>
    public void InitializeStats()
    {
        defaultPlayerDamage = DataPlayerDamage;
        defaultAttackSpeed = DataPlayerAttackSpeed;
        defaultDefense = DataPlayerDefense;
        defaultPlayerSpeed = DataPlayerMoveSpeed;

        currentPlayerSpeed = defaultPlayerSpeed;
        currentPlayerDamage = defaultPlayerDamage;
        currentProjectileSpeed = defaultProjectileSpeed;
        currentProjectileRange = defaultProjectileRange;
        currentProjectileType = defaultProjectileType;
        currentDefense = defaultDefense;
        currentMaxHP = defaultMaxHP;
        currentHP = currentMaxHP;
        currentExperience = 0;
        currentLevel = 1;
        currentCurrency = 0;
        currentShield = defaultShield;

        currentAttackSpeed = defaultAttackSpeed;

        // 버프 딕셔너리 초기화
        attackDamageBuffs.Clear();
        attackSpeedBuffs.Clear();
        movementSpeedBuffs.Clear();

        OnStatsChanged?.Invoke();
    }

    // 현재 스탯 프로퍼티들
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

    public float currentAttackSpeed
    {
        get => _currentAttackSpeed;
        set
        {
            if (Mathf.Abs(_currentAttackSpeed - value) > 0.0001f)
            {
                _currentAttackSpeed = value;
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

    // 버프된 스탯 계산 프로퍼티들
    public int buffedPlayerDamage
    {
        get
        {
            float totalBuff = 0f;
            foreach (var buff in attackDamageBuffs.Values)
            {
                totalBuff += buff;
            }
            return Mathf.RoundToInt(defaultPlayerDamage + totalBuff);
        }
    }
    public float buffedPlayerSpeed
    {
        get
        {
            float totalBuff = 0f;
            foreach (var buff in movementSpeedBuffs.Values)
            {
                totalBuff += buff;
            }
            return defaultPlayerSpeed + totalBuff;
        }
    }

    /// <summary>
    /// 버프를 추가합니다.
    /// </summary>
    public void AddBuff(string abilityName, BuffType buffType, float value)
    {
        switch (buffType)
        {
            case BuffType.AttackDamage:
                attackDamageBuffs[abilityName] = value;
                break;
            case BuffType.AttackSpeed:
                currentAttackSpeed += value;
                break;
            case BuffType.MovementSpeed:
                movementSpeedBuffs[abilityName] = value;
                break;
        }
        OnStatsChanged?.Invoke();
    }
    public void RemoveBuff(string abilityName, BuffType buffType, float value = 0f)
    {
        switch (buffType)
        {
            case BuffType.AttackDamage:
                attackDamageBuffs.Remove(abilityName);
                break;
            case BuffType.AttackSpeed:
                currentAttackSpeed -= value;
                break;
            case BuffType.MovementSpeed:
                movementSpeedBuffs.Remove(abilityName);
                break;
        }
        OnStatsChanged?.Invoke();
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
