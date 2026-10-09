' ---------------- environments ----------------

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>Lexical scope: name → (value, isConst); undeclared writes go to the global scope.</summary>
Public NotInheritable Class Environment

    Private ReadOnly _parent As Environment
    Private ReadOnly _slots As New Dictionary(Of String, Slot)

    Public Sub New(parent As Environment)
        _parent = parent
    End Sub

    Private Class Slot
        Public Value As Object
        Public ReadOnly IsConst As Boolean
        Public Sub New(value As Object, isConst As Boolean)
            Me.Value = value
            Me.IsConst = isConst
        End Sub
    End Class

    Public Function TryGet(name As String, ByRef value As Object) As Boolean
        Dim e = Me
        While e IsNot Nothing
            Dim s As Slot = Nothing
            If e._slots.TryGetValue(name, s) Then
                value = s.Value
                Return True
            End If
            e = e._parent
        End While
        Return False
    End Function

    Public Sub Define(name As String, value As Object, isConst As Boolean)
        ' re-declaration in the same block: overwrite (JS var semantics)
        _slots(name) = New Slot(value, isConst)
    End Sub

    Public Sub Assign(name As String, value As Object)
        Dim e = Me
        While e IsNot Nothing
            Dim s As Slot = Nothing
            If e._slots.TryGetValue(name, s) Then
                If s.IsConst Then
                    Throw JsRuntimeException.TypeError("Assignment to constant variable '" & name & "'")
                End If
                s.Value = value
                Return
            End If
            e = e._parent
        End While
        ' non-strict JS: implicit global
        Dim g = Me
        While g._parent IsNot Nothing
            g = g._parent
        End While
        g._slots(name) = New Slot(value, False)
    End Sub

    Public Function LookupOrThrow(name As String) As Object
        Dim v As Object = Nothing
        If TryGet(name, v) Then Return v
        Throw JsRuntimeException.ReferenceError(name)
    End Function
End Class
