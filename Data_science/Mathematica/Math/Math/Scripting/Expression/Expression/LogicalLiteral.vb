#Region "Microsoft.VisualBasic::a7fdc71af5d640f44c51b2900fc5d01a, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\LogicalLiteral.vb"

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

    '   Total Lines: 23
    '    Code Lines: 16 (69.57%)
    ' Comment Lines: 1 (4.35%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (26.09%)
    '     File Size: 702 B


    '     Class LogicalLiteral
    ' 
    '         Properties: logical
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetVariableSymbols, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    Public Class LogicalLiteral : Inherits Expression

        Public Property logical As Boolean

        Sub New(text As String)
            logical = text.ParseBoolean
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return If(logical, 1, 0)
        End Function

        Public Overrides Function ToString() As String
            Return logical.ToString
        End Function

        Public Overrides Iterator Function GetVariableSymbols() As IEnumerable(Of String)
            ' no variable reference for literal constant value
        End Function
    End Class
End Namespace
