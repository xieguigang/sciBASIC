#Region "Microsoft.VisualBasic::48ef65efb5e19a41d480c0d074ca0823, Data_science\Mathematica\Math\Math\Algebra\Helpers\Constants.vb"

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

    '   Total Lines: 46
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 39 (84.78%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.67 KB


    ' 
    ' /********************************************************************************/

#End Region

'Imports System.Text.RegularExpressions
'Imports System.Text

'Namespace BasicR.Helpers

'    Public Class Constants

'        ''' <summary>
'        ''' The constant PI and E is the system reserved constant
'        ''' </summary>
'        ''' <remarks></remarks>
'        Friend Constants As Dictionary(Of String, BasicR.MATRIX) = New Dictionary(Of String, BasicR.MATRIX) From {
'            {"e", sys.E},
'            {"pi", sys.PI}}

'        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^(,"
'        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!,"

'        Friend ConstantList As List(Of String) = New List(Of String) From {"pi", "e"}

'        ''' <summary>
'        ''' Add a user constant to the dictionary.
'        ''' (向字典之中添加用户自定义常数)
'        ''' </summary>
'        ''' <param name="Name"></param>
'        ''' <param name="value"></param>
'        ''' <remarks>
'        ''' const [name] [value]
'        ''' </remarks>
'        Public Sub Add(Name As String, value As String)
'            Call Constants.Add(Name.ToLower, Expression.Evaluate(value))
'            Call ConstantList.Clear()
'            Call ConstantList.AddRange(Constants.Keys)
'        End Sub

'        ''' <summary>
'        ''' Add a user const from the input of user on the console.
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub Add(statement As String)
'            Dim Name As String = statement.Split.First
'            Call Add(Name, Mid(statement, Len(Name) + 2))
'        End Sub
'    End Class
'End Namespace
