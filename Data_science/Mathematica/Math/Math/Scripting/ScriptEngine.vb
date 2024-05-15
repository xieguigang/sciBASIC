#Region "Microsoft.VisualBasic::d87b0e7ead61ecca80eda2d8d3ddb432, Data_science\Mathematica\Math\Math\Scripting\ScriptEngine.vb"

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

    '   Total Lines: 148
    '    Code Lines: 72
    ' Comment Lines: 54
    '   Blank Lines: 22
    '     File Size: 5.45 KB


    '     Module ScriptEngine
    ' 
    '         Properties: Expression, Scripts, StatementEngine
    ' 
    '         Function: Evaluate, ParseExpression, Shell
    ' 
    '         Sub: setFunction, SetFunction, setSymbol, (+2 Overloads) SetVariable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports r = System.Text.RegularExpressions.Regex

Namespace Scripting

    ''' <summary>
    ''' Math expression script engine.
    ''' </summary>
    Public Module ScriptEngine

        ''' <summary>
        ''' The default expression engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Expression As New ExpressionEngine

        ''' <summary>
        ''' all of the commands are stored at here
        ''' </summary>
        ''' <remarks>
        ''' .quit for do nothing and end of this program.
        ''' </remarks>
        Public ReadOnly Property StatementEngine As New Dictionary(Of String, Action(Of String)) From {
            {"const", AddressOf setSymbol},
            {"function", AddressOf setFunction},
            {"func", AddressOf setFunction},
            {"var", AddressOf setSymbol},
            {".quit", Sub(null$) null = Nothing}
        }

        ''' <summary>
        ''' Lambda expression table.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Scripts As New Hashtable

        ''' <summary>
        ''' var x = 100
        ''' const x = 100
        ''' </summary>
        ''' <param name="run"></param>
        Private Sub setSymbol(run As String)
            Dim assign As NamedValue(Of String) = run.GetTagValue("=", trim:=True)
            Dim value As Double = Expression.Evaluate(assign.Value)

            Expression.SetSymbol(assign.Name, value)
        End Sub

        ''' <summary>
        ''' func add(x, y) x+y
        ''' </summary>
        ''' <param name="run"></param>
        Private Sub setFunction(run As String)
            Call SetFunction(Expression, run)
        End Sub

        ''' <summary>
        ''' func add(x, y) x+y
        ''' </summary>
        ''' <param name="run"></param>
        ''' 
        <Extension>
        Public Sub SetFunction(engine As ExpressionEngine, run As String)
            Dim declares As String = r.Match(run, ".+\(.+?\)").Value
            Dim lambda As String = Mid(run, declares.Length)
            Dim name As String = declares.Split("("c).First
            Dim parameters As String() = declares.GetStackValue("(", ")").StringSplit("\s*,\s*")

            Call engine.AddFunction(declares, parameters, lambda)
        End Sub

        ''' <summary>
        ''' Run the simple script that stores in the <see cref="Scripts"/> table.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <returns></returns>
        Public Function Shell(statement$) As Double
            If InStr(statement, "<-") Then
                ' This is a value assignment statement
                Dim tokens As NamedValue(Of String) = statement.GetTagValue("<-", trim:=True)
                Dim result As Double = tokens.Value.DoCall(AddressOf Expression.Evaluate)

                Expression.SetSymbol(tokens.Name, result)

                Return result
            End If

            Dim token As String = statement.Split.First.ToLower

            If StatementEngine.ContainsKey(token) Then
                Call StatementEngine(token)(Mid(statement, Len(token) + 1).Trim)
                Return 0
            Else
                ' if the statement input from the user is not appears 
                ' in the engine dictionary, then maybe is a mathematics expression. 
                Dim result As Double = Expression.Evaluate(statement)
                ' You can treat the variable 'last' as a system variable for return 
                ' the Result of a multiple function script in the future of this 
                ' feature will add to this module.
                Expression.SetSymbol("$", result)
                Return result
            End If
        End Function

        ' 字符串插值脚本
        ' expr$ = "&blablabla $x + $y = $z blablabla...."

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
        ''' <param name="name"></param>
        ''' <param name="expr"></param>
        Public Sub SetVariable(name$, expr$)
            Call Expression.SetSymbol(name, expr)
        End Sub

        Public Sub SetVariable(name$, value As Double)
            Call Expression.SetSymbol(name, value)
        End Sub

        Public Function ParseExpression(expression As String) As Expression
            Return New ExpressionTokenIcer(expression) _
                .GetTokens _
                .ToArray _
                .DoCall(AddressOf ExpressionBuilder.BuildExpression)
        End Function
    End Module
End Namespace
