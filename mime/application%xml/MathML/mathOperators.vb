#Region "Microsoft.VisualBasic::95607dbbd4ed0173e6e281c63416082d, mime\application%xml\MathML\mathOperators.vb"

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

    '   Total Lines: 20
    '    Code Lines: 14 (70.00%)
    ' Comment Lines: 4 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (10.00%)
    '     File Size: 671 B


    '     Enum mathOperators
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace MathML

    Public Enum mathOperators
        <Description("+")> plus
        <Description("*")> times
        <Description("/")> divide
        <Description("^")> power
        <Description("-")> minus
        ''' <summary>关系运算符：等于 =</summary>
        <Description("=")> eq
        ''' <summary>关系运算符：小于等于 ≤</summary>
        <Description("≤")> leq
        ''' <summary>关系运算符：大于等于 ≥</summary>
        <Description("≥")> geq
        ''' <summary>关系运算符：不等于 ≠</summary>
        <Description("≠")> neq
    End Enum
End Namespace
