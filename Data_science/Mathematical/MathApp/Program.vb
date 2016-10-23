#Region "Microsoft.VisualBasic::d2ffc0d5dab1ae43696864908a7a003f, ..\visualbasic_App\Data_science\Mathematical\MathApp\Program.vb"

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

Imports Microsoft.VisualBasic.Mathematical

Module Program

    Public Function Main() As Integer
        Try
            Call New Form1().ShowDialog()
            Call DEBUG.Main()
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
        Dim e As Microsoft.VisualBasic.Mathematical.Types.SimpleExpression = SimpleParser.TryParse(sExpression)

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
