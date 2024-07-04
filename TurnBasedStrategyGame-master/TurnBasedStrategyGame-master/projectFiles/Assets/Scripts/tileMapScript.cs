using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using static System.Collections.IEnumerable;
using System.Threading;

public class tileMapScript : MonoBehaviour
{
    //Reference holders for the other two scripts that are currently running
    //alongside this script
    [Header("Manager Scripts")]
    public battleManagerScript BMS;
    public gameManagerScript GMS;

    //List of tiles that are used to generate the map
    //Try chaging tilesTypes to enum later   
    [Header("Tiles")]
    public Tile[] tileTypes;
    public int[,] tiles;

    //This is used when the game starts and there are pre-existing units
    //It uses this variable to check if there are any units and then maps them to the proper tiles
    [Header("Units on the board")]
    public GameObject unitsOnBoard;

    //This 2d array is the list of tile gameObjects on the board
    public GameObject[,] tilesOnMap;

    //This 2d array is the list of quadUI gameObjects on the board
    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForUnitMovementDisplay;
    public GameObject[,] quadOnMapCursor;

    //public is only to set them in the inspector, if you change these to private then you will
    //need to re-enable them in the inspector
    //Game object that is used to overlay onto the tiles to show possible movement
    public GameObject mapUI;
    //Game object that is used to highlight the mouse location
    public GameObject mapCursorUI;
    //Game object that is used to highlight the path the unit is taking
    public GameObject mapUnitMovementUI;

    //Nodes along the path of shortest path from the pathfinding
    public List<Node> currentPath = null;

    //Node graph for pathfinding purposes
    public Node[,] graph;

    //containers (parent gameObjects) for the UI tiles
    [Header("Containers")]
    public GameObject tileContainer;
    public GameObject UIQuadPotentialMovesContainer;
    public GameObject UIQuadCursorContainer;
    public GameObject UIUnitMovementPathContainer;


    //Set in the inspector, might change this otherwise.
    //This is the map size (please put positive numbers it probably wont work well with negative numbers)
    [Header("Board Size")]
    public int mapSizeX;
    public int mapSizeY;

    //In the update() function mouse down raycast sets this unit
    [Header("Selected Unit Info")]
    public GameObject selectedUnit;
    //These two are set in the highlightUnitRange() function
    //They are used for other things as well, mainly to check for movement, or finalize function
    public HashSet<Node> selectedUnitTotalRange;
    public HashSet<Node> selectedUnitMoveRange;

    public bool unitSelected = false;

    public int unitSelectedPreviousX;
    public int unitSelectedPreviousY;

    public GameObject previousOccupiedTile;


    //public AudioSource selectedSound;
    //public AudioSource unselectedSound;
    //public area to set the material for the quad material for UI purposes
    [Header("Materials")]
    public Material greenUIMat;
    public Material redUIMat;
    public Material blueUIMat;

    // State untuk testing
    private int[][][][] test_state;
    private int[][][][] tempState;

    private int[][] map;

    private bool mustAttack = false;

    [Header("Scripts")]
    [SerializeField] tempTempMinimax tempMinMax;
    [SerializeField] SoundManager soundManager;
    private void Start()
    {
        tempState = init4dState();
        test_state = init4dState();
        //Get the battlemanager running
        //BMS = GetComponent<battleManagerScript>();
        //GMS = GetComponent<gameManagerScript>();
        //Generate the map info that will be used
        generateMapInfo();
        //Generate pathfinding graph
        generatePathFindingGraph();
        //With the generated info this function will read the info and produce the map
        generateMapVisuals();
        //Check if there are any pre-existing units on the board
        setIfTileIsOccupied();

        map = GridMovementCostTo2DArray();
    }

