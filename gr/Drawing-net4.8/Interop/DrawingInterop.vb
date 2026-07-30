#Region "Microsoft.VisualBasic::97c0e3465b7f116afff6500e598675aa, gr\Drawing-net4.8\Interop\DrawingInterop.vb"

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

    '   Total Lines: 261
    '    Code Lines: 159 (60.92%)
    ' Comment Lines: 56 (21.46%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 46 (17.62%)
    '     File Size: 11.29 KB


    ' Module DrawingInterop
    ' 
    '     Function: (+3 Overloads) CTypeBrushObject, CTypeFontFamilyObject, CTypeFontObject, CTypeGraphicsPath, CTypeMatrixObject
    '               CTypePenObject, CTypeStringFormatObject
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush

''' <summary>
''' helper for make gdi+ graphics component conversion
''' </summary>
Public Module DrawingInterop

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="font"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeFontObject(font As Font) As System.Drawing.Font
        Return New System.Drawing.Font(font.Name, font.Size, CType(font.Style, System.Drawing.FontStyle))
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="stroke"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypePenObject(stroke As Pen) As System.Drawing.Pen
        Return New System.Drawing.Pen(stroke.Color, stroke.Width)
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="paint"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeBrushObject(paint As SolidBrush) As System.Drawing.SolidBrush
        Return New System.Drawing.SolidBrush(paint.Color)
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="paint"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeBrushObject(paint As TextureBrush) As System.Drawing.TextureBrush
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="paint"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeBrushObject(paint As Brush) As System.Drawing.Brush
        If TypeOf paint Is SolidBrush Then
            Return DirectCast(paint, SolidBrush).CTypeBrushObject
        Else
            Return DirectCast(paint, TextureBrush).CTypeBrushObject
        End If
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="f"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeFontFamilyObject(f As Microsoft.VisualBasic.Imaging.FontFamily) As System.Drawing.FontFamily
        If f Is Nothing Then Return Nothing
        Return New System.Drawing.FontFamily(f.Name)
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="f"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeStringFormatObject(f As Microsoft.VisualBasic.Imaging.StringFormat) As System.Drawing.StringFormat
        If f Is Nothing Then Return Nothing
        Return New System.Drawing.StringFormat With {
            .Alignment = CType(f.Alignment, System.Drawing.StringAlignment),
            .LineAlignment = CType(f.LineAlignment, System.Drawing.StringAlignment)
        }
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object.
    ''' Replays the transform operation parameters stored on the imaging Matrix onto a GDI+ Matrix.
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeMatrixObject(matrix As Microsoft.VisualBasic.Imaging.Matrix) As System.Drawing.Drawing2D.Matrix
        If matrix Is Nothing Then Return Nothing

        Dim g As System.Drawing.Drawing2D.Matrix

        If matrix.HasCustomInit Then
            g = New System.Drawing.Drawing2D.Matrix(matrix.SrcRect, matrix.DstPoints)
        Else
            Dim e As Single() = matrix.Elements
            g = New System.Drawing.Drawing2D.Matrix(e(0), e(1), e(2), e(3), e(4), e(5))
        End If

        ' The imaging Matrix only keeps the most recent value of each transform kind,
        ' so the operations are replayed in a fixed order.
        If matrix.RotateAtPoint.HasValue Then
            g.RotateAt(matrix.RotateAngle, matrix.RotateAtPoint.Value)
        ElseIf matrix.RotateAngle <> 0.0F Then
            g.Rotate(matrix.RotateAngle)
        End If

        If matrix.ScaleX <> 1.0F OrElse matrix.ScaleY <> 1.0F Then
            g.Scale(matrix.ScaleX, matrix.ScaleY, CType(matrix.ScaleOrder, System.Drawing.Drawing2D.MatrixOrder))
        End If

        If matrix.ShearX <> 0.0F OrElse matrix.ShearY <> 0.0F Then
            g.Shear(matrix.ShearX, matrix.ShearY, CType(matrix.ShearOrder, System.Drawing.Drawing2D.MatrixOrder))
        End If

        If matrix.TranslateX <> 0.0F OrElse matrix.TranslateY <> 0.0F Then
            g.Translate(matrix.TranslateX, matrix.TranslateY, CType(matrix.TranslateOrder, System.Drawing.Drawing2D.MatrixOrder))
        End If

        If matrix.MultiplyMatrix IsNot Nothing Then
            g.Multiply(matrix.MultiplyMatrix.CTypeMatrixObject, CType(matrix.MultiplyOrder, System.Drawing.Drawing2D.MatrixOrder))
        End If

        If matrix.IsInverted Then
            g.Invert()
        End If

        Return g
    End Function

    ''' <summary>
    ''' Convert of the .NET 8.0 visualbasic graphics component as .NET clr windows gdi+ object
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeGraphicsPath(path As GraphicsPath) As System.Drawing.Drawing2D.GraphicsPath
        Dim g As New System.Drawing.Drawing2D.GraphicsPath

        ' Sync fill mode (imaging enum values match System.Drawing.Drawing2D.FillMode)
        g.FillMode = CType(path.FillMode, System.Drawing.Drawing2D.FillMode)

        For Each op As GraphicsPath.op In path.AsEnumerable
            Select Case op.GetType
                Case GetType(GraphicsPath.op_AddArc)
                    Dim arc As GraphicsPath.op_AddArc = op
                    g.AddArc(arc.rect, arc.startAngle, arc.sweepAngle)

                Case GetType(GraphicsPath.op_AddBezier)
                    Dim bez As GraphicsPath.op_AddBezier = op
                    g.AddBezier(bez.pt1, bez.pt2, bez.pt3, bez.pt4)

                Case GetType(GraphicsPath.op_AddLine)
                    Dim ln As GraphicsPath.op_AddLine = op
                    g.AddLine(ln.a, ln.b)

                Case GetType(GraphicsPath.op_AddCurve)
                    Dim cv As GraphicsPath.op_AddCurve = op
                    g.AddCurve(cv.points)

                Case GetType(GraphicsPath.op_AddLines)
                    Dim lns As GraphicsPath.op_AddLines = op
                    g.AddLines(lns.points)

                Case GetType(GraphicsPath.op_Reset)
                    g.Reset()

                Case GetType(GraphicsPath.op_CloseAllFigures)
                    g.CloseAllFigures()

                Case GetType(GraphicsPath.op_CloseFigure)
                    g.CloseFigure()

                Case GetType(GraphicsPath.op_AddRectangle)
                    Dim rect As GraphicsPath.op_AddRectangle = op
                    g.AddRectangle(rect.rect)

                Case GetType(GraphicsPath.op_AddPolygon)
                    Dim poly As GraphicsPath.op_AddPolygon = op
                    g.AddPolygon(poly.points)

                Case GetType(GraphicsPath.op_AddEllipse)
                    Dim el As GraphicsPath.op_AddEllipse = op
                    ' r1/r2 are radii (half extents); GDI+ AddEllipse takes width/height
                    g.AddEllipse(el.x, el.y, el.r1 * 2.0F, el.r2 * 2.0F)

                Case GetType(GraphicsPath.op_AddString)
                    Dim str As GraphicsPath.op_AddString = op
                    Dim family As System.Drawing.FontFamily = str.fontFamily.CTypeFontFamilyObject
                    Dim style As System.Drawing.FontStyle = CType(str.style, System.Drawing.FontStyle)
                    Dim format As System.Drawing.StringFormat = str.format.CTypeStringFormatObject
                    g.AddString(str.s, family, style, str.size, str.pos, format)

                Case GetType(GraphicsPath.op_AddPie)
                    Dim pie As GraphicsPath.op_AddPie = op
                    ' AddPie has no RectangleF overload, only Rectangle
                    g.AddPie(System.Drawing.Rectangle.Round(pie.rect), pie.startAngle, pie.sweepAngle)

                Case GetType(GraphicsPath.op_AddClosedCurve)
                    Dim cc As GraphicsPath.op_AddClosedCurve = op
                    g.AddClosedCurve(cc.points, cc.tension)

                Case GetType(GraphicsPath.op_AddPath)
                    Dim subpath As GraphicsPath.op_AddPath = op
                    g.AddPath(subpath.path.CTypeGraphicsPath, subpath.connect)

                Case GetType(GraphicsPath.op_AddBeziers)
                    Dim bz As GraphicsPath.op_AddBeziers = op
                    g.AddBeziers(bz.points)

                Case GetType(GraphicsPath.op_AddEllipseRect)
                    Dim elr As GraphicsPath.op_AddEllipseRect = op
                    g.AddEllipse(elr.rect)

                Case GetType(GraphicsPath.op_StartFigure)
                    g.StartFigure()

                Case GetType(GraphicsPath.op_Flatten)
                    Dim fl As GraphicsPath.op_Flatten = op
                    g.Flatten(fl.matrix.CTypeMatrixObject, fl.flatness)

                Case GetType(GraphicsPath.op_Widen)
                    Dim wd As GraphicsPath.op_Widen = op
                    g.Widen(wd.pen.CTypePenObject, wd.matrix.CTypeMatrixObject, wd.flatness)

                Case GetType(GraphicsPath.op_Warp)
                    Dim wp As GraphicsPath.op_Warp = op
                    g.Warp(wp.destPoints, wp.srcRect, wp.matrix.CTypeMatrixObject, CType(wp.warpMode, System.Drawing.Drawing2D.WarpMode), wp.flatness)

                Case GetType(GraphicsPath.op_Transform)
                    Dim tf As GraphicsPath.op_Transform = op
                    g.Transform(tf.matrix.CTypeMatrixObject)

                Case GetType(GraphicsPath.op_Reverse)
                    g.Reverse()

                Case GetType(GraphicsPath.op_GetBounds)
                    ' op_GetBounds is a read-only query that computes the path bounds;
                    ' it does not mutate the path geometry, so there is nothing to apply.

                Case Else
                    Throw New NotImplementedException(op.GetType.FullName)
            End Select
        Next

        Return g
    End Function
End Module
