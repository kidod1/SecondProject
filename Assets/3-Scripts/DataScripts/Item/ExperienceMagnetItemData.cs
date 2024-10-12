using UnityEngine;

[CreateAssetMenu(menuName = "ItemData/ExperienceMagnetItem")]
public class ExperienceMagnetItemData : ScriptableObject
{
    [Tooltip("����ġ �������� �÷��̾�� �������� �ӵ�")]
    public float speed = 5f;

    [Tooltip("����ġ �������� �÷��̾�� �������� ���� �ð�")]
    public float duration = 2f;
}
