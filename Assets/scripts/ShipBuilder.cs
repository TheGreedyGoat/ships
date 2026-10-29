using System.IO.IsolatedStorage;
using UnityEngine;

public class ShipBuilder : MonoBehaviour
{

    [SerializeField] GameObject[] topPrefabs;
    [SerializeField] GameObject[] CannonDeckPrefabs;
    [SerializeField] GameObject[] bottomPrefabs;

    [SerializeField] int numCannonDecks;
    [SerializeField] int numCannonsPerDeck;

    Transform shipParent;

    [Header("Offsets")]
    [SerializeField] float cannonDeckHeight = 1;
    [SerializeField] float segmentWidth = 1;


    public void BuildShip()
    {
        if (shipParent != null)
        {
            DestroyImmediate(shipParent.gameObject);
        }
        shipParent = new GameObject("Ship Parent").transform;

        Vector3 segmentsStartZ = new(0, 0, segmentWidth * 0.5f);
        Vector3 zOffsetPerSegment = Vector3.forward * segmentWidth;
        Vector3 frontPartsZPosition = zOffsetPerSegment * numCannonsPerDeck;


        Vector3 cannonDeckStartY = Vector3.up * cannonDeckHeight * 0.5f;
        Vector3 yOffsetPerDeck = cannonDeckHeight * Vector3.up;
        Vector3 topPartsYPosition = yOffsetPerDeck * numCannonDecks;

        Transform bottom = new GameObject("Ship Bottom").transform;
        bottom.parent = shipParent;
        bottom.localPosition = Vector3.zero;

        GameObject bottomTail = Instantiate(bottomPrefabs[0], parent: bottom);
        GameObject bottomFront = Instantiate(bottomPrefabs[2], parent: bottom, position: frontPartsZPosition, rotation: Quaternion.identity);


        Transform top = new GameObject("Ship Top").transform;
        top.parent = shipParent;
        top.localPosition = Vector3.up * cannonDeckHeight * numCannonDecks;


        GameObject topTail = Instantiate(topPrefabs[0], parent: top, position: topPartsYPosition, rotation: Quaternion.identity);
        GameObject topFront = Instantiate(topPrefabs[2], parent: top, position: frontPartsZPosition + topPartsYPosition, rotation: Quaternion.identity);



        for (int j = 0; j < numCannonsPerDeck; j++)
        {
            GameObject bottomSegment = Instantiate(bottomPrefabs[1], position: segmentsStartZ + zOffsetPerSegment * j, parent: bottom, rotation: Quaternion.identity);
            GameObject topSegment = Instantiate(topPrefabs[1], position: segmentsStartZ + zOffsetPerSegment * j + topPartsYPosition, parent: bottom, rotation: Quaternion.identity);
        }

        Transform[] cannonDecks = new Transform[numCannonDecks];
        for (int i = 0; i < numCannonDecks; i++)
        {
            Vector3 currentDeckHeight = cannonDeckStartY + yOffsetPerDeck * i;

            Transform currentDeck = new GameObject($"Cannon Deck {i}").transform;
            cannonDecks[i] = currentDeck;
            currentDeck.parent = shipParent;
            currentDeck.localPosition = currentDeckHeight;

            GameObject tail = Instantiate(CannonDeckPrefabs[0], position: currentDeckHeight, parent: currentDeck, rotation: Quaternion.identity);
            tail.name = $"Tail CannonDeck {i}";
            for (int j = 0; j < numCannonsPerDeck; j++)
            {
                GameObject segment = Instantiate(CannonDeckPrefabs[1], position: currentDeckHeight + segmentsStartZ + zOffsetPerSegment * j, parent: currentDeck, rotation: Quaternion.identity);
            }

            GameObject front = Instantiate(CannonDeckPrefabs[2], position: currentDeckHeight + zOffsetPerSegment * numCannonsPerDeck, parent: currentDeck, rotation: Quaternion.identity);
            front.name = $"front CannonDeck {i}";
        }






    }
}
