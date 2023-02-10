using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Entity
{
  [Header("General")]
  public string name;
  public int level = 1;

  [Header("Health")]
  public int maxHealth;
  public int currentHealth;

  [Header("Mana")]
  public int maxMana;
  public int currentMana;

  [Header("Stamina")]
  public int maxStamina;
  public int currentStamina;

  [Header("Stats")]
  public int strength = 1;
  public int intelligence = 1;
  public int endurance = 1;
  public int willPower = 1;
  public float speed = 1f;

  [Header("Combat")]
  public float attackRange = 1f;
  public float attackTimer = 1f;
  public float attackCooldown = 1f;
  public bool inCombat = false;
  public GameObject target;
  public bool combatCoroutineRunning = false;
  public bool isDead = false;
}
