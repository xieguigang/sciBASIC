''' <summary>
''' Word 表格样式。
''' </summary>
Public Class TableStyle

    ''' <summary>表头背景色。</summary>
    Public Property HeaderBackColor As String = WordColors.TableHeaderBg

    ''' <summary>表头文字颜色。</summary>
    Public Property HeaderForeColor As String = WordColors.TableHeaderFg

    ''' <summary>表头是否加粗。</summary>
    Public Property HeaderBold As Boolean = True

    ''' <summary>边框颜色。</summary>
    Public Property BorderColor As String = WordColors.Black

    ''' <summary>边框粗细（以 1/8 pt 为单位，4 = 0.5pt, 8 = 1pt）。</summary>
    Public Property BorderSize As Integer = 4

    ''' <summary>交替行背景色（空字符串表示不交替）。</summary>
    Public Property AltRowBackColor As String = WordColors.TableAltRowBg

    ''' <summary>单元格内边距（twips）。</summary>
    Public Property CellPadding As Integer = 120

End Class