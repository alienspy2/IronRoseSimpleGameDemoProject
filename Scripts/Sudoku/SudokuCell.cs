// ------------------------------------------------------------
// @file    SudokuCell.cs
// @brief   개별 스도쿠 셀 하나의 시각적 상태와 숫자 표시를 관리하는 일반 클래스.
//          MonoBehaviour가 아니며, SudokuBoard에서 생성/관리된다.
// @deps    CellState, RoseEngine (GameObject, RectTransform, UIImage, UIText, Sprite, Color)
// @exports
//   class SudokuCell
//     Row: int (readonly)                           -- 셀의 행 (0~8)
//     Col: int (readonly)                           -- 셀의 열 (0~8)
//     CellObject: GameObject                        -- 셀의 루트 GameObject
//     State: CellState                              -- 현재 시각적 상태
//     IsGiven: bool                                 -- 고정 숫자 여부 (배경 스프라이트 결정에 사용)
//     SetNumber(int number, bool isGiven): void     -- 숫자 표시. 0이면 빈칸. isGiven이면 짙은 남색, 아니면 연필 색상.
//     SetState(CellState state): void               -- 배경 스프라이트를 상태에 맞게 교체
//     Clear(): void                                 -- 숫자를 지우고 Normal 상태로 복원
// @note    cellSprites 배열 인덱스: [0]=normal, [1]=given, [2]=selected, [3]=samegroup, [4]=error
//          SameNumber 상태는 SameGroup과 동일한 스프라이트([3])를 사용한다.
//          IsGiven이 true인 셀의 Normal 상태에서는 cell_given([1]) 스프라이트를 사용한다.
// ------------------------------------------------------------

using RoseEngine;

public class SudokuCell
{
    private static readonly Color GivenColor = new(0.1f, 0.1f, 0.3f, 1f);
    private static readonly Color UserInputColor = new(0.4f, 0.35f, 0.3f, 1f);

    private readonly Sprite[] _cellSprites;
    private readonly UIImage _image;
    private readonly UIText _numberText;

    public int Row { get; }
    public int Col { get; }
    public GameObject CellObject { get; }
    public CellState State { get; private set; }
    public bool IsGiven { get; private set; }

    public SudokuCell(int row, int col, GameObject cellGO, Sprite[] cellSprites)
    {
        Row = row;
        Col = col;
        CellObject = cellGO;
        _cellSprites = cellSprites;
        State = CellState.Normal;

        _image = cellGO.GetComponent<UIImage>()!;
        _numberText = cellGO.GetComponentInChildren<UIText>()!;
    }

    /// <summary>
    /// 셀에 표시할 숫자를 설정한다. 0이면 빈칸.
    /// isGiven이면 짙은 남색(펜), 아니면 연한 회색-갈색(연필).
    /// </summary>
    public void SetNumber(int number, bool isGiven)
    {
        IsGiven = isGiven;
        _numberText.text = number == 0 ? "" : number.ToString();
        _numberText.color = isGiven ? GivenColor : UserInputColor;
    }

    /// <summary>
    /// 셀의 시각적 상태(배경 스프라이트)를 변경한다.
    /// </summary>
    public void SetState(CellState state)
    {
        State = state;

        _image.sprite = state switch
        {
            CellState.Selected => _cellSprites[2],
            CellState.SameGroup => _cellSprites[3],
            CellState.SameNumber => _cellSprites[3],
            CellState.Error => _cellSprites[4],
            // Normal: isGiven이면 cell_given, 아니면 cell_normal
            _ => IsGiven ? _cellSprites[1] : _cellSprites[0]
        };
    }

    /// <summary>
    /// 숫자를 지우고 Normal 상태로 되돌린다.
    /// </summary>
    public void Clear()
    {
        _numberText.text = "";
        SetState(CellState.Normal);
    }
}