    private void Update()
    {
        //If input is left mouse down then select the unit
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedUnit == null)
            {
                //mouseClickToSelectUnit();
                mouseClickToSelectUnitV2();

            }
            //After a unit has been selected then if we get a mouse click, we need to check if the unit has entered the selection state (1) 'Selected'
            //Move the unit
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) && selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
            {


                if (selectTileToMoveTo())
                {
                    //selectedSound.Play();
                    Debug.Log("movement path has been located");
                    unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript>().x;
                    unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript>().y;
                    previousOccupiedTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
                    selectedUnit.GetComponent<UnitScript>().setWalkingAnimation();
                    moveUnit();

                    StartCoroutine(moveUnitAndFinalize());
                    //The moveUnit function calls a function on the unitScriptm when the movement is completed the finalization is called from that script.


                }

            }
            //Finalize the movement
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(2))
            {
                finalizeOption();
                GMS.endTurn();
            }

        }

        //Unselect unit with the right click
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit != null)
            {
                mustAttack = false;

                if (selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0 && selectedUnit.GetComponent<UnitScript>().combatQueue.Count == 0)
                {
                    if (selectedUnit.GetComponent<UnitScript>().unitMoveState != selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(3))
                    {
                        //unselectedSound.Play();
                        selectedUnit.GetComponent<UnitScript>().setIdleAnimation();
                        deselectUnit();
                    }
                }
                else if (selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 1)
                {
                    selectedUnit.GetComponent<UnitScript>().visualMovementSpeed = 0.5f;
                }
            }
        }
    }
    //This is from quill18Create's tutorial
    //You can find it by searching for grid based movement on youtube, he goes into explaining how everything works
    //The map layouts a bit different
    //all this does is set the tiles[x,y] to the corresponding tile
    public void generateMapInfo()
    {
        tiles = new int[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }
        tiles[2, 7] = 2;
        tiles[3, 7] = 2;

        tiles[6, 7] = 2;
        tiles[7, 7] = 2;

        tiles[2, 2] = 2;
        tiles[3, 2] = 2;

        tiles[6, 2] = 2;
        tiles[7, 2] = 2;

        tiles[0, 3] = 3;
        tiles[1, 3] = 3;
        tiles[0, 2] = 3;
        tiles[1, 2] = 3;

        tiles[0, 6] = 3;
        tiles[1, 6] = 3;
        tiles[2, 6] = 3;
        tiles[0, 7] = 3;
        tiles[1, 7] = 3;

        tiles[2, 3] = 3;
        tiles[0, 4] = 1;
        tiles[0, 5] = 1;
        tiles[1, 4] = 1;
        tiles[1, 5] = 1;
        tiles[2, 4] = 3;
        tiles[2, 5] = 3;

        tiles[4, 4] = 1;
        tiles[5, 4] = 1;
        tiles[4, 5] = 1;
        tiles[5, 5] = 1;

        tiles[7, 3] = 3;
        tiles[8, 3] = 3;
        tiles[9, 3] = 3;
        tiles[8, 2] = 3;
        tiles[9, 2] = 3;
        tiles[7, 4] = 3;
        tiles[7, 5] = 3;
        tiles[7, 6] = 3;
        tiles[8, 6] = 3;
        tiles[9, 6] = 3;
        tiles[8, 7] = 3;
        tiles[9, 7] = 3;
        tiles[8, 4] = 1;
        tiles[8, 5] = 1;
        tiles[9, 4] = 1;
        tiles[9, 5] = 1;


    }
    //Creates the graph for the pathfinding, it sets up the neighbours
    //This is also from Quill18Create's tutorial
    public void generatePathFindingGraph()
    {
        graph = new Node[mapSizeX, mapSizeY];

        //initialize graph 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        //calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //X is not 0, then we can add left (x - 1)
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                //X is not mapSizeX - 1, then we can add right (x + 1)
                if (x < mapSizeX - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                //Y is not 0, then we can add downwards (y - 1 ) 
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                //Y is not mapSizeY -1, then we can add upwards (y + 1)
                if (y < mapSizeY - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }


            }
        }
    }


    //In: 
    //Out: void
    //Desc: This instantiates all the information for the map, the UI Quads and the map tiles
    public void generateMapVisuals()
    {
        //generate list of actual tileGameObjects
        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMapForUnitMovementDisplay = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
        int index;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                index = tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tileVisualPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<ClickableTileScript>().tileX = x;
                newTile.GetComponent<ClickableTileScript>().tileY = y;
                newTile.GetComponent<ClickableTileScript>().map = this;
                newTile.transform.SetParent(tileContainer.transform);
                tilesOnMap[x, y] = newTile;


                GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIQuadPotentialMovesContainer.transform);
                quadOnMap[x, y] = gridUI;

                GameObject gridUIForPathfindingDisplay = Instantiate(mapUnitMovementUI, new Vector3(x, 0.502f, y), Quaternion.Euler(90f, 0, 0));
                gridUIForPathfindingDisplay.transform.SetParent(UIUnitMovementPathContainer.transform);
                quadOnMapForUnitMovementDisplay[x, y] = gridUIForPathfindingDisplay;

                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIQuadCursorContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor;

            }
        }
    }

    //Moves the unit
    public void moveUnit()
    {
        if (selectedUnit != null)
        {   
            selectedUnit.GetComponent<UnitScript>().MoveNextTile();

            // SFX
            int teamNum = selectedUnit.GetComponent<UnitScript>().teamNum;
            switch (teamNum) {
                case 0:
                    soundManager.PlayHumanMoveSFX();
                    break;
                case 1:
                    soundManager.PlaySkeleMoveSFX();
                    break;
                default:
                    Debug.LogError("Team Num not found, cannot play move sfx");
                    break;
            }
        }
    }

    //In: the x and y of a tile
    //Out: vector 3 of the tile in world space, theyre .75f off of zero
    //Desc: returns a vector 3 of the tile in world space, theyre .75f off of zero
    public Vector3 tileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }



    //In: 
    //Out: void
    //Desc: sets the tile as occupied, if a unit is on the tile
    public void setIfTileIsOccupied()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {
            //Debug.Log("Set if Tile is Occupied is Called");
            foreach (Transform unitOnTeam in team)
            {
                int unitX = unitOnTeam.GetComponent<UnitScript>().x;
                int unitY = unitOnTeam.GetComponent<UnitScript>().y;
                unitOnTeam.GetComponent<UnitScript>().tileBeingOccupied = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<ClickableTileScript>().unitOnTile = unitOnTeam.gameObject;
            }

        }
    }
    //In: x and y position of the tile to move to
    //Out: void
    //Desc: generates the path for the selected unit
    //Think this one is also partially from Quill18Create's tutorial
    public void generatePathTo(int x, int y)
    {

        if (selectedUnit.GetComponent<UnitScript>().x == x && selectedUnit.GetComponent<UnitScript>().y == y)
        {
            Debug.Log("clicked the same tile that the unit is standing on");
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript>().path = currentPath;

            mustAttack = true;

            return;
        }
        if (unitCanEnterTile(x, y) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal

            return;
        }

        selectedUnit.GetComponent<UnitScript>().path = null;
        currentPath = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in graph)
        {

            //Initialize to infite distance as we don't know the answer
            //Also some places are infinity
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        //if there is a node in the unvisited list lets check it
        while (unvisited.Count > 0)
        {
            //u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }


            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                //float alt = dist[u] + u.DistanceTo(n);
                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        //if were here we found shortest path, or no path exists
        if (prev[target] == null)
        {
            //No route;
            return;
        }
        currentPath = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPath.Reverse();

        selectedUnit.GetComponent<UnitScript>().path = currentPath;




    }

    //In: tile's x and y position
    //Out: cost that is requiredd to enter the tile
    //Desc: checks the cost of the tile for a unit to enter
    public float costToEnterTile(int x, int y)
    {

        if (unitCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;

        }

        //Gotta do the math here
        Tile t = tileTypes[tiles[x, y]];
        float dist = t.movementCost;

        return dist;
    }

    //change this when we add movement types
    //In:  tile's x and y position
    //Out: true or false if the unit can enter the tile that was entered
    //Desc: if the tile is not occupied by another team's unit, then you can walk through and if the tile is walkable 
    public bool unitCanEnterTile(int x, int y)
    {
        if (tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile != null)
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
            {
                return false;
            }
        }
        return tileTypes[tiles[x, y]].isWalkable;
    }


    //In:  
    //Out: void
    //Desc: uses a raycast to see where the mouse is pointing, this is used to select units
    public void mouseClickToSelectUnit()
    {
        GameObject tempSelectedUnit;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);



        if (Physics.Raycast(ray, out hit))
        {


            //Debug.Log(hit.transform.tag);
            if (unitSelected == false)
            {

                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                    {


                        tempSelectedUnit = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;
                        if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                            && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                            )
                        {
                            disableHighlightUnitRange();
                            selectedUnit = tempSelectedUnit;
                            selectedUnit.GetComponent<UnitScript>().map = this;
                            selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                            unitSelected = true;

                            highlightUnitRange();
                        }
                    }
                }

                else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
                {

                    tempSelectedUnit = hit.transform.parent.gameObject;
                    if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                          && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                        )
                    {

                        disableHighlightUnitRange();
                        selectedUnit = tempSelectedUnit;
                        selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                        //These were here before I don't think they do anything the unit location is set beforehand
                        //selectedUnit.GetComponent<UnitScript>().x = (int)selectedUnit.transform.position.x;
                        // selectedUnit.GetComponent<UnitScript>().y = (int)selectedUnit.transform.position.z;
                        selectedUnit.GetComponent<UnitScript>().map = this;
                        unitSelected = true;

                        highlightUnitRange();
                    }
                }
            }

        }
    }



    //In:  
    //Out: void
    //Desc: finalizes the movement, sets the tile the unit moved to as occupied, etc
    public void finalizeMovementPosition()
    {
        tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTileScript>().unitOnTile = selectedUnit;
        //After a unit has been moved we will set the unitMoveState to (2) the 'Moved' state


        selectedUnit.GetComponent<UnitScript>().setMovementState(2);

        highlightUnitAttackOptionsFromPosition();
        highlightTileUnitIsOccupying();
    }



    //In:  
    //Out: void
    //Desc: selects a unit based on the cursor from the other script
    public void mouseClickToSelectUnitV2()
    {

        if (unitSelected == false && GMS.tileBeingDisplayed != null)
        {

            if (GMS.tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile != null)
            {
                GameObject tempSelectedUnit = GMS.tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile;
                if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                               && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                               )
                {
                    disableHighlightUnitRange();
                    //selectedSound.Play();
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript>().map = this;
                    selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                    selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();
                    unitSelected = true;
                    highlightUnitRange();
                    
                }
            }
        }

    }
    //In:  
    //Out: void
    //Desc: finalizes the player's option, wait or attack
    public void finalizeOption()
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = getUnitAttackOptionsFromPosition();

        if (Physics.Raycast(ray, out hit))
        {

            //This portion is to ensure that the tile has been clicked
            //If the tile has been clicked then we need to check if there is a unit on it
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitScript>().x;
                    int unitY = unitOnTile.GetComponent<UnitScript>().y;

                    if (unitOnTile == selectedUnit)
                    {
                        // TODO: Disini harus checking, kalau same unit dan dia barusan move ke diri sendiri, tidak boleh
                        if (!mustAttack) {
                            disableHighlightUnitRange();
                        Debug.Log("ITS THE SAME UNIT JUST WAIT");
                        selectedUnit.GetComponent<UnitScript>().wait();
                        selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                        selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                        deselectUnit();
                        }
                        


                    }
                    else if (unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            Debug.Log("We clicked an enemy that should be attacked");
                            Debug.Log(selectedUnit.GetComponent<UnitScript>().currentHealthPoints);
                            StartCoroutine(BMS.attack(selectedUnit, unitOnTile));


                            StartCoroutine(deselectAfterMovements(selectedUnit, unitOnTile));
                        }
                    }
                }
            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
            {
                GameObject unitClicked = hit.transform.parent.gameObject;
                int unitX = unitClicked.GetComponent<UnitScript>().x;
                int unitY = unitClicked.GetComponent<UnitScript>().y;

                if (unitClicked == selectedUnit)
                {
                    // TODO: Disini harus checking, kalau same unit dan dia barusan move ke diri sendiri, tidak boleh
                    if (!mustAttack) {
                        disableHighlightUnitRange();
                    Debug.Log("ITS THE SAME UNIT JUST WAIT");
                    selectedUnit.GetComponent<UnitScript>().wait();
                    selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                    selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                    deselectUnit();
                    }
                    

                }
                else if (unitClicked.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                {
                    if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                    {

                        Debug.Log("We clicked an enemy that should be attacked");
                        Debug.Log("Add Code to Attack enemy");
                        //selectedUnit.GetComponent<UnitScript>().setAttackAnimation();
                        StartCoroutine(BMS.attack(selectedUnit, unitClicked));

                        // selectedUnit.GetComponent<UnitScript>().wait();
                        //Check if soemone has won
                        //GMS.checkIfUnitsRemain();
                        StartCoroutine(deselectAfterMovements(selectedUnit, unitClicked));
                    }
                }

            }
        }

    }

    //In:  
    //Out: void
    //Desc: de-selects the unit
    public void deselectUnit()
    {

        if (selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                soundManager.PlayCancelMoveSFX();
                selectedUnit.GetComponent<UnitScript>().setMovementState(0);


                selectedUnit = null;
                unitSelected = false;
            }
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(3))
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                unitSelected = false;
                selectedUnit = null;
            }
            else
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTileScript>().unitOnTile = null;
                tilesOnMap[unitSelectedPreviousX, unitSelectedPreviousY].GetComponent<ClickableTileScript>().unitOnTile = selectedUnit;

                selectedUnit.GetComponent<UnitScript>().x = unitSelectedPreviousX;
                selectedUnit.GetComponent<UnitScript>().y = unitSelectedPreviousY;
                selectedUnit.GetComponent<UnitScript>().tileBeingOccupied = previousOccupiedTile;
                selectedUnit.transform.position = tileCoordToWorldCoord(unitSelectedPreviousX, unitSelectedPreviousY);
                selectedUnit.GetComponent<UnitScript>().setMovementState(0);
                selectedUnit = null;
                unitSelected = false;
                soundManager.PlayCancelMoveSFX();
            }
        }
    }


    //In:  
    //Out: void
    //Desc: highlights the units range options (this is the portion shown in the video)
    public void highlightUnitRange()
    {
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyUnitsInMovementRange = new HashSet<Node>();

        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;


        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        string unitName = selectedUnit.GetComponent<UnitScript>().unitName;
        finalMovementHighlight = string.Equals(unitName, "Skeleton Soldier") ? getSoldierMovementOptions() : string.Equals(unitName, "Giga Mungus") ? getMungusMovementOptions() : string.Equals(unitName, "Skeleton Archer Bald") ? getBaldMovementOptions() : string.Equals(unitName, "Skeleton Archer") ? getArcherMovementOptions() : getUnitMovementOptions();
        totalAttackableTiles = getUnitTotalAttackableTiles(finalMovementHighlight, attRange, unitInitialNode);
        //Debug.Log("There are this many available tiles for the unit: "+finalMovementHighlight.Count);

        foreach (Node n in totalAttackableTiles)
        {

            if (tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript>().unitOnTile != null)
            {
                GameObject unitOnCurrentlySelectedTile = tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript>().unitOnTile;
                if (unitOnCurrentlySelectedTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {
                    finalEnemyUnitsInMovementRange.Add(n);
                }
            }
        }


        highlightEnemiesInRange(totalAttackableTiles);
        //highlightEnemiesInRange(finalEnemyUnitsInMovementRange);
        highlightMovementRange(finalMovementHighlight);
        //Debug.Log(finalMovementHighlight.Count);
        selectedUnitMoveRange = finalMovementHighlight;

        //This final bit sets the selected Units tiles, which can be accessible in other functions
        //Probably bad practice, but I'll need to get things to work for now (2019-09-30)
        selectedUnitTotalRange = getUnitTotalRange(finalMovementHighlight, totalAttackableTiles);
        //Debug.Log(unionTiles.Count);

        //Debug.Log("exiting the while loop");
        //This will for each loop will highlight the movement range of the units


    }


    //In:  
    //Out: void
    //Desc: disables the quads that are being used to highlight position
    public void disableUnitUIRoute()
    {
        foreach (GameObject quad in quadOnMapForUnitMovementDisplay)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {

                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }


    public HashSet<Node> getMungusMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            //Debug.Log(cost[n.x, n.y]);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (Mathf.Abs(unitInitialNode.x - neighbour.x) < 2 && Mathf.Abs(unitInitialNode.y - neighbour.y) < 2) {
                        if (!finalMovementHighlight.Contains(neighbour))
                        {
                            cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                            //Debug.Log(cost[neighbour.x, neighbour.y]);
                            if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                            {
                                //Debug.Log(cost[neighbour.x, neighbour.y]);
                                tempUIHighlight.Add(neighbour);
                            }
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
           
        }

        Debug.Log("The total amount of movable spaces for this unit is: " + finalMovementHighlight.Count);
        Debug.Log("We have used the function to calculate it this time");
        return finalMovementHighlight;
    }


    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            //Debug.Log(cost[n.x, n.y]);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                        //Debug.Log(cost[neighbour.x, neighbour.y]);
                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            //Debug.Log(cost[neighbour.x, neighbour.y]);
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();

        }

        Debug.Log("The total amount of movable spaces for this unit is: " + finalMovementHighlight.Count);
        Debug.Log("We have used the function to calculate it this time");
        return finalMovementHighlight;
    }

    public HashSet<Node> getBaldMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> seenUIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        
        for (int i = 0; i < moveSpeed; i++)
        {
            foreach (Node t in finalMovementHighlight)
            {
                foreach (Node tn in t.neighbours)
                {
                    tempUIHighlight.Add(tn);
                }
            }
            finalMovementHighlight = tempUIHighlight;
            tempUIHighlight = new HashSet<Node>();
            if (i < moveSpeed - 1)
            {
                seenUIHighlight.UnionWith(finalMovementHighlight);
            }
        }

        finalMovementHighlight.ExceptWith(seenUIHighlight);
        finalMovementHighlight.Add(unitInitialNode);

        Debug.Log("The total amount of movable spaces for this unit is: " + finalMovementHighlight.Count);
        Debug.Log("We have used the function to calculate it this time");
        return finalMovementHighlight;
    }

    //In:  
    //Out: HashSet<Node> of the tiles that can be reached by unit
    //Desc: returns the hashSet of nodes that the unit can reach from its position
    public HashSet<Node> getSoldierMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            if ((n.x == unitInitialNode.x+1 && n.y == unitInitialNode.y) ||
                (n.x == unitInitialNode.x-1 && n.y == unitInitialNode.y) ||
                (n.x == unitInitialNode.x && n.y == unitInitialNode.y+1) ||
                (n.x == unitInitialNode.x && n.y == unitInitialNode.y-1)){
                    cost[n.x, n.y] = costToEnterTile(n.x, n.y);
                    //Debug.Log(cost[n.x, n.y]);
                    if (moveSpeed - cost[n.x, n.y] >= 0)
                    {
                        UIHighlight.Add(n);
                    }
                }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (neighbour.x == unitInitialNode.x || neighbour.y == unitInitialNode.y){
                        if (!finalMovementHighlight.Contains(neighbour))
                        {
                            cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                            //Debug.Log(cost[neighbour.x, neighbour.y]);
                            if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                            {
                                //Debug.Log(cost[neighbour.x, neighbour.y]);
                                tempUIHighlight.Add(neighbour);
                            }
                        }
                    }
                }
            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
           
        }
        Debug.Log("The total amount of movable spaces for this unit is: " + finalMovementHighlight.Count);
        Debug.Log("We have used the function to calculate it this time");
        return finalMovementHighlight;
    }


    public HashSet<Node> getArcherMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            //Debug.Log(cost[n.x, n.y]);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        if (cost[neighbour.x, neighbour.y] == 0) {
                            cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                        } else {
                            if (cost[neighbour.x, neighbour.y] > costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y]) {
                                cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                            }
                        }
                        
                        //Debug.Log(cost[neighbour.x, neighbour.y]);
                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            //Debug.Log(cost[neighbour.x, neighbour.y]);
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
           
        }

        HashSet<Node> resultMove = new HashSet<Node>();
        foreach (Node e in finalMovementHighlight) {
            Debug.Log("finalMovementHighlight: " + e.x + ", " + e.y);
            if ((Mathf.Abs(unitInitialNode.x - e.x) == Mathf.Abs(unitInitialNode.y - e.y))){
                resultMove.Add(e);
            }
        }

        Debug.Log("The total amount of movable spaces for this unit is: " + resultMove.Count);
        Debug.Log("We have used the function to calculate it this time");
        return resultMove;
    }
    //In:  finalMovement highlight and totalAttackabletiles
    //Out: a hashSet of nodes that are the combination of the two inputs
    //Desc: returns the unioned hashSet
    public HashSet<Node> getUnitTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTiles = new HashSet<Node>();
        unionTiles.UnionWith(finalMovementHighlight);
        //unionTiles.UnionWith(finalEnemyUnitsInMovementRange);
        unionTiles.UnionWith(totalAttackableTiles);
        return unionTiles;
    }
    //In:  finalMovement highlight, the attack range of the unit, and the initial node that the unit was standing on
    //Out: hashSet Node of the total attackable tiles for the unit
    //Desc: returns a set of nodes that represent the unit's total attackable tiles
    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }

                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if (i < attRange - 1)
                {
                    seenNodes.UnionWith(neighbourHash);
                }

            }
            neighbourHash.ExceptWith(seenNodes);
            seenNodes = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighbourHash);
        }
        totalAttackableTiles.Remove(unitInitialNode);

        //Debug.Log("The unit node has this many attack options" + totalAttackableTiles.Count);
        return (totalAttackableTiles);
    }


    //In:  
    //Out: hashSet of nodes get all the attackable tiles from the current position
    //Desc: returns a set of nodes that are all the attackable tiles from the units current position
    public HashSet<Node> getUnitAttackOptionsFromPosition()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;


        neighbourHash = new HashSet<Node>();
        neighbourHash.Add(initialNode);
        for (int i = 0; i < attRange; i++)
        {
            foreach (Node t in neighbourHash)
            {
                foreach (Node tn in t.neighbours)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            neighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attRange - 1)
            {
                seenNodes.UnionWith(neighbourHash);
            }
        }
        neighbourHash.ExceptWith(seenNodes);
        neighbourHash.Remove(initialNode);
        return neighbourHash;
    }

    //In:  
    //Out: hashSet node that the unit is currently occupying
    //Desc: returns a set of nodes of the tile that the unit is occupying
    public HashSet<Node> getTileUnitIsOccupying()
    {

        int x = selectedUnit.GetComponent<UnitScript>().x;
        int y = selectedUnit.GetComponent<UnitScript>().y;
        HashSet<Node> singleTile = new HashSet<Node>();
        singleTile.Add(graph[x, y]);
        return singleTile;

    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's options
    public void highlightTileUnitIsOccupying()
    {
        if (selectedUnit != null)
        {
            highlightMovementRange(getTileUnitIsOccupying());
        }
    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's attackOptions from its position
    public void highlightUnitAttackOptionsFromPosition()
    {
        if (selectedUnit != null)
        {
            highlightEnemiesInRange(getUnitAttackOptionsFromPosition());
        }
    }

    //In:  Hash set of the available nodes that the unit can range
    //Out: void - it changes the quadUI property in the gameworld to visualize the selected unit's movement
    //Desc: This function highlights the selected unit's movement range
    public void highlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = blueUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }



    //In:  Hash set of the enemies in range of the selected Unit
    //Out: void - it changes the quadUI property in the gameworld to visualize an enemy
    //Desc: This function highlights the enemies in range once they have been added to a hashSet
    public void highlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = redUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }


    //In:  
    //Out: void 
    //Desc: disables the highlight
    public void disableHighlightUnitRange()
    {
        foreach (GameObject quad in quadOnMap)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //In:  
    //Out: void 
    //Desc: moves the unit then finalizes the movement
    public IEnumerator moveUnitAndFinalize()
    {
        disableHighlightUnitRange();
        disableUnitUIRoute();
        while (selectedUnit.GetComponent<UnitScript>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovementPosition();
        selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();
    }


    //In:  both units engaged in a battle
    //Out:  
    //Desc: deselects the selected unit after the action has been taken
    public IEnumerator deselectAfterMovements(GameObject unit, GameObject enemy)
    {
        //selectedSound.Play();
        selectedUnit.GetComponent<UnitScript>().setMovementState(3);
        disableHighlightUnitRange();
        disableUnitUIRoute();
        //If i dont have this wait for seconds the while loops get passed as the coroutine has not started from the other script
        //Adding a delay here to ensure that it all works smoothly. (probably not the best idea)
        yield return new WaitForSeconds(.25f);
        while (unit.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        while (enemy.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();

        }
        Debug.Log("All animations done playing");

        deselectUnit();
        GMS.endTurn();

    }

    //In:  
    //Out: true if there is a tile that was clicked that the unit can move to, false otherwise 
    //Desc: checks if the tile that was clicked is move-able for the selected unit
    public bool selectTileToMoveTo()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {

            if (hit.transform.gameObject.CompareTag("Tile"))
            {

                int clickedTileX = hit.transform.GetComponent<ClickableTileScript>().tileX;
                int clickedTileY = hit.transform.GetComponent<ClickableTileScript>().tileY;
                Node nodeToCheck = graph[clickedTileX, clickedTileY];
                //var unitScript = selectedUnit.GetComponent<UnitScript>();

                if (selectedUnitMoveRange.Contains(nodeToCheck))
                {
                    if ((hit.transform.gameObject.GetComponent<ClickableTileScript>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTileScript>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCheck)))
                    {
                        Debug.Log("We have finally selected the tile to move to");
                        generatePathTo(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }
            else if (hit.transform.gameObject.CompareTag("Unit"))
            {

                if (hit.transform.parent.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {
                    Debug.Log("Clicked an Enemy");
                }
                else if (hit.transform.parent.gameObject == selectedUnit)
                {

                    generatePathTo(selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y);
                    return true;
                }
            }

        }
        return false;
    }

    // CUSTOM ~Adi
    // Select Tile and Move To, dengan parameter x dan y
    public bool selectTileToMoveTo(int x, int y)
    {
        int clickedTileX = x;
        int clickedTileY = y;
        Node nodeToCheck = graph[clickedTileX, clickedTileY];
        //var unitScript = selectedUnit.GetComponent<UnitScript>();

        Debug.Log("SelectedUnitMoveRange");
        List<Node> temp = selectedUnitMoveRange.ToList();
        for (int i = 0; i < temp.Count; i++) {
            
            Debug.Log(temp[i].x + ", " + temp[i].y);
        }


        if (selectedUnitMoveRange.Contains(nodeToCheck))
        {
            if ((tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile == null || tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCheck)))
            {
                Debug.Log("We have finally selected the tile to move to");
                generatePathTo(clickedTileX, clickedTileY);

                return true;
            }
        }

        // else if (hit.transform.gameObject.CompareTag("Unit"))
        // {

        //     if (hit.transform.parent.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
        //     {
        //         Debug.Log("Clicked an Enemy");
        //     }
        //     else if(hit.transform.parent.gameObject == selectedUnit)
        //     {

        //         generatePathTo(selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y);
        //         return true;
        //     }
        // }


        return false;
    }

    // Custom ~Adi
    // mouseClickToSelectUnitV2, tapi pakai x dan y
    public void mouseClickToSelectUnitV2(int x, int y, GameObject selectedUnitAI)
    {

        if (unitSelected == false)
        {
            
                GameObject tempSelectedUnit = selectedUnitAI; // TODO: Diubah disini
                if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                               && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                               )
                {
                    // Debug.Log("V2 KEPLAY?");
                    disableHighlightUnitRange();
                    //selectedSound.Play();
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript>().map = this;
                    selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                    selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();
                    unitSelected = true;
                    highlightUnitRange();

                }
            
        }

    }

    // CUSTOM ~Adi
    // Finalize Option dengan parameter x dan y
    public void finalizeOption(int x, int y)
    {
        Debug.Log("TestFinalizedOption");
        HashSet<Node> attackableTiles = getUnitAttackOptionsFromPosition();


        
        //This portion is to ensure that the tile has been clicked
        //If the tile has been clicked then we need to check if there is a unit on it
        if (tilesOnMap[x, y].gameObject.CompareTag("Tile"))
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile != null)
            {
                GameObject unitOnTile = tilesOnMap[x, y].transform.GetComponent<ClickableTileScript>().unitOnTile;
                int unitX = unitOnTile.GetComponent<UnitScript>().x;
                int unitY = unitOnTile.GetComponent<UnitScript>().y;

                if (unitOnTile == selectedUnit)
                {
                    disableHighlightUnitRange();
                    Debug.Log("ITS THE SAME UNIT JUST WAIT");
                    selectedUnit.GetComponent<UnitScript>().wait();
                    selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                    selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                    deselectUnit();


                }
                else if (unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                {
                    if (unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                    {
                        Debug.Log("We clicked an enemy that should be attacked");
                        Debug.Log(selectedUnit.GetComponent<UnitScript>().currentHealthPoints);
                        StartCoroutine(BMS.attack(selectedUnit, unitOnTile));


                        StartCoroutine(deselectAfterMovements(selectedUnit, unitOnTile));
                    }
                }
            }
        }
        else if (tilesOnMap[x, y].transform.parent != null && tilesOnMap[x, y].transform.parent.gameObject.CompareTag("Unit"))
        {
            GameObject unitClicked = tilesOnMap[x, y].transform.parent.gameObject;
            int unitX = unitClicked.GetComponent<UnitScript>().x;
            int unitY = unitClicked.GetComponent<UnitScript>().y;

            if (unitClicked == selectedUnit)
            {
                disableHighlightUnitRange();
                Debug.Log("ITS THE SAME UNIT JUST WAIT");
                selectedUnit.GetComponent<UnitScript>().wait();
                selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                deselectUnit();

            }
            else if (unitClicked.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
            {
                if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                {

                    Debug.Log("We clicked an enemy that should be attacked");
                    Debug.Log("Add Code to Attack enemy");
                    //selectedUnit.GetComponent<UnitScript>().setAttackAnimation();
                    StartCoroutine(BMS.attack(selectedUnit, unitClicked));

                    // selectedUnit.GetComponent<UnitScript>().wait();
                    //Check if soemone has won
                    //GMS.checkIfUnitsRemain();
                    StartCoroutine(deselectAfterMovements(selectedUnit, unitClicked));
                }
            }

        }

        // Out of moves: End Turn
        GMS.amountOfEnemyAIDoneMove++;
        // TODO: Mungkin perlu dicek: Bagaimana jika AI menyerang musuh, tapi unit AInya yang mati
        if (GMS.amountOfEnemyAIDoneMove >= GMS.amountOfEnemyAI) {
            GMS.endTurn();
        }
    }

    // CUSTOM ~Adi
    // move unit and finalize, dengan x dan y
    public IEnumerator moveUnitAndFinalize(int x, int y, GameObject attackerUnit, GameObject attackedUnit)
    {
        disableHighlightUnitRange();
        disableUnitUIRoute();
        while (selectedUnit.GetComponent<UnitScript>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovementPosition();
        selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();

        // Finalize Option
        finalizeOption(x, y);

        // Jika bukan menyerang diri sendiri (alias skip turn)
        // Ketika menyerang diri sendiri, attackedUnit pasti null karena tidak ditemukan unit player yang sesuai koordinat attack
        if (attackedUnit != null) {
            StartCoroutine(BMS.attack(attackerUnit, attackedUnit));
        } else {
            GMS.endTurn();
        }
    }

    // Gerakan musuh
    /*
    Foreach enemy unit
    Pilih gerakan terserah
    Gerak ke kanan/kiri
    Tidak nyerang
    Sudah selesai, end turn
    */

    

    private int indexAItoMove(int[][][][] newstate){
        this.tempState = newstate;

        int defaultIndex = -99;
        
        for (int i = 0; i < GMS.team2.transform.childCount; i++){
            GameObject tile = GMS.team2.transform.GetChild(i).transform.gameObject;
            int x = 9 + (-1* (int) tile.GetComponent<UnitScript>().y);
            int y = (int) tile.GetComponent<UnitScript>().x;

            int int_type = GMS.intType(tile.GetComponent<UnitScript>().unitName);

            if (this.tempState[x][y][0][0] != int_type || (this.tempState[x][y][1][0] != 0 && this.tempState[x][y][1][1] != 0)){
                defaultIndex = i;
            } else if (this.tempState[x][y][0][0] == int_type){
                this.tempState[x][y][0][0] = 0;
                Debug.Log(tile.name + " not move");
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        for (int i = 0; i < 10; i++)
        {
            
            sb.Append("{");
            for (int j = 0; j < 10; j++)
            {
                
                sb.Append("{{" + tempState[i][j][0][0] + "},{");
                if (tempState[i][j][1].Length > 1) {
                    sb.Append(tempState[i][j][1][0] + ", " + tempState[i][j][1][1] + "}}\t");
                } else {
                    sb.Append("}}\t");
                }
            }
            sb.Append("}\n");
        }
        sb.Append("\n}");
        Debug.Log("BOARDDD hasil IndexAIToMove:");
        Debug.Log(sb.ToString());

        return defaultIndex;
    }

//     {                    SETELAH DIGANTI 0
// {{{4},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{4},{}}, 	}
// {{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	{{2},{}}, 	{{0},{}}, 	{{1},{}}, 	{{2},{}}, 	{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	}
// {{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	}
// {{{-4},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-4},{}}, 	}

// }

    private int[] AIMoveLoc(int index){
        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        for (int i = 0; i < 10; i++)
        {
            sb.Append("{");
            for (int j = 0; j < 10; j++){
                sb.Append("{{" + tempState[i][j][0][0] + "},{");
                if (tempState[i][j][1].Length > 1) {
                    sb.Append(tempState[i][j][1][0] + ", " + tempState[i][j][1][1] + "}}\t");
                } else {
                    sb.Append("}}\t");
                }
            }
            sb.Append("}\n");
        }
        sb.Append("\n}");
        Debug.Log("BOARDDD AI Move Loc:");
        Debug.Log(sb.ToString());

        int[] res = new int[] {-99,-99, -99, -99};

        if (index < 0) {
            return res;
        }

        for (int i = 0; i < 10; i++){
            for (int j = 0; j < 10; j++){
                if (tempState[i][j][0][0] > 0 && tempState [i][j][0][0] < 5){
                    res[0] = j; //x
                    res[1] = i; //y

                    res[2] = tempState[i][j][1][0];
                    res[3] = tempState[i][j][1][1];
                    
                    return res;
                }
            }
        }

        res[0] = -4;
        res[1] = -4;
        return res;
    }

// {
// {{{4},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{4},{}}, 	}
// {{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	{{2},{}}, 	{{0},{}}, 	{{1},{}}, 	{{2},{}}, 	{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	}
// {{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	}
// {{{-4},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-4},{}}, 	}

// }

    private GameObject attTargetSearch(int x, int y){
        GameObject target = null;
        for (int i = 0; i < GMS.team1.transform.childCount; i++)
        {
            GameObject tile = GMS.team1.transform.GetChild(i).gameObject;
            int xi = (int)tile.GetComponent<UnitScript>().x;
            int yi = (int)tile.GetComponent<UnitScript>().y;

            // if (tile.name == "toonSkeleSoldier (2)"){
            //     Debug.Log("var x: " + x + ", var y:" + y);
            //     Debug.Log("toon x: " + xi + ", toon y:" + yi);
            // }
    
            if (xi == x && yi == y){
                target = tile;
                break;
            }
        }
        return target;
    }
        
    public IEnumerator AITurn()
    {
        int[][][][] test_state = GMS.boardToState();

        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        for (int i = 0; i < 10; i++)
        {
            sb.Append("{");
            for (int j = 0; j < 10; j++)
            {
                sb.Append("{{" + test_state[i][j][0][0] + "},{}}, \t");
            }
            sb.Append("}\n");
        }
        sb.Append("\n}");
        Debug.Log("BOARDDD masuk minmax:");
        Debug.Log(sb.ToString());

        // Variables to handle thread result and completion status
        bool isMinimaxComplete = false;
        int[][][][] minimaxResult = null;

        // Start the minimax algorithm in a new thread
        Thread minimaxThread = new Thread(() =>
        {
            minimaxResult = tempMinMax.MinimaxAlgorithm(test_state, 3, true, map, 10, int.MinValue, int.MaxValue);
            isMinimaxComplete = true;
        });
        minimaxThread.Start();

        // Wait for the minimax algorithm to complete
        while (!isMinimaxComplete)
        {
            yield return null; // Wait for the next frame
        }

        // Use the result of the minimax algorithm
        test_state = minimaxResult;

        sb = new StringBuilder();
        sb.Append("{\n");
        for (int i = 0; i < 10; i++)
        {
            sb.Append("{");
            for (int j = 0; j < 10; j++)
            {
                sb.Append("{{" + test_state[i][j][0][0] + "},{");
                if (test_state[i][j][1].Length > 1)
                {
                    sb.Append(test_state[i][j][1][0] + ", " + test_state[i][j][1][1] + "}}\t");
                }
                else
                {
                    sb.Append("}}\t");
                }
            }
            sb.Append("}\n");
        }
        sb.Append("\n}");
        Debug.Log("BOARDDD hasil minmax:");
        Debug.Log(sb.ToString());

        Debug.Log("Current Team: " + GMS.currentTeam);
        GMS.amountOfEnemyAIDoneMove = 0;

        int idx_r = indexAItoMove(test_state);
        Debug.Log("idx r: " + idx_r);

        if (idx_r == -99)
        {
            GMS.endTurn();
            yield break;
        }

        selectedUnit = GMS.team2.transform.GetChild(idx_r).gameObject;
        GameObject attackerUnit = selectedUnit;

        int[] locationIdx = AIMoveLoc(idx_r);

        int targetTileX = locationIdx[0];
        int targetTileY = 9 + (-1 * locationIdx[1]);

        Debug.Log("index selected unit: " + idx_r);

        Debug.Log("Loc Now X: " + selectedUnit.GetComponent<UnitScript>().x);
        Debug.Log("Loc Now Y: " + selectedUnit.GetComponent<UnitScript>().y);

        Debug.Log("Target Tile X: " + targetTileX);
        Debug.Log("Target Tile Y: " + targetTileY);

        if (selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
        {
            Debug.Log("heyyy taegetttt: " + targetTileX + ", " + targetTileY);
            Node nodeToCheck = graph[targetTileX, targetTileY];

            mouseClickToSelectUnitV2(targetTileX, targetTileY, selectedUnit);

            yield return StartCoroutine(aiPath(targetTileX, targetTileY));

            if (selectTileToMoveTo(targetTileX, targetTileY))
            {
                Debug.Log("movement path has been located");
                unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript>().x;
                unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript>().y;
                previousOccupiedTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
                selectedUnit.GetComponent<UnitScript>().setWalkingAnimation();
                moveUnit();

                Debug.Log("sblm move");
                GameObject attackedUnit = attTargetSearch(locationIdx[2], 9 + (-1 * locationIdx[3]));
                Debug.Log("Att x board: " + locationIdx[2]);
                Debug.Log("Att y board: " + (9 + (-1 * locationIdx[3])));
                Debug.Log("sblm attack");
                Debug.Log("nama attacker: " + selectedUnit);
                Debug.Log("nama attacked: " + attackedUnit);

                Debug.Log("sesudah attack");
                yield return StartCoroutine(moveUnitAndFinalize(targetTileX, targetTileY, attackerUnit, attackedUnit));
                Debug.Log("sesudah move");
            }
        }

        Debug.Log("Tes Akhir");
        yield break;
    }

    public IEnumerator aiPath(int posx, int posy){
        GMS.unitPathToCursor = GMS.generateCursorRouteTo(posx, posy);

        int rtx = posx;
        int rty = posy;
        Debug.Log("pos x: " + posx);
        Debug.Log("pos y: " + posy);

        if (GMS.unitPathToCursor.Count != 0)
        {
            for (int i = 0; i < GMS.unitPathToCursor.Count; i++)
            {
                int nodeX = GMS.unitPathToCursor[i].x;
                int nodeY = GMS.unitPathToCursor[i].y;

                if (i == 0)
                {
                    GameObject quadToUpdate = quadOnMapForUnitMovementDisplay[nodeX, nodeY];
                    quadToUpdate.GetComponent<Renderer>().material = GMS.UICursor;
                }
                else if (i != 0 && (i + 1) != GMS.unitPathToCursor.Count)
                {
                    //This is used to set the indicator for tiles excluding the first/last tile
                    GMS.setCorrectRouteWithInputAndOutput(nodeX, nodeY, i);
                }
                else if (i == GMS.unitPathToCursor.Count - 1)
                {
                    //This is used to set the indicator for the final tile;
                    GMS.setCorrectRouteFinalTile(nodeX, nodeY, i);
                }

                quadOnMapForUnitMovementDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
            }
        }
        GMS.unitPathExists = true;

        yield return new WaitForSeconds(3);
    }


    public int[][] GridMovementCostTo2DArray() {
        // Assume grid is 10x10
        int[][] movementCostMap = new int[10][];
        for (int i = 0; i < 10; i++) {
            movementCostMap[i] = new int[10];
        }

        for (int i = 0; i < mapSizeX; i++) {
            for (int j = 0; j < mapSizeY; j++) {
                int y = 9 + (-1*j);
                int x = i;

                movementCostMap[x][y] = (int) tileTypes[tiles[i,j]].movementCost;
            }
        }

        // StringBuilder sb = new StringBuilder();
        // sb.Append("{\n");
        // for (int i = 0; i < 10; i++)
        // {
            
        //     sb.Append("{");
        //     for (int j = 0; j < 10; j++)
        //     {
                
        //         sb.Append("{" + movementCostMap[i][j] + "} ");
        //     }
        //     sb.Append("}\n");
        // }
        // sb.Append("\n}");
        // Debug.Log("Map:");
        // Debug.Log(sb.ToString());

        return movementCostMap;
    }

    // Helper function untuk initialize 4d state
    private int[][][][] init4dState() {
    int[][][][] state = new int[10][][][];
    for (int i = 0; i < 10; i++)
        {
            state[i] = new int[10][][];
            for (int j = 0; j < 10; j++)
            {
                state[i][j] = new int[2][];
                for (int k = 0; k < 2; k++)
                {
                    state[i][j][k] = new int[2];
                    state[i][j][k][0] = 0;
                }
            }
        }
    return state;
}
}


// {
// {{{4},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{4},{}}, 	}
// {{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	{{2},{}}, 	{{0},{}}, 	{{1},{}}, 	{{2},{}}, 	{{0},{}}, 	{{3},{}}, 	{{0},{}}, 	}
// {{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{1},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{-1},{}}, {{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	}
// {{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	}
// {{{-4},{}}, {{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-4},{}}, 	}

// }

// {
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	}
// {{{-1},{}}, {{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-1},{}}, 	}
// {{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-3},{}}, 	{{0},{}}, 	}
// {{{-4},{}}, {{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-2},{}}, 	{{0},{}}, 	{{0},{}}, 	{{-4},{}}, 	}

// }