#Region "Microsoft.VisualBasic::3a5e57143b7d97ff5548587405fe1595, sciBASIC#\Data\OCR\test\Module1.vb"

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

    '   Total Lines: 216
    '    Code Lines: 120
    ' Comment Lines: 32
    '   Blank Lines: 64
    '     File Size: 6.81 KB


    ' Module Module1
    ' 
    '     Function: draw
    ' 
    '     Sub: Main, OCRtest2, projectionTest, sliceTest2, translateTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports OCR

Module Module1

    Sub translateTest()
        Dim viewSize As New Size(10, 12)
        Dim size As New Size(2, 3)

        Call 0.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 8.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 9.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 10.TranslateRegion(size, viewSize).__DEBUG_ECHO

        Pause()
    End Sub

    Public Function draw(s As String) As Image
        Dim font As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Bold)
        Dim obj As Image

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString(s, font, Brushes.Black, New Point)
            obj = g.ImageResource '.CorpBlank
        End Using

        Return obj
    End Function

    'Sub CompareTest2()
    '    Dim a = draw("8")
    '    Dim b = draw(1)
    '    Dim C = draw(8)
    '    Dim query As Vector = a.ToVector.First
    '    Dim subject As Vector = b.ToVector.First
    '    Dim subject2 As Vector = C.ToVector.First
    '    Dim local As New GSW(Of Double)(query, subject, AddressOf Equals, AddressOf AsChar)
    '    Dim cutoff = 0.9
    '    Dim match1 As Match = local.GetMatches(local.MaxScore * cutoff).FirstOrDefault

    '    local = New GSW(Of Double)(query, subject2, AddressOf Equals, AddressOf AsChar)
    '    Dim match2 As Match = local.GetMatches(local.MaxScore * cutoff).FirstOrDefault


    '    Dim s1 = SSM(query, subject)
    '    Dim s2 = SSM(query, subject2)

    '    Pause()
    'End Sub

    Sub OCRtest2()
        Dim font As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Bold)
        Dim view As Image

        Using g = New Size(font.Height * 30, font.Height * 5).CreateGDIDevice
            Call g.DrawString("3.14159265353", font, Brushes.Black, New Point)
            Call g.DrawString("1666669", font, Brushes.Black, New Point(30, font.Height + 10))
            view = g.ImageResource
        End Using

        ' OCR test 
        Dim library As New Library(font, Library.Numeric)

        For Each c In view.GetCharacters(library)
            ' Call $"[{c.position}] {c.obj} = {c.score}".__DEBUG_ECHO
            Call Console.Write(c.obj)
        Next

        Pause()
    End Sub

    Public Sub projectionTest()
        Dim font As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Bold)
        Dim view As Image

        Using g = New Size(font.Height * 30, font.Height * 5).CreateGDIDevice
            Call g.DrawString("3.14159265353", font, Brushes.Black, New Point)
            Call g.DrawString("1666669", font, Brushes.Black, New Point(30, font.Height + 10))
            view = g.ImageResource
        End Using

        Dim test As Vector
        Dim test2 As Vector

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("3", font, Brushes.Black, New Point)
            test = g.ImageResource.CorpBlank.SliceSegments(New Size(3, 3))
        End Using

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("7", font, Brushes.Black, New Point)
            test2 = g.ImageResource.CorpBlank.SliceSegments(New Size(3, 3))
        End Using

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("8", font, Brushes.Black, New Point)
            test2 = g.ImageResource.CorpBlank.SliceSegments(New Size(3, 3))
        End Using

        '  Dim hv = view.Projection(True).Split(Function(d) d = 0R).ToArray
        '  Dim vv = view.Projection(False)

        For Each s In view.Slicing()
            Call s.Maps.SaveAs($"./asfsdfsdfsd/{s.Key}.png")

            Dim sss = s.Maps.SliceSegments(New Size(3, 3))
            Dim similarity = GlobalMatch.Similarity(sss, test, 3 * 3 / 2)
            Dim similarity2 = GlobalMatch.Similarity(sss, test2, 3 * 3 / 2)

            Pause()
        Next

        Call view.SaveAs("./view.png")

        Pause()

    End Sub

    Sub sliceTest2()

        Dim view = "D:\smartnucl_integrative\biodeepDB\METLINApp\mzCloud\test.png".LoadImage


        For Each s In view.Slicing()
            Call s.Maps.SaveAs($"./kkkkk/{s.Key}.png")
        Next

        Call view.SaveAs("./view2222222.png")


        Pause()
    End Sub

    Sub Main()
        Call projectionTest()
        Call sliceTest2()


        Call OCRtest2()

        ' Call CompareTest2()


        ' Call translateTest()


        Dim font As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Bold)
        Dim obj As Image

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("7", font, Brushes.Black, New Point)
            obj = g.ImageResource.CorpBlank
        End Using

        Dim view As Image

        Using g = New Size(500, 500).CreateGDIDevice
            Call g.DrawString("858++++", font, Brushes.Black, New Point)
            Call g.DrawString("0708665", font, Brushes.Black, New Point(50, font.Height + 60))

            view = g.ImageResource
        End Using

        ' OCR test 
        Dim library As New Library(font)

        For Each c In view.GetCharacters(library)
            Call $"[{c.position}] {c.obj} = {c.score}".__DEBUG_ECHO
        Next

        Pause()

        'Using buffer = BitmapBuffer.FromImage(view)
        '    Dim objSize = obj.Size
        '    Dim i As int = 0

        '    For Each region In buffer.RegionScan(Color.White, objSize)
        '        Call region.DrawRegion(objSize).SaveAs($"./dddddd/{++i}.png")
        '    Next

        '    Pause()
        'End Using

        For Each region In view.FindObjects(obj)
            Using gr = view.CreateCanvas2D
                Call gr.DrawRectangle(Pens.Red, region)
                Call gr.ImageResource.SaveAs($"./cfdddd/{region.ToString}.png")
            End Using
        Next

        Pause()

        Dim locations = view.FindObjects(obj).ToArray



        Call obj.SaveAs("./obj.png")
        Call view.SaveAs("./view.png")

        Using g = view.CreateCanvas2D()
            For Each window In locations
                ' Call view.ImageCrop(window).SaveAs($"./sub/{window.ToString}.png")
                Call g.DrawRectangle(Pens.Red, window)
            Next

            Call g.ImageResource.SaveAs("./frame.png")
        End Using

        Pause()
    End Sub
End Module
