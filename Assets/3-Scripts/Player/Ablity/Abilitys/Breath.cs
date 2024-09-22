using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Breath")]
public class Breath : Ability
{
    [Header("Ability Parameters")]
    public float breathDamage = 20f;        // �극���� ���ط�
    public float cooldownTime = 10f;        // �극���� ��Ÿ��
    public float breathDuration = 2f;       // �극���� ���� �ð�
    public float breathRange = 10f;         // �극���� ��Ÿ�
    public float breathAngle = 45f;         // �극���� ���� (�翷���� 22.5����)

    public GameObject breathPrefab;         // �극�� ������

    private Player playerInstance;
    private Coroutine breathCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        // �극�� �߻� �ڷ�ƾ ����
        if (breathCoroutine == null)
        {
            breathCoroutine = playerInstance.StartCoroutine(BreathRoutine());
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // �ʱ�ȭ ���� �߰�: �극�� �ڷ�ƾ ���� �� ���� �ʱ�ȭ
        if (breathCoroutine != null)
        {
            playerInstance.StopCoroutine(breathCoroutine);
            breathCoroutine = null;
        }

        // �ʿ信 ���� �߰� �ʱ�ȭ �۾� ����
    }

    private IEnumerator BreathRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(cooldownTime);
            FireBreath();
        }
    }

    private Vector3 GetPlayerDirection()
    {
        return playerInstance.GetFacingDirection();
    }

    private void FireBreath()
    {
        if (breathPrefab == null)
        {
            Debug.LogError("�극�� �������� �������� �ʾҽ��ϴ�.");
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

            // ����� �޽��� �߰�
            Debug.Log($"[Breath Ability] �극�� ���� - ��ġ: {spawnPosition}, ����: {direction}");
        }
        else
        {
            Debug.LogError("BreathAttack ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return Mathf.RoundToInt(breathDamage + 10f);
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            breathDamage += 10f;
        }
    }
}
