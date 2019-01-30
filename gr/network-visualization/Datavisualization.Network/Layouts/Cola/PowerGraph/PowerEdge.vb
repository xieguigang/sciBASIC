Namespace Layouts.Cola

    Public Class PowerEdge

        Public source As Integer
        Public target As Integer
        Public type As Integer

        Default Public Property Item(name As String) As Integer
            Get
                If name = NameOf(source) Then
                    Return source
                ElseIf name = NameOf(target) Then
                    Return target
                Else
                    Throw New NotImplementedException(name)
                End If
            End Get
            Set(value As Integer)
                If name = NameOf(source) Then
                    source = value
                ElseIf name = NameOf(target) Then
                    target = value
                Else
                    Throw New NotImplementedException(name)
                End If
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(source As Integer, target As Integer, type As Integer)
            Me.source = source
            Me.target = target
            Me.type = type
        End Sub
    End Class
End Namespace