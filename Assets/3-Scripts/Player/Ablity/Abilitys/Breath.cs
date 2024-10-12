using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Abilities/Breath")]
public class Breath : Ability
{
    [Header("Ability Parameters")]
    public float breathDamage = 20f;        // �극���� �⺻ ���ط�
    public float cooldownTime = 10f;        // �극���� �⺻ ��Ÿ��
    public float breathDuration = 2f;       // �극���� ���� �ð�
    public float breathRange = 10f;         // �극���� ��Ÿ�
    public float breathAngle = 45f;         // �극���� ���� (�翷���� 22.5����)

    [Tooltip("�극�� �ɷ� ���׷��̵� �� ������ ������ ������")]
    public int[] damageIncrements = { 10, 15, 20, 25, 30 }; // ���� 1~5

    public GameObject breathPrefab;         // �극�� ������

    private Player playerInstance;
    private Coroutine breathCoroutine;

    /// <summary>
    /// ���� ������ �´� ������ �������� ��ȯ�մϴ�.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageIncrements.Length)
        {
            return damageIncrements[currentLevel];
        }
        Debug.LogWarning($"Breath: currentLevel ({currentLevel})�� damageIncrements �迭�� ������ ������ϴ�. �⺻�� 0�� ��ȯ�մϴ�.");
        return 0;
    }

    /// <summary>
    /// �극�� �ɷ��� �÷��̾�� �����մϴ�. �ʱ� ���������� �극�� �߻縦 �����մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        playerInstance = player;

        // �극�� �߻� �ڷ�ƾ ����
        if (breathCoroutine == null)
        {
            breathCoroutine = playerInstance.StartCoroutine(BreathRoutine());
        }
    }

    /// <summary>
    /// �극�� �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ �������� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"Breath ���׷��̵�: ���� ���� {currentLevel}");

            // ���׷��̵� �� ������ ���� ����
            breathDamage += GetNextLevelIncrease();
        }
        else
        {
            Debug.LogWarning("Breath: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �������̵��Ͽ� ���� �� �� ������ �������� ���Խ�ŵ�ϴ�.
    /// </summary>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            int damageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}{System.Environment.NewLine}(Level {currentLevel + 1}: +{damageIncrease} ������)";
        }
        else
        {
            int finalDamageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}{System.Environment.NewLine}(Max Level: +{finalDamageIncrease} ������)";
        }
    }

    /// <summary>
    /// �극�� �߻� �ڷ�ƾ�Դϴ�. ��Ÿ�Ӹ��� �극���� �߻��մϴ�.
    /// </summary>
    private IEnumerator BreathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldownTime);
            FireBreath();
        }
    }

    /// <summary>
    /// �÷��̾��� ���� ������ ��ȯ�մϴ�.
    /// </summary>
    private Vector3 GetPlayerDirection()
    {
        return playerInstance.GetFacingDirection();
    }

    /// <summary>
    /// �극���� �߻��մϴ�.
    /// </summary>
    private void FireBreath()
    {
        if (breathPrefab == null)
        {
            Debug.LogError("Breath �������� �������� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        // �÷��̾��� ���� ������ �����ɴϴ�.
        Vector2 direction = GetPlayerDirection();

        // Breath ������ ����
        GameObject breath = Instantiate(breathPrefab, spawnPosition, Quaternion.identity);
        BreathAttack breathAttackScript = breath.GetComponent<BreathAttack>();

        if (breathAttackScript != null)
        {
            breathAttackScript.Initialize(breathDamage, breathRange, breathAngle, breathDuration, playerInstance);

            // Breath�� ������ �����մϴ�.
            breathAttackScript.SetDirection(direction);
        }
        else
        {
            Debug.LogError("BreathAttack ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ �� ȣ��˴ϴ�. �극�� �ڷ�ƾ�� �����ϰ� �������� �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();

        // �극�� �ڷ�ƾ ���� �� ���� �ʱ�ȭ
        if (breathCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(breathCoroutine);
            breathCoroutine = null;
        }

        currentLevel = 0;
    }
    private void OnValidate()
    {
        if (damageIncrements.Length != maxLevel)
        {
            Debug.LogWarning($"Breath: damageIncrements �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            Array.Resize(ref damageIncrements, maxLevel);
        }
    }
}