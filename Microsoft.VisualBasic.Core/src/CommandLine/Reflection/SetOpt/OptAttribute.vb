Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Property)>
    Public Class OptAttribute : Inherits Attribute

        Public ReadOnly Property Names As String()

        Default Public ReadOnly Property Value(args As CommandLine) As String
            Get
                If Names.IsNullOrEmpty Then
                    Return Nothing
                End If

                For Each name As String In Names
                    If args.ContainsParameter(name) Then
                        Return args(name)
                    End If
                Next

                Return Nothing
            End Get
        End Property

        Sub New(ParamArray names As String())
            Me.Names = names
        End Sub

    End Class
End Namespace