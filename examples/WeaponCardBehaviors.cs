using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using Godot;
using MoreLinq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Card
{
    public class SpearCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Diagonal;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Diagonal Attack[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.DiagonalAttack;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 8 + card.card.data.QualityModifier * 3;
    }

    public class SpearOfLonginusCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Diagonal;
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Diagonal Attack[/b]\n[b]Equip[/b]: add [b]Original Sin[/b] to Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.DiagonalAttack;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("originalSin");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var cardID = new CardDataID("originalSin");
            yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, hero.boardLocation.location, true, followUp: (newCard) =>
            {
                // we do it like this, to make sin replacement effects work
                // HACK: wouldn't work if we had a replacement effect that replaced the sin with another one that was permanent
                if (newCard.card.data.BaseID == cardID.BaseID)
                    return new PermanentlyAddCardToDeckAction(newCard.card.data);
                else
                    return NoopAction.Instance;
            });
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 12;
    }


    public class HopliteSpearCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Diagonal;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Diagonal Attack[/b]\n+1 Use when a Shield is [b]equipped[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.DiagonalAttack;
            yield return KeywordID.MultipleWeaponUses;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Shield;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 1;

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

                    if (!(CardRepository.GenerateCardBehaviorFromData(equippedCard.card.data) is BasicShieldCardBehavior))
                        yield break;


                    yield return new ModifyStickyOffsetAction(cardID, 1);
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(self.iD.value, new AnimationTrigger("upgrade"));
                }
            }
        }
    }

    public class BulwarkSpearCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Diagonal;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Diagonal Attack[/b]\nPlace [b]Illusory Walls[/b] at target's 4-neighbor spots";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.DiagonalAttack;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("illusoryWall");
        }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, target.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("illusoryWall"), l),
                        new DelayAction(10)))
            );
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 3;
    }

    public class HammerCardBehavior : BasicHammerCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Attack[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 16 + card.card.data.QualityModifier * 6;
    }


    public class GodosHammerCardBehavior : BasicHammerCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Attack[/b], [b]Equip[/b]: top Armor gets +{defense} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("defense", Defense(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
        }

        private int Defense(int quality) => 4 + quality * 2;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var topArmor = CombatService.GetTopHeroArmor(context);
            if (topArmor != null)
            {
                yield return new ModifyDefenseValueModifierAction(topArmor.iD.value, Defense(card.card.data.QualityModifier));
                yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                yield return new TriggerAnimationAction(topArmor.iD.value, new AnimationTrigger("upgrade"));
            } else
            {
                yield return new FizzleAction(card.iD.value);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 14 + card.card.data.QualityModifier * 4;
    }


    public class SolomonsHammerCardBehavior : BasicHammerCardBehavior
    {
        private int Heal(int quality) => 2 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Before [b]Slow Attack[/b]: [b]heal {heal}[/b] for each 4-neighbor Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
            yield return KeywordID.Heal;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Heal; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            var numMonsters = BoardUtils.Get4Neighbors(hero.boardLocation.location).Where(l => context._GetCardsWithBoardLocation(l).Any(e => e.isEnemy)).Count();

            if (numMonsters > 0)
            {
                var heal = Heal(weapon.card.data.QualityModifier) * numMonsters;
                yield return new HealTargetAction(hero.iD.value, heal, true);
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 14 + card.card.data.QualityModifier * 4;
    }

    public class MjoelnirCardBehavior : BasicHammerCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Before [b]Slow Attack[/b]: trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", LightningDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
            yield return KeywordID.SlowAttack;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Lightning; }

        private int LightningDamage(int quality) => 3 + quality * 1;
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 10 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            if (target != null && target.hasBoardLocation)
            {
                foreach (var a in CombatService.TriggerLightning(hero, LightningDamage(weapon.card.data.QualityModifier), context, HurtSource.Weapon, hero))
                    yield return a;
            }
        }
    }

    public class BattleHammerCardBehavior : BasicHammerCardBehavior
    {
        public override string GetTextualAttackValue(IGameOrSimEntity card) => "X";
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Attack[/b]\n[b]X[/b]: your Defense x {multiplier}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("multiplier", Multiplier(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
        }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.hasCardArea || card.cardArea.Area != CardArea.HeroGearWeapon || !card.hasCardInPileIndex)
                return null;

            int v;
            if (context.HeroEntity == null || !context.HeroEntity.hasDefenseValue)
                v = 0;
            else
            {
                var defense = context.HeroEntity.defenseValue.value;
                v = defense * Multiplier(card.card.data.QualityModifier);
            }

            var b = context.FilterBuffs<HammerArtisanBuff>().Sum(b => b.Modifier);
            v += b;

            return (v, AttackType.CloseRange);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 0;
        private int Multiplier(int quality) => 2 + quality;
    }

    public class SteadfastHammerCardBehavior : BasicHammerCardBehavior, IActiveEffectCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Attack[/b]\nWhen Armor loses Defense, get that much Attack";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowAttack;
            yield return KeywordID.Armor;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 12 + card.card.data.QualityModifier * 4;

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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is HeroBlocksWithArmorAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is HeroBlocksWithArmorAction b)
                {
                    if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                        yield break;
                    yield return new IncreaseAttackAction(cardID, b.blockedDamage, true);
                }
            }
        }
    }

    public abstract class BasicQuickWeaponCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Attack[/b]";

        public override IEnumerable<IActionData> OnPreAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            foreach(var a in base.OnPreAttack(hero, weapon, target, context))
                yield return a;
            yield return new SetQuickTurnAction(true);
            yield return new TriggerAnimationAction(hero.iD.value, new AnimationTrigger("quickAttack"));
        }
        public override bool IsQuickAttack => true;
    }

    public class QuickDaggerCardBehavior : BasicQuickWeaponCardBehavior
    {
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 2;
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
        }
    }

    public class StealthDaggerCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Attack[/b]: +1 Use if Hero has [b]Stealth[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Stealth;
        }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            if (hero.hasStealth && hero.stealth.value > 0)
            {
                yield return new ModifyStickyOffsetAction(weapon.iD.value, 1);
                yield return new TriggerAnimationAction(weapon.iD.value, new AnimationTrigger("upgrade"));
                yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                yield return new DelayAction(20);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 3;
    }

    public class PoisonedDaggerCardBehavior : BasicQuickWeaponCardBehavior
    {
        private int TriggerPoisonTimes(int quality) => 1 + quality;
        private int PoisonDuration(int quality) => 5 + quality * 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            if (TriggerPoisonTimes(quality) == 1)
                return "[b]Quick Attack[/b]\nApply [b]{poison} Poison[/b], trigger [b]Poison[/b]";
            else
                return "[b]Quick Attack[/b]\nApply [b]{poison} Poison[/b], trigger [b]Poison {triggerTimes}x[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("poison", PoisonDuration(quality)); yield return ("triggerTimes", TriggerPoisonTimes(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
            yield return KeywordID.QuickAttack;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Poison; }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            if (target != null && !target.isDestroyed && target.hasBoardLocation)
            {
                yield return new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("poison"));
                yield return new TriggerSoundAction("poison");
                yield return new ApplyPoisonAction(target.iD.value, PoisonDuration(weapon.card.data.QualityModifier));
                yield return new DelayAction(30);
            }

            yield return new RepeatTimesAction(TriggerPoisonTimes(weapon.card.data.QualityModifier), new TryToTriggerPoisonAction());
            yield return new DelayAction(30);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1;
    }

    public class SleepDaggerCardBehavior : BasicWeaponCardBehavior  
    {
        private int Sleep(int quality) => 4 + quality * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply {sleep} [b]Sleep[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("sleep", Sleep(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            yield return new DelayAction(20);
            if (target != null && target.hasID && target.hasBoardLocation && target.isEnemy)
                yield return new TrySleepAction(target.iD.value, Sleep(weapon.card.data.QualityModifier));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 2;
    }

    public class FearDaggerCardBehavior : BasicWeaponCardBehavior
    {
        public int Damage(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Target [b]retreats[/b], deal [b]{damage} Deck damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Retreat;
            yield return KeywordID.DeckDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Retreat; }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            if (target != null && target.hasID && target.hasBoardLocation)
            {
                yield return new DelayAction(30);
                yield return new TryToRemoveFromBoardAction(target.iD.value, RemoveFromBoardReason.Retreat);
                yield return new HurtMonstersInDeckAction(Damage(weapon.card.data.QualityModifier));
                yield return new DelayAction(30);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1;
    }

    public class NightmareBladeCardBehavior : BasicQuickWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Attack[/b]\nDeals 5x damage if target [b]Sleeps[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sleep; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 2;

        protected override IActionData CreateHurtTargetAction(IGameOrSimEntity target, int attackValue, bool pierce, IGameOrSimContext context)
        {
            if (target != null && target.hasSleeping)
                attackValue = attackValue * 5;
            return base.CreateHurtTargetAction(target, attackValue, pierce, context);
        }
    }

    public class PushingDaggerCardBehavior : BasicWeaponCardBehavior
    {
        private int Push(int quality) => 4 + quality * 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Push for {push}[/b] before damage";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("push", Push(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Push; yield return CardTag.Retreat; }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            yield return new PushAction(target.iD.value, Push(weapon.card.data.QualityModifier), BoardUtils.GetCardinalDirection(hero.boardLocation.location, target.boardLocation.location));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 1;
    }

    public class NeedleAndThreatCardBehavior : BasicQuickWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
            => "[b]Quick Attack[/b]\n[b]Equip[/b]: [b]equip[/b] 1 [b]{shuriken}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("shuriken", CardNames.Generate(new CardDataID("shuriken", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("shuriken", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Shuriken; yield return CardTag.Ranged; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CreateCardOnHeroAction(new CardDataID("shuriken", card.card.data.QualityModifier), forceEthereal: true);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;
    }

    public abstract class BasicBowCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Bow; yield return CardTag.Ranged; }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var b = context.FilterBuffs<BowArtisanBuff>().Sum(b => b.Modifier);
            return (GetSimpleAttackValue(card) + b, AttackType.Ranged);
        }

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (context.FilterBuffs<BowArtisanBuff>().Any()) 
                yield return new SetQuickTurnAction(true); // TODO: show bows as quick equip (particles, ...)

            foreach (var a in base.OnBeforeEquip(hero, card, context))
                yield return a;
        }
    }

    public class BowCardBehavior : BasicBowCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 3;
    }


    public class CompositeBowCardBehavior : BasicBowCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], target [b]retreats[/b], draw card at its spot";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Retreat;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Retreat; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            yield return new DelayAction(30);
            if (target != null && target.hasID)
            {
                yield return new TryToRemoveFromBoardAction(target.iD.value, RemoveFromBoardReason.Retreat);
            }

            if (lastKnownTargetBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastKnownTargetBoardLocation.Value).Any())
            {
                yield return new DrawCardAction(lastKnownTargetBoardLocation.Value, CardArea.HeroDrawPile);
                yield return new DelayAction(30);
            }
        }
    }

    public class PushingBowCardBehavior : BasicBowCardBehavior
    {
        private int Push(int quality) => 3 + quality * 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b]\n[b]Push for {push}[/b] before damage";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("push", Push(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Push;
            yield return KeywordID.Retreat;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Push; yield return CardTag.Retreat; }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (target != null && target.hasID && hero.hasBoardLocation && target.hasBoardLocation)
                yield return new PushAction(target.iD.value, Push(weapon.card.data.QualityModifier), BoardUtils.GetCardinalDirection(hero.boardLocation.location, target.boardLocation.location));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier * 1;
    }

    public class PoisonBowCardBehavior : BasicBowCardBehavior
    {
        private int TriggerPoisonTimes(int quality) => 2 + quality;
        private int PoisonDuration(int quality) => 8 + quality * 3;

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            if (target != null && !target.isDestroyed && target.hasBoardLocation)
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("poison"));
                yield return new TriggerSoundAction("poison");
                yield return new ApplyPoisonAction(target.iD.value, PoisonDuration(weapon.card.data.QualityModifier));
                yield return new DelayAction(30);
            }

            yield return new RepeatTimesAction(TriggerPoisonTimes(weapon.card.data.QualityModifier), new TryToTriggerPoisonAction());
            yield return new DelayAction(30);
        }

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            if (TriggerPoisonTimes(quality) == 1)
                return "[b]Ranged[/b]\nApply [b]{poison} Poison[/b], trigger [b]Poison[/b]";
            else
                return "[b]Ranged[/b]\nApply [b]{poison} Poison[/b], trigger [b]Poison {triggerTimes}x[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("poison", PoisonDuration(quality)); yield return ("triggerTimes", TriggerPoisonTimes(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Poison; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier;
    }

    public class PowderKegBowCardBehavior : BasicBowCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], Attack: place [b]O-Barrel[/b] in front of target";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("oBarrel");
        }

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (hero != null & target != null && !target.isDestroyed && hero.hasBoardLocation && target.hasBoardLocation)
            {
                var d = BoardUtils.GetCardinalDirection(target.boardLocation.location, hero.boardLocation.location);
                var l = target.boardLocation.location + d;
                if (BoardUtils.IsValid(l) && !context._GetCardsWithBoardLocation(l).Any())
                    yield return new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("oBarrel"), l, true),
                        new DelayAction(30));
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3;
    }

    public abstract class BasicSwordCardBehavior : BasicWeaponCardBehavior
    {
        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var b = context.FilterBuffs<SwordArtisanBuff>().Sum(b => b.Modifier);
            return (GetSimpleAttackValue(card) + b, AttackType.CloseRange);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sword; }
    }

    public class SwordCardBehavior : BasicSwordCardBehavior
    {
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 4;
    }

    public class RustySwordCardBehavior : BasicSwordCardBehavior
    {
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 1;
        public override int MaxUpgrade => 0;
    }

    public class SacrificialSwordCardBehavior : BasicSwordCardBehavior
    {
        private int Heal(int quality) => 3 + quality;
        private int Loss(int quality) => 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: lose [b]{loss} HP[/b]\n[b]Attack[/b]: [b]heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("loss", Loss(quality)); yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Heal; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, Loss(card.card.data.QualityModifier), HurtSource.Weapon, HurtType.Regular, true);
        }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            var healBy = Heal(weapon.card.data.QualityModifier);
            yield return new HealTargetAction(hero.iD.value, healBy, true);
            yield return new DelayAction(30);
        }

        //public override IEnumerable<IActionData> OnAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        //{
        //    foreach (var a in base.OnAttackHits(hero, weapon, target, context, attackType))
        //        yield return a;

        //}

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier * 2;
    }


    public class HungrySwordCardBehavior : BasicSwordCardBehavior
    {
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3;

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + Mathf.FloorToInt(card.card.data.QualityModifier / 2);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has +{attackIncrease} Attack while use counter is odd, [b]Attack[/b]: add [b]Gluttony[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackIncrease", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { foreach (var t in base.ProvidesTags()) yield return t; yield return CardTag.Sin; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("gluttony");
        }

        private int AttackIncrease(int quality) => 6 + quality * 2;

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var @base = base.TryGetResolvedAttackValueAndType(card, context);
            if (!@base.HasValue)
                return null;

            if (card.hasCardArea && (card.cardArea.Area == CardArea.Board || card.cardArea.Area == CardArea.HeroGearWeapon) && Sticky(card, context) % 2 == 0)
                return (@base.Value.value + AttackIncrease(card.card.data.QualityModifier), @base.Value.type);
            else
                return (@base.Value.value, @base.Value.type);
        }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            yield return new CreateCardInDrawPileStartingFromBoardAction(new CardDataID("gluttony"), hero.boardLocation.location, true);
        }
    }

    public class MakeshiftDaggerCardBehavior : BasicWeaponCardBehavior
    {
        private int Disarm(int quality) => 2 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply {disarm} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("disarm", -Disarm(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Disarm; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            yield return new DisarmAction(target.iD.value, Disarm(weapon.card.data.QualityModifier));
            yield return new DelayAction(10);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 5 + card.card.data.QualityModifier * 1;
    }

    public class HighlanderCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: [b]drops[/b] other Weapons, gets +{attackGain} Attack for each";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackGain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Equip;
        }

        private int AttackGain(int quality) => 3 + quality;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var equippedWeaponsOtherThanThis = context._GetCardsWithCardArea(CardArea.HeroGearWeapon).Where(e => e.iD.value != card.iD.value);
            foreach(var other in equippedWeaponsOtherThanThis)
            {
                yield return new RequestDropCardAction(other.iD.value, RequestDropReason.ForceDrop);
                yield return new IncreaseAttackAction(card.iD.value, AttackGain(card.card.data.QualityModifier), true);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 14 + card.card.data.QualityModifier * 3;
    }

    public class VaingloryCardBehavior : BasicWeaponCardBehavior
    {
        private int NumVanities(int quality) => 2 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: add {num} [b]Vanity[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", NumVanities(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("vanity");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SequenceAction(Enumerable.Repeat(new CreateCardInDrawPileStartingFromBoardAction(new CardDataID("vanity"), hero.boardLocation.location, true), NumVanities(card.card.data.QualityModifier)));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 13 + card.card.data.QualityModifier * 5;
    }

    public class MaceCardBehavior : BasicWeaponCardBehavior
    {
        private int Gain(int quality) => 3 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: gets [b]+{gain} Attack[/b] for each [b]flipped up[/b] Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("gain", Gain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var monsters = context._GetCardsWithCardArea(CardArea.Board).Where(e => e.isEnemy && e.isCardFlipped);
            foreach (var other in monsters)
            {
                yield return new IncreaseAttackAction(card.iD.value, Gain(card.card.data.QualityModifier), true);
                yield return new DelayAction(20);
            }
        }
    }

    public class MuradsMaceCardBehavior : BasicWeaponCardBehavior
    {
        private const int countdownInit = 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b] while not equipped: place [b]Wrath[/b] at 4-neighbor spots";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Equip;
            yield return KeywordID.Timer;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("wrath");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sin; }


        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new RemoveChannelingAction(card.iD.value);
        }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            yield return new ModifyChannelingAction(card.iD.value, countdownInit, true);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.hasBoardLocation || !(card.hasCardArea && card.cardArea.Area == CardArea.Board) || !card.isCardFlipped)
                yield break;

            var currentCountdown = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentCountdown > 0)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
            }

            currentCountdown = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentCountdown <= 0)
            {
                yield return new SequenceAction(
                     BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                         .Where(l => BoardUtils.Are4Neighbors(l, card.boardLocation.location))
                         .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                         .Select(l => new SequenceAction(
                             new TriggerSoundAction("cardCreateOnBoard"),
                             new CreateCardAtBoardLocationAction(new CardDataID("wrath"), l),
                             new DelayAction(10)
                         ))
                 );

                yield return new ModifyChannelingAction(card.iD.value, countdownInit, true);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, countdownInit, true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddChannelling(countdownInit);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 13 + card.card.data.QualityModifier * 4;
    }

    public class MorningstarCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b]: each equipped Armor gets +{increase} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("increase", ArmorIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        private int ArmorIncrease(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnAfterEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var equippedArmorCards = context._GetCardsWithCardArea(CardArea.HeroGearArmor);

            var a = equippedArmorCards.Select(c => new SequenceAction(
                new ModifyDefenseValueModifierAction(c.iD.value, ArmorIncrease(card.card.data.QualityModifier)),
                new TriggerSoundAction(new SoundTrigger("heroUpgrade")),
                new TriggerAnimationAction(c.iD.value, new AnimationTrigger("upgrade"))
                ));

            yield return new SequenceAction(a);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 1;
    }

    public class ZweihaenderCardBehavior : BasicWeaponCardBehavior
    {
        private int Gain(int quality) => 9 + quality * 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has [b]+{gain} Attack[/b] while no [b]Shield[/b] is equipped";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("gain", Gain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier * 1;

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.cardArea.Area != CardArea.HeroGearWeapon && card.cardArea.Area != CardArea.Board)
                return null;

            var @base = base.TryGetResolvedAttackValueAndType(card, context);
            if (!@base.HasValue)
                return null;


            if (context._GetCardsWithCardArea(CardArea.HeroGearArmor).Any(c => CardRepository.GenerateCardBehaviorFromData(c.card.data) is BasicShieldCardBehavior))
                return (@base.Value.value, @base.Value.type);
            else
                return (@base.Value.value + Gain(card.card.data.QualityModifier), @base.Value.type);
        }
    }

    public class KnifeCardBehavior : BasicWeaponCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => DifficultyUtils.GetStartingAutoEquipWeaponReuseModifier(3, context);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.AutoEquip;
            yield return KeywordID.MultipleWeaponUses;
        }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier;

        public override bool IsAutoEquip() => true;
    }

    public class OldKnifeCardBehavior : BasicWeaponCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.MultipleWeaponUses;
        }
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3 + card.card.data.QualityModifier;
    }

    public class WhipCardBehavior : BasicWeaponCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => DifficultyUtils.GetStartingAutoEquipWeaponReuseModifier(3 + card.card.data.QualityModifier, context);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b], [b]swap place[/b], apply [b]1 Sleep[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
            yield return KeywordID.AutoEquip;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (target != null && target.hasID && hero.hasBoardLocation && target.hasBoardLocation)
                yield return new ParallelAction(new MoveCardOnBoardAction(target.iD.value, hero.boardLocation.location, MoveReason.Swap), new MoveCardOnBoardAction(hero.iD.value, target.boardLocation.location, MoveReason.Swap));
        }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            yield return new DelayAction(20); // To have space before sleep is applied
            if (target != null && target.hasID && target.isEnemy)
            {
                yield return new TrySleepAction(target.iD.value, 1);
                yield return new DelayAction(10); // a bit more space is nicer
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1;

        public override bool IsAutoEquip() => true;
    }
    public class OldWhipCardBehavior : BasicWeaponCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => DifficultyUtils.GetStartingAutoEquipWeaponReuseModifier(3 + card.card.data.QualityModifier, context);
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Swap place[/b], apply [b]1 Sleep[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Sleep; }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (target != null && target.hasID && hero.hasBoardLocation && target.hasBoardLocation)
                yield return new ParallelAction(new MoveCardOnBoardAction(target.iD.value, hero.boardLocation.location, MoveReason.Swap), new MoveCardOnBoardAction(hero.iD.value, target.boardLocation.location, MoveReason.Swap));
        }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            yield return new DelayAction(20); // To have space before sleep is applied
            if (target != null && target.hasID && target.isEnemy)
            {
                yield return new TrySleepAction(target.iD.value, 1);
                yield return new DelayAction(10); // a bit more space is nicer
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1;
    }

    public class WeirdRodCardBehavior : BasicWeaponCardBehavior
    {
        public override bool IsQuickAttack => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b]\n[b]Quick Attack[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
            yield return KeywordID.AutoEquip;
        }

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        public override IEnumerable<IActionData> OnPreAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            foreach (var a in base.OnPreAttack(hero, weapon, target, context))
                yield return a;
            yield return new SetQuickTurnAction(true);
            yield return new TriggerAnimationAction(hero.iD.value, new AnimationTrigger("quickAttack"));
        }
        //public override IActionData OnAttack(IGameOrSimEntity hero, IGameOrSimEntity target, IGameOrSimContext context)
        //{
        //    return new SequenceAction(
        //        new ParallelAction(new MoveCardOnBoardAction(target.iD.value, hero.boardLocation.location), new MoveCardOnBoardAction(hero.iD.value, target.boardLocation.location)),
        //        base.OnAttack(hero, target, context)
        //    );
        //}

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 0 + card.card.data.QualityModifier;

        public override bool IsAutoEquip() => true;
    }

    public class HiddenBladeBehavior : BasicWeaponCardBehavior, IActiveEffectCardBehavior
    {
        public override bool IsQuickAttack => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Attack[/b]\nIs [b]equipped[/b] from Deck when [b]Sleep[/b] is applied";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
            yield return KeywordID.Equip;
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sleep; }

        public override IEnumerable<IActionData> OnPreAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            foreach (var a in base.OnPreAttack(hero, weapon, target, context))
                yield return a;
            yield return new SetQuickTurnAction(true);
            yield return new TriggerAnimationAction(hero.iD.value, new AnimationTrigger("quickAttack"));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier;

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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is SleepAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is SleepAction sa && sa.change > 0)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.HeroDrawPile)
                        yield break;

                    yield return new RequestEquipCardAction(self.iD.value);
                }
            }
        }
    }

    public class KunaiCardBehavior : BasicQuickWeaponCardBehavior
    {
        public override string GetTextualAttackValue(IGameOrSimEntity card) => "X";
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Attack[/b]\n[b]X[/b]: Attack of next\nWeapon in stack + {addedAttack}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("addedAttack", AddedAttack(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
        }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.cardArea.Area != CardArea.HeroGearWeapon || !card.hasCardInPileIndex)
                return null;

            var myIndex = card.cardInPileIndex.index;
            var weaponGearCards = context._GetCardsWithCardArea(CardArea.HeroGearWeapon);
            // find weapon with index that is
            // 1. larger than myIndex
            // 2. the smallest of those
            IGameOrSimEntity nextWeapon = null;
            foreach (var c in weaponGearCards)
                if (c.cardInPileIndex.index > myIndex)
                {
                    if (nextWeapon == null || nextWeapon.cardInPileIndex.index > c.cardInPileIndex.index)
                        nextWeapon = c;
                }

            var attackValueOfNextWeapon = (nextWeapon != null) ? nextWeapon.finalAttackValue.value : 0; // TODO: check if this is set

            return (attackValueOfNextWeapon + GetSimpleAttackValue(card), AttackType.CloseRange);
        }

        private int AddedAttack(int quality) => quality * 2 + 1;
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => AddedAttack(card.card.data.QualityModifier); // NOTE: we repurpose this a bit to mean the increase in attack value per quality
    }

    public class ShurikenCardBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;
        public override ProjectileType GetProjectileType() => ProjectileType.Shuriken;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Shuriken; yield return CardTag.Ranged; }

        public override bool IsQuickAttack => false;

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            if (context.FilterBuffs<ShurikenApplyPoisonBuff>().Any())
            {
                yield return new DelayAction(30);
                foreach (var b in context.FilterBuffs<ShurikenApplyPoisonBuff>())
                {
                    yield return new ActivateCardAction(b.Source);
                    if (target != null && !target.isDestroyed && target.hasBoardLocation)
                    {
                        yield return new SequenceAction(
                            new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("poison")),
                            new TriggerSoundAction("poison"),
                            new ApplyPoisonAction(target.iD.value, b.Duration),
                            new DelayAction(30)
                        );
                    }

                    yield return new RepeatTimesAction(b.TriggerTimes, new TryToTriggerPoisonAction());
                    yield return new DelayAction(30);
                    yield return new DeactivateCardAction(b.Source);
                }
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier * 1;
    }
    
    public class RapierCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has +{attackIncrease} Attack while no Armor is equipped";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackIncrease", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        private int AttackIncrease(int quality) => 6 + quality * 3;

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.hasCardArea || (card.cardArea.Area != CardArea.Board && card.cardArea.Area != CardArea.HeroGearWeapon))
                return (GetSimpleAttackValue(card), AttackType.CloseRange);

            var equippedArmorCards = context._GetCardsWithCardArea(CardArea.HeroGearArmor);
            if (equippedArmorCards.Empty())
                return (GetSimpleAttackValue(card) + AttackIncrease(card.card.data.QualityModifier), AttackType.CloseRange);
            else
                return (GetSimpleAttackValue(card), AttackType.CloseRange);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier;
    }

    public class FencingRapierCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply {disarm} Attack, gets 3x Attack if target's Attack is 0 or less";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("disarm", -Disarm(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Disarm; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            yield return new DisarmAction(target.iD.value, Disarm(weapon.card.data.QualityModifier));
            yield return new DelayAction(30);

            if (target != null && !target.isDestroyed && target.hasID && target.hasFinalAttackValue && target.finalAttackValue.value <= 0)
            {
                var weaponAttack = weapon.finalAttackValue.value;

                yield return new IncreaseAttackAction(weapon.iD.value, weaponAttack * 2, true); // triple by adding its value 2x
                yield return new DelayAction(30);
            }
        }

        private int Disarm(int quality) => 2 + quality;

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 8 + card.card.data.QualityModifier * 2;
    }

    public class CunningRapierCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has 4x Attack while no other Weapon equipped";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        public override int AfterAttackModifiersModifier(IGameOrSimEntity card, IGameOrSimContext context, int attackValueWithModifiers)
        {
            if (card.cardArea.Area != CardArea.HeroGearWeapon)
                return attackValueWithModifiers;

            var equippedWeaponCards = context._GetCardsWithCardArea(CardArea.HeroGearWeapon);

            if (equippedWeaponCards.Count <= 1)
                return attackValueWithModifiers * 4;
            else
                return attackValueWithModifiers;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier;
    }

    public class ConcealedRapierCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has 5x Attack while Hero has [b]Stealth[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Stealth;
        }

        public override int AfterAttackModifiersModifier(IGameOrSimEntity card, IGameOrSimContext context, int attackValueWithModifiers)
        {
            if (card.cardArea.Area != CardArea.HeroGearWeapon && card.cardArea.Area != CardArea.Board)
                return attackValueWithModifiers;

            if (context.HeroEntity != null && context.HeroEntity.hasStealth)
                return attackValueWithModifiers * 5;
            else
                return attackValueWithModifiers;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4 + card.card.data.QualityModifier;
    }

    public class DuelingRapierCardBehavior : BasicWeaponCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Has 6x Attack while exactly one Monster is in the room";

        public override int AfterAttackModifiersModifier(IGameOrSimEntity card, IGameOrSimContext context, int attackValueWithModifiers)
        {
            if (card.cardArea.Area != CardArea.HeroGearWeapon && card.cardArea.Area != CardArea.Board)
                return attackValueWithModifiers;

            if (context._GetEnemyCardsOnBoard(false).Count == 1)
                return attackValueWithModifiers * 6;
            else
                return attackValueWithModifiers;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 5 + card.card.data.QualityModifier;
    }

    public class InfusedSwordActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public InfusedSwordActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is CastScrollPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                yield break;

            var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as InfusedSwordCardBehavior;
            yield return new IncreaseAttackAction(cardID, behavior.AttackPerScroll(self.card.data.QualityModifier), true);
        }
    }

    public class InfusedSwordCardBehavior : BasicSwordCardBehavior, IActiveEffectCardBehavior
    {
        public int AttackPerScroll(int quality) => 4 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gets +{attackGain} Attack when a Scroll is cast";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackPerScroll(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Scroll;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Scroll; }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new InfusedSwordActionInterceptor(e.iD.value) });
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier;
    }

    public class LostSwordCardBehavior : BasicSwordCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]???[/b]";

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 99;
    }

    public class SamsonsSwordCardBehavior : BasicSwordCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "{numSins} Sins in Deck: [b]destroy[/b] them, get +{attackIncrease} Attack and +1 Use";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("numSins", NumSins); yield return ("attackIncrease", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sins;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sin; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            var sinsInDeck = CombatService.GetCardsInPileOrdered(context, CardArea.HeroDrawPile, c => CardUtils.GetCardType(c) == CardType.Sin).Take(NumSins).ToArray();
            if (sinsInDeck.Length >= NumSins)
            {
                yield return new ModifyStickyOffsetAction(weapon.iD.value, 1);

                yield return new IncreaseAttackAction(weapon.iD.value, AttackIncrease(weapon.card.data.QualityModifier), true);
                yield return new DelayAction(20);

                foreach (var s in sinsInDeck)
                {
                    yield return new PermanentlyRemoveCardFromDeckAction(s.card.data); // NOTE: for sins like original sin which are otherwise permanent
                    yield return new DestroyCardAction(s.iD.value, DestroyCardReason.Other);
                }
            }
        }

        private int NumSins => 3;
        private int AttackIncrease(int quality) => 5 + quality * 3;

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier * 2;
    }

    public class InfusedDaggerActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public InfusedDaggerActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is CastScrollPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                yield break;

            yield return new ModifyStickyOffsetAction(cardID, 1);
            yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
            yield return new TriggerAnimationAction(self.iD.value, new AnimationTrigger("upgrade"));
        }
    }

    public class InfusedDaggerCardBehavior : BasicWeaponCardBehavior, IActiveEffectCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Attack[/b]\n+1 Use when a Scroll is cast";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickAttack;
            yield return KeywordID.MultipleWeaponUses;
            yield return KeywordID.Scroll;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Scroll; }

        public override IEnumerable<IActionData> OnPreAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            foreach (var a in base.OnPreAttack(hero, weapon, target, context))
                yield return a;
            yield return new SetQuickTurnAction(true);
            yield return new TriggerAnimationAction(hero.iD.value, new AnimationTrigger("quickAttack"));
        }
        public override bool IsQuickAttack => true;

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddActionInterceptorSource(new IActionInterceptor[] { new InfusedDaggerActionInterceptor(e.iD.value) });
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier;
    }


    public class SoaringShieldFauxWeaponBehavior : BasicWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b]";
        public override string GetTextualAttackValue(IGameOrSimEntity card) => $"{DamageMultiplier(card.card.data.QualityModifier)}X";

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 0;

        private int DamageMultiplier(int quality) => SoaringShieldsBehavior.Multiplier(quality);

        private IGameOrSimEntity GetReferencedShield(IGameOrSimEntity weapon, IGameOrSimContext context)
        {
            if (weapon.hasSoaringShieldReference)
            {
                var shield = context._GetEntityWithID(weapon.soaringShieldReference.shield);
                if (shield != null && shield.hasCardArea && shield.cardArea.Area == CardArea.HeroGearArmor)
                {
                    return shield;
                }
            }
            return null;
        }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            int v = 0;
            var shield = GetReferencedShield(card, context);
            if (shield != null && shield.hasFinalDefenseValue)
                v = shield.finalDefenseValue.value;

            v *= DamageMultiplier(card.card.data.QualityModifier);

            return (v, AttackType.Ranged);
        }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            var shield = GetReferencedShield(weapon, context);
            if (shield != null)
            {
                yield return new RequestDropCardAction(shield.iD.value, RequestDropReason.Used);
            }
        }
    }

    public class HolyBladeBehavior : BasicWeaponCardBehavior
    {
        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 1;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Permanently [b]upgrades[/b] when it defeats a Monster";

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.HolyDamage;
        }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override int MaxUpgrade => 99;

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            // check if target was monster and was killed
            if (targetType == CardType.Monster)
            {
                if (initialTarget == null || !initialTarget.hasID || initialTarget.isDestroyed)
                {
                    // affect deck
                    yield return new PermanentlyRemoveCardFromDeckAction(weapon.card.data);
                    yield return new PermanentlyAddCardToDeckAction(new CardDataID(weapon.card.data.BaseID, Math.Min(MaxUpgrade, weapon.card.data.QualityModifier + 1)));

                    // affect weapon itself
                    yield return new UpgradeCardAction(weapon.iD.value, 1);
                    yield return new DelayAction(25);
                }
            }
        }

        protected override IActionData CreateHurtTargetAction(IGameOrSimEntity target, int attackValue, bool pierce, IGameOrSimContext context)
        {
            if (!target.hasID)
                return NoopAction.Instance;
            if (!target.hasBoardLocation)
                return NoopAction.Instance;

            return new AffectCardAndLocationAction(target.boardLocation.location, false, (e) => true,
                        e => new HurtTargetAction(e.iD.value, attackValue, HurtSource.Weapon, HurtType.Holy),
                        e => new SequenceAction(
                            new TriggerSoundAction("holy"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("holy")),
                            new DelayAction(14)
                        ));
        }
    }
}