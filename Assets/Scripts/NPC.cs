using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GMAI
{
  public class NPC : MonoBehaviour
  {
    // We are going to use a slightly different way to 
    // implement a MoveToPoint.
    // We will continuously poll and check if there is any 
    // waypoint in the Queue. If there is use a coroutine
    // to apply the movement. Else do nothing.
    // Here you will see the use of a Queue container.
    // Here you will also see the use of coroutine in several ways.

    private Queue<Vector3> mWaypoints = new Queue<Vector3>();
    private PathFinder mPF = new PathFinder();

    public float Speed = 1.0f;

    private GridCell mStart = null;
    private GridCell mDestination = null;

    public void SetStart(GridCell start)
    {
      mStart = start;
    }

    private void Start()
    {
      mPF.CalculateGCost = Grid_Viz.EuclideanCost;
      mPF.CalculateHCost = Grid_Viz.ManhattanCost;

      StartCoroutine(Coroutine_MoveTo());
    }

    IEnumerator Coroutine_MoveTo()
    {
      while(true)
      {
        while(mWaypoints.Count > 0)
        {
          yield return StartCoroutine(Coroutine_MoveToPoint(mWaypoints.Dequeue()));
        }
        yield return null;
      }
    }

    IEnumerator Coroutine_MoveToPoint(Vector3 endP)
    {
      float duration = (endP - transform.position).magnitude / Speed;
      yield return StartCoroutine(Coroutine_MoveToPointBasedOnTime(endP, duration));
    }

    IEnumerator Coroutine_MoveToPointBasedOnTime(Vector3 endP, float duration)
    {
      float elaspedTime = 0.0f;
      Vector3 startP = new Vector3(
        transform.position.x,
        transform.position.y,
        -1.0f);

      while(elaspedTime < duration)
      {
        transform.position = Vector3.Lerp(startP, endP, elaspedTime / duration);
        elaspedTime += Time.deltaTime;
        yield return null;
      }
    }

    public void Add(Vector2 pt)
    {
      mWaypoints.Enqueue(new Vector3(
        pt.x,
        pt.y,
        -1.0f));
    }

    public void SetDestination(GridCell cell)
    {
      if(mPF.Status == PathFindingStatus.RUNNING)
      {
        Debug.Log("PathFinder is running");
        return;
      }

      mWaypoints.Clear();

      mPF.Initialize(mStart, cell);

      mDestination = cell;

      StartCoroutine(Coroutine_FindPathSteps());
    }

    IEnumerator Coroutine_FindPathSteps()
    {
      while(mPF.Status == PathFindingStatus.RUNNING)
      {
        mPF.Step();
        yield return null;
      }
      if(mPF.Status == PathFindingStatus.FAILURE)
      {
        Debug.Log("Failed to find path to destination");
      }
      List<Vector2Int> reverse_indices = new List<Vector2Int>();
      if(mPF.Status == PathFindingStatus.SUCCESS)
      {
        PathFinder.PathFinderNode n = mPF.CurrentNode;
        while(n != null)
        {
          reverse_indices.Add(n.Location.Index);
          n = n.Parent;
        }

        // we found the path in reverse.
        for(int i = reverse_indices.Count - 1; i >= 0; i--)
        {
          Add(new Vector2(reverse_indices[i].x, reverse_indices[i].y));
        }
        //mStart = reverse_indices
        mStart = mDestination;
      }
    }
  }
}
