#Region "Microsoft.VisualBasic::c31c360f2bfd57c0dd76fa0f28387b67, Data_science\Mathematica\Math\Math\Algebra\Helpers\Variable.vb"

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

    '   Total Lines: 57
    '    Code Lines: 0
    ' Comment Lines: 47
    '   Blank Lines: 10
    '     File Size: 2.00 KB


    ' 
    ' /********************************************************************************/

#End Region

'Imports System.Text

'Namespace BasicR.Helpers

'    Public Class Variable

'        Friend Variables As Dictionary(Of String, MATRIX) = New Dictionary(Of String, MATRIX)

'        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^("
'        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!"

'        Friend VariableList As List(Of String) = New List(Of String)

'        ''' <summary>
'        ''' Add a user constant to the dictionary.
'        ''' (向字典之中添加用户自定义常数)
'        ''' </summary>
'        ''' <param name="Name"></param>
'        ''' <param name="value"></param>
'        ''' <remarks>
'        ''' const [name] [value]
'        ''' </remarks>
'        Public Sub [Set](Name As String, value As String)
'            Dim var As String = Name.ToLower

'            If VariableList.IndexOf(var) > -1 Then
'                Variables.Remove(var)
'            Else
'                Call VariableList.Clear()
'                Call VariableList.AddRange(Variables.Keys)
'            End If

'            Variables(var) = Expression.Evaluate(value)
'        End Sub

'        ''' <summary>
'        ''' Add a user const from the input of user on the console.
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub [Set](statement As String)
'            Dim Name As String = statement.Split.First
'            Call [Set](Name, Mid(statement, Len(Name) + 2))
'        End Sub

'        ''' <summary>
'        ''' Assign the new value for a variable
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub AssignValue(statement As String)
'            'var <- new_value_expression
'            Dim Tokens As String() = Split(statement, "<-")
'            Call [Set](Tokens.First.Trim, Expression.Evaluate(Tokens.Last.Trim))
'        End Sub
'    End Class
'End Namespace
