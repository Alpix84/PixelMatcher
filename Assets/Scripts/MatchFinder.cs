using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchFinder : MonoBehaviour
{
    public List<GameObject> currentMatches = new();
    private Board board;

    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }

    private IEnumerator FindAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];
                if (currentDot != null)
                {
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            if (leftDot.CompareTag(currentDot.tag) && rightDot.CompareTag(currentDot.tag))
                            {
                                if (currentDot.GetComponent<DotController>().isRowRocket 
                                    || leftDot.GetComponent<DotController>().isRowRocket 
                                    || rightDot.GetComponent<DotController>().isRowRocket
                                    )
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                
                                //check in center piece(s) of matches
                                if (currentDot.GetComponent<DotController>().isColumnRocket)
                                {
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                
                                //check in sides of matches
                                if (leftDot.GetComponent<DotController>().isColumnRocket)
                                {
                                    currentMatches.Union(GetColumnPieces(i - 1));
                                }
                                if (rightDot.GetComponent<DotController>().isColumnRocket)
                                {
                                    currentMatches.Union(GetColumnPieces(i + 1));
                                }
                                
                                if (!currentMatches.Contains(leftDot))
                                {
                                    currentMatches.Add(leftDot);
                                }
                                leftDot.GetComponent<DotController>().isMatched = true;
                                if (!currentMatches.Contains(rightDot))
                                {
                                    currentMatches.Add(rightDot);
                                }
                                rightDot.GetComponent<DotController>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<DotController>().isMatched = true;
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject topDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (topDot != null && downDot != null)
                        {
                            if (topDot.CompareTag(currentDot.tag) && downDot.CompareTag(currentDot.tag))
                            {
                                if (currentDot.GetComponent<DotController>().isColumnRocket 
                                    || topDot.GetComponent<DotController>().isColumnRocket
                                    || downDot.GetComponent<DotController>().isColumnRocket
                                   )
                                {
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                
                                //check in central piece(s) of matches
                                if (currentDot.GetComponent<DotController>().isRowRocket)
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                //check in sides of matches
                                if (topDot.GetComponent<DotController>().isRowRocket)
                                {
                                    currentMatches.Union(GetRowPieces(j + 1));
                                }
                                if (downDot.GetComponent<DotController>().isRowRocket)
                                {
                                    currentMatches.Union(GetRowPieces(j - 1));
                                }
                                
                                if (!currentMatches.Contains(topDot))
                                {
                                    currentMatches.Add(topDot);
                                }
                                topDot.GetComponent<DotController>().isMatched = true;
                                if (!currentMatches.Contains(downDot))
                                {
                                    currentMatches.Add(downDot);
                                }
                                downDot.GetComponent<DotController>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<DotController>().isMatched = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allDots[i, j] != null)
                {
                    if (board.allDots[i, j].CompareTag(color))
                    {
                        board.allDots[i, j].GetComponent<DotController>().isMatched = true;
                    }
                }
            }
        }
    }
    private List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new();

        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column,i] != null)
            {
                dots.Add(board.allDots[column,i]);
                board.allDots[column, i].GetComponent<DotController>().isMatched = true;
            }
        }
        return dots;
    }
    
    private List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new();

        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row ].GetComponent<DotController>().isMatched = true;
            }
        }
        return dots;
    }

    public void CheckRockets()
    {
        if (board.currentDot != null)
        {
            if (board.currentDot.isMatched)
            {
                board.currentDot.isMatched = false;
                int typeOfRocket = Random.Range(0, 100);
                if (typeOfRocket < 50)
                {
                    board.currentDot.MakeRowRocket();
                }
                else
                {
                    board.currentDot.MakeColumnRocket();
                }

            }else if (board.currentDot.otherDot != null)
            {
                DotController otherDot = board.currentDot.otherDot.GetComponent<DotController>();
                if (otherDot.isMatched)
                {
                    otherDot.isMatched = false;
                    int typeOfRocket = Random.Range(0, 100);
                    if (typeOfRocket < 50)
                    {
                        otherDot.MakeRowRocket();
                    }
                    else
                    {
                        otherDot.MakeColumnRocket();
                    }
                }
            }
        }
    }

}
