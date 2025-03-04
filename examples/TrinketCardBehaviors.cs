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
    public class ThickSkinTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Armor and Monsters have +2 Defense";

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new ModifyArmorDefenseBuff(2, source);
            yield return new ModifyEnemyArmorBuff(2, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class FrenzyTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Weapons and Monsters have +2 Attack";

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new ModifyWeaponAttackBuff(2, source);
            yield return new ModifyEnemyAttackBuff(2, source);
        }

        public override bool CanGetMultiple() => true;
    }

    // TODO: remove
    public class EverlastingPoisonTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]+3 Poison[/b] application";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Poison; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new EverlastingPoisonBuff(3, source);
        }

        public override bool CanGetMultiple() => true;
    }


    public class RingOfPoisonTrinketCardBehavior : BasicTrinketCardBehavior
    {
        private int Buff() => 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Turn End[/b]: trigger [b]Poison[/b] {triggerPoison} additional times";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("triggerPoison", Buff()); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Poison; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new ModifyPoisonStrengthBuff(Buff(), source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class MedusasRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy Prop[/b]: apply [b]3 Poison[/b] to Monsters, trigger [b]Poison[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
            yield return KeywordID.Prop;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Poison; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PoisonousPropsBuff(3, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class SwordArtisanTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Swords have +2 Attack";
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sword; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new SwordArtisanBuff(2, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class BowArtisanTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Bows have +2 Attack and gain [b]Quick Equip[/b]";
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Bow; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickEquip;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new BowArtisanBuff(2, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class HammerArtisanTrinketCardBehavior : BasicTrinketCardBehavior
    {
        private int Attack => 6;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hammers have +{attack} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attack", Attack); }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Hammer; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new HammerArtisanBuff(Attack, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class StaffArtisanTrinketCardBehavior : BasicTrinketCardBehavior
    {
        private int Attack => 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Staffs have +{attack} Attack and gain [b]equip[/b]: [b]equip[/b] [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attack", Attack); yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"), lang)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.ManaOrb; }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Staff; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new StaffArtisanBuff(Attack, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class DaggerArtisanTrinketCardBehavior : BasicTrinketCardBehavior
    {
        private int Attack => 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Daggers have +{attack} Attack and gain [b]Ranged Attack[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attack", Attack); }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Dagger; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new DaggerArtisanBuff(Attack, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class RoadrunnersRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Immune[/b] to damage from Props, +3 Prop damage";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Immune;
            yield return KeywordID.Prop;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new HeroImmunePropDamageBuff(source);
            yield return new PropDamageBuff(3, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class SleepoverTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When a Monster is [b]pushed for X[/b], apply [b]X Sleep[/b] to it";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Push; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PushingAppliesSleepBuff(source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class PierceRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Your ranged attacks have [b]Piercing Projectiles[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.PiercingProjectiles;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Ranged; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PiercingBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class PunctureRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Your Weapons ignore the target's armor";

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PunctureBuff(source);
        }
        public override bool CanGetMultiple() => false;
    }

    public class FawkesPendantTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "All Barrels are [b]X-Barrels[/b], +3 Prop damage";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prop;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("xBarrel");
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AllBarrelsAreXBarrelsBuff(source);
            yield return new PropDamageBuff(3, source);
        }
        public override bool CanGetMultiple() => true;
    }

    public class PropMastersRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        //public override string GenerateBaseDescriptionEN(int quality) => "Attacking Props does not cause your Weapon to [b]drop[/b]";
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Attacking Props does not reduce your Weapon's use counter";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Prop;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AttackingPropsDoesNotDropWeaponBuff(source);
        }
        public override bool CanGetMultiple() => false;
    }



    public class SinnersMightTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public SinnersMightTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        private IEnumerable<IActionData> Hurt(ActionExecutionContext context)
        {
            var damagePer = 1;
            if (context.GameOrSimContext._GetEnemyCardsOnBoard(true).Any())
            {
                var damages = BoardUtils.GetAll()
                    .OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                        e => new SequenceAction(
                            new TriggerSoundAction("holy"),
                            new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                            new HurtTargetAction(e.iD.value, damagePer, HurtSource.Trinket, HurtType.Holy),
                            new DelayAction(10)
                            ),
                        e => NoopAction.Instance
                    ));
                yield return new SequenceAction(damages);
                yield return new DelayAction(20);
            }
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is FlipCardAction || a is AtoneSinPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is FlipCardAction fa)
            {
                if (!fa.flip)
                    yield break; // card is not getting flipped up
                var flippedCard = context.GameOrSimContext._GetEntityWithID(fa.entityID);
                if (flippedCard == null)
                    yield break; // cannot find flipped card
                if (CardUtils.GetCardType(flippedCard) != CardType.Sin)
                    yield break; // flipped card is not a sin

                yield return new ActivateCardAction(self.iD.value);
                foreach(var h in Hurt(context))
                    yield return h;
                yield return new DeactivateCardAction(self.iD.value);
            } else if (a is AtoneSinPlaceholderAction)
            {

                yield return new ActivateCardAction(self.iD.value);
                foreach (var h in Hurt(context))
                    yield return h;
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class SinnersMightTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When a Sin is flipped or atoned for, deal [b]1 damage[/b] to each Monster";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sins;
            yield return KeywordID.HolyDamage;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sin; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new SinnersMightTrinketActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class RingOfFortitudeActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public RingOfFortitudeActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is FlipCardAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is FlipCardAction fa)
            {
                if (!fa.flip)
                    yield break; // card is not flipped up
                var flippedCard = context.GameOrSimContext._GetEntityWithID(fa.entityID);
                if (flippedCard == null)
                    yield break; // cannot find flipped card
                if (CardUtils.GetCardType(flippedCard) != CardType.Armor)
                    yield break; // flipped card is not armor
                if (!self.hasID)
                    yield break;

                yield return new ActivateCardAction(self.iD.value);
                yield return new CreateCardOnHeroAction(new CardDataID("rustySword"), true, forceEthereal: true);
                yield return new DelayAction(30);
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }


    public class RingOfFortitudeCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Armor card is flipped: [b]equip[/b] [b]Rusty Sword[/b] last in stack";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Armor;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("rustySword");
        }
        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new RingOfFortitudeActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class BristleRingTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public BristleRingTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is ModifyHealthAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is ModifyHealthAction mha)
            {
                if (mha.d >= 0)
                    yield break; // health is not modified negatively
                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null)
                    yield break; // hero not found
                if (mha.entityID != hero.iD.value)
                    yield break; // hero not affected

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break; // trinket is not in trinket area

                var damage = 2;

                var targetedEnemies = context.GameOrSimContext._GetEnemyCardsOnBoard(true);
                if (targetedEnemies.Any())
                {
                    IActionData damages = new SequenceAction(targetedEnemies
                        .Where(e => e.hasBoardLocation)
                        .Select(e => e.boardLocation.location)
                        .OrderBy(l => BoardUtils.GetSortIndex(l)
                        )
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Trinket, HurtType.Regular),
                            e =>
                            {
                                var locationDiff = BoardUtils.GetVectorDistance(hero.boardLocation.location, l);
                                var direction = locationDiff.AngleTo(new Vector2(1, 0));
                                return new SequenceAction(
                                    new DelayAction(12),
                                    new TriggerSoundAction("heroAttack"),
                                    new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", direction } }))
                                );
                            }
                        )));

                    yield return new ActivateCardAction(self.iD.value);
                    yield return damages;
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class BristleRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When you [b]lose HP[/b], deal [b]2 damage[/b] to each Monster";

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new BristleRingTrinketActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class RingOfHealthTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Exit Room[/b]:\n[b]heal 2[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExitRoom;
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Heal; }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ActivateCardAction(card.iD.value);
            yield return new HealTargetAction(hero.iD.value, 2, true);
            yield return new DelayAction(30);
            yield return new DeactivateCardAction(card.iD.value);
        }

        public override bool CanGetMultiple() => true;
    }

    public class HotheadLocketTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public HotheadLocketTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AtoneSinPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AtoneSinPlaceholderAction asa)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break;

                var damagePer = 1;

                if (context.GameOrSimContext._GetEnemyCardsOnBoard(true).Any())
                {
                    var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                            e => new HurtTargetAction(e.iD.value, damagePer, HurtSource.Trinket, HurtType.Regular),
                            e => new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                                new DelayAction(10)
                            )
                        ));
                    yield return new ActivateCardAction(self.iD.value);
                    yield return new SequenceAction(damages);
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class HotheadLocketTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "All Sins are [b]Rashness[/b]. Atone Sin: [b]1 damage[/b] to each Monster";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sins;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("rashness");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sin; }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new HotheadLocketTrinketActionInterceptor(e.iD.value) });

            // TODO: transform existing sins
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AllSinsAreRashnessBuff(source);
        }

        public override bool CanGetMultiple() => true;
    }

    // TODO: remove
    public class CatnapRingTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public CatnapRingTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is MoveCardOnBoardAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is MoveCardOnBoardAction ma)
            {
                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null)
                    yield break; // hero not found
                if (ma.moveReason == MoveReason.Walk)
                    yield break; // wrong move reason

                var movedEntity = context.GameOrSimContext._GetEntityWithID(ma.entityID);
                if (movedEntity == null)
                    yield break; // moved entity does not exist anymore
                if (!movedEntity.isEnemy)
                    yield break; // moved entity is not an enemy

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break; // trinket is not in trinket area

                yield return new ActivateCardAction(self.iD.value);
                yield return new TrySleepAction(ma.entityID, 2);
                yield return new DelayAction(15);
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class CatnapRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every [b]{turns}th Turn[/b] of a room, apply [b]1 Sleep[/b] to all Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("turns", Turns);  }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        public override bool CanGetMultiple() => false;

        public int Turns => 8;

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!hero.hasHeroRoomState || hero.heroRoomState.state != HeroRoomState.InRoom)
                yield break;
            if (!card.hasTrinketCounter)
                yield break;

            card.ReplaceTrinketCounter(card.trinketCounter.counter - 1);
            if (card.trinketCounter.counter <= 0)
            {
                var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth && e.isEnemy,
                            e => new SequenceAction(new TrySleepAction(e.iD.value, 1), new DelayAction(10)),
                            e => NoopAction.Instance
                        ));

                yield return new ActivateCardAction(card.iD.value);
                yield return new SequenceAction(t);
                yield return new DelayAction(10);
                yield return new DeactivateCardAction(card.iD.value);

                card.ReplaceTrinketCounter(Turns);
            }
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(Turns);
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasCardArea && card.cardArea.Area == CardArea.Trinkets)
                card.ReplaceTrinketCounter(Turns);
            yield break;
        }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasTrinketCounter)
                card.RemoveTrinketCounter();
            yield break;
        }
    }

    public class BhimasRingTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public BhimasRingTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is BeforeHeroAttackPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            var hero = context.GameOrSimContext.HeroEntity;
            if (hero == null)
                yield break; // hero not found

            //var weapon = CombatService.GetCurrentHeroWeapon(context.GameOrSimContext);
            //if (weapon == null)
            //    return;
            // HACK, TODO: do we not need to look at the weapon? is the hero's attack always the same as the currently equipped weapon's attack value?
            if (hero.attackValue.value >= 14)
            {
                yield return new ActivateCardAction(self.iD.value);
                yield return new CreateCardOnHeroAction(new CardDataID("armorPlate"));
                yield return new CreateCardOnHeroAction(new CardDataID("armorPlate"));
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class BhimasRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When you attack with 14 or more, [b]equip[/b] 2 [b]Armor Plates[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("armorPlate");
        }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new BhimasRingTrinketActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class ArthursPendantTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ArthursPendantTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is RequestEquipCardAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is RequestEquipCardAction ra)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break;

                var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                if (equippedCard == null)
                    yield break;

                if (CardUtils.GetCardType(equippedCard) != CardType.Armor)
                    yield break;

                var damagePer = 1;

                if (context.GameOrSimContext._GetEnemyCardsOnBoard(true).Any())
                {
                    var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                            e => new HurtTargetAction(e.iD.value, damagePer, HurtSource.Trinket, HurtType.Regular),
                            e => new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                                new DelayAction(10)
                            )
                        ));
                    yield return new ActivateCardAction(self.iD.value);
                    yield return new SequenceAction(damages);
                    yield return new DelayAction(10);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class ArthursPendantTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] Armor: deal [b]1 damage[/b] to each Monster";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ArthursPendantTrinketActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class ExtortionAmuletTrinketActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ExtortionAmuletTrinketActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is ModifyHealthAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is ModifyHealthAction mha)
            {
                if (mha.d < 0)
                    yield break; // entity is not healed

                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break;

                var affectedEntity = context.GameOrSimContext._GetEntityWithID(mha.entityID);
                if (affectedEntity == null)
                    yield break; // healed entity does not exist anymore
                if (!affectedEntity.isHero)
                    yield break; // healed entity is not hero

                var damage = mha.d;

                var targetedEnemies = context.GameOrSimContext._GetEnemyCardsOnBoard(true);
                if (targetedEnemies.Any())
                {
                    IActionData damages = new SequenceAction(targetedEnemies
                        .Where(e => e.hasBoardLocation)
                        .Select(e => e.boardLocation.location)
                        .OrderBy(l => BoardUtils.GetSortIndex(l)
                        )
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Trinket, HurtType.Regular),
                            e =>
                            {
                                var locationDiff = BoardUtils.GetVectorDistance(affectedEntity.boardLocation.location, l);
                                var direction = locationDiff.AngleTo(new Vector2(1, 0));
                                return new SequenceAction(
                                    new DelayAction(12),
                                    new TriggerSoundAction("heroAttack"),
                                    new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", direction } }))
                                );
                            }
                        )));

                    yield return new ActivateCardAction(self.iD.value);
                    yield return damages;
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class ExtortionAmuletTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When you [b]heal[/b], deal that much to each Monster";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Heal; }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ExtortionAmuletTrinketActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class TheMetronomeTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Your Hero is allowed to skip their turn";

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new WaitingAllowedBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class SubtleBodyCharmCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Deactivate[/b] Aura: [b]swap place[/b] with it instead";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Aura;
            yield return KeywordID.DeactivateAura;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Aura; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AuraRepositionBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class RapidFireRingTrinketCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Magic Missiles[/b] deal +1 damage and gain [b]Quick Cast[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickCast;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.MagicMissiles; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new MagicMissileDamageBuff(1, source);
            yield return new MagicMissileQuickCastBuff(source);
        }

        public override bool CanGetMultiple() => true;
    }

    // TODO: remove ventually
    public class EyeOfTheStormCardBehavior : BasicTrinketCardBehavior
    {
        //public override string GenerateDescription(IGameOrSimEntity card, IGameOrSimContext context) => "Lightning Orbs gain: take damage: [b]immune, +1 timer[/b]";
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lightning Orbs[/b] gain: [b]immune[/b], [b]+1 timer[/b] instead";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Immune;
            yield return KeywordID.Timer;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lightningOrb");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.LightningOrb; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new EyeOfTheStormBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }


    public class TheEternalFlameCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Card destroyed by fire: place a [b]Lingering Flame[/b] at its spot";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.FireDamage; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.LingeringFlame; yield return CardTag.FireDamage; }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
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
                if (a is AfterCardDestroyedPlaceholderAction ata)
                {
                    if (!ata.LastBoardLocation.HasValue)
                        yield break;
                    //if (ata.cardType != CardType.Prop && ata.cardType != CardType.Monster)
                    //    yield break;
                    if (ata.reason != DestroyCardReason.Fire)
                        yield break;

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    if (context.GameOrSimContext._GetCardsWithBoardLocation(ata.LastBoardLocation.Value).Any())
                        yield break;

                    yield return new ActivateCardAction(self.iD.value);

                    yield return new TriggerSoundAction("cardCreateOnBoard");
                    yield return new CreateCardAtBoardLocationAction(new CardDataID("lingeringFlame"), ata.LastBoardLocation.Value, flipUp: true);
                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }

        public override bool CanGetMultiple() => false;
    }


    public class ArchmagesCharmActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ArchmagesCharmActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is CastScrollPlaceholderAction || a is CastSpellPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                yield break;

            var currentArmor = CombatService.GetTopHeroArmor(context.GameOrSimContext);
            if (currentArmor == null)
                yield break; // no armor equipped

            yield return new ActivateCardAction(self.iD.value);
            yield return new ModifyDefenseValueModifierAction(currentArmor.iD.value, 1);
            yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
            yield return new TriggerAnimationAction(currentArmor.iD.value, new AnimationTrigger("upgrade"));
            yield return new DelayAction(15);
            yield return new DeactivateCardAction(self.iD.value);
        }
    }

    public class ArchmagesCharmCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "When a Spell or Scroll is cast, top Arcana gets [b]+1 Focus[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Scroll;
            yield return KeywordID.Spell;
            yield return KeywordID.Arcana;
        }
        //public override IEnumerable<CardTag> RequiredTags() { yield return CardTag.Spell; yield return CardTag.Scroll; } // TODO: cannot implement or, leave it out for now

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ArchmagesCharmActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }


    public class ThothsPendantActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ThothsPendantActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is SuccessfulExertPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                yield break;

            var hero = context.GameOrSimContext.HeroEntity;
            if (hero == null)
                yield break;

            yield return new ActivateCardAction(self.iD.value);

            // heal
            yield return new HealTargetAction(hero.iD.value, 1, true);

            // damage
            var targetedEnemies = context.GameOrSimContext._GetEnemyCardsOnBoard(true);
            if (targetedEnemies.Any())
            {
                var damage = 2;
                IActionData damages = new SequenceAction(targetedEnemies
                    .Where(e => e.hasBoardLocation)
                    .Select(e => e.boardLocation.location)
                    .OrderBy(l => BoardUtils.GetSortIndex(l)
                    )
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                        e => new HurtTargetAction(e.iD.value, damage, HurtSource.Trinket, HurtType.Regular),
                        e => new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                                new DelayAction(5)
                            )
                    )));
                yield return damages;

                yield return new DelayAction(20);
            }

            yield return new DeactivateCardAction(self.iD.value);
        }
    }


    public class ThothsPendantBehavior : BasicTrinketCardBehavior
    {
        private int Heal => 1;
        private int Damage => 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Successful [b]exert[/b]: [b]heal {heal}[/b] and deal [b]{damage} damage[/b] to each Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("heal", Heal); yield return ("damage", Damage); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
            yield return KeywordID.ExertX;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Exert; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Heal; }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new ThothsPendantActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class PhoenixCharmActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public PhoenixCharmActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is FailedExertPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                yield break;

            var hero = context.GameOrSimContext.HeroEntity;
            if (hero == null)
                yield break;

            yield return new ActivateCardAction(self.iD.value);
            yield return new CreateCardOnHeroAction(new CardDataID("manaOrb"));
            yield return new DeactivateCardAction(self.iD.value);
        }
    }
    public class PhoenixCharmBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] a [b]{manaOrb}[/b] whenever [b]exert[/b] fails";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"), lang)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.ExertX;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Exert; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.ManaOrb; }

        protected override void OnActivate(IGameOrSimEntity e)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new PhoenixCharmActionInterceptor(e.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class PocketWormholeCardBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Enter room[/b]: gain [b]1 Teleport Charge[/b]";

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Teleport; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.TeleportCharge;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasCardArea && card.cardArea.Area == CardArea.Trinkets)
            {
                yield return new ActivateCardAction(card.iD.value);
                yield return new ModifyTeleportCounterAction(hero.iD.value, 1, false);
                yield return new DelayAction(20);
                yield return new DeactivateCardAction(card.iD.value);
            }
        }

        public override bool CanGetMultiple() => true;
    }


    public class AugustinsCharmActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public AugustinsCharmActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is PoisonTriggeredPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                yield break;

            yield return new ActivateCardAction(self.iD.value);

            self.ReplaceTrinketCounter(self.trinketCounter.counter - 1);

            var disarm = 1;

            if (self.trinketCounter.counter <= 0)
            {
                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as AugustinsCharmBehavior;

                var targetedEnemies = context.GameOrSimContext._GetEnemyCardsOnBoard(true);
                yield return new SequenceAction(targetedEnemies.Select(e => e.boardLocation.location).OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                    e => new SequenceAction(
                        new DisarmAction(e.iD.value, disarm),
                        new DelayAction(10)
                        ),
                    e => NoopAction.Instance
                    )
                ));

                self.ReplaceTrinketCounter(behavior.NumPoison);
            }

            yield return new DeactivateCardAction(self.iD.value);
        }
    }
    public class AugustinsCharmBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every {numPoison}th time [b]poison triggers[/b]: all Monsters get -1 Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("numPoison", NumPoison); }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Poison; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Disarm; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }

        public int NumPoison => 4;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(NumPoison);
            card.AddActionInterceptorSource(new IActionInterceptor[] { new AugustinsCharmActionInterceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => false;
    }

    public class MindOverMatterBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster attacks ignore your Focus, but their damage is halved";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Rounding;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new MindOverMatterBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }


    public class BensKeyBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lightning[/b] damage +{additionalDamage}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("additionalDamage", 2); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Lightning; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new HeroLightningDamageBuff(2, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class StolenLocketBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quest Item[/b]\nHero can teleport to exit. Breaks after use";
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Teleport; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new StolenLocketBuff(source);
        }

        public IEnumerable<IActionData> OnAfterTeleport(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            yield return new PermanentlyRemoveCardFromDeckAction(card.card.data);
        }

        public override bool CanGetMultiple() => false;
    }



    public class SwordTradersRingActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public SwordTradersRingActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is ModifyAttackValueModifierAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is ModifyAttackValueModifierAction ma)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break;

                var target = context.GameOrSimContext._GetEntityWithID(ma.entityID);
                if (target == null || !target.isEnemy || !target.hasCardArea || target.cardArea.Area != CardArea.Board)
                    yield break;

                yield return new DelayAction(1); // HACK: to ensure that final attack value is recalculated

                if (!target.hasFinalAttackValue || target.finalAttackValue.value >= 0 || !target.hasAttackValue || target.attackValue.value <= 0) // monster must have base attack value, otherwise it has no attack (like shaman)
                    yield break;

                var weaponCardInDrawPile = CombatService.GetTopCardInPile(context.GameOrSimContext, CardArea.HeroDrawPile, e => CardUtils.GetCardType(e) == CardType.Weapon);
                if (weaponCardInDrawPile != null)
                {
                    yield return new ActivateCardAction(self.iD.value);

                    yield return new RequestEquipCardAction(weaponCardInDrawPile.iD.value, true);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class SwordTradersRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster's Attack is reduced below 0: [b]equip[/b] Weapon from Deck last in stack";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Disarm; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new SwordTradersRingActionInterceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => false;
    }

    public class IntimidationRingActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public IntimidationRingActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is OverkillMonsterPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is OverkillMonsterPlaceholderAction oma)
            {
                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null)
                    yield break;
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break; // trinket is not in trinket area
                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as IntimidationRingBehavior;

                var newNumber = self.trinketCounter.counter - oma.Amount;
                self.ReplaceTrinketCounter(newNumber);

                // counter check
                while (self.trinketCounter.counter <= 0)
                { // met threshold
                    self.ReplaceTrinketCounter(self.trinketCounter.counter + behavior.Amount); // NOTE: important to do this first, to make nested triggers not reduce this again

                    yield return new ActivateCardAction(self.iD.value);

                    var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                            e => new HurtTargetAction(e.iD.value, behavior.Damage, HurtSource.Trinket, HurtType.Regular),
                            e => new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                                new DelayAction(10)
                            )
                        ));
                    yield return new SequenceAction(damages);
                    yield return new DelayAction(10);

                    yield return new HealTargetAction(hero.iD.value, behavior.Heal, true);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class IntimidationRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every {amount} [b]Overkill[/b] damage: deal [b]{damage} damage[/b] to all Monsters and [b]heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("amount", Amount); yield return ("damage", Damage); yield return ("heal", Heal); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.OverkillX;
            yield return KeywordID.Heal;
        }

        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Heal;
        }

        public int Amount => 20;
        public int Damage => 6;
        public int Heal => 3;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(Amount);
            card.AddActionInterceptorSource(new IActionInterceptor[] { new IntimidationRingActionInterceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class TheBlinkFruitBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => $"[b]Teleport[/b] onto Spells instead of moving, Spells have [b]Eager[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Spell;
            yield return KeywordID.Eager;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Spell;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Teleport; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new TheBlinkFruitBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class AuramancersAmuletBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => $"Auras have [b]Eager[/b] and [b]Topdeck[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Aura;
            yield return KeywordID.Eager;
            yield return KeywordID.Rigged;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Aura;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AuramancersAmuletBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class CeridwensEarringBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => $"When a Potion is flipped up, its effect is triggered";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Potion;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Potion;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new CeridwensEarringBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class RingOfPyromaniaBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => $"Monster takes [b]6+ fire damage[/b]: deal [b]3 damage[/b] to other Monsters";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.FireDamage;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => true;

        public class Interceptor : IActionInterceptor
        {
            public readonly EntityID cardID;

            public Interceptor(EntityID cardID)
            {
                this.cardID = cardID;
            }

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterLoseHPPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is AfterLoseHPPlaceholderAction ma)
                {
                    if (ma.HurtType != HurtType.Fire)
                        yield break;
                    if (ma.Amount < 6)
                        yield break;

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;
                    var target = context.GameOrSimContext._GetEntityWithID(ma.EntityID);
                    if (target == null)
                        yield break;
                    if (!target.isEnemy)
                        yield break;

                    var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy && e.iD.value != ma.EntityID,
                            e => new HurtTargetAction(e.iD.value, 3, HurtSource.Trinket, HurtType.Fire),
                            e => new SequenceAction(
                                new TriggerSoundAction("fire"),
                                new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                new DelayAction(10)
                            )
                        ));
                    yield return new ActivateCardAction(self.iD.value);

                    yield return new SequenceAction(damages);

                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class RingOfPatienceBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every [b]{turns}th turn[/b] of a room, deal [b]{damage} damage[/b] to all Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("turns", Turns); yield return ("damage", Damage); }

        public int Turns => 18;
        public int Damage => 12;

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!hero.hasHeroRoomState || hero.heroRoomState.state != HeroRoomState.InRoom)
                yield break;
            if (!card.hasTrinketCounter)
                yield break;

            card.ReplaceTrinketCounter(card.trinketCounter.counter - 1);
            if (card.trinketCounter.counter <= 0)
            {
                var damages = BoardUtils.GetAll()
                        .OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                            e => new HurtTargetAction(e.iD.value, Damage, HurtSource.Trinket, HurtType.Regular),
                            e => new SequenceAction(
                                new TriggerSoundAction("heroAttack"),
                                new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                                new DelayAction(10)
                            )
                        ));
                yield return new ActivateCardAction(card.iD.value);
                yield return new SequenceAction(damages);
                yield return new DelayAction(10);
                yield return new DeactivateCardAction(card.iD.value);

                card.ReplaceTrinketCounter(Turns);
            }
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(Turns);
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasCardArea && card.cardArea.Area == CardArea.Trinkets)
            {
                card.ReplaceTrinketCounter(Turns);
            }
            yield break;
        }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasTrinketCounter)
                card.RemoveTrinketCounter();
            yield break;
        }

        public override bool CanGetMultiple() => true;
    }


    public class ChandrasEssenceActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public ChandrasEssenceActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterLoseHPPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterLoseHPPlaceholderAction hta)
            {
                if (hta.HurtType != HurtType.Fire)
                    yield break;
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break; // trinket is not in trinket area
                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as ChandrasEssenceBehavior;

                var newNumber = self.trinketCounter.counter - hta.Amount;
                self.ReplaceTrinketCounter(newNumber);

                // met threshold
                while (self.trinketCounter.counter <= 0)
                {
                    // NOTE: important to do this first, to make nested triggers not reduce this again
                    self.ReplaceTrinketCounter(self.trinketCounter.counter + behavior.Threshold);

                    yield return new ActivateCardAction(self.iD.value);

                    yield return new DelayAction(30);

                    //var targetLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l));
                    //yield return new SequenceAction(targetLocations
                    //    .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth && !e.isHero,
                    //        e => new HurtTargetAction(e.iD.value, behavior.Damage, HurtSource.Trinket, HurtType.Fire),
                    //        e => new SequenceAction(
                    //            e.boardLocation.location.Col == 0 ? new TriggerSoundAction(new SoundTrigger("fire")) : (IActionData)NoopAction.Instance,
                    //            new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                    //            new DelayAction(3)
                    //        )
                    //    )));
                    //    yield return new DelayAction(30);

                    yield return new CreateCardOnHeroAction(behavior.FlameDance, forceEthereal: true);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class ChandrasEssenceBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every [b]{threshold} fire damage[/b] dealt: [b]prepare {flameDance}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("threshold", Threshold); yield return ("flameDance", CardNames.Generate(FlameDance, lang)); }
        //public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        //{
        //    yield return KeywordID.FireDamage; // TODO: fire damage?
        //}
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.FireDamage;
        }
        public CardDataID FlameDance => new CardDataID("flameDance", 1);
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return FlameDance;
        }

        public int Threshold => 40;
        public int Damage => 6;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(Threshold);
            card.AddActionInterceptorSource(new IActionInterceptor[] { new ChandrasEssenceActionInterceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class FlashHealRingActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public FlashHealRingActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterTeleportPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterTeleportPlaceholderAction ma)
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                    yield break;

                var target = context.GameOrSimContext._GetEntityWithID(ma.EntityID);
                if (target == null)
                    yield break;
                if (!target.isHero)
                    yield break;

                var hero = context.GameOrSimContext.HeroEntity;
                if (hero == null)
                    yield break;

                yield return new ActivateCardAction(self.iD.value);
                yield return new HealTargetAction(hero.iD.value, 1, true);
                yield return new CreateCardOnHeroAction(new CardDataID("manaOrb"));
                yield return new DeactivateCardAction(self.iD.value);
            }
        }
    }

    public class FlashHealRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "After teleport: [b]heal 1[/b], [b]equip[/b] a [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"), lang)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Teleport; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.ManaOrb; yield return CardTag.Heal; }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new FlashHealRingActionInterceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => true;
    }

    public class TearOfNyxBehavior : BasicTrinketCardBehavior
    {
        private CardDataID StealthPotion() => new CardDataID("stealthPotion");
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("stealthPotion", CardNames.Generate(StealthPotion(), lang)); }
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Enter room[/b]: add [b]{stealthPotion}[/b] to Deck";

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Stealth; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return StealthPotion();
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasCardArea && card.cardArea.Area == CardArea.Trinkets)
            {
                yield return new ActivateCardAction(card.iD.value);

                var potionID = StealthPotion();
                yield return new PermanentlyAddCardToDeckAction(potionID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(potionID, hero.boardLocation.location, true); // TODO: create from trinket position instead?
                yield return new DelayAction(20);

                yield return new DeactivateCardAction(card.iD.value);
            }
        }

        public override bool CanGetMultiple() => true;
    }

    public class NinjutsuPendantBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Exit Stealth[/b]: [b]push[/b] 4-neighbor Monsters [b]for 3[/b]";

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Stealth; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Push; yield return CardTag.Retreat; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => false;

        public class Interceptor : IActionInterceptor
        {
            public readonly EntityID cardID;

            public Interceptor(EntityID cardID)
            {
                this.cardID = cardID;
            }

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is ExitStealthPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is ExitStealthPlaceholderAction ma)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    var hero = context.GameOrSimContext.HeroEntity;
                    if (hero == null || !hero.hasBoardLocation)
                        yield break;

                    yield return new ActivateCardAction(self.iD.value);

                    var heroLocation = hero.boardLocation.location;
                    yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                            .Where(l => BoardUtils.Are4Neighbors(l, heroLocation))
                            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                                e => new PushAction(e.iD.value, 3, BoardUtils.GetCardinalDirection(heroLocation, l)),
                                e => NoopAction.Instance,
                                requiresFlippedCard: false
                            )));

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class TeslasPartingGiftBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "After teleport: place [b]{lightningRod}[/b] at source";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("lightningRod", CardNames.Generate(new CardDataID("lightningRod"), lang)); }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Teleport; yield return CardTag.Lightning; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lightningRod");
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => false;

        public class Interceptor : IActionInterceptor
        {
            public readonly EntityID cardID;

            public Interceptor(EntityID cardID)
            {
                this.cardID = cardID;
            }

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterTeleportPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is AfterTeleportPlaceholderAction ata)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    var target = context.GameOrSimContext._GetEntityWithID(ata.EntityID);
                    if (target == null)
                        yield break;
                    if (!target.isHero)
                        yield break;

                    if (!context.GameOrSimContext._GetCardsWithBoardLocation(ata.locationBeforeTeleport).Any())
                    {
                        yield return new ActivateCardAction(self.iD.value);

                        // TODO: animation, sound
                        yield return new CreateCardAtBoardLocationAction(new CardDataID("lightningRod"), ata.locationBeforeTeleport, true);
                        yield return new DelayAction(30);

                        yield return new DeactivateCardAction(self.iD.value);
                    }
                }
            }
        }
    }

    public class EquivalenceRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy Prop[/b]: [b]prepare[/b] [b]{magicMissile}[/b]";

        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("magicMissile", CardNames.Generate(new CardDataID("magicMissile", quality), lang)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.MagicMissiles; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prepare;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("magicMissile");
        }

        public override bool CanGetMultiple() => true;

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
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
                if (a is AfterCardDestroyedPlaceholderAction ata)
                {
                    if (!ata.LastBoardLocation.HasValue)
                        yield break;
                    if (ata.cardType != CardType.Prop)
                        yield break;
                    if (ata.reason == DestroyCardReason.EndOfRoom)
                        yield break;

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    yield return new ActivateCardAction(self.iD.value);

                    var quality = 0;
                    yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);
                    yield return new DelayAction(30);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class LuckyCharmBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy Prop[/b]: place [b]Spiked Emerald[/b] at its spot";

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Loot; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prop;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("spikedEmerald");
        }

        public override bool CanGetMultiple() => false;

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
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
                if (a is AfterCardDestroyedPlaceholderAction ata)
                {
                    if (!ata.LastBoardLocation.HasValue)
                        yield break;
                    if (ata.cardType != CardType.Prop)
                        yield break;
                    if (ata.reason == DestroyCardReason.EndOfRoom)
                        yield break;

                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    if (context.GameOrSimContext._GetCardsWithBoardLocation(ata.LastBoardLocation.Value).Any())
                        yield break;

                    yield return new ActivateCardAction(self.iD.value);

                    yield return new TriggerSoundAction("cardCreateOnBoard");
                    yield return new CreateCardAtBoardLocationAction(new CardDataID("spikedEmerald"), ata.LastBoardLocation.Value, true);
                    yield return new DelayAction(20);

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }


    public class OgiersCollarBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every [b]{threshold} damage[/b] blocked by Armor: [b]equip[/b] Weapon from Deck last in stack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("threshold", Threshold); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Armor;
            yield return KeywordID.Equip;
        }

        public int Threshold => 20;

        public override bool CanGetMultiple() => false;

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(Threshold);
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public class Interceptor : IActionInterceptor
        {
            public readonly EntityID cardID;

            public Interceptor(EntityID cardID)
            {
                this.cardID = cardID;
            }

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is HeroBlocksWithArmorAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is HeroBlocksWithArmorAction ata)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    var newNumber = self.trinketCounter.counter - ata.blockedDamage;
                    self.ReplaceTrinketCounter(newNumber);

                    while (self.trinketCounter.counter <= 0)
                    {
                        var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as OgiersCollarBehavior;
                        self.ReplaceTrinketCounter(self.trinketCounter.counter + behavior.Threshold); // NOTE: important to do this first, to make nested triggers not reduce this again

                        yield return new ActivateCardAction(self.iD.value);

                        var weaponCardInDrawPile = CombatService.GetTopCardInPile(context.GameOrSimContext, CardArea.HeroDrawPile, (candidate) => CardUtils.GetCardType(candidate) == CardType.Weapon);
                        if (weaponCardInDrawPile != null)
                            yield return new RequestEquipCardAction(weaponCardInDrawPile.iD.value, true);
                        yield return new DelayAction(20);

                        yield return new DeactivateCardAction(self.iD.value);
                    }

                }
            }
        }
    }

    public class MagneticRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Armor gains [b]Omni-Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.OmniEquip;
            yield return KeywordID.Armor;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new ArmorOmniEquipBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class NonMagneticRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Arcana gains [b]Omni-Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.OmniEquip;
            yield return KeywordID.Arcana;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new ArmorOmniEquipBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }



    public class KnightsHeirloomBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero gains: [b]drop[/b] top Weapon, [b]equip[/b] Armor from Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Equip;
            yield return KeywordID.Armor;
            yield return KeywordID.Weapon;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new KnightsHeirloomBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class RoguesHeirloomBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero gains: [b]drop[/b] top 2 Armor, [b]equip[/b] Weapon from Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Equip;
            yield return KeywordID.Armor;
            yield return KeywordID.Weapon;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new RoguesHeirloomBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class MagesHeirloomBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero gains: [b]drop[/b] top 2 Arcana, [b]prepare[/b] Spell from Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Prepare;
            yield return KeywordID.Arcana;
            yield return KeywordID.Spell;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new MagesHeirloomBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class AssassinsBaubleBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "While Hero has [b]Stealth[/b], [b]Poison[/b] triggers deal [b]+1 damage[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
            yield return KeywordID.Poison;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Stealth;
            yield return CardTag.Poison;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new AssassinsBaubleBuff(1, source);
        }

        public override bool CanGetMultiple() => true;
    }


    public class RingOfExuberanceBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero [b]overheals X[/b]: top Weapon gets +X x 2 Attack";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            //yield return KeywordID.Overheal; // TODO
            yield return KeywordID.Weapon;
            yield return KeywordID.Armor;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Heal;
        }

        public override bool CanGetMultiple() => true;

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public class Interceptor : IActionInterceptor
        {
            public readonly EntityID cardID;

            public Interceptor(EntityID cardID)
            {
                this.cardID = cardID;
            }

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is OverhealPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is OverhealPlaceholderAction oa)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;

                    if (oa.EntityID != context.GameOrSimContext.HeroEntity.iD.value)
                        yield break;

                    if (oa.Amount > 0)
                    {
                        yield return new ActivateCardAction(self.iD.value);

                        var weapon = CombatService.GetTopHeroWeapon(context.GameOrSimContext, false);
                        if (weapon != null)
                            yield return new IncreaseAttackAction(weapon.iD.value, oa.Amount * 2, true);
                        yield return new DelayAction(10);

                        yield return new DeactivateCardAction(self.iD.value);
                    }

                }
            }
        }
    }

    public class PlutusRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero can [b]teleport[/b] to Loot (except in Mezzanines)";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Loot;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Loot;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PlutusRingBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class SealOfFateBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]???[/b]";

        public override bool CanGetMultiple() => false;
    }

    public class DivineRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Holy damage[/b] increased by {damage}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.HolyDamage;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.HolyDamage;
        }

        public int Damage => 2;

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new HolyDamageBuff(Damage, source);
        }

        public override bool CanGetMultiple() => true;
    }


    public class KramersPendantBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hammers lose [b]Slow Attack[/b] while a flipped Sin is in the room";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
            yield return KeywordID.Sins;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Hammer;
            yield return CardTag.Sin;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new KramersPendantBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }


    public class SetDressersEarringBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Cards [b]pushed[/b] into Hero cards [b]swap place[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
        }

        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Push;
        }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PushingSwapsBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }


    public class AmuletOfIceBehavior : BasicTrinketCardBehavior
    {
        public int Damage => 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster moves onto occupied spot: it gets [b]pushed[/b] into it [b]for {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
        }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Push; yield return CardTag.Retreat; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new TripwiresBuff(Damage, source);
        }

        public override bool CanGetMultiple() => false;
    }



    public class PiezoRingBehavior : BasicTrinketCardBehavior
    {
        public int Damage => 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Monster moves onto occupied spot: trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
        }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Lightning; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PiezoRingBuff(Damage, source);
        }

        public override bool CanGetMultiple() => true;
    }

    public class PlunderersRingBehavior : BasicTrinketCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Collect Loot: place [b]Lingering Flame[/b] at [b]Origin[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
            yield return KeywordID.Origin;
            yield return KeywordID.Loot;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }

        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Loot; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.LingeringFlame; yield return CardTag.FireDamage; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield return new PlunderersRingBuff(source);
        }

        public override bool CanGetMultiple() => false;
    }

    public class NobunagasLocketBehavior : BasicTrinketCardBehavior
    {
        public int Stealth => 2;
        public int NumEquips => 6;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Every {numEquips}th time Weapon is [b]equipped[/b]: gain [b]{stealth} Stealth[/b], [b]equip[/b] Armor from Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("numEquips", NumEquips); yield return ("stealth", Stealth); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Weapon;
            yield return KeywordID.Equip;
            yield return KeywordID.Armor;
            yield return KeywordID.Stealth;
        }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Stealth; }

        protected override IEnumerable<IBuff> GetBuffs(EntityID source)
        {
            yield break;
        }

        protected override void OnActivate(IGameOrSimEntity card)
        {
            card.ReplaceTrinketCounter(NumEquips);
            card.AddActionInterceptorSource(new IActionInterceptor[] { new Interceptor(card.iD.value) });
        }

        public override bool CanGetMultiple() => false;

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
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.Trinkets)
                        yield break;
                    var hero = context.GameOrSimContext.HeroEntity;
                    if (hero == null)
                        yield break;

                    var equippedCard = context.GameOrSimContext._GetEntityWithID(ra.CardID);
                    if (equippedCard == null)
                        yield break;

                    if (CardUtils.GetCardType(equippedCard) != CardType.Weapon)
                        yield break;

                    yield return new ActivateCardAction(self.iD.value);

                    self.ReplaceTrinketCounter(self.trinketCounter.counter - 1);

                    if (self.trinketCounter.counter <= 0)
                    {
                        var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as NobunagasLocketBehavior;

                        var armorInDrawPile = CombatService.GetTopCardInPile(context.GameOrSimContext, CardArea.HeroDrawPile, e => CardUtils.GetCardType(e) == CardType.Armor);
                        if (armorInDrawPile != null)
                            yield return new RequestEquipCardAction(armorInDrawPile.iD.value);

                        yield return new ModifyStealthAction(hero.iD.value, behavior.Stealth, false);
                        yield return new TriggerSoundAction("stealth");
                        if (hero.hasBoardLocation)
                            yield return new TriggerAnimationAction(context.GameOrSimContext._GetBoardLocationEntity(hero.boardLocation.location).iD.value, new AnimationTrigger("stealth"));
                        yield return new DelayAction(10);

                        self.ReplaceTrinketCounter(behavior.NumEquips);
                    }

                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }
}

