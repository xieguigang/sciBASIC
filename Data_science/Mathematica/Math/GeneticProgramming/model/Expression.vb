#Region "Microsoft.VisualBasic::ae360ea92a752d17742410c857dbb2ff, Data_science\Mathematica\Math\GeneticProgramming\model\Expression.vb"

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

    '   Total Lines: 27
    '    Code Lines: 9 (33.33%)
    ' Comment Lines: 10 (37.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 885 B


    '     Interface Expression
    ' 
    '         Properties: Depth, Terminal
    ' 
    '         Function: duplicate, eval, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model

    ''' <summary>
    ''' Represents an expression that can be evaluated for
    ''' a variable <tt>x</tt>.
    ''' </summary>
    Public Interface Expression

        ''' <returns> <tt>true</tt> IFF the expression is a terminal </returns>
        ReadOnly Property Terminal As Boolean

        ''' <returns> depth of the expression tree </returns>
        ReadOnly Property Depth As Integer

        ''' <param name="x"> double variable <tt>x</tt> </param>
        ''' <returns> result of the expression for <tt>x</tt> </returns>
        Function eval(x As Double) As Double

        ''' <returns> string representation of the expression </returns>
        Function toStringExpression() As String

        ''' <returns> exact duplicate of the expression </returns>
        Function duplicate() As Expression

    End Interface

End Namespace
