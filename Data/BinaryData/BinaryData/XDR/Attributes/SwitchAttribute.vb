Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class SwitchAttribute
        Inherits Attribute

        Public Sub New()
        End Sub
    End Class
End Namespace
