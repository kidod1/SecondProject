using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/FlashBlade")]
public class FlashBlade : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("��Ʈ �Ӱ谪. �� ���� �����ϸ� Į���� �߻��մϴ�.")]
    public int hitThreshold = 5;

    [Tooltip("������ Į���� ���ط�")]
    public int[] damageLevels = { 50, 60, 70, 80, 90 }; // ���� 1~5

    [Tooltip("Į���� ��Ÿ�")]
    public float range = 10f;

    [Tooltip("�߻��� Į�� ������")]
    public GameObject bladePrefab;

    [Tooltip("Į���� �̵� �ӵ�")]
    public float bladeSpeed = 15f;

    [Header("WWISE Sound Events")]
    [Tooltip("FlashBlade Į�� �߻� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event fireSound;

    [Tooltip("FlashBlade ���׷��̵� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event upgradeSound;

    [Tooltip("FlashBlade ���� �� ����� WWISE �̺�Ʈ")]
    public AK.Wwise.Event deactivateSound;

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    /// <param name="player">�ɷ��� ������ �÷��̾�</param>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;
    }

    /// <summary>
    /// FlashBlade �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���ط��� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
        }
        else
        {
            Debug.LogWarning("FlashBlade: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// �ɷ� ������ �ʱ�ȭ�ϰ� ��Ʈ ī��Ʈ�� �����մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// �÷��̾ ���� ���߽����� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="enemy">���� ���� Collider2D</param>
    public void OnProjectileHit(Collider2D enemy)
    {
        if (enemy == null)
            return;

        hitCount++;

        if (hitCount >= hitThreshold)
        {
            hitCount = 0;
            FireBlade();
        }
    }

    /// <summary>
    /// Į���� �߻��մϴ�.
    /// </summary>
    private void FireBlade()
    {
        if (bladePrefab == null || playerInstance == null)
            return;

        Vector3 spawnPosition = playerInstance.transform.position;

        // �÷��̾��� �Ĺ� ������ ����մϴ�.
        Vector2 facingDirection = playerInstance.GetFacingDirection();
        Vector2 backwardDirection = -facingDirection;

        if (backwardDirection == Vector2.zero)
        {
            Debug.LogWarning("FlashBlade: �÷��̾��� ������ �������� �ʾҽ��ϴ�.");
            return;
        }

        // Į���� ȸ���� �Ĺ� ���⿡ �°� �����մϴ�.
        float angle = Mathf.Atan2(backwardDirection.y, backwardDirection.x) * Mathf.Rad2Deg;

        // �������� �⺻ ���⿡ ���� ���� ���� (�ʿ� ��)
        float angleOffset = -90f; // �ʿ信 ���� ���� (��: 90f)
        angle += angleOffset;

        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);

        GameObject blade = Instantiate(bladePrefab, spawnPosition, spawnRotation);
        BladeProjectile bladeScript = blade.GetComponent<BladeProjectile>();

        if (bladeScript != null)
        {
            int currentDamage = GetCurrentDamage();
            bladeScript.Initialize(currentDamage, range, bladeSpeed, playerInstance, backwardDirection);
        }
        else
        {
            Debug.LogError("FlashBlade: bladePrefab�� BladeProjectile ������Ʈ�� �����ϴ�.");
        }

        // Debugging: ���� Ȯ���� ���� �α� �߰�
        Debug.Log($"Firing blade at angle: {angle}, Direction: {backwardDirection}, Spawn Position: {spawnPosition}");

        // FlashBlade Į�� �߻� �� WWISE ���� ���
        if (fireSound != null)
        {
            fireSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// �ɷ��� ������ ��ȯ�մϴ�. ������ ���� �������� 1���� �� ���� ������ ������ �����մϴ�.
    /// </summary>
    /// <returns>�ɷ� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < maxLevel)
        {
            // ���� ������ �ε����� currentLevel (currentLevel�� 0���� ����)
            int nextLevelIndex = currentLevel;
            int nextLevelDamage = (nextLevelIndex < damageLevels.Length) ? damageLevels[nextLevelIndex] : damageLevels[damageLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Lv {currentLevel + 1}:\n" +
                   $"- ��Ʈ �Ӱ谪: {hitThreshold}ȸ\n" +
                   $"- ���ط�: {nextLevelDamage}\n" +
                   $"- ��Ÿ�: {range}m\n";
        }
        else
        {
            // �ִ� ���� ����
            int maxLevelIndex = currentLevel - 1;
            int finalDamage = (maxLevelIndex < damageLevels.Length) ? damageLevels[maxLevelIndex] : damageLevels[damageLevels.Length - 1];

            return $"{baseDescription}\n" +
                   $"Max Level: {currentLevel}\n" +
                   $"- ��Ʈ �Ӱ谪: {hitThreshold}ȸ\n" +
                   $"- ���ط�: {finalDamage}\n" +
                   $"- ��Ÿ�: {range}m\n";
        }
    }

    /// <summary>
    /// ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���ط�</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel == 0)
        {
            return damageLevels[0];
        }
        else if (currentLevel - 1 < damageLevels.Length)
        {
            return damageLevels[currentLevel - 1];
        }
        Debug.LogWarning($"FlashBlade: currentLevel ({currentLevel})�� damageLevels �迭�� ������ ������ϴ�. ������ ���� ��ȯ�մϴ�.");
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// ���� ���� ������ �ʿ��� ���� ��ȯ�մϴ�.
    /// (�� �޼���� �� �̻� ������ �����Ƿ� ������ �� �ֽ��ϴ�.)
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        // �� �̻� ������ �����Ƿ� 0�� ��ȯ�ϰų� �޼��带 ������ �� �ֽ��ϴ�.
        return 0;
    }

    /// <summary>
    /// OnValidate �޼��带 ���� �迭�� ���̸� maxLevel�� ��ġ��ŵ�ϴ�.
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Debug.LogWarning($"FlashBlade: damageLevels �迭�� ���̰� maxLevel ({maxLevel})�� ��ġ���� �ʽ��ϴ�. �迭 ���̸� ����ϴ�.");
            Array.Resize(ref damageLevels, maxLevel);
        }
    }

    /// <summary>
    /// Gizmos�� ����Ͽ� FlashBlade �߻� ���� �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (playerInstance != null)
        {
            Vector2 facingDirection = playerInstance.GetFacingDirection();
            Vector2 backwardDirection = -facingDirection;

            Vector3 origin = playerInstance.transform.position;
            Vector3 direction = backwardDirection * 5f; // ����: 5 ���� ����

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + (Vector3)direction);
            Gizmos.DrawSphere(origin + (Vector3)direction, 0.2f);
        }
    }
}
