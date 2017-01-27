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

Imports System.Runtime.CompilerServices

Namespace Scripting

    ''' <summary>
    ''' Math expression script engine.
    ''' </summary>
    Public Module ScriptEngine

        ''' <summary>
        ''' The default expression engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Expression As Expression = Expression.DefaultEngine

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
                {".quit", Sub(null$) null = Nothing}}

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
        Public Function Shell(statement$) As Double
            Dim Token As String = statement.Split.First.ToLower

            ' This is a value assignment statement
            If InStr(statement, "<-") Then
                Return Expression.Variables.AssignValue(statement)
            End If

            If StatementEngine.ContainsKey(Token) Then
                Call StatementEngine(Token)(Mid(statement, Len(Token) + 1).Trim)
                Return 0
            Else
                ' if the statement input from the user is not appears 
                ' in the engine dictionary, then maybe is a mathematics expression. 
                Dim Result As Double = Expression.Evaluate(statement)
                ' You can treat the variable 'last' as a system variable for return 
                ' the Result of a multiple function script in the future of this 
                ' feature will add to this module.
                Expression.Variables.Set("$", Result)
                Return Result
            End If
        End Function

        ''' <summary>
        ''' <see cref="Shell"/> function name alias.
        ''' </summary>
        ''' <param name="statement$"></param>
        ''' <returns></returns>
        <Extension> Public Function Evaluate(statement$, Optional echo As Boolean = True) As Double
            Dim x# = Shell(statement)

            If echo Then
                Call statement.__DEBUG_ECHO
                Call $" = {x}".__DEBUG_ECHO
            End If

            Return x
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
End Namespace