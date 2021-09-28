using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.CharacterSelection;

public class EmptySpawnSystem : PlayerSpawnSystem
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override Transform GetSpawnPosition(NetworkSelectableCharacterPlayer roomPlayer)
    {
        return null;
    }

    protected override GameObject GetPlayerPrefab(NetworkSelectableCharacterPlayer roomPlayer)
    {
        return null;
    }
}
