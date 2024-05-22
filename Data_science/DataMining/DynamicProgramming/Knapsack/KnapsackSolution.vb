#Region "Microsoft.VisualBasic::8120929c9416b024567c9f4579a33f16, Data_science\DataMining\DynamicProgramming\Knapsack\KnapsackSolution.vb"

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

    '   Total Lines: 24
    '    Code Lines: 19 (79.17%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 879 B


    '     Class KnapsackSolution
    ' 
    '         Properties: Items, TotalWeight, Value
    ' 
    '         Function: Solve, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Knapsack

    Public Class KnapsackSolution

        Public Property Items As IList(Of Item)
        Public Property TotalWeight As Double
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Dim output = New StringBuilder()
            output.AppendLine(String.Format("value: {0}, total weight: {1}", Value, TotalWeight))
            output.AppendLine(" Products:" & String.Join(", ", Items))
            Return output.ToString()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Solve(items As IEnumerable(Of Item), capacity As Integer) As KnapsackSolution
            Return New KnapsackSolver(items, capacity).Solve
        End Function
    End Class
End Namespace
