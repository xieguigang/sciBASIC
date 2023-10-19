''' <summary>
''' bind data type to read/write
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
Public Class BindAttribute : Inherits Attribute

    Public ReadOnly Property Type As TypeCode

    Sub New(type As TypeCode)
        Me.Type = type
    End Sub

    Public Overrides Function ToString() As String
        Return Type.Description
    End Function
End Class

