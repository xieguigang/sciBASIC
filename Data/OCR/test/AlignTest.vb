#Region "Microsoft.VisualBasic::0de791d2c279a2b59edf032ed8c262ab, sciBASIC#\Data\OCR\test\AlignTest.vb"

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

    '   Total Lines: 71
    '    Code Lines: 54
    ' Comment Lines: 1
    '   Blank Lines: 16
    '     File Size: 2.45 KB


    ' Module AlignTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports OCR

Public Module AlignTest

    Sub Main()
        Dim symbols As New List(Of Map(Of Vector, Char))
        Dim font As New Font(FontFace.Arial, 20, FontStyle.Bold)
        Dim images As New Dictionary(Of Char, Image)

        For Each c As Char In "0123456789."
            Using g = New Size(font.Height, font.Height * 2).CreateGDIDevice
                Call g.DrawString(c, font, Brushes.Black, New Point)
                Call images.Add(c, g.ImageResource.CorpBlank)
                symbols += New Map(Of Vector, Char) With {
                    .Key = images(c).SliceSegments(New Size(3, 3)).Project(6),
                    .Maps = c
                    }
            End Using
        Next

        ' 计算出相似度结果矩阵来评价结果
        Dim view As Image

        Using g = New Size(font.Height * 1.25 * 11, font.Height).CreateGDIDevice
            Call g.DrawString("0123456789.", font, Brushes.Black, New Point)
            view = g.ImageResource
        End Using

        Dim s As New StringBuilder

        s.AppendLine(vbTab & symbols.Select(Function(sss) sss.Maps).JoinBy(vbTab))

        Dim d = view.Slicing.Select(Function(m) m.Maps).ToArray

        For Each sss In view.Slicing()
            Call s.Append(sss.Key.ToString)
            Call s.Append(vbTab)

            Call sss.Maps.SaveAs($"./align/{sss.Key}.png")

            Dim v = sss.Maps.SliceSegments(New Size(3, 3)).Project(6)
            Dim max As (score As Double, C As Char)

            For Each symbol In symbols
                Dim similarity = SSM(v, symbol.Key)  ' GlobalMatch.Similarity(symbol.Key, v, 3 * 3 / 2)
                Call s.Append(similarity)
                Call s.Append(vbTab)

                If similarity > max.score Then
                    max = (similarity, symbol.Maps)
                End If
            Next

            Call s.AppendLine(max.C)
        Next

        Call s.SaveTo("./align/matrix.xls")
        Call view.SaveAs("./align/view.png")

        Pause()
    End Sub
End Module
