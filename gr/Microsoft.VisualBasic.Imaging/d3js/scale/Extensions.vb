﻿#Region "Microsoft.VisualBasic::1084db3cca299772deacba15b5ca2b70, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\scale\Extensions.vb"

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

    '   Total Lines: 87
    '    Code Lines: 21
    ' Comment Lines: 61
    '   Blank Lines: 5
    '     File Size: 4.08 KB


    '     Module Extensions
    ' 
    '         Function: linear, LinearScale, ordinal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace d3js.scale

    ''' <summary>
    ''' + Continuous scales map a continuous, quantitative input domain to a continuous output range. 
    '''   If the range is also numeric, the mapping may be inverted. A continuous scale is not 
    '''   constructed directly; instead, try a linear, power, log, identity, time or sequential color 
    '''   scale.
    ''' + Sequential scales are similar to continuous scales in that they map a continuous, numeric 
    '''   input domain to a continuous output range. However, unlike continuous scales, the output range 
    '''   of a sequential scale is fixed by its interpolator and not configurable. These scales do not 
    '''   expose invert, range, rangeRound and interpolate methods.
    ''' + Unlike continuous scales, ordinal scales have a discrete domain and range. For example, an 
    '''   ordinal scale might map a set of named categories to a set of colors, or determine the 
    '''   horizontal positions of columns in a column chart.
    ''' </summary>
    ''' <remarks>
    ''' + https://stackoverflow.com/questions/29785238/d3-different-between-scale-in-ordinal-and-linear
    ''' </remarks>
    ''' 
    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' > Ordinal scales have a discrete domain, such as a set of names or categories.
        ''' > An ordinal scale's values must be coercible to a string, and the stringified 
        ''' > version of the domain value uniquely identifies the corresponding range value.
        ''' 
        ''' So, as an example, a domain of an ordinal scale may contain names, like so:
        '''
        ''' ```javascript
        ''' var ordinalScale = d3.scale.ordinal()
        '''     .domain(['Alice', 'Bob'])
        '''     .range([0, 100]);
        '''
        ''' ordinalScale('Alice'); // 0
        ''' ordinalScale('Bob');   // 100
        ''' ```
        ''' 
        ''' Notice how all values are strings. They cannot be interpolated. What Is between 
        ''' 'Alice' and 'Bob'? I don't know. Neither does D3.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function ordinal() As OrdinalScale
            Return New OrdinalScale
        End Function

        Public Function constant(val As Double) As ConstantScale
            Return New ConstantScale(val)
        End Function

        ''' <summary>
        ''' > Quantitative scales have a continuous domain, such as the set of real numbers, or dates.
        ''' 
        ''' As an example, you can construct the following scale:
        '''
        ''' ```javascript
        ''' var linearScale = d3.scale.linear()
        '''     .domain([0, 10])
        '''     .range([0, 100]);
        '''
        ''' linearScale(0);  // 0
        ''' linearScale(5);  // 50
        ''' linearScale(10); // 100
        ''' ```
        ''' 
        ''' Notice how D3 Is able To interpolate 5 even If we haven't specified it explicitly in the 
        ''' domain.
        ''' </summary>
        ''' <returns>
        ''' Constructs a new continuous scale with the unit domain [0, 1], the unit range [0, 1], 
        ''' the default interpolator and clamping disabled. Linear scales are a good default 
        ''' choice for continuous quantitative data because they preserve proportional differences. 
        ''' Each range value y can be expressed as a function of the domain value x: ``y = mx + b``.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function linear(Optional reverse As Boolean = False) As LinearScale
            Return New LinearScale(reverse)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LinearScale(domain As IEnumerable(Of Double), Optional reverse As Boolean = False) As LinearScale
            Return d3js.scale.linear(reverse:=reverse).domain(domain)
        End Function
    End Module
End Namespace
