using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Action;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{

    public class IllusoryWallCardBehavior : BasicContraptionCardBehavior
    {
        public override bool IsQuickActivate { get; } = true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Interact[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickInteract;
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new SetQuickTurnAction(true);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }
    public class SimpleIllusoryWallCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Interact[/b]: remove";

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }
    public class BarredDoorCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }
    public class NarrowTunnelBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Add 2 [b]{claustrophobia}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("claustrophobia", CardNames.Generate(claustrophobiaCardID, lang)); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return claustrophobiaCardID;
        }

        private static readonly CardDataID claustrophobiaCardID = new CardDataID("claustrophobia");

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("pickupSin"); // TODO
            yield return new CreateCardInDrawPileStartingFromBoardAction(claustrophobiaCardID, card.boardLocation.location, true, 20);
            yield return new PermanentlyAddCardToDeckAction(claustrophobiaCardID);
            yield return new TriggerSoundAction("pickupSin"); // TODO
            yield return new CreateCardInDrawPileStartingFromBoardAction(claustrophobiaCardID, card.boardLocation.location, true, 20);
            yield return new PermanentlyAddCardToDeckAction(claustrophobiaCardID);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class StrangeChestCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], place [b]20 Emeralds[/b] at its spot?";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
            yield return KeywordID.Eager;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald20");
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override bool IsFinal => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            UndoService.ClearUndoState(Utils.GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            yield return new DelayAction(40);

            var seed = context.GetRandomSeedForIngameRoom(card.iD.value.ID);
            var random = new MyRandom(seed);
            var spawnMimic = random.Chance(0.2f);
            if (spawnMimic)
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("unlucky"));
                yield return new TriggerSoundAction("fizzled"); // TODO: better sound
                yield return new DelayAction(20);
            }

            var boardLocation = (card.hasBoardLocation) ? card.boardLocation.location : BoardLocation.Create(0, 0);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            yield return new TriggerSoundAction("cardCreateOnBoard");
            if (spawnMimic)
                yield return new CreateCardAtBoardLocationAction(new CardDataID("mimic"), boardLocation);
            else
                yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald20"), boardLocation);

            context.UpdateDungeonRunStats(old => old.AddOpenedChest());
        }
    }

    public class EitherDoorCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Turn other [b]Either Door[/b] into [b]Wall[/b]";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("wall");
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

            foreach (var otherDoor in context._GetCardsWithCardArea(CardArea.Board).Where(c => c.card.data.BaseID == "eitherDoor" && c.iD.value != card.iD.value))
            {
                yield return new CreateCardAtBoardLocationAction(new CardDataID("wall"), otherDoor.boardLocation.location, true);
                yield return new DestroyCardAction(otherDoor.iD.value, DestroyCardReason.Other);
            }
        }
    }

    public class CellDoorCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Small Key[/b] required";

        public override BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            var keys = gameOrSimContext.CollectedKeysEntity.collectedKeys.value;
            return (keys > 0 && card.hasReachableVia) ? card.reachableVia.smoothedOnTargetPath : null;
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var keys = context.CollectedKeysEntity.collectedKeys.value;
            if (keys > 0)
            {
                yield return new AnimateSpendKeyAction(ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location));
                yield return new SpendKeyAction();
                yield return new TriggerSoundAction("unlock");
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }
    }

    public class SpikesCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Take [b]{damage} damage[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage); }

        private int Damage => 6;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, Damage, HurtSource.Contraption, HurtType.Regular, false);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }
    }

    public class PushWallBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Interact[/b]: [b]push for 3[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickInteract;
            yield return KeywordID.Push;
        }
        public override bool IsQuickActivate => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (hero.hasBoardLocation && card.hasBoardLocation)
            {
                yield return new SetQuickTurnAction(true);
                yield return new DelayAction(15);
                var dir = BoardUtils.GetCardinalDirection(hero.boardLocation.location, card.boardLocation.location);
                yield return new PushAction(card.iD.value, 3, dir);
            }
            if (card.hasID)
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimEntity card, IGameOrSimContext gameOrSimContext)
        {
            yield return new TriggerSoundAction("rockDestroy");
        }
    }

    public class HerotrapBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Lose [b]{damage} HP[/b], gain [b]{emeralds} Emeralds[/b] (usable multiple times)";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage); yield return ("emeralds", Emeralds); }

        private int Damage => 3;
        private int Emeralds => 5;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, Damage, HurtSource.Contraption, HurtType.Regular, true);
            yield return new DelayAction(30);
            var cardPosition = ((GameEntity)card).position.value;
            yield return new CollectGoldAction(Emeralds, cardPosition);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }
    }

    public class PaywallBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]{emeralds} Emeralds[/b] required";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("emeralds", Emeralds); }

        private int Emeralds => 20;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var curGold = context.CollectedGoldEntity.collectedGold.value;

            if (curGold >= Emeralds)
            {
                yield return new LoseGoldAction(Emeralds, card.boardLocation.location);
                yield return new DelayAction(20);
                yield return new TriggerSoundAction("unlock");
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            } else
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("denied"));
                yield return new TriggerSoundAction("fizzled"); // TODO
            }
        }
    }

    public class RopeBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gain option to [b]skip Floor I[/b] in next run";

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (GameStateService.Get() is DungeonRunState drs)
            {
                // make sure player does not undo this
                UndoService.ClearUndoState(GameType.DungeonRun); // NOTE: not very clean, but ok-ish

                var mOld = MetaSaveSystem.LoadOrDefault();
                var mNew = mOld.AddMiscTrigger(TheRopeQuestTemplate.BuildTriggerName(drs.TargetID, drs.Difficulty, drs.HeroClass));
                MetaSaveSystem.Save(mNew);
            }

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

            yield return new DelayAction(30);
            yield return new TriggerSoundAction("roomCleared");
            yield return new DelayAction(30);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }
    }

    public class SecretTunnelBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], [b]Interact from anywhere[/b]: teleport Hero to its spot";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
        }
        protected override bool HasOmniInteract => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                var boardLocation = card.boardLocation.location;
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                yield return new DelayAction(15);
                yield return new MoveCardOnBoardAction(hero.iD.value, boardLocation, MoveReason.Teleport);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }
    }

    public class ProtectionShrineBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Top Armor gets +3 Defense, Monsters get +1 Defense";

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

                var topArmor = CombatService.GetTopHeroArmor(context);
                if (topArmor != null)
                {
                    yield return new ModifyDefenseValueModifierAction(topArmor.iD.value, 3);
                    yield return new TriggerSoundAction(new SoundTrigger("heroUpgrade"));
                    yield return new TriggerAnimationAction(topArmor.iD.value, new AnimationTrigger("upgrade"));
                    yield return new DelayAction(10);
                }

                var enemies = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && c.isCardFlipped && c.hasHealth && c.isEnemy)
                    .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location)).ToList();
                foreach (var e in enemies)
                {
                    if (e.hasID)
                    {
                        yield return new ModifyDefenseValueModifierAction(e.iD.value, 1);
                        yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
                        yield return new TriggerAnimationAction(e.iD.value, new AnimationTrigger("upgrade"));
                        yield return new DelayAction(10);
                    }
                }
            }
        }
    }

    public class HealthShrineBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero and Monsters [b]heal 3[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

                var cards = context._GetCardsWithCardArea(CardArea.Board)
                    .Where(c => c.hasBoardLocation && c.isCardFlipped && c.hasHealth && (c.isEnemy || c.isHero))
                    .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location))
                    .ToList();
                foreach (var e in cards)
                {
                    if (e.hasID)
                    {
                        yield return new HealTargetAction(e.iD.value, 3, true);
                        yield return new DelayAction(10);
                    }
                }
            }
        }
    }

    public class SlumberShrineBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Interact[/b]: apply [b]2 Sleep[/b] to Hero and Monsters";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickInteract;
            yield return KeywordID.Sleep;
        }

        public override bool IsQuickActivate => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

                yield return new SetQuickTurnAction(true);

                var cards = context._GetCardsWithCardArea(CardArea.Board)
                    .Where(c => c.hasBoardLocation && c.isCardFlipped && (c.isEnemy || c.isHero))
                    .OrderBy(c => BoardUtils.GetSortIndex(c.boardLocation.location))
                    .ToList();
                foreach (var e in cards)
                {
                    if (e.hasID)
                    {
                        yield return new TrySleepAction(e.iD.value, 2);
                        yield return new DelayAction(10);
                    }
                }
            }
        }
    }

    public class RevelationShrineBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Quick Interact[/b]: [b]Flip[/b] up each card"; 
        
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.QuickInteract;
        }

        public override bool IsQuickActivate => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                yield return new SetQuickTurnAction(true);

                var lastBoardLocation = card.boardLocation.location;
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

                var targets = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && !c.isCardFlipped && !c.isHero);
                var t = targets
                    .OrderBy(t => BoardUtils.GetManhattenDistance(t.boardLocation.location, lastBoardLocation)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                    .Select(target => new FlipCardAction(target.iD.value, true));
                yield return ActionService.CreateStaggeredParallelAction(t, 10);
            }
        }
    }


    public class ResourceShrineBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Flip coin: draw card from either Monster or Hero's Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
        }

        public override bool IsFinal => true;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            UndoService.ClearUndoState(Utils.GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            if (card.hasBoardLocation)
            {
                var lastBoardLocation = card.boardLocation.location;
                var seed = context.GetRandomSeedForIngameRoom(card.iD.value.ID);

                yield return new DelayAction(40);

                var random = new MyRandom(seed);
                var drawFromHero = random.Chance(0.5f);

                if (drawFromHero)
                {
                    yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                    yield return new DrawCardAction(lastBoardLocation, CardArea.HeroDrawPile);
                }
                else
                {
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("unlucky"));
                    yield return new TriggerSoundAction("fizzled"); // TODO: better sound
                    yield return new DelayAction(20);
                    yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                    yield return new DrawCardAction(lastBoardLocation, CardArea.EnemyDrawPile);
                }
            }
        }
    }


    public class LightningRodCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Conducts [b]Lightning[/b], [b]Lightning[/b] damage +1";

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class FountainOfHealthBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("heal", 9); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HealTargetAction(hero.iD.value, 9, true);
            yield return new DelayAction(30);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class FountainOfVitalityBehavior : BaseShrineBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Raise Max HP by {raiseHP}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("raiseHP", Raise); }

        private int Raise => 3;

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new IncreaseMaxHealthTargetAction(hero.iD.value, Raise);
            yield return new DelayAction(30);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }


    public class LingeringFlameCardBehavior : BasicContraptionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to 4-neighbor Monsters, [b]Timer[/b]: [b]destroy[/b]";
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", Damage(card, context)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.FireDamage;
            yield return KeywordID.Timer;
            yield return KeywordID.FireDamage;
        }

        private int Damage(IGameOrSimEntity card, IGameOrSimContext context) => 3;

        private int ChannelInit => 2;

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped && card.hasBoardLocation)
            {
                var boardLocation = card.boardLocation.location;
                // damage
                var damage = Damage(card, context);
                yield return new ActivateCardAction(card.iD.value);

                yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Where(l => BoardUtils.Are4Neighbors(l, boardLocation))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.hasCard && e.isEnemy,
                        e => new HurtTargetAction(e.iD.value, damage, HurtSource.Contraption, HurtType.Fire),
                        e => new SequenceAction(
                            new TriggerSoundAction("fire"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                            new DelayAction(5)
                        )
                    )));

                if (card.hasID)
                {
                    var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
                    if (currentChannelling > 1)
                    {
                        yield return new ModifyChannelingAction(card.iD.value, -1, false);
                        yield return new DelayAction(15);

                    }
                    else
                    {
                        yield return new ModifyChannelingAction(card.iD.value, -1, false);
                        yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                    }
                }

                if (card.hasID)
                    yield return new DeactivateCardAction(card.iD.value);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddChannelling(ChannelInit);
        }

        protected override IEnumerable<IActionData> OnActivate(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }
}
