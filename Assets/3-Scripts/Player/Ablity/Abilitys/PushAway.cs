using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PushAway")]
public class PushAway : Ability
{
    public GameObject shockwavePrefab;
    private Shockwave shockwaveInstance;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (currentLevel == 0)
        {
            SpawnShockwave();
        }
        else
        {
            UpdateShockwave();
        }
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab != null)
        {
            GameObject shockwaveObject = Instantiate(shockwavePrefab, playerInstance.transform);
            shockwaveInstance = shockwaveObject.GetComponent<Shockwave>();
            if (shockwaveInstance != null)
            {
                shockwaveInstance.Initialize(playerInstance, currentLevel);
            }
            else
            {
                Debug.LogError("Shockwave component is missing from the prefab.");
            }
        }
        else
        {
            Debug.LogError("Shockwave prefab is null. Cannot spawn shockwave.");
        }
    }

    private void UpdateShockwave()
    {
        if (shockwaveInstance != null)
        {
            shockwaveInstance.Initialize(playerInstance, currentLevel);
        }
        else
        {
            Debug.LogError("Shockwave instance is not initialized.");
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            UpdateShockwave();
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (shockwaveInstance != null)
        {
            Destroy(shockwaveInstance.gameObject);
            shockwaveInstance = null;
        }
    }
}
