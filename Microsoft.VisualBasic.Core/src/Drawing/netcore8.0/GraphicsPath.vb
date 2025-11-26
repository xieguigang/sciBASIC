#Region "Microsoft.VisualBasic::50bbc96e336f3b3716dbcdacf99cf754, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\GraphicsPath.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 162
    '    Code Lines: 117 (72.22%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 45 (27.78%)
    '     File Size: 4.62 KB


    '     Class PathData
    ' 
    '         Properties: Points
    ' 
    '     Class GraphicsPath
    ' 
    '         Properties: PathData, PathPoints
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GenericEnumerator
    ' 
    '         Sub: AddArc, AddBezier, AddCurve, AddEllipse, AddLine
    '              AddLines, AddPolygon, AddRectangle, CloseAllFigures, CloseFigure
    '              Reset
    '         Class op
    ' 
    ' 
    ' 
    '         Class op_AddLine
    ' 
    '             Properties: a, b
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class op_AddBezier
    ' 
    '             Properties: pt1, pt2, pt3, pt4
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class op_AddCurve
    ' 
    '             Properties: points
    ' 
    '         Class op_AddLines
    ' 
    '             Properties: points
    ' 
    '         Class op_Reset
    ' 
    ' 
    ' 
    '         Class op_CloseAllFigures
    ' 
    ' 
    ' 
    '         Class op_CloseFigure
    ' 
    ' 
    ' 
    '         Class op_AddArc
    ' 
    '             Properties: rect, startAngle, sweepAngle
    ' 
    '         Class op_AddRectangle
    ' 
    '             Properties: rect
    ' 
    '         Class op_AddPolygon
    ' 
    '             Properties: points
    ' 
    '         Class op_AddEllipse
    ' 
    '             Properties: r1, r2, x, y
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq

Namespace Imaging

#If NET8_0_OR_GREATER Then

    Public Class PathData

        Public Property Points As PointF()

    End Class

    Public Class GraphicsPath : Implements Enumeration(Of op)

        Public ReadOnly Property PathData As PathData
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public ReadOnly Property PathPoints As PointF()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(points As IEnumerable(Of PointF))
            Call AddPolygon(points.ToArray)
            Call CloseAllFigures()
        End Sub

        Public MustInherit Class op

        End Class

        Public Class op_AddLine : Inherits op

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

        Public Class op_CloseFigure : Inherits op
        End Class

        Public Class op_AddArc : Inherits op
            Public Property rect As RectangleF
            Public Property startAngle As Single
            Public Property sweepAngle As Single
        End Class

        Public Class op_AddRectangle : Inherits op
            Public Property rect As RectangleF
        End Class

        Public Class op_AddPolygon : Inherits op
            Public Property points As PointF()
        End Class

        Public Class op_AddEllipse : Inherits op
            Public Property x As Single
            Public Property y As Single
            Public Property r1 As Single
            Public Property r2 As Single
        End Class

        Dim opSet As New List(Of op)

        Public Sub AddEllipse(x As Single, y As Single, r1 As Single, r2 As Single)
            Call opSet.Add(New op_AddEllipse With {.x = x, .y = y, .r1 = r1, .r2 = r2})
        End Sub

        Public Sub AddPolygon(points As PointF())
            Call opSet.Add(New op_AddPolygon With {.points = points})
        End Sub

        Public Sub AddRectangle(rect As RectangleF)
            Call opSet.Add(New op_AddRectangle With {.rect = rect})
        End Sub

        Public Sub AddArc(rect As RectangleF, startAngle!, sweepAngle!)
            Call opSet.Add(New op_AddArc With {.rect = rect, .startAngle = startAngle, .sweepAngle = sweepAngle})
        End Sub

        Public Sub AddLine(a As PointF, b As PointF)
            Call opSet.Add(New op_AddLine(a, b))
        End Sub

        Public Sub AddBezier(pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
            Call opSet.Add(New op_AddBezier(pt1, pt2, pt3, pt4))
        End Sub

        Public Sub AddCurve(ParamArray points As PointF())
            Call opSet.Add(New op_AddCurve With {.points = points})
        End Sub

        Public Sub AddLines(ParamArray points As PointF())
            Call opSet.Add(New op_AddLines With {.points = points})
        End Sub

        Public Sub Reset()
            Call opSet.Add(New op_Reset())
        End Sub

        Public Sub CloseAllFigures()
            Call opSet.Add(New op_CloseAllFigures())
        End Sub

        Public Sub CloseFigure()
            Call opSet.Add(New op_CloseFigure)
        End Sub

        Public Iterator Function GenericEnumerator() As IEnumerator(Of op) Implements Enumeration(Of op).GenericEnumerator
            For Each op As op In opSet
                Yield op
            Next
        End Function
    End Class
#End If
End Namespace
