#Region "Microsoft.VisualBasic::19345faa0f3ef9b15cb9998e47b73a64, gr\Microsoft.VisualBasic.Imaging\test\LineTest.vb"

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

    '   Total Lines: 44
    '    Code Lines: 30 (68.18%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (31.82%)
    '     File Size: 1.07 KB


    ' Module LineTest
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging

Module LineTest

    Sub Main1()
        Using g = New Size(1600, 900).CreateGDIDevice
            Dim line As New Line(0, 0, 100, 0)
            Dim down = line.ParallelShift(20)


            Call line.Draw(g)
            Call down.Draw(g)

            line = New Line(0, 0, 0, 100)
            Dim left = line.ParallelShift(20)

            Call line.Draw(g)
            Call left.Draw(g)


            For Each line In {
                New Line(200, 200, 450, 450),
                New Line(300, 532, 1026, 663),
                New Line(1200, 635, 1999, 99)
            }

                Dim cor = line.ParallelShift(-150)
                cor.Stroke.Color = Color.Red
                cor.Stroke.Width = 5

                Call line.Draw(g)
                Call cor.Draw(g)
            Next

            Call g.Save("./test.png", ImageFormats.Png)
        End Using



        Pause()
    End Sub
End Module
