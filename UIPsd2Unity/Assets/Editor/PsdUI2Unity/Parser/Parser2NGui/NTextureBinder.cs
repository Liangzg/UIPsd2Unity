﻿using UnityEngine;

namespace subjectnerdagreement.psdexport
{
    public class NTextureBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            UITexture mTexture = LayerWordBinder.swapComponent<UITexture>(gObj);

        }
#endif
    }
}