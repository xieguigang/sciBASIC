#Region "Microsoft.VisualBasic::459dc16373ca2103b5fa0d331acef90b, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\LogicalLiteral.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (26.32%)
    '     File Size: 523 B


    '     Class LogicalLiteral
    ' 
    '         Properties: logical
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
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
    End Class
End Namespace
