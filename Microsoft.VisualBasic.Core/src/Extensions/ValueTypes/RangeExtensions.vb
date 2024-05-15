#Region "Microsoft.VisualBasic::b1d0cec8228de1019b9c7e5945b6c7ef, Microsoft.VisualBasic.Core\src\Extensions\ValueTypes\RangeExtensions.vb"

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
    '    Code Lines: 10
    ' Comment Lines: 9
    '   Blank Lines: 3
    '     File Size: 778 B


    '     Module RangeExtensions
    ' 
    '         Function: Percentage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace ValueTypes

    Public Module RangeExtensions

        ''' <summary>
        ''' ```
        ''' d = <paramref name="value"/> - <see cref="DoubleRange.Min"/>
        ''' p% = d / <see cref="DoubleRange.Length"/> * 100%
        ''' ```
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="value#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Percentage(range As DoubleRange, value#) As Double
            Return If(value = range.Min, 0, (value - range.Min) / range.Length)
        End Function
    End Module
End Namespace
