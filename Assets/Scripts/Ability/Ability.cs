﻿using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [SerializeField] protected AbilityData abilityData;
    public AbilityData AbilityInfo { get { return abilityData; } protected set { abilityData = value; } }
    [SerializeField] protected AbilityEffect abilityEffect;
    [SerializeField] protected AudioClip soundEffect;
    [SerializeField] protected AudioSource audioSource;

    public List<PathNode> area { get; protected set; }
    public List<PathNode> aoe { get; protected set; }

    public virtual List<PathNode> GetNodesInRange(Unit user)
    {
        if (!user || !SceneController.Instance.Grid.NodeExists(user.Coords))
            return new List<PathNode>();

        Node start = SceneController.Instance.Grid.nodeList[user.Coords.x, user.Coords.y];
        area = new List<PathNode>();

        switch (abilityData.rangeType)
        {
            case AbilityData.AreaType.Pathfinding:
                area = Pathfinding.GetNodesInPathfindingRange(start, abilityData.minRange, abilityData.maxRange);
                break;
            case AbilityData.AreaType.Impulse:
                area = Pathfinding.GetNodesInImpulseRange(start, abilityData.minRange, abilityData.maxRange);
                break;
            case AbilityData.AreaType.Absolute:
                area = Pathfinding.GetNodesInAbsoluteRange(start, abilityData.minRange, abilityData.maxRange);
                break;
        }
        return area;

    }
    public virtual List<PathNode> GetAoe(Unit user, PathNode pathNode)
    {
        aoe = new List<PathNode>();

        switch (abilityData.areaType)
        {
            case AbilityData.AreaType.Pathfinding:
                aoe = Pathfinding.GetNodesInPathfindingRange(pathNode.node, abilityData.minAreaRange, abilityData.maxAreaRange);
                break;
            case AbilityData.AreaType.Impulse:
                aoe = Pathfinding.GetNodesInImpulseRange(pathNode.node, abilityData.minAreaRange, abilityData.maxAreaRange);
                break;
            case AbilityData.AreaType.Absolute:
                aoe = Pathfinding.GetNodesInAbsoluteRange(pathNode.node, abilityData.minAreaRange, abilityData.maxAreaRange);
                break;
            default:
                break;
        }
        return aoe;
    }

    public virtual void UseAbility(Unit user, List<PathNode> aoe)
    {
        if (audioSource && soundEffect)
        {
            audioSource.clip = soundEffect;
            audioSource.Play();
        }

        if (UIController.Instance.selectedAbilityId != 0)
        {
            user.DiscardCard(UIController.Instance.selectedAbilityId - 1);
            user.AnimatorUnit.SetTrigger("UseSpell");
            UIController.Instance.SetId(0);
            SceneController.Instance.SetSelectedAbility(null);
        }
    }

}