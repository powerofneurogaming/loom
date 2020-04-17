using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZoneController : Singleton<BuildZoneController>
{
    [SerializeField] int height = 10;

    public Transform referenceBlock;

    public List<SquareController>[] buildGrid;
    public Vector3[][,] GridLocation = new Vector3[2][,];

    public GameObject goldPrefab;
    public GameObject DropZonePrefab;
    public Transform DropZoneParent;
    float scale = 1; //current scale, will grab later from wall size if size need multiple scaling

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        
    }
  
    public void ConstructSection(int size, GameObject pos, int player)
    {
        //Initialize the build zone's array of square lists
        buildGrid = new List<SquareController>[size];
        for (int i = 0; i < size; i++)
        {
            buildGrid[i] = new List<SquareController>();
        }

        height = size;

        Vector3 refPos = pos.transform.position;
        GridLocation[player] = new Vector3[size, size + 1];
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j <= size; j++)
            {
                GridLocation[player][i,j] = (refPos + pos.transform.right * i * scale + pos.transform.up * j * scale);
            }
            var obj = Instantiate(DropZonePrefab, refPos + pos.transform.right * i * scale + pos.transform.up * size * scale, pos.transform.rotation);
            obj.transform.SetParent(pos.transform);
        }
    }

    //Attempts to add a given square to a column in the build zone, returns true if the square was properly added, false if there is no space in the column
    public bool AddSquare(int col, SquareController square, int player)
    {
        int count = buildGrid[col].Count;
        //Check if a half Gold block pairs with a priorly dropped half gold block in the same col
        if (count > 0 && GoldMatch(square, buildGrid[col][count - 1]))
        {
            int gHeight = count - 1;
            while (gHeight > 0 && GoldMatch(square, buildGrid[col][gHeight - 1]))
            {
                gHeight--;
            }
            //Link both halves to each other for scoring and falling purposes
            buildGrid[col][gHeight].pair = square;
            square.pair = buildGrid[col][gHeight];
            square.transform.position = GridLocation[0][col, height];
            square.SetZone(true, "Build", GridLocation[0][col, gHeight], col, gHeight);

            if (Manager.NumPlayers == 2)
            {
                square.mirror = PlayZoneController.instance.GetMirror(square);
                square.mirror.transform.position = GridLocation[1][col, height];
                square.mirror.SetZone(true, "Build", GridLocation[1][col, gHeight], col, gHeight);
            }
            AnalyticsManager.instance.FillEventLog("Block Dropped In Drop Zone", player, square);
            return true;
        }
        //Checks to see if the column is filled
        if (count < height)
        {
            //Tells the square it is now in the "Build" zone and which column and row it will end up in
            square.transform.position = GridLocation[0][col, height];
            square.SetZone(true, "Build", GridLocation[0][col, count], col, count);
            buildGrid[col].Add(square);

            if (Manager.NumPlayers == 2)
            {
                square.mirror = PlayZoneController.instance.GetMirror(square);
                square.mirror.transform.position = GridLocation[1][col, height];
                square.mirror.SetZone(true, "Build", GridLocation[1][col, count], col, count);
            }
            AnalyticsManager.instance.FillEventLog("Block Dropped In Drop Zone", player, square);
            return true;
        }
        //If it reaches this point without returning, the column is full, it can't fit any more blocks
        return false;
    }

    //Check if to see if 2 blocktypes make two halves of a gold
    public bool GoldMatch(SquareController a, SquareController b)
    {
        return (a.pair == null && b.pair == null) && ((a.tag == "left gold cube" && b.tag == "right gold cube") || (a.tag == "right gold cube" && b.tag == "left gold cube"));
    }

    public void GoldMerge(int row, int col)
    {
        SquareController goldHalf = buildGrid[col][row];
        buildGrid[col][row] = PlayZoneController.instance.GetGoldMerge(buildGrid[col][row]);
        buildGrid[col][row].transform.position = goldHalf.transform.position;
        buildGrid[col][row].SetZone(false, "Build", GridLocation[0][col, row], col, row);

        buildGrid[col][row].mirror = PlayZoneController.instance.GetMirror(buildGrid[col][row]);
        buildGrid[col][row].mirror.transform.position = goldHalf.mirror.transform.position;
        buildGrid[col][row].mirror.SetZone(false, "Build", GridLocation[1][col, row], col, row);

        PlayZoneController.instance.RecycleBlock(goldHalf.pair.mirror);
        PlayZoneController.instance.RecycleBlock(goldHalf.mirror);

        PlayZoneController.instance.RecycleBlock(goldHalf.pair);
        PlayZoneController.instance.RecycleBlock(goldHalf);
    }

    //Attempts to remove the block from the buildZone, returns true if the square is properly removed, false if it can't find it to remove
    public bool RemoveSquare(int col, int row, int type)
    {
        //Checks to make sure it has a proper row and/or column
        if (row < 0 || col < 0)
        {
            return false;
        }

        SquareController square = buildGrid[col][row];

        //Checks to see if it is a goldHalf with a pair, removes just the one half and makes sure the block
        if (square.pair != null)
        {
            //Checks if buildGrid square matches type with block to be deleted, if it does it swaps it out with it's pair
            if (square.type == type)
            {
                square = square.pair;
                buildGrid[col][row] = square;
            }
            // Removes reference to soon to be deleted block
            buildGrid[col][row].pair = null;

            //Create h val to scroll up through blocks
            int h = row + 1;
            //Checks for any pairs above to transition pairs down
            while (h < buildGrid[col].Count && buildGrid[col][h].pair != null)
            {
                //makes sure that any swapped "pair" references don't mess up the buildGrid locations
                if (buildGrid[col][h].type != type)
                {
                    buildGrid[col][h] = buildGrid[col][h].pair;
                }
                //Switches above gold half to match below gold match, and remove the above pairing link
                buildGrid[col][h - 1].pair = buildGrid[col][h].pair;
                buildGrid[col][h - 1].pair.pair = buildGrid[col][h - 1];
                buildGrid[col][h].pair = null;

                buildGrid[col][h - 1].pair.UnderGone();
                h++;
            }

            //If there is at any point a matching gold half without a pair, pair it with the last half, remove it from the grid, and let it reach the regular block fall loop
            if (h < buildGrid[col].Count && (type == 3 && buildGrid[col][h].type == 3) || (type == 4 && buildGrid[col][h].type == 4))
            {
                buildGrid[col][h - 1].pair = buildGrid[col][h];
                buildGrid[col][h - 1].pair.pair = buildGrid[col][h - 1];
                buildGrid[col].Remove(buildGrid[col][h]);

                buildGrid[col][h - 1].pair.UnderGone();
                row = h;
            }
            else
            {
                //If this is reached, it means that no more blocks need to fall
                return true;
            }
        }
        //Otherwise regularly removes block and tells all blocks above it to fall
        else
        {
            buildGrid[col].Remove(square);
        }

        for (int j = row; j < buildGrid[col].Count; j++)
        {
            //If a block has a matching square tell it to fall as well
            if (buildGrid[col][j].pair != null)
            {
                buildGrid[col][j].pair.UnderGone();
            }
            buildGrid[col][j].UnderGone();
        }
        return true;
    }
}
