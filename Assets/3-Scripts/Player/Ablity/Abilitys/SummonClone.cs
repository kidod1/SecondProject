using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    [Tooltip("������ Ŭ�� ������ ���� (��: 0.3f = 30%)")]
    [Range(0f, 2f)] // ������ ���� ���� ���� (0% ~ 200%)
    public float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f }; // ������ ������ ���� �迭

    public GameObject clonePrefab;  // ��� ������
    private GameObject cloneInstance;
    private RotatingObject rotatingObject;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {

        if (currentLevel < damageMultipliers.Length)
        {
            if (cloneInstance == null)
            {
                cloneInstance = Instantiate(clonePrefab, player.transform);
                rotatingObject = cloneInstance.GetComponent<RotatingObject>();
                if (rotatingObject != null)
                {
                    rotatingObject.player = player.transform;
                    rotatingObject.playerShooting = player;
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                    Debug.Log($"SummonClone applied at Level {currentLevel + 1} with Damage Multiplier: {damageMultipliers[currentLevel] * 100}%");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject ������Ʈ�� Ŭ�� �����տ� �����ϴ�.");
                }
            }
            else
            {
                if (rotatingObject != null)
                {
                    rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
                    Debug.Log($"SummonClone: ���� Ŭ���� Damage Multiplier�� {damageMultipliers[currentLevel] * 100}%�� ������Ʈ�Ǿ����ϴ�.");
                }
                else
                {
                    Debug.LogError("SummonClone: RotatingObject�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"SummonClone: currentLevel ({currentLevel + 1})�� damageMultipliers �迭 ������ �ʰ��߽��ϴ�. ������ ������ ������ ������ ����մϴ�.");
            if (rotatingObject != null)
            {
                rotatingObject.damageMultiplier = damageMultipliers[damageMultipliers.Length - 1];
                Debug.Log($"SummonClone: Ŭ���� Damage Multiplier�� {damageMultipliers[damageMultipliers.Length - 1] * 100}%�� �����Ǿ����ϴ�.");
            }
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ������ ������ �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"SummonClone ���׷��̵�: ���� ���� {currentLevel + 1}, ������ ���� {damageMultipliers[currentLevel] * 100}%");
            Apply(PlayManager.I.GetPlayer()); // ���׷��̵� �� Ŭ���� ������ ������ ������Ʈ
        }
        else
        {
            Debug.LogWarning("SummonClone: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ������ ������ (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageMultipliers.Length)
        {
            return Mathf.RoundToInt(damageMultipliers[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
            rotatingObject = null;
            Debug.Log("SummonClone ������ �ʱ�ȭ�Ǿ����ϴ�. Ŭ���� �ı��Ǿ����ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, damageMultipliers.Length: {damageMultipliers.Length}, maxLevel: {maxLevel}");

        if (currentLevel < damageMultipliers.Length && currentLevel >= 0)
        {
            float damageMultiplierPercent = damageMultipliers[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: Ŭ���� ������ {damageMultiplierPercent}% ����";
        }
        else if (currentLevel >= damageMultipliers.Length)
        {
            float maxDamageMultiplierPercent = damageMultipliers[damageMultipliers.Length - 1] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: Ŭ���� ������ {maxDamageMultiplierPercent}% ����";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
