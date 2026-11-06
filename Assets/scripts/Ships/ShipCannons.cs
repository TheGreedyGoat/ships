using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cannons2 : MonoBehaviour
{
    [SerializeField]
    Cannon[] cannons;

    int[] nextDecksToFire = { 0, 0 };

    int cannonDecks;
    int cannonCols;

    int Index(int side, int deck, int column) =>
        (side * cannonDecks + deck) * cannonCols + column;

    public void Setup(
        Vector3[,,] cannonPos,
        GameObject prefab,
        Transform parent
    )
    {
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        cannonDecks = cannonPos.GetLength(1);
        cannonCols = cannonPos.GetLength(2);
        cannons = new Cannon[2 * cannonDecks * cannonCols];
        for (int d = 0; d < cannonDecks; d++)
        {
            for (int c = 0; c < cannonCols; c++)
            {
                cannons[Index(0, d, c)] = Instantiate(
                        prefab,
                        transform.position + cannonPos[0, d, c],
                        Quaternion.Euler(new(0, 90, 0)),
                        transform
                    )
                    .GetComponent<Cannon>();

                cannons[Index(1, d, c)] = Instantiate(
                        prefab,
                        transform.position + cannonPos[1, d, c],
                        Quaternion.Euler(new(0, -90, 0)),
                        transform
                    )
                    .GetComponent<Cannon>();
            }
        }
    }

    public void FireNextDeck(int side)
    {
        Fire(side, nextDecksToFire[side]);
        nextDecksToFire[side]++;
        nextDecksToFire[side] %= cannonDecks;
    }

    public void ShowAim()
    {
        for (int s = 0; s < 2; s++)
        {
            for (int d = 0; d < cannonDecks; d++)
            {
                bool isNextToFire = nextDecksToFire[s] == d;
                for (int c = 0; c < cannonCols; c++)
                {
                    var cannon = cannons[Index(s, d, c)];
                    var reloadLeft = cannon.ReloadLeft;
                    // float t = Mathf.InverseLerp(Mathf.Max(reload, 0), 0, 5);
                    Vector3 start = cannon.transform.position;
                    Vector3 direction = -cannon.transform.forward;
                    Vector3 end = start + direction * 5;
                    Vector3 progress = end - direction * reloadLeft;

                    Debug.DrawLine(
                        start,
                        progress,
                        isNextToFire ? Color.lightGreen : Color.green
                    );
                    if (reloadLeft > 0)
                        Debug.DrawLine(
                            progress,
                            end,
                            isNextToFire ? Color.red : Color.softRed
                        );
                }
            }
        }
    }

    public void Fire(int side, int deck)
    {
        if (side != 0 && side != 1)
        {
            Debug.LogWarning(
                "Side parameter to fire has to be 0 (left) or 1 (right)"
            );
            return;
        }
        if (deck < 0 || deck >= cannonDecks)
        {
            Debug.LogWarning(
                $"This ship has {cannonDecks} decks. deck index must be les than that and >= 0"
            );
            return;
        }

        for (int c = 0; c < cannonCols; c++)
        {
            cannons[Index(side, deck, c)].Fire();
        }
    }
}
