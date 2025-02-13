using Base.Card;
using CardStuff.Card;
using Core.Card;
using Core.Utils;

public class Main : IModMain
{
    public string Version => "1.0.0";

    public void Register()
    {
        CardRepository.RegisterCard(new CardDefinition(new CardDataID("testCard"), "Test Card", new TestCardBehavior()).Knight());
    }
}

public class TestCardBehavior : BasicWeaponCardBehavior
{
    public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3;
}
