using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DotController : MonoBehaviour
{
    [Header("Board Variables")]
    public int row;
    public int column;
    public int previousRow;
    public int previousColumn;
    public int targetX;
    public int targetY;
    public bool isMatched;
    
    [Header("Swipe Variables")]
    public float swipeAngle;
    public float swipeResist = 1f;

    [Header("Powerup Variables")] 
    public bool isColorBomb;
    public bool isColumnRocket;
    public bool isRowRocket;
    public bool isBigBomb;
    public GameObject rowRocket;
    public GameObject columnRocket;
    public GameObject colorBomb;
    public GameObject bigBomb;
    
    private MatchFinder matchFinder; 
    private Board board;
    internal GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    


    void Start()
    {
        isColumnRocket = false;
        isRowRocket = false;
        isColorBomb = false;
        isBigBomb = false;
        
        board = FindObjectOfType<Board>();
        matchFinder = FindObjectOfType<MatchFinder>();
        
        /*targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
        previousRow = row;
        previousColumn = column;*/
    }
    
    //Testing powerups
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            
        }
    }

    void Update()
    {
        targetX = column;
        targetY = row;
        //Swap in Rows
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move towards target
            var position = transform.position;
            tempPosition = new Vector2(targetX, position.y);
            position = Vector2.Lerp(position, tempPosition, .1f);
            transform.position = position;
            if (board.allDots[column,row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            matchFinder.FindAllMatches();
        }
        else
        {
            //Directly set position
            var transform1 = transform;
            tempPosition = new Vector2(targetX, transform1.position.y);
            transform1.position = tempPosition;
        }
        //Swap in Columns
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move towards target
            var position = transform.position;
            tempPosition = new Vector2(position.x, targetY);
            position = Vector2.Lerp(position, tempPosition, .1f);
            transform.position = position;
            if (board.allDots[column,row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            matchFinder.FindAllMatches();
        }
        else
        {
            //Directly set position
            var transform1 = transform;
            tempPosition = new Vector2(transform1.position.x, targetY);
            transform1.position = tempPosition;
        }
    }

    private IEnumerator CheckMoveCoroutine()
    {
        //Powerups which works outside of matches
        if (isColorBomb)
        {
            matchFinder.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }else if (otherDot.GetComponent<DotController>().isColorBomb)
        {
            matchFinder.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<DotController>().isMatched = true;
        }else if (isBigBomb)
        {
            matchFinder.MatchAdjacentPieces(this.column,this.row);
        }else if (otherDot.GetComponent<DotController>().isBigBomb)
        {
            DotController otherDotController = otherDot.GetComponent<DotController>();
            matchFinder.MatchAdjacentPieces(otherDotController.column,otherDotController.row);
        }

        yield return new WaitForSeconds(.4f);
        
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<DotController>().isMatched)
            {
                otherDot.GetComponent<DotController>().row = row;
                otherDot.GetComponent<DotController>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.MOVE;
            }else
            {
                board.DestroyMatches();
            }
        }
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.MOVE)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.MOVE)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.WAIT;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI ;
            //Debug.Log(swipeAngle);
            MovePiecesCalculate();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.MOVE;
        }
    }

    private void MovePieces(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        otherDot.GetComponent<DotController>().column += -1 * (int)direction.x;
        otherDot.GetComponent<DotController>().row += -1 * (int)direction.y;
        column += (int)direction.x;
        row += (int)direction.y;
        StartCoroutine(CheckMoveCoroutine());
    }
    private void MovePiecesCalculate()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) 
        {
            //Right Swipe
            MovePieces(Vector2.right);
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) 
        {
            //Up Swipe
            MovePieces(Vector2.up);
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            MovePieces(Vector2.left);
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Dowm Swipe
            MovePieces(Vector2.down);
        }

        board.currentState = GameState.MOVE;
    }

    public void MakeRowRocket()
    {
        isRowRocket = true;
        GameObject rocket = Instantiate(rowRocket, transform.position, Quaternion.identity);
        rocket.transform.parent = this.transform;
    }

    public void MakeColumnRocket()
    {
        isColumnRocket = true;
        GameObject rocket = Instantiate(columnRocket, transform.position, Quaternion.identity);
        rocket.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject cBomb = Instantiate(colorBomb, transform.position, Quaternion.identity);
        cBomb.transform.parent = this.transform;
    }

    public void MakeBigBomb()
    {
        isBigBomb = true;
        GameObject bomb = Instantiate(bigBomb, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }
}
