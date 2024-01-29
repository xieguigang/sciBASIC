''' <summary>
''' bind data type to read/write
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
Public Class BindAttribute : Inherits Attribute

    Public ReadOnly Property Type As TypeCode
    Public ReadOnly Property Par As Object

    Sub New(type As TypeCode, Optional par As Object = Nothing)
        Me.Par = par
        Me.Type = type
    End Sub

    Public Overrides Function ToString() As String
        Return Type.Description
    End Function
End Class

