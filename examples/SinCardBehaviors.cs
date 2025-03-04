using Action;
using Base.Board;
using Base.Card;
using Base.Utils;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{

    public class VanityCardBehavior : BasicSinCardBehavior
    {
        public override bool IsCantripActive() => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero takes [b]2 damage[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }


        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, 2, HurtSource.Sin, HurtType.Regular, false);
            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }
    public class EnvyCardBehavior : BasicSinCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero takes [b]3 damage[/b]";

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, 3, HurtSource.Sin, HurtType.Regular, false);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }
    public class RashnessCardBehavior : BasicSinCardBehavior
    {
        public override bool IsCantripActive() => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero loses [b]1 HP[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, 1, HurtSource.Sin, HurtType.Regular, true);
            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class WrathCardBehavior : BasicSinCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Each Monster gets +1 Attack";

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                    e => new SequenceAction(
                        new IncreaseAttackAction(e.iD.value, 1, false),
                        new DelayAction(10)
                    ),
                    e => NoopAction.Instance
                ));
            yield return new SequenceAction(t);

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class DesireCardBehavior : BasicSinCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Each Monster [b]heals 2[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // heal monsters
            var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                    e => new SequenceAction(new HealTargetAction(e.iD.value, 2, true), new DelayAction(10)),
                    e => NoopAction.Instance
                ));
            yield return new SequenceAction(t);

            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true);
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isEthereal = true;
        }
    }

    public class GreedCardBehavior : BasicSinCardBehavior
    {
        public override bool IsCantripActive() => true;

        private int Cost => 10;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Lose [b]{cost} Emeralds[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("cost", Cost); }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new LoseGoldAction(Cost, null);
            
            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isTemporary = true;
        }
    }

    public class SlothCardBehavior : BasicSinCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "No action";

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isTemporary = true;
        }
    }

    public class GluttonyCardBehavior : BasicSinCardBehavior
    {
        public override bool IsCantripActive() => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply [b]1 Sleep[/b] to Hero";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
            yield return KeywordID.Replaceable;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TrySleepAction(hero.iD.value, 1);

            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
            e.isTemporary = true;
        }
    }

    public class OriginalSinBehavior : BasicSinCardBehavior
    {
        public override bool IsCantripActive() => true;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Hero loses [b]2 HP[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new HurtTargetAction(hero.iD.value, 2, HurtSource.Sin, HurtType.Regular, true);
            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);
        }
    }
}
