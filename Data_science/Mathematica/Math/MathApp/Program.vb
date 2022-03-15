#Region "Microsoft.VisualBasic::3ff60c573606a6eab38c8065bc127750, sciBASIC#\Data_science\Mathematica\Math\MathApp\Program.vb"

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
    '    Code Lines: 34
    ' Comment Lines: 2
    '   Blank Lines: 9
    '     File Size: 1.37 KB


    ' Module Program
    ' 
    '     Function: __calc, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Mathematical.Scripting.Types

Module Program

    Public Function Main() As Integer
        betaTest()
        '   Call CubicSplineTest.Test()
        Call DEBUG.Main()
        Try
            Call New Form1().ShowDialog()

        Catch ex As Exception
            Call App.LogException(ex)
            MsgBox(ex.ToString)
        End Try

        Return GetType(CLI).RunCLI(App.CommandLine, AddressOf __calc, AddressOf CLI.CalcImplicit)
    End Function

    Private Function __calc() As Integer
        Dim s As String, Cmdl As String = String.Empty

#If DEBUG Then
        Dim sExpression As String = "1-2-3+4+5+6+7+8+9+55%6*3^2"
        Dim e As SimpleExpression = SimpleParser.TryParse(sExpression)

        Console.WriteLine("> {0} = {1}", sExpression, e.Evaluate)
        Call DEBUG.Main()
#End If
        '(log(max(sinh(((1-2-3+4+5+6+7+8+9)-20)^0.5)+5,rnd(-10, 100)))!%5)^3!

        Do While Cmdl <> ".quit"
            Console.Write("> ")
            Cmdl = Console.ReadLine
            s = ScriptEngine.Shell(Cmdl)
            If Not String.IsNullOrEmpty(s) Then
                Console.WriteLine("  = {0}", s)
            End If
        Loop

        Return 0
    End Function
End Module
