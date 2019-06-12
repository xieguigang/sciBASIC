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