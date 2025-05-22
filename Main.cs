using System.Collections.Generic;
using Action;
using Base.Card;
using CardStuff.Card;
using Core.Action;
using Core.Card;
using Core.Utils;

public class Main : IModMain
{
    public string Version => typeof(Main).Assembly.GetName().Version.ToString();

    public void Register(IModRegister register)
    {
        // add a completely new hero to the game
        register.AddHero(new HeroData("stickman", 30, false, "Just a simple stickman"), "Stickman");

        // also give the hero a default starting deck
        register.AddStartingDeck(new StartingDeck("default", "stickman", "The Stickler", false, new CardDataID[]
        {
            new CardDataID("bodyArmor"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
        }));

        // add a completely new card, add it to knight and stickman
        register.AddCard(new CardDefinition(new CardDataID("testCard"), "Test Card", new TestCardBehavior()).AddForHero("knight").AddForHero("stickman").SetDropRate());
        register.AddInterceptor<TestCardBehavior.Interceptor>(); // add the interceptor for the card, which is required for cards that use an interceptor

        // completely remove a card from the game
        register.RemoveCard("sword");

        // update an existing card, adding it to the card pool for stickman
        register.UpdateCard(register.GetCard("lance").AddForHero("stickman"));
        
        // add a new starting deck for the knight
        register.AddStartingDeck(new StartingDeck("tester", "knight", "The Tester", false, new CardDataID[]
        {
            new CardDataID("knife"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
            new CardDataID("testCard"),
        }));

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
