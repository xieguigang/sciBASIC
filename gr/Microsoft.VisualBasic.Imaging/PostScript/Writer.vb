#Region "Microsoft.VisualBasic::a2035f927a0d4e45f123f7d67f8f5b65, gr\Microsoft.VisualBasic.Imaging\PostScript\Writer.vb"

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

'   Total Lines: 280
'    Code Lines: 145 (51.79%)
' Comment Lines: 92 (32.86%)
'    - Xml Docs: 79.35%
' 
'   Blank Lines: 43 (15.36%)
'     File Size: 9.61 KB


'     Class Writer
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: pen
' 
'         Sub: arct, beginTransparent, circle, closepath, (+2 Overloads) color
'              comment, dash, (+2 Overloads) Dispose, endTransparent, (+2 Overloads) font
'              grestore, gsave, image, line, lineto, linewidth, (+2 Overloads) moveto
'              note, rectangle, rotate, scale, setgray, showpage, stroke, text
'              translate, transparency
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace PostScript

    ''' <summary>
    ''' file writer for the postscript file
    ''' </summary>
    ''' <remarks>
    ''' The postscript coordinate system has its origin at the bottom-left corner
    ''' with the y axis pointing up, which is the opposite of the GDI+ top-left
    ''' origin. For that reason every geometry coordinate that is written through
    ''' this writer is flipped on the y axis by <see cref="Yf(Double)"/> so that
    ''' the generated postscript file looks identical to a GDI+ drawing. Text is
    ''' kept upright because it does not depend on a negative scale transform.
    ''' </remarks>
    Public Class Writer : Implements IDisposable

        ReadOnly fp As StreamWriter
        ReadOnly css As CSSEnvirnment

        Private disposedValue As Boolean

        ''' <summary>
        ''' the canvas height in the gdi+ coordinate space. used for flip the y axis.
        ''' </summary>
        ReadOnly canvasHeight As Single
        ReadOnly canvasWidth As Single

        Sub New(fp As StreamWriter, css As CSSEnvirnment)
            Me.fp = fp
            Me.css = css
            Me.canvasWidth = css.canvas.Width
            Me.canvasHeight = css.canvas.Height
        End Sub

        ''' <summary>
        ''' flip a gdi+ y coordinate (top-left origin) into a postscript y
        ''' coordinate (bottom-left origin).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Yf(y!) As Single
            Return canvasHeight - y
        End Function

        ''' <summary>
        ''' A helper function for make conversion from the css stroke object to the gdi+ pen object
        ''' </summary>
        ''' <param name="stroke"></param>
        ''' <returns></returns>
        Public Function pen(stroke As Stroke) As Pen
            Return css.GetPen(stroke)
        End Function

        ''' <summary>
        ''' moveto
        ''' </summary>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        Public Sub moveto(x!, y!)
            fprintf(fp, "%f %f moveto\n", x, Yf(y))
        End Sub

        ''' <summary>
        ''' moveto
        ''' </summary>
        Public Sub moveto(position As PointF)
            Call moveto(position.X, position.Y)
        End Sub

        ''' <summary>
        ''' show text at the given gdi+ location
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        Public Sub text(s As String, x!, y!)
            ' escape the special characters inside of the postscript literal string
            Dim escaped As String = s.Replace("\", "\\").Replace("(", "\(").Replace(")", "\)")
            fprintf(fp, "%f %f moveto (%s) show\n", x, Yf(y), escaped)
        End Sub

        ''' <summary>
        ''' show text at the given gdi+ location with an additional rotation.
        ''' the rotation follows the gdi+ convention (clockwise, positive angle),
        ''' which is mapped to a negative postscript rotation because of the y flip.
        ''' </summary>
        Public Sub text(s As String, x!, y!, rotation!)
            ' escape the special characters inside of the postscript literal string
            Dim escaped As String = s.Replace("\", "\\").Replace("(", "\(").Replace(")", "\)")

            If rotation = 0 Then
                fprintf(fp, "%f %f moveto (%s) show\n", x, Yf(y), escaped)
            Else
                fprintf(fp, "gsave %f %f translate %f rotate (%s) show grestore\n", x, Yf(y), -rotation, escaped)
            End If
        End Sub

        Public Sub lineto(x!, y!)
            fprintf(fp, "%f %f lineto\n", x, Yf(y))
        End Sub

        Public Sub line(x1!, y1!, x2!, y2!)
            fprintf(fp, "newpath\n %f %f moveto\n %f %f lineto\n stroke\n", x1, Yf(y1), x2, Yf(y2))
        End Sub

        ''' <summary>
        ''' stroke a circle outline at the given center with the given radius
        ''' </summary>
        Public Sub circle(center As PointF, radius As Single)
            fprintf(fp, "newpath %f %f %f 0 360 arc closepath stroke\n", center.X, Yf(center.Y), radius)
        End Sub

        ''' <summary>
        ''' fill a circle at the given center with the given radius
        ''' </summary>
        Public Sub fillCircle(center As PointF, radius As Single)
            fprintf(fp, "newpath %f %f %f 0 360 arc closepath fill\n", center.X, Yf(center.Y), radius)
        End Sub

        ''' <summary>
        ''' a correct postscript elliptical arc based on the gdi+ ellipse definition.
        ''' the arc is closed by <see cref="closepath"/> and should be followed by
        ''' <see cref="stroke"/> or <see cref="fill"/> by the caller.
        ''' </summary>
        ''' <param name="x!">left of the bounding rectangle (gdi+ space)</param>
        ''' <param name="y!">top of the bounding rectangle (gdi+ space)</param>
        ''' <param name="width!">width of the bounding rectangle</param>
        ''' <param name="height!">height of the bounding rectangle</param>
        ''' <param name="startAngle!">start angle, degree, clockwise in gdi+ convention</param>
        ''' <param name="sweepAngle!">sweep angle, degree, clockwise in gdi+ convention</param>
        Public Sub arct(x!, y!, width!, height!, startAngle!, sweepAngle!)
            Dim cx! = x + width / 2
            Dim cy! = y + height / 2
            Dim rx! = width / 2
            Dim ry! = height / 2

            ' translate to the ellipse center, scale the unit circle into the ellipse,
            ' the y axis is flipped through the writer so the gdi+ clockwise sweep is kept
            fprintf(fp, "gsave %f %f translate %f %f scale\n", cx, Yf(cy), rx, ry)
            fprintf(fp, "newpath 0 0 1 %f %f arc\n", startAngle, startAngle + sweepAngle)
            fprintf(fp, "grestore\n")
        End Sub

        ''' <summary>
        ''' stroke
        ''' </summary>
        Public Sub stroke()
            fprintf(fp, "stroke\n")
        End Sub

        ''' <summary>
        ''' fill the current path
        ''' </summary>
        Public Sub fill()
            fprintf(fp, "fill\n")
        End Sub

        Public Sub closepath()
            fprintf(fp, "closepath\n")
        End Sub

        ''' <summary>
        ''' draw a rectangle with optional fill color and optional stroke border.
        ''' </summary>
        ''' <param name="rect">the rectangle in gdi+ coordinate space</param>
        ''' <param name="fillColor">
        ''' the html color string of the fill, or nothing/empty to skip filling
        ''' </param>
        ''' <param name="border">
        ''' the css stroke of the border, or nothing to skip stroking
        ''' </param>
        Public Sub rectangle(rect As RectangleF, fillColor As String, border As Stroke)
            ' bottom-left / top-right in the postscript coordinate space
            Dim x1! = rect.X
            Dim y1! = Yf(rect.Y + rect.Height)
            Dim x2! = x1 + rect.Width
            Dim y2! = Yf(rect.Y)

            fprintf(fp, "newpath %f %f moveto %f %f lineto %f %f lineto %f %f lineto closepath\n", x1, y1, x2, y1, x2, y2, x1, y2)

            If Not fillColor.StringEmpty(, True) Then
                Call color(fillColor.TranslateColor)
                fprintf(fp, "gsave fill grestore\n")
            End If

            If border IsNot Nothing Then
                Dim pen As Pen = css.GetPen(border)

                Call linewidth(pen.Width)
                Call color(pen.Color)
                fprintf(fp, "stroke\n")
            End If
        End Sub

        ''' <summary>
        ''' setdash
        ''' </summary>
        ''' <param name="dash"></param>
        Public Sub dash(dash As Integer())
            If dash.IsNullOrEmpty Then
                fprintf(fp, "[] 0 setdash\n")
            Else
                fprintf(fp, "[%s %s] 0 setdash\n", dash(0), dash(1))
            End If
        End Sub

        ''' <summary>
        ''' setlinewidth
        ''' </summary>
        ''' <param name="width"></param>
        Public Sub linewidth(width As Single)
            fprintf(fp, "%f setlinewidth\n", width)
        End Sub

        ''' <summary>
        ''' setrgbcolor
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="g"></param>
        ''' <param name="b"></param>
        ''' <remarks>
        ''' the rgb color value parameter should be in value range of [0,1]
        ''' </remarks>
        Public Sub color(r!, g!, b!)
            fprintf(fp, "%3.2f %3.2f %3.2f setrgbcolor\n", r, g, b)
        End Sub

        ''' <summary>
        ''' setrgbcolor
        ''' </summary>
        ''' <param name="color"></param>
        Public Sub color(color As Color)
            fprintf(fp, "%3.2f %3.2f %3.2f setrgbcolor\n", color.R / 255, color.G / 255, color.B / 255)
        End Sub

        ''' <summary>
        ''' setgray
        ''' </summary>
        Public Sub setgray(g!)
            fprintf(fp, "%3.2f setgray\n", g)
        End Sub

        Public Sub transparency(ca As Single)
            fprintf(fp, "<< /CA %f /ca %f >> /TransparentState exch def\n", ca, ca)
        End Sub

        Public Sub beginTransparent()
            fprintf(fp, "gsave TransparentState setgstate\n")
        End Sub

        Public Sub endTransparent()
            fprintf(fp, "grestore\n")
        End Sub

        Public Sub gsave()
            fprintf(fp, "gsave\n")
        End Sub

        Public Sub grestore()
            fprintf(fp, "grestore\n")
        End Sub

        Public Sub translate(dx!, dy!)
            fprintf(fp, "%f %f translate\n", dx, Yf(dy))
        End Sub

        Public Sub rotate(angle!)
            fprintf(fp, "%f rotate\n", angle)
        End Sub

        Public Sub scale(sx!, sy!)
            fprintf(fp, "%f %f scale\n", sx, sy)
        End Sub

        Public Sub curveto(x1!, y1!, x2!, y2!, x3!, y3!)
            fprintf(fp, "%f %f %f %f %f %f curveto\n", x1, Yf(y1), x2, Yf(y2), x3, Yf(y3))
        End Sub

        Public Sub font(font As CSSFont)
            With css.GetFont(font)
                Call Me.font(.Name, .Size)
            End With
        End Sub

        ''' <summary>
        ''' setfont
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="fontsize!"></param>
        Public Sub font(name As String, fontsize!)
            fprintf(fp, "/%s findfont\n %f scalefont\n setfont\n", name, fontsize)
        End Sub

        ''' <summary>
        ''' write comment text
        ''' </summary>
        ''' <param name="noteText">
        ''' the comment text that may contains multiple data lines
        ''' </param>
        Public Sub note(noteText As String)
            For Each line As String In noteText.LineTokens
                Call fprintf(fp, "%% %s\n", line)
            Next
        End Sub

        ''' <summary>
        ''' write a line of comment text
        ''' </summary>
        ''' <param name="line"></param>
        Public Sub comment(line As String)
            Call fprintf(fp, "%% %s\n", line)
        End Sub

        Public Sub image(img As DataURI, x As Integer, y As Integer, width As Integer, height As Integer, scaleX As Double, scaleY As Double)
            ' Convert image data to Base64 string
            Dim base64Image As String = img.base64
            ' Calculate the new dimensions after scaling
            Dim newWidth As Integer = CInt(width * scaleX)
            Dim newHeight As Integer = CInt(height * scaleY)

            ' Start constructing the PostScript code
            fprintf(fp, "/width %s def\n", newWidth)
            fprintf(fp, "/height %s def\n", newHeight)
            fprintf(fp, "/xPos %s def\n", x)
            fprintf(fp, "/yPos %s def\n", Yf(y))

            ' Embed the Base64 encoded image data
            fprintf(fp, "/jpegData <~%s~> def\n", base64Image)

            ' Define the image source
            fprintf(fp, "/imageSource jpegData /Base64Decode filter /DCTDecode filter def\n")

            ' Translate the coordinate system to the desired position
            fprintf(fp, "xPos yPos translate\n")

            ' Scale the image
            fp.WriteLine(scaleX.ToString("0.##") & " " & scaleY.ToString("0.##") & " scale")

            ' Begin the image dictionary
            fprintf(fp, "<<\n")
            fprintf(fp, "  /ImageType 1\n")
            fprintf(fp, "  /Width %s\n", width)
            fprintf(fp, "  /Height %s\n", height)
            fprintf(fp, "  /BitsPerComponent 8\n")
            fprintf(fp, "  /ColorSpace /DeviceRGB\n")
            fprintf(fp, "  /DataSource imageSource\n")
            fprintf(fp, "  /Interpolate true\n")
            fprintf(fp, ">>\n")

            ' Draw the image on the page
            fprintf(fp, "image\n")
        End Sub

        ''' <summary>
        ''' [showpage] mark the end of current page
        ''' </summary>
        Public Sub showpage()
            Call fp.WriteLine("showpage")
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call fp.Flush()
                    Call fp.Close()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
