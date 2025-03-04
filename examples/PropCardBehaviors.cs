using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Card;
using Core.Service;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public class PillarCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public PillarCardBehavior() { }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(6, 6);
        }
    }

    public class WallCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
        }

        public override InteractionType InteractionType => InteractionType.None;
        public override BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => null;

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }
    }


    public class RoomCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context) { }
        public override CardGlowType CardGlowType => CardGlowType.MoveOver;
        public override BoardLocation[] AllowsInteractionVia(IGameOrSimEntity card, IGameOrSimEntity currentWeapon, IGameOrSimEntity currentSpell, IGameOrSimContext gameOrSimContext) => null;
    }

    public class BarrelCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(1, 1);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }
    }


    public class GravestoneCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: place [b]{zombie}[/b] at its spot";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("zombie", CardNames.Generate(new CardDataID("zombie"), lang)); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("zombie");
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(3, 3);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                var createdCardID = LoopingUtils.ModifyMonsterCard(new CardDataID("zombie"), LoopingUtils.LoopIndex(context), context.HeroEntity.card.data.BaseID, context.GetRandomSeedForIngameRoom());
                yield return new CreateCardAtBoardLocationAction(createdCardID, lastBoardLocation.Value);
                yield return new DelayAction(30);
            }
        }
    }

    public class RustyChestCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: place [b]20 Emeralds[/b] at its spot";

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald20");
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(9, 9);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald20"), lastBoardLocation.Value, true);
                context.UpdateDungeonRunStats(old => old.AddOpenedChest());
                yield return new DelayAction(30);
            }
        }
    }

    public class AchingChestCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Lose [b]3 HP[/b] each turn, [b]destroy[/b]: place [b]20 Emeralds[/b] at its spot";

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("emerald20");
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(30, 30);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
            {
                yield return new HurtTargetAction(card.iD.value, 3, HurtSource.Prop, HurtType.Regular, true);
            }
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald20"), lastBoardLocation.Value, true);
                context.UpdateDungeonRunStats(old => old.AddOpenedChest());
                yield return new DelayAction(30);
            }
        }
    }

    public class BloodyChestActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public BloodyChestActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterCardDestroyedPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterCardDestroyedPlaceholderAction dca) 
            {
                if (!self.hasCardArea || self.cardArea.Area != CardArea.Board || !self.hasBoardLocation || !self.hasTrinketCounter || !self.isCardFlipped)
                    yield break;
                if (dca.cardType != CardType.Monster)
                    yield break;
                    
                self.ReplaceTrinketCounter(self.trinketCounter.counter + 1);

                if (self.trinketCounter.counter >= 2)
                {
                    yield return new DestroyCardAction(self.iD.value, DestroyCardReason.BloodyChestTrigger); // HACK: special reason
                }
            }
        }
    }

    public class BloodyChestCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], defeat 2 Monsters: replace with [b]20 Emeralds[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
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

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(6, 6);
            e.AddTrinketCounter(0); // HACK: abusing trinket counter for internal counting
            e.AddActionInterceptorSource(new IActionInterceptor[] { new BloodyChestActionInterceptor(e.iD.value) });
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (reason == DestroyCardReason.BloodyChestTrigger && lastBoardLocation.HasValue)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                yield return new CreateCardAtBoardLocationAction(new CardDataID("emerald20"), lastBoardLocation.Value, true);
                context.UpdateDungeonRunStats(old => old.AddOpenedChest());
            }
        }
    }

    public class XBarrelCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: deal [b]{damage} damage[/b] to diagonal cards";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield break; }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", Damage(context)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
        }

        private int Damage(IGameOrSimContext context) => CalculateDamage(6, context);

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                var damage = Damage(context);
                var t = BoardUtils.GetAll()
                        //.OrderBy(l => BoardUtils.GetSortIndex(l))
                        .OrderBy(l => BoardUtils.GetManhattenDistance(l, lastBoardLocation.Value)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                        .Where(l => BoardUtils.AreOnDiagonal(l, lastBoardLocation.Value))
                        .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Prop, HurtType.Fire),
                            e =>
                            {
                                return new SequenceAction(
                                    new TriggerSoundAction("fire"),
                                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                    new DelayAction(5)
                                );
                            }
                        ));

                yield return new SequenceAction(t);
                yield return new DelayAction(14);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(1, 1);
        }
    }

    public class OBarrelCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: deal [b]{damage} damage[/b] to 8-neighbor cards";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield break; }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", Damage(context)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.EightNeighbor;
            yield return KeywordID.FireDamage;
        }

        private int Damage(IGameOrSimContext context) => CalculateDamage(6, context);

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                var damage = Damage(context);
                var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are8Neighbors(l, lastBoardLocation.Value))
                        .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Prop, HurtType.Fire),
                            e =>
                            {
                                return new SequenceAction(
                                    new TriggerSoundAction("fire"),
                                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                    new DelayAction(5)
                                );
                            }
                        ));

                yield return new SequenceAction(new SequenceAction(t));
                yield return new DelayAction(14);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(1, 1);
        }
    }


    public class IBarrelCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: deal [b]{damage} damage[/b] to cards in this column";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield break; }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", Damage(context)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
        }

        private int Damage(IGameOrSimContext context) => CalculateDamage(6, context);

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                var damage = Damage(context);
                var t = BoardUtils.GetAll()
                        //.OrderBy(l => BoardUtils.GetSortIndex(l))
                        .OrderBy(l => BoardUtils.GetManhattenDistance(l, lastBoardLocation.Value)) // NOTE: we break our usual order here, because it just makes more sense to do it like that
                        .Where(l => BoardUtils.AreOnSameCol(l, lastBoardLocation.Value))
                        .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Prop, HurtType.Fire),
                            e => new SequenceAction(
                                    new TriggerSoundAction("fire"),
                                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                    new DelayAction(5)
                                )
                        ));

                yield return new SequenceAction(t);
                yield return new DelayAction(14);
            }
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(1, 1);
        }
    }


    public class MirageCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: place [b]Illusory Walls[/b] at 4-neighbor spots";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.FourNeighbor;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("illusoryWall");
        }

        private int ChannelInit => 4;

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            // TODO: better destroy sound
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
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
                    yield return new SequenceAction(
                            BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                                .Where(l => BoardUtils.Are4Neighbors(l, card.boardLocation.location))
                                .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                                .Select(l => new SequenceAction(
                                    new TriggerSoundAction("cardCreateOnBoard"),
                                    new CreateCardAtBoardLocationAction(new CardDataID("illusoryWall"), l, true),
                                    new DelayAction(10))));
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit, true);
                }
            }
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
            e.AddHealth(6, 6);

            e.AddChannelling(ChannelInit);
        }
    }


    public class OldPillarCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: [b]destroy[/b]: deal [b]{damage} damage[/b] to 4-neighbor cards";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield break; }
        public override IEnumerable<(string, object)> GenerateDynamicDescriptionPlaceholders(IGameOrSimEntity card, IGameOrSimContext context, string lang) { yield return ("damage", Damage(context)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.FourNeighbor;
        }

        private int Damage(IGameOrSimContext context) => CalculateDamage(6, context);

        private int ChannelInit => 4;

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
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
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                var damage = Damage(context);
                var t = BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, lastBoardLocation.Value))
                        .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth,
                            e => new HurtTargetAction(e.iD.value, damage, HurtSource.Prop, HurtType.Regular),
                            e =>
                            {
                                var locationDiff = BoardUtils.GetVectorDistance(lastBoardLocation.Value, l);
                                var direction = locationDiff.AngleTo(new Vector2(1, 0));
                                return new SequenceAction(
                                    new TriggerSoundAction("heroAttack"), // TODO: better animation
                                    new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("meleeAttack", new Dictionary<string, object> { { "direction", direction } })),
                                    //new TriggerSoundAction("fire"),
                                    //new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                                    new DelayAction(5)
                                );
                            }
                        ));
                yield return new SequenceAction(t);

                yield return new DelayAction(15);
            }
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
            e.AddHealth(3, 3);
            e.AddChannelling(ChannelInit);
        }
        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;
    }


    public class LightningOrbCardBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Trigger [b]Lightning[/b] equal to [b]Time[/b]\n[b]Timer[/b]: [b]destroy[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.LightningX;
            yield return KeywordID.Timer;
            yield return KeywordID.FourNeighbor;
        }

        private int LightningDamage(IGameOrSimEntity card, IGameOrSimContext context) => CalculateDamage(((card.hasChannelling) ? card.channelling.value : 0), context);

        private int ChannelInit => 4;

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield break;
            //yield return new TriggerSoundAction("rockDestroy"); // TODO
            //yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
            {
                foreach(var a in CombatService.TriggerLightning(card, LightningDamage(card, context), context, HurtSource.Prop, card))
                    yield return a;

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
            }
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
            e.AddHealth(3, 3);
            e.AddChannelling(ChannelInit);
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.isCardFlipped && card.hasBoardLocation;
    }

    public class SarcophagusBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: place [b]Zealot[/b] at its spot";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("zealot");
        }

        private int ChannelInit => 6;

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy"); // TODO: better sound?
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped && card.hasBoardLocation)
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
                    yield return new DestroyCardAction(card.iD.value, DestroyCardReason.SarcophagusTrigger); // HACK: special reason
                }
            }
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason == DestroyCardReason.SarcophagusTrigger)
            {
                yield return new TriggerSoundAction("cardCreateOnBoard");
                var createdCardID = LoopingUtils.ModifyMonsterCard(new CardDataID("zealot"), LoopingUtils.LoopIndex(context), context.HeroEntity.card.data.BaseID, context.GetRandomSeedForIngameRoom());
                yield return new CreateCardAtBoardLocationAction(createdCardID, lastBoardLocation.Value);

                yield return new DelayAction(30);
            }
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
            e.AddHealth(15, 15);
            e.AddChannelling(ChannelInit);
        }
        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;
    }

    public class SacredStatueBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: add [b]{blasphemy}[/b] to Deck";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex, string lang) { yield return ("blasphemy", CardNames.Generate(new CardDataID("blasphemy"), lang)); }

        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("blasphemy");
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(9, 9);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("rockDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                yield return new DelayAction(20);
                var blasphemy = new CardDataID("blasphemy");
                yield return new TriggerSoundAction("pickupSin"); // TODO
                yield return new CreateCardInDrawPileStartingFromBoardAction(blasphemy, lastBoardLocation.Value, true, 30);
                yield return new PermanentlyAddCardToDeckAction(blasphemy);
                yield return new DelayAction(20);
            }
        }
    }

    public class PotionShelfBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Destroy[/b]: add random Potion to Deck";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Potion;
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(3, 3);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy");
            yield return new DelayAction(10);
        }

        public override IEnumerable<IActionData> OnAfterDestroy(BoardLocation? lastBoardLocation, DestroyCardReason reason, IGameOrSimContext context)
        {
            if (lastBoardLocation.HasValue && reason != DestroyCardReason.EndOfRoom)
            {
                var seed = context.GetRandomSeedForIngameRoom(lastBoardLocation.Value.GetStableHashCode());
                var potion = PotionUtils.GetRandomPotions(seed, 1, 0).First();
                yield return new PermanentlyAddCardToDeckAction(potion);
                yield return new CreateCardInDrawPileStartingFromBoardAction(potion, lastBoardLocation.Value, true, 30);
            }
        }
    }

    public class VoidRootBehavior : BasicPropCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Eager[/b], prevents Hero from [b]healing[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Eager;
            yield return KeywordID.Heal;
        }

        public override IEnumerable<IActionData> OnStartRoomOnBoardOrTrinket(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new FlipCardAction(card.iD.value, true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(6, 6);
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext context)
        {
            yield return new TriggerSoundAction("woodDestroy"); // TODO
            yield return new DelayAction(10);
        }


        public override IEnumerable<IActionData> OnTryToRemoveFromBoard(IGameOrSimEntity card, IGameOrSimContext context, RemoveFromBoardReason reason)
        {
            if (card.hasID)
                yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            foreach (var a in base.OnTryToRemoveFromBoard(card, context, reason)) yield return a;
        }

        public override IEnumerable<IActionData> OnHPReducedToZero(IGameOrSimEntity card, HurtType hurtType, IGameOrSimContext context)
        {
            if (card.hasID)
                yield return new RemoveGlobalBuffsFromSource(card.iD.value);
            foreach (var a in base.OnHPReducedToZero(card, hurtType, context)) yield return a;
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ApplyGlobalBuff(new PreventHeroHealingBuff(card.iD.value));
        }
    }


    public abstract class MovingWallBehavior : BasicPropCardBehavior
    {
        protected abstract BoardDirection BoardDirection { get; }

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]:\n[b]push for 3[/b]";

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Push;
            yield return KeywordID.Timer;
        }

        private int ChannelInit => 3;

        public override IEnumerable<IActionData> OnEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.isCardFlipped)
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

                    yield return new PushAction(card.iD.value, 3, BoardDirection);

                    if (card.hasID)
                        yield return new ModifyChannelingAction(card.iD.value, ChannelInit, true);
                }
            }
        }

        public override IEnumerable<IActionData> OnBeforeDestroy(IGameOrSimContext gameOrSimContext)
        {
            yield return new TriggerSoundAction("rockDestroy");
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            e.AddHealth(9, 9);

            e.AddChannelling(ChannelInit);
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
    }

    public class MovingWallDownBehavior : MovingWallBehavior
    {
        protected override BoardDirection BoardDirection => BoardDirection.South;
    }
    public class MovingWallUpBehavior : MovingWallBehavior
    {
        protected override BoardDirection BoardDirection => BoardDirection.North;
    }
    public class MovingWallLeftBehavior : MovingWallBehavior
    {
        protected override BoardDirection BoardDirection => BoardDirection.West;
    }
    public class MovingWallRightBehavior : MovingWallBehavior
    {
        protected override BoardDirection BoardDirection => BoardDirection.East;
    }
}
