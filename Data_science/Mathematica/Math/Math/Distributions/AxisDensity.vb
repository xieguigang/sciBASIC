#Region "Microsoft.VisualBasic::d2f8ce9168c23a4276783331ccc8c47c, Data_science\Mathematica\Math\Math\Distributions\AxisDensity.vb"

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

    '   Total Lines: 22
    '    Code Lines: 17 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 844 B


    '     Module AxisDensity
    ' 
    '         Function: GetClusters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.Quantile

Namespace Distributions

    Public Module AxisDensity

        <Extension>
        Public Iterator Function GetClusters(axis As IEnumerable(Of Double)) As IEnumerable(Of DoubleTagged(Of Double()))
            Dim sortted = axis.OrderBy(Function(xi) xi).ToArray
            Dim diff As Double() = NumberGroups.diff(sortted)
            Dim threshold As Double = diff.Quartile.Q3

            For Each group As NamedCollection(Of Double) In sortted.GroupBy(offset:=threshold)
                Yield New DoubleTagged(Of Double())(Val(group.name), group.value)
            Next
        End Function

    End Module
End Namespace
