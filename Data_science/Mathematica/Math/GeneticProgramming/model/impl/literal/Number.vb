#Region "Microsoft.VisualBasic::457d070c9cf1c06f25667f70e20154ab, Data_science\Mathematica\Math\GeneticProgramming\model\impl\literal\Number.vb"

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

    '   Total Lines: 45
    '    Code Lines: 33
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 1.14 KB


    '     Class Number
    ' 
    '         Properties: Depth, Terminal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: duplicate, eval, getNumber, toStringExpression
    ' 
    '         Sub: setNumber
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model.impl

    Public Class Number : Inherits AbstractExpression

        Private number As Double

        Public Sub New(number As Double)
            Me.number = number
        End Sub

        Public Overridable Function getNumber() As Double
            Return number
        End Function

        Public Overridable Sub setNumber(number As Double)
            Me.number = number
        End Sub

        Public Overrides Function duplicate() As Expression
            Return New Number(number)
        End Function

        Public Overrides Function eval(x As Double) As Double
            Return number
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
            Return String.Format("{0:g}", number)
        End Function

    End Class

End Namespace
