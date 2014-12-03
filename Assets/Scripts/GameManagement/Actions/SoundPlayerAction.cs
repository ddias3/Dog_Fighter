using UnityEngine;
using System.Collections;

namespace DogFighter
{
    public class SoundPlayerAction : Action
    {
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
                    }
                break;
            }
        }
    }
}

