#Region "Microsoft.VisualBasic::87cb2cb076b299972d41edf9d0f22cac, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\ValueMapping.vb"

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
End Module
