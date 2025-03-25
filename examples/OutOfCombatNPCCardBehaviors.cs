using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Action;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnlockScene;

namespace CardStuff.Card
{
    public abstract class BasicOutOfCombatNPCCardBehavior : ICardBehavior, IFinalCardBehavior
    {
        public abstract string GenerateBaseDescriptionEN(int quality, bool isEthereal);
        public virtual IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield break; }
        public virtual IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) => null;
        public virtual IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public virtual IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public virtual CardType CardType => CardType.NPC;
        public InteractionType InteractionType => InteractionType.Talk;
        public CardGlowType CardGlowType => CardGlowType.Interact;
        public bool ImmuneToBeingRemovedFromBoard => true;

        public virtual bool IsFinal => false;

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => false;
        public virtual BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => (card.hasReachableVia) ? card.reachableVia.smoothed4NeighborPath : null;
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => false;
        public virtual IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CombatService.MoveHeroNextToActions(hero, card);
            var intermediate = context._CreateEntity(); // create intermediate entity

            foreach (var a in OnTalk(intermediate, hero, card, context))
                yield return a;

            intermediate.isDestroyed = true; // destroy intermediate entity
        }

        protected abstract IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context);

        protected IEnumerable<IActionData> PayAgreedUponMoney(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var spentMoney = (intermediate.hasSpentMoney) ? intermediate.spentMoney.value : 0;
            if (spentMoney > 0)
            {
                yield return new TriggerSoundAction("treasureCollect");
                yield return new LoseGoldAction(spentMoney, card.boardLocation.location);
                //var finalGold = gc.collectedGold.value - spentMoney;
                //yield return new AnimateSpendEmeraldsAction(
                //    spentMoney,
                //    ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location),
                //    (int i) => gc.ReplaceCollectedGold(gc.collectedGold.value - 1));
                //gc.ReplaceCollectedGold(finalGold);
            }
        }

        public virtual IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public virtual IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public virtual IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public virtual IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            yield return new DestroyCardAction(card.iD.value, reason == RemoveFromBoardReason.EndOfRound ? DestroyCardReason.EndOfRoom : DestroyCardReason.Other, runDestroyTriggers: false);
        }
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public virtual void OnCreate(IGameOrSimEntity e, IGameOrSimContext context) { }

        // TODO: make into proper action
        protected static IEnumerable<IActionData> LeaveActions(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var targetLocation = BoardLocation.Create(4, 2);
            var path = PathService.CalculatePath(context, card.boardLocation.location, targetLocation, true, true, false, Pathfinding.PathPreference.PreferVisuallyShortest, context.HeroEntity.boardLocation.location);
            if (path != null)
            {
                var finalPath = path.Skip(1).Select(tpn => tpn.location).ToArray();
                // follow path
                var a = finalPath.Select(p => new MoveCardOnBoardAction(card.iD.value, p, MoveReason.Walk));
                yield return new SequenceAction(a);
            }
        }

        public IEnumerable<IActionData> OnAfterPushedOffBoard(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public class CardsmithNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Upgrade[/b] Cards...";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var a in UpgradeCards(intermediate, card, context))
                yield return a;

            yield return new DelayAction(30);
            if (intermediate.affectedCards.cardIDs.Length <= 0)
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
            else
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "A worthy exchange!" }, { "ttl", 2.0f } }));
            yield return new DelayAction(130);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        private IEnumerable<IActionData> UpgradeCards(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardID);
                yield return new PermanentlyAddCardToDeckAction(new CardDataID(cardID.BaseID, cardID.QualityModifier + 1));

                var upgradeCard = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                if (upgradeCard != null)
                {
                    yield return new MoveCardToBoardAction(upgradeCard.iD.value, card.boardLocation.location, true, keepCardHovering: true);
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    yield return new DelayAction(30);
                    yield return new TriggerSoundAction("heroUpgrade");
                    yield return new TriggerAnimationAction(upgradeCard.iD.value, new AnimationTrigger("upgrade"));
                    upgradeCard.ReplaceCard(new CardDataID(cardID.BaseID, cardID.QualityModifier + 1));
                    yield return new DelayAction(30);
                    yield return new MoveCardToDrawPileAction(upgradeCard.iD.value, false, 20);

                    // TODO: need to remove from original as well?
                    //e = context._GetCardsWithCardArea(CardArea.OriginalDeck).FirstOrDefault(e => e.card.data == cardID);
                    //e?.ReplaceCard(new CardDataID(cardID.BaseID, cardID.QualityModifier + 1));
                }
            }
        }
    }

    public class PyromaniacNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Remove[/b] Cards...";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var a in BurnCards(intermediate, card, context))
                yield return a;

            yield return new DelayAction(30);
            if (intermediate.affectedCards.cardIDs.Length <= 0)
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
            else
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Keep burning!" }, { "ttl", 2.0f } }));
            yield return new DelayAction(130);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        private IEnumerable<IActionData> BurnCards(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardID);
                var e = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                if (e != null)
                {
                    yield return new MoveCardToBoardAction(e.iD.value, card.boardLocation.location, true, keepCardHovering: true);
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    yield return new DelayAction(30);
                    yield return new TriggerSoundAction("fire");
                    yield return new TriggerAnimationAction(context._GetBoardLocationEntity(card.boardLocation.location).iD.value, new AnimationTrigger("burn"));
                    yield return new DestroyCardAction(e.iD.value, DestroyCardReason.Other, runDestroyTriggers: false);
                }
            }
        }
    }


    public class TraderNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Trade[/b] your Cards for random others..."; 
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
        }

        public override bool IsFinal => true;

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            UndoService.ClearUndoState(Utils.GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var a in TradeCards(intermediate, card, context))
                yield return a;

            yield return new DelayAction(30);
            if (intermediate.affectedCards.cardIDs.Length <= 0)
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
            else
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "A fair trade!" }, { "ttl", 2.0f } }));
            yield return new DelayAction(130);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        private IEnumerable<IActionData> TradeCards(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (intermediate.affectedCards.cardIDs.Length <= 0)
                yield break;

            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardID);
                var e = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                if (e != null)
                {
                    yield return new MoveCardToBoardAction(e.iD.value, card.boardLocation.location, true, keepCardHovering: true);
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey
                    //yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    //yield return new DelayAction(20);
                    yield return new DestroyCardAction(e.iD.value, DestroyCardReason.Other, runDestroyTriggers: false);
                }
                // TODO: need to remove from original as well?
                //    e = context._GetCardsWithCardArea(CardArea.OriginalDeck).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                //    if (e != null)
                //        e.isDestroyed = true;
            }
            yield return new DelayAction(30);

            // gain random cards
            var m = MetaSaveSystem.LoadOrDefault();
            var gameState = GameStateService._GetDungeonRunOrDefault();
            var seed = context.FloorSeed;
            var deck = context.DungeonRunDeckEntity?.dungeonRunDeck?.cards ?? new CardDataID[0];
            var softBannedCards = context.BanishedCardsEntity?.banishedCards?.cards ?? new CardDataID[0];
            var hardBannedCards = intermediate.affectedCards.cardIDs.Concat(HeroXPUtils.CurrentlyLockedCardsForHero(gameState.HeroClass, m.GetHeroXPLevel(gameState.HeroClass))).Concat(m.GetVetoedCards(gameState.HeroClass)).Distinct().ToArray();
            var randomCards = LootDropUtils.GetRandomEquipmentCards(intermediate.affectedCards.cardIDs.Length, gameState.NonLoopingFloor, gameState.HeroClass, deck, gameState.TargetID, seed, DungeonRunBuilder.LootLevel(gameState.TargetID, gameState.NonLoopingFloor), hardBannedCards, softBannedCards);
            foreach (var cardID in randomCards)
            {
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true, 30);
                // TODO: need to add to original as well?
                //var cardInOriginalDeck = CardUtils.CreateCard(context, cardID);
                //cardInOriginalDeck.ReplaceCardArea(CardArea.OriginalDeck);
            }
        }
    }

    public class JewelerNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Remove a Trinket[/b], [b]add another[/b]...";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Final;
        }

        public override bool IsFinal => true;

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            UndoService.ClearUndoState(Utils.GameType.DungeonRun); // NOTE: not very clean, but ok-ish

            foreach (var a in RemoveTrinket(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            if (intermediate.affectedCards.cardIDs.Length > 0)
            {
                yield return new FindLootAction(true, 0, false);
                yield return new DelayAction(30);
                yield return new EquipAllTrinketsFromDeckAction();
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Excellent..." }, { "ttl", 2.0f } }));
            } else
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
            }
            yield return new DelayAction(130);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        private IEnumerable<IActionData> RemoveTrinket(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardID);

                // also banish the trinket, so it does not appear again, neither in FindLootAction() right after this, or later
                yield return new BanishCardAction(new CardDataID(cardID.BaseID, 0));

                var e = context._GetCardsWithCardArea(CardArea.Trinkets).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                if (e != null)
                {
                    yield return new MoveCardToBoardAction(e.iD.value, card.boardLocation.location, true, keepCardHovering: true);
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    yield return new DelayAction(30);
                    yield return new DestroyCardAction(e.iD.value, DestroyCardReason.Other, runDestroyTriggers: false);
                }
                // TODO: need to remove from original as well?
                //    e = context._GetCardsWithCardArea(CardArea.OriginalDeck).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                //    if (e != null)
                //        e.isDestroyed = true;
            }
        }
    }

    public class CollectorNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Remove upgraded[/b] Cards...";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in RemoveCards(intermediate, card, context))
                yield return a;

            yield return new DelayAction(30);
            if (intermediate.affectedCards.cardIDs.Length > 0)
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "For my collection..." }, { "ttl", 2.0f } }));
            else
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
            yield return new DelayAction(130);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        private IEnumerable<IActionData> RemoveCards(IGameOrSimEntity intermediate, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardID);
                var e = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                if (e != null)
                {
                    yield return new MoveCardToBoardAction(e.iD.value, card.boardLocation.location, true, keepCardHovering: true);
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    yield return new DelayAction(30);
                    yield return new DestroyCardAction(e.iD.value, DestroyCardReason.Other, runDestroyTriggers: false);
                }
                // TODO: need to remove from original as well?
                //    e = context._GetCardsWithCardArea(CardArea.OriginalDeck).FirstOrDefault(e => e.card.data == cardID && !e.isDestroyed);
                //    if (e != null)
                //        e.isDestroyed = true;
            }
        }
    }

    public class MerchantNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Buy[/b] Cards...";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            var gameState = GameStateService._GetDungeonRunOrDefault();
            var m = MetaSaveSystem.LoadOrDefault();
            var seed = context.FloorSeed;
            var deck = context.DungeonRunDeckEntity?.dungeonRunDeck?.cards ?? new CardDataID[0];
            var softBannedCards = context.BanishedCardsEntity?.banishedCards?.cards ?? new CardDataID[0];
            var hardBannedCards = HeroXPUtils.CurrentlyLockedCardsForHero(gameState.HeroClass, m.GetHeroXPLevel(gameState.HeroClass)).Concat(m.GetVetoedCards(gameState.HeroClass)).Distinct().ToArray();
            var buyableCards =
                LootDropUtils.GetRandomEquipmentCards(7, gameState.NonLoopingFloor, gameState.HeroClass, deck, gameState.TargetID, seed, DungeonRunBuilder.LootLevel(gameState.TargetID, gameState.NonLoopingFloor), hardBannedCards, softBannedCards)
                .Concat(LootDropUtils.GetRandomTrinketCards(1, gameState.NonLoopingFloor, gameState.HeroClass, deck, gameState.TargetID, seed, hardBannedCards, softBannedCards))
                .InNaturalOrder();

            intermediate.AddTalkToNPC(card.card.data);
            intermediate.AddAffectedCards(buyableCards.ToArray()); // we are using affected cards component to tell popup what cards are buyable

            // mark cards as seen
            m = m.EnsureCardLevel(buyableCards, gameState.HeroClass, 1);
            MetaSaveSystem.Save(m);

            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true, 30);
                yield return new DelayAction(20);
                yield return new EquipAllTrinketsFromDeckAction();

                // TODO: need to add to original as well?
                //var cardInOriginalDeck = CardUtils.CreateCard(context, cardID);
                //cardInOriginalDeck.ReplaceCardArea(CardArea.OriginalDeck);
            }

            if (intermediate.affectedCards.cardIDs.Length > 0)
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Goodbye!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(60);

                foreach (var a in LeaveActions(card, context))
                    yield return a;
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            }
            else
                yield break;
        }
    }


    public class PeddlerNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Buy[/b] Trinkets...?";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("kosmemophobia");
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            var gameState = GameStateService._GetDungeonRunOrDefault();
            var m = MetaSaveSystem.LoadOrDefault();
            var seed = context.FloorSeed;
            var deck = context.DungeonRunDeckEntity?.dungeonRunDeck?.cards ?? new CardDataID[0];
            var softBannedCards = context.BanishedCardsEntity?.banishedCards?.cards ?? new CardDataID[0];
            var hardBannedCards = HeroXPUtils.CurrentlyLockedCardsForHero(gameState.HeroClass, m.GetHeroXPLevel(gameState.HeroClass)).Concat(m.GetVetoedCards(gameState.HeroClass)).Distinct().ToArray();
            var buyableCards =
                LootDropUtils.GetRandomTrinketCards(4, gameState.NonLoopingFloor, gameState.HeroClass, deck, gameState.TargetID, seed, hardBannedCards, softBannedCards)
                .InNaturalOrder();

            intermediate.AddTalkToNPC(card.card.data);
            intermediate.AddAffectedCards(buyableCards.ToArray()); // we are using affected cards component to tell popup what cards are buyable

            // mark cards as seen
            m = m.EnsureCardLevel(buyableCards, gameState.HeroClass, 1);
            MetaSaveSystem.Save(m);

            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true, 30);
                yield return new DelayAction(20);
                yield return new EquipAllTrinketsFromDeckAction();
                yield return new DelayAction(20);

                yield return new PermanentlyAddCardToDeckAction(new CardDataID("kosmemophobia"));
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardInDrawPileStartingFromBoardAction(new CardDataID("kosmemophobia"), card.boardLocation.location, true, 30);
                yield return new DelayAction(20);
            }

            if (intermediate.affectedCards.cardIDs.Length > 0)
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Goodbye!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(60);

                foreach (var a in LeaveActions(card, context))
                    yield return a;
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            }
            else
                yield break;
        }
    }



    public class ApothecaryNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Buy[/b] Potions...";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("findLoot"); // TODO: better sound

            var gameState = GameStateService._GetDungeonRunOrDefault();
            var m = MetaSaveSystem.LoadOrDefault();
            var buyableCards = CardRepository.RegularPotions.Select(d => new CardDataID(d, 0)).InNaturalOrder();

            intermediate.AddTalkToNPC(card.card.data);
            intermediate.AddAffectedCards(buyableCards.ToArray()); // we are using affected cards component to tell popup what cards are buyable

            // mark cards as seen
            m = m.EnsureCardLevel(buyableCards, gameState.HeroClass, 1);
            MetaSaveSystem.Save(m);

            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            foreach (var a in PayAgreedUponMoney(intermediate, card, context))
                yield return a;
            yield return new DelayAction(30);

            foreach (var cardID in intermediate.affectedCards.cardIDs)
            {
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true, 30);
                yield return new DelayAction(20);
                yield return new EquipAllTrinketsFromDeckAction();

                // TODO: need to add to original as well?
                //var cardInOriginalDeck = CardUtils.CreateCard(context, cardID);
                //cardInOriginalDeck.ReplaceCardArea(CardArea.OriginalDeck);
            }

            if (intermediate.affectedCards.cardIDs.Length > 0)
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Goodbye!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(60);

                foreach (var a in LeaveActions(card, context))
                    yield return a;
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
            }
            else
                yield break;
        }
    }

    public class QuartermasterNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class ExchequerNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class KingNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }
    }

    public class HealerNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        private int Heal => 3;
        private int Gold(IGameOrSimContext context) => LoopingUtils.GetNPCVendorPriceModifier(DifficultyUtils.GetNPCVendorPriceModifier(15, context), context);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Heal {heal}[/b] for [b]{gold} Emeralds[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("heal", Heal);  }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("gold", Gold(context)); }
        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("treasureCollect");
            yield return new LoseGoldAction(Gold(context), card.boardLocation.location);

            // heal
            yield return new HealTargetAction(hero.iD.value, Heal, true);
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Very well!" }, { "ttl", 1.0f } }));
        }

        public override BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            var gold = gameOrSimContext.CollectedGoldEntity.collectedGold.value;

            var canBeHealed = gameOrSimContext.HeroEntity.health.value < gameOrSimContext.HeroEntity.health.max;

            return (canBeHealed && gold >= Gold(gameOrSimContext) && card.hasReachableVia) ? card.reachableVia.smoothedOnTargetPath : null;
        }
    }

    // not used currently, replaced by constitution potion
    public class PriestNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Raise Max HP by {raiseHP}[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("raiseHP", 4); }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new IncreaseMaxHealthTargetAction(hero.iD.value, 4);
            yield return new DelayAction(90);
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Stay healthy!" }, { "ttl", 2.0f } }));
            yield return new DelayAction(60);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public abstract class CapturedHeroNPCCardBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override CardType CardType => CardType.CapturedHero;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CallForHelp(card, 6f);
        }

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CallForHelp(card, 3f);
        }

        protected abstract string HeroClass { get; }

        private IActionData CallForHelp(IGameOrSimEntity card, float ttl)
        {
            var m = MetaSaveSystem.LoadOrDefault();
            if (!MetaProgressionService.IsHeroUnlocked(m, HeroClass))
                return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Help me!" }, { "ttl", ttl } }));
            else
                return NoopAction.Instance;
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var mOld = MetaSaveSystem.LoadOrDefault();
            var mNew = mOld.AddUnlockedHero(HeroClass);
            MetaSaveSystem.Save(mNew);

            var unlockedHeroes = mNew.UnlockedHeroes.Except(mOld.UnlockedHeroes);
            //TmpMapSceneData.NewUnlockHeroes = TmpMapSceneData.NewUnlockHeroes.Union(unlockedHeroes).Distinct().ToArray();
            TmpUnlockSceneData.HeroUnlocks = TmpUnlockSceneData.HeroUnlocks.Union(unlockedHeroes).Distinct().ToArray();

            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Thank you!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(150);

            var targetBoardLocation = context._GetBoardLocationEntity(BoardLocation.Create(4, 2));
            // TODO: this is not correct and leads to npcs using weird paths
            var fullPath = targetBoardLocation.reachableVia.unsmoothedOnTargetPath.Prepend(context.HeroEntity.boardLocation.location);

            // follow path
            var a = fullPath.Select(p => new MoveCardOnBoardAction(card.iD.value, p, MoveReason.Walk));
            yield return new SequenceAction(a);

            yield return new DelayAction(60);
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "We'll meet again!" }, { "ttl", 3.0f } }));

            yield return new DelayAction(200);

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

            yield return new DelayAction(30);
            yield return new TriggerAnimationAction(context.HeroEntity.iD.value, new AnimationTrigger("heroUnlocked"));
            yield return new TriggerSoundAction("roomCleared");
            yield return new DelayAction(30);
        }
    }

    public class CapturedRogueBehavior : CapturedHeroNPCCardBehavior
    {
        protected override string HeroClass => "rogue";
    }
    public class CapturedMageBehavior : CapturedHeroNPCCardBehavior
    {
        protected override string HeroClass => "mage";
    }


    public interface IQuestGiverNPCBehavior : ICardBehavior
    {
        public string GetIntroTextEN();
    }

    public class PilgrimBehavior : BasicOutOfCombatNPCCardBehavior, IQuestGiverNPCBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public string GetIntroTextEN() => "I wanted to be worthy... but I cannot go on! Will you complete the ceremony on my behalf?";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Help me!" }, { "ttl", 3f } }));
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questDetails = new ThePilgrimQuestTemplate.Details(QuestState.Open, context.FloorEntity.floor.floor);
            intermediate.AddTalkToQuestGiver(questDetails);
            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            if (intermediate.isAcceptQuest)
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Thank you!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);

                var cardID = new CardDataID("ceremonialStaff");
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true);

                yield return new DelayAction(30);
                yield return new StartQuestAction(questDetails, card.boardLocation.location);
            }
            else
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", ":(" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
                var cardID = new CardDataID("shame", 2);
                yield return new TriggerSoundAction("debuff");
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, hero.boardLocation.location, true, 60);
                yield return new DelayAction(30);
            }

            yield return new DelayAction(30);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class ThiefBehavior : BasicOutOfCombatNPCCardBehavior, IQuestGiverNPCBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public string GetIntroTextEN() => "If I had known this Monster was protecting it, I would have never stolen it! I'll let you have it!";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Hey, you!" }, { "ttl", 3f } }));
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questDetails = new TheThiefQuestTemplate.Details(QuestState.Open, context.FloorEntity.floor.floor);
            intermediate.AddTalkToQuestGiver(questDetails);
            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            if (intermediate.isAcceptQuest)
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Good luck!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);

                var cardID = new CardDataID("stolenLocket");
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, card.boardLocation.location, true);

                yield return new DelayAction(30);
                yield return new StartQuestAction(questDetails, card.boardLocation.location);
            }
            else
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
                var cardID = new CardDataID("shame", 2);
                yield return new TriggerSoundAction("debuff");
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, hero.boardLocation.location, true, 60);
                yield return new DelayAction(30);
            }

            yield return new DelayAction(30);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class StatueBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questCard = context._GetCardsWithCardArea(CardArea.Quests).FirstOrDefault(c => c.card.data.BaseID == ThePilgrimQuestTemplate.QuestIDStatic);
            if (questCard != null && questCard.hasQuestDetails && questCard.questDetails.details.State == QuestState.Open)
            {
                var ceremonialStaffInDeck = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(c => c.hasCard && c.card.data.BaseID == "ceremonialStaff");
                if (ceremonialStaffInDeck != null)
                {
                    // present staff
                    var firstFreeLocation = BoardUtils.Get4Neighbors(card.boardLocation.location).First(n => !context._GetCardsWithBoardLocation(n).Any());
                    yield return new MoveCardToBoardAction(ceremonialStaffInDeck.iD.value, firstFreeLocation, true);
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey

                    yield return new DelayAction(30);
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "WORTHY." }, { "ttl", 3.0f } }));
                    yield return new DelayAction(180);

                    yield return new DestroyCardAction(ceremonialStaffInDeck.iD.value, DestroyCardReason.Other);
                    yield return new PermanentlyRemoveCardFromDeckAction(ceremonialStaffInDeck.card.data);
                    yield return new DelayAction(30);

                    // rewards
                    foreach (var n in new BoardLocation[] { BoardLocation.Create(1, 1), BoardLocation.Create(1, 3) })
                    {
                        yield return new TriggerSoundAction("cardCreateOnBoard");
                        yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald50"), n);
                        yield return new DelayAction(10);
                    }

                    yield return new TryCloseQuestAction(ThePilgrimQuestTemplate.QuestIDStatic, true);

                }
                else
                {
                    yield return new DelayAction(30);
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "NOT. WORTHY." }, { "ttl", 3.0f } }));
                    yield return new DelayAction(80);

                    yield return new TryCloseQuestAction(ThePilgrimQuestTemplate.QuestIDStatic, false);
                }
            }
            else
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 2.0f } }));
                yield return new DelayAction(30);
            }
        }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // fail quest, if not closed yet
            yield return new TryCloseQuestAction(ThePilgrimQuestTemplate.QuestIDStatic, false);
        }
    }

    public class GirlBehavior : BasicOutOfCombatNPCCardBehavior, IQuestGiverNPCBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public string GetIntroTextEN() => "Please help me! My brother was kidnapped and they brought him here. Can you rescue him?";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Help me!" }, { "ttl", 3f } }));
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questDetails = new TheBrotherQuestTemplate.Details(QuestState.Open, context.FloorEntity.floor.floor);
            intermediate.AddTalkToQuestGiver(questDetails);
            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            if (intermediate.isAcceptQuest)
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Thank you!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
                yield return new StartQuestAction(questDetails, card.boardLocation.location);
            }
            else
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", ":(" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
                var cardID = new CardDataID("shame", 2);
                yield return new TriggerSoundAction("debuff");
                yield return new PermanentlyAddCardToDeckAction(cardID);
                yield return new CreateCardInDrawPileStartingFromBoardAction(cardID, hero.boardLocation.location, true, 60);
                yield return new DelayAction(30);
            }

            yield return new DelayAction(30);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class GoblinQuestGiverBehavior : BasicOutOfCombatNPCCardBehavior, IQuestGiverNPCBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public string GetIntroTextEN() => "Bet you can't...";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Bet?" }, { "ttl", 3f } }));
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questDetails = card.questDetails.details;
            intermediate.AddTalkToQuestGiver(questDetails);
            intermediate.AddTalkToNPC(card.card.data);
            yield return new DelayUntilAction(() => !intermediate.hasTalkToNPC);

            if (intermediate.isAcceptQuest)
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Bet!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
                yield return new StartQuestAction(questDetails, card.boardLocation.location);
            }
            else
            {
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Boring!" }, { "ttl", 2.0f } }));
                yield return new DelayAction(80);
            }

            yield return new DelayAction(30);

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }
    }

    public class GoblinQuestCheckerBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        private (IQuestDetails details, IGameOrSimEntity card) GetQuestDetailsAndCard(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questID = card.questID.questID;
            var questCard = context._GetCardsWithCardArea(CardArea.Quests).FirstOrDefault(c => c.card.data.BaseID == questID);
            if (questCard != null && questCard.hasQuestDetails)
                return (questCard.questDetails.details, questCard);
            return (null, null);
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var (questDetails, questCard) = GetQuestDetailsAndCard(card, context);

            if (questDetails != null && questCard != null && CardRepository.GenerateCardBehaviorFromData(questCard.card.data) is GoblinQuestCardBehavior gqb)
            {
                // force quest completion, one way or another
                // NOTE: some quests complete before coming here, others need to be "forcefully" brought to a conclusion
                var finalState = gqb.ForceQuestStateCompletion(questDetails, context);

                if (finalState == QuestState.Completed)
                {
                    yield return new DelayAction(30);
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "You Win!" }, { "ttl", 3.0f } }));
                    yield return new DelayAction(30);

                    foreach (var a in gqb.OnSuccessfulCheck(card, questCard, hero, context))
                        yield return a;

                    yield return new DelayAction(60);

                    yield return new TryCloseQuestAction(questDetails.QuestID, true);
                }
                else
                {
                    yield return new DelayAction(30);
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "You Lose!" }, { "ttl", 3.0f } }));
                    yield return new DelayAction(30);

                    foreach (var a in gqb.OnFailedCheck(card, hero, context))
                        yield return a;

                    yield return new DelayAction(60);

                    yield return new TryCloseQuestAction(questDetails.QuestID, false);
                }
            }

            foreach (var a in LeaveActions(card, context))
                yield return a;
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
        }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // fail quest, if not closed yet
            var (questDetails, _) = GetQuestDetailsAndCard(card, context);
            if (questDetails != null && questDetails.State == QuestState.Open)
                yield return new TryCloseQuestAction(questDetails.QuestID, false);
        }
    }


    public class SisterBehavior : BasicOutOfCombatNPCCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var brotherInDeck = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(c => c.hasCard && c.card.data.BaseID == "brother");
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Brother!" }, { "ttl", 3f } }));
        }

        protected override IEnumerable<IActionData> OnTalk(IGameOrSimEntity intermediate, IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var questCard = context._GetCardsWithCardArea(CardArea.Quests).FirstOrDefault(c => c.card.data.BaseID == TheBrotherQuestTemplate.QuestIDStatic);
            if (questCard != null && questCard.hasQuestDetails && questCard.questDetails.details.State == QuestState.Open)
            {
                var brotherInDeck = context._GetCardsWithCardArea(CardArea.HeroDrawPile).FirstOrDefault(c => c.hasCard && c.card.data.BaseID == "brother");
                if (brotherInDeck != null)
                {
                    // present brother
                    var firstFreeLocation = BoardUtils.Get4Neighbors(card.boardLocation.location).First(n => !context._GetCardsWithBoardLocation(n).Any());
                    yield return new MoveCardToBoardAction(brotherInDeck.iD.value, firstFreeLocation, true);
                    yield return new TriggerSoundAction("cardCreateOnBoard"); // TODO
                    PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey

                    yield return new DelayAction(30);
                    yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Thank you!" }, { "ttl", 3.0f } }));
                    yield return new DelayAction(180);

                    yield return new PermanentlyRemoveCardFromDeckAction(brotherInDeck.card.data);
                    yield return new DelayAction(30);

                    // rewards
                    foreach (var n in new BoardLocation[] { BoardLocation.Create(1, 1), BoardLocation.Create(1, 3) })
                    {
                        yield return new TriggerSoundAction("cardCreateOnBoard");
                        yield return new CreateCardAtBoardLocationAction(new CardDataID("unidentifiedLoot"), n);
                        yield return new DelayAction(10);
                    }

                    foreach (var a in LeaveActions(card, context))
                        yield return a;
                    yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                    foreach (var a in LeaveActions(brotherInDeck, context))
                        yield return a;
                    yield return new DestroyCardAction(brotherInDeck.iD.value, DestroyCardReason.Other);

                    yield return new TryCloseQuestAction(TheBrotherQuestTemplate.QuestIDStatic, true);
                }
                else
                {
                    // shouldn't happen
                    yield return new DelayAction(30);
                    yield return new TryCloseQuestAction(TheBrotherQuestTemplate.QuestIDStatic, false);
                }
            }
            else
            {
                yield return new DelayAction(30);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "..." }, { "ttl", 3.0f } }));
                yield return new DelayAction(30);
            }
        }

        public override IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // fail quest, if not closed yet
            yield return new TryCloseQuestAction(TheBrotherQuestTemplate.QuestIDStatic, false);
        }
    }
}
