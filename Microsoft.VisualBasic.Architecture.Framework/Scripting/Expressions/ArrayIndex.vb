Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Scripting.Expressions

    Public Module ArrayIndex

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="exp$">
        ''' + ``1``, index=1
        ''' + ``1:8``, index=1, count=8
        ''' + ``1->8``, index from 1 to 8
        ''' + ``8->1``, index from 8 to 1
        ''' + ``1,2,3,4``, index=1 or  2 or 3 or 4
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function TranslateIndex(exp$) As Integer()
            If exp.IsPattern("-?\d+") Then
                Return {
                    CInt(exp)
                }
            ElseIf exp.IndexOf(":"c) > -1 Then
                Dim t = exp.Split(":"c)
                Dim from = CInt(t(Scan0))
                Dim count = CInt(t(1))
                Dim indcies%() = New Integer(count - 1) {}

                For i As Integer = 0 To count - 1
                    indcies(i) = i + from
                Next

                Return indcies
            ElseIf exp.IndexOf(","c) > -1 Then
                Dim t As Integer() = exp _
                    .Split(","c) _
                    .Select(Function(i) CInt(Val(i.Trim))) _
                    .ToArray
                Return t
            ElseIf InStr(exp, "->") > 0 Then
                Dim t As Integer() = Strings.Split(exp, "->") _
                    .Select(Function(s) CInt(s.Trim)) _
                    .ToArray
                Dim from = t(Scan0)
                Dim to% = t(1)
                Dim delta = If(from < [to], 1, -1)
                Dim out As New List(Of Integer)

                For i As Integer = from To [to] Step delta
                    out += i
                Next

                Return out
            Else
                Throw New SyntaxErrorException($"'{exp}' expression syntax error!")
            End If
        End Function
    End Module
End Namespace