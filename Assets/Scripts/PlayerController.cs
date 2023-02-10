using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Entity))]
public class PlayerController : MonoBehaviour
{
  [Header("Player")]
  public Player player;
  public Animator playerAnimator;
  float inputX = 0;
  float inputY = 0;
  bool isWalking = false;

  Rigidbody2D rb2D;
  Vector2 movement = Vector2.zero;

  // Start is called before the first frame update
  void Start()
  {
    isWalking = false;
    rb2D = GetComponent<Rigidbody2D>();
    player = GetComponent<Player>();
  }

  // Update is called once per frame
  void Update()
  {
    inputX = Input.GetAxis("Horizontal");
    inputY = Input.GetAxis("Vertical");
    movement = new Vector2(inputX, inputY);


    isWalking = (inputX != 0 || inputY != 0);

    if (isWalking)
    {
      playerAnimator.SetFloat("inputX", inputX);
      playerAnimator.SetFloat("inputY", inputY);
    }

    playerAnimator.SetBool("isWalking", isWalking);


    if (player.entity.attackTimer < 0)
      player.entity.attackTimer = 0;
    else
      player.entity.attackTimer -= Time.deltaTime;


    if (player.entity.attackTimer == 0 && !isWalking)
    {
      if (Input.GetButtonDown("Fire2"))
      {
        Debug.Log("Attack");
        playerAnimator.SetTrigger("attack");
        player.entity.attackTimer = player.entity.attackCooldown;
        Combat();
      }
    }

  }

  // FixedUpdate is called once per physics frame
  void FixedUpdate()
  {
    rb2D.MovePosition(rb2D.position + movement * (player.entity.speed * Time.fixedDeltaTime));
  }

  void OnTriggerStay2D(Collider2D collider)
  {
    if (collider.transform.tag == "Enemy")
    {
      player.entity.inCombat = true;
      player.entity.target = collider.transform.gameObject;
    }
  }

  void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.transform.tag == "Enemy")
    {
      player.entity.inCombat = false;
      player.entity.target = null;
    }
  }
  void Combat()
  {
    if (player.entity.target == null)
      return;

    Monster monster = player.entity.target.GetComponent<Monster>();

    if (monster.entity.isDead)
    {
      player.entity.target = null;
      return;
    }

    float distanceToTarget = Vector2.Distance(transform.position, player.entity.target.transform.position);

    if (distanceToTarget <= player.entity.attackRange)
    {
      int attackDamage = player.gameManager.CalculateDamage(player.entity, player.entity.strength);
      int targetDefense = player.gameManager.CalculateDefense(monster.entity, monster.entity.endurance);
      int damage = attackDamage - targetDefense;

      if (damage < 0)
        damage = 0;

      monster.entity.currentHealth -= damage;
      monster.entity.target = this.gameObject;
    }

  }
}
