/*
* Copyright (c) 2006, Jonas Beckeman
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of Jonas Beckeman nor the names of its contributors
*       may be used to endorse or promote products derived from this software
*       without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY JONAS BECKEMAN AND CONTRIBUTORS ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL JONAS BECKEMAN AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* HEADER_END*/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

namespace PhotoshopFile.Text
{

    public enum EffectTypes
    {
        cmns , dsdw , isdw , oglw , iglw , bevl , sofi
    }

    public static class EffectLayerFactory
    {
        public static LayerInfo Load(PsdBinaryReader reader)
        {
            var signature = reader.ReadAsciiChars(4);
            if (signature != "8BIM")
                throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");

            var key = reader.ReadAsciiChars(4);
            var length = reader.ReadInt32();
//            var startPosition = reader.BaseStream.Position;

            UnityEngine.Debug.Log("EffectLayerFactory key:" + key);

            LayerInfo result;
            switch (key)
            {
                case "iglw":
                case "oglw":
                    result = new GlowEffect(reader, key);
                    break;
                case "dsdw":
                case "isdw":
                    result = new ShadowEffect(reader, key);
                    break;
                case "bevl":
                    result = new BevelEffect(reader , key);
                    break;
                default:
                    result = new RawLayerInfo(reader, key, (int)length);
                    break;
            }

            return result;
        }
    }


}
