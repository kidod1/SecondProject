using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JokerDraw")]
public class JokerDraw : Ability
{
    [Tooltip("������ �Ϲ� ���� ��� Ȯ�� (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] instantKillChances; // ������ ��� Ȯ�� �迭

    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("JokerDraw Apply: �÷��̾� �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        Debug.Log($"JokerDraw��(��) �÷��̾�� ����Ǿ����ϴ�. ���� ����: {currentLevel + 1}");
    }

    /// <summary>
    /// ���͸� �������� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">������ ������ Collider2D</param>
    public void OnHitMonster(Collider2D enemy)
    {
        if (playerInstance == null)
        {
            Debug.LogError("JokerDraw OnHitMonster: �÷��̾� �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        Monster monster = enemy.GetComponent<Monster>();

        // �Ϲ� ���Ϳ� ���ؼ��� ��� Ȯ�� ����
        if (monster != null && !monster.isElite)
        {
            float currentChance = GetCurrentInstantKillChance();
            if (Random.value < currentChance)
            {
                monster.TakeDamage(monster.GetCurrentHP(), PlayManager.I.GetPlayerPosition());
                Debug.Log($"JokerDraw: {monster.name}��(��) �����׽��ϴ�! (Ȯ��: {currentChance * 100}%)");
            }
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ��� Ȯ���� ������ŵ�ϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"JokerDraw ���׷��̵�: ���� ���� {currentLevel + 1}, ��� Ȯ�� {GetCurrentInstantKillChance() * 100}%");
        }
        else
        {
            Debug.LogWarning("JokerDraw: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ���� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ��� Ȯ�� ���� (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            return Mathf.RoundToInt(instantKillChances[currentLevel] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: �Ϲ� ���� ��� Ȯ�� +{percentChance}%";
        }
        else if (currentLevel == maxLevel && currentLevel < instantKillChances.Length)
        {
            float percentChance = instantKillChances[currentLevel] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: �Ϲ� ���� ��� Ȯ�� +{percentChance}%";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        Debug.Log("JokerDraw ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���� ������ ��� Ȯ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ��� Ȯ��</returns>
    private float GetCurrentInstantKillChance()
    {
        if (currentLevel < instantKillChances.Length)
        {
            return instantKillChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"JokerDraw: ���� ���� {currentLevel + 1}�� instantKillChances �迭�� ������ ������ϴ�. ������ ���� ����մϴ�.");
            return instantKillChances[instantKillChances.Length - 1];
        }
    }
}
