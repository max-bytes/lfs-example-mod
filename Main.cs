using System.Collections.Generic;
using System.Linq;
using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Utils;

public class Main : IModMain
{
    public string Version => "1.0.0";

    public void Register(IModRegister register)
    {
        register.AddCard(new CardDefinition(new CardDataID("testCard"), "Test Card", new TestCardBehavior()).Knight());
        register.RemoveCard("sword");

        register.AddInterceptor<TestCardBehavior.Interceptor>();
    }
}

public class TestCardBehavior : BasicWeaponCardBehavior
{
    public override int GetSimpleAttackValue(IGameOrSimEntity card) => 12;
    public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => $"[b]Equip[/b] Armor: get +1 Attack";
    public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
    {
        yield return KeywordID.Equip;
        yield return KeywordID.Armor;
    }

    public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
    {
        e.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(e.iD.value) });
    }
    
    public class Interceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public Interceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is RequestEquipCardAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is RequestEquipCardAction ra)
            {
                if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                    yield break;
                    
                var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                if (equippedCard == null)
                    yield break;

                if (CardUtils.GetCardType(equippedCard) != CardType.Armor)
                    yield break;

                yield return new ActivateCardAction(self.iD.value);
                yield return new ModifyAttackValueModifierAction(self.iD.value, 1);
                yield return new DelayAction(10);
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }
}
