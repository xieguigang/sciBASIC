#Region "Microsoft.VisualBasic::272331e97bb5a9243714f4e883ce01cc, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\labeltest.vb"

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

    '   Total Lines: 35
    '    Code Lines: 29 (82.86%)
    ' Comment Lines: 1 (2.86%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (14.29%)
    '     File Size: 1.37 KB


    ' Module labeltest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.MachineVision.CCL

Public Module labeltest

    Sub Main()
        Dim img = "Z:\aaa.bmp".LoadImage
        Dim CELLS = CCLabeling.Process(BitmapBuffer.FromImage(img), background:=Color.White, 0).ToArray
        Dim pen As New Pen(Color.Red, 2)
        Dim pen2 As New Pen(Color.Blue, 2)
        Dim colors As LoopArray(Of Color) = Designer.GetColors("paper", 100)

        Using gfx As Graphics2D = Graphics2D.CreateDevice(img.Size)
            ' Call gfx.DrawImage(img, New Point)

            For Each item In CELLS
                If item.length > 3 AndAlso item.length < 1000 Then
                    Dim shape = item.AsEnumerable.ConcaveHull
                    Call gfx.DrawPolygon(New Pen(++colors, 2), shape)
                End If
            Next

            Call gfx.Flush()
            Call gfx.ImageResource.SaveAs("Z:/label6.png")
        End Using
    End Sub
End Module

