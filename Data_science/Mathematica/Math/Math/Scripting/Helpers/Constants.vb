#Region "Microsoft.VisualBasic::0f0d63b6210da2b50fcb68c5c4e756a3, Data_science\Mathematica\Math\Math\Scripting\Helpers\Constants.vb"

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

    '     Class Constants
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [GET]
    ' 
    '         Sub: (+3 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Text
Imports sys = System.Math

Namespace Scripting.Helpers

    Public Class Constants : Inherits MemoryCollection(Of Double)

        Sub New(engine As Expression)
            Call MyBase.New(engine)
            Call MyBase.Add(NameOf(sys.E), sys.E, False, True)
            Call MyBase.Add(NameOf(Math.PI), sys.PI, False, True)
            Call MyBase.Add(NameOf(Scan0), Scan0, False, True)
            Call __buildCache()
        End Sub

        ''' <summary>
        ''' 常量是区分大小写的
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function [GET](x As String, ByRef success As Boolean) As Double
            If objTable.ContainsKey(x) Then
                success = True
                Return objTable(x)
            Else
                success = False
                Return -1
            End If
        End Function

        ''' <summary>
        ''' Add a user constant to the dictionary.
        ''' (向字典之中添加用户自定义常数)
        ''' </summary>
        ''' <param name="Name">常数名称是大小写敏感的，变量的大小写却不敏感</param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Public Overloads Sub Add(Name As String, value As Double)
            If objTable.ContainsKey(Name) Then
                Dim msg As String = $"Constant not set as the const ""{Name}"" is already set in this engine."
                Throw New Exception(msg)
            Else
                Call MyBase.Add(Name, value, True, True)
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="expr">通过计算这个表达式来得到常数值</param>
        Public Overloads Sub Add(name As String, expr As String)
            Dim val As Double = __engine.Evaluation(expr)
            Call Add(name, val)
        End Sub

        ''' <summary>
        ''' Add a user const from the input of user on the console.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <remarks></remarks>
        Public Overloads Sub Add(statement As String)
            Dim Name As String = statement.Split.First
            Dim expr As String = Mid(statement, Len(Name) + 2)
            Call Add(Name, expr)
        End Sub
    End Class
End Namespace
