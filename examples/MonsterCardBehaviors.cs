using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Action;
using CardStuff.Card;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace Core.Card
{

    public class SkelegnonCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 6;
        protected override int BaseBaseDefense => 0;
    }

    public class SentryCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }
    }

    public class SkeletonCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 0;
    }
    public class OrcCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 5;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 0;
    }
    public class GruntCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 6;
        protected override int BaseBaseHealth => 24;
        protected override int BaseBaseDefense => 3;
    }

    public class MercenaryCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.WalkStraightAndDiagonalTowardsHero4Neighbor;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 11;
        protected override int BaseBaseDefense => 1;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }
    }

    public class BanditCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Teleports next to Hero if on same row or column";
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // teleport
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.AreOnSameRowOrCol(card.boardLocation.location, hero.boardLocation.location))
            {
                var target = BoardUtils.Get4Neighbors(hero.boardLocation.location)
                    .Where(l => BoardUtils.AreOnSameRowOrCol(card.boardLocation.location, l))
                    .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                    .OrderByDescending(l => BoardUtils.GetManhattenDistance(card.boardLocation.location, l))
                    .DefaultIfEmpty(BoardLocation.CreateUnsafe(-1, -1))
                    .First();
                if (target != BoardLocation.CreateUnsafe(-1, -1))
                {
                    yield return new MoveCardOnBoardAction(card.iD.value, target, MoveReason.Teleport);
                }
            }

            // attack
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }
        }
    }

    public class LackeyCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Swaps place[/b] after attack";
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
                if (card.hasID && card.hasBoardLocation && hero.hasID && hero.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero))
                    yield return new ParallelAction(new MoveCardOnBoardAction(card.iD.value, hero.boardLocation.location, MoveReason.Swap), new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Swap));
            }
        }
    }

    public class MimicBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Defeated[/b]: place [b]20 Emeralds[/b] at its spot";
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald20");
        }

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 1;

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue)
                yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald20"), lastBoardLocation.Value, true);
        }
    }


    public class GhoulCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 6;
        protected override int BaseBaseHealth => 27;
        protected override int BaseBaseDefense => 0;

        private int Heal(int quality, int loopIndex) => 3 + loopIndex * 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b] if not moved this turn";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("heal", Heal(quality, loopIndex)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;

            if (!card.isMovedThisTurn && card.hasID && card.hasCard)
                yield return new HealTargetAction(card.iD.value, Heal(card.card.data.QualityModifier, LoopingUtils.LoopIndex(context)), true);
        }
    }

    public class MummyBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 0;

        public override MovementType MovementType => MovementType.WalkOnlyDiagonalTowardsHero8Neighbor;
    }


    public class MedjayCardBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.WalkOnlyDiagonalTowardsHero8Neighbor;
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Diagonal;
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.DiagonalAttack;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are8Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;
        }

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 18;
        protected override int BaseBaseDefense => 3;
    }

    public class ZealotBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.CloseRange;
        public override MovementType MovementType => MovementType.WalkStraightAndDiagonalTowardsHero4Neighbor;
        protected override int BaseBaseAttack => 6;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Defeated[/b]: place [b]Sarcophagus[/b] at its spot";

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("sarcophagus");
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardAtBoardLocationAction(new CardDataID("sarcophagus"), lastBoardLocation.Value);
                yield return new DelayAction(30);
            }
        }
    }

    public class ZombieBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 5;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gain [b]1 Sleep[/b] after moving";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;

            if (card.isMovedThisTurn)
                yield return new TrySleepAction(card.iD.value, 1);
        }
    }

    public class PackRatBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 2;
        protected override int BaseBaseHealth => 6;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Defeated[/b]: draw non-[b]Pack Rat[/b] Monster at its spot";

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue)
                yield return new DrawCardAction(lastBoardLocation.Value, CardArea.EnemyDrawPile, filter: (c) => c.hasCard && c.card.data.BaseID != "packRat");
        }
    }

    public class ArcherCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Ranged;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 9;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Ranged[/b]\n[b]Cowardly[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            } 
            
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.HasCardinalLineOfSight(card.boardLocation.location, hero.boardLocation.location, (l) =>
            {
                var atLocation = context._GetCardsWithBoardLocation(l);
                if (!atLocation.Empty() && !atLocation.Any(e => e.isHero))
                    return false;
                return true;
            }))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }
        }
    }


    public class RangerCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.RangedDiagonal;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 11;
        protected override int BaseBaseDefense => 1;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Ranged[/b]\n[b]Shifty[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.RangedDiagonalAttack;
            yield return KeywordID.Shifty;
            yield return KeywordID.Retreat;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            bool isTraversableF(BoardLocation l)
            {
                var atLocation = context._GetCardsWithBoardLocation(l);
                return atLocation.Empty() || atLocation.Any(e => e.isHero);
            }
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && (
                BoardUtils.HasCardinalLineOfSight(card.boardLocation.location, hero.boardLocation.location, isTraversableF) ||
                BoardUtils.HasDiagonalLineOfSight(card.boardLocation.location, hero.boardLocation.location, isTraversableF)))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
                if (card.hasID)
                    yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
            }
        }
    }

    public class PitatiCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Ranged;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 2;
        protected override int BaseBaseHealth => 9;
        protected override int BaseBaseDefense => 1;
        private int Poison(int loopIndex) => 2 + loopIndex * 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Ranged[/b], [b]Shifty[/b]\nAttack: apply [b]{poison} Poison[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("poison", Poison(loopIndex)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
            yield return KeywordID.Poison;
            yield return KeywordID.Shifty;
            yield return KeywordID.Retreat;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.HasCardinalLineOfSight(card.boardLocation.location, hero.boardLocation.location, (l) =>
            {
                var atLocation = context._GetCardsWithBoardLocation(l);
                if (!atLocation.Empty() && !atLocation.Any(e => e.isHero))
                    return false;
                return true;
            }))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType, Poison(LoopingUtils.LoopIndex(context)));
                if (card.hasID)
                    yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
            }
        }
    }

    public class StalkerBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Ranged;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Ranged[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Ranged;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && CombatService.CanHeroBeTargetedByMonster(hero) && BoardUtils.HasCardinalLineOfSight(card.boardLocation.location, hero.boardLocation.location, (l) =>
            {
                var atLocation = context._GetCardsWithBoardLocation(l);
                if (!atLocation.Empty() && !atLocation.Any(e => e.isHero))
                    return false;
                return true;
            }))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }
        }
    }

    public class WizardCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Omni;
        public override ProjectileType ProjectileType => ProjectileType.WizardBall;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 0;
        private int ChannelInit(IGameOrSimContext context) => LoopingUtils.GetMonsterTimerModifier(6, 1, context);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: attacks";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
            yield return KeywordID.Timer;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                if (CombatService.CanHeroBeTargetedByMonster(hero))
                {
                    yield return new EnemyAttackAction(CalculateAttackValue(card), card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
                }
                else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                if (card != null && card.hasID) // card might die while attacking
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class VoidPriestCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Diagonal;
        public override ProjectileType ProjectileType => ProjectileType.WizardBall;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 18;
        protected override int BaseBaseDefense => 1;
        private int ChannelInit(IGameOrSimContext context) => LoopingUtils.GetMonsterTimerModifier(4, 2, context);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: [b]drop[/b] topmost Weapon";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
            yield return KeywordID.Timer;
            yield return KeywordID.Drop;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                var topWeapon = CombatService.GetTopHeroWeapon(context, false);
                if (CombatService.CanHeroBeTargetedByMonster(hero) && topWeapon != null)
                {
                    yield return new DelayAction(20);
                    yield return new SpawnProjectileAction(ProjectileType.WizardBall, ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location), ScreenPositionUtils.CalculateBoardPositionScreen(hero.boardLocation.location));
                    yield return new TriggerSoundAction(new SoundTrigger("debuff")); // TODO: better sound?
                    yield return new RequestDropCardAction(topWeapon.iD.value, RequestDropReason.ForceDrop);
                    yield return new DelayAction(10);
                }
                else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                if (card != null && card.hasID) // card might die while attacking
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
            }

            // melee attack
            if (card.hasBoardLocation && BoardUtils.Are8Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                yield return new DelayAction(20); // NOTE: delay a bit to bring space between tick/conjure and attack
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class MedusaCardBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.Omni;
        public override ProjectileType ProjectileType => ProjectileType.WizardBall; // TODO: better projectile
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 3;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 3;
        private int ChannelInit(IGameOrSimContext context) => 5; // keep timer at 5, even on higher difficulties -> impossible and unfun otherwise; LoopingUtils.GetMonsterTimerModifier(5, 1, context);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: attacks and applies [b]1 Sleep[/b]";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
            yield return KeywordID.Timer;
            yield return KeywordID.Sleep;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                if (CombatService.CanHeroBeTargetedByMonster(hero))
                {
                    yield return new EnemyAttackAction(CalculateAttackValue(card), card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
                    if (hero != null && hero.hasID && card.hasBoardLocation) // HACK, TODO: actually cancel sleeping if attack was cancelled
                        yield return new TrySleepAction(hero.iD.value, 1);
                }
                else
                {
                    yield return new FizzleAction(card.iD.value);
                }
                if (card != null && card.hasID) // card might die while attacking
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class HekaPriestBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 0;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 2;
        private int ChannelInit(IGameOrSimContext context) => LoopingUtils.GetMonsterTimerModifier(4, 1, context);
        private int Heal(int loopIndex) => 3 + loopIndex * 3;

        public override MovementType MovementType => MovementType.None;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: others [b]heal {heal}[/b]"; // NOTE: "other Monsters" too long, using "others" instead
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("heal", Heal(loopIndex)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
            yield return KeywordID.Timer;
            yield return KeywordID.Heal;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);

                var damagedEnemies = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && c.isCardFlipped && c.hasHealth && c.isEnemy && c.iD.value != card.iD.value && c.health.value < c.health.max).ToList();
                if (damagedEnemies.Count <= 0)
                {
                    yield return new FizzleAction(card.iD.value);
                }
                else
                {
                    foreach (var e in damagedEnemies)
                    {
                        yield return new SpawnProjectileAction(ProjectileType.EnemyBuff, ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location), ScreenPositionUtils.CalculateBoardPositionScreen(e.boardLocation.location));
                        yield return new HealTargetAction(e.iD.value, Heal(LoopingUtils.LoopIndex(context)), true);
                        yield return new DelayAction(10);
                    }
                }

                yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class GuardianCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 5;
        protected override int BaseBaseHealth => 42;
        protected override int BaseBaseDefense => 5;

        public override MovementType MovementType => MovementType.WalkOnlyStraightTowardsHero4Neighbor;
        private int Defense(int loopIndex) => 1 + loopIndex * 2;
        private int Attack(int loopIndex) => 1 + loopIndex * 2;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Gets +{defense} Defense and +{attack} Attack if moved this turn";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("defense", Defense(loopIndex)); yield return ("attack", Attack(loopIndex)); }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;


            if (card.isMovedThisTurn && card.hasID && card.hasCard)
            {
                yield return new DelayAction(10);
                yield return new ModifyChannelingAction(card.iD.value, 0, true);
                yield return new ModifyDefenseValueModifierAction(card.iD.value, Defense(LoopingUtils.LoopIndex(context)));
                yield return new ModifyAttackValueModifierAction(card.iD.value, Attack(LoopingUtils.LoopIndex(context)));
                yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("upgrade"));
                yield return new DelayAction(30);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            //e.AddChannelling(ChannelInit);
        }
    }

    public class ProtectorCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 2;
        protected override int BaseBaseHealth => 45;
        protected override int BaseBaseDefense => 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "If 8-neighbor Monster would take damage, take it instead";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
        }
        
        public override IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            foreach (var a in base.OnTryToRemoveFromBoard(card, context, reason)) yield return a;
            if (card.hasID)
                yield return new RemoveGlobalBuffsFromSource(card.iD.value);
        }

        public override IEnumerable<IActionData> OnHPReducedToZero(IGameOrSimEntity card, HurtType hurtType, IGameOrSimContext context)
        {
            if (card.hasID)
                yield return new RemoveGlobalBuffsFromSource(card.iD.value);

            foreach (var a in base.OnHPReducedToZero(card, hurtType, context)) yield return a;
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ApplyGlobalBuff(new ProtectorBuff(card.iD.value));
        }
    }

    public class RatConjurerBehavior : BasicMonsterCardBehavior
    {
        public override AttackType CalculateAttackType(IGameOrSimEntity card) => AttackType.CloseRange;
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 1;
        protected override int BaseBaseHealth => 15;
        protected override int BaseBaseDefense => 0;
        private int ChannelInit(IGameOrSimContext context) => LoopingUtils.GetMonsterTimerModifier(5, 1, context);

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: add [b]Pack Rat[/b] to Deck";
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Timer;
            yield return KeywordID.Cowardly;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("packRat");
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                //yield return new ActivateCardAction(card.iD.value);
                yield return new DelayAction(10);
                yield return new ModifyChannelingAction(card.iD.value, 0, true);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("cast"));
                yield return new TriggerSoundAction("cast");
                var createdCardID = LoopingUtils.ModifyMonsterCard(new CardDataID("packRat"), LoopingUtils.LoopIndex(context), context.HeroEntity.card.data.BaseID, context.GetRandomSeedForIngameRoom());
                yield return new CreateCardInDrawPileStartingFromBoardAction(createdCardID, card.boardLocation.location, true);
                yield return new DelayAction(30);
                if (card != null && card.hasID) // card might die while doing?
                {
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
                }
            }

            // melee attack
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                yield return new DelayAction(20); // NOTE: delay a bit to bring space between tick/conjure and attack
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class ShamanCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 0;
        protected override int BaseBaseHealth => 12;
        protected override int BaseBaseDefense => 0;
        private int ChannelInit(IGameOrSimContext context) => LoopingUtils.GetMonsterTimerModifier(4, 1, context);
        private int Defense(int loopIndex) => 2 + loopIndex * 5;

        public override MovementType MovementType => MovementType.None;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "[b]Eager[/b], [b]Cowardly[/b]\n[b]Timer[/b]: others get +{defense} Defense"; // NOTE: "other Monsters" too long, using "others" instead
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("defense", Defense(loopIndex)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Cowardly;
            yield return KeywordID.Retreat;
            yield return KeywordID.Timer;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            { // retreat
                yield return new CowardlyOrShiftyRetreatMonsterAction(card.iD.value);
                if (!card.hasBoardLocation)
                    yield break;
            }

            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);

                var enemies = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && c.isCardFlipped && c.hasHealth && c.isEnemy && c.iD.value != card.iD.value).ToList();

                if (enemies.Count <= 0)
                {
                    yield return new FizzleAction(card.iD.value);
                }
                else
                {
                    foreach (var e in enemies)
                    {
                        yield return new SpawnProjectileAction(ProjectileType.EnemyBuff, ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location), ScreenPositionUtils.CalculateBoardPositionScreen(e.boardLocation.location));
                        yield return new ModifyDefenseValueModifierAction(e.iD.value, Defense(LoopingUtils.LoopIndex(context)));
                        yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
                        yield return new TriggerAnimationAction(e.iD.value, new AnimationTrigger("upgrade"));
                        yield return new DelayAction(10);
                    }
                }

                yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(context));
        }
    }

    public class BruteCardBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 1;
        protected override int BaseBaseHealth => 33;
        protected override int BaseBaseDefense => 3;
        private int Attack(int loopIndex) => 2 + loopIndex * 4;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Gets +{attack} Attack if it was attacked this turn";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("attack", Attack(loopIndex)); }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBeenAttackedThisTurn)
            {
                yield return new IncreaseAttackAction(card.iD.value, Attack(LoopingUtils.LoopIndex(context)), false);
                yield return new DelayAction(30);
            }
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType);
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;
        }
    }


    public class TrenchRatBehavior : BasicMonsterCardBehavior
    {
        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 24;
        protected override int BaseBaseDefense => 6;
        private int Poison(int loopIndex) => 3 + loopIndex * 3;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            return "Attack: apply [b]{poison} Poison[/b]";
        }
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("poison", Poison(loopIndex)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, CalculateAttackType(card), ProjectileType, Poison(LoopingUtils.LoopIndex(context)));
            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;
        }
    }

    public class KidnapperBehavior : BasicMonsterCardBehavior
    {
        public override MovementType MovementType => MovementType.None;

        protected override int BaseBaseAttack => 4;
        protected override int BaseBaseHealth => 9;
        protected override int BaseBaseDefense => 0;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Defeated[/b]: place [b]Brother[/b] at its spot";

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("brother");
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue)
                yield return new CreateCardAtBoardLocationAction(new CardDataID("brother"), lastBoardLocation.Value);
            else
                yield return new CreateCardInDrawPileAction(new CardDataID("brother"));
        }
    }
}
