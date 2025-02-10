#Region "Microsoft.VisualBasic::9603a168195d56fe688acfcc2caf0cce, gr\Microsoft.VisualBasic.Imaging\PostScript\PostScriptBuilder.vb"

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

    '   Total Lines: 102
    '    Code Lines: 68 (66.67%)
    ' Comment Lines: 20 (19.61%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (13.73%)
    '     File Size: 4.04 KB


    '     Class PostScriptBuilder
    ' 
    '         Function: MakePaint, ToString
    ' 
    '         Sub: Add, BuildString, Clear, MakePaint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace PostScript

    ''' <summary>
    ''' A helper module for convert the postscript object as ASCII script text
    ''' </summary>
    Public Class PostScriptBuilder

        Dim paints As New List(Of PSElement)

        Friend size As Size
        Friend originx, originy As Single

        ''' <summary>
        ''' Add a painting shape element into the canvas
        ''' </summary>
        ''' <param name="paint"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(paint As PSElement)
            Call Me.paints.Add(paint)
        End Sub

        ''' <summary>
        ''' Clear all painting elements from the internal buffer.
        ''' </summary>
        Public Sub Clear()
            Call paints.Clear()
        End Sub

        ''' <summary>
        ''' make painting
        ''' </summary>
        ''' <param name="g">
        ''' should be png/svg/pdf graphics
        ''' </param>
        Public Sub MakePaint(g As IGraphics)
            For Each paint As PSElement In paints
                Call paint.Paint(g)
            Next
        End Sub

        Public Function MakePaint(driver As Drivers) As GraphicsData
            Dim drv As DeviceInterop = DriverLoad.UseGraphicsDevice(driver)
            Dim g As IGraphics = drv.CreateGraphic(size, Color.Transparent, 100)
            Call MakePaint(g)
            Return drv.GetData(g, {0, 0, 0, 0})
        End Function

        ''' <summary>
        ''' Get ascii postscript text
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Using s As New MemoryStream
                Using fp As New StreamWriter(s)
                    Call BuildString(fp)
                    Call fp.Flush()
                End Using

                Return Encoding.UTF8.GetString(s.ToArray)
            End Using
        End Function

        Public Overloads Sub BuildString(fp As StreamWriter)
            Dim g As New Writer(fp, New CSSEnvirnment(size))

            fprintf(fp, "%%!PS-Adobe-3.0 EPSF-3.0\n")
            fprintf(fp, "%%Extensions: CMYK ProcessColorModel")
            fprintf(fp, "%%+ExtGState")
            fprintf(fp, "%%%%DocumentData: Clean7Bit\n")
            fprintf(fp, "%%%\%Origin: %10.2f %10.2f\n", originx, originy)
            fprintf(fp, "%%%%BoundingBox: %10.2f %10.2f %10.2f %10.2f\n", originx, originy, size.Width, size.Height)
            fprintf(fp, "%%%%LanguageLevel: 3\n")
            fprintf(fp, "%%%%Pages: 1\n")
            fprintf(fp, "%%%%Page: 1 1                           \n")
            fprintf(fp, "%% Convert to PDF with something like this:\n")
            fprintf(fp, "%% gs -o OutputFileName.pdf -sDEVICE=pdfwrite -dEPSCrop InputFileName.ps\n")
            fprintf(fp, "%% PostScript generated using the PStools library\n")
            fprintf(fp, "%% from the Binghamton Optimality Research Group\n")
            fprintf(fp, "%% Get the library at https://github.com/profmadden/pstools\n")
            fprintf(fp, "%% This library is free to use, however you see fit.  It would be\n")
            fprintf(fp, "%% nice if you let us know that you're using it, though!\n")
            fprintf(fp, "%% Drop us an email at pmadden@binghamton.edu, or pop by our\n")
            fprintf(fp, "%% web page, https://optimal.cs.binghamton.edu\n")
            fprintf(fp, "%% Standard use-at-your-own-risk stuff applies....\n")
            fprintf(fp, "/Courier findfont 15 scalefont setfont\n")

            For Each paint As PSElement In Me.paints
                Call paint.WriteAscii(g)
            Next

            fprintf(fp, "%\n%\n%\n%EOF\n")
        End Sub
    End Class
End Namespace
