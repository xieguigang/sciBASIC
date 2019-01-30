Namespace Layouts.Cola


    Class LinkSets
        Private sets As any = New Object() {}
        Private n As Double = 0
        Public Function count() As Double
            Return Me.n
        End Function
        Private Function contains(id As Double) As Boolean
            Dim result = False
            Me.forAllModules(Function(m)
                                 If Not result AndAlso m.id = id Then
                                     result = True
                                 End If

                             End Function)
            Return result
        End Function
        Private Sub add(linktype As Double, m As [Module])
            Dim s As ModuleSet = If(Me.sets.Have(linktype), Me.sets(linktype), InlineAssignHelper(Me.sets(linktype), New ModuleSet()))
            s.add(m)
            Me.n += 1
        End Sub
        Private Sub remove(linktype As Double, m As [Module])
            Dim ms = DirectCast(Me.sets(linktype), ModuleSet)
            ms.remove(m)
            If ms.count() = 0 Then
                Delete(Me.sets, linktype)
            End If
            Me.n -= 1
        End Sub
        Private Sub forAll(f As Action(Of ModuleSet, Number))
            For Each linktype As var In Me.sets.keys
                f(DirectCast(Me.sets(linktype), ModuleSet), linktype)
            Next
        End Sub
        Private Sub forAllModules(f As Action(Of [Module]))
            Me.forAll(Function(ms, lt) ms.forAll(f))
        End Sub
        Private Function intersection(other As LinkSets) As LinkSets
            Dim result As New LinkSets()
            Me.forAll(Function(ms, lt)
                          If other.sets.Have(lt) Then
                              Dim i = ms.intersection(other.sets(lt))
                              Dim n = i.count()
                              If n > 0 Then
                                  result.sets(lt) = i
                                  result.n += n
                              End If
                          End If

                      End Function)
            Return result
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace