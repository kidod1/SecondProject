using UnityEngine;

[CreateAssetMenu(menuName = "SynergyAbilities/ElectricBeamAbility")]
public class ElectricBeamAbility : SynergyAbility
{
    public GameObject beamPrefab; // Àü±â ±¤¼± ÇÁ¸®ÆÕ

    private Player playerInstance;
    private GameObject activeBeam;

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;

        if (beamPrefab == null)
        {
            Debug.LogError("Beam prefab is not assigned.");
            return;
        }

        if (activeBeam == null && playerInstance != null)
        {
            activeBeam = Instantiate(beamPrefab, playerInstance.transform.position, Quaternion.identity);
            activeBeam.transform.SetParent(playerInstance.transform);

            ElectricBeam beamScript = activeBeam.GetComponent<ElectricBeam>();
            if (beamScript != null)
            {
                beamScript.Initialize(playerInstance.transform);
            }
            else
            {
                Debug.LogError("ElectricBeam script not found on the beam prefab.");
            }
        }
    }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
        if (activeBeam != null)
        {
            Destroy(activeBeam);
            activeBeam = null;
        }
    }
}
