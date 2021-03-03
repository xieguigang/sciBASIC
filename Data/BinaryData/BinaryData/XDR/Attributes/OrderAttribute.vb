Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class OrderAttribute
        Inherits Attribute

        Private _Order As UInteger

        Public Property Order As UInteger
            Get
                Return _Order
            End Get
            Private Set(value As UInteger)
                _Order = value
            End Set
        End Property

        Public Sub New(order As UInteger)
            Me.Order = order
        End Sub
    End Class
End Namespace
