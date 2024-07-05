using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string characterName;
    [TextArea(3, 10)]
    public string[] sentences;
    public Sprite[] backgrounds;
    public bool autoDisappear;
}
