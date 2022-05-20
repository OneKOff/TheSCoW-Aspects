﻿using System.Collections.Generic;
using UnityEngine;

public class MindFlow : Ability
{
    public override void UseAbility(Unit user, List<PathNode> aoe)
    {
        if (abilityData.epCost > user.energy)
        {
            return; // Not enough energy
        }

        base.UseAbility(user, aoe);
        user.ChangeEnergy(-abilityData.epCost);

        AbilityEffect aEffect;
        aEffect = ObjectPooler.Instance.SpawnFromPool(abilityEffect.EffectTag, 
            SceneController.Instance.Grid.nodeList[user.Coords.x, user.Coords.y].transform.position, abilityEffect.transform.rotation).GetComponent<AbilityEffect>();

        for (int i = 0; i < 2; i++)
        {
            user.DrawCard();
        }
    }
}