#Region "Microsoft.VisualBasic::5955d7b2da023f839fdeac0247017e8f, gr\Microsoft.VisualBasic.Imaging\SVG\ModelBuilder.vb"

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

'     Module ModelBuilder
' 
'         Function: ParseSVGPathData, PiePath, SVGPath, SVGPathData
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Language
Imports sys = System.Math

Namespace SVG

    Public Module ModelBuilder

        Public Function PiePath(x!, y!, width!, height!, startAngle!, sweepAngle!) As path
            Dim endAngle! = startAngle + sweepAngle
            Dim rX = width / 2
            Dim rY = height / 2
            Dim x1 = x + rX * sys.Cos(Math.PI * startAngle / 180)
            Dim y1 = y + rY * sys.Sin(Math.PI * startAngle / 180)
            Dim x2 = x + rX * sys.Cos(Math.PI * endAngle / 180)
            Dim y2 = y + rY * sys.Sin(Math.PI * endAngle / 180)
            Dim d = $"M{x},{y}  L{x1},{y1}  A{rX},{rY} 0 0,1 {x2},{y2} z" ' 1 means clockwise

            Return New path With {
                .d = d
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SVGPath(path As GraphicsPath) As path
            Return New path(path)
        End Function

        ''' <summary>
        ''' 下面的命令可用于路径数据：
        ''' 
        ''' M = moveto
        ''' L = lineto
        ''' H = horizontal lineto
        ''' V = vertical lineto
        ''' C = curveto
        ''' S = smooth curveto
        ''' Q = quadratic Belzier curve
        ''' T = smooth quadratic Belzier curveto
        ''' A = elliptical Arc
        ''' Z = closepath
        ''' 
        ''' 注释：以上所有命令均允许小写字母。大写表示绝对定位，小写表示相对定位。
        ''' </summary>
        ''' <returns></returns>
        <Extension>
        Public Function SVGPathData(path As GraphicsPath) As String
            Dim points = path.PathData _
               .Points _
               .Select(Function(pt) $"{pt.X} {pt.Y}")
            Dim sb As New StringBuilder("M" & points.First)

            For Each pt In points.Skip(1)
                Call sb.Append(" ")
                Call sb.Append("L" & pt)
            Next

            Call sb.Append(" ")
            Call sb.Append("Z")

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 解析SVG之中的path数据，并转换为等价的gdi+的path对象
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```vbnet
        ''' ' M0 0 L15 0 L8 15 L0 0 Z
        ''' 
        ''' Move(0, 0)
        ''' LineTo(15, 0)
        ''' LineTo(8, 15)
        ''' LineTo(0, 0)
        ''' Close()
        ''' ```
        ''' </remarks>
        <Extension>
        Public Function ParseSVGPathData(path As path) As GraphicsPath
            Dim scanner As New Pointer(Of Char)(path.d)
            Dim c As Char
            Dim a#
            Dim b#
            Dim buffer As New List(Of Char)
            Dim action As Char
            Dim gdiPath As New Path2D

            Do While Not scanner.EndRead
                ' Get current value and move forward 
                ' the Pointer by one step.
                c = ++scanner

                If Char.IsLetter(c) Then

                    If Not a.IsNaNImaginary OrElse Not b.IsNaNImaginary AndAlso action <> vbNullChar Then
                        Select Case action
                            Case "M"c
                                Call gdiPath.MoveTo(a, b)
                            Case "L"c
                                Call gdiPath.LineTo(a, b)
                            Case "Z"c
                                Call gdiPath.CloseAllFigures()
                            Case Else
                                Throw New NotImplementedException($"Action ""{action}""@{path.d}")
                        End Select
                    End If

                    ' clear buffer
                    action = c
                    a = Double.NaN
                    b = Double.NaN
                    buffer *= 0

                ElseIf c = " "c Then

                    ' 结束当前的token
                    If a.IsNaNImaginary Then
                        a = Val(buffer.CharString)
                    ElseIf b.IsNaNImaginary Then
                        b = Val(buffer.CharString)
                    Else
                        Throw New NotImplementedException(path.d)
                    End If

                    buffer *= 0

                End If
            Loop

            Return gdiPath.Path
        End Function
    End Module
End Namespace
