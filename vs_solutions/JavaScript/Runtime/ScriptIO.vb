#Region "Microsoft.VisualBasic::687aa61e4959857b3e82922c6ab65522, vs_solutions\JavaScript\Runtime\ScriptIO.vb"

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

    '   Total Lines: 26
    '    Code Lines: 16 (61.54%)
    ' Comment Lines: 1 (3.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (34.62%)
    '     File Size: 663 B


    '     Class ScriptIO
    ' 
    ' 
    ' 
    '     Class LogTextIO
    ' 
    '         Properties: Lines
    ' 
    '         Sub: WriteLine
    ' 
    '     Class ConsoleIO
    ' 
    '         Sub: WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>Sink for console.log etc.; records lines for test/e2e comparison.</summary>
    Public MustInherit Class ScriptIO

        Public MustOverride Sub WriteLine(text As String)

    End Class

    Public Class LogTextIO : Inherits ScriptIO

        Public ReadOnly Property Lines As New List(Of String)

        Public Overrides Sub WriteLine(text As String)
            Lines.Add(text)
        End Sub

    End Class

    Public Class ConsoleIO : Inherits ScriptIO

        Public Overrides Sub WriteLine(text As String)
            Call Console.WriteLine(text)
        End Sub
    End Class
End Namespace
