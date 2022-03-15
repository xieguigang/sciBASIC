#Region "Microsoft.VisualBasic::d8eab898b3c0f07ecdf4ff5b3cc3993a, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Topology\Inference.vb"

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

    '   Total Lines: 23
    '    Code Lines: 11
    ' Comment Lines: 9
    '   Blank Lines: 3
    '     File Size: 749.00 B


    '     Module Inference
    ' 
    '         Function: GAFInference
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Topology

    ''' <summary>
    ''' dy = A - B
    ''' </summary>
    Public Module Inference

        ''' <summary>
        ''' 使用遗传算法来进行网络拓扑结构的估算
        ''' </summary>
        ''' <param name="obs"></param>
        ''' <param name="popSize%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GAFInference(obs As ODEsOut, Optional popSize% = 500) As NamedValue(Of (alpha As Double(), beta As Double()))
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
