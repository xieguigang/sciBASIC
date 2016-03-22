Imports System.Text.RegularExpressions

Namespace BasicR

    Public Module Parser

        Const MATRIX_WIDTH As String = "width:=\d+"
        Const MATRIX_HEIGHT As String = "height:=\d+"
        Const MATRIX_SIZE As String = "\(\d+,\d+\)"
        Const MATRIX_DATA As String = "{.+}"

        ''' <summary>
        ''' Parsing a matrix object from a expression string.(从一个表达式之中解析出一个矩阵)
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Matrix(expression As String) As MATRIX
            Dim Data As String = Regex.Match(expression, MATRIX_DATA).Value
            Dim match As Match = Regex.Match(expression, MATRIX_SIZE)
            Dim Width, Height As Integer
            If match.Success Then
                Dim Size As String = match.Value.Replace("(", "").Replace(")", "")
                Dim Tokens As String() = Size.Split(","c)
                Width = Val(Tokens(Scan0))
                Height = Val(Tokens(1))
            Else
                Width = Val(Mid(Regex.Match(expression, MATRIX_WIDTH).Value, 8))
                Height = Val(Mid(Regex.Match(expression, MATRIX_HEIGHT).Value, 9))
            End If

            Dim MAT(Height - 1, Width - 1) As Double
            Dim idx As Integer
            Dim Data2 As Double() = (From e As String In Data.Replace("{", "").Replace("}", "").Split(","c) Select Val(e)).ToArray
            For i As Integer = 0 To Height - 1
                For j As Integer = 0 To Width - 1
                    MAT(i, j) = Data2(idx)
                    idx += 1
                Next
            Next

            Return New MATRIX With {
                .Ele = MAT,
                .Dim1 = Width,
                .Dim2 = Height
            }
        End Function
    End Module
End Namespace