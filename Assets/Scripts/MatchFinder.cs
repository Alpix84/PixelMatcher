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

    private List<GameObject> IsBigBomb(DotController dot1, DotController dot2, DotController dot3)
    {
        List<GameObject> currentDots = new();
        
        if (dot1.isBigBomb)
        {
            currentDots.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }
        if (dot2.isBigBomb)
        {
            currentDots.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }
        if (dot3.isBigBomb)
        {
            currentDots.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }
        
        return currentDots;
    }

    private List<GameObject> IsRowBomb( DotController dot1, DotController dot2, DotController dot3)
    {
        List<GameObject> currentDots = new();
        
        if (dot1.isRowRocket)
        {
            currentDots.Union(GetRowPieces(dot1.row));
        }
        if (dot2.isRowRocket)
        {
            currentDots.Union(GetRowPieces(dot2.row));
        }
        if (dot3.isRowRocket)
        {
            currentDots.Union(GetRowPieces(dot3.row));
        }
        
        return currentDots;
    }
    
    private List<GameObject> IsColumnBomb( DotController dot1, DotController dot2, DotController dot3)
    {
        List<GameObject> currentDots = new();
        
        if (dot1.isColumnRocket)
        {
            currentDots.Union(GetColumnPieces(dot1.column));
        }
        if (dot2.isColumnRocket)
        {
            currentDots.Union(GetColumnPieces(dot2.column));
        }
        if (dot3.isColumnRocket)
        {
            currentDots.Union(GetColumnPieces(dot3.column));
        }
        
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<DotController>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
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
                    DotController currentDotController = currentDot.GetComponent<DotController>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            DotController rightDotController = rightDot.GetComponent<DotController>();
                            DotController leftDotController = leftDot.GetComponent<DotController>();
                            if (leftDot.CompareTag(currentDot.tag) && rightDot.CompareTag(currentDot.tag))
                            {
                                currentMatches.Union(IsRowBomb(leftDotController, currentDotController,
                                    rightDotController));

                                currentMatches.Union(IsColumnBomb(leftDotController, rightDotController,
                                    currentDotController));

                                currentMatches.Union(IsBigBomb(leftDotController, rightDotController,
                                    currentDotController));
                                
                                GetNearbyPieces(currentDot,leftDot,rightDot);
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject topDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (topDot != null && downDot != null)
                        {
                            DotController downDotController = downDot.GetComponent<DotController>();
                            DotController topDotController = topDot.GetComponent<DotController>();
                            if (topDot.CompareTag(currentDot.tag) && downDot.CompareTag(currentDot.tag))
                            {
                                currentMatches.Union(IsColumnBomb(currentDotController, topDotController,
                                    downDotController));

                                currentMatches.Union(IsRowBomb(currentDotController, topDotController, downDotController));

                                currentMatches.Union(IsBigBomb(currentDotController, topDotController, downDotController));

                                
                                GetNearbyPieces(currentDot,topDot,downDot);
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


    public void MatchAdjacentPieces(int column, int row)
    {
        currentMatches.Union(GetAdjacentPieces(column, row));
    }
    private List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new();

        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                if (i >=0 && i < board.width && j >=0 && j < board.height)
                {
                    dots.Add(board.allDots[i, j]);
                    board.allDots[i, j].GetComponent<DotController>().isMatched = true;
                }
            }
        }
        return dots;
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
