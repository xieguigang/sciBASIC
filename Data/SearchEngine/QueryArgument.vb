Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class QueryArgument : Implements sIdEnumerable

    Public Property Name As String Implements sIdEnumerable.Identifier
    Public Property Expression As String

    Public Function Compile() As Expression
        Return Build(Expression)
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class