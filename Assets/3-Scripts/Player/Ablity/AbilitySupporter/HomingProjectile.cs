using UnityEngine;

public class HomingProjectile : Projectile
{
    private float homingStartDelay;
    private float homingSpeed;
    private float homingRange; // ���� ����
    private bool isHomingActive = false;

    private void Start()
    {
        Debug.Log("Homing activated!");
        Invoke(nameof(ActivateHoming), homingStartDelay);
    }

    public void Initialize(PlayerData playerStat, float startDelay, float speed, float range)
    {
        base.Initialize(playerStat);
        homingStartDelay = startDelay;
        homingSpeed = speed;
        homingRange = range;
    }

    private void ActivateHoming()
    {
        isHomingActive = true;
    }

    private void Update()
    {
        if (isHomingActive)
        {
            Monster targetMonster = FindClosestMonsterWithinRange();
            if (targetMonster != null)
            {
                Vector2 directionToMonster = (targetMonster.transform.position - transform.position).normalized;
                Debug.Log($"����ü ����: {directionToMonster} / ��ġ: {transform.position}");
                rb.velocity = Vector2.Lerp(rb.velocity, directionToMonster * homingSpeed, Time.deltaTime * 2);
            }
        }
    }

    private Monster FindClosestMonsterWithinRange()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster"); 
        Monster closestMonster = null;
        float closestDistance = homingRange;

        foreach (GameObject monsterObj in monsters)
        {
            Monster monster = monsterObj.GetComponent<Monster>();
            float distanceToMonster = Vector2.Distance(transform.position, monster.transform.position);
            if (distanceToMonster <= homingRange && distanceToMonster < closestDistance)
            {
                closestMonster = monster;
                closestDistance = distanceToMonster;
            }
        }

        return closestMonster;
    }

    // ������ ���� ������ �׷��ִ� �޼���
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, homingRange);
    }
}
