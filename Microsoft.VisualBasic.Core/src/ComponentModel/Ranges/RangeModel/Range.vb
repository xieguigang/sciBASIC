#Region "Microsoft.VisualBasic::4c2421304db1ca234e1160091b9ed5b7, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\Range.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (27.27%)
    '     File Size: 1.22 KB


    '     Class Range
    ' 
    '         Properties: Max, Min
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) IsInside, IsOverlapping, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges.Model

    Public Class Range(Of T As IComparable)
        Implements IRangeModel(Of T)

        Public ReadOnly Property Min As T Implements IRangeModel(Of T).Min
        Public ReadOnly Property Max As T Implements IRangeModel(Of T).Max

        Sub New(min As T, max As T)
            Me.Min = min
            Me.Max = max
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function IsInside(x As T) As Boolean Implements IRangeModel(Of T).IsInside
            Return (Language.GreaterThanOrEquals(x, Min) AndAlso Language.LessThanOrEquals(x, Max))
        End Function

        Public Function IsInside(range As IRangeModel(Of T)) As Boolean Implements IRangeModel(Of T).IsInside
            Return ((IsInside(range.Min)) AndAlso (IsInside(range.Max)))
        End Function

        Public Function IsOverlapping(range As IRangeModel(Of T)) As Boolean Implements IRangeModel(Of T).IsOverlapping
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
        End Function
    End Class

End Namespace
