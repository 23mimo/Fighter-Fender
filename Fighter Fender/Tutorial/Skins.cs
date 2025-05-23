using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Tutorial
{
    public class Skins
    {
        public List<Texture2D> AvailableSkins { get; private set; } = new List<Texture2D>();
        public int SelectedSkinIndex { get; private set; } = 0;

        public Texture2D CurrentSkin
        {
            get
            {
                if (AvailableSkins.Count == 0) return null;
                return AvailableSkins[SelectedSkinIndex];
            }
        }

        public void AddSkin(Texture2D texture)
        {
            AvailableSkins.Add(texture);
        }

        public void SelectNextSkin()
        {
            if (AvailableSkins.Count == 0) return;
            SelectedSkinIndex = (SelectedSkinIndex + 1) % AvailableSkins.Count;
        }

        public void SelectPreviousSkin()
        {
            if (AvailableSkins.Count == 0) return;
            SelectedSkinIndex = (SelectedSkinIndex - 1 + AvailableSkins.Count) % AvailableSkins.Count;
        }

        public void SetSkin(int index)
        {
            if (index >= 0 && index < AvailableSkins.Count)
                SelectedSkinIndex = index;
        }
    }
}
