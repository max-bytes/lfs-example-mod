using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{

    public class ToArmsAuraCardBehavior : BasicAuraCardBehavior
    {
        private int AttackBuff(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Weapons have +{attackBuff} Attack while {threshold} or more are equipped";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attackBuff", AttackBuff(quality)); yield return ("threshold", Threshold(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyWeaponAttackIfStackIsLargeEnoughBuff(AttackBuff(card.card.data.QualityModifier), Threshold(card.card.data.QualityModifier), source);
        }

        private int Threshold(int quality) => 3;
    }

    public class ShadowPresenceBehavior : BasicAuraCardBehavior
    {
        private int Multiplier(int quality) => 2 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Weapons have +X x {multiplier} Attack, where X is Hero's [b]Stealth[/b] value";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("multiplier", Multiplier(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Stealth;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyWeaponAttackByStealthBuff(Multiplier(card.card.data.QualityModifier), source);
        }
    }

    public class VicariousShameActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public VicariousShameActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is EnemyAttackAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is EnemyAttackAction ea)
            {
                if (ea.attackValue > 0)
                    yield break;

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                    yield break; // aura is not on board and flipped

                yield return new ActivateCardAction(self.iD.value);

                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as VicariousShameBehavior;

                // self hurt
                var enemy = context.GameOrSimContext._GetEntityWithID(ea.enemyID);
                if (enemy != null && enemy.hasCardArea && enemy.cardArea.Area == CardArea.Board && enemy.isCardFlipped && enemy.hasBoardLocation)
                {
                    yield return new HurtTargetAction(enemy.iD.value, behavior.SelfDamage(self.card.data.QualityModifier), HurtSource.Aura, HurtType.Regular);
                }

                // hurt others
                var damages = BoardUtils.GetAll()
                    .OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy && e.iD.value != ea.enemyID,
                        e => new HurtTargetAction(e.iD.value, behavior.OtherDamage(self.card.data.QualityModifier), HurtSource.Aura, HurtType.Regular),
                        e => new SequenceAction(
                            new TriggerSoundAction("heroAttack"),
                            new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                            new DelayAction(10)
                        )
                    ));
                yield return new SequenceAction(damages);
                yield return new DelayAction(10);

                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }


    public class VicariousShameBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster attacks for 0: takes [b]{selfDamage} damage[/b], other Monsters take [b]{otherDamage} damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("selfDamage", SelfDamage(quality)); yield return ("otherDamage", OtherDamage(quality)); }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Disarm; }

        public int SelfDamage(int quality) => 12 + quality * 3;
        public int OtherDamage(int quality) => 3 + quality * 1;

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new VicariousShameActionInterceptor(e.iD.value) });
        }
    }

    public class StrongholdAuraCardBehavior : BasicAuraCardBehavior
    {
        private int AttackBuff(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Weapons have +{attackBuff} Attack while Hero is 8-neighbor";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attackBuff", AttackBuff(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (context.HeroEntity != null && context.HeroEntity.hasID)
                yield return new ModifyWeaponAttackIf8NeighborBuff(AttackBuff(card.card.data.QualityModifier), context.HeroEntity.iD.value, source);
        }
    }

    public class HoldAuraCardBehavior : BasicAuraCardBehavior
    {
        private int Buff(int quality) => 1 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Armor has\n+{defenseBuff} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("defenseBuff", Buff(quality)); }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyArmorDefenseBuff(Buff(card.card.data.QualityModifier), source);
        }
    }


    public class DefianceAuraActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public DefianceAuraActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeEnemyAttackPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            var hero = context.GameOrSimContext.HeroEntity;
            if (hero == null || !hero.hasBoardLocation)
                yield break; // hero not found

            if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                yield break; // aura is not on board and flipped

            if (!BoardUtils.Are8Neighbors(hero.boardLocation.location, self.boardLocation.location)) // not 8-neighbors
                yield break;

            var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as DefianceAuraCardBehavior;

            yield return new ActivateCardAction(self.iD.value);
            yield return new SequenceAction(Enumerable.Repeat(new CreateCardOnHeroAction(new CardDataID("armorPlate")), behavior.Buff(self.card.data.QualityModifier)));
            yield return new DeactivateCardAction(self.iD.value);
        }
    }

    public class DefianceAuraCardBehavior : BasicAuraCardBehavior
    {
        public int Buff(int quality) => 1 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "If Hero is 8-neighbor and Monster attacks: [b]equip[/b] {armorPlates} [b]Armor Plate[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("armorPlates", Buff(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("armorPlate");
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new DefianceAuraActionInterceptor(e.iD.value) });
        }
    }

    public class PoisonTheWellCardBehavior : BasicAuraCardBehavior
    {
        private int Buff(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]+{triggerPoison} Poison[/b] application";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("triggerPoison", Buff(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Poison; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new EverlastingPoisonBuff(Buff(card.card.data.QualityModifier), source);
        }
    }

    public class NightmareAuraCardBehavior : BasicAuraCardBehavior
    {
        private int MaxDamage(int quality) => quality + 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: cards take damage equal to their [b]Sleep[/b] ({maxDamage} max)";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("maxDamage", MaxDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sleep; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new NightmareBuff(MaxDamage(card.card.data.QualityModifier), source);
        }
    }


    public class CommonPenanceAuraCardBehavior : BasicAuraCardBehavior
    {
        private int Damage(int quality) => 2 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: each Sin deals [b]{damage} damage[/b] to 8-neighbor Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.Sins;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sin; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.FireDamage; }

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // find all sins
            var sins = context._GetCardsWithCardArea(CardArea.Board)
                .Where(c => c.hasBoardLocation && c.isCardFlipped && CardUtils.GetCardType(c) == CardType.Sin)
                .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location));

            // for each sin, find neighbor monsters and burn them
            if (sins.Any())
            {
                yield return new ActivateCardAction(card.iD.value);

                var damage = Damage(card.card.data.QualityModifier);

                foreach (var sin in sins)
                {
                    yield return new ActivateCardAction(sin.iD.value);
                    foreach (var l in BoardUtils.Get8Neighbors(sin.boardLocation.location).OrderBy(ll => BoardUtils.GetSortIndex(ll)))
                    {
                        yield return new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                        e => new HurtTargetAction(e.iD.value, damage, HurtSource.Aura, HurtType.Fire),
                        e => new SequenceAction(
                            new TriggerSoundAction("fire"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                            new DelayAction(14)
                        ));
                    }
                    if (sin != null && sin.hasID)
                        yield return new DeactivateCardAction(sin.iD.value);
                }
                if (card != null && card.hasID)
                    yield return new DeactivateCardAction(card.iD.value);
                yield return new DelayAction(20);
            }
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class VenomousShurikenAuraCardBehavior : BasicAuraCardBehavior
    {
        private int TriggerPoisonTimes(int quality) => 1 + quality;
        private int GetPoisonDuration(int quality) => 4 + quality * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            if (TriggerPoisonTimes(quality) == 1)
                return "[b]Shuriken[/b] apply [b]{poison} Poison[/b] to target and trigger [b]Poison[/b]";
            else
                return "[b]Shuriken[/b] apply [b]{poison} Poison[/b] to target and trigger [b]Poison {triggerTimes}x[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("poison", GetPoisonDuration(quality)); yield return ("triggerTimes", TriggerPoisonTimes(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("shuriken");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Shuriken; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.Poison; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ShurikenApplyPoisonBuff(GetPoisonDuration(card.card.data.QualityModifier), TriggerPoisonTimes(card.card.data.QualityModifier), source);
        }
    }

    public class GracefulExitAuraCardBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Exit Room[/b]: [b]equip[/b] {numArmor} Armor from Deck, deal half Defense to each Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("numArmor", NumArmor(quality)); }

        private int NumArmor(int quality) => 2 + quality;

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExitRoom;
            yield return KeywordID.Equip;
            yield return KeywordID.HolyDamage;
            yield return KeywordID.Rounding;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
            {
                yield return new ActivateCardAction(card.iD.value);

                var num = NumArmor(card.card.data.QualityModifier);
                for(var i = 0;i < num;i++)
                {
                    var armorInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, e => CardUtils.GetCardType(e) == CardType.Armor);
                    if (armorInDrawPile != null)
                        yield return new RequestEquipCardAction(armorInDrawPile.iD.value);
                }
                yield return new DelayAction(20);

                var damage = Mathf.FloorToInt(hero.defenseValue.value / 2);
                if (damage > 0)
                {
                    if (context._GetEnemyCardsOnBoard(true).Count > 0)
                    {
                        var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                                e => new HurtTargetAction(e.iD.value, damage, HurtSource.Aura, HurtType.Holy),
                                e => new SequenceAction(
                                    new TriggerSoundAction("holy"),
                                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                                    new DelayAction(5)
                                )
                            )).ToList();
                        yield return new SequenceAction(t);
                        yield return new DelayAction(60);
                    }
                }

                yield return new DeactivateCardAction(card.iD.value);
            }
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class BlessedHammerBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Before Monsters act[/b]: deal [b]{damage} damage[/b] to 4-neighbors if top Weapon is Hammer";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Hammer; }

        private int Damage(int quality) => 5 + quality * 2;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        public override IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (hero == null || !hero.hasBoardLocation || !hero.isCardFlipped)
                yield break;

            var topWeapon = CombatService.GetTopHeroWeapon(context, false);
            if (topWeapon == null)
                yield break;

            if (CardRepository.GenerateCardBehaviorFromData(topWeapon.card.data) is BasicHammerCardBehavior)
            {
                var heroLocation = hero.boardLocation.location;
                var neighborCardIDsAndLocations = context._GetCardsWithCardArea(CardArea.Board)
                    .Where(c => c.hasHealth && BoardUtils.Are4Neighbors(heroLocation, c.boardLocation.location) && c.isCardFlipped)
                    .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location))
                    .Select(c => (c.iD.value, c.boardLocation.location))
                    .ToList();

                if (neighborCardIDsAndLocations.Count > 0)
                {
                    var damage = Damage(card.card.data.QualityModifier);

                    yield return new ActivateCardAction(card.iD.value);

                    foreach (var (id, location) in neighborCardIDsAndLocations)
                    {
                        yield return new TriggerSoundAction("heroAttack");
                        yield return new TriggerAnimationAction(context._GetBoardLocationEntity(location).iD.value,
                            new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", BoardUtils.GetCardinalDirection(heroLocation, location).ToRotation() - Mathf.Pi / 2 } }));
                        yield return new HurtTargetAction(id, damage, HurtSource.Aura, HurtType.Regular);

                        yield return new DelayAction(20);
                    }
                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(card.iD.value);
                }
            }
        }
    }

    public class BreakMoraleActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public BreakMoraleActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterRemovedFromBoardPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterRemovedFromBoardPlaceholderAction m)
            {
                if (m.reason == RemoveFromBoardReason.EndOfRound)
                    yield break;
                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null)
                    yield break; // hero not found
                if (!hero.isCardFlipped) // hero must be present and still on board 
                    yield break;

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped)
                    yield break; // aura is not on board and flipped

                if (m.cardType != CardType.Monster && m.cardType != CardType.Boss)
                    yield break; // returned card is not a monster

                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as BreakMoraleAuraCardBehavior;

                yield return new ActivateCardAction(self.iD.value);
                yield return new HurtMonstersInDeckAction(behavior.Damage(self.card.data.QualityModifier));
                yield return new DelayAction(30);
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class BreakMoraleAuraCardBehavior : BasicAuraCardBehavior
    {
        public int Damage(int quality) => 4 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster [b]retreats[/b]: deal [b]{damage} Deck damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Retreat;
            yield return KeywordID.DeckDamage;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Retreat; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new BreakMoraleActionInterceptor(e.iD.value) });
        }
    }

    public class SuperconductorAuraCardBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lightning[/b] deals [b]1 less damage[/b], but triggers twice as often";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Lightning; }
        public override int MaxUpgrade => 0;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SuperconductorBuff(source);
        }
    }

    public class TriboElectricityBehavior : BasicAuraCardBehavior
    {
        public override int MaxUpgrade => 0;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Loot conducts Lightning, [b]Turn End[/b]: trigger [b]Lightning[/b] equal to Loot cards";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Loot;
            yield return KeywordID.LightningX;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.Lightning; }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Loot; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriboElectricityBuff(source);
        }
    }

    public class SheerTenacityCardBehavior : BasicAuraCardBehavior
    {
        public int AdditionalExerts(int quality) => 2 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Exert[/b] is checked {amount} additional times";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("amount", AdditionalExerts(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExertX;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Exert; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new AdditionalExertsBuff(AdditionalExerts(card.card.data.QualityModifier), source);
        }
    }

    public class UnstableEnergyBehavior : BasicAuraCardBehavior
    {
        public int Damage(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Destroy [b]Mana Orb[/b]: trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.LightningX;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.ManaOrb; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.Lightning; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterCardDestroyedPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is AfterCardDestroyedPlaceholderAction ad && ad.CardID.BaseID == "manaOrb")
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board

                    var hero = context.GameOrSimContext.HeroEntity;
                    if (hero == null || !hero.hasBoardLocation)
                        yield break;

                    var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as UnstableEnergyBehavior;

                    yield return new ActivateCardAction(self.iD.value);

                    foreach (var la in CombatService.TriggerLightning(hero, selfBehavior.Damage(self.card.data.QualityModifier), context.GameOrSimContext, HurtSource.Aura, hero))
                        yield return la;
                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class CorrosiveSorceryBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: -1 Monster Defense, [b]equip {manaOrb}[/b] if Defense was reduced";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("manaOrb", CardNames.Generate(ManaOrb(quality), lang)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return ManaOrb(card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.ManaOrb; }

        private CardDataID ManaOrb(int quality) => new CardDataID("manaOrb", quality);

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var candidates = context._GetCardsWithCardArea(CardArea.Board)
                .Where(c => c.hasBoardLocation && c.isCardFlipped && c.hasFinalDefenseValue && c.finalDefenseValue.value > 0 && c.isEnemy)
                .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location))
                .ToList();

            if (candidates.Count > 0)
            {
                yield return new ActivateCardAction(card.iD.value);

                foreach(var c in candidates)
                {
                    yield return new AffectCardAndLocationAction(c.boardLocation.location, false, (e) => true,
                    e => new ModifyDefenseValueModifierAction(c.iD.value, -1),
                    e => new SequenceAction(
                        new TriggerAnimationAction(c.iD.value, new AnimationTrigger("debuff")),
                        new TriggerSoundAction("debuff"),
                        new DelayAction(14)
                    ));
                }

                yield return new CreateCardOnHeroAction(ManaOrb(card.card.data.QualityModifier));

                yield return new DeactivateCardAction(card.iD.value);
            }
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class ForceMultiplierBehaviorActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ForceMultiplierBehaviorActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is OverkillMonsterPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is OverkillMonsterPlaceholderAction oma)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                    yield break; // aura is not on board
                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as ForceMultiplierBehavior;

                // overkill threshold check
                if (oma.Amount >= behavior.Overkill(self.card.data.QualityModifier))
                { // met threshold

                    // modify top weapon
                    var topWeapon = CombatService.GetTopHeroWeapon(context.GameOrSimContext, false);
                    if (topWeapon != null)
                    {
                        yield return new ActivateCardAction(self.iD.value);

                        yield return new ModifyStickyOffsetAction(topWeapon.iD.value, 1);
                        yield return new ModifyAttackValueModifierAction(topWeapon.iD.value, -behavior.AttackDecrease(self.card.data.QualityModifier));
                        yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                        yield return new TriggerAnimationAction(topWeapon.iD.value, new AnimationTrigger("upgrade"));

                        yield return new DelayAction(30);
                        yield return new DeactivateCardAction(self.iD.value);
                    }
                }
            }
        }
    }

    public class ForceMultiplierBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Overkill {overkill}[/b]: top Weapon gets +1 Use and {attackDecrease} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("overkill", Overkill(quality)); yield return ("attackDecrease", -AttackDecrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.MultipleWeaponUses;
            yield return KeywordID.OverkillX;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; } 
        // TODO: requires tag "high attack"?

        public int Overkill(int quality) => 8 - quality * 2;
        public int AttackDecrease(int quality) => 8 - quality * 2;
        public override int MaxUpgrade => 3;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ForceMultiplierBehaviorActionInterceptor(e.iD.value) });
        }
    }

    // TODO: remove eventually
    public class TripwiresBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster trying to walk onto occupied spot is [b]pushed for {damage}[/b] instead";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.Push; yield return CardTag.Retreat; }

        public int Damage(int quality) => 2 + quality;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TripwiresBuff(Damage(card.card.data.QualityModifier), source);
        }
    }

    public class ZapActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ZapActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterTeleportPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterTeleportPlaceholderAction ta)
            {
                var target = context.GameOrSimContext._GetEntityWithID(ta.EntityID);
                if (target == null)
                    yield break; // target not found
                if (!target.isHero)
                    yield break; // not hero, who moved
                if (!target.hasBoardLocation)
                    yield break; // hero not on board

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                    yield break; // aura is not on board
                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as ZapBehavior;

                yield return new ActivateCardAction(self.iD.value);

                // make action quick
                yield return new SetQuickTurnAction(true);

                // flip 4-neighbors
                var t = context.GameOrSimContext._GetCardsWithCardArea(CardArea.Board)
                    .Where(c => c.hasBoardLocation && !c.isCardFlipped && c.isEnemy && BoardUtils.Are4Neighbors(target.boardLocation.location, c.boardLocation.location))
                    .OrderBy(t => BoardUtils.GetSortIndex(t.boardLocation.location)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                    .Select(t => new FlipCardAction(t.iD.value, true));
                yield return ActionService.CreateStaggeredParallelAction(t, 10);

                PathService.RecalculateReachableVia(context.GameOrSimContext); // HACK: to make newly appearing card not grey

                // trigger lightning
                foreach (var la in CombatService.TriggerLightning(target, behavior.Damage(self.card.data.QualityModifier), context.GameOrSimContext, HurtSource.Aura, target))
                    yield return la;
                yield return new DelayAction(20);

                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class ZapBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Teleport gains: [b]quick[/b], [b]flip[/b] 4-neighbor Monsters, trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
        }
        
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Aura; yield return CardTag.Lightning; }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Teleport; }

        public int Damage(int quality) => 3 + quality;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ZapActionInterceptor(e.iD.value) });
        }
    }

    public class NivsPresenceBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Card drawn from Hero Deck: deal [b]{damage} damage[/b] to Hero's 8-neighbors";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.FireDamage;
        }

        public int Damage(int quality) => 6 + quality * 2;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterCardDrawnPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is AfterCardDrawnPlaceholderAction dca)
                {
                    if (dca.fromPile != CardArea.HeroDrawPile)
                        yield break; // card not draw from hero pile

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board
                    if (context.GameOrSimContext.HeroEntity == null || !context.GameOrSimContext.HeroEntity.hasBoardLocation)
                        yield break; // no hero on board
                    var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as NivsPresenceBehavior;

                    // damage
                    var damage = behavior.Damage(self.card.data.QualityModifier);
                    yield return new ActivateCardAction(self.iD.value);

                    foreach (var l in BoardUtils.Get8Neighbors(context.GameOrSimContext.HeroEntity.boardLocation.location).OrderBy(ll => BoardUtils.GetSortIndex(ll)))
                    {
                        yield return new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                        e => new HurtTargetAction(e.iD.value, damage, HurtSource.Aura, HurtType.Fire),
                        e => new SequenceAction(
                            new TriggerSoundAction("fire"),
                            new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                            new DelayAction(14)
                        ));
                    }

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class RenunciationBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Drop[/b] Spell without casting: deal [b]{damage} damage[/b] to each Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Spell;
            yield return KeywordID.HolyDamage;
        }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Spell; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public int Damage(int quality) => 3 + quality;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeCardDropPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is BeforeCardDropPlaceholderAction rda)
                {
                    if (rda.reason != RequestDropReason.ForceDrop)
                        yield break; // card not draw from hero pile

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board
                    var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as RenunciationBehavior;

                    var droppedCard = context.GameOrSimContext._GetEntityWithID(rda.cardID);
                    if (droppedCard == null)
                        yield break;
                    if (!(CardRepository.GenerateCardBehaviorFromData(droppedCard.card.data) is BasicSpellCardBehavior))
                        yield break;

                    // damage
                    yield return new ActivateCardAction(self.iD.value);
                    var damage = selfBehavior.Damage(self.card.data.QualityModifier);
                    var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Aura, HurtType.Holy),
                            e => new SequenceAction(
                                new TriggerSoundAction("holy"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                                new DelayAction(10)
                            )
                        ));
                    yield return new SequenceAction(damages);
                    yield return new DelayAction(10);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class OrionsBlessingBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip Bow[/b]: it gets +1 Use and {attackReduction} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attackReduction", -AttackReduction(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.MultipleWeaponUses;
        }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Bow; }

        public int AttackReduction(int quality) => 2;

        public override int MaxUpgrade => 0;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board

                    var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                    if (equippedCard == null || !equippedCard.hasCard)
                        yield break;

                    if (CardUtils.GetCardType(equippedCard) != CardType.Weapon)
                        yield break;

                    if (!(CardRepository.GenerateCardBehaviorFromData(equippedCard.card.data) is BasicBowCardBehavior))
                        yield break;

                    var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as OrionsBlessingBehavior;

                    yield return new ActivateCardAction(self.iD.value);
                    yield return new ModifyStickyOffsetAction(equippedCard.iD.value, 1);
                    yield return new ModifyAttackValueModifierAction(equippedCard.iD.value, -selfBehavior.AttackReduction(self.card.data.QualityModifier));
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(equippedCard.iD.value, new AnimationTrigger("upgrade"));
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class SoaringShieldsBehavior : BasicAuraCardBehavior
    {
        public static int Multiplier(int quality) => 2 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Unarmed Hero can throw top Shield, dealing {multiplier}x Defense damage";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("multiplier", Multiplier(quality)); }

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("soaringShield", card.card.data.QualityModifier);
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Shield; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SoaringShieldsBuff(card.card.data.QualityModifier, source);
        }
    }


    public class SalvagingArmsBehavior : BasicAuraCardBehavior
    {
        public override int MaxUpgrade => 5;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "If Weapon [b]drops[/b], move it to the bottom instead and take [b]{damage} damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", SalvagingArmsBuff.SelfDamage(quality)); }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Weapon;
            yield return KeywordID.Drop;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SalvagingArmsBuff(card.card.data.QualityModifier, source);
        }
    }

    public class KindlingFlameBehavior : BasicAuraCardBehavior
    {
        public override int MaxUpgrade => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Spells and Scrolls deal [b]double fire damage[/b] while there is a [b]{lingeringFlame}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"), lang)); }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.LingeringFlame;
            yield return CardTag.FireDamage;
            // TODO: spells or scrolls, cannot currently implement as we'd need an OR
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Spell;
            yield return KeywordID.Scroll;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new KindlingFlameBuff(source);
        }
    }

    public class StokeTheFlamesBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]{lingeringFlame}[/b] gains: place copies with +{timer} Timer at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"), lang)); yield return ("timer", PlusTimer(quality)); }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.LingeringFlame; }

        public int PlusTimer(int quality) => 1 + quality;

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeActivateContraptionPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is BeforeActivateContraptionPlaceholderAction ra)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board

                    var target = context.GameOrSimContext._GetEntityWithID(ra.contraptionID);
                    if (target == null || !target.hasCard || !target.hasBoardLocation)
                        yield break;

                    if (!(CardRepository.GenerateCardBehaviorFromData(target.card.data) is LingeringFlameCardBehavior))
                        yield break;

                    var targetBoardLocation = target.boardLocation.location;

                    var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as StokeTheFlamesBehavior;

                    yield return new ActivateCardAction(self.iD.value);

                    var emptyLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, targetBoardLocation))
                        .Where(l => !context.GameOrSimContext._GetCardsWithBoardLocation(l).Any())
                        .ToList();
                    yield return new SequenceAction(emptyLocations
                        .Select(l => new SequenceAction(
                                            new TriggerSoundAction("cardCreateOnBoard"),
                                            new CreateCardAtBoardLocationAction(new CardDataID("lingeringFlame"), l, flipUp: true, followUp: (card) =>
                                            {
                                                var channelling = (target.hasChannelling ? target.channelling.value : 0) + selfBehavior.PlusTimer(self.card.data.QualityModifier);
                                                card.ReplaceChannelling(channelling);
                                                return NoopAction.Instance;
                                            }),
                                            new DelayAction(10))));

                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class HolyFireBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Dealing [b]X fire damage[/b] also deals [b]X+{increase} holy damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("increase", Increase(quality)); }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.FireDamage; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.HolyDamage;
        }

        public int Increase(int quality) => 1 + quality;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is HurtTargetAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is HurtTargetAction ha)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                        yield break; // aura is not on board

                    if (ha.amount <= 0)
                        yield break;
                    if (ha.hurtType != HurtType.Fire)
                        yield break;

                    var target = context.GameOrSimContext._GetEntityWithID(ha.targetID);
                    if (target == null || !target.hasCard || !target.hasID)
                        yield break;

                    var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as HolyFireBehavior;
                    var amount = ha.amount + selfBehavior.Increase(self.card.data.QualityModifier);

                    yield return new DelayAction(1);

                    // HACK: check again
                    target = context.GameOrSimContext._GetEntityWithID(ha.targetID);
                    if (target == null || !target.hasCard || !target.hasID)
                        yield break;
                    var targetID = target.iD.value;
                    var boardLocation = target.hasBoardLocation ? (BoardLocation?)target.boardLocation.location : null;

                    yield return new DelayAction(20);

                    yield return new ActivateCardAction(self.iD.value);

                    yield return new TriggerSoundAction("holy");
                    if (boardLocation.HasValue)
                        yield return new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(boardLocation.Value).iD.value, new AnimationTrigger("holy"));
                    yield return new HurtTargetAction(targetID, amount, HurtSource.Aura, HurtType.Holy);
                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class PushbackBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster attacks: [b]push[/b] Hero's 4-neighbor Monsters [b]for {push}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("push", Push(quality)); }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Push; }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
            yield return KeywordID.FourNeighbor;
        }

        public int Push(int quality) => 3 + quality;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeEnemyAttackPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.isCardFlipped || !self.hasBoardLocation)
                    yield break; // aura is not on board

                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null || !hero.hasBoardLocation)
                    yield break;

                var selfBehavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as PushbackBehavior;

                yield return new ActivateCardAction(self.iD.value);

                yield return new DelayAction(20);

                var heroLocation = hero.boardLocation.location;
                yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, heroLocation))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                            e => new PushAction(e.iD.value, selfBehavior.Push(self.card.data.QualityModifier), BoardUtils.GetCardinalOrDiagonalDirection(heroLocation, l)),
                            e => NoopAction.Instance,
                            requiresFlippedCard: false
                        )));

                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class SpreadingPlagueBehavior : BasicAuraCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply [b]{poison} Poison[/b] to [b]pushed[/b] Monsters, Poison affects Monsters in Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("poison", Poison(quality)); }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Poison; }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Push; }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
            yield return KeywordID.Push;
        }

        public int Poison(int quality) => 6 + quality * 2;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SpreadingPlagueBuff(Poison(card.card.data.QualityModifier), source);
        }
    }
}
