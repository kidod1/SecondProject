using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/SwordDance")]
public class SwordDance : SynergyAbility
{
    [Header("Sword Dance Parameters")]
    public GameObject swordPrefab;          // 검 프리팹
    public int numberOfSwords = 3;          // 검의 개수
    public float abilityDuration = 5f;      // 능력 지속 시간
    public float rotationSpeed = 100f;      // 검 회전 속도
    public int damageAmount = 20;           // 데미지 양
    public float abilityCooldown = 10f;     // 이 능력의 쿨다운 시간 설정

    private Player playerInstance;
    private GameObject[] swords;            // 생성된 검 오브젝트 배열
    private bool isAbilityActive = false;

    private void OnEnable()
    {
        // 쿨다운 시간을 설정
        cooldownDuration = abilityCooldown;
    }

    public override void Apply(Player player)
    {
        if (isAbilityActive)
            return; // 이미 능력이 활성화되어 있으면 중복 실행 방지

        playerInstance = player;

        Debug.Log($"Applying {abilityName} to {playerInstance.name}");

        // 능력 활성화
        player.StartCoroutine(ActivateSwordDance());
    }

    private IEnumerator ActivateSwordDance()
    {
        isAbilityActive = true;

        // 검 생성
        swords = new GameObject[numberOfSwords];
        float angleStep = 360f / numberOfSwords;

        for (int i = 0; i < numberOfSwords; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 positionOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f; // 플레이어로부터의 거리 설정

            GameObject sword = Instantiate(swordPrefab, playerInstance.transform.position + (Vector3)positionOffset, Quaternion.identity);
            sword.transform.SetParent(playerInstance.transform); // 플레이어의 자식으로 설정

            // 검의 회전 스크립트 추가 또는 초기화
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

        // 능력 지속 시간 동안 대기
        yield return new WaitForSeconds(abilityDuration);

        // 검 제거
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
        // 업그레이드 로직을 여기에 추가할 수 있습니다.
        // 예: 데미지 증가, 지속 시간 증가 등
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
        isAbilityActive = false;
    }
}
