#Region "Microsoft.VisualBasic::32c4aefba9f7a2d1b604d31e44c596a2, Data_science\Mathematica\Math\GeneticProgramming\model\impl\AbstractExpression.vb"

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
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 748 B


    '     Class AbstractExpression
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model.impl

    Public MustInherit Class AbstractExpression
        Implements Expression

        Public MustOverride ReadOnly Property Depth As Integer Implements Expression.Depth
        Public MustOverride ReadOnly Property Terminal As Boolean Implements Expression.Terminal

        Public MustOverride Function duplicate() As Expression Implements Expression.duplicate
        Public MustOverride Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Function eval(x As Double) As Double Implements Expression.eval

        Public Overrides Function ToString() As String
            Return toStringExpression()
        End Function

    End Class
End Namespace
