#Region "Microsoft.VisualBasic::56eb4c6f98e1ce4a4d37922d2fc455fc, Data_science\Darwinism\NonlinearGrid\NonlinearGrid\DEMO.vb"

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

    ' Module DEMO
    ' 
    '     Function: populate
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure

Module DEMO

    Sub Main()
        Dim network As New List(Of Sample)

        network += populate({1, 1, 1, 1, 1, -10, 99, 1}, 0, 100)
        network += populate({1, 1, 1, 1, 0, -10, 99, 1}, 100, 100)
        network += populate({0, 0, 0, 0, 0, 50, 1, 0}, -50, 300)
        network += populate({0, 1, 1, 1, 1, 50, -100, 1}, 50, 100)
        network += populate({0, 0, 0, 0, 1, 100, 10, 1}, 500, 100)
        network += populate({0, 0, 0, 0, 1, 50, 10, 0}, -500, 300)

        ' Call Program.RunFitProcess(network, network.First.status.Length, "./test_demo.Xml", Nothing, 5000)
        Call network.SampleSetCreator().GetXml.SaveTo("./DEMO_dataset.Xml")
    End Sub

    Private Iterator Function populate([in] As Double(), out As Double, n As Integer) As IEnumerable(Of Sample)
        For i As Integer = 0 To n
            Yield New Sample With {.status = [in], .target = {out}}
        Next
    End Function
End Module
