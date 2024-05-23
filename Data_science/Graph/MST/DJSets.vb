#Region "Microsoft.VisualBasic::17d5fc7aa17c33380e868ac6aa09e872, Data_science\Graph\MST\DJSets.vb"

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

    '   Total Lines: 47
    '    Code Lines: 39 (82.98%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.24 KB


    '     Class DJSets
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: inDJSets
    ' 
    '         Sub: addSet, insertIntoSet, printSets
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MinimumSpanningTree

    Friend Class DJSets

        Private djSets As List(Of DJSet)

        Public Sub New()
            djSets = New List(Of DJSet)()
        End Sub

        Public Sub printSets()
            Dim firstTime = True
            For Each djSet In djSets
                If firstTime Then
                    firstTime = False
                    djSet.print()
                Else
                    Console.Write(",  ")
                    djSet.print()
                End If
            Next
            Console.WriteLine()
        End Sub

        Public Sub addSet(root As Integer)
            djSets.Add(New DJSet(root))
        End Sub

        Public Function inDJSets(root As Integer) As Boolean
            For Each djSet In djSets
                If djSet.Root = root Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub insertIntoSet(root As Integer, i As Integer)
            For Each djSet In djSets
                If djSet.Root = root Then
                    djSet.add(i)
                End If
            Next
        End Sub
    End Class

End Namespace
