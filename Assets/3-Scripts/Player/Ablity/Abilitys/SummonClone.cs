using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    public GameObject clonePrefab;
    private GameObject cloneInstance;
    private RotatingObject rotatingObject;
    public float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f };

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            cloneInstance = Instantiate(clonePrefab, player.transform);
            rotatingObject = cloneInstance.GetComponent<RotatingObject>();
            if (rotatingObject != null)
            {
                rotatingObject.player = player.transform;
                rotatingObject.playerShooting = player;
                rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
            }
            else
            {
                Debug.LogError("클론 프리팹에 RotatingObject 컴포넌트가 없습니다.");
            }
        }
        else
        {
            if (rotatingObject != null)
            {
                rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
            }
            else
            {
                Debug.LogError("Rotating Object가 초기화되지 않았습니다.");
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
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return (int)(damageMultipliers[currentLevel] * 100);
        }
        return 0;
    }
}
