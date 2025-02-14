using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public abstract class BasicMageWeaponCardBehavior : BasicWeaponCardBehavior
    {
        public override ProjectileType GetProjectileType() => ProjectileType.MageAttack;
    }

    public class NoviceWandCardBehavior : BasicMageWeaponCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => DifficultyUtils.GetStartingAutoEquipWeaponReuseModifier(3, context);

        public override AttackType GetAttackType() => AttackType.Ranged;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Auto-Equip[/b], [b]Ranged[/b]\nHas +{attackGain} Attack every second turn";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackGain(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.AutoEquip;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Ranged; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card)
        {
            return 1;
        }

        private int AttackGain(int quality) => 4 + quality;

        public override IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine)
        {
            if (onMezzanine)
                yield break;
            yield return new RequestEquipCardAction(card.iD.value);
        }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var attackGain = 0;
            if (context.CurrentTurn % 2 == 1)
                attackGain = AttackGain(card.card.data.QualityModifier);

            return (GetSimpleAttackValue(card) + attackGain, GetAttackType());
        }

        public override bool IsAutoEquip() => true;
    }

    public abstract class BasicStaffCardBehavior : BasicMageWeaponCardBehavior
    {
        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var b = context.FilterBuffs<StaffArtisanBuff>().Sum(b => b.Modifier);
            return (GetSimpleAttackValue(card) + b, AttackType.CloseRange);
        }

        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Staff; }

        public override IEnumerable<IActionData> OnBeforeEquip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var b in context.FilterBuffs<StaffArtisanBuff>())
            {
                yield return new ActivateCardAction(b.Source);
                yield return new CreateCardOnHeroAction(new CardDataID("manaOrb"));
                yield return new DeactivateCardAction(b.Source);
            }

            foreach (var a in base.OnBeforeEquip(hero, card, context))
                yield return a;
        }
    }

    public class ImbuedStaffCardBehavior : BasicStaffCardBehavior
    {
        private int AttackIncrease(int quality) => 5 + quality * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Focus 2[/b]: has +{attackGain} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FocusX;
        }

        public override (int value, AttackType type)? TryGetResolvedAttackValueAndType(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card == null || context.HeroEntity == null)
                return null;

            var @base = base.TryGetResolvedAttackValueAndType(card, context);
            var baseValue = @base.HasValue ? @base.Value.value : 0;

            var defense = context.HeroEntity.hasDefenseValue ? context.HeroEntity.defenseValue.value : 0;
            if (defense >= 2)
                return (baseValue + AttackIncrease(card.card.data.QualityModifier), AttackType.CloseRange);
            else
                return (baseValue, AttackType.CloseRange);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 4;
    }

    public class CeremonialStaffCardBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quest Item[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuestItem;
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 2 + card.card.data.QualityModifier * 1;
    }

    public class ManaStaffCardBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Before Attack[/b]: [b]equip[/b] 2 [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Staff; yield return CardTag.ManaOrb; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            yield return new CreateCardOnHeroAction(new CardDataID("manaOrb", weapon.card.data.QualityModifier));
            yield return new CreateCardOnHeroAction(new CardDataID("manaOrb", weapon.card.data.QualityModifier));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 2;
    }

    public class FocusStaffCardBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Focus {focus}[/b]: attack damage is dealt to all other Monsters too";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("focus", Focus); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FocusX;
        }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            var defense = context.HeroEntity.defenseValue.value;
            var attack = context.HeroEntity.attackValue.value;
            if (defense >= Focus)
            {
                yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy && (initialTarget == null || !initialTarget.hasID || e.iD.value != initialTarget.iD.value),
                        e => new HurtTargetAction(e.iD.value, attack, HurtSource.Weapon, HurtType.Regular),
                        e => new SequenceAction(
                            new TriggerSoundAction("heroAttack"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                            new DelayAction(5)
                        )
                    )));
                yield return new DelayAction(30);
            }
        }

        private int Focus => 5;

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7 + card.card.data.QualityModifier * 2;
    }

    public class ExertStaffCardBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Exert {exert}[/b]: gets {multiplier}x Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("exert", Exert); yield return ("multiplier", Multiplier); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExertX;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Staff; yield return CardTag.Exert; }

        private int Exert => 4;
        private int Multiplier => 4;

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 1;

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            foreach (var (ea, success) in CombatService.TryExert(Exert, context))
            {
                yield return ea;

                var curHeroAttack = hero.attackValue.value; // is needed(?), because hero might have buffs that affect the final damage... or not?

                if (success)
                {
                    yield return new IncreaseAttackAction(weapon.iD.value, curHeroAttack * (Multiplier - 1), true);
                }
                yield return new DelayAction(30);
            }
        }
    }

    public class LightningStaffCardBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Before Attack[/b]: trigger [b]Lightning {lightning}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("lightning", Lightning(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Staff; yield return CardTag.Lightning; }

        private int Lightning(int quality) => 3 + quality;

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
        }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            if (!hero.hasBoardLocation) yield break;

            foreach (var a in CombatService.TriggerLightning(hero, Lightning(weapon.card.data.QualityModifier), context, HurtSource.Weapon, hero))
                yield return a;

            yield return new DelayAction(30);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 2;
    }

    public class IncineratorWandCardBehavior : BasicMageWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        private int Exert => 3;
        private int IncinerateDamage(int quality) => 3 + quality * 1;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], [b]Exert {exert}[/b]: deal [b]{damage} damage[/b] to each card";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("exert", Exert); yield return ("damage", IncinerateDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.ExertX;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Exert; yield return CardTag.Ranged; yield return CardTag.FireDamage; }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            foreach (var (ea, success) in CombatService.TryExert(Exert, context))
            {
                yield return ea;

                if (success)
                {
                    // incinerate effect
                    yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                            e => new HurtTargetAction(e.iD.value, IncinerateDamage(weapon.card.data.QualityModifier), HurtSource.Weapon, HurtType.Fire),
                            e => new SequenceAction(
                                new TriggerSoundAction("fire"),
                                new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                new DelayAction(5)
                            )
                        )));
                    yield return new DelayAction(30);
                }
            }
        }

        protected override IActionData CreateHurtTargetAction(IGameOrSimEntity target, int attackValue, bool pierce, IGameOrSimContext context)
        {
            if (!target.hasID)
                return NoopAction.Instance;
            return new SequenceAction(
                new TriggerSoundAction("fire"),
                target.hasBoardLocation ? new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("burn")) : (IActionData)NoopAction.Instance,
                new HurtTargetAction(target.iD.value, attackValue, HurtSource.Weapon, HurtType.Fire, pierce));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6 + card.card.data.QualityModifier * 2;
    }

    public class FlameWandCardBehavior : BasicMageWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], Place [b]{lingeringFlame}[/b] at target's 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.FireDamage;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.LingeringFlame; yield return CardTag.Ranged; yield return CardTag.FireDamage; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (target != null && target.hasBoardLocation)
            {
                var freeLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, target.boardLocation.location))
                        .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                        .ToList();
                if (freeLocations.Any())
                {
                    yield return new SequenceAction(
                        freeLocations.Select(l => new SequenceAction(
                            new TriggerSoundAction("cardCreateOnBoard"),
                            new CreateCardAtBoardLocationAction(new CardDataID("lingeringFlame"), l, flipUp: true),
                            new DelayAction(10)
                        ))
                    );
                }
            }
        }

        protected override IActionData CreateHurtTargetAction(IGameOrSimEntity target, int attackValue, bool pierce, IGameOrSimContext context)
        {
            if (!target.hasID)
                return NoopAction.Instance;
            return new SequenceAction(
                new TriggerSoundAction("fire"),
                target.hasBoardLocation ? new TriggerAnimationAction(context._GetBoardLocationEntity(target.boardLocation.location).iD.value, new AnimationTrigger("burn")) : (IActionData)NoopAction.Instance,
                new HurtTargetAction(target.iD.value, attackValue, HurtSource.Weapon, HurtType.Fire, pierce));
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 3;
    }

    public class SpellWandCardBehavior : BasicMageWeaponCardBehavior, IActiveEffectCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        private int AttackIncrease(int quality) => 4 + quality * 1;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], gets +{attackGain} Attack when a Spell is [b]prepared[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Prepare;
            yield return KeywordID.Equip;
            yield return KeywordID.Spell;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Spell; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Ranged; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 7;

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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is PrepareSpellPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                    yield break;

                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as SpellWandCardBehavior;
                yield return new IncreaseAttackAction(cardID, behavior.AttackIncrease(self.card.data.QualityModifier), true);
            }
        }
    }


    public class BlinkWandCardBehavior : BasicMageWeaponCardBehavior, IActiveEffectCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        private int AttackIncrease(int quality) => 5 + quality * 1;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], gets +{attackGain} Attack when your Hero [b]teleports[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attackGain", AttackIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Prepare;
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Teleport; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Ranged; }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 6;

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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterTeleportPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (!self.hasCardArea || !(self.cardArea.Area == CardArea.HeroGearWeapon || (self.cardArea.Area == CardArea.Board && self.isCardFlipped)))
                    yield break;

                var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as BlinkWandCardBehavior;
                yield return new IncreaseAttackAction(cardID, behavior.AttackIncrease(self.card.data.QualityModifier), true);
            }
        }
    }


    public class WandOfRehearsalCardBehavior : BasicMageWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        private int Exert() => 5;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], [b]Attack:  Exert {exert}[/b]: +1 Use";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("exert", Exert()); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.ExertX;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Ranged; yield return CardTag.Exert; }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            var exert = Exert();
            foreach (var (ea, success) in CombatService.TryExert(exert, context))
            {
                yield return ea;

                if (success)
                {
                    yield return new ModifyStickyOffsetAction(weapon.iD.value, 1);
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(weapon.iD.value, new AnimationTrigger("upgrade"));

                    yield return new DelayAction(20);
                }
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 8 + card.card.data.QualityModifier * 3;
    }


    public class MaticksWandBehavior : BasicMageWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], [b]Attack[/b]: [b]prepare {magicMissile}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("magicMissile", CardNames.Generate(new CardDataID("magicMissile"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.MagicMissiles; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("magicMissile");
        }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity initialTarget, IGameOrSimContext context, CardType targetType)
        {
            yield return new CreateCardOnHeroAction(new CardDataID("magicMissile"), forceEthereal: true);
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3;
    }


    public class AmenhotepsStaffBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Place [b]Emeralds[/b] at target's 4-neighbor spots";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald01");
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Staff; yield return CardTag.Loot; }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            if (target == null || !target.hasBoardLocation)
                yield return new FizzleAction(hero.iD.value);

            var freeLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, target.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .ToList();

            if (freeLocations.Any())
            {
                yield return new SequenceAction(
                    freeLocations.Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("emerald01"), l, true),
                        new DelayAction(10)
                    ))
                );
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 9 + card.card.data.QualityModifier * 3;
    }

    public class WandOfMammonBehavior : BasicMageWeaponCardBehavior
    {
        public override AttackType GetAttackType() => AttackType.Ranged;

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Ranged[/b], [b]Attack[/b]: place [b]Emeralds[/b] between Hero and target";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.MultipleWeaponUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Loot; }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald01");
        }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            if (hero != null & target != null && !target.isDestroyed && hero.hasBoardLocation && target.hasBoardLocation)
            {
                var d = BoardUtils.GetCardinalDirection(hero.boardLocation.location, target.boardLocation.location);
                var l = hero.boardLocation.location + d;
                while (BoardUtils.IsValid(l) && !context._GetCardsWithBoardLocation(l).Any())
                {
                    yield return new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("emerald01"), l, true),
                        new DelayAction(10));
                    l += d;
                }
            }
        }

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 3;
    }

    public class MimicStaffBehavior : BasicStaffCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Attack[/b]: trigger attack effects of all [b]equipped[/b] Weapons below";

        public override int GetSimpleAttackValue(IGameOrSimEntity card) => 1 + card.card.data.QualityModifier * 2;

        private static IEnumerable<(IGameOrSimEntity, IWeaponCardBehavior)> GetCardsInStackBelow(int minDepth, IGameOrSimContext context)
        {
            var cards = context._GetCardsWithCardArea(CardArea.HeroGearWeapon)
                .Where(c => c.hasID && c.hasCardInPileIndex && c.cardInPileIndex.index > minDepth)
                .Select(c => (card: c, behavior: CardRepository.GenerateCardBehaviorFromData(c.card.data) as IWeaponCardBehavior))
                .Where(t => t.behavior != null)
                .OrderByDescending(t => t.card.cardInPileIndex.index);
            foreach (var t in cards)
                yield return t;
        }

        // wrapper that encapsulates the nested trigger logic and the workaround to use a component for storing the current depth
        // NOTE: produces error in godot build, but that error does not really affect anything
        // failed to determine namespace and class for script: res://Card/MageWeaponsCardBehaviors.cs. Parse error: Unexpected token: Symbol
        // see https://github.com/godotengine/godot/issues/43751
        private class LogicWrapper : IDisposable, IEnumerator<(IGameOrSimEntity card, IWeaponCardBehavior behavior)> 
        {
            private readonly bool isRoot;
            private readonly int depth;
            private readonly IEnumerator<(IGameOrSimEntity, IWeaponCardBehavior)> cardsBelow;
            private readonly IGameOrSimEntity weapon;

            public LogicWrapper(IGameOrSimEntity weapon, IGameOrSimContext context)
            {
                this.weapon = weapon;
                isRoot = !weapon.hasMimicStaffDepthCounter;
                depth = weapon.hasMimicStaffDepthCounter ? weapon.mimicStaffDepthCounter.value : 0;
                cardsBelow = GetCardsInStackBelow(depth, context).GetEnumerator();
            }

            public (IGameOrSimEntity card, IWeaponCardBehavior behavior) Current => cardsBelow.Current;
            object IEnumerator.Current => cardsBelow.Current;

            public void Dispose()
            {
                if (isRoot && weapon.hasMimicStaffDepthCounter)
                    weapon.RemoveMimicStaffDepthCounter();
                cardsBelow.Dispose();
            }

            public bool MoveNext()
            {
                var b = cardsBelow.MoveNext();
                if (b)
                    weapon.ReplaceMimicStaffDepthCounter(cardsBelow.Current.Item1.cardInPileIndex.index);

                return b;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<IActionData> OnBeforeAttack(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context)
        {
            using var e = new LogicWrapper(weapon, context);
            while (e.MoveNext())
                foreach (var a in e.Current.behavior.OnBeforeAttack(hero, weapon, target, context))
                    yield return a;
        }

        public override IEnumerable<IActionData> OnPreAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType)
        {
            using var e = new LogicWrapper(weapon, context);
            while (e.MoveNext())
                foreach (var a in e.Current.behavior.OnPreAttackHits(hero, weapon, target, context, attackType))
                    yield return a;
        }

        public override IEnumerable<IActionData> OnPostAttackHits(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, AttackType attackType, BoardLocation? lastKnownTargetBoardLocation)
        {
            using var e = new LogicWrapper(weapon, context);
            while (e.MoveNext())
                foreach (var a in e.Current.behavior.OnPostAttackHits(hero, weapon, target, context, attackType, lastKnownTargetBoardLocation))
                    yield return a;
        }

        public override IEnumerable<IActionData> OnAfterAttacksHaveHit(IGameOrSimEntity hero, IGameOrSimEntity weapon, IGameOrSimEntity target, IGameOrSimContext context, CardType targetType)
        {
            using var e = new LogicWrapper(weapon, context);
            while (e.MoveNext())
                foreach (var a in e.Current.behavior.OnAfterAttacksHaveHit(hero, weapon, target, context, targetType))
                    yield return a;
        }
    }
}
