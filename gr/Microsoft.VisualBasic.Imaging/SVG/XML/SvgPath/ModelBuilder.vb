#Region "Microsoft.VisualBasic::8256dabf4d9efa12f0f6b340d0a561d0, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\ModelBuilder.vb"

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

    '   Total Lines: 238
    '    Code Lines: 153 (64.29%)
    ' Comment Lines: 55 (23.11%)
    '    - Xml Docs: 78.18%
    ' 
    '   Blank Lines: 30 (12.61%)
    '     File Size: 8.72 KB


    '     Module ModelBuilder
    ' 
    '         Function: GlyphPath, ParseSVGPathData, PiePath, SVGPathData
    ' 
    '         Sub: [Call]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Namespace SVG.PathHelper

    ''' <summary>
    ''' gdi+ object to svg model convertor
    ''' </summary>
    Public Module ModelBuilder

        Public Function PiePath(x!, y!, width!, height!, startAngle!, sweepAngle!) As String
            Dim endAngle! = startAngle + sweepAngle
            Dim rX = width / 2
            Dim rY = height / 2
            Dim x1 = x + rX * std.Cos(std.PI * startAngle / 180)
            Dim y1 = y + rY * std.Sin(std.PI * startAngle / 180)
            Dim x2 = x + rX * std.Cos(std.PI * endAngle / 180)
            Dim y2 = y + rY * std.Sin(std.PI * endAngle / 180)
            ' Dim d = $"M{x},{y}  L{x1},{y1}  A{rX},{rY} 0 0,1 {x2},{y2} z" ' 1 means clockwise
            Dim d As String = $"M {x},{y} a {rX} {rY} 0 1 1 0 1 z"

            Return d
        End Function

        ''' <summary>
        ''' Translate the gdi+ graphic path object to the svg path data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 下面的命令可用于路径数据：
        ''' 
        ''' + ``M`` = moveto(``M X,Y``)
        ''' + ``L`` = lineto(``L X,Y``)
        ''' + ``H`` = horizontal lineto(``H X``)
        ''' + ``V`` = vertical lineto(``V Y``)
        ''' + ``C`` = curveto(``C X1,Y1,X2,Y2,ENDX,ENDY``)
        ''' + ``S`` = smooth curveto(``S X2,Y2,ENDX,ENDY``)
        ''' + ``Q`` = quadratic Belzier curve(``Q X,Y,ENDX,ENDY``)
        ''' + ``T`` = smooth quadratic Belzier curveto(``T ENDX,ENDY``)
        ''' + ``A`` = elliptical Arc(``A RX,RY,XROTATION,FLAG1,FLAG2,X,Y``)
        ''' + ``Z`` = closepath()
        ''' 
        ''' 注释：以上所有命令均允许小写字母。大写表示绝对定位，小写表示相对定位。
        ''' </remarks>
        <Extension>
        Public Function SVGPathData(path As GraphicsPath) As String
            Dim points = path.PathData.Points _
               .Select(Function(pt) $"{pt.X} {pt.Y}") _
               .ToArray
            Dim sb As New StringBuilder("M" & points.First)

            For Each pt In points.Skip(1)
                Call sb.Append(" ")
                Call sb.Append("L" & pt)
            Next

            Call sb.Append(" ")
            Call sb.Append("Z")

            Return sb.ToString
        End Function

#If NET48 Then
        Public Function GlyphPath(c As Char, font As Font) As String
            Using g As New GraphicsPath
                g.AddString(c.ToString, font.FontFamily, font.Style, font.Size, New PointF, New StringFormat)
                Return g.SVGPathData
            End Using
        End Function
#End If
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
        Public Function ParseSVGPathData(path As SvgPath) As GraphicsPath
            Dim scanner As New Pointer(Of Char)(path.D)
            Dim c As Char
            Dim parameters As New List(Of Double)
            Dim buffer As New List(Of Char)
            Dim action As Char = Text.ASCII.NUL
            Dim g As New Path2D

            Do While Not scanner.EndRead
                ' Get current value and move forward the Pointer 
                ' by one step.
                c = ++scanner

                If Char.IsLetter(c) Then

                    If Not action = Text.ASCII.NUL Then
                        Call g.Call(action, parameters, path)
                    End If

                    ' clear buffer
                    action = c
                    parameters *= 0
                    buffer *= 0

                ElseIf c = " "c Then

                    ' 结束当前的token
                    parameters += Val(buffer.CharString)
                    buffer *= 0

                ElseIf Char.IsDigit(c) Then
                    buffer += c
                Else
                    Throw New NotImplementedException($"Unknown ""{c}""@{path.D}")
                End If
            Loop

            Return g.Path
        End Function

        ''' <summary>
        ''' Invoke gdi+ path build action
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="action"></param>
        ''' <param name="parameters"></param>
        ''' <param name="path"></param>
        <Extension>
        Private Sub [Call](g As Path2D, action As Char, parameters As List(Of Double), path As SvgPath)
            Select Case action
                Case "M"c
                    Call g.MoveTo(parameters(0), parameters(1))
                Case "m"c
                    Call g.MoveTo(parameters(0), parameters(1), relative:=True)

                Case "L"c
                    Call g.LineTo(parameters(0), parameters(1))
                Case "l"c
                    Call g.LineTo(parameters(0), parameters(1), relative:=True)

                Case "H"c
                    ' 水平平行线
                    ' 变X不变Y
                    Call g.HorizontalTo(parameters(0))
                Case "h"c
                    Call g.HorizontalTo(parameters(0), relative:=True)

                Case "V"c
                    ' 垂直平行线
                    ' 变Y不变X
                    Call g.VerticalTo(parameters(0))
                Case "v"c
                    Call g.VerticalTo(parameters(0), relative:=True)

                Case "C"c
                    Dim i As i32 = 0

                    Call g.CurveTo(
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i)
                    )
                Case "c"c
                    Dim i As i32 = 0

                    Call g.CurveTo(
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                                        _
                        relative:=True
                    )

                Case "S"c
                    Call g.SmoothCurveTo(parameters(0), parameters(1), parameters(2), parameters(4))
                Case "s"c
                    Call g.SmoothCurveTo(parameters(0), parameters(1), parameters(2), parameters(4), relative:=True)

                Case "Q"c
                    Call g.QuadraticBelzier(parameters(0), parameters(1), parameters(3), parameters(4))
                Case "q"c
                    Call g.QuadraticBelzier(parameters(0), parameters(1), parameters(3), parameters(4), relative:=True)

                Case "A"c
                    Dim i As i32 = 0

                    Call g.EllipticalArc(
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i)
                    )
                Case "a"c
                    Dim i As i32 = 0

                    Call g.EllipticalArc(
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                        parameters(++i),
                                        _
                        relative:=True
                    )

                Case "Z"c, "z"c
                    Call g.CloseAllFigures()
                Case Else
                    Throw New NotImplementedException($"Action ""{action}""@{path.D}")
            End Select
        End Sub
    End Module
End Namespace
