#Region "Microsoft.VisualBasic::65c984e26f3f9f6f6d05f41a4dd45cb3, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\SimulatorModel\Individual.vb"

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

    '   Total Lines: 7
    '    Code Lines: 4
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 178.00 B


    ' Interface Individual
    ' 
    '     Sub: Tick
    ' 
    ' Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Public Interface Individual

    Sub Tick(adjacents As IEnumerable(Of Individual))

End Interface

Public Delegate Function ToInteger(Of T As Individual)(a As T) As Integer
