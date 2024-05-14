#Region "Microsoft.VisualBasic::797bf5dae1fba00822715c3b2e9c4e31, gr\network-visualization\Visualizer\Styling\Expression\Numeric\ContinuousNumber.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.13 KB


    '     Class ContinuousNumber
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    Public Class ContinuousNumber : Implements IGetSize

        ReadOnly getValue As Func(Of Node, Double)
        ReadOnly range As DoubleRange

        Sub New(map As MapExpression)
            Dim selector = map.propertyName.SelectNodeValue
            Dim values As Double() = map.values _
                .Select(AddressOf Val) _
                .ToArray

            range = New DoubleRange(values(0), values(1))
            getValue = Function(node As Node) Val(selector(node))
        End Sub

        Public Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            Return nodes _
                .RangeTransform(getValue, range) _
                .Select(Function(map)
                            Return New Map(Of Node, Single)(map.Key, map.Maps)
                        End Function)
        End Function
    End Class
End Namespace
