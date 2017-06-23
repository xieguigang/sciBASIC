#Region "Microsoft.VisualBasic::2decd1a93acc1fa435c90b58259633af, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Ranges\IRanges.vb"

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

Namespace ComponentModel.Ranges

    Public Interface IRange(Of T As IComparable)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        ReadOnly Property Min As T

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        ReadOnly Property Max As T

    End Interface

    ''' <summary>
    ''' Represents a generic range with minimum and maximum values
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IRanges(Of T As IComparable)
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
        Function IsInside(range As IRanges(Of T)) As Boolean

        ''' <summary>
        ''' Check if the specified range overlaps with this range
        ''' </summary>
        ''' <param name="range">Range to check for overlapping</param>
        ''' <returns><b>True</b> if the specified range overlaps with this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsOverlapping(range As IRanges(Of T)) As Boolean
    End Interface
End Namespace
