using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SpotlightAttack")]
public class SpotlightAttack : Ability
{
    public GameObject spotlightPrefab;
    private SpotlightEffect spotlightInstance;

    [SerializeField]
    private int[] damageValues = { 3, 5, 7, 10, 15 };
    [SerializeField]
    private float[] rangeValues = { 2f, 2.5f, 2.5f, 2.5f, 3f };
    [SerializeField]
    private float[] intervalValues = { 1f, 1f, 1f, 0.75f, 0.75f };

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            GameObject spotlightObject = Instantiate(spotlightPrefab, player.transform);
            spotlightInstance = spotlightObject.GetComponent<SpotlightEffect>();
            if (spotlightInstance != null)
            {
                spotlightInstance.player = player;
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1;
            }
            else
            {
                Debug.LogError("스포트 라이트 컴포넌트가 존재하지 않습니다.");
            }
        }
        else
        {
            if (spotlightInstance != null)
            {
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1; 
            }
            else
            {
                Debug.LogError("스포트라이트 이펙트가 초기화되지 않았습니다.");
            }
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    public override void ResetLevel()
    {
        if (spotlightInstance != null)
        {
            Destroy(spotlightInstance.gameObject);
            spotlightInstance = null;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return damageValues[currentLevel];
        }
        return 0;
    }
}
