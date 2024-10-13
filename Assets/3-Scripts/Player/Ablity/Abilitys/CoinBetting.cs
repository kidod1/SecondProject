using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/CoinBetting")]
public class CoinBetting : Ability
{
    [Tooltip("��ź�� ���ط�")]
    public int damage = 50;

    [Tooltip("��ź�� �����ϴ� ��ٿ� �ð� (��)")]
    public float cooldown = 5f;

    [Tooltip("��ź�� ���� �ð� (��)")]
    public float bombDuration = 10f;

    [Tooltip("������ ���� ��ź ������")]
    public GameObject coinBombPrefab;

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
    /// �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���ط��� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            damage += 10; // ���÷� ���ط��� 10�� ������Ŵ

            // ���� �� �� ���ط� ������Ʈ
            if (playerInstance != null)
            {
                // ���� Ȱ��ȭ�� ���� ���� �ɷ��� �ִٸ� ���ط��� ������Ʈ
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
        base.ResetLevel();

        // �ڷ�ƾ ����
        if (bombCoroutine != null)
        {
            playerInstance.StopCoroutine(bombCoroutine);
            bombCoroutine = null;
        }

        // ���� ����� ���� ����
        RemoveCurrentBuff();
    }

    /// <summary>
    /// ���� ������ ������ �������� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ���������� ���ط� ������</returns>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            return 10; // ���� �������� ���ط��� 10 �����ϵ��� ����
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
            yield return new WaitForSeconds(cooldown);
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
            coinBombScript.Initialize(damage, bombDuration);
        }
        else
        {
            Debug.LogError("CoinBetting: CoinBomb ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
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
        return $"{baseDescription}\n" +
               $"���� ����: {currentLevel + 1}\n" +
               $"��ź ���ط�: {damage}\n" +
               $"��ٿ�: {cooldown}��\n" +
               $"��ź ���� �ð�: {bombDuration}��";
    }

}
