﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Move))]
public class Unit : Entity
{
    public event Action<Unit, int> OnHealthChanged;
    public event Action<Unit, int> OnEnergyChanged;
    public event Action<Unit, int> OnTimeChanged;
    public event Action<Unit> OnUnitDeath;
    public event Action OnFinishAbilityUse;

    [Header("General")] 
    [SerializeField] private UnitBarPack unitBarPack;
    [SerializeField] private UnitData unitData;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private Transform pivot;
    [SerializeField] private GameObject marker;
    [SerializeField] private MeshRenderer highlight;
    [SerializeField] private int teamId;
    
    // Stats
    public int health { get; private set; }
    public int energy { get; private set; }
    public int time { get; private set; }

    // General
    public UnitData UnitData { get { return unitData; } private set { unitData = value; } }
    public Animator AnimatorUnit { get { return animator; } private set { animator = value; } }
    public int TeamId { get { return teamId; } private set { teamId = value; } }
    // Abilities
    public DeckManager DeckManager = new DeckManager();

    public bool usingAbility { get; private set; } = false;

    private UnitBarPack _boundBarPack;

    private void Start()
    {
        if (unitBarPack != null)
        {
            _boundBarPack = Instantiate(unitBarPack, GameController.Instance.UIController.WorldUIParent);
            _boundBarPack.BindUnit(this);
        }

        GameController.Instance.Grid.unitList.Add(this);
        GameController.Instance.SceneController.OnUnitSelect += MarkerUnit;

        marker.SetActive(false);
        SetNearbyCoordsAndPosition();

        if (teamId == 0) { return; }

        health = unitData.maxHealth;
        energy = unitData.maxEnergy;
        time = unitData.maxTime;

        DeckManager.SetStartingDeck(UnitData.StartingDeck);
        for (int i = 0; i < 3; i++)
        {
            DeckManager.DrawCard();
        }
        switch (teamId)
        {
            case 1:
                highlight.material.color *= Color.blue;
                break;
            case 2:
                highlight.material.color *= Color.red;
                break;
        }

        SceneController.Counter[teamId - 1]++;
        Debug.Log("Team " + teamId + " counter: " + SceneController.Counter[teamId - 1]);
        OnUnitDeath += (Unit unit) =>
        {
            if (unit == this)
            {
                GameController.Instance.SceneController.OnUnitSelect -= MarkerUnit;
                Destroy(_boundBarPack.gameObject);
                
                Destroy(pivot.gameObject, 3f);
            }
        };
        OnUnitDeath += GameController.Instance.SceneController.UnitDeath;
    }

    private Vector2Int GetNearbyCoords(Vector3 startPoint, Vector2Int gridSize, float nodeSize)
    {
        Vector2Int newCoords = new Vector2Int(Mathf.RoundToInt((transform.position.x - startPoint.x) / nodeSize),
            Mathf.RoundToInt((transform.position.z - startPoint.z) / nodeSize));

        if (newCoords.x < 0 || newCoords.x >= gridSize.x || newCoords.y < 0 || newCoords.y >= gridSize.y) // if not in grid range
            newCoords = new Vector2Int(-1, -1);
        return newCoords;
    }
    private void SetNearbyCoordsAndPosition()
    {
        Vector2Int testCoords;

        testCoords = GetNearbyCoords(GameController.Instance.Grid.transform.position, new Vector2Int(GameController.Instance.Grid.XSize,
            GameController.Instance.Grid.YSize), GameController.Instance.Grid.NodeSize);
        if (testCoords != new Vector2Int(-1, -1))
        {
            coords = testCoords;
        }
        pivot.transform.position = GameController.Instance.Grid.transform.position +
                new Vector3(coords.x * GameController.Instance.Grid.NodeSize, pivot.transform.position.y, 
                    coords.y * GameController.Instance.Grid.NodeSize);
    }
    private void MarkerUnit(Unit unit)
    {
        if (unit == this)
        {
            marker?.SetActive(true);
            return;
        }
        marker?.SetActive(false);
    }
    
    public void ChangeHealth(int value)
    {
        health += value;
        health = Mathf.Clamp(health, 0, unitData.maxHealth);
        OnHealthChanged?.Invoke(this, health);

        if (health <= 0)
        {
            animator?.SetTrigger("Death");
            OnUnitDeath?.Invoke(this);
            // Dead, show animation, remove unit from scene soon, subtract from counter above
        }
        else
            if (value < 0)
            {
                animator?.SetTrigger("TakeHit");
            }
    }
    public void ChangeEnergy(int value)
    {
        energy += value;
        energy = Mathf.Clamp(energy, 0, unitData.maxEnergy);
        OnEnergyChanged?.Invoke(this, energy);
    }
    public void ChangeTime(int value)
    {
        time += value;
        time = Mathf.Clamp(time, 0, unitData.maxTime);
        OnTimeChanged?.Invoke(this, time);
    }

    public IEnumerator MoveByPath(List<PathNode> path)
    {
        usingAbility = true;
        animator?.SetBool("Moving", true);

        Vector3 destination = path[path.Count - 1].node.transform.position;
        path.RemoveAt(path.Count - 1);
        pivot.rotation = Quaternion.LookRotation(destination - pivot.position);

        while ((destination - pivot.position).magnitude > moveSpeed * Time.deltaTime * 2)
        {
            pivot.position += (destination - pivot.position).normalized * moveSpeed * Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        pivot.position = destination;

        if (path.Count > 0)
        {
            StartCoroutine(MoveByPath(path));
        }
        else
        {
            usingAbility = false;
            animator?.SetBool("Moving", false);
            OnFinishAbilityUse?.Invoke();
        }
    }
}
