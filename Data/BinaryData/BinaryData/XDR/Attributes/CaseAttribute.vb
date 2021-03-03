Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=True)>
    Public Class CaseAttribute
        Inherits Attribute

        Public ReadOnly Value As Object

        Public Sub New(val As Object)
            Dim vT As Type = val.GetType()

            If vT Is GetType(Integer) OrElse vT.IsEnum Then
                Value = val
            Else
                Throw New InvalidOperationException("required enum type or int")
            End If
        End Sub
    End Class
End Namespace
