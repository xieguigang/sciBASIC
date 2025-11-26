#Region "Microsoft.VisualBasic::f7b101b261fe8dad0da78b849a84de9a, gr\Drawing-net4.8\Graphics\DrawingInterop.vb"

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

    '   Total Lines: 98
    '    Code Lines: 55 (56.12%)
    ' Comment Lines: 33 (33.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (10.20%)
    '     File Size: 3.70 KB


    ' Module DrawingInterop
    ' 
    '     Function: (+3 Overloads) CTypeBrushObject, CTypeFontObject, CTypeGraphicsPath, CTypePenObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET8_0_OR_GREATER Then

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Image = Microsoft.VisualBasic.Imaging.Image
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
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CTypeGraphicsPath(path As GraphicsPath) As System.Drawing.Drawing2D.GraphicsPath
        Dim g As New System.Drawing.Drawing2D.GraphicsPath

        For Each op As GraphicsPath.op In path.AsEnumerable
            Select Case op.GetType
                Case GetType(GraphicsPath.op_AddArc)
                    Dim arc As GraphicsPath.op_AddArc = op
                    g.AddArc(arc.rect, arc.startAngle, arc.sweepAngle)
                Case Else
                    Throw New NotImplementedException
            End Select
        Next

        Return g
    End Function
End Module
#End If
