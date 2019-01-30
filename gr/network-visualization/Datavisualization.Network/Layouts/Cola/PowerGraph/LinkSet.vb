Namespace Layouts.Cola

    Public Class LinkSets

        Dim sets As New Dictionary(Of String, ModuleSet)

        Public ReadOnly Property count() As Integer

        Public Function contains(id As Integer) As Boolean
            Dim result = False

            Me.forAllModules(Sub(m)
                                 If Not result AndAlso m.id = id Then
                                     result = True
                                 End If
                             End Sub)
            Return result
        End Function

        Public Sub add(linktype As Double, m As [Module])
            Dim s As ModuleSet = If(Me.sets.Have(linktype), Me.sets(linktype), InlineAssignHelper(Me.sets(linktype), New ModuleSet()))
            s.add(m)
            Me._count += 1
        End Sub

        Public Sub remove(linktype As Double, m As [Module])
            Dim ms = DirectCast(Me.sets(linktype), ModuleSet)
            ms.remove(m)
            If ms.count() = 0 Then
                Delete(Me.sets, linktype)
            End If
            Me._count -= 1
        End Sub

        Public Sub forAll(f As Action(Of ModuleSet, Integer))
            For Each linktype As String In Me.sets.Keys
                f(Me.sets(linktype), linktype)
            Next
        End Sub

        Private Sub forAllModules(f As Action(Of [Module]))
            Me.forAll(Sub(ms, lt) Call ms.forAll(f))
        End Sub

        Public Function intersection(other As LinkSets) As LinkSets
            Dim result As New LinkSets()

            Me.forAll(Sub(ms, lt)
                          If other.sets.ContainsKey(lt.ToString) Then
                              Dim i = ms.intersection(other.sets(lt))
                              Dim n = i.count()

                              If n > 0 Then
                                  result.sets(lt) = i
                                  result._count += n
                              End If
                          End If
                      End Sub)

            Return result
        End Function

        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace