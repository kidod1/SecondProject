using UnityEngine;

public class ExperienceMagnetItem : MonoBehaviour
{
    [SerializeField]
    private ExperienceMagnetItemData itemData;

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

            // 아이템을 파괴하여 한 번만 작동하도록 합니다.
            Destroy(gameObject);
        }
    }
}
