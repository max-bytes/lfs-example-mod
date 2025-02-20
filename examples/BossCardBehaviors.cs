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
    public class OrokCardBehavior : BasicBossCardBehavior
    {
        public OrokCardBehavior() : base(18, 4, 0)
        {
        }

        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.BossImmunity;
        }

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "You won't\ndefeat me!" }, { "ttl", 3.0f } }));
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "You... did it..." }, { "ttl", 3.0f } }));
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Come back!" }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);
            }
        }

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            // increase attack, replacing attack value (not final attack value)
            var bossStrength = DifficultyUtils.GetBossStrengthIncrease(context);
            var attack = ModifyBossAttackBasedOnDifficulty(BaseAttack, bossStrength) + card.bossPhase.phase;
            card.ReplaceAttackValue(attack, card.attackValue.attackType, card.attackValue.quickAttack, card.attackValue.slowAttack);
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("upgrade"));
            yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Again!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(120);
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("lackey"), 1),
                Enumerable.Repeat(new CardDataID("sentry"), 2),
                Enumerable.Repeat(new CardDataID("archer"), 2),
            }.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;

        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base;
    }

    public class BansheeCardBehavior : BasicBossCardBehavior
    {
        public override AttackType AttackType => AttackType.Omni;
        public override ProjectileType ProjectileType => ProjectileType.WizardBall;
        public override MovementType MovementType => MovementType.None;
        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;

        public BansheeCardBehavior() : base(24, 2)
        {
        }

        private int ChannelInit(IGameOrSimEntity card, IGameOrSimContext context) => Math.Max(1, 4 - LoopingUtils.LoopIndex(context));

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: attacks, then [b]swaps place[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.BossImmunity;
        }

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "This is\nthe end!" }, { "ttl", 3.0f } }));
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "The end...\nafter all." }, { "ttl", 3.0f } }));
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Come back!" }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
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
                if (CombatService.CanHeroBeTargetedByMonster(hero))
                    yield return new EnemyAttackAction(CalculateFinalAttackValue(card), card.iD.value, hero.iD.value, AttackType, ProjectileType);
                else
                    yield return new FizzleAction(card.iD.value);

                if (CombatService.CanHeroBeTargetedByMonster(hero) && card.hasBoardLocation && hero.hasBoardLocation && card.hasID && hero.hasID)
                    yield return new ParallelAction(
                        new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Swap),
                        new MoveCardOnBoardAction(card.iD.value, hero.boardLocation.location, MoveReason.Swap)
                    );
                if (card.hasID)
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(e, context));
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            // increase attack, replacing attack value (not final attack value)
            var bossStrengthFromDifficulty = DifficultyUtils.GetBossStrengthIncrease(context);

            var finalAttack = ModifyBossAttackBasedOnLoop(ModifyBossAttackBasedOnDifficulty(BaseAttack, bossStrengthFromDifficulty) + card.bossPhase.phase, LoopingUtils.LoopIndex(context));
            card.ReplaceAttackValue(finalAttack, card.attackValue.attackType, card.attackValue.quickAttack, card.attackValue.slowAttack);
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("upgrade"));
            yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Again!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(120);
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("armoredOrc"), 3),
                Enumerable.Repeat(new CardDataID("brute"), 1),
                Enumerable.Repeat(new CardDataID("archer"), 3),
                Enumerable.Repeat(new CardDataID("shaman"), 1),
            }.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;

        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 5;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 10;
    }


    public class StyxActionInterceptor : IActionInterceptor
    {
        public readonly EntityID cardID;

        public StyxActionInterceptor(EntityID cardID)
        {
            this.cardID = cardID;
        }

        public bool Prefilter(IActionData a, IGameOrSimEntity self, ActionExecutionContext context) => a is AfterCardDestroyedPlaceholderAction;
        public IEnumerable<IActionData> OnEndAction(IActionData a, IGameOrSimEntity self, ActionExecutionContext context)
        {
            if (a is AfterCardDestroyedPlaceholderAction dca)
            {
                if (!dca.LastBoardLocation.HasValue)
                    yield break;
                if (!self.hasCardArea || self.cardArea.Area != CardArea.EnemyDrawPile)
                    yield break;
                if (dca.cardType != CardType.Monster) 
                    yield break;

                if (context.GameOrSimContext._GetCardsWithBoardLocation(dca.LastBoardLocation.Value).Any())
                    yield break;

                yield return new MoveCardToBoardAction(self.iD.value, dca.LastBoardLocation.Value, false);
            }
        }
    }

    public class StyxBehavior : BasicBossCardBehavior
    {
        public override AttackType AttackType => AttackType.CloseRange;
        public override MovementType MovementType => MovementType.None;
        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;

        public StyxBehavior() : base(27, 3)
        {
        }

        private int ChannelInit(IGameOrSimEntity card, IGameOrSimContext context) => Math.Max(1, 6 - card.bossPhase.phase - DifficultyUtils.GetBossStrengthIncrease(context) - LoopingUtils.LoopIndex(context));

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: replace with [b]Zombie[/b], return when Monster is defeated";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.FourNeighbor;
            yield return KeywordID.BossImmunity;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("zombie");
        }

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Rise my\nchildren!" }, { "ttl", 3.0f } }));
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "My return...\nto soil." }, { "ttl", 3.0f } }));
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Come back!" }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, 0, true);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("cast"));
                yield return new TriggerSoundAction("cast");

                var boardLocation = card.boardLocation.location;
                yield return new MoveCardToDrawPileAction(card.iD.value, true, 20);
                var createdCardID = LoopingUtils.ModifyMonsterCard(new CardDataID("zombie"), LoopingUtils.LoopIndex(context), hero.card.data.BaseID, context.GetRandomSeedForIngameRoom());
                yield return new CreateCardAtBoardLocationAction(createdCardID, boardLocation);
            }

            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(e, context));
            e.AddActionInterceptorSource(new IActionInterceptor[] { new StyxActionInterceptor(e.iD.value) });
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Again!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(120);
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("ghoul"), 5),
                Enumerable.Repeat(new CardDataID("stalker"), 2),
            }.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;

        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 5;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 10;
    }

    public class RahotepBehavior : BasicBossCardBehavior
    {
        public override AttackType AttackType => AttackType.Diagonal;
        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;
        private int Attack(int loopIndex) => 1 + loopIndex * 2;
        private int Heal(int loopIndex) => 3 + loopIndex * 3;

        public RahotepBehavior() : base(27, 2)
        {
        }

        private int ChannelInit(IGameOrSimEntity card, IGameOrSimContext context) => Math.Max(1, 6 - card.bossPhase.phase - DifficultyUtils.GetBossStrengthIncrease(context) - LoopingUtils.LoopIndex(context));

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: others [b]flip[/b], get +{attack} Attack and [b]heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("attack", Attack(loopIndex)); yield return ("heal", Heal(loopIndex)); }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.DiagonalAttack;
            yield return KeywordID.Heal;
            yield return KeywordID.BossImmunity;
        }

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Horus guide me!" }, { "ttl", 3.0f } }));
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "May Osiris judge me..." }, { "ttl", 3.0f } }));
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Come back!" }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, 0, true);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("cast"));
                yield return new TriggerSoundAction("cast");

                var enemies = context._GetCardsWithCardArea(CardArea.Board).Where(c => c.hasBoardLocation && c.hasHealth && c.isEnemy && c.iD.value != card.iD.value).ToList();

                if (enemies.Count <= 0)
                {
                    yield return new FizzleAction(card.iD.value);
                }
                else
                {
                    foreach (var e in enemies)
                    {
                        yield return new SpawnProjectileAction(ProjectileType.EnemyBuff, ScreenPositionUtils.CalculateBoardPositionScreen(card.boardLocation.location), ScreenPositionUtils.CalculateBoardPositionScreen(e.boardLocation.location));

                        if (!e.isCardFlipped)
                            yield return new FlipCardAction(e.iD.value, true);

                        yield return new IncreaseAttackAction(e.iD.value, Attack(LoopingUtils.LoopIndex(context)), false);
                        yield return new HealTargetAction(e.iD.value, Heal(LoopingUtils.LoopIndex(context)), true);
                        yield return new DelayAction(10);
                    }
                }
                yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
            }

            if (card.hasBoardLocation && BoardUtils.Are8Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(e, context));
            e.AddActionInterceptorSource(new IActionInterceptor[] { new StyxActionInterceptor(e.iD.value) });
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Again!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(120);
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("medjay"), 2),
                Enumerable.Repeat(new CardDataID("zealot"), 2),
                Enumerable.Repeat(new CardDataID("protector"), 1),
                Enumerable.Repeat(new CardDataID("pitati"), 2),
            }.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 5;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 10;
    }



    public class CoelescusCardBehavior : BasicBossCardBehavior // TODO
    {
        public override AttackType AttackType => AttackType.Diagonal;

        public CoelescusCardBehavior() : base(27, 3, 0)
        {
        }

        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: place [b]{tacticalRegroup}[/b] at Hero's 4-neighbor spots";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("tacticalRegroup", CardNames.Generate(new CardDataID("tacticalRegroup"))); }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new CardDataID("tacticalRegroup");
        }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.DiagonalAttack;
            yield return KeywordID.BossImmunity;
            yield return KeywordID.FourNeighbor;
        }

        private int ChannelInit(IGameOrSimEntity card, IGameOrSimContext context) => Math.Max(1, 7 - card.bossPhase.phase - DifficultyUtils.GetBossStrengthIncrease(context) - LoopingUtils.LoopIndex(context));

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Time will\ntell..." }, { "ttl", 3.0f } }));
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "My time...\nhas come..." }, { "ttl", 3.0f } }));
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Inevitable..." }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Again!" }, { "ttl", 3.0f } }));
            yield return new DelayAction(120);
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            var currentChannelling = (card.hasChannelling) ? card.channelling.value : 0;
            if (currentChannelling > 1)
            {
                yield return new ModifyChannelingAction(card.iD.value, -1, false);
                yield return new DelayAction(15);
            }
            else
            {
                yield return new ModifyChannelingAction(card.iD.value, 0, true);
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("cast"));
                yield return new TriggerSoundAction("cast");

                // create tacticalRegroup
                yield return new SequenceAction(
                    BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                        .Where(l => BoardUtils.Are4Neighbors(l, hero.boardLocation.location))
                        .Where(l => !context._GetCardsWithBoardLocation(l).Any())
                        .Select(l => new SequenceAction(
                            new TriggerSoundAction("cardCreateOnBoard"),
                            new CreateCardAtBoardLocationAction(new CardDataID("tacticalRegroup"), l),
                            new DelayAction(10)
                        ))
                );
                yield return new DelayAction(30);

                yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
            }

            if (card.hasBoardLocation && BoardUtils.Are8Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(e, context));
            e.AddActionInterceptorSource(new IActionInterceptor[] { new StyxActionInterceptor(e.iD.value) });
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("medjay"), 2),
                Enumerable.Repeat(new CardDataID("zealot"), 2),
                Enumerable.Repeat(new CardDataID("protector"), 1),
                Enumerable.Repeat(new CardDataID("pitati"), 2),
            }.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 5;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 10;
    }

    public abstract class AvoozlBaseBehavior : BasicBossCardBehavior
    {
        protected readonly string nextAvoozl;

        public AvoozlBaseBehavior(int baseHealth, int baseAttack, int baseDefense, string nextAvoozl) : base(baseHealth, baseAttack, baseDefense)
        {
            this.nextAvoozl = nextAvoozl;
        }

        public static int NumPhasesStatic => 3;
        public override int NumPhases => NumPhasesStatic;
        public override MovementType MovementType => MovementType.None;

        protected IEnumerable<IActionData> TransformInto(string replaceWith, IGameOrSimEntity card, IGameOrSimContext context, Func<IGameOrSimEntity, IEnumerable<IActionData>> afterF)
        {
            var boardLocation = card.boardLocation.location;
            var bossPhase = card.bossPhase.phase;
            var oldBossID = card.iD.value;
            yield return new TriggerSoundAction("stealth");
            yield return new TriggerAnimationAction(context._GetBoardLocationEntity(boardLocation).iD.value, new AnimationTrigger("stealth"));
            yield return new DelayAction(10);
            yield return new CreateCardAtBoardLocationAction(new CardDataID(replaceWith), boardLocation, true, true);
            yield return new DestroyCardAction(card.iD.value, DestroyCardReason.Other, true, false, true);
            var newBoss = context._GetEnemyCardsOnBoard(false).Where(c => c.isBoss && c.iD.value != oldBossID).FirstOrDefault();
            if (newBoss != null)
            {
                newBoss.ReplaceBossPhase(bossPhase);
                foreach (var a in afterF(newBoss))
                    yield return a;
            }
        }

        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.BossImmunity;
        }

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        protected override IEnumerable<IActionData> OnDefeated(IGameOrSimEntity card, IGameOrSimContext context)
        {
            var bossHP = card.health.value;
            IEnumerable<IActionData> AfterTransform(IGameOrSimEntity newBoss)
            {
                newBoss.ReplaceHealth(bossHP, newBoss.health.value);
                yield return new DelayAction(60);
                yield return new TriggerAnimationAction(newBoss.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Æ¡ÆÆ¡Æ¡¡Æ¡\nÆÆÆ¡ÆÆÆ¡Æ¡\nÆÆÆ¡ÆÆÆÆ¡¡Æ¡ÆÆ¡" }, { "ttl", 3.0f } }));
                yield return new DelayAction(120);
            }
            foreach (var a in TransformInto("avoozlIntro", card, context, AfterTransform))
                yield return a;
        }

        public override IEnumerable<IActionData> OnHeroRetreat(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "þþ" }, { "ttl", 2.0f } }));
        }

        protected override IEnumerable<IActionData> ResetBossStats(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new DelayAction(60);
        }

        protected override IEnumerable<IActionData> OnAdvancedPhase(IGameOrSimEntity card, IGameOrSimContext context)
        {
            static IEnumerable<IActionData> AfterTransform(IGameOrSimEntity newBoss)
            {
                yield return new DelayAction(60);
                yield return new TriggerAnimationAction(newBoss.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "ÆþðÐ¡" }, { "ttl", 3.0f } }));
                yield return new DelayAction(120);
            }
            if (nextAvoozl != null)
                foreach (var a in TransformInto(nextAvoozl, card, context, AfterTransform))
                    yield return a;
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {
                Enumerable.Repeat(new CardDataID("guardian"), 2),
                Enumerable.Repeat(new CardDataID("trenchRat"), 2),
                Enumerable.Repeat(new CardDataID("voidPriest"), 1),
                Enumerable.Repeat(new CardDataID("ranger"), 1),
                Enumerable.Repeat(new CardDataID("medusa"), 2),
            }.SelectMany(e => e);
        }
    }

    public class AvoozlIntroCardBehavior : AvoozlBaseBehavior
    {
        public AvoozlIntroCardBehavior() : base(36, 0, 0, "avoozlA")
        {
        }

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "";

        protected override IEnumerable<IActionData> OnGreetHero(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("speechbubble", new Dictionary<string, object>() { { "text", "Ðþ¡Æð" }, { "ttl", 3.0f } }));
            yield return new DelayAction(200);

            // replace with next
            foreach (var a in TransformInto(nextAvoozl, card, context, (newBoss) => { return Enumerable.Empty<IActionData>(); }))
                yield return a;
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield break;
        }

        protected override IEnumerable<CardDataID> AddedMonstersForPhase(int targetPhase, IGameOrSimContext context)
        {
            return new List<IEnumerable<CardDataID>>() {}.SelectMany(e => e);
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base;
    }

    public class AvoozlACardBehavior : AvoozlBaseBehavior
    {
        public AvoozlACardBehavior() : base(36, 7, 7, "avoozlB")
        {
        }
        private int Defense(int loopIndex) => 3 + loopIndex * 4;

        public override MovementType MovementType => MovementType.WalkOnlyStraightTowardsHero4Neighbor;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Attack[/b]: +{defense} Defense";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("defense", Defense(loopIndex)); }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                yield return new ModifyDefenseValueModifierAction(card.iD.value, Defense(LoopingUtils.LoopIndex(context)));
                yield return new TriggerSoundAction(new SoundTrigger("enemyUpgrade"));
                yield return new TriggerAnimationAction(card.iD.value, new AnimationTrigger("upgrade"));

                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);

            }

            foreach (var a in CombatService.MoveAccordingToTelegraphed(card, context))
                yield return a;
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 10;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 20;
    }

    public class AvoozlBCardBehavior : AvoozlBaseBehavior
    {
        public AvoozlBCardBehavior() : base(36, 7, 7, "avoozlC")
        {
        }

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Teleports next to Hero if on same row or column";

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

            if (card.hasBoardLocation && BoardUtils.Are4Neighbors(card.boardLocation.location, hero.boardLocation.location) && CombatService.CanHeroBeTargetedByMonster(hero))
            {
                var attackValue = CalculateFinalAttackValue(card);
                yield return new EnemyAttackAction(attackValue, card.iD.value, hero.iD.value, AttackType, ProjectileType);
            }
        }

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 10;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 20;
    }

    public class AvoozlCCardBehavior : AvoozlBaseBehavior
    {
        public AvoozlCCardBehavior() : base(36, 7, 7, null)
        {
        }

        private int ChannelInit(IGameOrSimEntity card, IGameOrSimContext context) => Math.Max(1, 4 - DifficultyUtils.GetBossStrengthIncrease(context) - LoopingUtils.LoopIndex(context));

        public override AttackType AttackType => AttackType.Omni;
        public override ProjectileType ProjectileType => ProjectileType.WizardBall;
        public override MovementType MovementType => MovementType.None;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Timer[/b]: attacks, then [b]swaps place[/b]";
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Timer;
            yield return KeywordID.BossImmunity;
        }

        protected override IEnumerable<IActionData> OnSleepingEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true); // when sleeping, reset channeling progress
        }

        protected override IEnumerable<IActionData> OnActiveEnemyTurn(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
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
                if (CombatService.CanHeroBeTargetedByMonster(hero))
                    yield return new EnemyAttackAction(CalculateFinalAttackValue(card), card.iD.value, hero.iD.value, AttackType, ProjectileType);
                else
                    yield return new FizzleAction(card.iD.value);

                if (CombatService.CanHeroBeTargetedByMonster(hero) && card.hasBoardLocation && hero.hasBoardLocation && card.hasID && hero.hasID)
                    yield return new ParallelAction(
                        new MoveCardOnBoardAction(hero.iD.value, card.boardLocation.location, MoveReason.Swap),
                        new MoveCardOnBoardAction(card.iD.value, hero.boardLocation.location, MoveReason.Swap)
                    );
                if (card.hasID)
                    yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
            }
        }

        public override IEnumerable<IActionData> OnAfterFlip(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override IEnumerable<IActionData> OnAfterMovedToPile(IGameOrSimEntity hero, IGameOrSimEntity card, IGameOrSimContext context)
        {
            foreach (var a in base.OnAfterMovedToPile(hero, card, context))
                yield return a;
            yield return new ModifyChannelingAction(card.iD.value, ChannelInit(card, context), true);
        }

        public override void OnCreate(IGameOrSimEntity e, IGameOrSimContext context)
        {
            base.OnCreate(e, context);

            e.AddChannelling(ChannelInit(e, context));
        }

        public override bool RequiresAttention(IGameOrSimEntity card) => card.hasChannelling && card.channelling.value == 1;

        protected override int ModifyBossAttackBasedOnDifficulty(int @base, int bossStrength) => @base + 5 * bossStrength;
        protected override int ModifyBossDefenseBasedOnDifficulty(int @base, int bossStrength) => @base + 10 * bossStrength;
        protected override int ModifyBossAttackBasedOnLoop(int @base, int loop) => @base + loop * 10;
        protected override int ModifyBossDefenseBasedOnLoop(int @base, int loop) => @base + loop * 20;
    }
}
