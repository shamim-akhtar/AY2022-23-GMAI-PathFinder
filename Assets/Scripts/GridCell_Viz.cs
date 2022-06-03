using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMAI
{
  /// <summary>
  /// class GridCell_Viz
  /// We will use this class to change the visualisation
  /// properties of the grid cell. Later on we will associate this
  /// class with the pathfinder Node.
  /// </summary>
  public class GridCell_Viz : MonoBehaviour
  {
    public SpriteRenderer InnerSpriteRenderer;
    public SpriteRenderer OuterSpriteRenderer;

    //public bool Walkable = true;
    public bool Walkable
    {
      get;
      set;
    } = true;

    public void SetColor_Inner(Color col)
    {
      InnerSpriteRenderer.color = col;
    }
    public void SetColor_Outer(Color col)
    {
      OuterSpriteRenderer.color = col;
    }
  }
}
