using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Ability", menuName = "Boss Transformation/Boss Ability")]
public class BossAbility : ScriptableObject
{
    public string abilityName;
    public float magicCost;
    public float cooldown;
    public KeyCode activationKey;
    public GameObject effectPrefab;
    public float damage;
    public float range;
    public Sprite abilityIcon;
    public string description;

    // Animation state for this ability
    public States animationState;
}