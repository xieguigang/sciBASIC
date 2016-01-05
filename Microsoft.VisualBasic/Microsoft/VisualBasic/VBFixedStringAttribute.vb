Imports Microsoft.VisualBasic.CompilerServices
Imports System

Namespace Microsoft.VisualBasic
    <AttributeUsage(AttributeTargets.Field, Inherited:=False, AllowMultiple:=False)> _
    Public NotInheritable Class VBFixedStringAttribute
        Inherits Attribute
        ' Methods
        Public Sub New(Length As Integer)
            If ((Length < 1) OrElse (Length > &H7FFF)) Then
                Throw New ArgumentException(Utils.GetResourceString("Invalid_VBFixedString"))
            End If
            Me.m_Length = Length
        End Sub


        ' Properties
        Public ReadOnly Property Length As Integer
            Get
                Return Me.m_Length
            End Get
        End Property


        ' Fields
        Private m_Length As Integer
    End Class
End Namespace

