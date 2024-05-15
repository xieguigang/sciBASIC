#Region "Microsoft.VisualBasic::41b61ec31306d7bec8bde887b326d10f, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\IRangeModel.vb"

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
    '    Code Lines: 8
    ' Comment Lines: 22
    '   Blank Lines: 4
    '     File Size: 1.40 KB


    '     Interface IRangeModel
    ' 
    '         Function: (+2 Overloads) IsInside, IsOverlapping
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Ranges.Model

    ''' <summary>
    ''' Represents a generic range with minimum and maximum values
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IRangeModel(Of T As IComparable)
        Inherits IRange(Of T)

        ''' <summary>
        ''' Check if the specified value is inside this range
        ''' </summary>
        ''' <param name="x">Value to check</param>
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsInside(x As T) As Boolean

        ''' <summary>
        ''' Check if the specified range is inside this range
        ''' </summary>
        ''' <param name="range">Range to check</param>
        ''' <returns><b>True</b> if the specified range is inside this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsInside(range As IRangeModel(Of T)) As Boolean

        ''' <summary>
        ''' Check if the specified range overlaps with this range
        ''' </summary>
        ''' <param name="range">Range to check for overlapping</param>
        ''' <returns><b>True</b> if the specified range overlaps with this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsOverlapping(range As IRangeModel(Of T)) As Boolean
    End Interface
End Namespace
