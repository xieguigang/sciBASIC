#Region "Microsoft.VisualBasic::659abe06b6875515fc14318084fb6079, ..\sciBASIC#\Data_science\Bootstrapping\Topology\Inference.vb"

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
