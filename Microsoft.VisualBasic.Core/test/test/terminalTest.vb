﻿#Region "Microsoft.VisualBasic::8b96471fe334e4aeb7810404c0ecb948, sciBASIC#\Microsoft.VisualBasic.Core\test\terminalTest.vb"

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

    '   Total Lines: 12
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 360 B


    ' Module terminalTest
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Module terminalTest

    ReadOnly autoCompleteCandidates As String() = {"file.copy", "file.delete", "file.cache", "file.rename"}

    Sub Main1()
        'Dim shell As New Shell(PS1.Fedora12, AddressOf Console.WriteLine)

        'shell.autoCompleteCandidates.Add("file.copy", "file.delete", "file.cache", "file.rename")
        'shell.Run()

        Call main2()
    End Sub

    Sub main2()
        Dim shell As New LineEditor("foo") With {.HeuristicsMode = "vb"}
        Dim s As Value(Of String) = ""

        shell.AutoCompleteEvent =
            Function(a, pos)
                Dim prefix = a.Substring(0, pos)
                Dim ls As String()

                If prefix.StringEmpty Then
                    ls = autoCompleteCandidates
                Else
                    ls = autoCompleteCandidates.Where(Function(c) c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Select(Function(c) c.Substring(pos)).ToArray
                End If

                Return New Completion(prefix, ls)
            End Function

        Do While s = shell.Edit("> ", "") IsNot Nothing
            Console.WriteLine(s.Value)
        Loop
    End Sub
End Module
