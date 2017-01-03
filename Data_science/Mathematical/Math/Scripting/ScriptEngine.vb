#Region "Microsoft.VisualBasic::02c1fcd504fb9e4fcbfde18be7bc1eee, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\ScriptEngine.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

''' <summary>
''' Math expression script engine.
''' </summary>
Public Module ScriptEngine

    ''' <summary>
    ''' The default expression engine.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Expression As Expression =
        Mathematical.Expression.DefaultEngine

    ''' <summary>
    ''' all of the commands are stored at here
    ''' </summary>
    ''' <remarks>
    ''' .quit for do nothing and end of this program.
    ''' </remarks>
    Public ReadOnly Property StatementEngine As Dictionary(Of String, Action(Of String)) =
        New Dictionary(Of String, Action(Of String)) From {
            {"const", AddressOf Expression.Constant.Add},
            {"function", AddressOf Expression.Functions.Add},
            {"func", AddressOf Expression.Functions.Add},
            {"var", AddressOf Expression.Variables.Set},
            {".quit", Sub(NULL As String) NULL = Nothing}}

    ''' <summary>
    ''' Lambda expression table.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Scripts As New Hashtable

    ''' <summary>
    ''' Run the simple script that stores in the <see cref="Scripts"/> table.
    ''' </summary>
    ''' <param name="statement"></param>
    ''' <returns></returns>
    Public Function Shell(statement$) As String
        Dim Token As String = statement.Split.First.ToLower

        If InStr(statement, "<-") Then  'This is a value assignment statement
            Call Expression.Variables.AssignValue(statement)
            Return String.Empty
        End If

        If StatementEngine.ContainsKey(Token) Then
            Call StatementEngine(Token)(Mid(statement, Len(Token) + 1).Trim)
            Return String.Empty
        Else  'if the statement input from the user is not appears in the engine dictionary, then maybe is a mathematics expression. 
            Dim Result As Double = Expression.Evaluate(statement)
            Expression.Variables.Set("$", Result)  'You can treat the variable 'last' as a system variable for return the result of a multiple function script in the future of this feature will add to this module.

            Return Result
        End If
    End Function

    ''' <summary>
    ''' Set variable value
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="expr"></param>
    Public Sub SetVariable(Name$, expr$)
        Call Expression.Variables.Set(Name, expr)
    End Sub

    ''' <summary>
    ''' Add constant object
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="expr"></param>
    Public Sub AddConstant(Name$, expr$)
        Call Expression.Constant.Add(Name, expr)
    End Sub
End Module
