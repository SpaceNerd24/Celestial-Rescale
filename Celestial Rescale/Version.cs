using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version verson = new System.Version("0.2.0");

        public void Awake()
        {
            Debug.Log("[CelestialRescale]" + "[SpaceNerd24]" + " Your Current Version: " + verson);
        }
    }
}
