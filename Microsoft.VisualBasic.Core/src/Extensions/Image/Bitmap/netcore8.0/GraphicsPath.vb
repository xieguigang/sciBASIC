Imports System.Drawing

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

        Public Class op_AddBezier : Inherits op

            Public Property pt1 As PointF
            Public Property pt2 As PointF
            Public Property pt3 As PointF
            Public Property pt4 As PointF

            Sub New(pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
                _pt1 = pt1
                _pt2 = pt2
                _pt3 = pt3
                _pt4 = pt4
            End Sub
        End Class

        Public Class op_AddCurve : Inherits op

            Public Property points As PointF()

        End Class

        Public Class op_AddLines : Inherits op

            Public Property points As PointF()

        End Class

        Public Class op_Reset : Inherits op
        End Class

        Public Class op_CloseAllFigures : Inherits op
        End Class

        Dim opSet As New List(Of op)

        Public Sub AddLine(a As PointF, b As PointF)
            opSet.Add(New Op_AddLine(a, b))
        End Sub

        Public Sub AddBezier(pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
            opSet.Add(New op_AddBezier(pt1, pt2, pt3, pt4))
        End Sub

        Public Sub AddCurve(ParamArray points As PointF())
            opSet.Add(New op_AddCurve With {.points = points})
        End Sub

        Public Sub AddLines(ParamArray points As PointF())
            opSet.Add(New op_AddLines With {.points = points})
        End Sub

        Public Sub Reset()
            opSet.Add(New op_Reset())
        End Sub

        Public Sub CloseAllFigures()
            opSet.Add(New op_CloseAllFigures())
        End Sub
    End Class
#End If
End Namespace