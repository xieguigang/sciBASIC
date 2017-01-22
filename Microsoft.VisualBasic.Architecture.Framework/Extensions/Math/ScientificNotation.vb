Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Mathematical

    Public Module ScientificNotation

        ''' <summary>
        ''' 返回零表示比较小的常数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="INF"></param>
        ''' <returns></returns>
        Public Function PowerLog10(x#, Optional INF% = 5) As Single
            Dim pow# = Math.Log10(Math.Abs(x))

            If pow < -INF Then
                Return pow
            End If
            If pow > INF Then
                Return pow
            End If

            Return 0
        End Function

        Public Function UsingScientificNotation(x#, Optional INF% = 5) As Boolean
            Return ScientificNotation.PowerLog10(x, INF) <> 0!
        End Function

        ''' <summary>
        ''' 分别处理正常的小数或者科学记数法的小数
        ''' </summary>
        ''' <param name="n#"></param>
        ''' <param name="decimal%"></param>
        ''' <returns></returns>
        <Extension> Public Function FormatNumeric(n#, decimal%) As String
            Dim s$ = n.ToString

            If InStr(s, "E", CompareMethod.Text) > 0 Then
                ' 科学记数法
                Dim t$() = s.Split("e"c, "E"c)
                t(0) = Math.Round(Val(t(0)), [decimal])
                s = t(0) & "E" & t(1)

                Return s
            End If

            If UsingScientificNotation(n) Then
                s = FormatScientificNotation(n, [decimal])
            Else
                s = Math.Round(n, [decimal]).ToString
            End If

            Return s
        End Function

        ''' <summary>
        ''' 强制格式化为科学记数法
        ''' </summary>
        ''' <param name="decimal%"></param>
        ''' <returns></returns>
        Public Function FormatScientificNotation(n#, decimal%) As String
            Dim power = Fix(Math.Log10(n))
            Dim s = n.ToString.Split("E"c, "e"c).First
            Dim t$() = s.Split("."c)

            ' 处理整数部分
            Dim int As String = t(Scan0)
            Dim intpower = int.Length - 1

            If intpower > 0 Then  ' 整数部分不止一个字符长度，即数位大于等于2
                int = int.First & "." & Mid(int, 1) & t(1)
                s = Math.Round(Val(int), [decimal])
            Else
                s = int & "." & t(1)
                s = Math.Round(Val(s), [decimal])
            End If

            s = s & $"E{power + intpower}"

            Return s
        End Function
    End Module
End Namespace