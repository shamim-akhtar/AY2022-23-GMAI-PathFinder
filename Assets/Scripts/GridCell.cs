using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMAI
{
  public class GridCell : Node
  {
    Grid_Viz mGrid;
    public GridCell(Grid_Viz grid, Vector2Int index)
      : base(index)
    {
      mGrid = grid;
    }
    public override List<Node> GetNeighbours()
    {
      return mGrid.GetNeighbours(Index);
    }
  }

}