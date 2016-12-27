#Region "Microsoft.VisualBasic::dc46f0a2124bce1808ed4a9e16a766ad, ..\sciBASIC#\Data_science\Mathematical\MathApp\CLI.vb"

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
Imports Microsoft.VisualBasic.Mathematical.Types

Module CLI

    ''' <summary>
    ''' Execute not found
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function CalcImplicit(args As CommandLine.CommandLine) As Integer
        Dim expr As String = args.CLICommandArgvs
        Dim sep As SimpleExpression = ExpressionParser.TryParse(expr)
        Dim n As Double = sep.Evaluate
        Call Console.WriteLine(n)
        Return n
    End Function
End Module
