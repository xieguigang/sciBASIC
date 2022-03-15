#Region "Microsoft.VisualBasic::75d33877a42e5a7dd0874cad46098feb, sciBASIC#\Data_science\Mathematica\Math\MathApp\CLI.vb"

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

    '   Total Lines: 18
    '    Code Lines: 11
    ' Comment Lines: 5
    '   Blank Lines: 2
    '     File Size: 583.00 B


    ' Module CLI
    ' 
    '     Function: CalcImplicit
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Mathematical.Scripting.Types

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
