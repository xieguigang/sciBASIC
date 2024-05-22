#Region "Microsoft.VisualBasic::41c56cf4373292a5487ac0ff2b8e0d18, Data_science\Mathematica\Math\GeneticProgramming\model\impl\Variable.vb"

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
    '     File Size: 812 B


    '     Class Variable
    ' 
    '         Properties: Depth, Terminal
    ' 
    '         Function: duplicate, eval, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model.impl

    Public Class Variable : Inherits AbstractExpression

        Public Shared ReadOnly X As Variable = New Variable()

        Public Overrides Function duplicate() As Expression
            Return X
        End Function

        Public Overrides Function eval(x As Double) As Double
            Return x
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overrides Function toStringExpression() As String
            Return "X"
        End Function

    End Class

End Namespace
