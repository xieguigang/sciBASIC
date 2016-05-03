Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel

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

    <Extension>
    Public Function Groups(Of TagObject As INumberTag)(source As IEnumerable(Of TagObject), offset As Integer) As GroupResult(Of TagObject, Integer)()
        Dim list As New List(Of GroupResult(Of TagObject, Integer))
        Dim orders As TagObject() = (From x As TagObject
                                     In source
                                     Select x
                                     Order By x.Tag Ascending).ToArray
        Dim tag As TagObject = orders(Scan0)
        Dim tmp As New List(Of TagObject) From {tag}

        For Each x As TagObject In orders.Skip(1)
            If x.Tag - tag.Tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                tmp += x
            Else
                tag = x
                list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
                tmp = New List(Of TagObject) From {x}
            End If
        Next

        If tmp.Count > 0 Then
            list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
        End If

        Return list
    End Function
End Module

Public Interface INumberTag

    ReadOnly Property Tag As Integer
End Interface