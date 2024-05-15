#Region "Microsoft.VisualBasic::1dc78bb37bdd82f48f6b85cbc48a3483, Data_science\DataMining\BinaryTree\test\AffinityPropagationDemo.vb"

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

    '   Total Lines: 26
    '    Code Lines: 19
    ' Comment Lines: 1
    '   Blank Lines: 6
    '     File Size: 670 B


    ' Module AffinityPropagationDemo
    ' 
    '     Function: demo
    ' 
    '     Sub: Main2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.BinaryTree.AffinityPropagation
Imports Microsoft.VisualBasic.Serialization.JSON

Module AffinityPropagationDemo

    Sub Main2()
        Dim matrix_raw As IEnumerable(Of Double()) = demo()
        Dim method As New AffinityPropagation(matrix_raw)
        Dim clusters = method.Fit

        Call Console.WriteLine(clusters.GetJson)

        Pause()
    End Sub

    Private Iterator Function demo() As IEnumerable(Of Double())
        ' [0,0,0,3,3,3]

        Yield {1, 2}
        Yield {1, 4}
        Yield {1, 0}
        Yield {4, 2}
        Yield {4, 4}
        Yield {4, 0}
    End Function
End Module
