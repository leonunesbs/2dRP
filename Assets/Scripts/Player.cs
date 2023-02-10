using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
  public Entity entity;

  [Header("Game Manager")]
  public GameManager gameManager;

  [Header("Regeneration")]
  public bool regenHealth = true;
  public bool regenMana = true;
  public int regenHealthTick = 1;
  public float regenHealthRate = 5;
  public int regenManaTick = 1;
  public float regenManaRate = 10;

  [Header("Respawn")]
  public float respawnTime = 5f;
  public GameObject prefab;
  Transform respawnPoint;
  Vector3 defaultRespawnPoint = new Vector3(0, 0, 0);


  [Header("Player UI")]
  public Slider healthSlider;
  public Slider manaSlider;
  public Slider staminaSlider;
  public Slider experienceSlider;
  public Text levelText;


  // Start is called before the first frame update
  void Start()
  {
    if (gameManager == null)
    {
      Debug.LogError("Game Manager is null");
      return;
    }

    entity.maxHealth = gameManager.CalculateHealth(entity);
    entity.maxMana = gameManager.CalculateMana(entity);
    entity.maxStamina = gameManager.CalculateStamina(entity);

    entity.currentHealth = entity.maxHealth;
    entity.currentMana = entity.maxMana;
    entity.currentStamina = entity.maxStamina;

    healthSlider.maxValue = entity.maxHealth;
    healthSlider.value = entity.currentHealth;

    manaSlider.maxValue = entity.maxMana;
    manaSlider.value = entity.currentMana;

    staminaSlider.maxValue = entity.maxStamina;
    staminaSlider.value = entity.currentStamina;

    experienceSlider.maxValue = 0;
    experienceSlider.value = 0;

    levelText.text = "Level: " + entity.level;

    // Start the regeneration
    StartCoroutine(RegenerateHealth());
    StartCoroutine(RegenerateMana());
  }

  void Update()
  {
    if (entity.isDead)
      return;

    if (entity.currentHealth <= 0)
    {
      Die();
    }



    healthSlider.value = entity.currentHealth;
    manaSlider.value = entity.currentMana;
    staminaSlider.value = entity.currentStamina;
  }

  IEnumerator RegenerateHealth()
  {
    while (true)
    {
      if (regenHealth)
      {
        if (entity.currentHealth < entity.maxHealth)
        {
          Debug.Log("Regenerating health");
          entity.currentHealth += regenHealthTick;
          yield return new WaitForSeconds(regenHealthRate);
        }
      }
      yield return null;
    }
  }

  IEnumerator RegenerateMana()
  {
    while (true)
    {
      if (regenMana)
      {

        if (entity.currentMana < entity.maxMana)
        {
          Debug.Log("Regenerating mana");
          entity.currentMana += regenManaTick;
          yield return new WaitForSeconds(regenManaRate);
        }
      }
      yield return null;
    }
  }

  void Die()
  {
    entity.currentHealth = 0;
    entity.isDead = true;
    entity.target = null;

    StopAllCoroutines();
    StartCoroutine(Respawn());
  }

  IEnumerator Respawn()
  {
    GetComponent<PlayerController>().enabled = false;
    yield return new WaitForSeconds(respawnTime);

    GameObject newPlayer = Instantiate(prefab, defaultRespawnPoint, transform.rotation, null);

    newPlayer.name = prefab.name;
    newPlayer.GetComponent<Player>().entity.isDead = false;
    newPlayer.GetComponent<Player>().entity.combatCoroutineRunning = false;
    newPlayer.GetComponent<PlayerController>().enabled = true;

    Destroy(this.gameObject);
  }
}
