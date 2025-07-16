using UnityEngine;

[CreateAssetMenu(fileName = "ShadowBossTransformation", menuName = "Boss Transformation/Shadow Boss Transformation")]
public class ShadowBossTransformation : BossTransformation
{
    [Header("Shadow Boss Specific Settings")]
    public GameObject shadowBoltPrefab;
    public GameObject shadowWavePrefab;
    public float shadowStrikeRange = 3f;
    public float shadowBoltRange = 8f;
    public float teleportRange = 8f;
    public ParticleSystem teleportEffect;

    private void OnEnable()
    {
        bossName = "Shadow Master";
        transformationDuration = 45f;
        magicCostPerSecond = 3f;
        transformationColor = new Color(0.4f, 0.1f, 0.6f, 1f); 

        moveSpeedMultiplier = 1.4f;
        healthMultiplier = 1.3f;
        magicMultiplier = 1.5f;

        abilities.Clear();

        var shadowBolt = CreateShadowBoltAbility();
        abilities.Add(shadowBolt);

        var shadowWave = CreateShadowWaveAbility();
        abilities.Add(shadowWave);

        var shadowStrike = CreateShadowStrikeAbility();
        abilities.Add(shadowStrike);

        var teleport = CreateTeleportAbility();
        abilities.Add(teleport);
    }

    private BossAbility CreateShadowBoltAbility()
    {
        var ability = ScriptableObject.CreateInstance<BossAbility>();
        ability.abilityName = "Shadow Bolt";
        ability.magicCost = 15f;
        ability.cooldown = 2f;
        ability.activationKey = KeyCode.Z;
        ability.damage = 18f;
        ability.range = shadowBoltRange;
        ability.animationState = States.attackTwo;
        ability.description = "Fires a dark energy projectile";
        return ability;
    }

    private BossAbility CreateShadowWaveAbility()
    {
        var ability = ScriptableObject.CreateInstance<BossAbility>();
        ability.abilityName = "Shadow Wave";
        ability.magicCost = 35f;
        ability.cooldown = 8f;
        ability.activationKey = KeyCode.X;
        ability.damage = 12f;
        ability.range = 10f;
        ability.animationState = States.hit;
        ability.description = "Creates expanding waves of dark energy";
        return ability;
    }

    private BossAbility CreateShadowStrikeAbility()
    {
        var ability = ScriptableObject.CreateInstance<BossAbility>();
        ability.abilityName = "Shadow Strike";
        ability.magicCost = 20f;
        ability.cooldown = 3f;
        ability.activationKey = KeyCode.C;
        ability.damage = 25f;
        ability.range = shadowStrikeRange;
        ability.animationState = States.attackOne;
        ability.description = "Devastating close-range shadow attack";
        return ability;
    }

    private BossAbility CreateTeleportAbility()
    {
        var ability = ScriptableObject.CreateInstance<BossAbility>();
        ability.abilityName = "Shadow Teleport";
        ability.magicCost = 25f;
        ability.cooldown = 10f;
        ability.activationKey = KeyCode.V;
        ability.damage = 0f;
        ability.range = teleportRange;
        ability.animationState = States.jump; 
        ability.description = "Teleport through shadows";
        return ability;
    }
}