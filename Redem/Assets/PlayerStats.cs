using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<float> health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<float> hunger = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<float> temp = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private float hungerPerSecond = 100f / (12f * 60f); //lose all health every 12 minutes

    private float maxHealth;
    private float maxHunger;
    private float maxTemp;

    //health stats
    [SerializeField] private float hurtIncrement = 15f;
    private float hurtPang;
    [SerializeField] private float hurtPerSecond = 100f / (6f * 60f); //lose all health every 6 minutes

    // Start is called before the first frame update
    void Start()
    {
        maxHealth = health.Value;
        maxHunger = hunger.Value;
        maxTemp = temp.Value;
    }

    // Update is called once per frame
    void Update()
    {
        bool starving = HungerUpdate();
        bool cold = TempUpdate();
        HealthUpdate(starving, cold);
    }

    private bool HungerUpdate()
    {
        bool starving = false;

        if(hunger.Value > 0f)
        {
            hunger.Value -= hungerPerSecond * Time.deltaTime;
            starving = false;
        }
        else
        {
            starving = true;
        }

        return starving;
    }

    private bool TempUpdate()
    {
        return false;
    }

    private void HealthUpdate(bool starving, bool cold)
    {
        bool dead = false;
        if(starving)
        {
            hurtPang += hurtPerSecond;
        }
        if(cold)
        {
            hurtPang += hurtPerSecond;
        }

        if (hurtPang >= hurtIncrement)
        {
            health.Value -= hurtPang;
            hurtPang = 0f;
        }

        if (health.Value <= 0f)
        {
            dead = true;
        }

        if(dead)
        {
            Die();
        }
    }

    private void Die()
    {
        //make player go completely limp
    }
}
