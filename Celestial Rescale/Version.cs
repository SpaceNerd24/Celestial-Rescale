using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version number = new System.Version("0.1.1");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + " [" + number + "] "+ " Your Current Version: " + number);
        }
    }
}
