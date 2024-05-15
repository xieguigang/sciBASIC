#Region "Microsoft.VisualBasic::d46cf247ab44d72299a09e5076309005, Data_science\DataMining\DataMining\DecisionTree\VisualDebugger.vb"

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

    '   Total Lines: 51
    '    Code Lines: 41
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 2.23 KB


    '     Module VisualDebugger
    ' 
    '         Sub: Print, PrintLegend
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DecisionTree

    ''' <summary>
    ''' Display debug info on console
    ''' </summary>
    Public Module VisualDebugger

        Public Sub Print(node As TreeNode, result As String)
            If node?.childNodes Is Nothing OrElse node.childNodes.Count = 0 Then
                Dim seperatedResult = result.Split(" "c)

                For Each item In seperatedResult
                    If item.Equals(seperatedResult(0)) Then
                        Console.ForegroundColor = ConsoleColor.Magenta
                        ' empty if but better than checking at .ToUpper() and .ToLower() if
                    ElseIf item.Equals("--") OrElse item.Equals("-->") Then
                    ElseIf item.Equals("YES") OrElse item.Equals("NO") Then
                        Console.ForegroundColor = ConsoleColor.Green
                    ElseIf item.ToUpper().Equals(item) Then
                        Console.ForegroundColor = ConsoleColor.Cyan
                    Else
                        Console.ForegroundColor = ConsoleColor.Yellow
                    End If

                    Console.Write($"{item} ")
                    Console.ResetColor()
                Next

                Console.WriteLine()
            Else
                For Each child In node.childNodes
                    Print(child, result & " -- " & child.edge.ToLower() & " --> " & child.name.ToUpper())
                Next
            End If
        End Sub

        Public Sub PrintLegend(headline As String)
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine(vbLf & $"{headline}")
            Console.ForegroundColor = ConsoleColor.Magenta
            Console.WriteLine("Magenta color indicates the root node")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("Yellow color indicates an edge")
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.WriteLine("Cyan color indicates a node")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Green color indicates a decision")
            Console.ResetColor()
        End Sub
    End Module
End Namespace
