using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Core.Card
{
    public class BodyArmorCardBehavior : BasicArmorCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 3;
    }

    public class WornBreastplateCardBehavior : BasicArmorCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 5;
        public override int MaxUpgrade => 0;
    }

    public class ArmorPlateCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class HelmetCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 2;
    }

    public class LeatherCapCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }
        public override int MaxUpgrade => 0;

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;
    }

    public class CapOfHealingCardBehavior : BasicArmorCardBehavior
    {
        private int Heal(int quality) => 1 + quality;
        public override bool IsQuickEquip => true;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: [b]heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Heal;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var healBy = Heal(card.card.data.QualityModifier);
            yield return new HealTargetAction(hero.iD.value, healBy, true);
        }
    }


    public class HornedHoodCardBehavior : BasicArmorCardBehavior
    {
        private int Damage(int quality) => 4 + quality * 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: 8-neighbor Monsters flip, take [b]{damage} Damage[/b] and [b]retreat[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.Retreat;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Retreat; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var heroLocation = context.HeroEntity.boardLocation.location;

            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => !e.isCardFlipped && e.isEnemy,
                        e => new FlipCardAction(e.iD.value, true),
                        e => NoopAction.Instance,
                        requiresFlippedCard: false
                    )));

            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                        e => new HurtTargetAction(e.iD.value, Damage(card.card.data.QualityModifier), HurtSource.Armor, HurtType.Regular),
                        e => NoopAction.Instance,
                        requiresFlippedCard: true
                    )));

            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                        e => new TryToRemoveFromBoardAction(e.iD.value, RemoveFromBoardReason.Retreat),
                        e => NoopAction.Instance,
                        requiresFlippedCard: true
                    )));
        }
    }

    public class PauldronArmorCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.AutoEquip;
        }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;
        public override bool IsAutoEquip() => true;
    }

    public class BootsCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            if (isEthereal)
                return "";
            else
                return "[b]Equip[/b]: [b]equip[/b] an [b]ethereal[/b] copy of this card";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Ethereal;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.isEthereal)
                yield return new CreateCardOnHeroAction(card.card.data, forceEthereal: true);
        }
    }

    public class PushInBootsCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;

        private int Push(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: [b]push[/b] 8-neighbor Monsters [b]for {push}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("push", Push(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.Equip;
            yield return KeywordID.QuickEquip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Push; yield return CardTag.Retreat; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 2;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var heroLocation = context.HeroEntity.boardLocation.location;
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                        e => new PushAction(e.iD.value, Push(card.card.data.QualityModifier), BoardUtils.GetCardinalOrDiagonalDirection(heroLocation, l)),
                        e => NoopAction.Instance,
                        requiresFlippedCard: false
                    )));
        }
    }

    public class GauntletCardBehavior : BasicArmorCardBehavior, ICantrippingCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: [b]equip[/b] Weapon from Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Weapon;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var weaponCardInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
            if (weaponCardInDrawPile != null)
                yield return new RequestEquipCardAction(weaponCardInDrawPile.iD.value);
            else
                yield return new FizzleAction(hero.iD.value);
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.Equip;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardUtils.GetCardType(candidate) == CardType.Weapon;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }

    public class HeavyHandCardBehavior : BasicArmorCardBehavior
    {
        private int AdditionalAttack(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: top Weapon gets +{additionalAttack} Attack, add [b]Sloth[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("additionalAttack", AdditionalAttack(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Weapon;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("sloth");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var weapon = CombatService.GetTopHeroWeapon(context, false);
            if (weapon != null)
                yield return new IncreaseAttackAction(weapon.iD.value, AdditionalAttack(card.card.data.QualityModifier), true);
            yield return new CreateCardInDrawPileStartingFromBoardAction(new CardDataID("sloth"), hero.boardLocation.location, true); // add sloth also when no weapon equipped
        }
    }

    public class SpikedBodyArmorCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Equip[/b]: deal Hero's Defense as damage to 4-neighbor cards";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.SlowEquip;
            yield return KeywordID.Equip;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 5 + card.card.data.QualityModifier * 2;

        public override bool IsSlowEquip => true;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var finalDefenseValue = hero.hasDefenseValue ? hero.defenseValue.value : 0;
            var heroLocation = context.HeroEntity.boardLocation.location;

            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                        e => new HurtTargetAction(e.iD.value, finalDefenseValue, HurtSource.Armor, HurtType.Regular),
                        e =>
                        {
                            var locationDiff = BoardUtils.GetVectorDistance(heroLocation, l);
                            var direction = locationDiff.AngleTo(new Vector2(1, 0));
                            return new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", direction } })),
                                new DelayAction(10)
                            );
                        }
                    ));

            yield return new SequenceAction(t);
        }
    }

    public class BagoasBeltArmorCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;

        private CardDataID PotionOfPoison(int quality) => new CardDataID("potionOfPoison", quality);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: add 2 [b]{potionOfPoison}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("potionOfPoison", CardNames.Generate(PotionOfPoison(quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Poison; yield return CardTag.Potion; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return PotionOfPoison(card.card.data.QualityModifier);
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var potionID = PotionOfPoison(card.card.data.QualityModifier);
            for (var i = 0; i < 2; i++)
            {
                yield return new PermanentlyAddCardToDeckAction(potionID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(potionID, hero.boardLocation.location, true);
            }
        }
    }



    public class SoporificBeltArmorCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;

        private CardDataID PotionOfSleep(int quality) => new CardDataID("potionOfSleep", quality);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: add 2 [b]{potionOfSleep}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("potionOfSleep", CardNames.Generate(PotionOfSleep(quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; yield return CardTag.Potion; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return PotionOfSleep(card.card.data.QualityModifier);
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var potionID = PotionOfSleep(card.card.data.QualityModifier);
            for (var i = 0; i < 2; i++)
            {
                yield return new PermanentlyAddCardToDeckAction(potionID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(potionID, hero.boardLocation.location, true);
            }
        }
    }


    public class QuartermastersBeltBehavior : BasicArmorCardBehavior
    {
        private CardDataID OilOfSharpness(int quality) => new CardDataID("oilOfSharpness", Mathf.FloorToInt(quality / 2f));
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: add 2 [b]{oilOfSharpness}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("oilOfSharpness", CardNames.Generate(OilOfSharpness(quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Potion; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return OilOfSharpness(card.card.data.QualityModifier);
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var potionID = OilOfSharpness(card.card.data.QualityModifier);
            for (var i = 0; i < 2; i++)
            {
                yield return new PermanentlyAddCardToDeckAction(potionID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(potionID, hero.boardLocation.location, true);
            }
        }
    }

    public class PoorJestersTopArmorCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: apply [b]{sleep} Sleep[/b]\nto each Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("sleep", Sleep(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;
        private int Sleep(int quality) => 3 + quality;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth && e.isEnemy,
                        e => new SequenceAction(new TrySleepAction(e.iD.value, Sleep(card.card.data.QualityModifier)), new DelayAction(10)),
                        e => NoopAction.Instance
                    ));

            yield return new SequenceAction(t);
        }
    }

    public class JestersBottomsBehavior : BasicArmorCardBehavior, IActiveEffectCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: -1 Defense, each Monster gets {disarm} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("disarm", -Disarm(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Disarm; }

        private int Disarm(int quality) => 1;

        public override IEnumerable<IActionData>? OnEndTurnWhileEquipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyDefenseValueModifierAction(card.iD.value, -1);
            yield return new DelayAction(1); // HACK

            // disarm
            var targetedEnemies = context._GetEnemyCardsOnBoard(true);
            yield return new SequenceAction(targetedEnemies.Select(e => e.boardLocation.location).OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                    e => new DisarmAction(e.iD.value, Disarm(card.card.data.QualityModifier)),
                    e => NoopAction.Instance
                    )
                ));

            if (card.finalDefenseValue.value <= 0)
            {
                yield return new RequestDropCardAction(card.iD.value, RequestDropReason.Used);
            }
        }
    }

    public class ReactiveArmorCardBehavior : BasicArmorCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier;

        public int ArmorPerEquip(int quality) => 1 + Mathf.FloorToInt(quality / 2f);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "While [b]Equipped[/b]: +{armorPerEquip} Defense whenever a Weapon is equipped";// After [b]Equip[/b]: +{AdditionalArmor} armor if weapon stack is larger than armor stack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("armorPerEquip", ArmorPerEquip(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
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
                    if (!self.hasCardArea || !((self.cardArea.Area == CardArea.HeroGearArmor)))
                        yield break;

                    var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                    if (equippedCard == null)
                        yield break;

                    if (CardUtils.GetCardType(equippedCard) != CardType.Weapon)
                        yield break;

                    var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as ReactiveArmorCardBehavior;
                    yield return new ModifyDefenseValueModifierAction(cardID, behavior.ArmorPerEquip(self.card.data.QualityModifier));
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(self.iD.value, new AnimationTrigger("upgrade"));
                    yield return new DelayAction(5);
                }
            }
        }
    }


    public class PlagueDoctorCapeBehavior : BasicArmorCardBehavior, IActiveEffectCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier;

        public int PoisonPerEquip(int quality) => 1 + Mathf.FloorToInt(quality / 2f);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equipped[/b]: apply [b]{poison} Poison[/b] to each Monster when a Weapon is [b]equipped[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("poison", PoisonPerEquip(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Poison;
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
                    if (!self.hasCardArea || !((self.cardArea.Area == CardArea.HeroGearArmor)))
                        yield break;

                    var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                    if (equippedCard == null)
                        yield break;

                    if (CardUtils.GetCardType(equippedCard) != CardType.Weapon)
                        yield break;

                    var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as PlagueDoctorCapeBehavior;

                    yield return new ActivateCardAction(self.iD.value);
                    yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                                e => new ApplyPoisonAction(e.iD.value, behavior.PoisonPerEquip(self.card.data.QualityModifier)),
                                e => new SequenceAction(
                                    new TriggerSoundAction("poison"),
                                    new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("poison")),
                                    new DelayAction(10)
                                )
                            )));
                    if (self.hasID)
                        yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class BrokenShieldArmorCardBehavior : BasicShieldCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3;
        public override int MaxUpgrade => 0;
    }

    public class ClericsShieldCardBehavior : BasicShieldCardBehavior
    {
        private int Heal(int quality) => 1 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: [b]heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Heal;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var healBy = Heal(card.card.data.QualityModifier);
            yield return new HealTargetAction(hero.iD.value, healBy, true);
        }
    }

    // TODO: remove, eventually
    public class GhostShieldBehavior : BasicShieldCardBehavior, IActiveEffectCardBehavior
    {
        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: [b]drop[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
        }

        public override IEnumerable<IActionData> OnEndTurnWhileEquipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new RequestDropCardAction(card.iD.value, RequestDropReason.ForceDrop);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }
    public class WoodenShieldCardBehavior : BasicShieldCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: +{additionalArmor} Defense if it is the only equipped Shield";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("additionalArmor", AdditionalArmor(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier;

        public int AdditionalArmor(int quality) => 4 + quality * 3;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var numShieldCards = context._GetCardsWithCardArea(CardArea.HeroGearArmor).Where(c => CardRepository.GenerateCardBehaviorFromData(c.card.data) is BasicShieldCardBehavior).Count();
            if (numShieldCards <= 1)
            {
                yield return new ModifyDefenseValueModifierAction(card.iD.value, AdditionalArmor(card.card.data.QualityModifier));
                yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("upgrade"));
            } else
            {
                yield return new FizzleAction(card.iD.value);
            }
        }
    }

    public class SpikedShieldBehavior : BasicShieldCardBehavior, IActiveEffectCardBehavior
    {
        private int Damage(int quality) => 4 + quality * 1;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose Defense[/b]: deal [b]{damage} damage[/b] to 4-neighbor cards";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Shield; yield return CardTag.Disarm; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterDefend(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var heroLocation = context.HeroEntity.boardLocation.location;
            var finalDamage = Damage(card.card.data.QualityModifier);

            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, heroLocation))
                    .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                        e => new HurtTargetAction(e.iD.value, finalDamage, HurtSource.Armor, HurtType.Regular),
                        e =>
                        {
                            var locationDiff = BoardUtils.GetVectorDistance(heroLocation, l);
                            var direction = locationDiff.AngleTo(new Vector2(1, 0));
                            return new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", direction } })),
                                new DelayAction(10)
                            );
                        }
                    ));

            yield return new SequenceAction(t);

            foreach (var a in base.OnAfterDefend(hero, card, context))
                yield return a;
        }
    }

    public class BucklerBehavior : BasicShieldCardBehavior, IActiveEffectCardBehavior
    {
        private int Disarm(int quality) => 2 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose Defense[/b]: 8-neighbor Monsters get {disarm} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("disarm", -Disarm(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Shield; yield return CardTag.Disarm; }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterDefend(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                        e => new SequenceAction(new DisarmAction(e.iD.value, Disarm(card.card.data.QualityModifier)), new DelayAction(20)),
                        e => NoopAction.Instance
                    )));

            foreach (var a in base.OnAfterDefend(hero, card, context))
                yield return a;
        }
    }

    public class IceShieldCardBehavior : BasicShieldCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Sturdy[/b]\n[b]Turn End[/b]: -1 Defense";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sturdy;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => GetRegularDefenseValue(card.card.data.QualityModifier);

        public static int GetRegularDefenseValue(int quality) => 3 + quality;

        public override int? TryGetResolvedDefenseValue(IGameOrSimEntity entity, IGameOrSimContext context)
        {
            return GetSimpleDefenseValue(entity);
        }

        public override IActionData OnDefend(int blockedDamage, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            return NoopAction.Instance;
        }

        public override IEnumerable<IActionData>? OnEndTurnWhileEquipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyDefenseValueModifierAction(card.iD.value, -1);
            yield return new DelayAction(1); // HACK
            if (card.finalDefenseValue.value <= 0)
            {
                yield return new RequestDropCardAction(card.iD.value, RequestDropReason.Used);
            }
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
        }
    }


    public class GeldkatzeCardBehavior : BasicArmorCardBehavior
    {
        private int Gain(int quality) => 10 + quality * 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: gain [b]{gain} Emeralds[/b], add [b]Greed[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("gain", Gain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("greed");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var gain = Gain(card.card.data.QualityModifier);
            var pos = ScreenPositionUtils.CalculateBoardPositionScreen(hero.boardLocation.location);
            yield return new CollectGoldAction(gain, pos);

            yield return new CreateCardInDrawPileStartingFromBoardAction(new CardDataID("greed"), hero.boardLocation.location, true);
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;
    }

    public class MajesticArmorCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: place [b]Envy[/b] at 4-neighbor spots";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("envy");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("envy"), l),
                        new DelayAction(10)
                    ))
            );
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 3;
    }


    public class BrewmastersTunicBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: place [b]Barrels[/b] at 4-neighbor spots";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("barrel");
        }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("barrel"), l),
                        new DelayAction(10)
                    ))
            );
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 2;
    }

    public class AchillesArmorBehavior : BasicArmorCardBehavior
    {
        private int TimerStart => 5;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: start [b]Timer {time}[/b], [b]Timer[/b]: [b]lose 3 HP[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("time", TimerStart); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.Equip;
        }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, TimerStart, true);
        }

        public override IActionData OnDropRequest(IGameOrSimEntity card, RequestDropReason reason, IGameOrSimContext context)
        {
            if (card.hasChannelling)
                card.RemoveChannelling();

            return base.OnDropRequest(card, reason, context);
        }

        public override IEnumerable<IActionData> OnEndTurnWhileEquipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasChannelling)
            {
                if (card.channelling.value > 1)
                {
                    yield return new ModifyChannelingAction(card.iD.value, -1, false);
                } else if (card.channelling.value <= 1)
                {
                    card.RemoveChannelling();
                    var lostHP = 3;
                    yield return new ActivateCardAction(card.iD.value);
                    yield return new HurtTargetAction(hero.iD.value, lostHP, HurtSource.Armor, HurtType.Regular, true);
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(card.iD.value);
                }
            }
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 3;
    }

    public class PocketlessCloakArmorCardBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: place [b]{shuriken}[/b] at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("shuriken", CardNames.Generate(new CardDataID("shuriken", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("shuriken", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Shuriken; yield return CardTag.Ranged; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("shuriken", card.card.data.QualityModifier), l, forceEthereal: true),
                        new DelayAction(10)
                    ))
            );
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2;
    }

    public class StealthCloakBehavior : BasicArmorCardBehavior
    {
        private int Stealth(int quality) => 4 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: gain [b]{stealth} Stealth[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("stealth", Stealth(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Stealth;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Stealth;
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyStealthAction(hero.iD.value, Stealth(card.card.data.QualityModifier), false);
            yield return new TriggerSoundAction("stealth");
            yield return new TriggerAnimationAction(context._GetBoardLocationEntity(hero.boardLocation.location).iD.value, new AnimationTrigger("stealth"));
            yield return new DelayAction(10);
        }
    }

    public class EirenesCrownArmorCardBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;

        private int Heal(int quality) => 2 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: [b]heal {heal}[/b], place [b]Desires[/b] at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("desire");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Heal; yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var healBy = Heal(card.card.data.QualityModifier);
            yield return new HealTargetAction(hero.iD.value, healBy, true);

            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("desire"), l),
                        new DelayAction(10)
                    ))
            );
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 1;
    }

    public class WingsOfHypnosBehavior : BasicArmorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: place [b]{lesserHeal}[/b] at 4-neighbor spots, [b]apply {sleep} Sleep[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("lesserHeal", CardNames.Generate(new CardDataID("lesserHeal"))); yield return ("sleep", Sleep(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lesserHeal");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Heal; }

        public int Sleep(int quality) => 4 + quality;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("lesserHeal"), l, forceEthereal: true),
                        new DelayAction(10)
                    ))
            );

            yield return new TrySleepAction(hero.iD.value, Sleep(card.card.data.QualityModifier));
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;
    }


    public class OutriderHelmetBehavior : BasicArmorCardBehavior
    {
        public override bool IsQuickEquip => true;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Equip[/b]: [b]flip[/b] every Armor in room, they get +{defenseIncrease} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("defenseIncrease", DefenseIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
            yield return KeywordID.Equip;
        }

        public int DefenseIncrease(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var armor = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l =>
                {
                    var c = context._GetCardsWithBoardLocation(l).FirstOrDefault();
                    if (c != null && CardUtils.GetCardType(c) == CardType.Armor)
                        return c;
                    return null;
                }).WhereNotNull()
                .ToList();

            if (armor.Any())
            {
                foreach (var a in armor)
                {
                    if (a.hasCard && a.hasID)
                    {
                        if (!a.isCardFlipped)
                            yield return new FlipCardAction(a.iD.value, true);

                        yield return new ModifyDefenseValueModifierAction(a.iD.value, DefenseIncrease(card.card.data.QualityModifier));
                        yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                        yield return new TriggerAnimationAction(a.iD.value, new AnimationTrigger("upgrade"));
                        yield return new DelayAction(10);
                    }
                }
            }
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        protected override int GetSimpleDefenseValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;
    }
}