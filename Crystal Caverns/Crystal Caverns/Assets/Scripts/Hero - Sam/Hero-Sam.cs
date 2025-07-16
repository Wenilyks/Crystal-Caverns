using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum States
{
    idle,
    run,
    jump,
    death,
    hit,
    attackOne,
    attackTwo,
    attackThree
}

public class Hero2 : MonoBehaviour, IDamageable
{
    [Header("Player stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxMagicAura = 100f;
    [SerializeField] private float currentMagicAura = 100f;
    [SerializeField] private float magicRegenRate = 5f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lives = 5;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image magicAuraBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text magicAuraText;

    [Header("Animation settings")]
    [SerializeField] private float barAnimationSpeed = 0.1f;
    [SerializeField] private float barAnimationDuration = 0.5f;

    private float targetHealthValue = 1f;
    private float targetMagicValue = 1f;


    [Header("Combat")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject fireballPrefab2;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float attackCooldown = 0.7f;
    [SerializeField] private int rainFireballCount = 5;
    [SerializeField] private float rainSpread = 5f;
    [SerializeField] private float rainHeight = 3f;
    [SerializeField] private float groundPoundRadius = 3f;
    [SerializeField] private int groundPoundDamage = 25;
    [SerializeField] private float fireballMagicCost = 20f;
    [SerializeField] private float rainFireballMagicCost = 25f;
    [SerializeField] private float groundPoundMagicCost = 25f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Transformation")]
    [SerializeField] private List<BossTransformation> unlockedTransformations = new List<BossTransformation>();
    [SerializeField] private KeyCode transformationKey = KeyCode.T;
    [SerializeField] private int currentTransformationIndex = 0;
    [SerializeField] private Transform transformationUI;
    [SerializeField] private Image transformationIcon;
    [SerializeField] private TMP_Text transformationText;
    [SerializeField] private Image transformationCooldownOverlay;

    [Header("Effects")]
    [SerializeField] private GameObject groundPoundEffect;
    [SerializeField] private ParticleSystem magicAura;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteHolder2;
    [SerializeField] private Transform groundCheck;

    [Header("Inventory")]
    [SerializeField] private UI_Inventory UI_Inventory;
    [SerializeField] private Transform positionToDrop;

    private Inventory inventory;

    private bool isGrounded = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isGettingHit = false;

    private bool isTransformed = false;
    private BossTransformation currentTransformation;
    private float transformationTimer = 0f;
    private float transformationCooldown = 0f;
    private float transformationCooldownTime = 60f;

    private float originalMoveSpeed;
    private float originalMaxHealth;
    private float originalMaxMagicAura;
    private RuntimeAnimatorController originalAnimator;
    private Vector3 originalScale;

    private Dictionary<KeyCode, BossAbility> transformationAbilities = new Dictionary<KeyCode, BossAbility>();
    private Dictionary<BossAbility, float> abilityCooldowns = new Dictionary<BossAbility, float>();

    private Rigidbody2D rb;
    private Animator anim;
    public static Hero2 Instance { get; set; }

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }

    private void Awake()
    {
        Instance = this;
        inventory = new Inventory();
        rb = GetComponent<Rigidbody2D>();
        anim = spriteHolder2.GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemWorld itemWorld = collision.GetComponent<ItemWorld>();
        if (itemWorld != null)
        {
            inventory.AddItem(itemWorld.GetItem());
            itemWorld.DestroySelf();
        }

        Chest chest = collision.GetComponent<Chest>();
        if (chest != null)
        {
            chest.OpenChest();
        }
    }

    private void Start()
    {
        UI_Inventory.SetInventory(inventory);
        if (healthBar != null) healthBar.fillAmount = 1f;
        if (magicAuraBar != null) magicAuraBar.fillAmount = 1f;
        UpdateTexts();

        originalMoveSpeed = speed;
        originalMaxHealth = maxHealth;
        originalMaxMagicAura = maxMagicAura;
        originalAnimator = anim.runtimeAnimatorController;
        originalScale = spriteHolder2.localScale;

        UpdateTransformationUI();
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            UI_Inventory.gameObject.SetActive(!UI_Inventory.gameObject.activeSelf);
            if (UI_Inventory.gameObject.activeSelf)
            {
                UI_Inventory.RefreshInventoryItems();
            }
        }

        if (UI_Inventory.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Q) && UI_Inventory.GetSelectedItem() != null){
            Item itemToDrop = UI_Inventory.GetSelectedItem();
            inventory.RemoveItem(itemToDrop.itemType, 1);
            var dropItem = new Item()
            {
                itemType = itemToDrop.itemType,
                amount = 1
            };
            ItemWorld.SpawnItemWorld(positionToDrop.position, dropItem, dropItem.amount);
        }

        if (UI_Inventory.gameObject.activeSelf && Input.GetKeyDown(KeyCode.R) && UI_Inventory.GetSelectedItem() != null)
        {
            UseItem(UI_Inventory.GetSelectedItem());
        }

        if (isGrounded && !isAttacking && !isGettingHit)
        {
            Debug.Log("Idle state lol");
            State = States.idle;
        }
        if (!isAttacking)
        {
            if (Input.GetButton("Horizontal"))
                Run();

            if (isGrounded && Input.GetButtonDown("Jump"))
                Jump();
        }

        if (currentMagicAura < maxMagicAura)
        {
            currentMagicAura += magicRegenRate * Time.deltaTime;
            currentMagicAura = Mathf.Clamp(currentMagicAura, 0, maxMagicAura);
            targetMagicValue = currentMagicAura / maxMagicAura;
        }

        if (healthBar != null)
        {
            healthBar.DOFillAmount(targetHealthValue, barAnimationDuration);
        }

        if (magicAuraBar != null)
        {
            magicAuraBar.DOFillAmount(targetMagicValue, barAnimationDuration);  
        }

        if (canAttack && !isTransformed)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                State = States.idle;
                if (!(currentMagicAura >= fireballMagicCost)) return;
                AudioManager.Instance.PlaySFX("Wizard attack 1");
                isAttacking = true;
                canAttack = false;
                State = States.attackTwo;
                StartCoroutine(PerformAttack(States.attackTwo));
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (!(currentMagicAura >= rainFireballMagicCost)) return;
                isAttacking = true;
                canAttack = false;
                State = States.attackOne;
                StartCoroutine (PerformAttack(States.attackOne));
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (!(currentMagicAura >= groundPoundMagicCost)) return;
                AudioManager.Instance.PlaySFX("Ground pound");
                isAttacking = true;
                canAttack = false;
                State = States.attackThree;
                StartCoroutine(PerformAttack(States.attackThree));
            }
        }
        UpdateTexts();

        HandleTransformation();
        UpdateTransformationTimer();
        UpdateAbilityCooldowns();

        UpdateTransformationUI();
    }

    private void HandleTransformation()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTransformed)
        {
            CycleTransformation();
        }

        if (Input.GetKeyDown(transformationKey))
        {
            if (!isTransformed && transformationCooldown <= 0)
            {
                if (unlockedTransformations.Count > 0 && currentTransformationIndex < unlockedTransformations.Count)
                {
                    Transform();
                }
            }
            else if (isTransformed)
            {
                Detransform();
            }
        }

        if (isTransformed && currentTransformation != null)
        {
            foreach (var ability in transformationAbilities)
            {
                if (Input.GetKeyDown(ability.Key))
                {
                    UseTransformationAbility(ability.Value);
                }
            }
        }
    }

    private void CycleTransformation()
    {
        if (unlockedTransformations.Count == 0) return;
        
        currentTransformationIndex = (currentTransformationIndex + 1) % unlockedTransformations.Count;
        UpdateTransformationUI();
    }
    
    private void Transform()
    {
        if (unlockedTransformations.Count == 0) return;
        
        currentTransformation = unlockedTransformations[currentTransformationIndex];
        isTransformed = true;
        transformationTimer = currentTransformation.transformationDuration;
        
        ApplyTransformationStats();
        ApplyTransformationVisuals();
        SetupTransformationAbilities();
        
        if (currentTransformation.transformationEffect != null)
        {
            GameObject effect = Instantiate(currentTransformation.transformationEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        AudioManager.Instance.PlaySFX("Transform");
        
        Debug.Log($"Transformed into {currentTransformation.bossName}!");
    }
    
    private void Detransform()
    {
        if (!isTransformed) return;
        
        isTransformed = false;
        transformationCooldown = transformationCooldownTime;
        
        RestoreOriginalStats();
        RestoreOriginalVisuals();
        ClearTransformationAbilities();
        
        AudioManager.Instance.PlaySFX("Detransform");
        
        Debug.Log("Detransformed back to normal!");
    }
    
    private void ApplyTransformationStats()
    {
        speed = originalMoveSpeed * currentTransformation.moveSpeedMultiplier;
        maxHealth = originalMaxHealth * currentTransformation.healthMultiplier;
        maxMagicAura = originalMaxMagicAura * currentTransformation.magicMultiplier;
        
        currentHealth = Mathf.Min(currentHealth + (maxHealth * 0.2f), maxHealth);
        currentMagicAura = Mathf.Min(currentMagicAura + (maxMagicAura * 0.3f), maxMagicAura);
        
        targetHealthValue = currentHealth / maxHealth;
        targetMagicValue = currentMagicAura / maxMagicAura;
    }
    
    private void ApplyTransformationVisuals()
    {
        if (currentTransformation.bossAnimator != null)
        {
            anim.runtimeAnimatorController = currentTransformation.bossAnimator;
            if (currentTransformation.bossAnimator.name == "boss 2 controller")
                spriteHolder2.GetComponent<SpriteRenderer>().flipX = true;
        }
        
        SpriteRenderer spriteRenderer = spriteHolder2.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentTransformation.transformationColor;
        }
        
        spriteHolder2.localScale = originalScale * 1.1f;
    }
    
    private void SetupTransformationAbilities()
    {
        transformationAbilities.Clear();
        abilityCooldowns.Clear();
        
        foreach (var ability in currentTransformation.abilities)
        {
            transformationAbilities[ability.activationKey] = ability;
            abilityCooldowns[ability] = 0f;
        }
    }
    
    private void RestoreOriginalStats()
    {
        speed = originalMoveSpeed;
        maxHealth = originalMaxHealth;
        maxMagicAura = originalMaxMagicAura;
        
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentMagicAura = Mathf.Min(currentMagicAura, maxMagicAura);
        
        targetHealthValue = currentHealth / maxHealth;
        targetMagicValue = currentMagicAura / maxMagicAura;
    }
    
    private void RestoreOriginalVisuals()
    {
        if (currentTransformation.bossAnimator.name == "boss 2 controller")
            spriteHolder2.GetComponent<SpriteRenderer>().flipX = false;
        anim.runtimeAnimatorController = originalAnimator;
        
        SpriteRenderer spriteRenderer = spriteHolder2.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        spriteHolder2.localScale = originalScale;
    }
    
    private void ClearTransformationAbilities()
    {
        transformationAbilities.Clear();
        abilityCooldowns.Clear();
    }
    
    private void UpdateTransformationTimer()
    {
        if (isTransformed)
        {
            transformationTimer -= Time.deltaTime;
            
            currentMagicAura -= currentTransformation.magicCostPerSecond * Time.deltaTime;
            targetMagicValue = currentMagicAura / maxMagicAura;
            
            if (transformationTimer <= 0 || currentMagicAura <= 0)
            {
                Detransform();
            }
        }
        
        if (transformationCooldown > 0)
        {
            transformationCooldown -= Time.deltaTime;
        }
    }
    
    private void UpdateAbilityCooldowns()
    {
        var keys = new List<BossAbility>(abilityCooldowns.Keys);
        foreach (var ability in keys)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] -= Time.deltaTime;
            }
        }
    }
    
    private void UseTransformationAbility(BossAbility ability)
    {
        if (!isTransformed || currentTransformation == null) return;
        if (abilityCooldowns[ability] > 0) return;
        if (currentMagicAura < ability.magicCost) return;
        
        StartCoroutine(ExecuteTransformationAbility(ability));
        
        abilityCooldowns[ability] = ability.cooldown;
        
        currentMagicAura -= ability.magicCost;
        targetMagicValue = currentMagicAura / maxMagicAura;
        
        Debug.Log($"Used {ability.abilityName}!");
    }
    
    private IEnumerator ExecuteTransformationAbility(BossAbility ability)
    {
        isAttacking = true;
        canAttack = false;
        
        State = ability.animationState;
        
        float animationLength = GetAnimationLength(ability.animationState);
        yield return new WaitForSeconds(animationLength * 0.5f);
        
        ExecuteAbilityEffect(ability);
        
        yield return new WaitForSeconds(animationLength * 0.5f);
        
        isAttacking = false;
        canAttack = true;
    }

    private void ExecuteAbilityEffect(BossAbility ability)
    {
        switch (ability.abilityName)
        {
            case "Boss Fireball":
                SpawnBossFireball(ability);
                break;
            case "Fire Hands":
                ExecuteFireHands(ability);
                break;
            case "Fireball Rain":
                StartCoroutine(SpawnBossFireballRain(ability));
                break;
            case "Shadow Bolt":
                ExecuteShadowBolt(ability);
                break;
            case "Shadow Wave":
                StartCoroutine(ExecuteShadowWave(ability));
                break;
            case "Shadow Strike":
                ExecuteShadowStrike(ability);
                break;
            case "Shadow Teleport":
                StartCoroutine(ExecuteShadowTeleport(ability));
                break;
            default:
                if (ability.effectPrefab != null)
                {
                    GameObject effect = Instantiate(ability.effectPrefab, fireballSpawnPoint.position, Quaternion.identity);
                    Destroy(effect, 3f);
                }
                break;
        }
    }

    private void ExecuteShadowBolt(BossAbility ability)
    {
        ShadowBossTransformation shadowTransform = currentTransformation as ShadowBossTransformation;
        if (shadowTransform?.shadowBoltPrefab == null) return;

        GameObject shadowBolt = Instantiate(shadowTransform.shadowBoltPrefab, fireballSpawnPoint.position, Quaternion.identity);
        shadowBolt.GetComponentInChildren<SpriteRenderer>().color = new Color(0.5f, 0f, 0.8f, 1f);

        float direction = spriteHolder2.localScale.x > 0 ? 1 : -1;
        Vector2 boltDirection = new Vector2(direction, 0);

        ShadowBolt boltScript = shadowBolt.GetComponent<ShadowBolt>();
        if (boltScript == null)
        {
            boltScript = shadowBolt.AddComponent<ShadowBolt>();
        }

        boltScript.Initialize(10f, boltDirection, ability.damage, true);

        AudioManager.Instance?.PlaySFX("Shadow_Bolt");
    }

    private IEnumerator ExecuteShadowWave(BossAbility ability)
    {
        ShadowBossTransformation shadowTransform = currentTransformation as ShadowBossTransformation;
        if (shadowTransform?.shadowWavePrefab == null) yield break;

        int waveCount = 5;
        for (int i = 0; i < waveCount; i++)
        {
            float angle = (180f / (waveCount - 1)) * i - 90f; 
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (spriteHolder2.localScale.x < 0)
            {
                direction.x = -direction.x;
            }

            Vector3 spawnPos = transform.position + (Vector3)(direction * 1.5f);
            GameObject shadowWave = Instantiate(shadowTransform.shadowWavePrefab, spawnPos, Quaternion.identity);

            shadowWave.GetComponentInChildren<SpriteRenderer>().color = new Color(0.3f, 0f, 0.5f, 0.8f);

            ShadowWave waveScript = shadowWave.GetComponent<ShadowWave>();
            if (waveScript == null)
            {
                waveScript = shadowWave.AddComponent<ShadowWave>();
            }

            waveScript.Initialize(6f, direction, ability.damage, true);

            yield return new WaitForSeconds(0.1f);
        }

        AudioManager.Instance?.PlaySFX("Shadow_Wave");
    }

    private void ExecuteShadowStrike(BossAbility ability)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, ability.range, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable enemyScript = enemy.GetComponent<IDamageable>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(ability.damage);

                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * 30f, ForceMode2D.Impulse);
                }
            }
        }

        if (groundPoundEffect != null)
        {
            GameObject effect = Instantiate(groundPoundEffect, transform.position, Quaternion.identity);
            effect.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0f, 0.5f, 0.8f);
            Destroy(effect, 2f);
        }

        AudioManager.Instance?.PlaySFX("Shadow_Strike");
    }

    private IEnumerator ExecuteShadowTeleport(BossAbility ability)
    {
        SpriteRenderer sprite = spriteHolder2.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            float fadeTime = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.2f, elapsedTime / fadeTime);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        float teleportDistance = 5f;
        float direction = spriteHolder2.localScale.x > 0 ? 1 : -1;
        Vector3 teleportPosition = transform.position + new Vector3(direction * teleportDistance, 0, 0);

        RaycastHit2D groundHit = Physics2D.Raycast(teleportPosition, Vector2.down, 5f, groundLayer);
        if (groundHit.collider != null)
        {
            teleportPosition.y = groundHit.point.y + 1f; 
        }

        transform.position = teleportPosition;

        if (sprite != null)
        {
            Color originalColor = sprite.color;
            float fadeTime = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0.2f, 1f, elapsedTime / fadeTime);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        AudioManager.Instance?.PlaySFX("Shadow_Teleport");
    }

    private void SpawnBossFireball(BossAbility ability)
    {
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        float direction = spriteHolder2.localScale.x > 0 ? 1 : -1;
        
        fireball.transform.localScale = new Vector3(0.4f * direction, 0.4f, 0.4f);
        fireball.GetComponent<SpriteRenderer>().color = Color.red;
        
        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            fireballScript.Initialize(fireballSpeed * 1.5f, direction, true);
        }
        
        Destroy(fireball, 5f);
    }
    
    private void ExecuteFireHands(BossAbility ability)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, ability.range, enemyLayer);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable enemyScript = enemy.GetComponent<IDamageable>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(ability.damage);
            }
        }
    }
    
    private IEnumerator SpawnBossFireballRain(BossAbility ability)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 spawnPoint = new Vector3(
                transform.position.x + Random.Range(-4f, 4f),
                transform.position.y + 6f,
                0
            );
            
            GameObject fireball = Instantiate(fireballPrefab2, spawnPoint, Quaternion.identity);
            fireball.transform.localRotation = Quaternion.Euler(0, 0, -90f);
            
            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            if (fireballRb != null)
            {
                fireballRb.linearVelocity = new Vector2(Random.Range(-3f, 3f), -10f);
            }
            
            Destroy(fireball, 4f);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void UpdateTransformationUI()
    {
        if (transformationIcon != null && transformationText != null)
        {
            if (unlockedTransformations.Count > 0)
            {
                var currentTrans = unlockedTransformations[currentTransformationIndex];
                transformationIcon.sprite = currentTrans.bossSprite;
                transformationText.text = currentTrans.bossName;

                if (!isTransformed && transformationCooldown > 0)
                {
                    transformationIcon.color = Color.red; // On cooldown
                }
                else
                {
                    transformationIcon.color = Color.white; // Available
                }
            }
        }

        if (transformationCooldownOverlay != null)
        {
            if (transformationCooldown > 0)
            {
                transformationCooldownOverlay.fillAmount = transformationCooldown / transformationCooldownTime;
                transformationCooldownOverlay.gameObject.SetActive(true);
            }
            else
            {
                transformationCooldownOverlay.fillAmount = 0f;
                transformationCooldownOverlay.gameObject.SetActive(false);
            }
        }

        if (transformationText != null && transformationCooldown > 0)
        {
            int remainingSeconds = Mathf.CeilToInt(transformationCooldown);
            var currentTrans = unlockedTransformations[currentTransformationIndex];
            transformationText.text = $"{currentTrans.bossName} ({remainingSeconds}s)";
        }
    }

    public void UnlockTransformation(BossTransformation transformation)
    {
        if (!unlockedTransformations.Contains(transformation))
        {
            unlockedTransformations.Add(transformation);
            Debug.Log($"Unlocked transformation: {transformation.bossName}!");
            
            StartCoroutine(ShowUnlockNotification(transformation));
        }
    }
    
    private IEnumerator ShowUnlockNotification(BossTransformation transformation)
    {
        Debug.Log($"🎉 NEW TRANSFORMATION UNLOCKED: {transformation.bossName}!");
        yield return new WaitForSeconds(3f);
    }
    
    public void OnBossDefeated(BossController boss)
    {
        BossTransformation transformation = FindTransformationForBoss(boss);
        if (transformation != null)
        {
            UnlockTransformation(transformation);
        }
    }

    private BossTransformation FindTransformationForBoss(BossController boss)
    {
        if (boss is FireWizardBossController)
        {
            return Resources.Load<BossTransformation>("Transformations/FireWizardTransformation");
        }
        else if (boss is ShadowBossController)
        {
            return Resources.Load<BossTransformation>("Transformations/ShadowBossTransformation");
        }

        return null;
    }

    private void UseItem(Item item)
    {
        switch (item.itemType)
        {
            case Item.ItemType.HealthPotion:
                currentHealth += 20;
                targetHealthValue = currentHealth / maxHealth;
                if (currentHealth > 100) currentHealth = 100;
                inventory.RemoveItem(item.itemType);
                break;

            case Item.ItemType.ManaPotion:
                currentMagicAura += 20;
                targetMagicValue = currentMagicAura / maxMagicAura;
                if (currentMagicAura > 100) currentMagicAura = 100;
                inventory.RemoveItem(item.itemType);
                break;
        }
    }
    public void RestoreMagic(float magicAmount)
    {
        currentMagicAura += magicAmount;
        currentMagicAura = Mathf.Clamp(currentMagicAura, 0, maxMagicAura);
        targetMagicValue = currentMagicAura / maxMagicAura;
    }

    private void UpdateTexts()
    {
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHealth} / {(int)maxHealth}";
        }

        if (magicAuraText != null)
        {
            magicAuraText.text = $"{(int)currentMagicAura} / {(int)maxMagicAura}";
        }
    }
    private void Run()
    {
        if (isGrounded) State = States.run;

        float moveInput = Input.GetAxis("Horizontal");
        Vector3 dir = transform.right * moveInput;

        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);

        if (moveInput != 0)
        {
            Vector3 scale = spriteHolder2.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveInput > 0 ? 1 : -1);
            spriteHolder2.localScale = scale;
        }
    }

    private void Jump()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator PerformAttack(States attackType)
    {
        isAttacking = true;
        canAttack = false;
        if (attackType == States.attackTwo)
        {
            float animationLength = GetAnimationLength(attackType) / 1.7f;
            yield return new WaitForSeconds(animationLength);
            SpawnFireball();
            currentMagicAura -= fireballMagicCost;
        }

        else if (attackType == States.attackOne)
        {
            float animationLength = GetAnimationLength(attackType);
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(SpawnFireballRain());
            currentMagicAura -= rainFireballMagicCost;
        }

        else if (attackType == States.attackThree)
        {
            float animationLength = GetAnimationLength(attackType);
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(GroundPound());
            currentMagicAura -= groundPoundMagicCost;
        }

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void SpawnFireball()
    {
        Debug.Log("SpawnFireball method called");

        if (fireballPrefab == null)
        {
            Debug.LogError("fireballPrefab is null!");
            return;
        }

        if (fireballSpawnPoint == null)
        {
            Debug.LogError("fireballSpawnPoint is null!");
            return;
        }

        Debug.Log($"Attempting to spawn fireball at position: {fireballSpawnPoint.position}");
        Debug.Log($"Fireball prefab layer: {fireballPrefab.layer}");
        Debug.Log($"Fireball prefab name: {fireballPrefab.name}");

        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

        if (fireball == null)
        {
            Debug.LogError("Failed to instantiate fireball!");
            return;
        }

        Debug.Log($"Fireball successfully created! Name: {fireball.name}, Layer: {fireball.layer}");
        Debug.Log($"Fireball position: {fireball.transform.position}");
        Debug.Log($"Fireball active: {fireball.activeInHierarchy}");

        // Check renderer component
        SpriteRenderer renderer = fireball.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Debug.Log($"Renderer enabled: {renderer.enabled}");
            Debug.Log($"Renderer color: {renderer.color}");
            Debug.Log($"Sorting layer: {renderer.sortingLayerName}");
            Debug.Log($"Order in layer: {renderer.sortingOrder}");
            Debug.Log($"Sprite: {renderer.sprite}");
        }
        else
        {
            Debug.LogError("No SpriteRenderer found on fireball!");
        }

        float direction = spriteHolder2.localScale.x > 0 ? 1 : -1;

        fireball.transform.localScale = new Vector3(0.4f * direction, 0.4f, 0.4f);

        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            Debug.Log("Fireball script found, initializing...");
            fireballScript.Initialize(fireballSpeed, direction, true);
        }
        else
        {
            Debug.LogError("Fireball script not found on instantiated object!");
        }

        Destroy(fireball, 5f);
    }

    public IEnumerator SpawnFireballRain()
    {
        Debug.Log("HERE");
        if (fireballPrefab != null)
        {
            Vector3 playerPosition = transform.position;

            for (int i = 0; i < rainFireballCount; i++)
            {
                float xOffset;
                if (i % 2 == 0)
                {
                    xOffset = UnityEngine.Random.Range(0f, rainSpread);
                }
                else
                {
                    xOffset = -UnityEngine.Random.Range(0f, rainSpread);
                }

                Vector3 spawnPoint = new Vector3(
                    playerPosition.x + xOffset,
                    playerPosition.y + rainHeight,
                    0
                );

                AudioManager.Instance.PlaySFX("Magic sphere");
                GameObject fireball = Instantiate(fireballPrefab2, spawnPoint, Quaternion.identity);

                fireball.transform.localRotation = Quaternion.Euler(0, 0, -90f);

                Debug.Log($"Fireball spawned at: {spawnPoint}");

                Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
                if (fireballRb != null)
                {
                    Vector2 fallVelocity = new Vector2(
                        Random.Range(-2f, 2f),
                        -fireballSpeed * 0.7f
                    );
                    fireballRb.linearVelocity = fallVelocity;
                }

                Destroy(fireball, 4f);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator GroundPound()
    {
        if (groundPoundEffect != null)
        {
            GameObject effect = Instantiate(groundPoundEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (magicAura != null)
        {
            magicAura.Play();
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, groundPoundRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            IDamageable enemyScript = enemy.GetComponent<IDamageable>();
            if (enemyRb != null)
            {
                enemyScript.TakeDamage(groundPoundDamage);
                Vector2 knowbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knowbackDirection * 40, ForceMode2D.Impulse);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    public void TakeDamage(float damage)
    {
        State = States.hit;
        isGettingHit = true;

        Debug.Log("Player is taking damage");

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        targetHealthValue = currentHealth / maxHealth;
        StartCoroutine(TakeDamage());
        Update();

        if (currentHealth == 0)
        {
            Destroy(gameObject);
            return;
        }

    }

    private IEnumerator TakeDamage()
    {
        Debug.Log($"current state is {State.ToString()}");
        float animationLength = GetAnimationLength(States.hit);

        yield return new WaitForSeconds(animationLength);
        isGettingHit = false;
    }

    private float GetAnimationLength(States state)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        string animationName = "";
        switch (state)
        {
            case States.attackOne:
                animationName = "attackOne"; 
                break;
            case States.attackTwo:
                animationName = "attackTwo";
                break;
            case States.attackThree:
                animationName = "attackThree";
                break;
            case States.hit:
                animationName = "hit";
                break;
        }

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        return 1f; 
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded && !isAttacking)
            State = States.jump;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void GetDamage()
    {
        lives -= 10;
        Debug.Log(lives);
    }
}