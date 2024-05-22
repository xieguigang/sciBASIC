#Region "Microsoft.VisualBasic::32e10c9bd83ae9824a0744ef74c27c2e, Data_science\Mathematica\Math\Randomizer\FastRandom\IProvideRandomValues.vb"

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

    '   Total Lines: 29
    '    Code Lines: 10 (34.48%)
    ' Comment Lines: 14 (48.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (17.24%)
    '     File Size: 1.40 KB


    ' Interface IProvideRandomValues
    ' 
    '     Properties: IsThreadSafe
    ' 
    '     Function: [Next], NextFloat
    ' 
    '     Sub: NextFloats
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices


Public Interface IProvideRandomValues
    ReadOnly Property IsThreadSafe As Boolean

    ''' <summary>
    ''' Generates a random float. Values returned are from 0.0 up to but not including 1.0.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Function NextFloat() As Double

    ''' <summary>
    ''' Fills the elements of a specified array of bytes with random numbers.
    ''' </summary>
    ''' <param name="buffer">An array of bytes to contain random numbers.</param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub NextFloats(buffer As Double())

    ''' <summary>
    ''' Returns a random integer that is within a specified range.
    ''' </summary>
    ''' <param name="minValue">The inclusive lower bound of the random number returned.</param>
    ''' <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    ''' <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue
    ''' equals maxValue, minValue is returned.</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Function [Next](minValue As Integer, maxValue As Integer) As Integer
End Interface
