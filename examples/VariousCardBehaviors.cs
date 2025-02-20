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
using System;
using System.Collections.Generic;
using System.Linq;
using static UnlockScene;

namespace Core.Card
{
    public class HeroCardBehavior : ICardBehavior
    {
        private (IGameOrSimEntity currentSpell, BasicSpellCardBehavior spellBehavior, bool heroIsValidTarget) CheckForDroppableSpellInteraction(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var currentSpell = CombatService.GetTopHeroSpell(context);
            if (card.hasBoardLocation && currentSpell != null && currentSpell.hasCard && CardRepository.GenerateCardBehaviorFromData(currentSpell.card.data) is BasicSpellCardBehavior sb)
            {
                var validTarget = sb.IsValidTarget(card, currentSpell, card, card.boardLocation.location, context);
                return (currentSpell, sb, validTarget);
            }
            return default;
        }

        private IGameOrSimEntity CheckForKnightHeirloomInteraction(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!context.FilterBuffs<KnightsHeirloomBuff>().Any())
                return null;

            var topWeapon = CombatService.GetTopHeroWeapon(context, true);
            if (topWeapon == null)
                return null;
            return topWeapon;
        }
        private IList<IGameOrSimEntity> CheckForRogueHeirloomInteraction(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!context.FilterBuffs<RoguesHeirloomBuff>().Any())
                return null;

