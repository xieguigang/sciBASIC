Imports Microsoft.VisualBasic.CompilerServices
Imports System

Namespace Microsoft.VisualBasic
    <AttributeUsage(AttributeTargets.Field, Inherited:=False, AllowMultiple:=False)> _
    Public NotInheritable Class VBFixedArrayAttribute
        Inherits Attribute
        ' Methods
        Public Sub New(UpperBound1 As Integer)
            If (UpperBound1 < 0) Then
                Throw New ArgumentException(Utils.GetResourceString("Invalid_VBFixedArray"))
            End If
            Me.FirstBound = UpperBound1
            Me.SecondBound = -1
        End Sub

        Public Sub New(UpperBound1 As Integer, UpperBound2 As Integer)
            If ((UpperBound1 < 0) OrElse (UpperBound2 < 0)) Then
                Throw New ArgumentException(Utils.GetResourceString("Invalid_VBFixedArray"))
            End If
            Me.FirstBound = UpperBound1
            Me.SecondBound = UpperBound2
        End Sub


        ' Properties
        Public ReadOnly Property Bounds As Integer()
            Get
                If (Me.SecondBound = -1) Then
                    Return New Integer() { Me.FirstBound }
                End If
                Return New Integer() { Me.FirstBound, Me.SecondBound }
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                If (Me.SecondBound = -1) Then
                    Return (Me.FirstBound + 1)
                End If
                Return ((Me.FirstBound + 1) * (Me.SecondBound + 1))
            End Get
        End Property


        ' Fields
        Friend FirstBound As Integer
        Friend SecondBound As Integer
    End Class
End Namespace

