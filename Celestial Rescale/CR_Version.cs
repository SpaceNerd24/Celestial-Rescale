﻿using UnityEngine;

namespace CelestialRescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class CR_Version : MonoBehaviour
    {
        public static readonly System.Version version = new System.Version("0.3.4");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + " Your Current Version: " + version);
        }
    }
}
