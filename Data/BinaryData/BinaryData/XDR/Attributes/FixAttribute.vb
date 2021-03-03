Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class FixAttribute
        Inherits Attribute

        Private _Length As UInteger

        Public Property Length As UInteger
            Get
                Return _Length
            End Get
            Private Set(value As UInteger)
                _Length = value
            End Set
        End Property

        Public Sub New(length As UInteger)
            If length = 0 Then Throw New ArgumentException("length must be greater than zero")
            Me.Length = length
        End Sub
    End Class
End Namespace
