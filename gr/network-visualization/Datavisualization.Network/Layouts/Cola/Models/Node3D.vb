Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class Node3D

        ' if fixed, layout will not move the node from its specified starting position
        Public fixed As Boolean
        Public width As Double
        Public height As Double
        Public px As Double
        Public py As Double
        Public bounds As Rectangle2D
        Public variable As Variable

        Public x As Double, y As Double, z As Double

        Default Public Property Axis(dimName As String) As Double
            Get
                Select Case dimName
                    Case NameOf(x)
                        Return x
                    Case NameOf(y)
                        Return y
                    Case NameOf(z)
                        Return z
                    Case Else
                        Throw New NotImplementedException(dimName)
                End Select
            End Get
            Set(value As Double)
                Select Case dimName
                    Case NameOf(x)
                        x = value
                    Case NameOf(y)
                        y = value
                    Case NameOf(z)
                        z = value
                    Case Else
                        Throw New NotImplementedException(dimName)
                End Select
            End Set
        End Property

        Public Sub New(Optional x As Double = 0, Optional y As Double = 0, Optional z As Double = 0)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub
    End Class
End Namespace