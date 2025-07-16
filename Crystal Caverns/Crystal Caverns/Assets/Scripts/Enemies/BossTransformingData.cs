using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Transformation", menuName = "Boss Transformation/Boss Transformation")]
public class BossTransformation : ScriptableObject
{
    public string bossName;
    public Sprite bossSprite;
    public RuntimeAnimatorController bossAnimator;
    public List<BossAbility> abilities = new List<BossAbility>();
    public float transformationDuration = 30f;
    public float magicCostPerSecond = 2f;
    public Color transformationColor = Color.white;
    public GameObject transformationEffect;

    // Boss specific stats
    public float moveSpeedMultiplier = 1.2f;
    public float healthMultiplier = 1.5f;
    public float magicMultiplier = 1.3f;
}