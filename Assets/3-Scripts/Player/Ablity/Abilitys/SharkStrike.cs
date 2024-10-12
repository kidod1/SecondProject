using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    [Tooltip("������ ��� ������ ������")]
    public int[] damageIncreases;  // ������ ��� ������ ������ �迭

    public GameObject sharkPrefab;  // ��� ������
    public int hitThreshold = 5;  // ���� �Ӱ谪
    public float sharkSpeed = 5f;  // ��� �ӵ�
    public float chaseDelay = 0.5f;  // ��� �߰� ���� �� ��� �ð�
    public float maxSearchTime = 3f; // �� ���͸� ã�� �ִ� �ð�

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        playerInstance = player;
        Debug.Log($"SharkStrike applied. Current Level: {currentLevel + 1}");
    }

    /// <summary>
    /// �÷��̾ ���� ���߽����� �� ȣ��Ǵ� �޼���
    /// </summary>
    /// <param name="enemy">���� ���� �ݶ��̴�</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;
        Debug.Log($"SharkStrike HitCount: {hitCount}/{hitThreshold}");

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    /// <summary>
    /// �� �����ϴ� �޼���
    /// </summary>
    private void SpawnShark()
    {
        if (sharkPrefab != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                // ������ ���� ������ �������� ������
                int damageIncrease = GetSharkDamageIncrease();
                sharkInstance.Initialize(sharkSpeed, chaseDelay, maxSearchTime, damageIncrease);
                Debug.Log($"SharkStrike: �� �����Ǿ����ϴ�. ������ ������: {damageIncrease}");
            }
            else
            {
                Debug.LogError("SharkStrike: Shark ������Ʈ�� �����տ� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("SharkStrike: Shark �������� �����ϴ�. �� ������ �� �����ϴ�.");
        }
    }


    /// <summary>
    /// ���� ������ ���� ��� ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>����� ������ ������ (����)</returns>
    private int GetSharkDamageIncrease()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel];
        }
        else
        {
            Debug.LogWarning($"SharkStrike: currentLevel ({currentLevel}) exceeds damageIncreases �迭 ����. ������ ������ �������� ����մϴ�.");
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1) // maxLevel�� 5��� currentLevel�� 0~4
        {
            currentLevel++;
            Debug.Log($"SharkStrike upgraded to Level {currentLevel + 1}. ������ ������: {damageIncreases[currentLevel]}");
        }
        else
        {
            Debug.LogWarning("SharkStrike: Already at max level.");
        }
    }

    /// <summary>
    /// ���� ������ ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ������ ������ (����)</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;  // ���� Ƚ�� �ʱ�ȭ
        currentLevel = 0;
        Debug.Log("SharkStrike level has been reset.");
    }

    /// <summary>
    /// �ɷ��� ���� ���¿� ȿ���� �����ϴ� ���ڿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        Debug.Log($"GetDescription called. Current Level: {currentLevel + 1}, damageIncreases.Length: {damageIncreases.Length}, maxLevel: {maxLevel}");

        if (currentLevel < damageIncreases.Length && currentLevel >= 0)
        {
            int damageIncrease = damageIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {hitThreshold}ȸ ���� ������ ���� ����ٴϴ� ��� ��ȯ. ������ +{damageIncrease}";
        }
        else if (currentLevel >= damageIncreases.Length)
        {
            int maxDamageIncrease = damageIncreases[damageIncreases.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� {hitThreshold}ȸ ���� ������ ���� ����ٴϴ� ��� ��ȯ. ������ +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}
