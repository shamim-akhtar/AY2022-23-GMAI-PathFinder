using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMAI
{
  #region The Node class
  // We are now going to create a map agnostic pathfinder.
  // For this to work, we will need to create an abstract 
  // class that will represent the actual type of map cell representation.

  /// <summary>
  /// A pathfinder doesnt need the visibility of the entire map.
  /// All it needs is the cost of traversing from one node to another
  /// in the neightbour.
  /// 
  /// </summary>
  public abstract class Node
  {
    public Node(Vector2Int index)
    {
      Index = index;
    }

    // This index represents the cell within the 2d array of cells.
    public Vector2Int Index;
    public abstract List<Node> GetNeighbours();
  }
  #endregion

  #region The Status of Pathfinding
  public enum PathFindingStatus
  {
    NOT_INITIALISED,
    RUNNING,
    SUCCESS,
    FAILURE,
  }
  #endregion


  public class PathFinder
  {
    #region PathFinderNode implementation
    // PathFinderNode is an encapsulation of the above Node
    // and the various costs of traversing the grid.
    // As you are searching for path, your apllication is actually
    // creating a tree at runtime. This PathFinderNode is a node within this tree.
    public class PathFinderNode
    {
      // G cost is the total traversal cost until now.
      public float G { get; private set; }

      // H cost is the heuristic cost (approximate/predicted cost from
      // the current location to the goal/destination
      public float H { get; private set; }

      // F cost is the total cost. F = G + h
      public float F { get; private set; }

      public PathFinderNode Parent { get; set; } = null;

      public Node Location { get; private set; }
      public PathFinderNode(Node node,
        PathFinderNode parent,
        float gcost,
        float hcost)
      {
        Parent = parent;
        Location = node;
        H = hcost;
        SetGCost(gcost);
      }

      public void SetGCost(float gcost)
      {
        G = gcost;
        F = G + H;
      }
    }
    #endregion

    #region The Open and the Closed lists and associated methods.
    List<PathFinderNode> mOpenList = new List<PathFinderNode>();
    List<PathFinderNode> mClosedList = new List<PathFinderNode>();

    // Return -1 if not found. Else return the index of the node.
    int IsInList(Node node, List<PathFinderNode> myList)
    {
      for(int i = 0; i < myList.Count; ++i)
      {
        if(node.Index == myList[i].Location.Index)
        {
          return i;
        }
      }
      return -1;
    }

    PathFinderNode GetLeastCostNode(List<PathFinderNode> myList)
    {
      int best_index = 0;
      float least_cost = myList[0].F;
      for(int i = 1; i < myList.Count; ++i)
      {
        if(least_cost > myList[i].F)
        {
          least_cost = myList[i].F;
          best_index = i;
        }
      }

      PathFinderNode n = myList[best_index];
      return n;
    }

    #endregion

    #region The cost functions
    // Cost functions
    // We will use delegates to calculate cost.
    // We do not want to hard code our cost function impementation.
    // We would rather allow the application developer to create 
    // their own cost function, or decide which cost function to use.
    public delegate float DelegateCostFunction(Vector2Int a, Vector2Int b);
    public DelegateCostFunction CalculateHCost;
    public DelegateCostFunction CalculateGCost;

    #endregion

    public Node Start { get; private set; }
    public Node Destination { get; private set; }

    public PathFindingStatus Status { get; private set; } = PathFindingStatus.NOT_INITIALISED;
    public PathFinderNode CurrentNode { get; private set; } = null;

    // Now we will implement the actual pathfinding.
    public bool Initialize(Node start, Node destination)
    {
      if (CalculateHCost == null || CalculateGCost == null)
        return false;

      if(Status == PathFindingStatus.RUNNING)
      {
        // Path finding is in progress. We cannot do a new pathfinding.
        return false;
      }

      Reset();

      Start = start;
      Destination = destination;

      float gcost = 0.0f;

      float hcost = CalculateHCost(Start.Index, Destination.Index);
      CurrentNode = new PathFinderNode(Start, null, gcost, hcost);

      mOpenList.Add(CurrentNode);

      Status = PathFindingStatus.RUNNING;
      return true;
    }

    public PathFindingStatus Step()
    {
      mClosedList.Add(CurrentNode);

      if(mOpenList.Count == 0)
      {
        Status = PathFindingStatus.FAILURE;
        return Status;
      }

      CurrentNode = GetLeastCostNode(mOpenList);
      mOpenList.Remove(CurrentNode);

      // check if the destination is found.
      if(CurrentNode.Location.Index == Destination.Index)
      {
        // we have found our destination.
        Status = PathFindingStatus.SUCCESS;
        return Status;
      }

      List<Node> neighbours = CurrentNode.Location.GetNeighbours();
      foreach(Node n in neighbours)
      {
        // we do our search through the neibouring cells.
        if(IsInList(n, mClosedList) == -1)
        {
          // this means the node doesnt exist in the closelist.
          // Now calculate the G cost that means the cost until now.
          float gcost = CurrentNode.G + CalculateGCost(CurrentNode.Location.Index, n.Index);
          float hcost = CalculateHCost(n.Index, Destination.Index);

          // Now check if the cell (the new node that we want to explore n)
          // is already there in the openlist.
          int id = IsInList(n, mOpenList);
          if(id == -1)
          {
            // it does not exist in the openlist.
            // so just add it to the openlist.
            PathFinderNode pfn = new PathFinderNode(n, CurrentNode, gcost, hcost);
            mOpenList.Add(pfn);
          }
          else
          {
            // The node exists in the openlist.
            // Update the G cost if the new gcost is less than the old gcost.
            float oldGCost = mOpenList[id].G;
            if(gcost < oldGCost)
            {
              mOpenList[id].Parent = CurrentNode;
              mOpenList[id].SetGCost(gcost);
            }
          }
        }
      }
      Status = PathFindingStatus.RUNNING;
      return Status;
    }
    void Reset()
    {
      mOpenList.Clear();
      mClosedList.Clear();
      Status = PathFindingStatus.NOT_INITIALISED;
      CurrentNode = null;
    }
  }
}
