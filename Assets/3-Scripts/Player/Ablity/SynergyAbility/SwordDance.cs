using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SwordDance")]
public class SwordDance : SynergyAbility
{
    [Header("�˹� �Ķ����")]
    [InspectorName("�� ������")]
    public GameObject swordPrefab;

    [InspectorName("�� ����")]
    public int numberOfSwords = 3;

    [InspectorName("�ɷ� ���� �ð�")]
    public float abilityDuration = 5f;

    [InspectorName("�� ȸ�� �ӵ�")]
    public float rotationSpeed = 100f;

    [InspectorName("������ ��")]
    public int damageAmount = 20;

    [InspectorName("�ɷ� ��ٿ�")]
    public float abilityCooldown = 10f;

    private Player playerInstance;
    private GameObject[] swords;
    private bool isAbilityActive = false;

    private void OnEnable()
    {
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        if (isAbilityActive)
            return;

        playerInstance = player;
        player.StartCoroutine(ActivateSwordDance());
    }

    private IEnumerator ActivateSwordDance()
    {
        isAbilityActive = true;
        swords = new GameObject[numberOfSwords];
        float angleStep = 360f / numberOfSwords;

        for (int i = 0; i < numberOfSwords; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 positionOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
            GameObject sword = Instantiate(swordPrefab, playerInstance.transform.position + (Vector3)positionOffset, Quaternion.identity);
            sword.transform.SetParent(playerInstance.transform);

            RotatingSword rotatingSword = sword.GetComponent<RotatingSword>();
            if (rotatingSword != null)
            {
                rotatingSword.Initialize(playerInstance.transform, rotationSpeed, damageAmount);
            }

            swords[i] = sword;
        }

        yield return new WaitForSeconds(abilityDuration);

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
        // ���׷��̵� ���� �߰� ����
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
        isAbilityActive = false;
    }
}