            var top2Armor = CombatService.GetTopNHeroArmor(context, 2).ToList();
            if (top2Armor.Count < 2)
                return null;
            return top2Armor;
        }
        private IList<IGameOrSimEntity> CheckForMageHeirloomInteraction(IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!context.FilterBuffs<MagesHeirloomBuff>().Any())
                return null;

            var top2Armor = CombatService.GetTopNHeroArmor(context, 2).ToList();
            if (top2Armor.Count < 2)
                return null;
            return top2Armor;
        }

        public string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield break; }
        public IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) 
        {
            var description = "";
            var spellInteractionInfo = CheckForDroppableSpellInteraction(card, context);
            var knightHeirloomInfo = CheckForKnightHeirloomInteraction(card, context);
            var rogueHeirloomInfo = CheckForRogueHeirloomInteraction(card, context);
            var mageHeirloomInfo = CheckForMageHeirloomInteraction(card, context);

            if (spellInteractionInfo != default)
            {
                var spellName = CardNames.Generate(spellInteractionInfo.currentSpell.card.data);
                if (spellInteractionInfo.heroIsValidTarget)
                {
                    description = $"[b]Cast {spellName}[/b]"; // TODO: translate
                } else
                {
                    description = $"[b]Drop {spellName}[/b]"; // TODO: translate
                }
            }
            else if (knightHeirloomInfo != null)
            {
                description = "[b]Drop[/b] top Weapon, [b]equip[/b] Armor from Deck"; // TODO: translate
            }
            else if (rogueHeirloomInfo != null)
            {
                description = "[b]Drop[/b] top 2 Armor, [b]equip[/b] Weapon from Deck"; // TODO: translate
            }
            else if (mageHeirloomInfo != null)
            {
                description = "[b]Drop[/b] top 2 Arcana, [b]prepare[/b] Spell from Deck"; // TODO: translate
            }
            else if (context.FilterBuffs<WaitingAllowedBuff>().Any())
                description = "Skip Turn"; // TODO: translate
            yield return ("", description); // special "" key, replace whole text
        }
        public IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; } // TODO: keywords depending on interaction: cast, drop, (skip)
        public IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public CardType CardType => CardType.Hero;
        public InteractionType InteractionType => InteractionType.None;
        public CardGlowType CardGlowType => CardGlowType.MoveOver;
        public bool ImmuneToBeingRemovedFromBoard => true;

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => false;
        public BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext context)
        {
            var spellInteractionInfo = CheckForDroppableSpellInteraction(card, context);
            if (spellInteractionInfo != default && !spellInteractionInfo.heroIsValidTarget) // there is a spell equipped, but hero is not a valid target -> cancel spell on interaction
                return Array.Empty<BoardLocation>();

            if (CheckForKnightHeirloomInteraction(card, context) != null)
                return Array.Empty<BoardLocation>();
            if (CheckForRogueHeirloomInteraction(card, context) != null)
                return Array.Empty<BoardLocation>();
            if (CheckForMageHeirloomInteraction(card, context) != null)
                return Array.Empty<BoardLocation>();

            if (context.FilterBuffs<WaitingAllowedBuff>().Any())
                return Array.Empty<BoardLocation>();

            return null;
        }
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => false;
        public IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var spellInteractionInfo = CheckForDroppableSpellInteraction(card, context);
            if (spellInteractionInfo != default && !spellInteractionInfo.heroIsValidTarget) // there is a spell equipped, but hero is not a valid target -> cancel spell on interaction
            {
                yield return new RequestDropCardAction(spellInteractionInfo.currentSpell.iD.value, RequestDropReason.ForceDrop);
                yield break;
            }

            var knightHeirloomInfo = CheckForKnightHeirloomInteraction(card, context);
            if (knightHeirloomInfo != null)
            {
                yield return new RequestDropCardAction(knightHeirloomInfo.iD.value, RequestDropReason.ForceDrop);
                var armorFromDeck = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, (card) => CardUtils.GetCardType(card) == CardType.Armor);
                if (armorFromDeck != null)
                {
                    yield return new RequestEquipCardAction(armorFromDeck.iD.value);
                } else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                yield break;
            }

            var rogueHeirloomInfo = CheckForRogueHeirloomInteraction(card, context);
            if (rogueHeirloomInfo != null)
            {
                foreach(var armor in rogueHeirloomInfo)
                    yield return new RequestDropCardAction(armor.iD.value, RequestDropReason.ForceDrop);
                var weaponFromDeck = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, (card) => CardUtils.GetCardType(card) == CardType.Weapon);
                if (weaponFromDeck != null)
                {
                    yield return new RequestEquipCardAction(weaponFromDeck.iD.value);
                }
                else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                yield break;
            }

            var mageHeirloomInfo = CheckForMageHeirloomInteraction(card, context);
            if (mageHeirloomInfo != null)
            {
                foreach (var armor in mageHeirloomInfo)
                    yield return new RequestDropCardAction(armor.iD.value, RequestDropReason.ForceDrop);
                var spellFromDeck = CombatService.GetTopCardInPile(context, CardArea.HeroDrawPile, (card) => CardUtils.GetCardType(card) == CardType.Spell);
                if (spellFromDeck != null)
                {
                    yield return new RequestEquipCardAction(spellFromDeck.iD.value);
                }
                else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                yield break;
            }

            if (context.FilterBuffs<WaitingAllowedBuff>().Any())
            {
                yield return new TriggerAnimationAction(hero.iD.value, new AnimationTrigger("wait"));
                yield return new DelayAction(40);
            }
        }
        public IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (context.FilterBuffs<TriboElectricityBuff>().Any())
                return _TriboElectricity(card, context);
            else
                return null;
        }

        private IEnumerable<IActionData> _TriboElectricity(IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var b in context.FilterBuffs<TriboElectricityBuff>())
            {
                var numLoot = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && CardUtils.GetCardType(c) == CardType.Loot && c.isCardFlipped).Count();
                if (numLoot > 0)
                {
                    foreach (var a in CombatService.TriggerLightning(card, numLoot, context, HurtSource.Loot, card))
                        yield return a;
                }
            }
        }

        public IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason) { yield break; }
        public IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.isHero = true;
            e.isCardFlipped = true;
            e.AddHeroRoomState(HeroRoomState.InRoom);
            e.AddHealth(1, 1);
        }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public abstract class ExitFloorCardBehavior : ICardBehavior
    {
        public virtual string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield break; }
        public IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) => null;
        public IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public abstract CardType CardType { get; }
        public InteractionType InteractionType => InteractionType.Move;
        public CardGlowType CardGlowType => CardGlowType.MoveOver;
        public bool ImmuneToBeingRemovedFromBoard => true;

        public IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.isCardFlipped)
                yield return new FlipCardAction(card.iD.value, true);
        }

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => false;
        public BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            return (card.hasReachableVia) ? card.reachableVia.smoothedOnTargetPath : null;
        }
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => false;
        public abstract IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context);

        public IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public virtual IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            if (reason == RemoveFromBoardReason.EndOfRound)
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.EndOfRoom, runDestroyTriggers: false);
        }
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }

        public IEnumerable<IActionData> OnAfterPushedOffBoard(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public class StairsCardBehavior : ExitFloorCardBehavior
    {
        public override CardType CardType => CardType.Exit;

        public override IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CombatService.MoveHeroOntoActions(context, card);

            yield return new TriggerSoundAction("doorOpen"); // TODO?
            yield return new ModifyHeroRoomStateAction(hero.iD.value, HeroRoomState.ExitFloor);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }

    public class UnlockedDoorCardBehavior : ExitFloorCardBehavior
    {
        public override CardType CardType => CardType.Door;

        public override IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CombatService.MoveHeroOntoActions(context, card);

            yield return new TriggerSoundAction("doorOpen"); // TODO?
            yield return new ModifyHeroRoomStateAction(hero.iD.value, HeroRoomState.ExitFloor);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }

    public class ClimbingRopeCardBehavior : ExitFloorCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Skip Floor I[/b]";
        public override CardType CardType => CardType.Exit;

        public override IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return CombatService.MoveHeroOntoActions(context, card);

            // delete trigger in meta
            if (GameStateService.Get() is DungeonRunState drs)
            {
                // make sure player does not undo this
                UndoService.ClearUndoState(GameType.DungeonRun); // NOTE: not very clean, but ok-ish

                var mOld = MetaSaveSystem.LoadOrDefault();
                var mNew = mOld.RemoveMiscTrigger(TheRopeQuestTemplate.BuildTriggerName(drs.TargetID, drs.Difficulty, drs.HeroClass));
                MetaSaveSystem.Save(mNew);
            }

            yield return new TriggerSoundAction("doorOpen"); // TODO: better sound
            yield return new ModifyHeroRoomStateAction(hero.iD.value, HeroRoomState.ExitFloor);

            // HACK: skip main floor (mezzanineNumber 0)
            (hero as GameEntity).AddForcedNextMezzanineNumber(1);

            // HACK: we use a quest as a token to pick correct mezzanines, immediately finish the quest itself though
            yield return new StartQuestAction(new TheRopeQuestTemplate.Details(QuestState.Open), card.boardLocation.location, true);
            yield return new TryCloseQuestAction(TheRopeQuestTemplate.QuestIDStatic, true, true);

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }

    public class LockedDoorCardBehavior : ICardBehavior
    {
        private readonly bool winged;

        public LockedDoorCardBehavior(bool winged)
        {
            this.winged = winged;
        }

        public string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]{keyName}[/b]\nrequired";
        public IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("keyName", TranslationServer.Translate((winged) ? "Winged Key" : "Key")); }
        public IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) => null;
        public IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public CardType CardType => CardType.LockedDoor;
        public InteractionType InteractionType => InteractionType.Activate;
        public CardGlowType CardGlowType => CardGlowType.Interact;
        public bool ImmuneToBeingRemovedFromBoard => true;

        public IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => false;
        public BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            return (gameOrSimContext.IsFloorWon && card.hasReachableVia) ? card.reachableVia.smoothed4NeighborPath : null;
        }
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => false;
        public IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var lockedDoorLocation = card.boardLocation.location;
            yield return CombatService.MoveHeroNextToActions(hero, card);
            context.SpentWingedKey = true;
            var gameState = GameStateService.Get();
            var pt = gameState switch
            {
                PuzzleGameState _ => ProjectileType.RoomKey,
                DungeonRunState drs => DungeonRunBuilder.IsBossFloor(drs) ? ProjectileType.BossKey : ProjectileType.WingedKey,
                _ => ProjectileType.WingedKey
            };
            yield return new AnimateSpendWingedKeyAction(ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location), pt);
            yield return new TriggerSoundAction("unlock");

            var doorID = "stairs";
            if (gameState is PuzzleGameState pgs && pgs.Index < PuzzleBuilder.NumPuzzles(pgs.TargetID) - 1)
                doorID = "unlockedDoor";
            IGameOrSimEntity createdCard = null;
            yield return new CreateCardAtBoardLocationAction(new CardDataID(doorID), lockedDoorLocation, true, true, (card) => { createdCard = card; return NoopAction.Instance; });
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
            PathService.RecalculateReachableVia(context); // HACK: to make newly appearing card not grey and allow interaction
            yield return new DelayAction(30);
            var b = CardRepository.GenerateCardBehaviorFromData(createdCard.card.data);
            foreach (var a in b.OnInteraction(hero, createdCard, context)) // HACK: force direct interaction with newly created door
                yield return a;
        }
        public IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            if (reason == RemoveFromBoardReason.EndOfRound)
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.EndOfRoom, runDestroyTriggers: false);
        }
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public void OnCreate(IGameOrSimEntity e, IGameOrSimContext context) { }

        public IEnumerable<IActionData> OnAfterPushedOffBoard(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public abstract class ExitRoomCardBehavior : ICardBehavior {
        public abstract string GenerateBaseDescriptionEN(int quality, bool isEthereal);
        public IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield break; }
        public IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) => null;
        public IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public abstract CardType CardType { get; }
        public InteractionType InteractionType => InteractionType.Move;
        public CardGlowType CardGlowType => CardGlowType.MoveOver;
        public bool ImmuneToBeingRemovedFromBoard => true;

        public IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => false;
        public BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            if (card.hasReachableVia)
                return card.reachableVia.smoothedOnTargetPath;
            else if (gameOrSimContext.FilterBuffs<StolenLocketBuff>().Any())
                return new BoardLocation[0]; // teleport
            else
                return null;
        }
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            return !card.hasReachableVia && gameOrSimContext.FilterBuffs<StolenLocketBuff>().Any();
        }
        public IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!card.hasReachableVia)
            { // HACK: we detect the teleport via this
                yield return new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Teleport);

                var teleportEnablerBuff = context.FilterBuffs<StolenLocketBuff>().FirstOrDefault();
                if (teleportEnablerBuff != null)
                {
                    var teleportEnablerCard = context._GetEntityWithID(teleportEnablerBuff.Source);
                    if (teleportEnablerCard != null && teleportEnablerCard.hasCard && CardRepository.GenerateCardBehaviorFromData(teleportEnablerCard.card.data) is StolenLocketBehavior slb)
                    {
                        foreach (var a in slb.OnAfterTeleport(hero, teleportEnablerCard, context))
                            yield return a;
                    }
                }
            }
            else
            {
                yield return CombatService.MoveHeroOntoActions(context, card);
            }

            yield return new TriggerSoundAction("doorOpen");
            yield return new ModifyHeroRoomStateAction(hero.iD.value, HeroRoomState.ExitRoom);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);

            foreach (var a in OnAfterExited(hero, card, context))
                yield return a;
        }

        protected abstract IEnumerable<IActionData> OnAfterExited(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context);

        public IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            if (reason == RemoveFromBoardReason.EndOfRound)
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.EndOfRoom, runDestroyTriggers: false);
        }
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public void OnCreate(IGameOrSimEntity e, IGameOrSimContext context) { }

        public IEnumerable<IActionData> OnAfterPushedOffBoard(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public class HallWayCardBehavior : ExitRoomCardBehavior
    {
        public override CardType CardType => CardType.Hallway;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Find Loot[/b]\n[b]Go To Next Room[/b]\n(draws new cards)";

        protected override IEnumerable<IActionData> OnAfterExited(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
    }

    public class BacktrackCardBehavior : ExitRoomCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Retrace Your Steps[/b]\n(draws new cards)";

        public override CardType CardType => CardType.Backtrack;

        protected override IEnumerable<IActionData> OnAfterExited(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            context.NumBacktracksEntity.ReplaceNumBacktracks(context.NumBacktracksEntity.numBacktracks.num + 1);

            context.UpdateDungeonRunStats(old => old.AddBacktrack());

            yield break;
        }
    }

    public class TacticalRegroupCardBehavior : ExitRoomCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Retreat And Try Again[/b]\n(draws new cards)"; // TODO: wording

        public override CardType CardType => CardType.Backtrack;

        protected override IEnumerable<IActionData> OnAfterExited(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new BossesTauntComeBackAction();

            // TODO: rework
            context.NumBacktracksEntity.ReplaceNumBacktracks(context.NumBacktracksEntity.numBacktracks.num + 1);
            context.UpdateDungeonRunStats(old => old.AddBacktrack());

            yield break;
        }
    }

    public abstract class BasicLootCardBehavior : ICardBehavior
    {
        public virtual string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public virtual IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield break; }
        public virtual IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) => null;
        public IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context) { yield break; }

        public CardType CardType => CardType.Loot;
        public InteractionType InteractionType => InteractionType.Collect;
        public CardGlowType CardGlowType => CardGlowType.MoveOver;
        public bool ImmuneToBeingRemovedFromBoard => false;

        public bool IsOriginRelevantWhenInteracting(IGameOrSimContext context) => context.FilterBuffs<PlunderersRingBuff>().Any();
        public BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext)
        {
            if (!gameOrSimContext.isOnMezzanine && gameOrSimContext.FilterBuffs<PlutusRingBuff>().Any())
                return new BoardLocation[0];
            else
                return (card.hasReachableVia) ? card.reachableVia.smoothedOnTargetPath : null;
        }
        public bool InteractionByTeleport(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => !gameOrSimContext.isOnMezzanine && gameOrSimContext.FilterBuffs<PlutusRingBuff>().Any();
        public virtual IEnumerable<IActionData> OnInteraction(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (!context.isOnMezzanine && context.FilterBuffs<PlutusRingBuff>().Any())
                yield return new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Teleport);
            else
                yield return CombatService.MoveHeroOntoActions(context, card);

            foreach (var a in OnCollect(hero, card, context))
                yield return a;

            if (context.FilterBuffs<PlunderersRingBuff>().Any() && hero.hasPreviousBoardLocation && !context._GetCardsWithBoardLocation(hero.previousBoardLocation.location).Any())
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardAtBoardLocationAction(new CardDataID("lingeringFlame"), hero.previousBoardLocation.location, flipUp: true);
                yield return new DelayAction(10);
            }
        }
        public abstract IEnumerable<IActionData> OnCollect(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context);
        public virtual IEnumerable<IActionData> OnSlowInteraction(IGameOrSimEntity hero, EntityID cardID, BoardLocation heroLocationBeforeEnemyTurn, BoardLocation cardLocationAtStartOfTurn, IGameOrSimContext context)
        {
            yield break;
        }
        public IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;

        public IEnumerable<IActionData> OnBeforeEnemyTurnWhileOnBoardAndFlipped(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnStartRoomInPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext gameOrSimContext, bool onMezzanine) { yield break; }
        public IEnumerable<IActionData> OnExitRoomWhileOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) => null;
        public IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            yield return new DestroyCardAction(card.iD.value, reason == RemoveFromBoardReason.EndOfRound ? DestroyCardReason.EndOfRoom : DestroyCardReason.Other, runDestroyTriggers: false);
        }
        public IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context) { yield break; }
        public virtual void OnCreate(IGameOrSimEntity e, IGameOrSimContext context) { }

        public bool RequiresAttention(IGameOrSimEntity card) => false;
    }

    public class CellKeyLootCardBehavior : BasicLootCardBehavior
    {
        public override IEnumerable<IActionData> OnCollect(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("treasureCollect");

            var cardPosition = ((GameEntity)card).position.value;
            yield return new AnimatedCollectKeyAction(cardPosition);

            yield return new CollectKeyAction(1);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }


    public class UnidentifiedLootCardBehavior : BasicLootCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Find Loot...[/b]";

        public override IEnumerable<IActionData> OnCollect(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var seedOffset = (card.hasBoardLocation) ? card.boardLocation.location.GetStableHashCode() : -1;
            yield return new FindLootAction(false, seedOffset, false);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }


    public class DeckInventoryBehavior : BasicLootCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "{deck}";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex)
        {
            yield break;
        }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang)
        {
            string deckName = null;
            if (card != null && card.hasDeckInventory)
            {
                var heroClass = card.deckInventory.heroClass;
                var deckID = card.deckInventory.deckID;
                deckName = StartingDeckBuilder.GetDeckName(heroClass, deckID);
            }
            var deckFormat = (deckName != null) ? $"[b]{deckName}[/b]" : "";
            yield return ("deck", deckFormat);
        }

        public override IEnumerable<IActionData> OnCollect(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card == null || !card.hasDeckInventory)
                yield break;

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);

            var mOld = MetaSaveSystem.LoadOrDefault();
            var deckID = card.deckInventory.deckID;
            var mNew = mOld.AddUnlockedDeck(deckID);
            MetaSaveSystem.Save(mNew);
            // TODO: let quartermaster react?
            var heroClass = context.HeroEntity.card.data.BaseID; // HACK
            TmpUnlockSceneData.DeckUnlocks = TmpUnlockSceneData.DeckUnlocks.Append((heroClass, deckID)).Distinct().ToArray();

            yield return new DelayAction(30);
            yield return new TriggerAnimationAction(context.HeroEntity.iD.value, new AnimationTrigger("deckUnlocked"));
            yield return new TriggerSoundAction("roomCleared");
            yield return new DelayAction(30);
        }
    }

    public abstract class BasicGoldLootCardBehavior : BasicLootCardBehavior
    {
        public abstract int Worth { get; }

        public override IEnumerable<IActionData> OnCollect(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var cardPosition = ((GameEntity)card).position.value;
            yield return new CollectGoldAction(Worth, cardPosition);

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
    }

    public class Emerald1CardBehavior : BasicGoldLootCardBehavior
    {
        public override int Worth => 1;
    }
    public class Emerald5CardBehavior : BasicGoldLootCardBehavior
    {
        public override int Worth => 5;
    }
    public class Emerald10CardBehavior : BasicGoldLootCardBehavior
    {
        public override int Worth => 10;
    }
    public class Emerald20CardBehavior : BasicGoldLootCardBehavior
    {
        public override int Worth => 20;
    }
    public class Emerald50CardBehavior : BasicGoldLootCardBehavior
    {
        public override int Worth => 50;
    }
}
