using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

using Pathfinder;

using BepInEx;
using BepInEx.Hacknet;

using HarmonyLib;
using Hacknet.Screens;
using Hacknet.Extensions;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using Hacknet.Gui;
using Pathfinder.GUI;
using Microsoft.Xna.Framework;

namespace ExtensionSearch
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class ExtSearchPlugin : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.extsearch";
        public const string ModName = "Extension Search Bar";
        public const string ModVer = "1.0.0";

        public override bool Load()
        {
            HarmonyInstance.PatchAll(typeof(ExtSearchPlugin).Assembly);

            return true;
        }
    }

    [HarmonyPatch]
    public class ExtSearchPatch
    {
        private static ReadOnlyCollection<ExtensionInfo> loadedExtensions;
        private static List<ExtensionInfo> filteredExtensions = new List<ExtensionInfo>();

        private static string filter = "";

        private static readonly SpriteFont font = GuiData.font;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtensionsMenuScreen),nameof(ExtensionsMenuScreen.LoadExtensions))]
        public static void GetLoadedExtensions(ExtensionsMenuScreen __instance)
        {
            loadedExtensions = new ReadOnlyCollection<ExtensionInfo>(__instance.Extensions);
            filteredExtensions = __instance.Extensions;
        }

        private static Rectangle bounds;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtensionsMenuScreen),nameof(ExtensionsMenuScreen.Draw))]
        public static void GrabDrawBoundsForExtensionsMenu(Rectangle dest)
        {
            bounds = dest;
        }

        private static int searchBoxID = PFButton.GetNextID();
        private static readonly Vector2 extTitle = font.MeasureString("E X T E N S I O N S");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtensionsMenuScreen),nameof(ExtensionsMenuScreen.DrawExtensionList))]
        public static bool DrawNewExtensionList(ExtensionsMenuScreen __instance)
        {
            if(!__instance.HasLoaded) { return true; }
            if(!loadedExtensions.Any()) { return true; }

            Vector2 searchBoxOffset = new Vector2(bounds.X + extTitle.X + 25f, bounds.Y + 50f);
            filter = TextBox.doTextBox(searchBoxID, (int)searchBoxOffset.X, (int)searchBoxOffset.Y,
                350, 1, filter, GuiData.smallfont);

            if (filter.IsNullOrWhiteSpace()) {
                __instance.Extensions = loadedExtensions.ToList();
                return true;
            }

            var searchedExts = loadedExtensions.Where(ext => ext.Name.ToLower().Contains(filter));
            if(!searchedExts.Any())
            {
                TextItem.doLabel(new Vector2(bounds.X, bounds.Y + extTitle.Y + 30f),
                    "No Extensions Found with Current Filter :(", Color.White);
                return false;
            }

            filteredExtensions = searchedExts.ToList();
            __instance.Extensions = filteredExtensions;

            return true;
        }
    }
}
