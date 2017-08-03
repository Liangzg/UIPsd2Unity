/* Copyright (c) 2017 LiangZG. See license.txt for your rights */

using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    public class GradientEffect : EffectBase
    {

        public Color TopColor { get; private set; }

        public Color BottomColor { get; private set; }

        public void ParseDescriptor(DynVal gradDyn)
        {
            DynVal cols = gradDyn.FindDynVal("Clrs");
            parseColors(cols);
        }


        private void parseColors(DynVal cols)
        {
            DynVal topDyn = cols.Children[cols.Children.Count - 1].FindDynVal("RGBC");
            TopColor = getColor(topDyn);

            DynVal bottomDyn = cols.Children[0].FindDynVal("RGBC");
            BottomColor = getColor(bottomDyn);
        }

        private Color getColor(DynVal dv)
        {
            if(dv == null)  return Color.black;

            int r = (int)(double)dv.FindDynVal("Rd").Value;
            int g = (int)(double)dv.FindDynVal("Grn").Value;
            int b = (int)(double)dv.FindDynVal("Bl").Value;
            return Util.FromArgb(r, g, b);
        }
    }
}