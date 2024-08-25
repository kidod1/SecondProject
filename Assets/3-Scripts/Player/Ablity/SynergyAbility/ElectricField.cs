using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;

    public float damageFieldDuration = 1f; // 전기장 지속 시간
    public float damageInterval = 0.25f; // 데미지 간격
    public int damageAmount = 20; // 데미지 양
    public float cooldownDurations = 20f; // 쿨다운 시간

    private Player playerInstance;
    private GameObject activeDamageField;

    public override void Apply(Player player)
    {
        // 이전에 할당된 플레이어 인스턴스가 있다면 이벤트 제거
        if (playerInstance != null)
        {
            // 기존에 등록된 리스너 제거 로직이 불필요하므로 제거합니다.
            Debug.Log("Previous player instance was removed.");
        }

        playerInstance = player;
        Debug.Log("Electric Field ability applied to player.");

        // 프리팹을 플레이어에게 소환
        if (activeDamageField == null)
        {
            activeDamageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);
            activeDamageField.transform.SetParent(playerInstance.transform);

            DamageField damageFieldScript = activeDamageField.GetComponent<DamageField>();
            if (damageFieldScript != null)
            {
                damageFieldScript.Initialize(this, playerInstance); // 데미지 필드 초기화
                activeDamageField.SetActive(true); // 활성화 상태로 소환
            }
            else
            {
                Debug.LogError("DamageField component is missing on the prefab.");
            }
        }
    }

    public override void Upgrade()
    {
        // 업그레이드 논리를 여기에 추가할 수 있음
    }
}
