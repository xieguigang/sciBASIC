#Region "Microsoft.VisualBasic::d11a3e9326f4af208403f18aa42f9af6, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\SymbolExpression.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 3 (11.54%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (23.08%)
    '     File Size: 750 B


    '     Class SymbolExpression
    ' 
    '         Properties: symbolName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetVariableSymbols, ToString
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

        Public Overrides Iterator Function GetVariableSymbols() As IEnumerable(Of String)
            Yield symbolName
        End Function
    End Class
End Namespace
