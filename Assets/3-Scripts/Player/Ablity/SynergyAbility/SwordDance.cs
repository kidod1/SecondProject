using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SwordDance")]
public class SwordDance : SynergyAbility
{
    [Header("Sword Dance Parameters")]
    public GameObject swordPrefab;          // �� ������
    public int numberOfSwords = 3;          // ���� ����
    public float abilityDuration = 5f;      // �ɷ� ���� �ð�
    public float rotationSpeed = 100f;      // �� ȸ�� �ӵ�
    public int damageAmount = 20;           // ������ ��
    public float abilityCooldown = 10f;     // �� �ɷ��� ��ٿ� �ð� ����

    private Player playerInstance;
    private GameObject[] swords;            // ������ �� ������Ʈ �迭
    private bool isAbilityActive = false;

    private void OnEnable()
    {
        // ��ٿ� �ð��� ����
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        if (isAbilityActive)
            return; // �̹� �ɷ��� Ȱ��ȭ�Ǿ� ������ �ߺ� ���� ����

        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // �ɷ� Ȱ��ȭ
        player.StartCoroutine(ActivateSwordDance());
    }

    private IEnumerator ActivateSwordDance()
    {
        isAbilityActive = true;

        // �� ����
        swords = new GameObject[numberOfSwords];
        float angleStep = 360f / numberOfSwords;

        for (int i = 0; i < numberOfSwords; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 positionOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f; // �÷��̾�κ����� �Ÿ� ����

            GameObject sword = Instantiate(swordPrefab, playerInstance.transform.position + (Vector3)positionOffset, Quaternion.identity);
            sword.transform.SetParent(playerInstance.transform); // �÷��̾��� �ڽ����� ����

            // ���� ȸ�� ��ũ��Ʈ �߰� �Ǵ� �ʱ�ȭ
            RotatingSword rotatingSword = sword.GetComponent<RotatingSword>();
            if (rotatingSword != null)
            {
                rotatingSword.Initialize(playerInstance.transform, rotationSpeed, damageAmount);
            }
            else
            {
                Debug.LogError("Sword prefab is missing RotatingSword component.");
            }

            swords[i] = sword;
        }

        // �ɷ� ���� �ð� ���� ���
        yield return new WaitForSeconds(abilityDuration);

        // �� ����
        for (int i = 0; i < swords.Length; i++)
        {
            if (swords[i] != null)
            {
                Destroy(swords[i]);
            }
        }

        swords = null;
        isAbilityActive = false;
    }

    public override void Upgrade()
    {
        // ���׷��̵� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
        // ��: ������ ����, ���� �ð� ���� ��
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
        isAbilityActive = false;
    }
}
