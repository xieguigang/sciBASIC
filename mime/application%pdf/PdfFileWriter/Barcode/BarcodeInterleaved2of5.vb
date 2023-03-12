Imports stdNum = System.Math

''' <summary>
''' Barcode interleaved 2 of 5 class
''' </summary>
Public Class BarcodeInterleaved2of5
    Inherits Barcode
    ''' <summary>
    ''' Code table for interleave 2 of 5 barcode
    ''' </summary>
    Public Shared ReadOnly CodeTable As Byte(,) = {
        {1, 1, 2, 2, 1},        ' 0
        {2, 1, 1, 1, 2},        ' 1
        {1, 2, 1, 1, 2},        ' 2
        {2, 2, 1, 1, 1},        ' 3
        {1, 1, 2, 1, 2},        ' 4
        {2, 1, 2, 1, 1},        ' 5
        {1, 2, 2, 1, 1},        ' 6
        {1, 1, 1, 2, 2},        ' 7
        {2, 1, 1, 2, 1},        ' 8
        {1, 2, 1, 2, 1}}        ' 9


    ''' <summary>
    ''' Barcode width
    ''' </summary>
    ''' <param name="BarIndex">Code array index</param>
    ''' <returns>float bar width</returns>

    Public Overrides Function BarWidth(BarIndex As Integer) As Integer
        ' leading bars
        If BarIndex < 4 Then Return 1

        ' ending bars
        If BarIndex >= BarCount - 3 Then Return If(BarIndex = BarCount - 3, 2, 1)

        ' code index
        BarIndex -= 4
        Dim CodeIndex As Integer = 2 * (BarIndex / 10)
        If (BarIndex And 1) <> 0 Then CodeIndex += 1

        ' code
        Dim Code = _CodeArray(CodeIndex)
        Return CodeTable(Code, (BarIndex Mod 10) / 2)
    End Function

    ''' <summary>
    ''' Barcode interleave 2 of 5 constructor
    ''' </summary>
    ''' <param name="Text">Text</param>
    ''' <param name="AddChecksum">Add checksum digit</param>
    Public Sub New(Text As String, Optional AddChecksum As Boolean = False)
        ' test argument
        If String.IsNullOrWhiteSpace(Text) Then Throw New ApplicationException("Barcode Interleave 2 of 5: Input text is null or empty")

        ' save text
        Me.Text = Text

        ' text length
        Dim Length = Text.Length
        If AddChecksum Then Length += 1
        If (Length And 1) <> 0 Then Throw New ApplicationException("Barcode Interleave 2 of 5: Text length must be even (including checksum)")

        ' barcode array
        _CodeArray = New Integer(Length - 1) {}

        ' make sure it is all digits
        Dim CodePtr = 0

        For Each Chr As Char In Text
            If Chr < "0"c OrElse Chr > "9"c Then Throw New ApplicationException("Barcode interleave 2 of 5: Invalid character (must be 0 to 9)")
            _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodePtr), CodePtr - 1)) = CInt(AscW(Chr) - Asc("0"c))
        Next

        ' calculate checksum
        If AddChecksum Then Checksum()

        ' set number of bars
        BarCount = 7 + 10 * (Length / 2)

        ' set total width
        TotalWidth = 8 + 14 * (Length / 2)

        ' exit
        Return
    End Sub


    ' Code EAN-13 checksum calculations


    Private Sub Checksum()
        ' calculate checksum
        Dim ChkSum = 3 * _CodeArray(0)
        Dim [End] = _CodeArray.Length - 1

        For Index = 1 To [End] - 1 Step 2
            ChkSum += _CodeArray(Index) + 3 * _CodeArray(Index + 1)
        Next

        ' final checksum
        ChkSum = ChkSum Mod 10
        If ChkSum <> 0 Then ChkSum = 10 - ChkSum
        _CodeArray([End]) = ChkSum

        ' add it to text
        Text = Text & Microsoft.VisualBasic.ChrW(ChkSum + Asc("0"c)).ToString()
        Return
    End Sub
End Class
