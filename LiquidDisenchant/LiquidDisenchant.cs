/*
 * LiquidDisenchant v3.0.1.8 by LiquidAtoR
 * Additional contributors: CarlMGregory
 * Additional Code Supply: Hawker, FPSWare
 *
 * Sorry for not keeping a changelog with this plugin.
 * Must've escaped my attention while updating.
 *
 * This plugin disenchants items in your inventory.
 * It will only DE uncommon, non soulbound items!
 * If you want it to DE other stuff read the forum for needed changes.
 *
 * 2013/29/06   v3.0.1.8
 *               Unified the code to work with all languages.
 *               Shortened waiting times, and added some more Archaeology fragments to ignore.
 *
 * 2012/xx/11   v3.0.1.7
 *               Forgot what I did, haha.
 * 2012/xx/11   v3.0.1.6
 *               Added a few more lockboxes and enchanting mats to the ignore lists.
 * 2012/xx/11   v3.0.1.5
 *               Added Lockboxes to the ignore lists.
 *              v3.0.1.4
 *               Mixed up local copy with actual release file (local copy DE'd blue items, release should not do this).
 *              v3.0.1.3
 *               Another change in the API on bot/player state changed in the plugin file.
 *              v3.0.1.2
 *               Temporary commented out the Styx.CommonBot.LootTargeting.LootMobs because it always returns true.
 *              v3.0.1.1
 *               Removed the references to Styx.Combat as they have been removed from the API and caused compiler errors on startup of HB.
 *              v3.0.0.0
 *               Revised last bits to make it fully functional with 2.5.6187.420.
 *              v2.5.0.1
 *               Revised inventory checkup to exclude bankitems and added cataclysm enchanting mats to the ignore list. Compatible with 2.0.0.3895.
 *              v2.1.0.6
 *               Revised last bits to make it fully functional with 2.0.0.3120.
 *              v2.0.0.5
 *               Revised last bits to make it fully functional with 1.9.4.5.
 *              v2.0.0.4
 *               Revised last bits to make it fully functional with HB2.
 *              v2.0.0.3
 *               Ironing out some more problems with HB2.
 *              v2.0.0.2
 *               No more Compiler Errors.
 *              v2.0.0.1
 *                Initial Public Release.
 */

namespace PluginLiquidDisenchant3
{
    using Styx;
	using Styx.Common;
	using Styx.Common.Helpers;
	using Styx.CommonBot;
	using Styx.CommonBot.Frames;
	using Styx.CommonBot.Inventory;
	using Styx.CommonBot.Profiles;
	using Styx.Helpers;
    using Styx.Pathing;
    using Styx.Plugins;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
	using System.Windows.Media;
    using System.Xml.Linq;

	//Special Thanks to CarlMGregory for making it work better

    public class LiquidDisenchant3 : HBPlugin
    {
        public override string Name { get { return "LiquidDisenchant 3.0"; } }
        public override string Author { get { return "LiquidAtoR"; } }
        public override Version Version { get { return new Version(3,0,1,8); } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return "Disenchant"; } }
        private Thread deThread = null;
		private bool _init;
		
        public override void Initialize()
        {
            if (_init) return;
            base.OnEnable();
            Logging.Write(LogLevel.Normal, Colors.DarkRed, "LiquidDisenchant 3.0 ready for use...");
            _init = true;
        }
		
		#region When NOT to disenchant
		
        public override void Pulse()
        {
		if (_init)
			{
			//Thanks FPSWare for his plugins for Hunter and Druid, code from there,
			//Don't start disenchanting when the following things occur,
            if (!SpellManager.HasSpell(13262) ||
				Battlegrounds.IsInsideBattleground || 
					StyxWoW.Me.IsActuallyInCombat || 
						StyxWoW.Me.Mounted || 
							StyxWoW.Me.IsDead || 
								StyxWoW.Me.IsGhost)
			{ 
				return;
				}
			}
            DisenchantItems();
        }

		//Revised by CarlMGregory
        public override void OnButtonPress()
		{
			for (int a = alreadyDisenchanted.Count-1; a >= 0; a--)
				{
					alreadyDisenchanted.RemoveAt(a);
				}
            DisenchantItems();
        }
		#endregion
		
        #region Disenchanting
		//Shamelessly ripped from Hawker's Demonlock code,
		//I had to dig deep into my archives for this lock's CC,
		//But I knew the disenchant code was in there.
		
        private static List<WoWItem> alreadyDisenchanted = new List<WoWItem>();

        private List<uint> ignoreItems = new List<uint>() {
            // mats
            10940, 10938, 10939, 10998, 10978, 11082, 11083, 11084, 11134, 11139, 11174, 11175,
            11176, 11177, 11178, 14343, 14344, 16202, 16203, 16204, 20725, 22445, 22446, 22447,
            22448, 22449, 22450, 34052, 34053, 34054, 34055, 34056, 34057, 46849, 49649, 52718,
			52719, 74250,

            // rods
            44452,

            // lockboxes
            4632, 4633, 4634, 4636, 4637, 4638, 5758, 5759, 5760, 31952, 43622, 68729, 88567,

            // healthstones
            5509, 5510, 5511, 5512, 9421, 19004, 19005, 19006,  
            19007, 19008, 19009, 19010, 19011, 19012, 19013, 
            22103, 22104, 22105, 36889, 36890, 36891, 36892,
            36893, 36894,

            // soulstones
			5232, 16892, 16893, 16895, 16896, 22116, 36895,

            // firestones
			40773, 41169, 41170, 41171, 41172, 41173, 41174,

            // spellstones
            41191, 41192, 41193, 41194, 41195, 41196,
			
			// archaeology keystones
			52843, 63127, 63128, 64392, 64394, 64395, 64396, 64397, 79868, 79869, 95373,
        };

        private void DisenchantItems()
        {
            List<WoWItem> targetItems = ObjectManager.GetObjectsOfType<WoWItem>(false);
			
            for (int a = targetItems.Count-1; a >= 0; a--)
            {
                    if (ignoreItems.Contains(targetItems[a].Entry) || alreadyDisenchanted.Contains(targetItems[a]))
                    {
                        targetItems.RemoveAt(a);
                    }
                    else if 
						(targetItems[a].IsSoulbound || 
							targetItems[a].IsAccountBound || 
								ignoreItems.Contains(targetItems[a].Entry) || 
									targetItems[a].Quality == WoWItemQuality.Poor || 
										targetItems[a].Quality == WoWItemQuality.Common ||
											targetItems[a].Quality == WoWItemQuality.Rare ||
												targetItems[a].Quality == WoWItemQuality.Epic)
					{
						alreadyDisenchanted.Add(targetItems[a]);
						targetItems.RemoveAt(a);
					}
            }

                if (Equals(null, targetItems)) { return; }

                foreach (WoWItem deItem in targetItems)
                {
					if(deItem.BagSlot != -1)
					{
						Logging.Write(LogLevel.Normal, Colors.DarkRed, "[LiquidDisenchant]: {0} (Entry:{1}).", deItem.Name, deItem.Entry);
						SpellManager.Cast(13262);
						StyxWoW.SleepForLagDuration();
						Lua.DoString("UseItemByName(\"" + deItem.Name + "\")");
						StyxWoW.SleepForLagDuration();

                    while (StyxWoW.Me.IsCasting)
						{
							Thread.Sleep(250);
						}

                    Thread.Sleep(2000);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();
					
						if (!StyxWoW.Me.IsActuallyInCombat)
						{
							Thread.Sleep(1000);
							alreadyDisenchanted.Add(deItem);
						}
					}
                }
            return;
        }
        #endregion
    }
}