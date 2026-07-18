#Region "Microsoft.VisualBasic::72ac7a678fa2f11e748c84e876151d9f, Data_science\MachineLearning\CellularAutomaton\SimulatorModel\Individual.vb"

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

    '   Total Lines: 18
    '    Code Lines: 5 (27.78%)
    ' Comment Lines: 9 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 722 B


    ' Interface Individual
    ' 
    '     Sub: Commit, Tick
    ' 
    ' Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Public Interface Individual

    ''' <summary>
    ''' 根据邻居的当前状态计算下一代状态（仅写入缓冲，不直接修改“当前状态”），
    ''' 以保证同一世代基于同一帧状态同步演化。
    ''' </summary>
    ''' <param name="adjacents">相邻细胞的当前状态序列。</param>
    Sub Tick(adjacents As IEnumerable(Of Individual))

    ''' <summary>
    ''' 将 <see cref="Tick"/> 计算出的下一代状态提交为“当前状态”（双缓冲提交）。
    ''' 在所有细胞的 <see cref="Tick"/> 完成后统一调用。
    ''' </summary>
    Sub Commit()

End Interface

Public Delegate Function ToInteger(Of T As Individual)(a As T) As Integer
