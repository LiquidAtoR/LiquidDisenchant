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
        public override string Name { get { return "Liquid Disenchant 3.0"; } }
        public override string Author { get { return "LiquidAtoR"; } }
        public override Version Version { get { return new Version(3,0,1,5); } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return "Disenchant"; } }
        private Thread deThread = null;

		#region When NOT to disenchant
		
        public override void Pulse()
        {
		//Thanks FPSWare for his plugins for Hunter and Druid, code from there,
		//Don't start disenchanting when the following things occur,
            if (!SpellManager.HasSpell("Disenchant") ||
				Battlegrounds.IsInsideBattleground || 
					StyxWoW.Me.IsActuallyInCombat || 
						StyxWoW.Me.Mounted || 
							StyxWoW.Me.IsDead || 
								StyxWoW.Me.IsGhost)
			{ 
			return; 
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
			52719,

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
						Lua.DoString("CastSpellByName(\"Disenchant\")");
						StyxWoW.SleepForLagDuration();
						Lua.DoString("UseItemByName(\"" + deItem.Name + "\")");
						StyxWoW.SleepForLagDuration();

                    while (StyxWoW.Me.IsCasting)
						{
							Thread.Sleep(250);
						}

                    Thread.Sleep(2500);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();
					
						if (!StyxWoW.Me.IsActuallyInCombat)
						{
							Thread.Sleep(1500);
							alreadyDisenchanted.Add(deItem);
						}
					}
                }
            return;
        }
        #endregion
    }
}