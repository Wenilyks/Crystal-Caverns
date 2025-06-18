using UnityEngine;

[System.Serializable]
public class MapData
{
    public string mapName;
    public string mapDescription;
    public string sceneName;
    public Sprite mapBackground;
    public Sprite mapPreview;
    public Sprite background;
    public Color mapThemeColor = Color.white;
    public bool isUnlocked = true;
}