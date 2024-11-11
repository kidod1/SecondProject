using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/CoinBetting")]
public class CoinBetting : Ability
{
    [Header("Coin Bomb Settings")]
    [Tooltip("�� �������� ��ź�� ���ط�")]
    public int[] damagePerLevel = { 50, 60, 70, 80, 90 }; // ���� 1~5

    [Tooltip("�� �������� ��ź�� �����ϴ� ��ٿ� �ð� (��)")]
    public float[] cooldownPerLevel = { 5f, 4.5f, 4f, 3.5f, 3f }; // ���� 1~5

    [Tooltip("�� �������� ��ź�� ���� �ð� (��)")]
    public float[] bombDurationPerLevel = { 10f, 12f, 14f, 16f, 18f }; // ���� 1~5

    [Tooltip("������ ���� ��ź ������")]
    public GameObject coinBombPrefab;

    [Tooltip("���� �� ������ ����Ʈ ������")]
    public GameObject explosionEffectPrefab; // �߰��� ����

    private Player playerInstance;
    private Coroutine bombCoroutine;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("CoinBetting Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        // ���� ��ź ���� �ڷ�ƾ ����
        if (bombCoroutine == null)
        {
            bombCoroutine = player.StartCoroutine(DropCoinBombs());
        }
    }

    /// <summary>
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ��Ÿ�Ӱ� ���ط�, ��ź ���� �ð��� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Debug.Log($"CoinBetting ���׷��̵�: ���� ���� {currentLevel + 1}");

            // ���׷��̵� �� ������, ��Ÿ��, ��ź ���� �ð� ������Ʈ
            if (playerInstance != null)
            {
                // �ʿ� �� �߰� ������ ������ �� �ֽ��ϴ�.
            }
        }
        else
        {
            Debug.LogWarning("CoinBetting: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        // �ڷ�ƾ ����
        if (bombCoroutine != null)
        {
            playerInstance.StopCoroutine(bombCoroutine);
            bombCoroutine = null;
        }

        // ���� ����� ���� ����
        RemoveCurrentBuff();
        currentLevel = 0;
    }

    /// <summary>
    /// ���� ������ ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ���ط� ������</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel + 1] - damagePerLevel[currentLevel];
        }
        return 0;
    }

    /// <summary>
    /// ���� �ð����� ���� ��ź�� ����߸��� �ڷ�ƾ
    /// </summary>
    private IEnumerator DropCoinBombs()
    {
        while (true)
        {
            DropCoinBomb();
            float currentCooldown = GetCurrentCooldownTime();
            yield return new WaitForSeconds(currentCooldown);
        }
    }

    /// <summary>
    /// ���� �÷��̾� ��ġ�� ���� ��ź�� �����մϴ�.
    /// </summary>
    private void DropCoinBomb()
    {
        if (coinBombPrefab == null)
        {
            Debug.LogError("CoinBetting: coinBombPrefab�� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;
        GameObject coinBombInstance = Instantiate(coinBombPrefab, spawnPosition, Quaternion.identity);

        CoinBomb coinBombScript = coinBombInstance.GetComponent<CoinBomb>();
        if (coinBombScript != null)
        {
            int currentDamage = GetCurrentDamage();
            float currentBombDuration = GetCurrentBombDuration();

            // ���� ����Ʈ �������� �Բ� �ʱ�ȭ
            coinBombScript.Initialize(currentDamage, currentBombDuration, explosionEffectPrefab);
        }
        else
        {
            Debug.LogError("CoinBetting: CoinBomb ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    private int GetCurrentDamage()
    {
        if (currentLevel < damagePerLevel.Length)
        {
            return damagePerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})�� damagePerLevel �迭�� ������ ������ϴ�. �⺻�� {damagePerLevel[damagePerLevel.Length - 1]}�� ��ȯ�մϴ�.");
        return damagePerLevel[damagePerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ��ź ���� �ð��� ��ȯ�մϴ�.
    /// </summary>
    private float GetCurrentBombDuration()
    {
        if (currentLevel < bombDurationPerLevel.Length)
        {
            return bombDurationPerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})�� bombDurationPerLevel �迭�� ������ ������ϴ�. �⺻�� {bombDurationPerLevel[bombDurationPerLevel.Length - 1]}�� ��ȯ�մϴ�.");
        return bombDurationPerLevel[bombDurationPerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �´� ��Ÿ�� �ð��� ��ȯ�մϴ�.
    /// </summary>
    private float GetCurrentCooldownTime()
    {
        if (currentLevel < cooldownPerLevel.Length)
        {
            return cooldownPerLevel[currentLevel];
        }
        Debug.LogWarning($"CoinBetting: currentLevel ({currentLevel})�� cooldownPerLevel �迭�� ������ ������ϴ�. �⺻�� {cooldownPerLevel[cooldownPerLevel.Length - 1]}�� ��ȯ�մϴ�.");
        return cooldownPerLevel[cooldownPerLevel.Length - 1];
    }

    /// <summary>
    /// ���� ������ �����մϴ�.
    /// </summary>
    private void RemoveCurrentBuff()
    {
        // ���� ������ ������ �����Ƿ� �ʿ� �� �߰� ����
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        if (currentLevel < damagePerLevel.Length && currentLevel < cooldownPerLevel.Length && currentLevel < bombDurationPerLevel.Length)
        {
            description += $"���� {currentLevel + 1}:\n" +
                           $"- ��ź ���ط�: {damagePerLevel[currentLevel]}\n" +
                           $"- ��ٿ�: {cooldownPerLevel[currentLevel]}��\n" +
                           $"- ��ź ���� �ð�: {bombDurationPerLevel[currentLevel]}��";
        }
        else
        {
            description += "�ִ� ������ �����߽��ϴ�.";
        }
        return description;
    }


    private void OnValidate()
    {
        // �迭�� ���̰� maxLevel�� ��ġ�ϵ��� ����
        if (damagePerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: damagePerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref damagePerLevel, maxLevel);
        }

        if (cooldownPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: cooldownPerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref cooldownPerLevel, maxLevel);
        }

        if (bombDurationPerLevel.Length != maxLevel)
        {
            Debug.LogWarning($"CoinBetting: bombDurationPerLevel �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            System.Array.Resize(ref bombDurationPerLevel, maxLevel);
        }
    }
}
