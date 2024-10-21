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
    public float breathAngle = 45f;         // �극���� ���� (��ü �극�� ����)

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
            return $"{baseDescription}\n" +
                   $"���� ����: {currentLevel + 1}\n" +
                   $"�극�� ���� ������ ����: +{damageIncrease}\n" +
                   $"�극�� ���� �ð�: {breathDuration}��\n" +
                   $"�극�� ��Ÿ��: {cooldownTime}��";
        }
        else
        {
            int finalDamageIncrease = GetNextLevelIncrease();
            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel + 1}\n" +
                   $"�극�� ���� ������ ����: +{finalDamageIncrease}\n" +
                   $"�극�� ���� ����: {breathRange}m\n" +
                   $"�극�� ���� ����: {breathAngle}��\n" +
                   $"�극�� ���� �ð�: {breathDuration}��\n" +
                   $"�극�� ��Ÿ��: {cooldownTime}��";
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
    /// �극���� �÷��̾ �ٶ󺸴� �������� �߻��մϴ�. ������ 0��, 90��, 180��, 270�� �� ���� ����� �������� ���ε˴ϴ�.
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
        Vector2 baseDirection = GetPlayerDirection();

        if (baseDirection == Vector2.zero)
        {
            Debug.LogWarning("Breath: �÷��̾��� ������ �������� �ʾҽ��ϴ�.");
            return;
        }

        // �÷��̾ ���� �ִ� ������ ���� ��� (������ �������� �ð� �ݴ� ����)
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // ������ ���� ����� 90���� ���� (0, 90, 180, 270)
        float mappedAngle = RoundToNearest90(baseAngle);

        if (mappedAngle == 90)
        {
            mappedAngle = 270;
        }
        else if (mappedAngle == 270)
        {
            mappedAngle = 90;
        }
        // ���ε� ���� ���� ���
        Vector2 mappedDirection = new Vector2(Mathf.Cos(mappedAngle * Mathf.Deg2Rad), Mathf.Sin(mappedAngle * Mathf.Deg2Rad)).normalized;

        if (mappedDirection.y == 1)
        {
            mappedDirection.y = -1;
        }
        else if (mappedDirection.y == -1)
        {
            mappedDirection.y = 1;
        }

        // �극�� �ν��Ͻ� ���� �� ȸ�� ����
        GameObject breath = Instantiate(breathPrefab, spawnPosition, Quaternion.Euler(0, 0, mappedAngle));
        BreathAttack breathAttackScript = breath.GetComponent<BreathAttack>();

        if (breathAttackScript != null)
        {
            breathAttackScript.Initialize(breathDamage, breathRange, breathAngle, breathDuration, playerInstance);

            // Breath�� ������ �����մϴ�.
            breathAttackScript.SetDirection(mappedDirection);
        }
        else
        {
            Debug.LogError("BreathAttack ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }

        // �극�� ������Ʈ�� �������� �����Ͽ� �÷��̾��� ������ ��ȯ�� ������ ��ġ�� �ʵ��� �մϴ�.
        breath.transform.localScale = Vector3.one;

        // �ڽ� ParticleSystem�� Start Rotation ����
        ParticleSystem ps = breath.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startRotation = (mappedAngle + 90f) * Mathf.Deg2Rad; // ParticleSystem�� ���� ������ ȸ��
        }
        else
        {
            Debug.LogWarning("Breath �������� �ڽĿ� ParticleSystem�� �����ϴ�.");
        }

        // �극�� ������Ʈ�� ȸ���� �����Ͽ� ParticleSystem�� �ùٸ��� ȸ���ϵ��� �մϴ�.
        breath.transform.rotation = Quaternion.Euler(0, 0, mappedAngle);
    }


    /// <summary>
    /// �־��� ������ ���� ����� 90���� �ݿø��մϴ�.
    /// </summary>
    /// <param name="angle">�ݿø��� ���� (�� ����)</param>
    /// <returns>���� ����� 90���� ���</returns>
    private float RoundToNearest90(float angle)
    {
        // 0~360�� ������ ���� ����
        angle = Mathf.Repeat(angle, 360f);

        // ������ 45���� ���� ��, �ݿø��Ͽ� ���� ����� 90�� ����� ����
        float correctionAngle = Mathf.Round(angle / 90f) * 90f;
        return correctionAngle;

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
