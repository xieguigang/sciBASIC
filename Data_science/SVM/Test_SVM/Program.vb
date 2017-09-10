#Region "Microsoft.VisualBasic::9409aeb529bbc0f6c1253b55ea50c0c2, ..\sciBASIC#\Data_science\SVM\Test_SVM\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.SVM
Imports Microsoft.VisualBasic.DataMining.SVM.Model
Imports Microsoft.VisualBasic.DataMining.SVM.Model.ColorClass
Imports Microsoft.VisualBasic.Imaging
Imports [Class] = Microsoft.VisualBasic.DataMining.ComponentModel.ColorClass

Module Program

    Sub Main()
        Dim cs = InsertDefault()
        Call cs.Calculate(Optimizers.GradientDescent)

        With New Size(650, 500).CreateGDIDevice
            Call cs.Draw(.Graphics, .Width, .Height)
            Call .ImageResource _
                 .SaveAs("../../../SVM-plot.png")
        End With
    End Sub

    Function InsertDefault() As CartesianCoordinateSystem
        Dim toAdd As New List(Of LabeledPoint)
        Dim colors As Dictionary(Of Model.ColorClass, [Class]) = [Class] _
            .FromEnums(Of Model.ColorClass) _
            .ToEnumsTable(Of Model.ColorClass)

        toAdd.Add(New LabeledPoint(0.4, 0.4, colors(RED)))
        toAdd.Add(New LabeledPoint(0.7, 0.6, colors(RED)))
        toAdd.Add(New LabeledPoint(0.2, 0.6, colors(BLUE)))
        toAdd.Add(New LabeledPoint(0.4, 0.9, colors(BLUE)))
        toAdd.Add(New LabeledPoint(1, 1, colors(BLUE)))
        toAdd.Add(New LabeledPoint(1, 0.75, colors(BLUE)))
        toAdd.Add(New LabeledPoint(0.6, 0.59, colors(BLUE)))
        toAdd.Add(New LabeledPoint(0.14, 0.5, colors(RED)))

        Dim line As New Line(0, 0.4, 1, 0.9)
        Dim cartesianCoordinateSystem As New CartesianCoordinateSystem(points:=toAdd) With {
            .Line = line
        }

        Return cartesianCoordinateSystem
    End Function
End Module
