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

    private Player playerInstance;
    private int hitCount = 0;

    /// <summary>
    /// ���� ������ �ش��ϴ� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < damageLevels.Length)
        {
            return Mathf.RoundToInt(damageLevels[currentLevel]);
        }
        return 0;
    }

    /// <summary>
    /// FlashBlade �ɷ��� �÷��̾�� �����մϴ�.
    /// </summary>
    public override void Apply(Player player)
    {
        if (player == null)
            return;

        playerInstance = player;
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

        // Debugging: ���� Ȯ���� ���� �α� �߰�
        Debug.Log($"Firing blade at angle: {angle}, Direction: {backwardDirection}, Spawn Position: {spawnPosition}");

        // FlashBlade Į�� �߻� �� WWISE ���� ���
        if (fireSound != null)
        {
            fireSound.Post(playerInstance.gameObject);
        }
    }

    /// <summary>
    /// FlashBlade �ɷ��� ���׷��̵��մϴ�. ������ ������ ������ ���ط��� �����մϴ�.
    /// </summary>
    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            Debug.Log($"FlashBlade ���׷��̵�: ���� ���� {currentLevel + 1}");
        }
        else
        {
            Debug.LogWarning("FlashBlade: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ �´� ���ط��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ���ط�</returns>
    private int GetCurrentDamage()
    {
        if (currentLevel < damageLevels.Length)
        {
            return damageLevels[currentLevel];
        }
        return damageLevels[damageLevels.Length - 1];
    }

    /// <summary>
    /// �ɷ� ������ �������̵��Ͽ� ���� �� �� ���ط� ������ ���Խ�ŵ�ϴ�.
    /// </summary>
    /// <returns>�ɷ��� ���� ���ڿ�</returns>
    public override string GetDescription()
    {
        if (currentLevel < damageLevels.Length && currentLevel >= 0)
        {
            int damageValue = damageLevels[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {hitThreshold}ȸ ���� ������ �Ĺ����� Į�� �߻�. ���ط�: {damageValue}";
        }
        else if (currentLevel >= damageLevels.Length)
        {
            int maxDamageValue = damageLevels[damageLevels.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� {hitThreshold}ȸ ���� ������ �Ĺ����� Į�� �߻�. ���ط�: {maxDamageValue}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }

    /// <summary>
    /// �ɷ��� ������ �ʱ�ȭ�ϰ� ��Ʈ ī��Ʈ�� �����մϴ�.
    /// </summary>
    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;
        currentLevel = 0;
    }

    /// <summary>
    /// Editor �󿡼� ��ȿ�� �˻�
    /// </summary>
    private void OnValidate()
    {
        if (damageLevels.Length != maxLevel)
        {
            Array.Resize(ref damageLevels, maxLevel);
        }
    }

    /// <summary>
    /// Gizmos�� ����Ͽ� Į�� �߻� ���� �ð�ȭ
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
