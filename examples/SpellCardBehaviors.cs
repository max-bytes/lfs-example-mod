using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Action;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public class FlameWallBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Health;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to cards in this row and place [b]{lingeringFlame}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"))); }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Spell; 
            yield return CardTag.FireDamage; 
            yield return CardTag.LingeringFlame;
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
        }

        private int GetDamage(int quality) => 5 + quality * 3;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var t = BoardUtils.GetAll()
                .OrderBy(l => BoardUtils.GetSortIndex(l))
                //.OrderBy(l => BoardUtils.GetManhattenDistance(l, targetLocation)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                .Where(l => BoardUtils.AreOnSameRow(l, targetLocation))
                .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, GetDamage(spell.card.data.QualityModifier), HurtSource.Spell, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                ));
            yield return new SequenceAction(new SequenceAction(t), new DelayAction(14));

            var t2 = BoardUtils.GetAll()
                .OrderBy(l => BoardUtils.GetSortIndex(l))
                //.OrderBy(l => BoardUtils.GetManhattenDistance(l, targetLocation)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                .Where(l => BoardUtils.AreOnSameRow(l, targetLocation));


            // place lingering flames
            var freeLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.AreOnSameRow(l, targetLocation))
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

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasHealth && target.isCardFlipped && !target.isHero;
        }
    }

    public class MeteorBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Health;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damageCenter} damage[/b] to target and [b]{damageOthers} damage[/b] to 8-neighbors";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damageCenter", DamageCenter(quality)); yield return ("damageOthers", DamageOthers(quality)); }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Spell;
            yield return CardTag.FireDamage;
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.FireDamage;
        }

        private int DamageCenter(int quality) => 7 + quality * 2;
        private int DamageOthers(int quality) => 4 + quality * 1;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            yield return new AffectCardAndLocationAction(targetLocation, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, DamageCenter(spell.card.data.QualityModifier), HurtSource.Spell, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(targetLocation).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(15)
                    )
                );

            var t = BoardUtils.GetAll()
                .OrderBy(l => BoardUtils.GetSortIndex(l))
                .Where(l => BoardUtils.Are8Neighbors(l, targetLocation))
                .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, DamageOthers(spell.card.data.QualityModifier), HurtSource.Spell, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                ));
            yield return new SequenceAction(new SequenceAction(t), new DelayAction(14));
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasHealth && target.isCardFlipped && !target.isHero;
        }
    }

    public class MagicMissileBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Health;

        // TODO: specify quick cast if quick cast buff is active
        //public override bool IsQuickCast => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Deal [b]{damage} damage[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("quickCast", ""); }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", GetDamage(card, context)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.MagicMissiles; }

        protected int GetDamage(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var modifiers = context.FilterBuffs<MagicMissileDamageBuff>().Sum(b => b.Modifier);
            return 3 + card.card.data.QualityModifier * 1 + modifiers;
        }

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var card = context._GetCardsWithBoardLocation(targetLocation).FirstOrDefault();
            if (card != null)
            {
                if (context.FilterBuffs<MagicMissileQuickCastBuff>().Any())
                    yield return new SetQuickTurnAction(true);

                yield return new VisualHeroRangedAttackAction(hero.iD.value, card.iD.value, () => new SequenceAction(
                    new SpawnProjectileAction(ProjectileType.MageAttack, hero, card),
                    new HurtTargetAction(card.iD.value, GetDamage(spell, context), HurtSource.Spell, HurtType.Regular) // TODO hurt-type?
                    ));
                // TODO: sound, ...
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasHealth && target.isCardFlipped && !target.isHero;
        }
    }

    // TODO: remove eventually
    public class ConjureLightningBehavior : BasicSpellCardBehavior
    {
        public override bool IsQuickCast => true;
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Cast[/b]: place [b]Lightning Orb[/b] at [b]Origin[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickCast;
            yield return KeywordID.Origin;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lightningOrb");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.LightningOrb; yield return CardTag.Lightning; }

        public override int MaxUpgrade => 0; // TODO?

        public override bool IsOriginRelevantWhenCasting(IGameOrSimContext context) => true;
        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            yield return new SetQuickTurnAction(true);

            if (hero.hasPreviousBoardLocation && !context._GetCardsWithBoardLocation(hero.previousBoardLocation.location).Any())
            {
                // TODO: animation, sound
                yield return new CreateCardAtBoardLocationAction(new CardDataID("lightningOrb"), hero.previousBoardLocation.location, true);
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }
    }

    public class ConjureFlamesBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Place [b]{lingeringFlame}[/b] at [b]Origin[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Origin;
            yield return KeywordID.MultipleSpellUses;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.LingeringFlame; }

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;

        public override bool IsOriginRelevantWhenCasting(IGameOrSimContext context) => true;
        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            if (hero.hasPreviousBoardLocation && !context._GetCardsWithBoardLocation(hero.previousBoardLocation.location).Any())
            {
                // TODO: animation, sound
                yield return new CreateCardAtBoardLocationAction(new CardDataID("lingeringFlame"), hero.previousBoardLocation.location, flipUp: true);
                yield return new DelayAction(20);
            }
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }
    }

    public class ChainLightningBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Trigger [b]Lightning {damage}[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.MultipleSpellUses;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.Lightning; }

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1;

        protected int GetDamage(int quality) => 3 + quality * 1;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            foreach (var a in CombatService.TriggerLightning(hero, GetDamage(spell.card.data.QualityModifier), context, HurtSource.Spell, hero))
                yield return a;
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.isHero;
        }
    }

    public class OwlVisionBehavior : BasicSpellCardBehavior
    {
        public override bool IsQuickCast => true;
        public override SpellTarget SpellTarget => SpellTarget.Scroll;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Prepare[/b]: [b]flip[/b] each Scroll, [b]Quick Cast[/b]: [b]cast[/b] target Scroll";
        }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Scroll;
            yield return KeywordID.Prepare;
            yield return KeywordID.QuickCast;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Scroll; }

        public override IEnumerable<IActionData> OnPrepare(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var targets = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && !c.isCardFlipped && c.hasCard && CardUtils.GetCardType(c.card.data) == CardType.Scroll);
            var t = targets
                .OrderBy(t => BoardUtils.GetManhattenDistance(t.boardLocation.location, hero.boardLocation.location)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                .Select(target => new FlipCardAction(target.iD.value, true));
            yield return ActionService.CreateStaggeredParallelAction(t, 10);
        }

        public override int MaxUpgrade => 0;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var target = context._GetCardsWithBoardLocation(targetLocation).FirstOrDefault();
            if (target != null)
            {
                var targetBehavior = CardRepository.GenerateCardBehaviorFromData(target.card.data);
                if (targetBehavior is BasicScrollCardBehavior scb)
                {
                    var targetID = target.iD.value;
                    var quality = target.card.data.QualityModifier;
                    foreach (var a in scb.OnCast(hero, target, context))
                        yield return a;
                    foreach (var a in scb.OnAfterCast(hero, targetID, quality, context, targetLocation))
                        yield return a;
                }
            }
            yield return new SetQuickTurnAction(true);
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && CardUtils.GetCardType(target.card.data) == CardType.Scroll;
        }
    }

    public class RearrangementBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Teleport;

        private int Teleport(int quality) => 1;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Prepare[/b]: gain [b]{teleport} Teleport Charge[/b], [b]on teleport[/b]: draw card at source";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("teleport", Teleport(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.TeleportCharge;
            yield return KeywordID.Replaceable;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.Teleport; }

        public override IEnumerable<IActionData> OnPrepare(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyTeleportCounterAction(hero.iD.value, Teleport(card.card.data.QualityModifier), false);
            yield return new DelayAction(20);
        }

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            // all happens on trigger
            yield break;
        }

        public override int MaxUpgrade => 0;

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return false; // casting happens on trigger
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

            public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterTeleportPlaceholderAction;
            public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
            {
                if (a is AfterTeleportPlaceholderAction ata)
                {
                    if (!self.hasCardArea || self.cardArea.Area != CardArea.HeroGearWeapon || !self.hasCardInPileIndex || self.cardInPileIndex.index != 0)
                        yield break;

                    var entity = context.GameOrSimContext._GetEntityWithID(ata.EntityID);
                    if (entity == null || !entity.isHero)
                        yield break;

                    yield return new CastSpellPlaceholderAction(); // because we are not casting the spell directly, but indirectly
                    yield return new ActivateCardAction(self.iD.value);

                    var behavior = CardRepository.GenerateCardBehaviorFromData(self.card.data) as RearrangementBehavior;

                    if (!context.GameOrSimContext._GetCardsWithBoardLocation(ata.locationBeforeTeleport).Any())
                    {
                        yield return new DrawCardAction(ata.locationBeforeTeleport, CardArea.HeroDrawPile);
                    } else
                    {
                        yield return new FizzleAction(context.GameOrSimContext.HeroEntity.iD.value);
                    }

                    yield return new RequestDropCardAction(self.iD.value, RequestDropReason.Used);
                    yield return new DeactivateCardAction(self.iD.value);
                }
            }
        }
    }

    public class FlameDanceBehavior : BasicSpellCardBehavior
    {
        public override bool IsQuickCast => true;
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Cast[/b]: deal [b]{damage} damage[/b] to each 8-neighbor card";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.MultipleSpellUses;
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.QuickCast;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.FireDamage; }

        private int GetDamage(int quality) => 2;

        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 1 + card.card.data.QualityModifier;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            yield return new SetQuickTurnAction(true);

            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, GetDamage(spell.card.data.QualityModifier), HurtSource.Spell, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                ));
            yield return new SequenceAction(new SequenceAction(t), new DelayAction(14));
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }
    }

    public class MeticulousStudyBehavior : BasicSpellCardBehavior, ICantrippingCardBehavior
    {
        public override bool IsQuickCast => true;
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Cast[/b]: draw card at [b]Origin[/b], [b]Exert {exert}[/b]: +1 Use";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("exert", Exert); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExertX;
            yield return KeywordID.QuickCast;
            yield return KeywordID.Replaceable;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        { 
            yield return CardTag.Exert;
            yield return CardTag.Spell;
        }

        private int Exert => 4;

        public override int MaxUpgrade => 0;

        public override bool IsOriginRelevantWhenCasting(IGameOrSimContext context) => true;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            yield return new SetQuickTurnAction(true);

            // draw card
            if (hero.hasPreviousBoardLocation && !context._GetCardsWithBoardLocation(hero.previousBoardLocation.location).Any())
            {
                var cardInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile);
                if (cardInDrawPile != null)
                {
                    yield return new DelayAction(14);
                    yield return new DrawCardAction(hero.previousBoardLocation.location, CardArea.HeroDrawPile, cardInDrawPile.iD.value);
                }
                else
                {
                    yield return new FizzleAction(hero.iD.value);
                }
            }
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }

            // exert
            foreach (var (ea, success) in CombatService.TryExert(Exert, context))
            {
                yield return ea;

                if (success)
                {
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(spell.iD.value, new AnimationTrigger("upgrade"));
                    yield return new ModifyStickyOffsetAction(spell.iD.value, 1);
                    yield return new DelayAction(20);
                }
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => true;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.CastSpell;
    }

    public class BarrelmancyBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Health;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Place [b]O-Barrels[/b] at 4-neighbor spots";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("oBarrel");
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; }

        public override int MaxUpgrade => 0;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var emptyLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, targetLocation))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .ToList();

            if (emptyLocations.Any())
            {
                yield return new SequenceAction(emptyLocations
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("oBarrel"), l, true),
                        new DelayAction(10))));
            } 
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasHealth && target.isCardFlipped && !target.isHero;
        }
    }

    public class CallTheStormBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Place [b]{lightningRod}[/b] at 4-neighbor spots, [b]Exert {exert}[/b]: trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("exert", Exert(quality)); yield return ("damage", Damage(quality)); yield return ("lightningRod", CardNames.Generate(new CardDataID("lightningRod"))); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lightningRod");
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExertX;
            yield return KeywordID.LightningX;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.Lightning; yield return CardTag.Exert; }

        private int Damage(int quality) => 8 + quality * 2;
        private int Exert(int quality) => 6;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("lightningRod"), l, followUp: (card) =>
                        {
                            // HACK: we need to do a proper flip up action, because otherwise lightning rod's buff does not get activated properly
                            return new FlipCardAction(card.iD.value, true);
                        })
                    ))
            );

            foreach (var (ea, success) in CombatService.TryExert(Exert(spell.card.data.QualityModifier), context))
            {
                yield return ea;

                if (success)
                {
                    foreach (var a in CombatService.TriggerLightning(hero, Damage(spell.card.data.QualityModifier), context, HurtSource.Spell, hero))
                        yield return a;
                }
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }
    }

    public class ArcaneBarrierBehavior : BasicSpellCardBehavior
    {
        public override SpellTarget SpellTarget => SpellTarget.Hero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Prepare[/b] 2 [b]{magicMissile}[/b], place [b]Illusory Walls[/b] at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("magicMissile", CardNames.Generate(new CardDataID("magicMissile", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prepare;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("magicMissile", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.MagicMissiles; }

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var quality = spell.card.data.QualityModifier;
            yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);
            yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);

            if (hero.hasBoardLocation)
                yield return new SequenceAction(
                    BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                        .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                        .Select(l => new SequenceAction(
                            new TriggerSoundAction("cardCreateOnBoard"),
                            new CreateCardAtBoardLocationAction(new CardDataID("illusoryWall"), l),
                            new DelayAction(10)))
                );
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasCard && target.isCardFlipped && target.isHero;
        }
    }

    public class CircuitCloserBehavior : BasicSpellCardBehavior
    {
        private int Damage(int quality) => 4 + quality * 2;
        public override SpellTarget SpellTarget => SpellTarget.Health;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Move target to [b]Origin[/b], trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Origin;
            yield return KeywordID.LightningX;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Spell; yield return CardTag.Lightning; }

        public override bool IsOriginRelevantWhenCasting(IGameOrSimContext context) => true;

        public override IEnumerable<IActionData> _OnCast(GameEntity hero, IGameOrSimEntity spell, BoardLocation targetLocation, GameContext context)
        {
            var target = context._GetCardsWithBoardLocation(targetLocation).FirstOrDefault();
            if (target != null && target.hasID)
            {
                // teleport
                if (hero.hasPreviousBoardLocation && !context._GetCardsWithBoardLocation(hero.previousBoardLocation.location).Any())
                {
                    // TODO: animation, sound
                    yield return new MoveCardOnBoardAction(target.iD.value, hero.previousBoardLocation.location, MoveReason.Teleport);
                }

                foreach (var a in CombatService.TriggerLightning(hero, Damage(spell.card.data.QualityModifier), context, HurtSource.Spell, hero))
                    yield return a;
            }
        }

        public override bool IsValidTarget(IGameOrSimEntity hero, IGameOrSimEntity spell, IGameOrSimEntity target, BoardLocation targetLocation, IGameOrSimContext context)
        {
            return target.hasHealth && target.isCardFlipped && !target.isHero;
        }
    }
}
