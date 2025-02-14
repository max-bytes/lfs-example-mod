using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public abstract class BasicMageArcana : BasicArmorCardBehavior
    {
        public override bool IsArcana => true;
    }

    public class LightningRobeCardBehavior : BasicMageArcana, IActiveEffectCardBehavior
    {
        private int Damage(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose Focus[/b]: trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Lightning; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterDefend(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in CombatService.TriggerLightning(context.HeroEntity, Damage(card.card.data.QualityModifier), context, HurtSource.Armor, context.HeroEntity))
                yield return a;

            if (card.finalDefenseValue.value <= 0)
                yield return new RequestDropCardAction(card.iD.value, RequestDropReason.Used);
        }
    }

    public class ApprenticeRobeArmorCardBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public override int MaxUpgrade => 0;

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;
    }


    public class AttunedRobeActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public AttunedRobeActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is CastSpellPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearArmor || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                yield break;

            var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as AttunedRobeArmorCardBehavior;
            yield return new ModifyDefenseValueModifierAction(cardID, behavior.FocusGain(self.card.data.QualityModifier));
            yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
            yield return new TriggerAnimationAction(self.iD.value, new AnimationTrigger("upgrade"));
        }
    }
    public class AttunedRobeArmorCardBehavior : BasicMageArcana, IActiveEffectCardBehavior
    {
        public int FocusGain(int quality) => 1 + Mathf.FloorToInt(quality / 3f);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gets +{focusGain} Focus when a Spell is cast";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("focusGain", FocusGain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Spell;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Spell; }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new AttunedRobeActionInterceptor(e.iD.value) });
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + Mathf.CeilToInt(card.card.data.QualityModifier / 3f * 2);
    }

    public class TaxCollectorsBootsCardBehavior : BasicMageArcana
    {
        public int FocusGain(int quality) => 2 + quality;
        private int Exert => 1;

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var (ea, success) in CombatService.TryExert(Exert, context))
            {
                yield return ea;

                if (success)
                {
                    yield return new ModifyDefenseValueModifierAction(card.iD.value, FocusGain(card.card.data.QualityModifier));
                }
            }
        }

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Before [b]equip[/b]:\n[b]Exert {exert}[/b]: gets +{focusGain} Focus";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("focusGain", FocusGain(quality)); yield return ("exert", Exert); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.ExertX;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Exert; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3;
    }

    public class ManaOrbCardBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: -1 Focus if top Arcana";
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.ManaOrb; }

        public override IEnumerable<IActionData>? OnEndTurnWhileEquipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var topmostArmor = CombatService.GetTopHeroArmor(context);

            if (topmostArmor != null && topmostArmor.iD.value == card.iD.value)
            {
                yield return new ModifyDefenseValueModifierAction(card.iD.value, -1);
                yield return new DelayAction(1); // HACK
                if (card.finalDefenseValue.value <= 0)
                {
                    yield return new RequestDropCardAction(card.iD.value, RequestDropReason.Used);
                }
            }
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier;

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class ThinkersHatActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ThinkersHatActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeEnemyAttackPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            var hero = context.GameOrSimContext.HeroEntity;
            if (hero == null || !hero.hasBoardLocation)
                yield break; // hero not found

            if (!self.hasCardArea || self.cardArea.Area != CardArea.HeroGearArmor)
                yield break; // arcana is not equipped

            var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as ThinkersHatBehavior;

            yield return new ActivateCardAction(self.iD.value);
            yield return new HurtTargetAction(hero.iD.value, behavior.HPLoss(self.card.data.QualityModifier), HurtSource.Armor, HurtType.Regular, true);
            yield return new DelayAction(40);
            yield return new DeactivateCardAction(self.iD.value);
            yield return new RequestDropCardAction(self.iD.value, RequestDropReason.ForceDrop);
            yield return new DelayAction(20);
        }
    }

    public class ThinkersHatBehavior : BasicMageArcana, IActiveEffectCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster attacks: [b]lose {hpLoss} HP[/b], [b]drop[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("hpLoss", HPLoss(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
        }

        public int HPLoss(int quality) => 3;

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier * 2;

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ThinkersHatActionInterceptor(e.iD.value) });
        }
    }


    public class ManaVialBelt : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: add 2 [b]{manaPotion}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaPotion", CardNames.Generate(ManaPotion(quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return ManaPotion(card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Potion; yield return CardTag.ManaOrb; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var manaPotionID = ManaPotion(card.card.data.QualityModifier);
            yield return new PermanentlyAddCardToDeckAction(manaPotionID);
            yield return new PermanentlyAddCardToDeckAction(manaPotionID);
            yield return new CreateCardInDrawPileStartingFromBoardAction(manaPotionID, hero.boardLocation.location, true);
            yield return new CreateCardInDrawPileStartingFromBoardAction(manaPotionID, hero.boardLocation.location, true);
        }

        public CardDataID ManaPotion(int quality) => new CardDataID("manaPotion", quality);

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;
    }

    public class StrangeRobeBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: gain [b]{num} Teleport Charges[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", Num(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.TeleportCharge;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Teleport;
        }

        public int Num(int quality) => 3 + quality;

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyTeleportCounterAction(hero.iD.value, Num(card.card.data.QualityModifier), false);
        }
    }

    public class MaticksRobeBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: place [b]{magicMissile}[/b] at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("magicMissile", CardNames.Generate(new CardDataID("magicMissile", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("magicMissile", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.MagicMissiles; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("magicMissile", card.card.data.QualityModifier), l, forceEthereal: true),
                        new DelayAction(10)
                    ))
            );
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3;
    }

    public class VandalsCapeBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: deal [b]12 damage[/b] to each Prop";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prop;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 2;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && CardUtils.GetCardType(e) == CardType.Prop,
                e => new HurtTargetAction(e.iD.value, 12, HurtSource.Armor, HurtType.Regular),
                e => new SequenceAction(
                    new TriggerSoundAction("heroAttack"),
                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                    new DelayAction(10)
                )
            )));
        }
    }
    

    public class SorcerersGloveBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: [b]flip[/b], then [b]prepare[/b] copies of 8-neighbor Spells";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Prepare;
            yield return KeywordID.EightNeighbor;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Spell; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {

            // flip
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => !e.isCardFlipped && CardUtils.GetCardType(e) == CardType.Spell,
                        e => new FlipCardAction(e.iD.value, true),
                        e => NoopAction.Instance,
                        requiresFlippedCard: false
                    )));

            // prepare
            var spells = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                .Select(l =>
                {
                    var c = context._GetCardsWithBoardLocation(l).FirstOrDefault();
                    if (c != null && c.isCardFlipped && CardUtils.GetCardType(c) == CardType.Spell)
                        return c;
                    return null;
                }).WhereNotNull()
                .ToList();

            if (spells.Any())
            {
                foreach (var spell in spells)
                {
                    if (spell.hasCard)
                    {
                        yield return new CreateCardOnHeroAction(spell.card.data, forceEthereal: true);
                        yield return new DelayAction(20);
                    }
                }
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;
    }


    public class BattleMageRobeBehavior : BasicMageArcana, IActiveEffectCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster attacks: [b]equip[/b] [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.ManaOrb;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 2;

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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeEnemyAttackPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null || !hero.hasBoardLocation)
                    yield break; // hero not found

                if (!self.hasCardArea || self.cardArea.Area != CardArea.HeroGearArmor)
                    yield break; // arcana is not equipped

                yield return new ActivateCardAction(self.iD.value);
                yield return new CreateCardOnHeroAction(new CardDataID("manaOrb"));
                yield return new DelayAction(20);
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class MoonstoneCircletBehavior : BasicMageArcana
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b]: [b]equip[/b] [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaOrb", CardNames.Generate(ManaOrb(quality))); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return ManaOrb(card.card.data.QualityModifier);
        }

        private CardDataID ManaOrb(int quality) => new CardDataID("manaOrb", quality);
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.ManaOrb;
        }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.AutoEquip;
        }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CreateCardOnHeroAction(ManaOrb(card.card.data.QualityModifier));
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1;
        public override bool IsAutoEquip() => true;
    }



    public class SpellcraftersGloveBehavior : BasicMageArcana, ICantrippingCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: [b]prepare[/b] Spell from Deck, [b]equip[/b] [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Prepare;
            yield return KeywordID.Spell;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Spell;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.ManaOrb;
        }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var spellInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
            if (spellInDrawPile != null)
                yield return new RequestEquipCardAction(spellInDrawPile.iD.value);

            yield return new CreateCardOnHeroAction(new CardDataID("manaOrb"));
            yield return new DelayAction(20);
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.Equip;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardUtils.GetCardType(candidate) == CardType.Spell;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }
}
