using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NCell {
    public GameObject cell;
    public bool empty;
    public Vector2 Position;
    public Coordinate Coordinate;
    public NCell(int X, int Y, bool _empty)
    {
        empty = _empty;
        Coordinate coord = new Coordinate(X, Y);
        Coordinate = coord;
        Position = new Vector2(coord.x * NCellController.CellSize + NCellController.CellSize / 2, coord.y * NCellController.CellSize + NCellController.CellSize / 2);
        cell = null;
        NCellController.cells.Add(this);
    }
    public NCell(int X, int Y)
    {
        empty = false;
        Coordinate coord = new Coordinate(X, Y);
        Coordinate = coord;
        Position = new Vector2(coord.x * NCellController.CellSize + NCellController.CellSize / 2, coord.y * NCellController.CellSize + NCellController.CellSize / 2);
        cell = PoolScript.instance.GetObjectFromPool("Cell", Position, Quaternion.Euler(0, 0, 0));
        cell.transform.parent = NCellController.instance.transform;
        //NCellController.cells.Add(this);
    }
    public NCell Manifest()
    {
        if (NCellController.FindCell(Coordinate, NCellController.cells) != null)
        {
            NCellController.FindCell(Coordinate, NCellController.cells).Del();
        }
        if (!empty && cell == null)
        {
            //NCellController.cells.Add(this);
            return new NCell(Coordinate.x, Coordinate.y);
        }
        else
        {
            //NCellController.cells.Add(this);
            return this;
        }
    }
    public void Del()
    {
        NCellController.cells.Remove(NCellController.FindCell(Coordinate, NCellController.cells));
        if (cell)
        {
            this.cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        }
        if (cell != null)
        {
            PoolScript.instance.ReturnObjectToPool(cell);
        }
    }
}
[System.Serializable]
public class Coordinate
{
    public int x;
    public int y;
    public Coordinate(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public static Coordinate RandomCoordinateToStartPolymino(int n)
    {
        bool output = false;
        int _y = 0;
        int _x = 0;
        while (!output)
        {
            output = true;
            _x = Random.Range(0, n);
            _y = Random.Range(0, n);
            foreach (Polymino poly in CellController.polyminos)
            {
                foreach (Cell cl in poly.cells)
                {
                    if (cl.Coordinate.x == _x || cl.Coordinate.y == _y)
                    {
                        output = false;
                    }
                }
            }
        }
        return new Coordinate(_x, _y);
    }
    public bool Equals(Coordinate _coord)
    {
        if (_coord == null) return false;
        if (_coord.x == x && _coord.y == y) return true;
        return false;
    }
}
