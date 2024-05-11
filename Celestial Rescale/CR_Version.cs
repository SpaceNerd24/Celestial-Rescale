using UnityEngine;
using Celestial_Rescale.API;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class CR_Version : MonoBehaviour
    {
        public static readonly System.Version version = new System.Version("0.3.2");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + "[SpaceNerd24]" + " Your Current Version: " + version);
        }
    }
}
