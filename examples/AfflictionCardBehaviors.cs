using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public class FatigueAfflictionBehavior : BasicAfflictionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal)
        {
            if (HPLoss(quality) == 0)
                return "No action";
            else
                return "Lose [b]{hpLoss} HP[/b]";
        }
        private int HPLoss(int quality) => quality;
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("hpLoss", HPLoss(quality)); }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("pickupSin"); // TODO

            var loss = HPLoss(card.card.data.QualityModifier);
            if (loss > 0)
            {
                yield return new HurtTargetAction(hero.iD.value, loss, HurtSource.Affliction, HurtType.Regular, true);
            }

            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }
    }

    public class AdamophobiaAfflictionBehavior : BasicAfflictionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], you cannot gain [b]Emeralds[/b], [b]Turn End[/b]: [b]lose {loss} Emeralds[/b]";

        private int GoldLoss(int quality) => 1 + quality;
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("loss", GoldLoss(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }

        public override IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ApplyGlobalBuff(new CannotGainGoldDebuff(card.iD.value));
        }

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ActivateCardAction(card.iD.value);
            yield return new DelayAction(15);
            yield return new TriggerSoundAction("pickupSin"); // TODO
            yield return new LoseGoldAction(GoldLoss(card.card.data.QualityModifier), null);
            yield return new DelayAction(15);
            yield return new DeactivateCardAction(card.iD.value);
        }
    }

    public class ClaustrophobiaAfflictionBehavior : BasicAfflictionCardBehavior, ICantrippingCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], [b]Turn End[/b]: take [b]{damage} damage[/b] if Hero can reach less than {minSpotThreshold} spots";

        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("damage", Damage(quality)); yield return ("minSpotThreshold", MinSpotThreshold(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Replaceable;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        private int Damage(int quality) => 1;
        private int MinSpotThreshold(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnTurnEndWhileOnBoardAndFlippedOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            // count reachable empty spots
            var reachableSpots = context._GetAllBoardLocationEntities().Where(e => e.hasReachableVia && !context._GetCardsWithBoardLocation(e.boardLocation.location).Any()).Count();

            if (reachableSpots < MinSpotThreshold(card.card.data.QualityModifier))
            {
                yield return new ActivateCardAction(card.iD.value);
                yield return new DelayAction(15);
                yield return new TriggerSoundAction("pickupSin"); // TODO
                yield return new HurtTargetAction(hero.iD.value, Damage(card.card.data.QualityModifier), HurtSource.Affliction, HurtType.Regular);
                yield return new DelayAction(15);
                yield return new DeactivateCardAction(card.iD.value);
            }
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => true;
    }

    public class BlasphemyAfflictionBehavior : BasicAfflictionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], [b]Lose HP[/b]: [b]lose {loss}[/b] more [b]HP[/b]";

        private int HPLoss(int quality) => 1 + quality;
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("loss", HPLoss(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }

        public override IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ApplyGlobalBuff(new HeroHPLossModifierBuff(HPLoss(card.card.data.QualityModifier), card.iD.value));
        }
    }

    public class KosmemophobiaAfflictionBehavior : BasicAfflictionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Drop[/b] {num} Trinket(s)";

        private int Num(int quality) => 1 + quality;
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("num", Num(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Drop;
            yield return KeywordID.Trinket;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var trinkets = CombatService.GetTopNEquippedTrinkets(context, Num(card.card.data.QualityModifier));
            foreach (var trinket in trinkets)
            {
                yield return new TriggerSoundAction("debuff");
                yield return new RequestDropCardAction(trinket.iD.value, RequestDropReason.ForceDrop);
                yield return new DelayAction(15);
            }

            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }
    }


    public class WoundAfflictionBehavior : BasicAfflictionCardBehavior
    {
        private int ChannelInit => 4;
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], [b]Timer[/b]: [b]lose {hpLoss} HP[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("hpLoss", HPLoss(quality)); }

        private int HPLoss(int quality) => 2 + quality;
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Timer;
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
            {
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
                        yield return new ActivateCardAction(card.iD.value);
                        yield return new ModifyChannelingAction(card.iD.value, -1, false);
                        yield return new HurtTargetAction(hero.iD.value, HPLoss(card.card.data.QualityModifier), HurtSource.Affliction, HurtType.Regular, true);
                        yield return new DelayAction(15);
                        yield return new ModifyChannelingAction(card.iD.value, ChannelInit, true);
                        yield return new DeactivateCardAction(card.iD.value);
                    }
                }
            }
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit, true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit, true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddChannelling(ChannelInit);
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.isCardFlipped && card.hasBoardLocation && card.hasChannelling && card.channelling.value == 1;

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
        }
    }


    public class ShameCardBehavior : BasicAfflictionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose {hpLoss} HP[/b], remove from Deck if unupgraded, otherwise downgrade";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("hpLoss", HPLoss(quality)); }

        private int HPLoss(int quality) => 1;
        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var loss = HPLoss(card.card.data.QualityModifier);
            if (loss > 0)
            {
                yield return new HurtTargetAction(hero.iD.value, loss, HurtSource.Affliction, HurtType.Regular, true);
            }

            var cardDataID = card.card.data;
            if (cardDataID.QualityModifier <= 0)
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardDataID);
                yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other);
                yield return new DelayAction(20);
                yield return new TriggerSoundAction("heroUpgrade"); // TODO: better sound?
                yield return new DelayAction(20);
            } else
            {
                yield return new PermanentlyRemoveCardFromDeckAction(cardDataID);
                yield return new PermanentlyAddCardToDeckAction(new CardDataID(cardDataID.BaseID, cardDataID.QualityModifier - 1));

                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("debuff"));
                yield return new TriggerSoundAction("debuff");
                card.ReplaceCard(new CardDataID(cardDataID.BaseID, cardDataID.QualityModifier - 1));
                yield return new DelayAction(30);

                yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
            }
        }
    }

    public class FutilityAfflictionBehavior : BasicAfflictionCardBehavior, ICantrippingCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Lose {hpLoss} HP[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("hpLoss", HPLoss(quality)); }

        private int HPLoss(int quality) => 1 + quality;
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }

        protected override IEnumerable<IActionData> OnPickup(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("pickupSin"); // TODO

            var loss = HPLoss(card.card.data.QualityModifier);
            if (loss > 0)
            {
                yield return new HurtTargetAction(hero.iD.value, loss, HurtSource.Affliction, HurtType.Regular, true);
            }

            if (card.hasBoardLocation)
            {
                yield return new DrawCardAction(card.boardLocation.location, CardArea.HeroDrawPile);
                yield return new TryToRemoveFromBoardAction(card.iD.value, RemoveFromBoardReason.Replace);
            }
        }

        public bool IsCantripActive() => true;
        public ICantrippingCardBehavior.CantripTarget GetCantripType() => ICantrippingCardBehavior.CantripTarget.DrawOntoBoard;
        public ICantrippingCardBehavior.CantripTrigger GetCantripTrigger() => ICantrippingCardBehavior.CantripTrigger.Interact;
        public bool ApplyDrawFilter(IGameOrSimEntity candidate) => true;
    }
}
