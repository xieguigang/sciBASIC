Imports Microsoft.VisualBasic.Serialization.JSON

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
                    If args.BoolFlags.IndexOf(name) > -1 Then
                        Return "True"
                    End If
                Next

                Return Nothing
            End Get
        End Property

        Sub New(ParamArray names As String())
            Me.Names = names
        End Sub

        Public Overrides Function ToString() As String
            Return Names.GetJson
        End Function

    End Class
End Namespace