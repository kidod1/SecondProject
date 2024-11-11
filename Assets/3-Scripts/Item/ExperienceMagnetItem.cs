using UnityEngine;
using AK.Wwise; // Wwise 네임스페이스 추가

public class ExperienceMagnetItem : MonoBehaviour
{
    [SerializeField]
    private ExperienceMagnetItemData itemData;

    // 추가된 부분: Wwise 이벤트 참조
    [SerializeField]
    private AK.Wwise.Event pickupSoundEvent; // Wwise 이벤트 할당을 위해 추가

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 Transform을 저장
            Transform playerTransform = other.transform;

            // 씬 내의 모든 경험치 아이템을 찾습니다.
            ExperienceItem[] experienceItems = FindObjectsOfType<ExperienceItem>();

            foreach (ExperienceItem expItem in experienceItems)
            {
                // 각 경험치 아이템에 플레이어에게 끌려오도록 명령
                expItem.StartAttractingToPlayer(playerTransform, itemData.speed, itemData.duration);
            }

            // Wwise 이벤트 호출: 아이템 픽업 시 사운드 재생
            if (pickupSoundEvent != null)
            {
                // 게임 오브젝트의 위치에서 사운드를 재생
                pickupSoundEvent.Post(gameObject);
            }
            else
            {
                Debug.LogWarning("Pickup Sound Event가 할당되지 않았습니다.");
            }

            // 아이템을 파괴하여 한 번만 작동하도록 합니다.
            Destroy(gameObject);
        }
    }
}
