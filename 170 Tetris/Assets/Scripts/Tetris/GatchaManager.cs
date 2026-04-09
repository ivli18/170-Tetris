using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "GatchaManager", menuName = "Scriptable Objects/GatchaManager")]
public class GatchaManager : ScriptableObject
{
    public PieceCollection[] GatchaTableFull;
    private PieceCollection[] GatchaTableAvaliable;

    public int[] baseOddsTable = { 100, 80, 60, 40, 20, 10, 1 };

    public void ResetAvailiablePieces()
    {
        GatchaTableAvaliable = GatchaTableFull;
    }

    public PieceData RollGatcha(int[] oddsTable, bool repeats)
    {
        PieceData piece = null;
        while (piece == null)
        {
            // Choose a table to pull from based on the oddsTable weights
            int oddsSum = 0;
            foreach (int i in oddsTable)
            {
                oddsSum += i;
            }
            int rnd = UnityEngine.Random.Range(0, oddsSum);
            int tableToPull = 0;
            for (int i = 0; i < oddsTable.Length; i++)
            {
                if (rnd < oddsTable[i])
                {
                    tableToPull = i;
                    break;
                }
                rnd -= oddsTable[i];
            }

            // See if the table is empty, will automatically reroll if so (no failsafe is currently implemented to prevent loops)
            if (GatchaTableAvaliable[tableToPull].pieces.Count != 0)
            {
                // Pull a random piece from the chosen table, then remove it
                int randPiece = UnityEngine.Random.Range(0, GatchaTableAvaliable[tableToPull].pieces.Count);
                piece = GatchaTableAvaliable[tableToPull].pieces[randPiece];
                GatchaTableAvaliable[tableToPull].pieces.RemoveAt(randPiece);
            }
        }

        return piece;
    }

    public PieceData RollGatcha()
    {
        return RollGatcha(baseOddsTable, false);
    }

    public PieceCollection[] GetGatchaTableAvaliable()
    {
        return GatchaTableAvaliable;
    }
}

[System.Serializable]
public class PieceCollection
{
    [SerializeField]
    public List<PieceData> pieces;
}
