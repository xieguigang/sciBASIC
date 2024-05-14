#Region "Microsoft.VisualBasic::3c4f462dd3198974583933ba03d16a6c, Data_science\DataMining\DataMining\ValueMapping.vb"

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

    '   Total Lines: 76
    '    Code Lines: 47
    ' Comment Lines: 20
    '   Blank Lines: 9
    '     File Size: 2.74 KB


    ' Module ValueMapping
    ' 
    '     Function: Discretization, ModalNumber, Z
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module ValueMapping

    ''' <summary>
    ''' Gets the modal number of the ranking mapping data set.(求取众数)
    ''' </summary>
    ''' <param name="data">The ranked mapping encoding value.(经过Rank Mapping处理过后的编码值)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 当不存在相同的分组元素数目的时候，会直接取第一个元素的值作为众数
    ''' 当存在相同的分组元素数目的时候，会取最大的元素值作为众数
    ''' </remarks>
    Public Function ModalNumber(data As Integer()) As Integer
        Dim Avg As Double = data.Average
        Dim Min = (From n In data Where n < Avg Select n).ToArray
        Dim Max = (From n In data Where n >= Avg Select n).ToArray
        Dim Mdn As Integer

        If Min.Length > Max.Length Then
            Mdn = Min.Average
        Else
            Mdn = Max.Average
        End If

        Return Mdn
    End Function

    ''' <summary>
    ''' 执行连续数值类型的数据的离散化操作，这个操作常用于决策树的构建
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Discretization(data As IEnumerable(Of Double), levels As Integer) As Discretizer
        Return New Discretizer(data, levels)
    End Function

    ''' <summary>
    ''' z-score transform of the data vector
    ''' </summary>
    ''' <param name="a"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Z(a As IEnumerable(Of EntityClusterModel)) As IEnumerable(Of EntityClusterModel)
        Dim matrix = a.ToArray
        Dim names As String() = matrix _
            .Select(Function(v) v.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        For Each v As EntityClusterModel In matrix
            Dim xv As Vector = v(names).AsVector
            Dim zscore As Double() = xv.Z.ToArray
            Dim t As New Dictionary(Of String, Double)

            For i As Integer = 0 To names.Length - 1
                t(names(i)) = zscore(i)
            Next

            Yield New EntityClusterModel With {
                .ID = v.ID,
                .Properties = t
            }
        Next
    End Function
End Module
