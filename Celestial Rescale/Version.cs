using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version number = new System.Version("0.1.3");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + "[SpaceNerd24]" + " Your Current Version: " + number);
        }
    }
}
