#Region "Microsoft.VisualBasic::2b5fa84921f0a3ffeb3e50e838427e54, Data_science\DataMining\network\NeuralNetwork\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: CastTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier.Neuron

Namespace NeuralNetwork.Models

    Public Module Extensions

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="row">第一个元素为分类，其余元素为属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CastTo(row As RowObject) As Entity
            Dim LQuery = From s As String In row.Skip(1) Select Val(s) '
            Return New Entity With {.Y = Val(row.First), .Properties = LQuery.ToArray}
        End Function
    End Module
End Namespace
