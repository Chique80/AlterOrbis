using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.CharacterSelection;

namespace Mirror.Matchmaking
{
    public class LANMatchmakingManager : NetworkMatchmakingManager
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public override void CreateMatch()
        {
            NetworkCharacterSelectionManager.singleton.StartHost();
        }

        public override void SearchMatch(string matchName)
        {
            NetworkCharacterSelectionManager.singleton.StartClient();
        }
    }
}

