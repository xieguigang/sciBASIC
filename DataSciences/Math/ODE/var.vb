Imports Microsoft.VisualBasic.Language

Public Class var : Inherits float

    Public Property Index As Integer
    Public Property Name As String

    Public Shared ReadOnly type As Type = GetType(var)

    Sub New()
    End Sub

    Sub New(name As String)
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{Index}] {Name} As System.Double = {value}"
    End Function

    Public Overloads Shared Operator =(var As var, x As Double) As var
        var.value = x
        Return var
    End Operator

    Public Overloads Shared Narrowing Operator CType(x As var) As Integer
        Return x.Index
    End Operator

    Public Overloads Shared Operator <>(var As var, x As Double) As var
        Throw New NotSupportedException
    End Operator
End Class
