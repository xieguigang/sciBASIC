Imports System.Runtime.CompilerServices

''' <summary>
''' Simple number vector grouping
''' </summary>
Public Module NumberGroups

    <Extension>
    Public Function Groups(source As IEnumerable(Of Integer), offset As Integer) As List(Of Integer())
        Dim list As New List(Of Integer())
        Dim orders As Integer() = (From n As Integer
                                   In source
                                   Select n
                                   Order By n Ascending).ToArray
        Dim tag As Integer = orders(Scan0)
        Dim tmp As New List(Of Integer) From {tag}

        For Each x As Integer In orders.Skip(1)
            If x - tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                tmp += x
            Else
                tag = x
                list += tmp.ToArray
                tmp = New List(Of Integer) From {x}
            End If
        Next

        If tmp.Count > 0 Then
            list += tmp.ToArray
        End If

        Return list
    End Function
End Module
