using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ElectricField")]
public class ElectricField : SynergyAbility
{
    public GameObject damageFieldPrefab;
    public float damageFieldDuration = 1f;
    public float damageInterval = 0.25f;
    public int damageAmount = 10;
    public float effectRadius = 5f;

    private Player playerInstance;
    private Coroutine damageCoroutine;

    public override void Apply(Player player)
    {
        if (playerInstance != null)
        {
            playerInstance.OnMonsterEnter.RemoveListener(OnMonsterEnter);
        }

        playerInstance = player;
        playerInstance.OnMonsterEnter.AddListener(OnMonsterEnter);
    }

    private void OnMonsterEnter(Collider2D collider)
    {
        if (collider.CompareTag("Monster"))
        {
            if (damageCoroutine != null)
            {
                playerInstance.StopCoroutine(damageCoroutine);
            }
            damageCoroutine = playerInstance.StartCoroutine(SpawnDamageField());
        }
    }

    private IEnumerator SpawnDamageField()
    {
        GameObject damageField = Instantiate(damageFieldPrefab, playerInstance.transform.position, Quaternion.identity);
        damageField.transform.SetParent(playerInstance.transform);
        DamageField damageFieldScript = damageField.GetComponent<DamageField>();
        damageFieldScript.Initialize(damageAmount, damageInterval);
        yield return new WaitForSeconds(damageFieldDuration);
        Destroy(damageField);
    }

    public override void Upgrade()
    {
        // 시너지 능력은 업그레이드 없음
    }
}
