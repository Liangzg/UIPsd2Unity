using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace subjectnerdagreement.psdexport
{
    public interface IBinding
    {
        void StartBinding(GameObject gObj ,string args , string layerName);

        void ExitBinding(GameObject g, string args, string layerName);
    }


    public abstract class ABinder : IBinding
    {
        public virtual void StartBinding(GameObject gObj, string args, string layerName)
        {
        }

        public virtual void ExitBinding(GameObject g, string args, string layerName)
        {
        }
    }
}
