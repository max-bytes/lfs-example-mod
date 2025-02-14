using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Card
{

    public class WallOfIceScrollCardBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] [b]{iceShield}[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("iceShield", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Shield; }

        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex)
        { yield return ("iceShield", CardNames.Generate(new CardDataID("iceShield", quality))); }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CreateCardOnHeroAction(new CardDataID("iceShield", card.card.data.QualityModifier), forceEthereal: true);
        }
    }

    public class ForesightScrollCardBehavior : BasicScrollCardBehavior, ICantrippingCardBehavior
    {
        public override bool IsQuickCast { get; } = true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Cast[/b]: [b]flip[/b] each card, draw non-Scroll card";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickCast;
            yield return KeywordID.Replaceable;
        }
        public override int MaxUpgrade => 0;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var targets = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && !c.isCardFlipped && !c.isHero);
            var t = targets
                .OrderBy(t => BoardUtils.GetManhattenDistance(t.boardLocation.location, hero.boardLocation.location)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                .Select(target => new FlipCardAction(target.iD.value, true));
            yield return ActionService.CreateStaggeredParallelAction(t, 10);
        }
        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                var cardInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
                if (cardInDrawPile != null && cardInDrawPile.iD.value != cardID)
                {
                    yield return new DelayAction(14);
                    yield return new DrawCardAction(lastBoardLocation.Value, CardArea.HeroDrawPile, cardInDrawPile.iD.value);
                }
            }
        }
        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardUtils.GetCardType(candidate) != CardType.Scroll;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }

    public class LightningStrikeBehavior : BasicScrollCardBehavior
    {
        public override bool IsSlowCast { get; } = true;

        private int Lightning(int quality) => 8 + quality * 3;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Cast[/b]: place [b]{lightningRod}[/b], trigger [b]Lightning {damage}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Lightning(quality)); yield return ("lightningRod", CardNames.Generate(new CardDataID("lightningRod"))); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lightningRod");
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowCast;
            yield return KeywordID.LightningX;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Lightning; }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            // place lightning rod
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                yield return new CreateCardAtBoardLocationAction(new CardDataID("lightningRod"), lastBoardLocation.Value, false, followUp: (card) =>
                {
                    // HACK: we need to do a proper flip up action, because otherwise lightning rod's buff does not get activated properly
                    return new FlipCardAction(card.iD.value, true); 
                });
            }

            foreach (var a in CombatService.TriggerLightning(hero, Lightning(qualityModifier), context, HurtSource.Scroll, hero))
                yield return a;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // things happen in OnAfterCast()
            yield break; 
        }
    }

    public class GreaterHealScrollCardBehavior : BasicScrollCardBehavior
    {
        private int Heal(int quality) => 4 + quality * 1;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Heal; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HealTargetAction(hero.iD.value, Heal(card.card.data.QualityModifier), true);
            yield return new DelayAction(30);
        }
    }

    public class LesserHealScrollCardBehavior : BasicScrollCardBehavior
    {
        private int Heal(int quality) => 1 + quality * 1;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Heal; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HealTargetAction(hero.iD.value, Heal(card.card.data.QualityModifier), true);
            yield return new DelayAction(30);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class PreparationScrollCardBehavior : BasicScrollCardBehavior, ICantrippingCardBehavior
    {
        public override bool IsOneShot => true;

        public override int MaxUpgrade => 8;

        private int UpgradeTimes(int quality) => 1 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Draw Scroll and temporarily [b]upgrade[/b] it {upgradeTimes}x";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("upgradeTimes", UpgradeTimes(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Scroll;
            yield return KeywordID.Replaceable;
            yield return KeywordID.OneShot;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Scroll; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            // draw card
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                var scrollCardInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
                if (scrollCardInDrawPile != null && scrollCardInDrawPile.iD.value != cardID)
                {
                    yield return new DelayAction(14);
                    yield return new DrawCardAction(lastBoardLocation.Value, CardArea.HeroDrawPile, scrollCardInDrawPile.iD.value);
                    yield return new UpgradeCardAction(scrollCardInDrawPile.iD.value, UpgradeTimes(qualityModifier));
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
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardUtils.GetCardType(candidate) == CardType.Scroll;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }

    public class ReconsiderScrollCardBehavior : BasicScrollCardBehavior
    {
        public override int MaxUpgrade => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Return every Hero card in room into Deck and draw new ones";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Scroll;
            yield return KeywordID.OneShot;
            yield return KeywordID.Final;
        }
        public override bool IsOneShot => true;
        public override bool IsFinal => true;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            UndoService.ClearUndoState(GameType.DungeonRun); // NOTE: not very clean, but ok-ish
            yield break;
        }
        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            // find all hero cards
            var tuples = BoardUtils.GetAll()
                    .OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => (l, context._GetCardsWithBoardLocation(l).FirstOrDefault()))
                    .Where(t => t.Item2 != null)
                    .Where(t => CardUtils.IsHeroGear(t.Item2))
                    .ToList();

            foreach(var (l, card) in tuples)
            {
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }

            yield return new ShufflePileCardAction(CardArea.HeroDrawPile, false);

            var newLocations = tuples.Select(t => t.l);
            if (lastBoardLocation.HasValue)
                newLocations = newLocations
                    .Append(lastBoardLocation.Value)
                    .Distinct() // needed in exceptional cases like casting with owl vision
                    .OrderBy(l => BoardUtils.GetSortIndex(l));

            foreach (var l in newLocations)
            {
                yield return new DrawCardAction(l, CardArea.HeroDrawPile);
            }
        }
    }

    public class IncinerateScrollCardBehavior : BasicScrollCardBehavior
    {
        private int GetDamage(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to each card";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Scroll;
            yield return CardTag.FireDamage; 
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // TODO: fizzle?
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                    e => new HurtTargetAction(e.iD.value, GetDamage(card.card.data.QualityModifier), HurtSource.Scroll, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                )));
            yield return new DelayAction(30);
        }
    }

    public class ManaRainBehavior : BasicScrollCardBehavior
    {
        private int GetDamagePer(int quality) => 2 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to each card for every [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamagePer(quality)); yield return ("manaOrb", CardNames.Generate(new CardDataID("manaOrb"))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("manaOrb");
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.ManaOrb; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var numEquippedManaOrbs = context._GetCardsWithCardArea(CardArea.HeroGearArmor).Where(c => c.card.data.BaseID == "manaOrb").Count();
            var damage = numEquippedManaOrbs * GetDamagePer(card.card.data.QualityModifier);
            if (damage > 0)
            {
                yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                        e => new HurtTargetAction(e.iD.value, damage, HurtSource.Scroll, HurtType.Regular), // TODO: hurt type?
                        e => new SequenceAction(
                            new TriggerSoundAction("heroAttack"), // sound?
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", -Mathf.Pi / 2 } })),
                            new DelayAction(5)
                        )
                    )));
                yield return new DelayAction(30);
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }
    }

    public class PoisonCloudScrollCardBehavior : BasicScrollCardBehavior
    {
        private int PoisonDuration(int quality) => 5 + quality * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply [b]{poison} Poison[/b] to each Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("poison", PoisonDuration(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Poison; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // TODO: fizzle?
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                        e => new ApplyPoisonAction(e.iD.value, PoisonDuration(card.card.data.QualityModifier)),
                        e => new SequenceAction(
                            new TriggerSoundAction("poison"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("poison")),
                            new DelayAction(10)
                        )
                    )));
            yield return new DelayAction(30);
        }
    }

    public class RingOfFireScrollCardBehavior : BasicScrollCardBehavior
    {
        public override bool IsSlowCast { get; } = true;

        private int GetDamage(int quality) => 9 + quality * 4;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Cast[/b]: [b]flip[/b] 8-neighbors, deal [b]{damage} damage[/b] to each";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowCast;
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Scroll; 
            yield return CardTag.FireDamage; 
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // flip
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => !e.isCardFlipped,
                        e => new FlipCardAction(e.iD.value, true),
                        e => NoopAction.Instance,
                        requiresFlippedCard: false
                    )));

            // hurt
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Where(l => BoardUtils.Are8Neighbors(l, hero.boardLocation.location))
                .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, GetDamage(card.card.data.QualityModifier), HurtSource.Scroll, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                )));
            yield return new DelayAction(14);
        }
    }

    public class FireColumnScrollCardBehavior : BasicScrollCardBehavior
    {
        public override bool IsSlowCast { get; } = true;

        private int GetDamage(int quality) => 11 + quality * 4;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Slow Cast[/b]: flip cards in same column, deal [b]{damage} damage[/b] to each";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.SlowCast;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Scroll; 
            yield return CardTag.FireDamage; 
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // flip
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.AreOnSameCol(l, hero.boardLocation.location) && l != hero.boardLocation.location)
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => !e.isCardFlipped,
                        e => new FlipCardAction(e.iD.value, true),
                        e => NoopAction.Instance,
                        requiresFlippedCard: false
                    )));

            // hurt
            yield return new SequenceAction(BoardUtils.GetAll()
                .OrderBy(l => BoardUtils.GetManhattenDistance(l, hero.boardLocation.location)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                .Where(l => BoardUtils.AreOnSameCol(l, hero.boardLocation.location) && l != hero.boardLocation.location)
                .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                    e => new HurtTargetAction(e.iD.value, GetDamage(card.card.data.QualityModifier), HurtSource.Scroll, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                )));
            yield return new DelayAction(14);
        }
    }

    public class ForgedByFlamesScrollCardBehavior : BasicScrollCardBehavior
    {
        private int AdditionalAttack(int quality) => 10 + quality * 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Drop[/b] bottom Weapon, then top Weapon gets +{additionalAttack} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("additionalAttack", AdditionalAttack(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Weapon;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var wb = CombatService.GetBottomHeroWeapon(context, false);
            if (wb != null) 
                yield return new RequestDropCardAction(wb.iD.value, RequestDropReason.ForceDrop);

            var w = CombatService.GetTopHeroWeapon(context, false);
            if (w == null)
            {
                yield return new FizzleAction(hero.iD.value);
            }
            else
            {
                yield return new IncreaseAttackAction(w.iD.value, AdditionalAttack(card.card.data.QualityModifier), true);
            }
        }
    }

    public class PairOfShurikenScrollCardBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] 2 [b]{shuriken}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("shuriken", CardNames.Generate(new CardDataID("shuriken", quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("shuriken", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Shuriken; yield return CardTag.Ranged; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var quality = card.card.data.QualityModifier;
            if (card.hasBoardLocation)
            {
                var location = card.boardLocation.location;
                yield return new CreateCardOnHeroStartingFromBoardAction(new CardDataID("shuriken", quality), location, forceEthereal: true);
                yield return new CreateCardOnHeroStartingFromBoardAction(new CardDataID("shuriken", quality), location, forceEthereal: true);
            }
            else
            {
                yield return new CreateCardOnHeroAction(new CardDataID("shuriken", quality), forceEthereal: true);
                yield return new CreateCardOnHeroAction(new CardDataID("shuriken", quality), forceEthereal: true);
            }
        }
    }

    public class MissileBarrageCardBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Prepare[/b] 2 [b]{magicMissile}[/b], [b]Exert {exert}[/b]: [b]prepare[/b] 1 more";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("magicMissile", CardNames.Generate(new CardDataID("magicMissile", quality))); yield return ("exert", Exert); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.ExertX;
            yield return KeywordID.Prepare;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("magicMissile", card.card.data.QualityModifier);
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.MagicMissiles; yield return CardTag.Exert; }

        private int Exert => 2;
        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var quality = card.card.data.QualityModifier;
            yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);
            yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);

            foreach (var (ea, success) in CombatService.TryExert(Exert, context))
            {
                yield return ea;

                if (success)
                {
                    yield return new CreateCardOnHeroAction(new CardDataID("magicMissile", quality), forceEthereal: true);
                }
            }
        }
    }

    public class AtonementScrollCardBehavior : BasicScrollCardBehavior
    {
        private int GetDamage(int quality) => 6 + quality * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Flip and [b]destroy[/b] all\nSins. For each, deal [b]{damage} damage[/b] to Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", GetDamage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sins;
            yield return KeywordID.HolyDamage;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Sin; }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var sins = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && CardUtils.GetCardType(c) == CardType.Sin).ToList();

            if (sins.Count <= 0)
            {
                yield return new FizzleAction(hero.iD.value);
            } else
            {
                foreach(var s in sins)
                {
                    if (!s.isCardFlipped)
                    {
                        yield return new FlipCardAction(s.iD.value, true);
                        yield return new DelayAction(10);
                    }
                }
                yield return new SequenceAction(sins.OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location))
                .Select(c => new AffectCardAndLocationAction(c.boardLocation.location, false, (e) => true,
                    e => {
                        return new SequenceAction(
                            new DestroyCardAction(e.iD.value, DestroyCardReason.Fire, true),
                            new PermanentlyRemoveCardFromDeckAction(e.card.data) // necessary for original sin
                        );
                    },
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(c.boardLocation.location).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(10)
                    )
                )));

                yield return new DelayAction(30);

                var damage = sins.Count * GetDamage(card.card.data.QualityModifier);
                yield return new SequenceAction(new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                    e => new HurtTargetAction(e.iD.value, damage, HurtSource.Scroll, HurtType.Holy),
                    e => new SequenceAction(
                        new TriggerSoundAction("holy"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                        new DelayAction(10)
                    )
                ))), new DelayAction(30));
            }
        }
    }

    public class ShieldSlamScrollCardBehavior : BasicScrollCardBehavior
    {
        private int NumShields(int quality) => 1 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] {num} Shield from Deck, deal Defense to highest HP Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", NumShields(quality)); }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.HolyDamage;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Shield;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var shieldsInDrawPile = CombatService.GetTopNCardsInPile(context, CardArea.HeroDrawPile, NumShields(card.card.data.QualityModifier), ApplyDrawFilter);
            foreach(var shield in shieldsInDrawPile)
            {
                yield return new RequestEquipCardAction(shield.iD.value);
            }
            yield return new DelayAction(20);

            var targetedEnemyLocations = CombatService.GetHighestHPMonsters(context).Select(c => c.boardLocation.location).ToList();
            var damage = hero.defenseValue.value;
            if (damage <= 0 || targetedEnemyLocations.Count <= 0)
            {
                yield return new FizzleAction(hero.iD.value);
            } else
            {
                yield return new SequenceAction(targetedEnemyLocations
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                    e => new HurtTargetAction(e.iD.value, damage, HurtSource.Scroll, HurtType.Holy),
                    e => new SequenceAction(
                        new TriggerSoundAction("holy"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                        new DelayAction(10)
                    )
                )));
                yield return new DelayAction(30);
            }
        }

        private bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardRepository.GenerateCardBehaviorFromData(candidate.card.data) is BasicShieldCardBehavior;
    }

    public class BladeFlurryScrollCardBehavior : BasicScrollCardBehavior
    {
        private int DamagePer(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]{damage} damage[/b] to each Monster for every 2 equipped Weapons";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", DamagePer(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var weaponGearCards = context._GetCardsWithCardArea(CardArea.HeroGearWeapon);
            var damage = Mathf.FloorToInt(weaponGearCards.Count / 2) * DamagePer(card.card.data.QualityModifier);
            var targetedEnemies = context._GetEnemyCardsOnBoard(true);

            if (damage <= 0 || targetedEnemies.Count <= 0)
            {
                yield return new FizzleAction(hero.iD.value);
            } else
            {
                yield return new SequenceAction(targetedEnemies.Select(e => e.boardLocation.location).OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && !e.isHero,
                    e => new HurtTargetAction(e.iD.value, damage, HurtSource.Scroll, HurtType.Regular),
                    e => new SequenceAction(
                        new TriggerSoundAction("heroAttack"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", 0f } })),
                        new DelayAction(10)
                    )
                )));
                yield return new DelayAction(30);
            }
        }
    }

    public class RepossessionScrollCardBehavior : BasicScrollCardBehavior
    {
        private int Disarm(int quality) => 1 + Mathf.FloorToInt(quality / 2f);
        private int ShurikenModifier(int quality) => quality;// Mathf.CeilToInt(card.card.data.QualityModifier / 2f);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Each Monster gets {disarm} Attack, place [b]{shuriken}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("disarm", -Disarm(quality)); yield return ("shuriken", CardNames.Generate(new CardDataID("shuriken", ShurikenModifier(quality)))); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("shuriken", ShurikenModifier(card.card.data.QualityModifier));
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Disarm; yield return CardTag.Shuriken; yield return CardTag.Ranged; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var targetedEnemies = context._GetEnemyCardsOnBoard(true);

            yield return new SequenceAction(targetedEnemies.Select(e => e.boardLocation.location).OrderBy(l => BoardUtils.GetSortIndex(l))
            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.isEnemy,
                e => new SequenceAction(new DisarmAction(e.iD.value, Disarm(card.card.data.QualityModifier)), new DelayAction(5)),
                e => NoopAction.Instance
                )
            ));
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                yield return new DelayAction(30);
                yield return new SequenceAction(
                    new TriggerSoundAction("cardCreateOnBoard"),
                    new CreateCardAtBoardLocationAction(new CardDataID("shuriken", ShurikenModifier(qualityModifier)), lastBoardLocation.Value, forceEthereal: true),
                    new DelayAction(30)
                );
            }
        }
    }


    public class ShadowStrikeCardBehavior : BasicScrollCardBehavior
    {
        private int Stealth(int quality) => 3;
        private int AttackModifier(int quality) => 3 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gain [b]{stealth} Stealth[/b], top Weapon gets +{additionalAttack} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("stealth", Stealth(quality)); yield return ("additionalAttack", AttackModifier(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Stealth; }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyStealthAction(hero.iD.value, Stealth(card.card.data.QualityModifier), false);
            yield return new TriggerSoundAction("stealth");
            yield return new TriggerAnimationAction(context._GetBoardLocationEntity(hero.boardLocation.location).iD.value, new AnimationTrigger("stealth"));
            yield return new DelayAction(10);

            var w = CombatService.GetTopHeroWeapon(context, false);
            if (w != null)
            {
                yield return new IncreaseAttackAction(w.iD.value, AttackModifier(card.card.data.QualityModifier), true);
                yield return new DelayAction(10);
            }
        }
    }

    public class ShroudBehavior : BasicScrollCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 2 + card.card.data.QualityModifier;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Omni-Cast[/b], [b]Quick Cast[/b]: gain [b]1 Stealth[/b]";
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Stealth; }
        public override bool IsQuickCast => true;
        public override bool HasOmniCast => true;
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
            yield return KeywordID.QuickCast;
            yield return KeywordID.MultipleScrollUses;
            yield return KeywordID.OmniCast;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyStealthAction(hero.iD.value, 1, false);
            yield return new TriggerSoundAction("stealth");
            yield return new TriggerAnimationAction(context._GetBoardLocationEntity(hero.boardLocation.location).iD.value, new AnimationTrigger("stealth"));
            yield return new DelayAction(10);
        }
    }

    public class ArbitrageBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose {cost} Emeralds[/b], place [b]10 Emeralds[/b] at 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("cost", Cost(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Loot; }

        private int Cost(int quality) => Math.Max(1, 25 - quality * 3);

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald10");
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new LoseGoldAction(Cost(card.card.data.QualityModifier), null);
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            yield return new SequenceAction(
                BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .Select(l => new SequenceAction(
                        new TriggerSoundAction("cardCreateOnBoard"),
                        new CreateCardAtBoardLocationAction(new CardDataID("emerald10"), l, true),
                        new DelayAction(10)
                    ))
            );
        }
    }


    public class HammerAndAnvilBehavior : BasicScrollCardBehavior, ICantrippingCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Equip[/b] Hammer from Deck, top Armor gets +{defense} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("defense", Defense(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Hammer; }

        private int Defense(int quality) => 5 + quality * 2;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var mayFizzle = true;
            var hammerInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
            if (hammerInDrawPile != null)
            {
                yield return new RequestEquipCardAction(hammerInDrawPile.iD.value);
                mayFizzle = false;
            }

            var topArmor = CombatService.GetTopHeroArmor(context);
            if (topArmor != null)
            {
                yield return new ModifyDefenseValueModifierAction(topArmor.iD.value, Defense(card.card.data.QualityModifier));
                yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                yield return new TriggerAnimationAction(topArmor.iD.value, new AnimationTrigger("upgrade"));
                mayFizzle = false;
            }

            if (mayFizzle)
                yield return new FizzleAction(card.iD.value);
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.Equip;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardRepository.GenerateCardBehaviorFromData(candidate.card.data) is BasicHammerCardBehavior;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }


    public class ApothecaryListBehavior : BasicScrollCardBehavior
    {
        public override bool IsOneShot => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Add {num} random Potions to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", Num(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
            yield return KeywordID.Potion;
            yield return KeywordID.OneShot;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Scroll;
            yield return CardTag.Potion;
        }

        public override bool IsFinal => true;

        private int Num(int quality) => 5 + quality * 1;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var boardLocation = card.hasBoardLocation ? card.boardLocation.location : hero.boardLocation.location;

            UndoService.ClearUndoState(GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            var seed = context.GetRandomSeedForIngameRoom(card.iD.value.ID);
            var potions = PotionUtils.GetRandomPotions(seed, Num(card.card.data.QualityModifier), 0);
            yield return new SequenceAction(potions.Select(p => new SequenceAction(new PermanentlyAddCardToDeckAction(p), new CreateCardInDrawPileStartingFromBoardAction(p, boardLocation, true))));
        }
    }


    public class BoosterPackBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Add {num} random Cards to Deck and remove itself";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", Num(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Scroll;
        }

        public override int MaxUpgrade => 0;

        public override bool IsFinal => true;

        private int Num(int quality) => 8;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            UndoService.ClearUndoState(GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            // gain random cards
            var m = MetaSaveSystem.LoadOrDefault();
            var gameState = GameStateService._GetDungeonRunOrDefault();
            var seed = context.FloorSeed;
            var deck = context.DungeonRunDeckEntity?.dungeonRunDeck?.cards ?? new CardDataID[0];
            var hardBannedCards = HeroXPUtils.CurrentlyLockedCardsForHero(gameState.HeroClass, m.GetHeroXPLevel(gameState.HeroClass)).Concat(m.GetVetoedCards(gameState.HeroClass)).Distinct().ToArray();
            var randomCards = LootDropUtils.GetRandomEquipmentCards(Num(card.card.data.QualityModifier), gameState.NonLoopingFloor, gameState.HeroClass, deck, gameState.TargetID, seed, DungeonRunBuilder.LootLevel(gameState.TargetID, gameState.NonLoopingFloor), hardBannedCards, Array.Empty<CardDataID>());
            foreach (var cardID in randomCards)
            {
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true, 0);
            }

            yield return new PermanentlyRemoveCardFromDeckAction(card.card.data);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class DoubleCastBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Prepare[/b] a copy of top [b]prepared[/b] Spell, [b]upgrade[/b] it {times}x";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("times", QualityIncrease(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Prepare;
            yield return KeywordID.Spell;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Spell; }

        private int QualityIncrease(int quality) => quality + 1;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var target = CombatService.GetTopHeroSpell(context, false);
            if (target != null)
            {
                yield return new CreateCardOnHeroAction(target.card.data, forceEthereal: true);
                yield return new DelayAction(20);

                // upgrade
                var newTarget = CombatService.GetTopHeroSpell(context, true); // HACK
                if (newTarget != null)
                    yield return new UpgradeCardAction(newTarget.iD.value, QualityIncrease(card.card.data.QualityModifier));
            } else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }
    }

    public class SampleTastingBehavior : BasicScrollCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Trigger the effects of all Potions in the deck";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Potion;
        }
        public override IEnumerable<CardTag> RequiresTags() { yield return CardTag.Potion; }

        public override int MaxUpgrade => 0;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var potionsInDeck = CombatService.GetCardsInPileOrdered(context, CardArea.HeroDrawPile, c => CardUtils.GetCardType(c) == CardType.Potion).ToArray();
            if (potionsInDeck.Length > 0)
            {
                foreach(var potion in potionsInDeck)
                {
                    if (CardRepository.GenerateCardBehaviorFromData(potion.card.data) is BasicPotionCardBehavior pb)
                    {
                        foreach(var a in pb.OnConsume(hero, potion.card.data, context))
                            yield return a;
                        yield return new DelayAction(20);
                    }
                }
            }
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }
    }

    public class ImprovisedTrapBehavior : BasicScrollCardBehavior
    {
        private int Sleep(int quality) => 6 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Draw Monster, apply [b]{sleep} Sleep[/b] and place [b]O-Barrels[/b] at its 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("sleep", Sleep(quality)); }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Scroll; yield return CardTag.Sleep; }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Sleep;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("oBarrel");
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // things happen in OnAfterCast()
            yield break;
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                var monsterInDrawPile = CombatService.GetTopCardInPile(context, CardArea.EnemyDrawPile);
                if (monsterInDrawPile != null && monsterInDrawPile.hasID)
                {
                    // draw
                    yield return new DrawCardAction(lastBoardLocation.Value, CardArea.EnemyDrawPile, monsterInDrawPile.iD.value);
                    if (monsterInDrawPile.hasID)
                        yield return new FlipCardAction(monsterInDrawPile.iD.value, true);

                    // sleep
                    if (monsterInDrawPile.hasID)
                        yield return new TrySleepAction(monsterInDrawPile.iD.value, Sleep(qualityModifier));
                }

                // barrels
                var emptyLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, lastBoardLocation.Value))
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

            }
            else
            {
                yield return new FizzleAction(hero.iD.value);
            }
        }
    }

    public class DivineFavorCardBehavior : BasicScrollCardBehavior, ICantrippingCardBehavior
    {
        private int Damage(int quality) => 1 + quality;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to each Monster, draw Armor";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Armor;
            yield return KeywordID.Replaceable;
            yield return KeywordID.HolyDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.HolyDamage; }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var damage = Damage(card.card.data.QualityModifier);
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
            .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                e => new HurtTargetAction(e.iD.value, damage, HurtSource.Scroll, HurtType.Holy),
                e => new SequenceAction(
                    new TriggerSoundAction("holy"),
                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("holy")),
                    new DelayAction(10)
                )
            )));
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                var cardInDrawPile = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, ApplyDrawFilter);
                if (cardInDrawPile != null && cardInDrawPile.iD.value != cardID)
                {
                    yield return new DelayAction(14);
                    yield return new DrawCardAction(lastBoardLocation.Value, CardArea.HeroDrawPile, cardInDrawPile.iD.value);
                }
            }
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardUtils.GetCardType(candidate) == CardType.Armor;
    }

    public class EscalationBehavior : BasicScrollCardBehavior
    {
        private int Increase(int quality) => 1 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Cast[/b]: top Weapon gets +{increase} Use, draw Monster";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("increase", Increase(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickCast;
            yield return KeywordID.MultipleWeaponUses;
            yield return KeywordID.Weapon;
        }

        public override bool IsQuickCast { get; } = true;

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var w = CombatService.GetTopHeroWeapon(context, false);
            if (w == null)
            {
                yield return new FizzleAction(hero.iD.value);
            }
            else
            {
                yield return new ModifyStickyOffsetAction(w.iD.value, Increase(card.card.data.QualityModifier));
                yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                yield return new TriggerAnimationAction(w.iD.value, new AnimationTrigger("upgrade"));
            }
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            if (lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
            {
                var monsterInDrawPile = CombatService.GetTopCardInPile(context, CardArea.EnemyDrawPile);
                if (monsterInDrawPile != null && monsterInDrawPile.hasID)
                {
                    // draw
                    yield return new DrawCardAction(lastBoardLocation.Value, CardArea.EnemyDrawPile, monsterInDrawPile.iD.value);
                    if (monsterInDrawPile.hasID)
                        yield return new FlipCardAction(monsterInDrawPile.iD.value, true);
                }
            }
        }

        public override int MaxUpgrade => 3;
    }

    public class HeavyReadingBehavior : BasicScrollCardBehavior, ICantrippingCardBehavior
    {
        protected override int StickyBase(IGameOrSimEntity card, IGameOrSimContext context) => 2;
        private int Push(int quality) => 2 + quality * 2;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Swap place[/b], [b]push for {push}[/b], return to Deck: draw Aura";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("push", Push(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.MultipleScrollUses;
            yield return KeywordID.Replaceable;
            yield return KeywordID.Aura;
        }
        public override IEnumerable<CardTag> RequiresTags()
        {
            yield return CardTag.Aura;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.Push;
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.hasImmunityToFalling = true;
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!hero.hasBoardLocation || !card.hasBoardLocation || hero.boardLocation.location == card.boardLocation.location)
            {
                yield return new FizzleAction(hero.iD.value);
                yield break;
            }

            yield return new ParallelAction(new MoveCardOnBoardAction(card.iD.value, hero.boardLocation.location, MoveReason.Swap), new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Swap));

            var dir = BoardUtils.GetCardinalOrDiagonalDirection(hero.boardLocation.location, card.boardLocation.location);
            yield return new PushAction(card.iD.value, Push(card.card.data.QualityModifier), dir);
        }

        public override IEnumerable<IActionData> OnAfterCast(IGameOrSimEntity hero, EntityID cardID, int qualityModifier, IGameOrSimContext context, BoardLocation? lastBoardLocation)
        {
            var topAura = CombatService.GetTopNCardsInPile(context, CardArea.HeroDrawPile, 1, ApplyDrawFilter).FirstOrDefault();
            if (topAura != null && lastBoardLocation.HasValue && !context._GetCardsWithBoardLocation(lastBoardLocation.Value).Any())
                yield return new DrawCardAction(lastBoardLocation.Value, CardArea.HeroDrawPile, topAura.iD.value);
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => CardRepository.GenerateCardBehaviorFromData(candidate.card.data) is BasicAuraCardBehavior;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
    }


    public class FireAndIceBehavior : BasicScrollCardBehavior
    {
        private int Increase(int quality) => 1 + quality;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Place [b]{lingeringFlame}[/b] at 4-neighbor spots, all [b]Timers[/b] get +{increase}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("lingeringFlame", CardNames.Generate(new CardDataID("lingeringFlame"))); yield return ("increase", Increase(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.Timer;
            yield return KeywordID.FireDamage;
        }
        public override IEnumerable<CardTag> ProvidesTags()
        {
            yield return CardTag.LingeringFlame;
            yield return CardTag.FireDamage;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("lingeringFlame");
        }

        public override IEnumerable<IActionData> OnCast(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!hero.hasBoardLocation)
                yield return new FizzleAction(hero.iD.value);
            else
            {
                var freeLocations = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
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

            // timers
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasChannelling,
                    e => new ModifyChannelingAction(e.iD.value, Increase(card.card.data.QualityModifier), false),
                    e => new SequenceAction(
                        new TriggerSoundAction("heroUpgrade"),
                        new DelayAction(15)
                    )
                )));

        }

        public override int MaxUpgrade => 5;
    }

}