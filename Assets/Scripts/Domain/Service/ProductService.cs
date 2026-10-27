using System.Collections.Generic;
using UnityEngine;
using System;

public class ProductService
{
    readonly GameState game;
    readonly TroopFactory troopFactory;
    public ProductService(
        GameState game,
        TroopFactory troopFactory
    ){
        this.game = game;
        this.troopFactory = troopFactory;
    }

    public void CreateTroop(PlayerSide player,TroopType type,HexPosition position)
    {
        PlayerState producer = player == PlayerSide.Red ? game.RedState:game.BlueState;
        int troopPrice = Data.GetTroopPrice(type);
        if(producer.SpendableMoney(troopPrice) && !game.BoardState.IsTroopAlreadyExist(position))
        try
        {
            producer.SpendMoney(Data.GetTroopPrice(type));
        }
        catch(ArgumentOutOfRangeException ex)
        {
            Debug.Log(ex.Message);
            return;
        }

        Troop newTroop = troopFactory.CreateTroop(type,player);
        game.BoardState.AddTroop(newTroop,position);
        
    }

}