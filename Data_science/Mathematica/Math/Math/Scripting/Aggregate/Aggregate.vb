#Region "Microsoft.VisualBasic::98ae7eeaa7c39573d6d9503a971814fc, Data_science\Mathematica\Math\Math\Scripting\Aggregate\Aggregate.vb"

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
    '    Code Lines: 15 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (16.67%)
    '     File Size: 668 B


    '     Delegate Function
    ' 
    ' 
    '     Class Aggregate
    ' 
    '         Function: GetAggregater
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting

    Public Delegate Function IAggregate(data As IEnumerable(Of Double)) As Double

    Public Class Aggregate

        Public Shared Function GetAggregater(name As String) As IAggregate
            Select Case LCase(name).Trim
                Case "min" : Return AddressOf Enumerable.Min
                Case "max" : Return AddressOf Enumerable.Max
                Case "mean" : Return AddressOf Enumerable.Average
                Case "sum" : Return AddressOf Enumerable.Sum
                Case Else
                    Throw New InvalidCastException(name)
            End Select
        End Function
    End Class
End Namespace
