#Region "Microsoft.VisualBasic::be910c7052ce588d26e7084892359f54, Microsoft.VisualBasic.Core\test\indexTest.vb"

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

    ' Module indexTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Trinity

Module indexTest

    Sub Main()
        Dim index As New WordSimilarityIndex(Of String)

        For Each item As String In 5000.SeqRandom.Select(Function(l) RandomASCIIString(20, True))
            If Not index.HaveKey(item) Then
                Call index.AddTerm(item, item)
            End If
        Next

        Dim result = index.FindMatches("Aaaaaaaaaaaaaaaaaaaa").ToArray

        Pause()
    End Sub
End Module
