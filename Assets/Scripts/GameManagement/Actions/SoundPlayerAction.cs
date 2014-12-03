using UnityEngine;
using System.Collections;

namespace DogFighter
{
    public class SoundPlayerAction : Action
    {
		public AudioClip explosion;
		public AudioClip flareLaunch;
		public AudioClip laserShot;
		public AudioClip rocketFire;
		public AudioClip turboBoost;
		public AudioClip warning;

        public override void ActionStart()
        {
            // do nothing
        }

        public override void ActionUpdate()
        {
            // do nothing
        }
        
        public override void ActionFixedUpdate()
        {
            // do nothing
        }
        
        public override void ActionOnGUI()
        {
            // do nothing
        }
        
        public override void ReceiveMessage(Action action, string message)
        {
            string[] messageTokens = message.Split(' ');
            
            switch (messageTokens[0])
            {
            case "play":
                switch (messageTokens[1])
                {
                case "explosion":
                    AudioSource.PlayClipAtPoint(explosion, Vector3.zero);
                    break;
                case "flareLaunch":
                    AudioSource.PlayClipAtPoint(flareLaunch, Vector3.zero);
                    break;
                case "laserShot":
                    AudioSource.PlayClipAtPoint(laserShot, Vector3.zero);
                    break;
                case "rocketFire":
                    AudioSource.PlayClipAtPoint(rocketFire, Vector3.zero);
                    break;
                case "turboBoost":
                    AudioSource.PlayClipAtPoint(turboBoost, Vector3.zero);
                    break;
                case "warning":
                    AudioSource.PlayClipAtPoint(warning, Vector3.zero);
                    break;
                }
                break;
            }
        }
    }
}

