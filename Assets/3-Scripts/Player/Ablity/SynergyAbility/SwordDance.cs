using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SwordDance")]
public class SwordDance : SynergyAbility
{
    [Header("검무 파라미터")]
    [InspectorName("검 프리팹")]
    public GameObject swordPrefab;

    [InspectorName("검 개수")]
    public int numberOfSwords = 3;

    [InspectorName("능력 지속 시간")]
    public float abilityDuration = 5f;

    [InspectorName("검 회전 속도")]
    public float rotationSpeed = 100f;

    [InspectorName("데미지 양")]
    public int damageAmount = 20;

    [InspectorName("능력 쿨다운")]
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
        // 업그레이드 로직 추가 가능
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
        isAbilityActive = false;
    }
}
