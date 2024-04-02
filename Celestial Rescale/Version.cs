using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version version = new System.Version("0.3.1");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + "[SpaceNerd24]" + " Your Current Version: " + version);
        }
    }
}
