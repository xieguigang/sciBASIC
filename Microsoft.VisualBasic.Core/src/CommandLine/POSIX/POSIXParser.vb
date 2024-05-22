#Region "Microsoft.VisualBasic::e46c85a5b3d42b865153ab4fde167e4a, Microsoft.VisualBasic.Core\src\CommandLine\POSIX\POSIXParser.vb"

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

    '   Total Lines: 31
    '    Code Lines: 24 (77.42%)
    ' Comment Lines: 1 (3.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (19.35%)
    '     File Size: 1.12 KB


    '     Module POSIXParser
    ' 
    '         Function: JoinTokens
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Parsers

Namespace CommandLine.POSIX

    Module POSIXParser

        Public Iterator Function JoinTokens(tokens As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim continuteToken As String = Nothing

            For Each item As String In tokens
                If item = "?" OrElse item = "??" OrElse CliArgumentParsers.IsPossibleLogicFlag(item) Then
                    ' 在这里使用nothing来和""产生的空字符串进行区分
                    If Not continuteToken Is Nothing Then
                        Yield continuteToken
                        continuteToken = Nothing
                    End If

                    Yield item
                ElseIf continuteToken Is Nothing Then
                    continuteToken = item
                Else
                    continuteToken = $"{continuteToken} {item}"
                End If
            Next

            If Not continuteToken Is Nothing Then
                Yield continuteToken
            End If
        End Function
    End Module
End Namespace
