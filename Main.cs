using System.Collections.Generic;
using System.Linq;
using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Utils;
using Core.Card;
using Core.Utils;

public class Main : IModMain
{
    public string Version => "1.0.0";

    public void Register(IModRegister register)
    {
        register.AddCard(new CardDefinition(new CardDataID("testCard"), "Test Card", new TestCardBehavior()).Knight());
        register.RemoveCard("sword");
    }
}

public class TestCardBehavior : BasicWeaponCardBehavior
{
    public override int GetSimpleAttackValue(IGameOrSimEntity card) => 12;
}
