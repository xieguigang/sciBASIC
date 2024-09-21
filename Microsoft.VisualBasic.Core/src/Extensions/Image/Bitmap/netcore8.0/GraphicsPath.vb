Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility

Namespace Imaging

#If NET8_0_OR_GREATER Then

    Public Class GraphicsPath

        Public MustInherit Class op

        End Class

        Public Class Op_AddLine : Inherits op

            Public Property a As PointF
            Public Property b As PointF

            Sub New(a As PointF, b As PointF)
                _a = a
                _b = b
            End Sub

        End Class

        Dim opSet As New List(Of op)

        Public Sub AddLine(a As PointF, b As PointF)
            opSet.Add(New Op_AddLine(a, b))
        End Sub

    End Class
#End If
End Namespace