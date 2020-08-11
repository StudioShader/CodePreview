using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polymino {

    public List<Cell> cells = new List<Cell>();

    public int n;

    public List<List<bool>> cellsInMass = new List<List<bool>>();

    public Polymino()
    {

    }

    public Polymino(int number, Coordinate thisCoordinate)
    {
        n = number;
        int x = 0;
        int y = 0;
        for (int j = 0; j < n; j++)
        {
            cellsInMass.Add(new List<bool>());
            for (int k = 0; k < n; k++)
            {
                cellsInMass[j].Add(false);
            }
        }
        cellsInMass[x][y] = true;
        Cell cl1 = new Cell(thisCoordinate);
        cells.Add(cl1);
        int i = 1;
        while (i < number)
        {
            int r = Random.Range(0, 4);
            switch (r)
            {
                case 0:
                    if (cellsInMass[x + 1][y])
                    {
                        x++;
                        i--;
                    }
                    else
                    {
                        x++;
                    }
                    break;
                case 1:
                    if (x > 0) {
                        if (cellsInMass[x - 1][y])
                        {
                            x--;
                            i--;
                        }
                        else
                        {
                            x--;
                        }
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 2:
                    if (y > 0)
                    {
                        if (cellsInMass[x][y - 1])
                        {
                            y--;
                            i--;
                        }
                        else
                        {
                            y--;
                        }
                    }
                    else
                    {
                        i--;
                    }
                    break;
                case 3:
                    if (cellsInMass[x][y + 1])
                    {
                        y++;
                        i--;
                    }
                    else
                    {
                        y++;
                    }
                    break;
            }
            if (!cellsInMass[x][y])
            {
                cellsInMass[x][y] = true;
                Cell cl = new Cell(thisCoordinate.x + x,thisCoordinate.y + y);
                cells.Add(cl);
            }
            i++;
        }
    }

    public bool CheckForCollisions(Polymino poly)
    {
        foreach (Cell cl1 in cells)
        {
            foreach(Cell cl2 in poly.cells)
            {
                if (cl1.Coordinate.x == cl2.Coordinate.x && cl1.Coordinate.y == cl2.Coordinate.y)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void del()
    {
        foreach (Cell cl in cells)
        {
            cl.del();
        }
    }
}
