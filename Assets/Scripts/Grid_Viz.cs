using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMAI
{
  /// <summary>
  /// This Monobehavior component is responsible for creating the
  /// grid and visualise it.
  /// </summary>
  public class Grid_Viz : MonoBehaviour
  {
    // The following two variables represent the
    // size of the grid.
    public int Size_X;
    public int Size_Y;

    // This is the prefab for each cell within the grid.
    public GameObject PrefabGridCell;

    // This is the prefab for our 2d NPC.
    // You can change it to your model/sprite later on.
    public GameObject PrefabNPC;

    // The visual representation of the destination.
    public GameObject Destination;

    // Lets define some colors for different state of the cell.
    public Color COLOR_WALKABLE = Color.cyan;
    public Color COLOR_NON_WALKABLE = Color.black;

    // We also need to represent the grid as a 2d array.
    private GameObject[,] mGridCells = null;
    private GridCell[,] mCells = null;
    private NPC mNpc = null;

    // Start is called before the first frame update
    void Start()
    {
      CreateNPC();
      CreateGrid();
    }

    void CreateNPC()
    {
      GameObject obj = Instantiate(PrefabNPC, new Vector3(0, 0, 0.0f), Quaternion.identity);
      mNpc = obj.GetComponent<NPC>();
    }

    void CreateGrid()
    {
      // initialize the 2d array.
      mGridCells = new GameObject[Size_X, Size_Y];
      mCells = new GridCell[Size_X, Size_Y];

      for (int x = 0; x < Size_X; ++x)
      {
        for (int y = 0; y < Size_Y; ++y)
        {
          GameObject obj = Instantiate(PrefabGridCell, new Vector3(x, y, 1.0f), Quaternion.identity);
          obj.name = "cell_" + x + "_" + y;
          obj.transform.SetParent(transform);
          mGridCells[x, y] = obj;
          mCells[x, y] = new GridCell(this, new Vector2Int(x, y));
        }
      }
      mNpc.SetStart(mCells[0, 0]);
    }

    // Update is called once per frame
    void Update()
    {
      if (Input.GetMouseButtonDown(0))
      {
        RaycastAnd_ToggleWalkable();
      }
      if (Input.GetMouseButtonDown(1))
      {
        RaycastAnd_SetDestination();
      }
    }

    void RaycastAnd_SetDestination()
    {
      Vector2 rayPos = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

      RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0.0f);
      if (hit)
      {
        GameObject obj = hit.transform.gameObject;
        Destination.transform.position = new Vector3(
          obj.transform.position.x,
          obj.transform.position.y,
          -0.2f);
        Destination.SetActive(true);
        //mNpc.Add(new Vector2(obj.transform.position.x, obj.transform.position.y));
        int x = (int)obj.transform.position.x;
        int y = (int)obj.transform.position.y;
        mNpc.SetDestination(mCells[x, y]);
      }

    }

    void RaycastAnd_ToggleWalkable()
    {
      Vector2 rayPos = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

      RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0.0f);
      if (hit)
      {
        GameObject obj = hit.transform.gameObject;
        GridCell_Viz gridCell_Viz = obj.GetComponent<GridCell_Viz>(); 
        if(gridCell_Viz)
        {
          ToggleWalkable(gridCell_Viz);
        }
      }
    }

    void ToggleWalkable(GridCell_Viz cell)
    {
      if(cell.Walkable)
      {
        cell.Walkable = false;
        // we also want to visually represent the change.
        cell.SetColor_Inner(COLOR_NON_WALKABLE);
      }
      else
      {
        cell.Walkable = true;
        // we also want to visually represent the change.
        cell.SetColor_Inner(COLOR_WALKABLE);
      }
    }

    public List<Node> GetNeighbours(Vector2Int index)
    {
      List<Node> n = new List<Node>();
      int x = index.x;
      int y = index.y;

      // check up.
      if(y < Size_Y - 1)
      {
        int i = x;
        int j = y + 1;
        if(mGridCells[i,j].GetComponent<GridCell_Viz>().Walkable)
        {
          n.Add(mCells[i, j]);
        }
      }
      // check right.
      if (x < Size_X - 1)
      {
        int i = x + 1;
        int j = y;
        if (mGridCells[i, j].GetComponent<GridCell_Viz>().Walkable)
        {
          n.Add(mCells[i,j]);
        }
      }
      // check bottom.
      if (y > 0)
      {
        int i = x;
        int j = y - 1;
        if (mGridCells[i, j].GetComponent<GridCell_Viz>().Walkable)
        {
          n.Add(mCells[i, j]);
        }
      }
      // check left.
      if (x > 0)
      {
        int i = x - 1;
        int j = y;
        if (mGridCells[i, j].GetComponent<GridCell_Viz>().Walkable)
        {
          n.Add(mCells[i, j]);
        }
      }
      return n;
    }

    static public float ManhattanCost(Vector2Int a, Vector2Int b)
    {
      return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    static public float EuclideanCost(Vector2Int a, Vector2Int b)
    {
      return Mathf.Sqrt(
        (a.x - b.x) * (a.x - b.x) +
        (a.y - b.y) * (a.y - b.y));
    }
  }
}