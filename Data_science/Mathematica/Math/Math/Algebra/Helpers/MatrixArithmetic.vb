#Region "Microsoft.VisualBasic::0574a76475a319e2f8d600b71aa25036, Data_science\Mathematica\Math\Math\Algebra\Helpers\MatrixArithmetic.vb"

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

    '   Total Lines: 50
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 44 (88.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (12.00%)
    '     File Size: 2.26 KB


    ' 
    ' /********************************************************************************/

#End Region

'Imports System.Text

'Namespace BasicR.Helpers

'    ''' <summary>
'    ''' Matrix calculation define in the programming meanning, not mathematics.
'    ''' (非数学意义上的矩阵运算)
'    ''' </summary>
'    ''' <remarks></remarks>
'    Public Class MatrixArithmetic

'        ''' <summary>
'        ''' 枚举矩阵对象之间的所有操作
'        ''' </summary>
'        ''' <remarks></remarks>
'        Public ReadOnly Arithmetics As Dictionary(Of String, System.Func(Of BasicR.MATRIX, BasicR.MATRIX, BasicR.MATRIX)) =
'            New Dictionary(Of String, Func(Of BasicR.MATRIX, BasicR.MATRIX, BasicR.MATRIX)) From {
'                {"+", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a + b},
'                {"-", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a - b},
'                {"*", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a * b},
'                {"/", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a / b},
'                {"\", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a \ b},
'                {"%", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a Mod b},
'                {"^", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a.Pow(b)},
'                {"!", AddressOf MatrixArithmetic.Factorial}}

'        Public Function Evaluate(a As BasicR.MATRIX, b As BasicR.MATRIX, op As String) As BasicR.MATRIX
'            Dim Method = Arithmetics(op)
'            Return Method(a, b)
'        End Function

'        ''' <summary>
'        ''' 
'        ''' </summary>
'        ''' <param name="a"></param>
'        ''' <param name="b">Useless parameter</param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        Public Shared Function Factorial(a As BasicR.MATRIX, b As BasicR.MATRIX) As BasicR.MATRIX
'            Dim result As BasicR.MATRIX = New BasicR.MATRIX(Width:=a.Width, Height:=a.Height)

'            For row As Integer = 0 To a.Height - 1
'                For col As Integer = 0 To a.Width - 1
'                    result(row, col) = Mathematical.Helpers.Arithmetic.Factorial(a(row, col), -1)
'                Next
'            Next
'            Return result
'        End Function
'    End Class
'End Namespace
