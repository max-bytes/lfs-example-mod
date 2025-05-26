using Base.Board;
using Base.Card;
using CardStuff.Utils;
using Core.Card;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardStuff.Card
{
    public static class Registrator
    {
        public static void RegisterStartingDecks()
        {
            var sd = new StartingDeck[]
            {
                new StartingDeck("default", "knight", "The Rust Bucket", false, new List<IEnumerable<CardDataID>>() {
                    Enumerable.Repeat(new CardDataID("knife"), 1),
                    Enumerable.Repeat(new CardDataID("rustySword", 0), 4),
                    Enumerable.Repeat(new CardDataID("brokenShield", 0), 4),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("shieldMaiden", "knight", "The Shield Maiden", true, new List<IEnumerable<CardDataID>>() {
                    Enumerable.Repeat(new CardDataID("knife"), 1),
                    Enumerable.Repeat(new CardDataID("shieldSlam", 0), 1),
                    Enumerable.Repeat(new CardDataID("woodenShield", 0), 1),
                    Enumerable.Repeat(new CardDataID("gauntlet", 0), 1),
                    Enumerable.Repeat(new CardDataID("brokenShield", 0), 4),
                    Enumerable.Repeat(new CardDataID("wound", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("minimalist", "knight", "The Minimalist", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("knife", 0), 1),
                        Enumerable.Repeat(new CardDataID("clericsShield", 0), 1),
                        Enumerable.Repeat(new CardDataID("heavyHand", 0), 1),
                        Enumerable.Repeat(new CardDataID("adamophobia", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("gourmand", "knight", "The Gourmand", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("hungrySword", 0), 3),
                        Enumerable.Repeat(new CardDataID("heavyHand", 0), 2),
                        Enumerable.Repeat(new CardDataID("wornBreastplate", 0), 1),
                        Enumerable.Repeat(new CardDataID("greaterHeal", 0), 1),
                        Enumerable.Repeat(new CardDataID("incinerate", 0), 1),
                        Enumerable.Repeat(new CardDataID("fatigue", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("historian", "knight", "The Historian", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("centurionsHelmet", 0), 1),
                        Enumerable.Repeat(new CardDataID("eirenesCrown", 0), 1),
                        Enumerable.Repeat(new CardDataID("achillesArmor", 0), 1),
                        Enumerable.Repeat(new CardDataID("wingsOfHypnos", 0), 1),
                        Enumerable.Repeat(new CardDataID("knife", 0), 1),
                        Enumerable.Repeat(new CardDataID("godosHammer", 0), 1),
                        Enumerable.Repeat(new CardDataID("hopliteSpear", 0), 1),
                        Enumerable.Repeat(new CardDataID("muradsMace", 0), 1),
                        Enumerable.Repeat(new CardDataID("originalSin", 0), 1),
                }.SelectMany(e => e).ToArray()),

                new StartingDeck("default", "rogue", "What Rogues Like", false, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("makeshiftDagger"), 3),
                        Enumerable.Repeat(new CardDataID("shuriken", 0), 3),
                        Enumerable.Repeat(new CardDataID("whip", 0), 1),
                        Enumerable.Repeat(new CardDataID("leatherCap", 0), 2),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("courtJester", "rogue", "The Court Jester", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("pushingDagger"), 3),
                        Enumerable.Repeat(new CardDataID("poorJestersTop", 0), 2),
                        Enumerable.Repeat(new CardDataID("whip", 0), 1),
                        Enumerable.Repeat(new CardDataID("jestersBottoms", 0), 1),
                        Enumerable.Repeat(new CardDataID("hornedHood", 0), 1),
                        Enumerable.Repeat(new CardDataID("blasphemy", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("thief", "rogue", "The Thief", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("whip", 0), 1),
                        Enumerable.Repeat(new CardDataID("hiddenBlade", 0), 1),
                        Enumerable.Repeat(new CardDataID("repossession", 0), 1),
                        Enumerable.Repeat(new CardDataID("foresight", 0), 1),
                        Enumerable.Repeat(new CardDataID("quartermastersBelt", 0), 1),
                        Enumerable.Repeat(new CardDataID("pushInBoots", 0), 1),
                        Enumerable.Repeat(new CardDataID("claustrophobia", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("joker", "rogue", "The Joker", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("whip", 0), 1),
                        Enumerable.Repeat(new CardDataID("boosterPack", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("fencer", "rogue", "The Fencer", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("whip", 0), 1),
                        Enumerable.Repeat(new CardDataID("duelingRapier", 0), 1),
                        Enumerable.Repeat(new CardDataID("concealedRapier", 0), 1),
                        Enumerable.Repeat(new CardDataID("fencingRapier", 0), 1),
                        Enumerable.Repeat(new CardDataID("cunningRapier", 0), 1),
                        Enumerable.Repeat(new CardDataID("buckler", 0), 1),
                        Enumerable.Repeat(new CardDataID("stealthCloak", 0), 1),
                        Enumerable.Repeat(new CardDataID("pushInBoots", 0), 1),
                        Enumerable.Repeat(new CardDataID("wound", 0), 1),
                }.SelectMany(e => e).ToArray()),

                new StartingDeck("default", "mage", "Scholar Of Magic", false, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("magicMissile"), 3),
                        Enumerable.Repeat(new CardDataID("imbuedStaff"), 2),
                        Enumerable.Repeat(new CardDataID("apprenticeRobe"), 3),
                        Enumerable.Repeat(new CardDataID("noviceWand", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("volatileSorcerer", "mage", "The Volatile Sorcerer", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("chainLightning"), 2),
                        Enumerable.Repeat(new CardDataID("lightningStrike"), 1),
                        Enumerable.Repeat(new CardDataID("owlVision"), 1),
                        Enumerable.Repeat(new CardDataID("apothecaryList"), 1),
                        Enumerable.Repeat(new CardDataID("vandalsCape"), 2),
                        Enumerable.Repeat(new CardDataID("noviceWand", 0), 1),
                        Enumerable.Repeat(new CardDataID("blasphemy", 0), 1),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("aristocrat", "mage", "The Aristocrat", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("noviceWand", 0), 1),
                        Enumerable.Repeat(new CardDataID("sorcerersGloves"), 2),
                        Enumerable.Repeat(new CardDataID("wandOfMammon"), 2),
                        Enumerable.Repeat(new CardDataID("taxCollectorsBoots"), 2),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("theorist", "mage", "The Theorist", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("thinkersHat"), 2),
                        Enumerable.Repeat(new CardDataID("meticulousStudy"), 1),
                        Enumerable.Repeat(new CardDataID("preparation"), 1),
                        Enumerable.Repeat(new CardDataID("ringOfFire"), 2),
                        Enumerable.Repeat(new CardDataID("blasphemy"), 2),
                }.SelectMany(e => e).ToArray()),
                new StartingDeck("illusionist", "mage", "The Illusionist", true, new List<IEnumerable<CardDataID>>() {
                        Enumerable.Repeat(new CardDataID("noviceWand", 0), 1),
                        Enumerable.Repeat(new CardDataID("strangeRobe", 0), 1),
                        Enumerable.Repeat(new CardDataID("attunedRobe", 0), 1),
                        Enumerable.Repeat(new CardDataID("arcaneBarrier", 0), 1),
                        Enumerable.Repeat(new CardDataID("barrelmancy", 0), 1),
                        Enumerable.Repeat(new CardDataID("flameDance", 0), 1),
                        Enumerable.Repeat(new CardDataID("rearrangement", 0), 1),
                        Enumerable.Repeat(new CardDataID("wandOfRehearsal", 0), 1),
                        Enumerable.Repeat(new CardDataID("futility", 0), 1),
                }.SelectMany(e => e).ToArray()),
            };
            foreach (var s in sd)
                StartingDeckBuilder.AddDeck(s);
        }

        public static void RegisterCards()
        {
            // considerations for drop rate picking:
            // * complexity of card
            // * can card be used on its own
            var baseDefinitions = new CardDefinition[]
            {
                // armor
                new CardDefinition(new CardDataID("helmet"), "Helmet", new HelmetCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("capOfHealing"), "Robin's Hat", new RobinsHatCardBehavior()).SetDropRate().Rogue(),

                new CardDefinition(new CardDataID("centurionsHelmet"), "Centurion's Helmet", new CenturionsHelmetBehavior()).Knight().SetDropRate(2),
                new CardDefinition(new CardDataID("robeOfRiches"), "Robe Of Riches", new RobeOfRichesBehavior()).Mage().SetDropRate(1),
                new CardDefinition(new CardDataID("boots"), "Boots", new BootsCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("pushInBoots"), "Push In Boots", new PushInBootsCardBehavior()).SetDropRate(1).Rogue(),

                new CardDefinition(new CardDataID("bodyArmor"), "Body Armor", new BodyArmorCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("pauldron"), "Pauldron", new PauldronArmorCardBehavior()).SetDropRate(1).Knight(),

                new CardDefinition(new CardDataID("brokenShield"), "Broken Shield", new BrokenShieldArmorCardBehavior()).Knight(),
                new CardDefinition(new CardDataID("wornBreastplate"), "Worn Breastplate", new WornBreastplateCardBehavior()).Knight(),

                new CardDefinition(new CardDataID("leatherCap"), "Leather Cap", new LeatherCapCardBehavior()).Rogue(),
                new CardDefinition(new CardDataID("woodenShield"), "Wooden Shield", new WoodenShieldCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("clericsShield"), "Cleric's Shield", new ClericsShieldCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("spikedShield"), "Spiked Shield", new SpikedShieldBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("gauntlet"), "Gauntlet", new GauntletCardBehavior()).SetDropRate(1).Knight().Rogue(),
                new CardDefinition(new CardDataID("heavyHand"), "Heavy Hand", new HeavyHandCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("spikedBodyArmor"), "Spiked Armor", new SpikedBodyArmorCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("poorJestersTop"), "Poor Jester's Top", new PoorJestersTopArmorCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("reactiveArmor"), "Reactive Armor", new ReactiveArmorCardBehavior()), // TODO: remove
                new CardDefinition(new CardDataID("plagueDoctorCape"), "Plague Doctor Cape", new PlagueDoctorCapeBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("majesticArmor"), "Majestic Armor", new MajesticArmorCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("brewmastersTunic"), "Brewmaster's Tunic", new BrewmastersTunicBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("achillesArmor"), "Achilles' Armor", new AchillesArmorBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("pocketlessCloak"), "Pocketless Cloak", new PocketlessCloakArmorCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("stealthCloak"), "Stealth Cloak", new StealthCloakBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("eirenesCrown"), "Eirene's Crown", new EirenesCrownArmorCardBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("bagoasBelt"), "Bagoas' Belt", new BagoasBeltArmorCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("soporificBelt"), "Soporific Belt", new SoporificBeltArmorCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("quartermastersBelt"), "Quartermaster's Belt", new QuartermastersBeltBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("geldkatze"), "Geldkatze", new GeldkatzeCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("jestersBottoms"), "Jester's Bottoms", new JestersBottomsBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("buckler"), "Buckler", new BucklerBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("hornedHood"), "Horned Hood", new HornedHoodCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("outriderHelmet"), "Outrider Helmet", new OutriderHelmetBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("vandalsCape"), "Vandal's Cape", new VandalsCapeBehavior()).SetDropRate(2).Rogue().Mage(),

                new CardDefinition(new CardDataID("wingsOfHypnos"), "Wings Of Hypnos", new WingsOfHypnosBehavior()).SetDropRate(2, dropInPaltry: false).Knight().Rogue().Mage(),

                new CardDefinition(new CardDataID("apprenticeRobe"), "Apprentice Robe", new ApprenticeRobeArmorCardBehavior()).Mage(),
                new CardDefinition(new CardDataID("attunedRobe"), "Attuned Robe", new AttunedRobeArmorCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("lightningRobe"), "Lightning Robe", new LightningRobeCardBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("taxCollectorsBoots"), "Tax Collector's Boots", new TaxCollectorsBootsCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("thinkersHat"), "Thinker's Hat", new ThinkersHatBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("manaVialBelt"), "Mana Vial Belt", new ManaVialBelt()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("strangeRobe"), "Strange Robe", new StrangeRobeBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("maticksRobe"), "Matick's Robe", new MaticksRobeBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("sorcerersGloves"), "Sorcerer's Glove", new SorcerersGloveBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("battleMageRobe"), "Battle Mage Robe", new BattleMageRobeBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("moonstoneCirclet"), "Moonstone Circlet", new MoonstoneCircletBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("spellcraftersGloves"), "Spellcrafter's Glove", new SpellcraftersGloveBehavior()).SetDropRate(1).Mage(),
                
                

                // weapons
                new CardDefinition(new CardDataID("knife"), "Knife", new KnifeCardBehavior()).Knight(),
                new CardDefinition(new CardDataID("oldKnife"), "Old Knife", new OldKnifeCardBehavior()).Knight(),
                new CardDefinition(new CardDataID("armingSword"), "Arming Sword", new ArmingSwordCardBehavior()).Knight().SetDropRate(),
                new CardDefinition(new CardDataID("highlander"), "Highlander", new HighlanderCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("vainglory"), "Vainglory", new VaingloryCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("muradsMace"), "Murad's Mace", new MuradsMaceCardBehavior()).SetDropRate(2).Knight(),
                //new CardDefinition(new CardDataID("greatsword"), "Greatsword", new GreatswordCardBehavior()).SetDropRate(), // TODO: remove/rework
                new CardDefinition(new CardDataID("hammer"), "Hammer", new HammerCardBehavior()).SetDropRate(dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("godosHammer"), "Godo's Hammer", new GodosHammerCardBehavior()).SetDropRate(1, dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("steadfastHammer"), "Steadfast Hammer", new SteadfastHammerCardBehavior()).SetDropRate(1, dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("solomonsHammer"), "Solomon's Hammer", new SolomonsHammerCardBehavior()).SetDropRate(1, dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("mjoelnir"), "Mjölnir", new MjoelnirCardBehavior()).SetDropRate(2, dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("battleHammer"), "Battle Hammer", new BattleHammerCardBehavior()).SetDropRate(1, dropInPaltry: false).Knight(),
                new CardDefinition(new CardDataID("lance"), "Spear", new SpearCardBehavior()).SetDropRate().Knight(), // TODO: rename lance to spear
                new CardDefinition(new CardDataID("bulwarkLance"), "Bulwark Spear", new BulwarkSpearCardBehavior()).SetDropRate(1).Knight(), // TODO: rename lance to spear
                new CardDefinition(new CardDataID("hopliteSpear"), "Hoplite Spear", new HopliteSpearCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("spearOfLonginus"), "Spear Of Longinus", new SpearOfLonginusCardBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("kunai"), "Kunai", new KunaiCardBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("sword"), "Sword", new SwordCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("mace"), "Mace", new MaceCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("zweihaender"), "Zweihänder", new ZweihaenderCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("sacrificialSword"), "Sacrifical Sword", new SacrificialSwordCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("morningstar"), "Morningstar", new MorningstarCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("rapier"), "Rapier", new RapierCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("fencingRapier"), "Fencing Rapier", new FencingRapierCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("cunningRapier"), "Cunning Rapier", new CunningRapierCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("concealedRapier"), "Concealed Rapier", new ConcealedRapierCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("weirdRod"), "Weird Rod", new WeirdRodCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("whip"), "Whip", new WhipCardBehavior()).Rogue(),
                new CardDefinition(new CardDataID("oldWhip"), "Old Whip", new OldWhipCardBehavior()).Rogue(),
                new CardDefinition(new CardDataID("shuriken"), "Shuriken", new ShurikenCardBehavior()).Rogue(),
                new CardDefinition(new CardDataID("infusedSword"), "Infused Sword", new InfusedSwordCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("infusedDagger"), "Infused Dagger", new InfusedDaggerCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("samsonsSword"), "Samson's Sword", new SamsonsSwordCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("hungrySword"), "Hungry Sword", new HungrySwordCardBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("hiddenBlade"), "Hidden Blade", new HiddenBladeBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("holyBlade"), "Holy Blade", new HolyBladeBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("duelingRapier"), "Dueling Rapier", new DuelingRapierCardBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("mimicStaff"), "Mimic Staff", new MimicStaffBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("masterShuriken"), "Master Shuriken", new MasterShurikenCardBehavior()).SetDropRate(2).Rogue(),


                new CardDefinition(new CardDataID("lostSword"), "Lost Sword", new LostSwordCardBehavior()),

                new CardDefinition(new CardDataID("rustySword"), "Rusty Sword", new RustySwordCardBehavior()).Knight(),
                new CardDefinition(new CardDataID("makeshiftDagger"), "Makeshift Dagger", new MakeshiftDaggerCardBehavior()).Rogue(),

                new CardDefinition(new CardDataID("quickDagger"), "Quick Dagger", new QuickDaggerCardBehavior()).SetDropRate().Rogue(), // TODO: change portrait
                new CardDefinition(new CardDataID("stealthDagger"), "Stealth Dagger", new StealthDaggerCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("sleepDagger"), "Sleep Dagger", new SleepDaggerCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("fearDagger"), "Fear Dagger", new FearDaggerCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("poisonedDagger"), "Poisoned Dagger", new PoisonedDaggerCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("pushingDagger"), "Pushing Dagger", new PushingDaggerCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("needleAndThreat"), "Needle And Threat", new NeedleAndThreatCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("nightmareBlade"), "Nightmare Blade", new NightmareBladeCardBehavior()).SetDropRate(1).Rogue(),

                new CardDefinition(new CardDataID("bow"), "Bow", new BowCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("compositeBow"), "Composite Bow", new CompositeBowCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("pushingBow"), "Pushing Bow", new PushingBowCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("powderKegBow"), "Powder Keg Bow", new PowderKegBowCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("poisonBow"), "Poison Bow", new PoisonBowCardBehavior()).SetDropRate(1).Rogue(),

                new CardDefinition(new CardDataID("noviceWand"), "Novice Wand", new NoviceWandCardBehavior()).Mage(),
                new CardDefinition(new CardDataID("imbuedStaff"), "Imbued Staff", new ImbuedStaffCardBehavior()).Mage(),
                new CardDefinition(new CardDataID("manaStaff"), "Mana Staff", new ManaStaffCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("lightningStaff"), "Lightning Staff", new LightningStaffCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("incineratorWand"), "Incinerator Wand", new IncineratorWandCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("flameWand"), "Flame Wand", new FlameWandCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("spellWand"), "Spell Wand", new SpellWandCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("wandOfRehearsal"), "Wand Of Rehearsal", new WandOfRehearsalCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("amenhotepsStaff"), "Amenhotep's Staff", new AmenhotepsStaffBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("focusStaff"), "Focus Staff", new FocusStaffCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("exertStaff"), "Exert Staff", new ExertStaffCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("blinkWand"), "Blink Wand", new BlinkWandCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("maticksWand"), "Matick's Wand", new MaticksWandBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("wandOfMammon"), "Wand Of Mammon", new WandOfMammonBehavior()).SetDropRate(1).Mage(),
                
                // quest items
                new CardDefinition(new CardDataID("ceremonialStaff"), "Ceremonial Staff", new CeremonialStaffCardBehavior()),
                new CardDefinition(new CardDataID("stolenLocket"), "Stolen Locket", new StolenLocketBehavior()),
                
                // scrolls
                new CardDefinition(new CardDataID("poisonCloud"), "Poison Cloud", new PoisonCloudScrollCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("foresight"), "Foresight", new ForesightScrollCardBehavior()).SetDropRate(1).Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("preparation"), "Preparation", new PreparationScrollCardBehavior()).SetDropRate(1).Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("reconsider"), "Reconsider", new ReconsiderScrollCardBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("greaterHeal"), "Greater Heal", new GreaterHealScrollCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("incinerate"), "Incinerate", new IncinerateScrollCardBehavior()).SetDropRate().Knight().Mage(),
                new CardDefinition(new CardDataID("wallOfIce"), "Wall Of Ice", new WallOfIceScrollCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("ringOfFire"), "Ring Of Fire", new RingOfFireScrollCardBehavior()).SetDropRate(1).Knight().Mage(),
                new CardDefinition(new CardDataID("fireColumn"), "Fire Column", new FireColumnScrollCardBehavior()).SetDropRate(1).Knight().Mage(),
                new CardDefinition(new CardDataID("forgedByFlames"), "Forged By Flames", new ForgedByFlamesScrollCardBehavior()).SetDropRate(1).Knight().Rogue(),
                new CardDefinition(new CardDataID("pairOfShuriken"), "Shuriken Dance", new ShurikenDanceBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("atonement"), "Atonement", new AtonementScrollCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("hammerAndAnvil"), "Hammer And Anvil", new HammerAndAnvilBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("shieldSlam"), "Shield Slam", new ShieldSlamScrollCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("bladeFlurry"), "Blade Flurry", new BladeFlurryScrollCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("repossession"), "Repossession", new RepossessionScrollCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("missileBarrage"), "Missile Barrage", new MissileBarrageCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("manaRain"), "Mana Rain", new ManaRainBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("apothecaryList"), "Apothecary List", new ApothecaryListBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("lightningStrike"), "Lightning Strike", new LightningStrikeBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("arbitrage"), "Arbitrage", new ArbitrageBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("doublecast"), "Doublecast", new DoubleCastBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("sampleTasting"), "Sample Tasting", new SampleTastingBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("shadowStrike"), "Shadow Strike", new ShadowStrikeCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("shroud"), "Shroud", new ShroudBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("improvisedTrap"), "Improvised Trap", new ImprovisedTrapBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("boosterPack"), "Booster Pack", new BoosterPackBehavior()).Rogue(),
                new CardDefinition(new CardDataID("divineFavor"), "Divine Favor", new DivineFavorCardBehavior()).Knight().SetDropRate(1),
                new CardDefinition(new CardDataID("fireAndIce"), "Fire And Ice", new FireAndIceBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("escalation"), "Escalation", new EscalationBehavior()).SetDropRate(2).Rogue(),
                new CardDefinition(new CardDataID("heavyReading"), "Heavy Reading", new HeavyReadingBehavior()).SetDropRate(2).Rogue(),
                

                // spells
                new CardDefinition(new CardDataID("magicMissile"), "Magic Missile", new MagicMissileBehavior()).Mage(),
                new CardDefinition(new CardDataID("flameWall"), "Flame Wall", new FlameWallBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("meteor"), "Meteor", new MeteorBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("conjureLightning"), "Conjure Lightning", new ConjureLightningBehavior()), // TODO: remove eventually
                new CardDefinition(new CardDataID("conjureFlames"), "Conjure Flames", new ConjureFlamesBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("chainLightning"), "Chain Lightning", new ChainLightningBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("owlVision"), "Owl Vision", new OwlVisionBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("rearrangement"), "Rearrangement", new RearrangementBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("flameDance"), "Fire Dance", new FlameDanceBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("meticulousStudy"), "Meticulous Study", new MeticulousStudyBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("barrelmancy"), "Barrelmancy", new BarrelmancyBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("callTheStorm"), "Call The Storm", new CallTheStormBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("arcaneBarrier"), "Arcane Barrier", new ArcaneBarrierBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("circuitCloser"), "Circuit Closer", new CircuitCloserBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("emeraldBolt"), "Emerald Bolt", new EmeraldBoltBehavior()).Mage().SetDropRate(1),
                

                // auras
                new CardDefinition(new CardDataID("toArms"), "To Arms!", new ToArmsAuraCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("shadowPresence"), "Shadow Presence", new ShadowPresenceBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("vicariousShame"), "Vicarious Shame", new VicariousShameBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("stronghold"), "Stronghold", new StrongholdAuraCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("hold"), "Hold!", new HoldAuraCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("poisonTheWell"), "Poison The Well", new PoisonTheWellCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("gracefulExit"), "Graceful Exit", new GracefulExitAuraCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("blessedHammer"), "Blessed Hammer", new BlessedHammerBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("breakMorale"), "Break Morale", new BreakMoraleAuraCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("forceMultiplier"), "Force Multiplier", new ForceMultiplierBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("soaringShields"), "Soaring Shields", new SoaringShieldsBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("tripwires"), "Tripwires", new TripwiresBehavior()), // TODO: remove
                new CardDefinition(new CardDataID("orionsBlessing"), "Orion's Blessing", new OrionsBlessingBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("salvagingArms"), "Salvaging Arms", new SalvagingArmsBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("nightmare"), "Nightmare", new NightmareAuraCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("burningSin"), "Common Penance", new CommonPenanceAuraCardBehavior()).SetDropRate(1).Knight(),
                new CardDefinition(new CardDataID("venomousShuriken"), "Venomous Shuriken", new VenomousShurikenAuraCardBehavior()).SetDropRate(1).Rogue(),
                new CardDefinition(new CardDataID("defiance"), "Defiance", new DefianceAuraCardBehavior()).SetDropRate(2).Knight(),
                new CardDefinition(new CardDataID("superconductor"), "Superconductor", new SuperconductorAuraCardBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("triboelectricity"), "Triboelectricity", new TriboElectricityBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("zap"), "Zap!", new ZapBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("sheerTenacity"), "Sheer Tenacity", new SheerTenacityCardBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("unstableEnergy"), "Unstable Energy", new UnstableEnergyBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("corrosiveSorcery"), "Corrosive Sorcery", new CorrosiveSorceryBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("nivsPresence"), "Niv's Presence", new NivsPresenceBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("renunciation"), "Renunciation", new RenunciationBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("stokeTheFlames"), "Stoke The Flames", new StokeTheFlamesBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("kindlingFlame"), "Kindling Flame", new KindlingFlameBehavior()).SetDropRate(1).Mage(),
                new CardDefinition(new CardDataID("holyFire"), "Holy Fire", new HolyFireBehavior()).Knight().SetDropRate(1),
                new CardDefinition(new CardDataID("pushback"), "Pushback", new PushbackBehavior()).Rogue().SetDropRate(1),
                new CardDefinition(new CardDataID("spreadingPlague"), "Spreading Plague", new SpreadingPlagueBehavior()).SetDropRate(2).Rogue(),
                
                
                
                // trinkets
                new CardDefinition(new CardDataID("thickSkin"), "Thick Skin", new ThickSkinTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("frenzy"), "Frenzy", new FrenzyTrinketCardBehavior()).SetDropRate().Knight().Rogue(),
                new CardDefinition(new CardDataID("everlastingPoison"), "Everlasting Poison", new EverlastingPoisonTrinketCardBehavior()), // TODO: remove
                new CardDefinition(new CardDataID("ringOfPoison"), "Ring Of Poison", new RingOfPoisonTrinketCardBehavior()).SetDropRate().Rogue(),

                new CardDefinition(new CardDataID("swordArtisan"), "Sword Artisan", new SwordArtisanTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("hammerArtisan"), "Hammer Artisan", new HammerArtisanTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("bowArtisan"), "Bow Artisan", new BowArtisanTrinketCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("ringOfHealth"), "Ring Of Health", new RingOfHealthTrinketCardBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("hotheadLocket"), "Hothead Locket", new HotheadLocketTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("bristleRing"), "Bristle Ring", new BristleRingTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("ringOfFortitude"), "Ring Of Fortitude", new RingOfFortitudeCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("ogiersCollar"), "Ogier's Collar", new OgiersCollarBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("roadrunnersRing"), "Roadrunner's Ring", new RoadrunnersRingTrinketCardBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("sleepover"), "Sleepover", new SleepoverTrinketCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("pierceRing"), "Pierce Ring", new PierceRingTrinketCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("punctureRing"), "Puncture Ring", new PunctureRingTrinketCardBehavior()).SetDropRate(2, dropInPaltry: false).Knight().Rogue(),
                new CardDefinition(new CardDataID("fawkesPendant"), "Fawkes' Pendant", new FawkesPendantTrinketCardBehavior()).SetDropRate().Knight().Rogue(),
                new CardDefinition(new CardDataID("propMastersRing"), "Prop Master's Ring", new PropMastersRingTrinketCardBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("catnapRing"), "Catnap Ring", new CatnapRingTrinketCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("bhimasRing"), "Bhimas' Ring", new BhimasRingTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("sinnersMight"), "Sinner's Might", new SinnersMightTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("arthursPendant"), "Arthur's Pendant", new ArthursPendantTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("extortionAmulet"), "Extortion Amulet", new ExtortionAmuletTrinketCardBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("theMetronome"), "The Metronome", new TheMetronomeTrinketCardBehavior()).SetDropRate(),
                new CardDefinition(new CardDataID("medusasRing"), "Medusa's Ring", new MedusasRingTrinketCardBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("subtleBodyCharm"), "Subtle Body Charm", new SubtleBodyCharmCardBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("rapidFireRing"), "Rapid Fire Ring", new RapidFireRingTrinketCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("eyeOfTheStorm"), "Eye Of The Storm", new EyeOfTheStormCardBehavior()), // TODO: remove eventually
                new CardDefinition(new CardDataID("theEternalFlame"), "The Eternal Flame", new TheEternalFlameCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("archmageCharm"), "Archmage's Charm", new ArchmagesCharmCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("thothsPendant"), "Thoth's Pendant", new ThothsPendantBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("phoenixCharm"), "Phoenix Charm", new PhoenixCharmBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("mindOverMatter"), "Mind Over Matter", new MindOverMatterBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("bensKey"), "Ben's Key", new BensKeyBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("augustinsCharm"), "Augustin's Charm", new AugustinsCharmBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("swordTradersRing"), "Sword Trader's Ring", new SwordTradersRingBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("intimidationRing"), "Intimidation Ring", new IntimidationRingBehavior()).SetDropRate().Knight().Rogue(),
                new CardDefinition(new CardDataID("theBlinkFruit"), "The Blink Fruit", new TheBlinkFruitBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("auramancersAmulet"), "Auramancer's Amulet", new AuramancersAmuletBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("magneticRing"), "Magnetic Ring", new MagneticRingBehavior()).SetDropRate().Knight().Rogue(),
                new CardDefinition(new CardDataID("nonMagneticRing"), "Non-Magnetic Ring", new NonMagneticRingBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("ceridwensEarring"), "Ceridwen's Earring", new CeridwensEarringBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("ringOfPatience"), "Ring Of Patience", new RingOfPatienceBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("chandrasEssence"), "Chandra's Essence", new ChandrasEssenceBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("pocketWormhole"), "Pocket Wormhole", new PocketWormholeCardBehavior()).SetDropRate().Knight().Rogue().Mage(),
                new CardDefinition(new CardDataID("ringOfExuberance"), "Ring Of Exuberance", new RingOfExuberanceBehavior()).SetDropRate().Knight(),
                new CardDefinition(new CardDataID("flashHealRing"), "Flash Heal Ring", new FlashHealRingBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("tearOfNyx"), "Tear Of Nyx", new TearOfNyxBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("ninjutsuPendant"), "Ninjutsu Pendant", new NinjutsuPendantBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("teslasPartingGift"), "Tesla's Parting Gift", new TeslasPartingGiftBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("equivalenceRing"), "Equivalence Ring", new EquivalenceRingBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("ringOfPyromania"), "Ring Of Pyromania", new RingOfPyromaniaBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("staffArtisan"), "Staff Artisan", new StaffArtisanTrinketCardBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("luckyCharm"), "Lucky Charm", new LuckyCharmBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("assassinsBauble"), "Assassin's Bauble", new AssassinsBaubleBehavior()).SetDropRate().Rogue(),
                new CardDefinition(new CardDataID("plutusRing"), "Plutus' Ring", new PlutusRingBehavior()).SetDropRate().Mage(),
                new CardDefinition(new CardDataID("divineRing"), "Divine Ring", new DivineRingBehavior()).Knight().SetDropRate(1),
                new CardDefinition(new CardDataID("kramersPendant"), "Kramer's Pendant", new KramersPendantBehavior()).Knight().SetDropRate(1),
                new CardDefinition(new CardDataID("setDressersEarring"), "Set Dresser's Earring", new SetDressersEarringBehavior()).Rogue().SetDropRate(1),
                new CardDefinition(new CardDataID("amuletOfIce"), "Amulet Of Ice", new AmuletOfIceBehavior()).SetDropRate(2, dropInPaltry: false).Rogue(),
                new CardDefinition(new CardDataID("plunderersRing"), "Plunderer's Ring", new PlunderersRingBehavior()).SetDropRate(2).Mage(),
                new CardDefinition(new CardDataID("daggerArtisan"), "Dagger Artisan", new DaggerArtisanTrinketCardBehavior()).Rogue().SetDropRate(1),
                new CardDefinition(new CardDataID("piezoRing"), "Piezo Ring", new PiezoRingBehavior()).Mage().SetDropRate(1, dropInPaltry: false),
                new CardDefinition(new CardDataID("nobunagasLocket"), "Nobunaga's Locket", new NobunagasLocketBehavior()).Rogue().SetDropRate(3),

                new CardDefinition(new CardDataID("knightsHeirloom"), "Knight's Heirloom", new KnightsHeirloomBehavior()).Knight().SetDropRate(),
                new CardDefinition(new CardDataID("roguesHeirloom"), "Rogue's Heirloom", new RoguesHeirloomBehavior()).Rogue().SetDropRate(),
                new CardDefinition(new CardDataID("magesHeirloom"), "Mage's Heirloom", new MagesHeirloomBehavior()).Mage().SetDropRate(),

                new CardDefinition(new CardDataID("sealOfFate"), "Seal Of Fate", new SealOfFateBehavior()),
                
                // ethereal cards
                new CardDefinition(new CardDataID("iceShield"), "Ice Shield", new IceShieldCardBehavior()).SetEtherealGear(),
                new CardDefinition(new CardDataID("armorPlate"), "Armor Plate", new ArmorPlateCardBehavior()).SetEtherealGear(),
                new CardDefinition(new CardDataID("manaOrb"), "Mana Orb", new ManaOrbCardBehavior()).SetEtherealGear(),
                new CardDefinition(new CardDataID("ghostShield"), "Ghost Shield", new GhostShieldBehavior()).SetEtherealGear(),
                new CardDefinition(new CardDataID("lesserHeal"), "Lesser Heal", new LesserHealScrollCardBehavior()).SetEtherealGear(),
                new CardDefinition(new CardDataID("soaringShield"), "Soaring Shield", new SoaringShieldFauxWeaponBehavior()).SetEtherealGear(),
                
                // contraptions
                new CardDefinition(new CardDataID("illusoryWall"), "Illusory Wall", new IllusoryWallCardBehavior()),
                new CardDefinition(new CardDataID("simpleIllusoryWall"), "Illusory Wall", new SimpleIllusoryWallCardBehavior()),
                new CardDefinition(new CardDataID("barredDoor"), "Barred Door", new BarredDoorCardBehavior()),
                new CardDefinition(new CardDataID("narrowTunnel"), "Narrow Tunnel", new NarrowTunnelBehavior()),
                new CardDefinition(new CardDataID("strangeChest"), "Strange Chest", new StrangeChestCardBehavior()),
                new CardDefinition(new CardDataID("eitherDoor"), "Either Door", new EitherDoorCardBehavior()),
                new CardDefinition(new CardDataID("cellDoor"), "Cell Door", new CellDoorCardBehavior()),
                new CardDefinition(new CardDataID("spikes"), "Spikes", new SpikesCardBehavior()),
                new CardDefinition(new CardDataID("herotrap"), "Herotrap", new HerotrapBehavior()),
                new CardDefinition(new CardDataID("paywall"), "Paywall", new PaywallBehavior()),
                new CardDefinition(new CardDataID("pushWall"), "Push Wall", new PushWallBehavior()),
                new CardDefinition(new CardDataID("rope"), "Rope", new RopeBehavior()),
                new CardDefinition(new CardDataID("lightningRod"), "Lightning Rod", new LightningRodCardBehavior()),
                new CardDefinition(new CardDataID("secretTunnel"), "Secret Tunnel", new SecretTunnelBehavior()),
                new CardDefinition(new CardDataID("shrineProtection"), "Protection Shrine", new ProtectionShrineBehavior()),
                new CardDefinition(new CardDataID("shrineHealth"), "Health Shrine", new HealthShrineBehavior()),
                new CardDefinition(new CardDataID("shrineSlumber"), "Slumber Shrine", new SlumberShrineBehavior()),
                new CardDefinition(new CardDataID("shrineRevelation"), "Revelation Shrine", new RevelationShrineBehavior()),
                new CardDefinition(new CardDataID("shrineResource"), "Resource Shrine", new ResourceShrineBehavior()),
                new CardDefinition(new CardDataID("fountainOfHealth"), "Fountain of Health", new FountainOfHealthBehavior()),
                new CardDefinition(new CardDataID("fountainOfVitality"), "Fountain of Vitality", new FountainOfVitalityBehavior()),
                
                
                
                // enemies
                new CardDefinition(new CardDataID("skeleton"), "Skeleton", new SkeletonCardBehavior()),
                new CardDefinition(new CardDataID("skelegnon"), "Skelegnon", new SkelegnonCardBehavior()),
                new CardDefinition(new CardDataID("sentry"), "Sentry", new SentryCardBehavior()),
                new CardDefinition(new CardDataID("lackey"), "Lackey", new LackeyCardBehavior()),
                new CardDefinition(new CardDataID("mercenary"), "Mercenary", new MercenaryCardBehavior()),
                new CardDefinition(new CardDataID("zombie"), "Zombie", new ZombieBehavior()),
                new CardDefinition(new CardDataID("packRat"), "Pack Rat", new PackRatBehavior()),
                new CardDefinition(new CardDataID("brute"), "Brute", new BruteCardBehavior()),
                new CardDefinition(new CardDataID("orc"), "Orc", new OrcCardBehavior()),
                new CardDefinition(new CardDataID("armoredOrc"), "Grunt", new GruntCardBehavior()),
                new CardDefinition(new CardDataID("bandit"), "Bandit", new BanditCardBehavior()),
                new CardDefinition(new CardDataID("medusa"), "Medusa", new MedusaCardBehavior()),
                new CardDefinition(new CardDataID("mimic"), "Mimic", new MimicBehavior()),

                new CardDefinition(new CardDataID("ghoul"), "Ghoul", new GhoulCardBehavior()),
                new CardDefinition(new CardDataID("shaman"), "Shaman", new ShamanCardBehavior()),
                new CardDefinition(new CardDataID("archer"), "Archer", new ArcherCardBehavior()),
                new CardDefinition(new CardDataID("pitati"), "Pitati", new PitatiCardBehavior()),
                new CardDefinition(new CardDataID("ranger"), "Ranger", new RangerCardBehavior()),
                new CardDefinition(new CardDataID("stalker"), "Stalker", new StalkerBehavior()),
                new CardDefinition(new CardDataID("wizard"), "Wizard", new WizardCardBehavior()),
                new CardDefinition(new CardDataID("hekaPriest"), "Heka Priest", new HekaPriestBehavior()),
                new CardDefinition(new CardDataID("ratConjurer"), "Rat Conjurer", new RatConjurerBehavior()),
                new CardDefinition(new CardDataID("mummy"), "Mummy", new MummyBehavior()),
                new CardDefinition(new CardDataID("medjay"), "Medjay", new MedjayCardBehavior()),
                new CardDefinition(new CardDataID("guardian"), "Guardian", new GuardianCardBehavior()),
                new CardDefinition(new CardDataID("kidnapper"), "Kidnapper", new KidnapperBehavior()),
                new CardDefinition(new CardDataID("protector"), "Protector", new ProtectorCardBehavior()),
                new CardDefinition(new CardDataID("zealot"), "Zealot", new ZealotBehavior()),
                new CardDefinition(new CardDataID("trenchRat"), "Trench Rat", new TrenchRatBehavior()),
                new CardDefinition(new CardDataID("voidPriest"), "Void Priest", new VoidPriestCardBehavior()),


                new CardDefinition(new CardDataID("banshee"), "Banshee", new BansheeCardBehavior()),
                new CardDefinition(new CardDataID("styx"), "Styx", new StyxBehavior()),
                new CardDefinition(new CardDataID("orok"), "Orok", new OrokCardBehavior()),
                new CardDefinition(new CardDataID("rahotep"), "Rahotep", new RahotepBehavior()),
                new CardDefinition(new CardDataID("coelescus"), "Coelescus", new CoelescusCardBehavior()),
                new CardDefinition(new CardDataID("avoozlIntro"), "Å¥ØΩZ£", new AvoozlIntroCardBehavior()),
                new CardDefinition(new CardDataID("avoozlA"), "Å¥ØΩZ£", new AvoozlACardBehavior()),
                new CardDefinition(new CardDataID("avoozlB"), "Å¥ØΩZ£", new AvoozlBCardBehavior()),
                new CardDefinition(new CardDataID("avoozlC"), "Å¥ØΩZ£", new AvoozlCCardBehavior()),


                // npcs
                new CardDefinition(new CardDataID("healer"), "Healer", new HealerNPCCardBehavior()),
                new CardDefinition(new CardDataID("priest"), "Healer", new PriestNPCCardBehavior()),
                new CardDefinition(new CardDataID("cardsmith"), "Cardsmith", new CardsmithNPCCardBehavior()),
                new CardDefinition(new CardDataID("merchant"), "Merchant", new MerchantNPCCardBehavior()),
                new CardDefinition(new CardDataID("trader"), "Trader", new TraderNPCCardBehavior()),
                new CardDefinition(new CardDataID("collector"), "Collector", new CollectorNPCCardBehavior()),
                new CardDefinition(new CardDataID("jeweler"), "Jeweler", new JewelerNPCCardBehavior()),
                new CardDefinition(new CardDataID("apothecary"), "Apothecary", new ApothecaryNPCCardBehavior()),
                new CardDefinition(new CardDataID("peddler"), "Peddler", new PeddlerNPCCardBehavior()),


                new CardDefinition(new CardDataID("pyromaniac"), "Pyromaniac", new PyromaniacNPCCardBehavior()),
                new CardDefinition(new CardDataID("quartermaster"), "Quartermaster", new QuartermasterNPCCardBehavior()),
                new CardDefinition(new CardDataID("taxCollector"), "Exchequer", new ExchequerNPCCardBehavior()),
                new CardDefinition(new CardDataID("kingStoned"), "Petrified King", new KingNPCCardBehavior()),
                new CardDefinition(new CardDataID("king"), "King", new KingNPCCardBehavior()),
                //new CardDefinition(new CardDataID("fortuneTeller"), "Fortune Teller", new BasicNPCCardBehavior("banish", "[b]Banish[/b] Cards")),
                new CardDefinition(new CardDataID("rogueCaptured"), "Rogue", new CapturedRogueBehavior()),
                new CardDefinition(new CardDataID("mageCaptured"), "Mage", new CapturedMageBehavior()),
                new CardDefinition(new CardDataID("pilgrim"), "Pilgrim", new PilgrimBehavior()),
                new CardDefinition(new CardDataID("thief"), "Thief", new ThiefBehavior()),
                new CardDefinition(new CardDataID("statue"), "Statue", new StatueBehavior()),
                new CardDefinition(new CardDataID("girl"), "Girl", new GirlBehavior()),
                new CardDefinition(new CardDataID("sister"), "Sister", new SisterBehavior()),
                new CardDefinition(new CardDataID("brother"), "Brother", new BrotherBehavior()),
                new CardDefinition(new CardDataID("goblinQuestGiver"), "Goblin", new GoblinQuestGiverBehavior()),
                new CardDefinition(new CardDataID("goblinQuestChecker"), "Goblin", new GoblinQuestCheckerBehavior()),

                // sins
                new CardDefinition(new CardDataID("vanity"), "Vanity", new VanityCardBehavior()),
                new CardDefinition(new CardDataID("envy"), "Envy", new EnvyCardBehavior()),
                new CardDefinition(new CardDataID("rashness"), "Rashness", new RashnessCardBehavior()),
                new CardDefinition(new CardDataID("wrath"), "Wrath", new WrathCardBehavior()),
                new CardDefinition(new CardDataID("desire"), "Desire", new DesireCardBehavior()),
                new CardDefinition(new CardDataID("greed"), "Greed", new GreedCardBehavior()),
                new CardDefinition(new CardDataID("sloth"), "Sloth", new SlothCardBehavior()),
                new CardDefinition(new CardDataID("gluttony"), "Gluttony", new GluttonyCardBehavior()),
                new CardDefinition(new CardDataID("originalSin"), "Original Sin", new OriginalSinBehavior()),
                

                // afflictions
                new CardDefinition(new CardDataID("fatigue"), "Fatigue", new FatigueAfflictionBehavior()),
                new CardDefinition(new CardDataID("claustrophobia"), "Claustrophobia", new ClaustrophobiaAfflictionBehavior()),
                new CardDefinition(new CardDataID("adamophobia"), "Adamophobia", new AdamophobiaAfflictionBehavior()),
                new CardDefinition(new CardDataID("blasphemy"), "Blasphemy", new BlasphemyAfflictionBehavior()),
                new CardDefinition(new CardDataID("kosmemophobia"), "Kosmemophobia", new KosmemophobiaAfflictionBehavior()),
                new CardDefinition(new CardDataID("wound"), "Wound", new WoundAfflictionBehavior()),
                new CardDefinition(new CardDataID("futility"), "Futility", new FutilityAfflictionBehavior()),
                new CardDefinition(new CardDataID("shame"), "Shame", new ShameCardBehavior()),
                
                // backtrack cards
                new CardDefinition(new CardDataID("backtrack"), "Backtrack", new BacktrackCardBehavior()),
                new CardDefinition(new CardDataID("tacticalRegroup"), "Tactical Regroup", new TacticalRegroupCardBehavior()),

                new CardDefinition(new CardDataID("reckless"), "Reckless", new RecklessBacktrackPunishmentCardBehavior()),
                new CardDefinition(new CardDataID("fragile"), "Fragile", new FragileBacktrackPunishmentCardBehavior()),
                new CardDefinition(new CardDataID("careless"), "Careless", new CarelessBacktrackPunishmentCardBehavior()),
                new CardDefinition(new CardDataID("myopic"), "Myopic", new MyopicBacktrackPunishmentCardBehavior()),
                new CardDefinition(new CardDataID("tired"), "Tired", new TiredBacktrackPunishmentCardBehavior()),

                // potions
                new CardDefinition(new CardDataID("largeHPPotion"), "Large HP Potion", new LargeHPPotionBehavior()),
                new CardDefinition(new CardDataID("constitutionPotion"), "Constitution Potion", new ConstitutionPotionBehavior()),
                new CardDefinition(new CardDataID("oilOfSharpness"), "Oil Of Sharpness", new OilOfSharpnessBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("potionOfPoison"), "Potion Of Poison", new PotionOfPoisonBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("potionOfFire"), "Potion Of Fire", new PotionOfFireBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("smallHPPotion"), "Small HP Potion", new SmallHPPotionBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("manaPotion"), "Mana Potion", new ManaPotionBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("blinkPotion"), "Blink Potion", new BlinkPotionBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("potionOfSleep"), "Potion Of Sleep", new PotionOfSleepBehavior()).RegularPotion(),
                new CardDefinition(new CardDataID("stealthPotion"), "Stealth Potion", new StealthPotionBehavior()).RegularPotion(),
                
                // other cards
                new CardDefinition(new CardDataID("hallway"), "Hallway", new HallWayCardBehavior()),
                //new CardDefinition(new CardDataID("gatekeeper"), "Lifekeeper", new GatekeeperCardBehavior()),
                new CardDefinition(new CardDataID("stairs"), "Stairs", new StairsCardBehavior()),
                new CardDefinition(new CardDataID("lockedWingDoor"), "Winged Door", new LockedDoorCardBehavior(true)),
                new CardDefinition(new CardDataID("lockedDoor"), "Locked Door", new LockedDoorCardBehavior(false)),
                new CardDefinition(new CardDataID("unlockedDoor"), "Door", new UnlockedDoorCardBehavior()),
                new CardDefinition(new CardDataID("climbingRope"), "Climbing Rope", new ClimbingRopeCardBehavior()),
                new CardDefinition(new CardDataID("cellKey"), "Small Key", new CellKeyLootCardBehavior()),
                new CardDefinition(new CardDataID("unidentifiedLoot"), "Unidentified Loot", new UnidentifiedLootCardBehavior()),
                new CardDefinition(new CardDataID("deckInventory"), "Deck Inventory", new DeckInventoryBehavior()),
                new CardDefinition(new CardDataID("emerald01"), "Emerald", new Emerald1CardBehavior()),
                new CardDefinition(new CardDataID("emerald05"), "5 Emeralds", new Emerald5CardBehavior()),
                new CardDefinition(new CardDataID("emerald10"), "10 Emeralds", new Emerald10CardBehavior()),
                new CardDefinition(new CardDataID("emerald20"), "20 Emeralds", new Emerald20CardBehavior()),
                new CardDefinition(new CardDataID("emerald50"), "50 Emeralds", new Emerald50CardBehavior()),
                new CardDefinition(new CardDataID("spikedEmerald"), "Spiked Emerald", new SpikedEmeraldCardBehavior()),
                //new CardDefinition(new CardDataID("unidentifiedTrinket"), "Strange Trinket", new UnidentifiedTrinketCardBehavior()),
                new CardDefinition(new CardDataID("pillar"), "Pillar", new PillarCardBehavior()),
                new CardDefinition(new CardDataID("wall"), "Wall", new WallCardBehavior()),
                new CardDefinition(new CardDataID("barrel"), "Barrel", new BarrelCardBehavior()),
                new CardDefinition(new CardDataID("rustyChest"), "Rusty Chest", new RustyChestCardBehavior()),
                new CardDefinition(new CardDataID("bloodyChest"), "Bloody Chest", new BloodyChestCardBehavior()),
                new CardDefinition(new CardDataID("achingChest"), "Aching Chest", new AchingChestCardBehavior()),
                new CardDefinition(new CardDataID("gravestone"), "Gravestone", new GravestoneCardBehavior()),
                new CardDefinition(new CardDataID("mirage"), "Mirage", new MirageCardBehavior()),
                new CardDefinition(new CardDataID("oldPillar"), "Old Pillar", new OldPillarCardBehavior()),
                new CardDefinition(new CardDataID("sarcophagus"), "Sarcophagus", new SarcophagusBehavior()),
                new CardDefinition(new CardDataID("sacredStatue"), "Sacred Statue", new SacredStatueBehavior()),
                new CardDefinition(new CardDataID("potionShelf"), "Potion Shelf", new PotionShelfBehavior()),
                new CardDefinition(new CardDataID("voidRoot"), "Void Root", new VoidRootBehavior()),
                new CardDefinition(new CardDataID("tangleRoot"), "Tangle Root", new TangleRootBehavior()),
                new CardDefinition(new CardDataID("leadRoot"), "Lead Root", new LeadRootBehavior()),
                new CardDefinition(new CardDataID("movingWallUp"), "Moving Wall", new MovingWallUpBehavior()),
                new CardDefinition(new CardDataID("movingWallDown"), "Moving Wall", new MovingWallDownBehavior()),
                new CardDefinition(new CardDataID("movingWallLeft"), "Moving Wall", new MovingWallLeftBehavior()),
                new CardDefinition(new CardDataID("movingWallRight"), "Moving Wall", new MovingWallRightBehavior()),


                new CardDefinition(new CardDataID("xBarrel"), "X-Barrel", new XBarrelCardBehavior()),
                new CardDefinition(new CardDataID("oBarrel"), "O-Barrel", new OBarrelCardBehavior()),
                new CardDefinition(new CardDataID("iBarrel"), "I-Barrel", new IBarrelCardBehavior()),

                new CardDefinition(new CardDataID("lightningOrb"), "Lightning Orb", new LightningOrbCardBehavior()), // TODO: remove
                new CardDefinition(new CardDataID("lingeringFlame"), "Lingering Flame", new LingeringFlameCardBehavior()),

                // quests
                new CardDefinition(new CardDataID(ThePilgrimQuestTemplate.QuestIDStatic), "The Pilgrim", new ThePilgrimQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(TheBrotherQuestTemplate.QuestIDStatic), "The Brother", new TheBrotherQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(TheThiefQuestTemplate.QuestIDStatic), "The Thief", new TheThiefQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(TheRopeQuestTemplate.QuestIDStatic), "The Rope", new TheRopeQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(PropHaterQuestTemplate.QuestIDStatic), "Prop Hater", new PropHaterQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(AbstinenceQuestTemplate.QuestIDStatic), "Abstinence", new AbstinenceQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(KeyhoarderQuestTemplate.QuestIDStatic), "Keyhoarder", new KeyhoarderQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(UntouchedQuestTemplate.QuestIDStatic), "Untouched", new UntouchedQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(UnfinisherQuestTemplate.QuestIDStatic), "Unfinisher", new UnfinisherQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(TacticalLendQuestTemplate.QuestIDStatic), "Tactical Lend", new TacticalLendQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(HalfFullQuestTemplate.QuestIDStatic), "Half Full", new HalfFullQuestTemplate.Behavior()),
                new CardDefinition(new CardDataID(TreadLightlyQuestTemplate.QuestIDStatic), "Tread Lightly", new TreadLightlyQuestTemplate.Behavior()),
            };

            IEnumerable<CardDefinition> allDefinitions = baseDefinitions;

            // dynamically add rooms
            allDefinitions = allDefinitions.Concat(BaseRoomConfigs.Configs.Values.Select(c => new CardDefinition(new CardDataID(c.ID), c.NameEN, new RoomCardBehavior())));

            foreach (var d in allDefinitions)
            {
                CardRepository.AddCardDefinition(d, false);
            }
        }

        public static void RegisterTowers()
        {
            var regularQuestBuilder = new QuestBuilder(new TheRopeQuestTemplate(),
                    new PropHaterQuestTemplate(), new AbstinenceQuestTemplate(), new KeyhoarderQuestTemplate(), new UntouchedQuestTemplate(),
                    new UnfinisherQuestTemplate(), new TacticalLendQuestTemplate(), new HalfFullQuestTemplate(), new TreadLightlyQuestTemplate(),
                    new TheThiefQuestTemplate(), new TheBrotherQuestTemplate(), new ThePilgrimQuestTemplate()
                    );
            var darkTowerQuestBuilder = new QuestBuilder(new TheRopeQuestTemplate(),
                    new PropHaterQuestTemplate(), new AbstinenceQuestTemplate(), new KeyhoarderQuestTemplate(), new UntouchedQuestTemplate(),
                    new UnfinisherQuestTemplate(), new TacticalLendQuestTemplate(), new HalfFullQuestTemplate(), new TreadLightlyQuestTemplate()
                    );
            var debugQuestBuilder = new QuestBuilder(new TheRopeQuestTemplate(), new AbstinenceQuestTemplate());

            DungeonRunBuilder.RegisterBuilder("paltryTower", new TowerBuilderPaltry(new QuestBuilder(new TheRopeQuestTemplate(), new UntouchedQuestTemplate()))); // TODO: alternative
            DungeonRunBuilder.RegisterBuilder("gatheringTower", new TowerBuilderGathering(OS.IsDebugBuild() ? debugQuestBuilder : regularQuestBuilder));
            DungeonRunBuilder.RegisterBuilder("cemeteryTower", new TowerBuilderCemetery(OS.IsDebugBuild() ? debugQuestBuilder : regularQuestBuilder));
            DungeonRunBuilder.RegisterBuilder("desertTower", new TowerBuilderDesert(OS.IsDebugBuild() ? debugQuestBuilder : regularQuestBuilder));
            DungeonRunBuilder.RegisterBuilder("shiftingTower", new TowerBuilderShifting(OS.IsDebugBuild() ? debugQuestBuilder : regularQuestBuilder));
            DungeonRunBuilder.RegisterBuilder("darkTower", new TowerBuilderDark(OS.IsDebugBuild() ? debugQuestBuilder : darkTowerQuestBuilder));
        }

        internal static void RegisterHeroes()
        {
            CardRepository.AddCardDefinition(new CardDefinition(new CardDataID("knight"), "Knight", new HeroCardBehavior()), false);
            CardRepository.AddCardDefinition(new CardDefinition(new CardDataID("rogue"), "Rogue", new HeroCardBehavior()), false);
            CardRepository.AddCardDefinition(new CardDefinition(new CardDataID("mage"), "Mage", new HeroCardBehavior()), false);
            HeroBuilder.AddHero(new HeroData("knight", 24, false, "A holy warrior who walks the line between righteousness and sin. Uses powerful weapons and shields themselves with heavy armor."));
            HeroBuilder.AddHero(new HeroData("rogue", 21, true, "A cunning thief who has perfected the art of deception and trickery. Often attacks from a safe distance, uses deadly poison or slips past monsters unnoticed."));
            HeroBuilder.AddHero(new HeroData("mage", 15, true, "A wise mage who has studied the infinite depths of magic all his life. With zealous focus and wits, using staves, wands and powerful spells, he channels the elements."));
        }
    }
}