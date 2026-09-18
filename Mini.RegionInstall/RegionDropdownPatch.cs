// <copyright file="RegionDropdownPatch.cs" company="miniduikboot">
// This file is part of Mini.RegionInstaller.
//
// Mini.RegionInstaller is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Mini.RegionInstaller is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Mini.RegionInstaller.  If not, see https://www.gnu.org/licenses/
// </copyright>

namespace Mini.RegionInstall
{
    using System.Linq;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patch for <see cref="ServerDropdown"/> to format and display custom regions in a grid layout.
    /// </summary>
    [HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    public static class RegionDropdownPatch
    {
        /// <summary>
        /// Prefix patch to override server dropdown option population and arrange buttons in columns.
        /// </summary>
        /// <param name="__instance">The server dropdown instance.</param>
        /// <returns>False to skip the original method execution.</returns>
        [HarmonyPrefix]
        public static bool Prefix(ServerDropdown __instance)
        {
            if (__instance == null || ServerManager.Instance?.AvailableRegions == null)
            {
                return true;
            }

            var regions = ServerManager.Instance.AvailableRegions.ToList();
            if (regions.Count == 0)
            {
                return true;
            }

            int maxPerColumn = 6;
            float columnWidth = 3.2f;
            float rowSpacing = 0.42f;

            int totalRegions = regions.Count;
            int totalColumns = Mathf.Max(1, Mathf.CeilToInt((float)totalRegions / maxPerColumn));
            int maxRows = Mathf.Min(totalRegions, maxPerColumn);

            float bgWidth = totalColumns > 1 ? (columnWidth * totalColumns) + 1.2f : 5.0f;
            float bgHeight = (maxRows * rowSpacing) + 0.9f;

            float startX = -0.1f; 
            float startY = __instance.y_posButton - 0.05f;

            if (__instance.background != null)
            {
                __instance.background.size = new Vector2(bgWidth, bgHeight);
                float xOffset = (totalColumns - 1) * (columnWidth / 2f);
                __instance.background.transform.localPosition = new Vector3(xOffset, __instance.initialYPos - ((bgHeight - 1.2f) / 2f), 0f);
            }

            int index = 0;
            foreach (var region in regions)
            {
                if (region == null)
                {
                    continue;
                }

                var button = __instance.ButtonPool.Get<ServerListButton>();
                if (button == null)
                {
                    continue;
                }

                int column = index / maxPerColumn;
                int row = index % maxPerColumn;

                float xPos = startX + (column * columnWidth);
                float yPos = startY - (row * rowSpacing);

                button.transform.localPosition = new Vector3(xPos, yPos, -1f);
                button.transform.localScale = Vector3.one;

                if (button.Text != null)
                {
                    button.Text.enableAutoSizing = false;
                    button.Text.fontSize = 3.5f;
                    button.Text.text = region.Name;
                    button.Text.ForceMeshUpdate(false, false);
                }

                if (button.Button != null)
                {
                    button.Button.OnClick.RemoveAllListeners();
                    IRegionInfo capturedRegion = region;
                    button.Button.OnClick.AddListener((System.Action)(() => __instance.ChooseOption(capturedRegion)));
                    __instance.controllerSelectable.Add(button.Button);
                }

                index++;
            }

            return false;
        }
    }
}