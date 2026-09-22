using HarmonyLib;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago
{
    public static class DeathLinkMessages
    {
        private static Dictionary<string, string[]> _DeathLinkMessageDict = new Dictionary<string, string[]>()
        {
            {"Abandoned Mine", ["{0} experienced a cave-in.", "{0} left the minecart on the wrong side of the track.", "Quoth {0}: \"Life? I Abandoned Mine.\""]},
            {"Apple Orchard", ["{0} scorched a bit more than the sundial."]},
            {"Aquarium", ["{0} is swimming with the fishes in the Aquarium.", "{0}'s tank contains a dead herring."]},
            {"Archives", ["{0} was archived.", "NO TRACE IS DISCOVERED OF MISSING SYNKA HEIR […]"]},
            {"Atelier", ["{0} should have been more rosewary.", "{0} drew a blank in the Atelier."]},
            {"Attic", ["{0} is now collecting dust in the Attic."]},
            {"Ballroom", ["{0} danced until they dropped in the Ballroom.", "{0} really dropped the ball.", "{0} found out they had two left feet"]},
            {"Basement", ["{0} got lost in a maze of storage crates.", "{0} fell down the elevator shaft into the Basement."]},
            {"Bedroom", ["{0} was baffled in the Bedroom.", "{0} found a monster under their bed."]},
            {"Bedroom Closet", ["{0} took a long nap in the Bedroom Closet."]},
            {"Billiard Room", ["{0} failed at basic math.", "{0} aimed poorly."]},
            {"Blackbridge Grotto", ["{0} 01100100 01101001 01100101 01100100", "{0}'s boot drive failed and spent all day reinstalling.", "{0} tested if they were still connected to the server."]},
            {"Boiler Room", ["{0} lost power in the Boiler Room.", "{0} should have duct."]},
            {"Book Shop", ["{0} thought read rhymed with dead in the Bookshop.", "{0} read their own obituary.", "{0} didn't find a new clue."]},
            {"Boudoir", ["{0} chose 2 die in the Boudoir."]},
            {"Break Room", ["{0} broke in the Break Room.", "{0} took an unexpected nap in the Break Room.", "{0} forgot their Staff Keycard in the Break Room."]},
            {"Breakfast Nook", ["{0} ate too much breakfast.", "{0} found out they were allergic to bacon and eggs."]},
            {"Bunk Room", ["{0} fell from the top bunk.", "Today was a good day for {0}, but it had to end."]},
            {"Campsite", ["{0} tucked into the tent and Called It a Day", "{0} got a little too close to the campfire."]},
            {"Casino", ["{0} gambled away their inheritance.", "{0} caught a snake. It bit back.", "{0} is betting it all on the next run."]},
            {"Catacombs", ["{0} fell into an open coffin.", "What did {0} expect in the Catacombs?"]},
            {"Chamber of Mirrors", ["{0} trapped themself in the Chamber of Mirrors.", "{0} did not break the glass in case of emergency."]},
            {"Chapel", ["{0} couldn't afford to pay their tithes.", "{0} was caught taking from the alms"]},
            {"Classroom", ["{0} wisely chose death over the Final Exam."]}, //Potentially add grade detection for: "{0} is not smarter than a __th grader."
            {"Clock Tower", ["{0} took the short way down a spiral staircase."]},
            {"Cloister", ["{0} shut themselves away in the Cloister.", "{0} is the new statue of the Cloister."]},
            {"Cloister of Dauja", ["{0} found out why you let roosting birds lie."]},
            {"Cloister of Draxus", ["{0}'s life met a dead end."]},
            {"Cloister of Joya", ["{0} was smote down for enjoying pineapple on pizza.", "{0} was smote down for putting ketchup on eggs.", "{0} was smote down for not enjoying pineapple on pizza."]},
            {"Cloister of Lydia", ["{0} was tactfully reminded of the limitations of ambition."]},
            {"Cloister of Mila", ["{0} gave themselves in service of death."]},
            {"Cloister of Orinda", ["Through her, the way to {0}'s death was revealed."]},
            {"Cloister of Rynna", ["{0} still wasn't lucky enough to make it."]},
            {"Cloister of Veia", ["{0} now sleeps in soot and ash."]},
            {"Closed Exhibit", ["{0} was arrested by Fenn Aries security.", "{0} was caught trying to steal the crown."]},
            {"Closet", ["{0} is now the Skeleton in the Closet.", "{0} accidentally got locked in the closet."]},
            {"Coat Check", ["{0} checked themselves at the Coat Check.", "{0} forgot to collect their coat before leaving."]},
            {"Commissary", ["{0} got decommissioned in the Commissary.", "{0} was replaced by a Banana in the Commissary.", "{0} is stuck waiting to play Dirigiblocks in the Commissary."]},
            {"Conference Room", ["{0} is stuck in a meeting that could have been an email.", "Whenever {0} would be dead throughout the house..."]},
            {"Conservatory", ["{0} failed to conserve their energy in the Conservatory.", "{0} set their death to Commonplace."]},
            {"Corridor", ["{0} was left unlocked.", "{0} had to corr it a day."]},
            {"Corriyard", ["{0} got dirt on the carpet."]},
            {"Courtyard", ["{0} got given an inch and took a yard.", "{0} lost the Court case."]},
            {"Darkroom", ["{0} couldn't find the light switch.", "{0}'s lights went out.", "{0} became dark, darker, yet darker"]},
            {"Den", ["{0} spent the entire day searching for the Secret of the Den.", "{0} spent a min or ten million in the Den."]},
            {"Dining Room", ["{0} is calling it the Die-ning Room, instead.", "{0} only made it to rank 7.", "{0} was allergic to salmon."]},
            {"Dormitory", ["{0} failed to pull an all-nighter on their thesis.", "{0} forgot to study for the drafting exam."]},
            {"Dovecote", ["{0} found themself too peckish to continue.", "{0} dove too deep into the bowels of the house."]},
            {"Drafting Studio", ["{0} removed themself from the drafting pool.", "{0} drafted the wrong studio."]},
            {"Drawing Room", ["{0}'s gait lengths are now 0.", "{0}s are slightly deader than days.", "{0}'s time is drawing to a close."]},
            {"East Wing Hall", ["{0} is in the decEast Hall."]},
            {"Electric Eel Aquarium", ["{0} discovered where the Electric Eel Aquarium's power comes from."]},
            {"Empty Closet", ["{0} wasn't adjacent to a Red Room."]},
            {"Entrance Hall", ["{0} thought it was an Exit Hall.", "{0} didn't know how to make an Entrance.", "{0} spent the rest of the day entranced in the Entrance Hall."]},
            //{"Fall it a day?", ["{0} Has Tripped and can't get back up.", "{0} was banned for cheating.", "{0} fell out of the world."]}, To Do, add fall it a day handling
            {"Foundation", ["{0} reached the Basement without the elevator.", "{0} found an unused repellent at the corner of the room and accidentally sprayed on self."]},
            {"Foyer", ["{0} went bust in the Foyer."]},
            {"Freezer", ["{0} lost their cool in the Freezer.", "More than {0}'s accounts were frozen."]},
            {"Funeral Parlor", ["{0} honored H.S.S.'s memory.", "After opening the wrong box, {0} finds themself in one of their own."]},
            {"Furnace", ["{0} got a bit too hot-tempered in the Furnace.", "{0} had a meltdown.", "{0} couldn’t get to the other side of the Furnace."]},
            {"Gallery", ["{0} failed to appreciate modern art.", "{0} pondered too long."]},
            {"Garage", ["{0}'s car failed to start.", "{0} doesn't know how to drive stickshift.", "{0} learned the difference between acronyms and abbreviations."]},
            {"Geist Bedroom", ["{0}'s soul still wanders the Geist Bedroom.", "{0} gave up the ghost."]},
            {"Gemstone Caverns", ["{0} collapsed (in) the Gemstone Cavern.", "{0} delved too greedily and too deep in the Gemstone Cavern."]},
            {"Goldfish Aquarium", ["{0} learned the hard way why they're the snack that smiles back."]},
            {"Great Hall", ["{0} drafted Great Hall and must now Hall It A Day.", "{0} is now in heaven (locked doors)."]},
            {"Greenhouse", ["The Greenhouse's Soil Quality has improved with the new Compost: {0}", "A broken {0} was used in the Greenhouse.", "{0} found a watering can't."]},
            {"Guess Bedroom", ["{0} did not guess correctly."]},
            {"Guest Bedroom", ["{0} 'guest' incorrectly.", "{0} mistook the Guest Bedroom for a Resident Bedroom."]},
            {"Gymnasium", ["{0} forgot that the Gymnasium would take away their steps.", "{0} will never be balling."]},
            {"Hallway", ["{0} must HALL it a day."]},
            //{"Hallway (tomorrow)", ["{0} decided to get to tomorrow a bit faster."]}, //TODO handle the same name upgrade cases
            {"Hallway Closet", ["{0} wasn't adjacent to a Hallway."]},
            {"Her Ladyship's Chamber", ["{0} followed her Ladyship to the grave.", "{0} found HLC and can finally die happy.", "Does it never end? For {0}, it does."]},
            {"Her Ladyship's Spare Room", ["{0} questioned why Her Ladyship needs so many bedrooms."]},
            {"Hovel", ["{0} was terminated on suspicion of not watering the plants.", "{0} had no luck testing their full house theory. As expected."]},
            {"Indoor Nursery", ["{0} grew the flowers for their own grave.", "{0} is now pushing up daisies."]},
            {"Inner Sanctum", ["{0} couldn't find the way to Outer Sanctum.", "Inner Sanctum? {0} hardly knows 'er!"]},
            {"Kitchen", ["{0} couldn't afford lunch.", "{0} was boiled into soup.", "{0} checked the fridge for food too many times."]},
            {"Laboratory", ["{0} experimented with ending the day in the Laboratory.", "Each time {0} receives a deathlink, they must call it a day.", "{0} Iodine Tin Oxygen Nobelium Phosphorus Erbium Astatine Iodine Oxygen Nitrogen Aluminum"]},
            {"Laundry Room", ["{0} swapped ALL of their steps with ALL of their IQ.", "{0} lost a sock in the washing machine."]},
            {"Lavatory", ["{0} spent the entire day searching for the Secret of the Lavatory.", "{0} went to the wrong throne."]},
            {"Library", ["{0} was removed from the estate for talking in the Library.", "{0} forgot to pay their overdue fees.", "{0} tended a rose vine."]},
            {"Locker Room", ["{0} was stuffed in a locker.", "{0} lost the key to their locker."]},
            {"Locksmith", ["{0} got locked in the Locksmith's.", "{0} couldn't find the key to their heart "]},
            {"Lost & Found", ["{0} was lost but not found.", "{0} needed an investor."]},
            {"Maid's Chamber", ["{0} was tidied up by the maids.", "{0} was cleaned."]},
            {"Mail Room", ["{0} will be delivered tomorrow.", "{0} was shipped to a different estate by accident."]},
            {"Master Bedroom", ["Open only in the event of {0}'s death."]},
            {"Mechanarium", ["{0} has opened their last door of eight.", "{0}'s gears ground to a halt."]},
            {"Morning Room", ["{0} misspelt the Morning Room.", "{0} tried entering the Morning Room at 12:01 PM."]},
            {"Mount Holly Giftshop", ["{0} died of old age waiting to buy Dirigiblocks.", "{0} bought the ticket to A New Day. Thank you for visiting Mount Holly Gift Shop!"]},
            {"Music Room", ["{0} thought their injur-key was minor, but it was major.", "{0} faced the music in the Music Room.", "{0} was denoted in verse."]},
            {"Nook", ["{0} had to look for a book that they took from the Nook.", "{0} made a rookie error."]},
            {"Nurse's Station", ["{0} was found skipping classes in the Nurse's Station.", "Not even the Nurse's station was enough to save {0}", "{0} botched their infinite steps setup."]},
            {"Nursery", ["{0} read the story of the Dead prince.", "{0} is going to be nursing their injuries for a while."]},
            {"Observatory", ["{0} got starstruck in the Observatory.", "In observing the stars {0} forgot to observe their surroundings.", "{0} tried to add another word to the spiral of stars."]},
            {"Office", ["{0} got called on the carpet in the Office."]},
            {"Orindian Ruins", ["{0} was ruined in the Ruins."]},
            {"Pantry", ["{0} starved in the Pantry.", "{0} was cut short like the Coffe in the Pantry."]},
            {"Parlor", ["{0} did not die in the Parlor Room | {0} is playing Blue Prince | {0} has DeathLink enabled"]},
            {"Passageway", ["{0} passaged away."]},
            {"Patio", ["{0} gets an L + Patio."]},
            {"Planetarium", ["{0} failed to plan for the Planetarium."]},
            {"Pool Hall", ["{0} couldn't catch a break.", "{0} tried to break and run but they only broke."]},
            {"Precipice", ["{0} fell off the Precipice."]},
            {"Private Drive", ["{0} hit a pothole in the Private Drive."]},
            {"Pump Room", ["{0} found themself with an empty tank."]},
            {"Quest Bedroom", ["{0} failed their quest in the Quest Bedroom."]},
            {"Reading Nook", ["{0} got lost in a book."]},
            {"Reservoir", ["{0} failed to swim across the Reservoir."]},
            {"Room 46", ["{0} was 86ed from room 46.", "{0} tried to light the hearth in Room 46."]},
            {"Room 8", ["{0} was kicked out for the night by their Room 8.", "{0}'s sins weighed too heavily."]},
            {"Root Cellar", ["{0} couldn't find a Turnip", "{0} was rooted in place, permanently."]},
            {"Rotunda", ["{0} got too dizzy to continue in the Rotunda.", "{0} got turned around in the Rotunda."]},
            {"Rough Drafts", ["{0} still seeks what's left of the lies we cast.", "{0} should have been more rosewary.", "{0} did not find the true path."]},
            {"Rumpus Room", ["{0} fell asleep at the sound of a dropping coin in the Rumpus Room.", "{0} can't wait for the dawn of the following day.", "And, as it was foretold, {0} must call it a day here."]},
            {"Sanctum", ["{0} found the Sanctum door closed from behind him."]},
            {"Safehouse", ["{0} fell into a subway track.", "{0} looked a little too close at a grenade launcher.", "{0} caught the last train for the coast."]},
            {"Sauna", ["{0} was thawed and melted in the Sauna.", "{0} passed out from heatstroke."]},
            {"Schoolhouse", ["{0} flunked out of the Schoolhouse.", "{0} was expelled for cheating."]},
            {"Sealed Entrance", ["{0} swung their power hammer a little too hard."]},
            {"Secret Garden", ["{0} tried to weather the Secret Garden, but it was all in vane.", "{0} forgot the fruit went to the conference room on the other side of the house."]},
            {"Secret Passage", ["Deads to a {0} of a color of your choice.", "{0} thought they could pull a black book in the secret passage."]},
            {"Security", ["{0} is feeling insecure.", "{0} sang their swansong."]},
            {"Servant's Quarter", ["{0} was drawn and Quartered."]},
            {"Shelter", ["{0} is hiding out in the Shelter.", "{0} mistakenly thought the Shelter would shelter them from death."]},
            {"Showroom", ["{0} didn't get their inheritance in time.", "{0} went bankrupt attempting to purchase the Trophy of Wealth."]},
            {"Shrine", ["{0} completed their pilgrimage.", "{0} did not have the blessing of the High Roller.", "{0} thought blessing of the general was useful"]},
            {"Solarium", ["{0} got sunburned in the Solarium."]},
            {"Spare Bedroom", ["{0} forgot that the Spare Bedroom doesn't give any steps."]},
            {"Spare Foyer", ["{0} forgot that the Spare Foyer doesn't have any busts."]},
            {"Spare Great Hall", ["{0} forgot that the Spare Great Hall doesn't have a lever.", "{0} died in the Spare Great Hall and deserved it."]},
            {"Spare Greenroom", ["{0} forgot that the Spare Greenroom doesn't give any gems."]},
            {"Spare Hall", ["{0} forgot that the Spare Hall doesn't give any keys."]},
            {"Spare Master Bedroom", ["{0} forgot that the Upgrade Disk they are looking for is in the HLC."]},
            {"Spare Patio", ["{0} realized why you don't take out supporting walls."]},
            {"Spare Room", ["{0} was extraneous in the Spare Room.", "{0} decided to get to finally get to work on the Spare Room.", "{0} was not spared in the Spare Room"]},
            {"Spare Secret Passage", ["{0} forgot that nothing is written behind the bookcase in the Spare Secret Passage.", "{0} was spared from the secret of passing through the room."]},
            {"Spare Servant's Quarters", ["{0} found out all the spare keys were unlabelled."]},
            {"Spare Terrace", ["{0} realized why you don't take out supporting walls."]},
            {"Spare Veranda", ["{0} realized why you don't take out supporting walls."]},
            {"Speakeasy", ["{0} drank too much moonshine.", "{0} wasn't able to figure out basic addition."]},
            {"Starfish Aquarium", ["Is this {0}? No, this is Patrick."]},
            {"Storeroom", ["+1 Key, +1 Gem, +1 Coin, +1 Dead {0}."]},
            {"Study", ["{0} has hit the books."]},
            {"Terrace", ["{0} met a Terrace-ble fate.", "{0} was torn from us in the Terrace."]},
            {"The Armory", ["{0} got disarmed in the Armory.", "{0} is saying knight knight.", "{0} got axed."]},
            {"The Grounds", ["{0} failed their out of Grounds.", "{0} got grounded."]},
            {"The Kennel", ["{0} spent the rest of the day petting the dogs.", "{0} went out looking for Carter.", "{0} went out looking for Thoughtless", "{0} went out looking for Hot Sauce.", "{0} went out looking for Drawbridge.", "{0} is dogsitting Carter, Thoughtless, Hot Sauce, and Drawbridge."]},
            {"The Pool", ["{0} was removed from the estate for running in The Pool.", "{0} forgot their swim trunks."]},
            {"The Underpass", ["{0} collapsed after walking for miles in The Underpass.", "{0} accidentally locked the padlock in the underpass."]},
            {"Throne Room", ["{0} descended the throne in the Throne Room.", "{0} was executed for high treason."]},
            {"Tomb", ["{0} followed in Herbert's footsteps.", "{0} found the definitive Dead End."]},
            {"Tool Shed", ["{0} shed their remaining steps in the Toolshed.", "{0} got lost looking for the Secret Garden Key."]},
            {"Trading Post", ["{0} was traded for 3 Ivory Dice.", "{0} got a little too close to dynamite."]},
            {"Treasure Trove", ["The true death of {0} is in the room but not in the house.", "{0} got lost looking for the true treasure of the trove.", "A deathlink from {0} is not the true treasure of the trove."]},
            {"Trophy Room", ["{0} atrophied.", "{0} got a posthumous participation trophy."]},
            {"Tunnel", ["{0} is still drafting Tunnels.", "{0} got tunnel vision.", "{0} has found the light at the end of the Tunnel."]},
            {"Unknown", ["{0} ventured to parts Unknown and was never seen again.", "There's a thousand reasons {0} should call it a day."]},
            {"Utility Closet", ["{0} got their wires crossed in the Utility Closet.", "{0} blew a fuse in the Utility Closet."]},
            {"Vault", ["{0} found out what happens when you get locked inside the Vault.", "{0} got arrested for robbery.", "{0} MAIL TOES POOL PEEK MAID POOL"]},
            {"Veranda", ["{0} still wasn't lucky enough to make it.", "Greater chance of killing {0} in Green Rooms.", "{0} got ran over by Dave."]},
            {"Vestibule", ["The Vestibule's doors jammed with {0} inside.", "{0} found all four doors stuck shut in the Vestibule."]},
            {"Walk-In Closet", ["{0} never walked out of the Walk-In Closet.", "{0} learned why it wasn't called a \"Walk-Out Closet\"."]},
            {"Weight Room", ["{0} isn't pulling their weight.", "{0} weighed too long in the wait room.", "{0} didn't have a spotter in the Weight Room."]},
            {"Well", ["{0} forgot to toss a coin in the well.", "{0} fell in the well."]},
            {"West Path", ["{0} got distracted by a birdbath.", "{0} passed the river."]},
            {"West Wing Hall", ["{0} in the West Wing are more likely to be dead.", "{0} really went downhill after Sorkin left."]},
            {"Wine Cellar", ["{0} got hangover from too much drinking.", "{0} found out why the wine in the Wine Cellar is red."]},
            {"Workshop", ["{0} tried assembling the 9th contraption in the Workshop.", "{0} tried to combine a Watering Can with a Battery Pack.", "{0} learned the hard way why there's no Power Watering Can"]},
        };
        private static Dictionary<string, string[]> _DeathLinkSpoilerMessageDict = new Dictionary<string, string[]>()
        {
            {"Apple Orchard", ["{0} scorched a bit more than the sundial."]},
            {"Boudoir", ["{0} went to bed early for Christmas."]},
            {"Clock Tower", ["At the sacred hour, the bells tolled for {0}.", "{0} is still waiting for the Sacred Hour."]},
            {"Inner Sanctum", ["{0} failed under a Rogue Moon"]},
            {"Precipice", ["{0} got checkmated in the Precipice."]}, //TODO get Chess type: "{0} forgot they needed a [knight/bishop/rook]."
            {"Room 46", ["{0} fell down and broke their crown."]},
            {"Room 8", ["{0} failed to properly ruminate in Room 8.,"]},
            {"Study", ["Queen to D8. Checkmate, {0}."]},

        };
        public static Dictionary<string, string[]> DeathLinkMsgDict { get; private set; } = new Dictionary<string, string[]>();

        public static void Initialize() {
            if (SpoilersEnabled) {
                EnableSpoilers();
                return;
            }
            DeathLinkMsgDict = _DeathLinkMessageDict;
        }
        public static bool SpoilersEnabled = false;

        public static void EnableSpoilers()
        {
            if (!SpoilersEnabled)
            {
                DeathLinkMsgDict = new();
                foreach (string key in _DeathLinkMessageDict.Keys)
                {
                    if (_DeathLinkSpoilerMessageDict.ContainsKey(key))
                    {
                        DeathLinkMsgDict[key] = [.. _DeathLinkMessageDict[key], .. _DeathLinkSpoilerMessageDict[key]];
                    }
                    else {
                        DeathLinkMsgDict[key] = _DeathLinkMessageDict[key];
                    }
                }
                SpoilersEnabled |= true;
            }
        }
        public static void DisableSpoilers()
        {
            if (SpoilersEnabled)
            {
                DeathLinkMsgDict = _DeathLinkMessageDict;
            }
            SpoilersEnabled |= false;
        }
    }
}
