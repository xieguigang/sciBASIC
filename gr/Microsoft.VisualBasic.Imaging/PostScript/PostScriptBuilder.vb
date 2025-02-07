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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(paint As PSElement)
            Call Me.paints.Add(paint)
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

            fprintf(fp, "%%%%EOF\n")
        End Sub
    End Class
End Namespace