using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ParasiticNest")]
public class ParasiticNest : Ability
{
    [Tooltip("������ ���� Ȯ�� (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float[] infectionChances; // ������ ���� Ȯ�� �迭

    private Player playerInstance;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("ParasiticNest Apply: �÷��̾� �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;
        Debug.Log($"ParasiticNest��(��) �÷��̾�� ����Ǿ����ϴ�. ���� ����: {currentLevel + 1}");
    }

    /// <summary>
    /// ����ü�� ���Ϳ� �¾��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">������ ������ Collider2D</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        float currentChance = GetCurrentInfectionChance();
        if (Random.value <= currentChance)
        {
            Monster monster = enemy.GetComponent<Monster>();
            if (monster != null && !monster.isInfected)
            {
                monster.isInfected = true;
                monster.StartCoroutine(ApplyInfectionEffect(monster));
                Debug.Log($"ParasiticNest: {monster.name}��(��) �����Ǿ����ϴ�! (Ȯ��: {currentChance * 100}%)");
            }
        }
    }

    /// <summary>
    /// ���� ȿ���� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="monster">���� ��� ����</param>
    /// <returns></returns>
    private IEnumerator ApplyInfectionEffect(Monster monster)
    {
        SpriteRenderer renderer = monster.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.green;

            yield return new WaitForSeconds(0.5f);
            renderer.color = originalColor;
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        playerInstance = null;
        Debug.Log("ParasiticNest ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���� ������ ���� Ȯ�� �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ���� Ȯ�� ���� (�ۼ�Ʈ)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < infectionChances.Length)
        {
            return Mathf.RoundToInt(infectionChances[currentLevel + 1] * 100); // �ۼ�Ʈ�� ��ȯ
        }
        return 0;
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���� Ȯ���� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"ParasiticNest ���׷��̵�: ���� ���� {currentLevel + 1}, ���� Ȯ�� {GetCurrentInfectionChance() * 100}%");
        }
        else
        {
            Debug.LogWarning("ParasiticNest: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel && currentLevel < infectionChances.Length)
        {
            float percentChance = infectionChances[currentLevel] * 100f;
            return $"{baseDescription}\nLv {currentLevel + 1}: ����ü ��Ʈ �� ���� Ȯ�� +{percentChance}%";
        }
        else if (currentLevel == maxLevel && currentLevel < infectionChances.Length)
        {
            float percentChance = infectionChances[currentLevel] * 100f;
            return $"{baseDescription}\n�ִ� ���� ����: ����ü ��Ʈ �� ���� Ȯ�� +{percentChance}%";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����";
        }
    }

    /// <summary>
    /// ���� ������ ���� Ȯ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���� Ȯ��</returns>
    private float GetCurrentInfectionChance()
    {
        if (currentLevel < infectionChances.Length)
        {
            return infectionChances[currentLevel];
        }
        else
        {
            Debug.LogWarning($"ParasiticNest: ���� ���� {currentLevel + 1}�� infectionChances �迭�� ������ ������ϴ�. ������ ���� ����մϴ�.");
            return infectionChances[infectionChances.Length - 1];
        }
    }
}
