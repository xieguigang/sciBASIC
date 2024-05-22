#Region "Microsoft.VisualBasic::bac0b2173c35d3b5e32a69163a0bc3b5, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\SymbolExpression.vb"

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
    '    Code Lines: 14 (63.64%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 605 B


    '     Class SymbolExpression
    ' 
    '         Properties: symbolName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' symbol x
    ''' </summary>
    Public Class SymbolExpression : Inherits Expression

        Public ReadOnly Property symbolName As String

        Sub New(symbolName As String)
            Me.symbolName = symbolName
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return env.GetSymbolValue(symbolName)
        End Function

        Public Overrides Function ToString() As String
            Return symbolName
        End Function
    End Class
End Namespace
