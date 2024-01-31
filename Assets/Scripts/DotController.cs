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
    public bool isMatched = false;

    private MatchFinder matchFinder; 
    private Board board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;
    public float swipeResist = 1f;


    void Start()
    {
        board = FindObjectOfType<Board>();
        matchFinder = FindObjectOfType<MatchFinder>();
        /*targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
        previousRow = row;
        previousColumn = column;*/
    }
    void Update()
    {
        //FindMatches();
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(0f, 0f, 0f, .2f);
        }
        targetX = column;
        targetY = row;
        //Swap in Rows
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move towards target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
            if (board.allDots[column,row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            matchFinder.FindAllMatches();
        }
        else
        {
            //Directly set position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }
        //Swap in Columns
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move towards target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
            if (board.allDots[column,row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            matchFinder.FindAllMatches();
        }
        else
        {
            //Directly set position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCoroutine()
    {
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
                board.currentState = GameState.MOVE;
            }else
            {
                board.DestroyMatches();
            }
            otherDot = null;
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
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI ;
            //Debug.Log(swipeAngle);
            MovePieces();
            board.currentState = GameState.WAIT;
        }
        else
        {
            board.currentState = GameState.MOVE;
        }
    }

    private void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) 
        {
            //Right Swipe
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<DotController>().column -= 1;
            column += 1;
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) 
        {
            //Up Swipe
            otherDot = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<DotController>().row -= 1;
            row += 1;
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            otherDot = board.allDots[column - 1 , row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<DotController>().column += 1;
            column -= 1;
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Dowm Swipe
            otherDot = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<DotController>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCoroutine());
    }

    //Old matchfinder, unused currently
    private void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1.CompareTag(this.gameObject.tag) && rightDot1.CompareTag(this.gameObject.tag))
            {
                leftDot1.GetComponent<DotController>().isMatched = true;
                rightDot1.GetComponent<DotController>().isMatched = true;
                isMatched = true;
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject topDot1 = board.allDots[column, row + 1 ];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (topDot1.CompareTag(this.gameObject.tag) && downDot1.CompareTag(this.gameObject.tag))
            {
                topDot1.GetComponent<DotController>().isMatched = true;
                downDot1.GetComponent<DotController>().isMatched = true;
                isMatched = true;
            }
        }
    }
}
