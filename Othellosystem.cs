using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Othellosystem : MonoBehaviour
{
    const int Field_Size_X = 8;
    const int Field_Size_Y = 8;

    public enum ePieceState
    {
        None,
        Front,
        Back,
    }

    private GameObject _BoardObject = null;

    private GameObject [,] _FieldObject = new GameObject [Field_Size_X, Field_Size_Y];

    private OthelloPiece [,] _Piece = new OthelloPiece [Field_Size_X, Field_Size_Y];

    private ePieceState [,] _FieldPieceState = new ePieceState[Field_Size_X, Field_Size_Y];

    private GameObject _CursorObject = null;
    private GameObject _BackObject = null;
    
    [SerializeField] GameObject _BoardPrefab = null;
    [SerializeField] GameObject _PiecePrefab = null;
    [SerializeField] GameObject _CursorPrefab = null;
    [SerializeField] GameObject _XlinePrefab = null;
    [SerializeField] GameObject _YlinePrefab = null;
    [SerializeField] GameObject _BackPrefab = null;

    private int _CursorX = 0;
    private int _CursorY = 0;

    private ePieceState _NowPiece = ePieceState.Back;

    class Position
    {
        public int _x;
        public int _y;

        public Position(int x, int y)
        {
            _x = x;
            _y = y;
        }
    }

    // 処理の方向、上から時計回り
    int[] Turn_Direction_X = new int[] {0, 1, 1, 1, 0, -1, -1, -1};
    int[] Turn_Direction_Y = new int[] {1, 1, 0, -1, -1, -1, 0, 1};

    public Text _Turn;
    public Text _ScoreText;
    
    private const string WhiteTurn = "白のターン";
    private const string BlackTurn = "黒のターン";

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < Field_Size_X; i++)
        {
            for (int j = 0; j < Field_Size_Y; j++)
            {
                GameObject NewObject = GameObject.Instantiate<GameObject>(_PiecePrefab);
                OthelloPiece NewPiece = NewObject.GetComponent<OthelloPiece>();
                NewObject.transform.localPosition = new Vector3(i - 3.5f, 0.0f, j - 3.5f);
                _FieldObject[i, j] = NewObject;
                _Piece[i, j] = NewPiece;
                _FieldPieceState[i, j] = ePieceState.None;
            }
            _FieldPieceState[3, 3] = ePieceState.Back;
            _FieldPieceState[4, 3] = ePieceState.Front;
            _FieldPieceState[3, 4] = ePieceState.Front;
            _FieldPieceState[4, 4] = ePieceState.Back;
        }

        _BoardObject = GameObject.Instantiate<GameObject>(_BoardPrefab);
        _CursorObject = GameObject.Instantiate<GameObject>(_CursorPrefab);
        _BackObject = GameObject.Instantiate<GameObject>(_BackPrefab);

        // ラインの生成
        for (int i = 0; i <= 8; i++)
        {
            GameObject temp = GameObject.Instantiate<GameObject>(_XlinePrefab);
            LineRenderer XLine = temp.GetComponent<LineRenderer>();
            XLine.SetPosition(0, new Vector3(-4.0f, 0.55f, i - 4.0f));
            XLine.SetPosition(1, new Vector3(4.0f, 0.55f, i - 4.0f));
        }
        for (int j = 0; j <= 8; j++)
        {
            GameObject temp = GameObject.Instantiate<GameObject>(_YlinePrefab);
            LineRenderer YLine = temp.GetComponent<LineRenderer>();
            YLine.SetPosition(0, new Vector3(j - 4.0f, 0.51f, 4.0f));
            YLine.SetPosition(1, new Vector3(j - 4.0f, 0.51f, -4.0f));
        }

        _Turn.text = BlackTurn;
    }

    // Update is called once per frame
    void Update()
    {
        int dX = 0;
        int dY = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            dY += 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            dY -= 1;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            dX += 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            dX -= 1;
        }
        _CursorX += dX;
        _CursorY += dY;
        
        // オセロ盤から出ないように
        if (_CursorX < 0)
        {
            _CursorX = 0;
        }
        else if (_CursorX > 7)
        {
            _CursorX = 7;
        }

        if (_CursorY < 0)
        {
            _CursorY = 0;
        }
        else if (_CursorY > 7)
        {
            _CursorY = 7;
        }

        _CursorObject.transform.localPosition = new Vector3(_CursorX - 3.5f, 0.5f, _CursorY - 3.5f);

         if (Input.GetKeyDown(KeyCode.Return))
        {
            if (0 <= _CursorX && _CursorX < Field_Size_X && 0 <= _CursorY && _CursorY < Field_Size_Y && _FieldPieceState[_CursorX, _CursorY] == ePieceState.None && Turn(false) > 0)
            {
                _FieldPieceState[_CursorX, _CursorY] = _NowPiece;
                Turn(true);
                _NowPiece = ((_NowPiece == ePieceState.Back) ? ePieceState.Front : ePieceState.Back);
                _Turn.text = ((_NowPiece == ePieceState.Back) ? BlackTurn : WhiteTurn);
            }
        }

        // ピースの状態反映
        UpdatePieceState();

        // 全て埋まれば終了
        UpdateFieldState();

        // 置ける場所がないとパス、２が返る
        PutCheck();
        if (PutCheck() == 2)
        {
            _NowPiece = ((_NowPiece == ePieceState.Back) ? ePieceState.Front : ePieceState.Back);
            _Turn.text = ((_NowPiece == ePieceState.Back) ? BlackTurn : WhiteTurn);
            
            // 両方パスだと試合終了
            PutCheck();
            if (PutCheck() == 2)
            {
                GameOver();
            }
        }
    }

    int Turn(bool isTurn)
    {
        ePieceState AnotherColor = ((_NowPiece == ePieceState.Back) ? ePieceState.Front : ePieceState.Back);
        bool Sandwich = false;
        List<Position> PositionList = new List<Position>();
        int count = 0;

        for (int i = 0; i < 8; i ++)
        {
            int deltaX = 0, deltaY = 0; 
            int x = _CursorX;
            int y = _CursorY;
            deltaX += Turn_Direction_X[i];
            deltaY += Turn_Direction_Y[i];
            Sandwich = false;
            PositionList.Clear();
            while (true)
            {
                x += deltaX;
                y += deltaY;
                if (!(0 <= x && x < Field_Size_X && 0 <= y  && y < Field_Size_Y))
                {
                    break;
                }
                
                if (_FieldPieceState[x, y] == AnotherColor)
                {
                    PositionList.Add(new Position(x, y));
                }
                else if (_FieldPieceState[x, y] == _NowPiece)
                {
                    Sandwich = true;
                    break;
                }
                else
                {
                    break;
                }
            }
            // ひっくり返し処理
            if (Sandwich)
            {
                count += PositionList.Count;
                if (isTurn)
                {
                    for (int j = 0; j < PositionList.Count; j++)
                    {
                        Position pos = PositionList[j];
                        _FieldPieceState[pos._x, pos._y] = _NowPiece;
                        _Piece[pos._x, pos._y].StartTurnAnimation();
                    }
                }
            }
        }
        return count;
    }

    //ピースの状態反映
    void UpdatePieceState()
    {   
        for (int i = 0; i < Field_Size_X; i++)
        {
            for(int j = 0; j < Field_Size_Y; j++)
            {
                _Piece[i, j].SetState(_FieldPieceState[i, j]);
            }
        }
    }

    // 全て埋まれば終了
    void UpdateFieldState()
    {
        int NoneCount = 0;

        for (int i = 0; i < Field_Size_X; i++)
        {
            for(int j = 0; j < Field_Size_Y; j++)
            {
                if (_FieldPieceState[i, j] == ePieceState.None)
                {
                    NoneCount ++;
                }
            }
        }

        if (NoneCount == 0)
        {
            GameOver();
        }
        else
        {
            return;
        }
    }

    // 試合終了処理
    void GameOver()
    {
        int Black = BlackScore();
        int White = WhiteScore();
        _Turn.text = "ゲーム終了";
        if (White > Black)
            _ScoreText.text = "White win!!\r\n" + White + ":" + Black;
        else if (Black > White)
            _ScoreText.text = "Black win!!\r\n" + Black + ":" + White;
        else
            _ScoreText.text = "Draw!!\r\n" + White + ":" + Black;
    }

    // 黒の集計
    private int BlackScore()
    {
        int count = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (_FieldPieceState[x, y] == ePieceState.Back)
                {
                    count++;
                }
            }
        }
        return count;
    }

    // 白の集計
    private int WhiteScore()
    {
        int count = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (_FieldPieceState[x, y] == ePieceState.Front)
                {
                    count++;
                }
            }
        }
        return count;
    }

    // 置ける場所の確認
    int CanTurnStone(int Ax, int Ay)
    {
        ePieceState AnotherColor = ((_NowPiece == ePieceState.Back) ? ePieceState.Front : ePieceState.Back);
        int CanTurnPiece = 0;
        int CanPutCount = 0;
        if (_FieldPieceState[Ax, Ay] == ePieceState.None)
        {
            for (int i = 0; i < 8; i ++)
            {
                int deltaX = 0, deltaY = 0;
                int x = Ax;
                int y = Ay;
                deltaX += Turn_Direction_X[i];
                deltaY += Turn_Direction_Y[i];
                while (true)
                {
                    x += deltaX;
                    y += deltaY;
                    if (!(0 <= x && x < Field_Size_X && 0 <= y  && y < Field_Size_Y))
                    {
                        CanTurnPiece = 0;
                        break;
                    }
                    
                    if (_FieldPieceState[x, y] == AnotherColor)
                    {
                        CanTurnPiece++;
                    }
                    else if (_FieldPieceState[x, y] == _NowPiece)
                    {
                        if (CanTurnPiece > 0)
                        {
                            CanPutCount++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        CanTurnPiece = 0;
                        break;
                    }
                }
            }
        }
        return CanPutCount;
    }

    // 置ける場所があると1を返す
    int PutCheck()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (CanTurnStone(x, y) > 0)
                {
                    return 1;
                }
            }
        }
        return 2;
    }
}