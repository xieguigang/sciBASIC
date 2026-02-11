#Region "Microsoft.VisualBasic::3cad01111d931db59ddb926f0c39d2d9, Data_science\Mathematica\SignalProcessing\SignalProcessing\Cluster\NetworkSignalType.vb"

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

    '   Total Lines: 34
    '    Code Lines: 13 (38.24%)
    ' Comment Lines: 15 (44.12%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (17.65%)
    '     File Size: 863 B


    ' Enum NetworkSignalType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Public Enum NetworkSignalType

    ''' <summary>
    ''' 孤立节点信息（异常数据）
    ''' </summary>
    <Description("孤立节点信息（异常数据）")>
    Isolated = 1

    ''' <summary>
    ''' 递增信号（异常数据）
    ''' </summary>
    <Description("递增信号（异常数据）")>
    Increasing = 2

    ''' <summary>
    ''' 递减信号（异常数据）
    ''' </summary>
    <Description("递减信号（异常数据）")>
    Decreasing = 3

    ''' <summary>
    ''' 周期性震荡信号（好数据）
    ''' </summary>
    <Description("周期性震荡信号（好数据）")>
    Oscillating = 4

    ''' <summary>
    ''' 随机信号（好数据）
    ''' </summary>
    <Description("随机信号（好数据）")>
    RandomSignal = 5
End Enum

