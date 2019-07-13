#Region "Microsoft.VisualBasic::81dc3bbe4b63f750c4ac4b1effa1ee76, Microsoft.VisualBasic.Core\Extensions\Math\ScientificNotation.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module ScientificNotation
    ' 
    '         Function: FormatScientificNotation, PowerLog10, UsingScientificNotation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports sys = System.Math

Namespace Math

    ''' <summary>
    ''' 科学记数法
    ''' </summary>
    Public Module ScientificNotation

        ''' <summary>
        ''' 返回零表示比较小的常数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="INF">当位数超过这个值之后将会被判定为非常大或者非常小的一个数</param>
        ''' <returns></returns>
        Public Function PowerLog10(x#, Optional INF% = 5) As Single
            Dim pow# = sys.Log10(sys.Abs(x))

            If pow < -INF Then
                Return pow
            End If
            If pow > INF Then
                Return pow
            End If

            Return 0
        End Function

        ''' <summary>
        ''' 是否需要科学记数法来格式化？
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="INF%"></param>
        ''' <returns></returns>
        Public Function UsingScientificNotation(x#, Optional INF% = 5) As Boolean
            Return ScientificNotation.PowerLog10(x, INF) <> 0!
        End Function

        ''' <summary>
        ''' 强制格式化为科学记数法
        ''' </summary>
        ''' <param name="decimal%"></param>
        ''' <returns></returns>
        Public Function FormatScientificNotation(n#, decimal%) As String
            If n# = 0R Then
                Return "0"
            End If

            Dim power = Fix(sys.Log10(n))
            Dim s = n.ToString.Split("E"c, "e"c).First
            Dim t$() = s.Split("."c)

            ' 处理整数部分
            Dim int As String = t(Scan0)
            Dim intpower = int.Length - 1

            If intpower > 0 Then  ' 整数部分不止一个字符长度，即数位大于等于2
                int = int.First & "." & Mid(int, 1) & t(1)
                s = sys.Round(Val(int), [decimal])
            Else
                s = int & "." & t(1)
                s = sys.Round(Val(s), [decimal])
            End If

            s = s & $"E{power + intpower}"

            Return s
        End Function
    End Module
End Namespace
