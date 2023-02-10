using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Monster : MonoBehaviour
{
  [Header("Controller")]
  public Entity entity;
  public GameManager gameManager;

  [Header("Patrol")]
  public List<Transform> patrolPoints;
  public int patrolPointsId;
  public float arrivalDistance = 0.5f;
  public float waitTime = 5f;

  Transform targetPatrolPoint;
  int currentPatrolPoint = 0;
  float lastDistanceToPatrolPoint = 0f;
  float currentWaitTime = 0f;

  [Header("Rewards")]
  public int experienceReward = 10;
  public int minGoldReward = 0;
  public int maxGoldReward = 10;

  [Header("Respawn")]
  public GameObject prefab;
  public bool respawn = true;
  public float respawnTime = 5f;

  [Header("Monster UI")]
  public Slider healthSlider;
  public Text monsterNameUI;


  Rigidbody2D rb2d;
  Animator animator;


  private void Start()
  {
    rb2d = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    entity.maxHealth = gameManager.CalculateHealth(entity);
    entity.maxMana = gameManager.CalculateMana(entity);
    entity.maxStamina = gameManager.CalculateStamina(entity);

    entity.currentHealth = entity.maxHealth;
    entity.currentMana = entity.maxMana;
    entity.currentStamina = entity.maxStamina;

    healthSlider.maxValue = entity.maxHealth;
    healthSlider.value = entity.currentHealth;

    monsterNameUI.text = entity.name;

    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Patrol Point"))
    {
      int ID = obj.GetComponent<PatrolPointsID>().ID;
      if (ID == patrolPointsId)
      {
        patrolPoints.Add(obj.transform);
      }
    }

    currentWaitTime = waitTime;

    if (patrolPoints.Count > 0)
    {
      targetPatrolPoint = patrolPoints[currentPatrolPoint];
      lastDistanceToPatrolPoint = Vector2.Distance(transform.position, targetPatrolPoint.position);
    }
  }

  private void Update()
  {
    if (entity.isDead)
      return;

    healthSlider.value = entity.currentHealth;

    if (entity.currentHealth <= 0)
      Die();

    if (!entity.inCombat)
    {
      if (patrolPoints.Count > 0)
        Patrol();
      else
        Idle();
    }
    else
    {
      if (entity.attackTimer > 0)
        entity.attackTimer -= Time.deltaTime;

      if (entity.attackTimer < 0)
        entity.attackTimer = 0;

      if (entity.target != null && entity.inCombat)
      {
        if (!entity.combatCoroutineRunning)
          StartCoroutine(Combat());
      }
      else
      {
        Debug.Log("Target is null");
        entity.combatCoroutineRunning = false;
        StopCoroutine(Combat());
      }
    }
  }

  private void OnTriggerStay2D(Collider2D collider)
  {
    if (collider.tag == "Player" && !entity.isDead)
    {
      entity.inCombat = true;
      entity.target = collider.gameObject;
      entity.target.GetComponent<BoxCollider2D>().isTrigger = true;
    }
    // else
    // {
    //   entity.inCombat = false;
    //   entity.target = null;
    // }
  }
  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.tag == "Player")
    {
      entity.inCombat = false;
      if (entity.target)
      {

        entity.target.GetComponent<BoxCollider2D>().isTrigger = false;
        entity.target = null;
      }
    }
  }

  void Idle()
  {
    animator.SetBool("isWalking", false);
  }

  void Patrol()
  {
    if (entity.isDead)
      return;

    float distanceToTarget = Vector2.Distance(transform.position, targetPatrolPoint.position);

    if (distanceToTarget <= arrivalDistance || distanceToTarget > lastDistanceToPatrolPoint)
    {
      animator.SetBool("isWalking", false);
      if (currentWaitTime <= 0)
      {
        currentPatrolPoint++;

        if (currentPatrolPoint >= patrolPoints.Count)
          currentPatrolPoint = 0;

        targetPatrolPoint = patrolPoints[currentPatrolPoint];
        lastDistanceToPatrolPoint = Vector2.Distance(transform.position, targetPatrolPoint.position);

        currentWaitTime = waitTime;
      }
      else
        currentWaitTime -= Time.deltaTime;
    }
    else
    {
      lastDistanceToPatrolPoint = distanceToTarget;
      animator.SetBool("isWalking", true);
    }
    Vector2 direction = (targetPatrolPoint.position - transform.position).normalized;
    animator.SetFloat("inputX", direction.x);
    animator.SetFloat("inputY", direction.y);
    rb2d.MovePosition(rb2d.position + direction * (entity.speed * Time.fixedDeltaTime));
  }

  IEnumerator Combat()
  {
    animator.SetBool("isWalking", false);
    entity.combatCoroutineRunning = true;
    while (true)
    {
      yield return new WaitForSeconds(entity.attackCooldown);

      if (entity.target != null && !entity.target.GetComponent<Player>().entity.isDead)
      {
        float distance = Vector2.Distance(entity.target.transform.position, transform.position);

        if (distance <= entity.attackRange)
        {
          int monsterDamage = gameManager.CalculateDamage(entity, entity.strength);
          int targetDefense = gameManager.CalculateDefense(entity.target.GetComponent<Player>().entity, entity.target.GetComponent<Player>().entity.endurance);
          int damage = monsterDamage - targetDefense;

          if (damage < 0)
            damage = 0;

          entity.target.GetComponent<Player>().entity.currentHealth -= damage;
          Debug.Log("Monster dealt " + damage + " damage to player");


        }
      }
    }
  }

  void Die()
  {
    animator.SetBool("isWalking", false);
    entity.currentHealth = 0;
    entity.isDead = true;
    entity.inCombat = false;
    entity.target = null;
    patrolPoints.Clear();


    // add exp
    // gameManager.AddExperience(experienceReward);

    // add gold
    // gameManager.AddGold(Random.Range(minGoldReward, maxGoldReward));

    Debug.Log("Monster died");

    StopAllCoroutines();

    if (respawn)
      StartCoroutine(Respawn());
  }

  IEnumerator Respawn()
  {
    yield return new WaitForSeconds(respawnTime);

    GameObject newMonster = Instantiate(prefab, transform.position, transform.rotation, null);
    newMonster.name = prefab.name;
    newMonster.GetComponent<Monster>().entity.isDead = false;
    newMonster.GetComponent<Monster>().entity.combatCoroutineRunning = false;

    Destroy(this.gameObject);
  }

}
