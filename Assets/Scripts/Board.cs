using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WAIT,
    MOVE
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.MOVE;
    public int width;
    public int height;
    public int offset;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject[,] allDots;
    public DotController currentDot;
    public GameObject destroyEffect;
    private BackgroundTile[,] allTiles;
    private MatchFinder matchFinder;

    
    // Start is called before the first frame update
    void Start()
    {
        matchFinder = FindObjectOfType<MatchFinder>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offset);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = $"( {i}, {j} )";
                int dotToUse = Random.Range(0, dots.Length);

                int retries = 0;
                while (WillMatch(i,j,dots[dotToUse]) && retries < 200)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    retries++;
                }
                retries = 0;
                
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<DotController>().row = j;
                dot.GetComponent<DotController>().column = i;
                dot.transform.parent = this.transform;
                dot.name = $"( {i}, {j} )";
                allDots[i, j] = dot;
            }
            
        }
    }

    private bool WillMatch(int column, int row, GameObject piece)
    {
        if (column > 1)
        {
            if (allDots[column - 1,row].CompareTag(piece.tag) && allDots[column - 2, row].CompareTag(piece.tag))
            {
                return true;
            }
        }
        if (row > 1)
        {
            if (allDots[column,row - 1].CompareTag(piece.tag) && allDots[column, row - 2].CompareTag(piece.tag))
            {
                return true;
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column,row].GetComponent<DotController>().isMatched)
        {
            //Creating rockets based on length of matches
            if (matchFinder.currentMatches.Count == 4 || matchFinder.currentMatches.Count == 7)
            {
                matchFinder.CheckRockets();
            }
            
            matchFinder.currentMatches.Remove(allDots[column, row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[column,row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    DestroyMatchesAt(i,j);
                }
            }
        }
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<DotController>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<DotController>().row = j;
                    piece.GetComponent<DotController>().column = i;
                    piece.transform.parent = this.transform;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<DotController>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(.3f);
        
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.3f);
            DestroyMatches();
        }
        matchFinder.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(1f);
        currentState = GameState.MOVE;

    }
}
