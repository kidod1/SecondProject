using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricBeam")]
public class ElectricBeam : SynergyAbility
{
    public GameObject beamPrefab; // 전기 광선 프리팹
    public float rotationSpeed = 30f; // 도는 속도
    public int damage = 20; // 데미지
    public float range = 600f; // 사거리

    private Player playerInstance;
    private GameObject activeBeam;
    private Coroutine rotationCoroutine;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (activeBeam == null)
        {
            activeBeam = Instantiate(beamPrefab, playerInstance.transform.position, Quaternion.identity);
            activeBeam.transform.SetParent(playerInstance.transform);

            rotationCoroutine = playerInstance.StartCoroutine(RotateBeam());
        }
    }

    private IEnumerator RotateBeam()
    {
        while (true)
        {
            if (activeBeam != null)
            {
                activeBeam.transform.RotateAround(playerInstance.transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);

                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerInstance.transform.position, range / 100f);

                foreach (Collider2D hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Monster"))
                    {
                        Monster monster = hitCollider.GetComponent<Monster>();
                        if (monster != null)
                        {
                            monster.TakeDamage(damage);
                        }
                    }
                }
            }

            yield return null;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (rotationCoroutine != null && playerInstance != null)
        {
            playerInstance.StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        if (activeBeam != null)
        {
            Destroy(activeBeam);
            activeBeam = null;
        }
    }
}
