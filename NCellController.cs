using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCellController : MonoBehaviour {

    public static float CellSize { get; set; }

    public static GameObject instance;

    private Coordinate playerCoordinate;

    private GameObject player;

    public int direction = 1, predirection = 2;

    public int MaxPathLength = 7, MinPathLength = 1;

    public NCell lastCell;

    [SerializeField]
    public int pathLength;

    [SerializeField]
    List<NCell> curPath = new List<NCell>();

    public float screenWidth, screenHeight;
    public int cellWidth, cellHeight;

    public static List<Vector2> pathLengths = new List<Vector2>();

    [SerializeField]
    private List<NCell> auxiliaryList = new List<NCell>();

    private List<NCell> DelList = new List<NCell>();

    public static List<NCell> cells = new List<NCell>();

    [SerializeField]
    public NCell startCell, curCell, preStartCell;
    public void Start()
    {
        pathLengths.Clear();
        instance = gameObject;
        screenHeight = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().orthographicSize;
        screenWidth = screenHeight / 16 * 9;
        player = GameObject.FindGameObjectWithTag("Player");
        GameObject cell1 = PoolScript.instance.GetObjectFromPool("Cell", Vector3.zero, Quaternion.Euler(0, 0, 0));
        CellSize = cell1.GetComponent<BoxCollider2D>().size.x;
        PoolScript.instance.ReturnObjectToPool(cell1);
        cellWidth = Mathf.FloorToInt(screenWidth / CellSize) + 1;
        cellHeight = Mathf.FloorToInt(screenHeight / CellSize) + 1;
        preStartCell = new NCell(1, 0, true);
        lastCell = new NCell(0, 0, false);
        cells.Add(lastCell);
        cells.Add(preStartCell);
        lastCell.Manifest();
        preStartCell.Manifest();

        pathLengths.Add(new Vector2(lastCell.Position.x + CellSize/2, lastCell.Position.y));

        int pDir = direction;
        int pPreDir = predirection;
        NCell pLastCell = lastCell;

        int localLengthsCount = pathLengths.Count;
        
        curPath = createPath();
        while (CheckForSelfIntersections(curPath) || CheckForDistanceBetweenCells(preStartCell, startCell))
        {
            RefreshLengths(localLengthsCount);
            CreateClearData(pLastCell, pDir, pPreDir);
            for (int i = curPath.Count - 1; i >= 0; i--)
            {
                curPath[i].Del();
            }
            curPath = createPath();
        }
        ManifestListOfNCells(curPath);
        curPath.Add(preStartCell);
    }

    public void Update()
    {
        MarkBlack(DelList);
        MarkRed(auxiliaryList);
        playerCoordinate = new Coordinate(Mathf.FloorToInt(player.transform.position.x / CellSize), Mathf.FloorToInt(player.transform.position.y / CellSize));
        for (int i = DelList.Count - 1; i >= 0; i--)
        {
            if (Mathf.Abs(DelList[i].Position.x - GameObject.FindGameObjectWithTag("MainCamera").transform.position.x) > screenWidth + CellSize || Mathf.Abs(DelList[i].Position.y - GameObject.FindGameObjectWithTag("MainCamera").transform.position.y) > screenHeight + CellSize)
            {
                DelList[i].Del();
                DelList.Remove(DelList[i]);
            }
        }
        List<NCell> list = new List<NCell>();
        if (preStartCell.Coordinate.Equals(playerCoordinate))
        {
            NCell _startCell = startCell;
            int pDir = direction;
            int pPreDir = predirection;
            NCell pLastCell = lastCell;
            int localLengthsCount = pathLengths.Count;
            list = createPath();
            //CheckForDistance(startCell, list)   || CheckForDistanceBetweenCells(preStartCell, startCell)     || CheckForDistance(preStartCell, list)
            int a = 0;
            while (CheckForIntersections(list, curPath) || CheckForSelfIntersections(list) || CheckForDistanceBetweenCells(preStartCell, startCell) || CheckForDistance(preStartCell, list) || CheckForDistanceBetweenCells(_startCell,startCell))
            {
                a++;
                if(a > 200)
                {
                    /*
                    Debug.Log("OVERLOADDED   " + preStartCell.Position.x + " " + preStartCell.Position.y + " " + startCell.Position.x + " " + startCell.Position.y + " " + _startCell.Position.x + " " + _startCell.Position.y);
                    Debug.Log(CheckForIntersections(list, curPath));
                    Debug.Log(CheckForSelfIntersections(list));
                    Debug.Log(CheckForDistanceBetweenCells(preStartCell, startCell));
                    Debug.Log(CheckForDistance(preStartCell, list));
                    Debug.Log(CheckForDistanceBetweenCells(_startCell, startCell));
                    */
                    Time.timeScale = 0;
                    break;
                }
                RefreshLengths(localLengthsCount);
                CreateClearData(pLastCell, pDir, pPreDir);
                for (int i = list.Count - 1; i >=0; i--)
                {
                    list[i].Del();
                }
                list = createPath();
            }
            preStartCell = _startCell;
            foreach(NCell cell in auxiliaryList)
            {
                DelList.Add(cell);
            }
            auxiliaryList = curPath;
            curPath = list;
            ManifestListOfNCells(curPath);
        }
    }

    public List<NCell> createPath()
    {
        List<NCell> currentLocalList = new List<NCell>();

        int cellCount = 0;
        while (cellCount < pathLength)
        {
            int localLength = DetermineLineLength();
            cellCount += localLength;
            switch (direction)
            {
                case 1:
                    if (predirection == 4)
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                    }
                    else
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y), currentLocalList);
                        }
                    }
                    break;
                case 2:
                    if (predirection == 1)
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y), currentLocalList);
                        }
                    }
                    else
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y + 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y + 1), currentLocalList);
                        }
                    }
                    break;
                case 3:
                    if (predirection == 2)
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                    }
                    else
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x - 1, lastCell.Coordinate.y, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y), currentLocalList);
                        }
                    }
                    break;
                case 4:
                    if (predirection == 3)
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y), currentLocalList);
                        }
                    }
                    else
                    {
                        while (localLength > 1)
                        {
                            localLength -= 2;
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1, true));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        if (localLength == 1)
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1, false));
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, true));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                        else
                        {
                            currentLocalList.Add(new NCell(lastCell.Coordinate.x, lastCell.Coordinate.y - 1, false));
                            lastCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y - 1), currentLocalList);
                        }
                    }
                    break;
            }
            int _dir = direction;
            if ((direction == 1 && localLength % 2 == 1 && predirection == 4) || (direction == 1 && localLength % 2 == 0 && predirection == 2) || (direction == 3 && localLength % 2 == 0 && predirection == 2) || (direction == 3 && localLength % 2 == 1 && predirection == 4))
            {
                direction = 2;
            }
            else
            if ((direction == 1 && localLength % 2 == 0 && predirection == 4) || (direction == 1 && localLength % 2 == 1 && predirection == 2) || (direction == 3 && localLength % 2 == 1 && predirection == 2) || (direction == 3 && localLength % 2 == 0 && predirection == 4))
            {
                direction = 4;
            }
            else
            if ((direction == 2 && localLength % 2 == 1 && predirection == 3) || (direction == 2 && localLength % 2 == 0 && predirection == 1) || (direction == 4 && localLength % 2 == 1 && predirection == 3) || (direction == 4 && localLength % 2 == 0 && predirection == 1))
            {
                direction = 1;
            }
            else
            if ((direction == 2 && localLength % 2 == 1 && predirection == 1) || (direction == 2 && localLength % 2 == 0 && predirection == 3) || (direction == 4 && localLength % 2 == 1 && predirection == 1) || (direction == 4 && localLength % 2 == 0 && predirection == 3))
            {
                direction = 3;
            }
            predirection = _dir;

            if (predirection == 3 && direction == 4 || predirection == 2 && direction == 1)
            {
                startCell = FindCell(new Coordinate(lastCell.Coordinate.x + 1, lastCell.Coordinate.y), currentLocalList);
            }
            if (predirection == 3 && direction == 2 || predirection == 4 && direction == 1)
            {
                startCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y + 1), currentLocalList);
            }
            if (predirection == 4 && direction == 3 || predirection == 1 && direction == 2)
            {
                startCell = FindCell(new Coordinate(lastCell.Coordinate.x - 1, lastCell.Coordinate.y), currentLocalList);
            }
            if (predirection == 1 && direction == 4 || predirection == 2 && direction == 3)
            {
                startCell = FindCell(new Coordinate(lastCell.Coordinate.x, lastCell.Coordinate.y - 1), currentLocalList);
            }
            pathLengths.Add((lastCell.Position + startCell.Position) / 2);
        }
        return currentLocalList;
    }
    public bool CheckForIntersections(List<NCell> currentLocalList, List<NCell> anotherList)
    {
        for (int i = 0; i < currentLocalList.Count - 1; i++)
        {
            for (int j = 0; j < anotherList.Count - 1; j++)
            {
                if (currentLocalList[i].Coordinate.Equals(anotherList[j].Coordinate) && currentLocalList[i].empty != anotherList[j].empty)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CheckForSelfIntersections(List<NCell> list)
    {
        for (int i = 0; i < list.Count-1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[i].Coordinate.Equals(list[j].Coordinate) && list[i].empty != list[j].empty)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CheckForDistance(NCell startPoint, List<NCell> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            if (Mathf.Abs(list[i].Coordinate.x - startPoint.Coordinate.x) < cellWidth/2 && Mathf.Abs(list[i].Coordinate.y - startPoint.Coordinate.y) < cellHeight / 2)
            {
                return true;
            }
        }
        return false;
    }
    public int DetermineLineLength()
    {
        int localLength = Random.Range(MinPathLength, MaxPathLength);
        return localLength;
    }
    public static NCell FindCell(Coordinate coord, List<NCell> cells)
    {
        NCell outCell = null;
        foreach (NCell cell in cells)
        {
            if (cell.Coordinate.Equals(coord))
            {
                outCell = cell;
            }
        }
        if (outCell != null) return outCell; else return null;
    }
    public void DebugList(List<NCell> list)
    {
        foreach (NCell cell in list)
        {
            Debug.Log(cell.Coordinate.x + "   " + cell.Coordinate.y + " " + cell.empty);
        }
    }
    public void MarkRed(List<NCell> list)
    {
        foreach(NCell cell in list)
        {
            if (cell.cell)
            {
                cell.cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
            }
        }
    }
    public void MarkBlack(List<NCell> list)
    {
        foreach (NCell cell in list)
        {
            if (cell.cell)
            {
                cell.cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            }
        }
    }
    void ManifestListOfNCells(List<NCell> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = list[i].Manifest();
        }
    }
    public void CreateClearData(NCell pLastNCell, int pDir,int pPreDir)
    {
        lastCell = pLastNCell;
        predirection = pPreDir;
        direction = pDir;
    }
    public bool CheckForDistanceBetweenCells(NCell cell1, NCell cell2)
    {
        if (Mathf.Abs(cell1.Coordinate.x - cell2.Coordinate.x) < 6 && Mathf.Abs(cell1.Coordinate.y - cell2.Coordinate.y) < 8)
        {
            return true;
        }
        return false;
    }
    public void RefreshLengths(int count)
    {
        int c = pathLengths.Count;
        for (int i = count; i < c; i++)
        {
            pathLengths.RemoveAt(count);
        }
    }
    public void CellsCheck()
    {
        for (int i = cells.Count; i >= 0; i--)
        {
            //
        }
    }
}
