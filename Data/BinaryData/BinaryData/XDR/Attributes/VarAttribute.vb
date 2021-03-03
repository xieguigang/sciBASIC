Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=False)>
    Public Class VarAttribute
        Inherits Attribute

        Private _MaxLength As UInteger

        Public Property MaxLength As UInteger
            Get
                Return _MaxLength
            End Get
            Private Set(value As UInteger)
                _MaxLength = value
            End Set
        End Property

        Public Sub New()
            MaxLength = UInteger.MaxValue
        End Sub

        Public Sub New(maxLength As UInteger)
            If maxLength = 0 Then Throw New ArgumentException("length must be greater than zero")
            Me.MaxLength = maxLength
        End Sub
    End Class
End Namespace
