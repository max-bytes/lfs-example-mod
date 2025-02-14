using Action;
using Base.Board;
using Base.Card;
using CardStuff.Action;
using CardStuff.Service;
using CardStuff.Utils;
using Core.Action;
using Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{

    public class LargeHPPotionBehavior : BasicPotionCardBehavior
    {
        public override bool IsCantripActive() => false;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
        }
        public override IEnumerable<CardTag> ProvidesTags() { yield return CardTag.Potion; yield return CardTag.Heal; }

        public override int MaxUpgrade => 0;

        private int Heal => 6;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new HealTargetAction(hero.iD.value, Heal, true);
        }
    }

    public class ConstitutionPotionBehavior : BasicPotionCardBehavior
    {
        public override bool IsCantripActive() => false;

        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Raise Max HP by {raiseHP}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("raiseHP", Raise); }

        public override int MaxUpgrade => 0;

        private int Raise => 1;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new IncreaseMaxHealthTargetAction(hero.iD.value, Raise);
        }
    }

    public class OilOfSharpnessBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Topmost Weapon gets +{additionalAttack} Attack";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("additionalAttack", AdditionalAttack(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Replaceable;
        }

        private int AdditionalAttack(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            var w = CombatService.GetTopHeroWeapon(context, false);
            if (w != null)
            {
                yield return new IncreaseAttackAction(w.iD.value, AdditionalAttack(cardID.QualityModifier), true);
            }
        }
    }

    public class PotionOfPoisonBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply [b]{poison} Poison[/b] to all Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("poison", Poison(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Poison;
            yield return KeywordID.Replaceable;
        }

        private int Poison(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                        e => new ApplyPoisonAction(e.iD.value, Poison(cardID.QualityModifier)),
                        e => new SequenceAction(
                            new TriggerSoundAction("poison"),
                            new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("poison")),
                            new DelayAction(10)
                        )
                    )));
        }
    }

    public class PotionOfFireBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Deal [b]{damage} damage[/b] to all Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("damage", Damage(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.FireDamage;
            yield return KeywordID.Replaceable;
        }
        private int Damage(int quality) => 1 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                .Select(l => new AffectCardAndLocationAction(l, false, (e) => e.hasHealth && e.isEnemy,
                    e => new HurtTargetAction(e.iD.value, Damage(cardID.QualityModifier), HurtSource.Potion, HurtType.Fire),
                    e => new SequenceAction(
                        new TriggerSoundAction("fire"),
                        new TriggerAnimationAction(context._GetBoardLocationEntity(l).iD.value, new AnimationTrigger("burn")),
                        new DelayAction(5)
                    )
                )));
        }
    }

    public class StealthPotionBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Gain [b]{stealth} Stealth[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("stealth", Stealth(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Stealth;
            yield return KeywordID.Replaceable;
        }
        private int Stealth(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new ModifyStealthAction(hero.iD.value, Stealth(cardID.QualityModifier), false);
            yield return new TriggerSoundAction("stealth");
            yield return new TriggerAnimationAction(context._GetBoardLocationEntity(hero.boardLocation.location).iD.value, new AnimationTrigger("stealth"));
            yield return new DelayAction(10);
        }
    }

    public class PotionOfSleepBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "Apply [b]{sleep} Sleep[/b] to all Monsters";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("sleep", Sleep(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Sleep;
            yield return KeywordID.Replaceable;
        }

        private int Sleep(int quality) => 2 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new SequenceAction(BoardUtils.GetAll().OrderBy(l => BoardUtils.GetSortIndex(l))
                    .Select(l => new AffectCardAndLocationAction(l, true, (e) => e.hasHealth && e.isEnemy,
                        e => new SequenceAction(new TrySleepAction(e.iD.value, Sleep(cardID.QualityModifier)), new DelayAction(10)),
                        e => NoopAction.Instance
                    )));
        }
    }

    public class SmallHPPotionBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]Heal {heal}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("heal", Heal(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Heal;
            yield return KeywordID.Replaceable;
        }

        private int Heal(int quality) => 1 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new HealTargetAction(hero.iD.value, Heal(cardID.QualityModifier), true);
        }
    }

    public class ManaPotionBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "[b]equip[/b] [b]{manaOrb}[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("manaOrb", CardNames.Generate(ManaOrb(quality))); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.Equip;
            yield return KeywordID.Replaceable;
        }
        public override IEnumerable<CardDataID> GetRelatedCards(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return ManaOrb(card.card.data.QualityModifier);
        }

        private CardDataID ManaOrb(int quality) => new CardDataID("manaOrb", quality);

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new CreateCardOnHeroAction(ManaOrb(cardID.QualityModifier));
        }
    }

    public class BlinkPotionBehavior : BasicPotionCardBehavior
    {
        public override string GenerateBaseDescriptionEN(int quality, bool isEthereal) => "gain [b]{num} Teleport Charge[/b]";
        public override IEnumerable<(string, object)> GenerateStaticDescriptionPlaceholders(int quality, int loopIndex) { yield return ("num", Num(quality)); }
        public override IEnumerable<KeywordID> GenerateKeywords(IGameOrSimEntity card, IGameOrSimContext context)
        {
            yield return KeywordID.TeleportCharge;
            yield return KeywordID.Replaceable;
        }

        private int Num(int quality) => 1 + quality;

        public override IEnumerable<IActionData> OnConsume(IGameOrSimEntity hero, CardDataID cardID, IGameOrSimContext context)
        {
            yield return new ModifyTeleportCounterAction(hero.iD.value, Num(cardID.QualityModifier), false);
        }
    }
}

