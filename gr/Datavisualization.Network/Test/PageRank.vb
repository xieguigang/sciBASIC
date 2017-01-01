Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic

Namespace pagerank


	'
	' * description:	implemented PageRank on undirected, weighted graph
	' * Reference:	http://en.wikipedia.org/wiki/Pagerank
	' * developed by JIA Xun
	' * website:	jia1546.is-programmer.com
	' * E-mail:	jia1546@163.com
	' 

	Public Class PageRank
		Private map As IDictionary(Of String, List(Of MapEntry)) = Nothing
		Private rankedList As IList(Of PageRankNode) = Nothing

		Public Sub New()
			map = New Dictionary(Of String, List(Of MapEntry))
		End Sub

        '	
        '	 * construct linked list representation of graph using hash map
        '	 * this version uses file to initialize
        '	 
        Public Overridable Sub initializeMap(addr As String)
            'Dim inputStream As java.io.BufferedReader = Nothing
            'Dim line As String = Nothing
            'Try
            '	inputStream = New java.io.BufferedReader(New java.io.FileReader(addr))
            '	line = inputStream.readLine()

            '	Dim node1 As String = Nothing, node2 As String = Nothing
            '	Dim edgeWeight As Double = 0
            '	Do While line IsNot Nothing
            '		line = line.Trim()

            '		'too complex, another simple method to handle this
            '		'int FirstPosition = line.indexOf('\t');
            '		'int LastPosition = line.lastIndexOf('\t');
            '		Dim entries As String() = StringHelperClass.StringSplit(line, vbTab, True)

            '		node1 = entries(0)
            '		node2 = entries(1)
            '		edgeWeight = Convert.ToDouble(entries(2))

            '		Me.addEntry(node1, node2, edgeWeight)
            '		Me.addEntry(node2, node1, edgeWeight)

            '		line = inputStream.readLine()
            '	Loop
            '	inputStream.close()
            'Catch e As java.io.FileNotFoundException
            '	Console.WriteLine(e.ToString())
            '	Console.Write(e.StackTrace)
            'Catch e As java.io.IOException
            '	Console.WriteLine(e.ToString())
            '	Console.Write(e.StackTrace)
            'End Try

            'Console.WriteLine("initialize map from file finished...")
        End Sub

        ''' <summary>
        ''' * undirected graph, add entry ``(node1, &lt;node2, weight>), (node2, &lt;node1, weight>)``.
        ''' (对于有向图，只需要添加一次就好了，对于无向图，则添加两次，两个方向)
        ''' </summary>
        ''' <param name="[to]"></param>
        ''' <param name="from"></param>
        ''' <param name="edgeWeight"></param>
        Public Overridable Sub addEntry(from As String, [to] As String, Optional edgeWeight As Double = 0.5R)
            Dim mapEntry As New MapEntry(from, edgeWeight)
            If Me.map.ContainsKey([to]) Then
                If Not map([to]).Contains(mapEntry) Then Me.map([to]).Add(mapEntry)
            Else
                Dim list As New List(Of MapEntry)
                list.Add(mapEntry)
                Me.map([to]) = list
            End If
        End Sub

        Public Sub AddEntry(from As String, [to] As String, directed As Boolean, Optional weight As Double = 0.5R)
            Call addEntry(from, [to], weight)

            If Not directed Then
                Call addEntry([to], from, weight)
            End If
        End Sub

        ''' <summary>
        ''' 有些page只会有其他的page指向进来，但是没有指向出去，则<see cref="map"/>里面会不存在这个节点，计算会出错，则在这里修正这个错误
        ''' </summary>
        Private Sub __checkGaps()
            Dim allNodes As String() = map.Values _
                .Select(
                    Function(x) x.Select(
                    Function(y) y.Identifier)) _
                .Unlist _
                .Distinct _
                .ToArray

            For Each id$ In allNodes
                If Not map.ContainsKey(id) Then
                    Call map.Add(id, New List(Of MapEntry))
                End If
            Next
        End Sub

        Public Overridable Sub rank(iterations As Integer, dampingFactor As Double)
            Dim lastRanking As New Dictionary(Of String, Double)
            Dim nextRanking As New Dictionary(Of String, Double)

            ' 因为可能会因为填补空缺而添加入新的节点，则map.Count变了，需要在这之前进行确认
            Call __checkGaps()

            Dim startRank As Double = 1.0 / map.Count

            For Each key As String In map.Keys
                lastRanking(key) = startRank
            Next

            Dim dampingFactorComplement As Double = 1.0 - dampingFactor

            For times As Integer = 0 To iterations - 1
                For Each key As String In map.Keys
                    Dim totalWeight As Double = 0
                    For Each entry As MapEntry In map(key)
                        totalWeight += (entry.Weight * lastRanking(entry.Identifier) / (map(entry.Identifier).Count + 1))
                    Next

                    Dim nextRank As Double = dampingFactorComplement + (dampingFactor * totalWeight)

                    nextRanking(key) = nextRank
                Next
                lastRanking = nextRanking
            Next

            ' Console.WriteLine(iterations & " times iteration finished...")

            rankedList = PageRankVector(lastRanking)

        End Sub

        Public Overridable Sub saveRankedResults(writeAddr As String)
            'Dim file As New File(writeAddr)
            'Dim writer As java.io.PrintWriter = Nothing

            'Try
            '	writer = New java.io.PrintWriter(file)
            '	For Each node As PageRankNode In rankedList
            '		writer.println(node.Identifier & vbTab & node.Rank)
            '	Next node
            '	Console.WriteLine("save ranked results finished...")
            '	writer.close()
            'Catch e As java.io.FileNotFoundException
            '	Console.WriteLine(e.ToString())
            '	Console.Write(e.StackTrace)
            'End Try
        End Sub

        '	
        '	 * show pagerank results in pretty presentation
        '	 
        Public Overridable Sub showResults(topK As Integer)
            Console.WriteLine("--------------------------------------")
            Console.WriteLine("     node     |          rank         ")
            Console.WriteLine("--------------------------------------")

            Dim startIndex As Integer = 0

            If topK >= rankedList.Count Then
                topK = rankedList.Count
            End If

            For i As Integer = 0 To topK - 1
                Dim key As String = rankedList(startIndex).Identifier

                Do While key.StartsWith("N") 'this is a pattern node
                    startIndex += 1
                    If startIndex > rankedList.Count Then
                        Console.WriteLine("number of T nodes < top-K")
                        Environment.Exit(0)
                    End If
                    key = rankedList(startIndex).Identifier
                Loop

                'double rank = lastRanking.get(key);
                Dim rank As Double = rankedList(startIndex).Rank
                Console.WriteLine("     " & key & "    |" & "     " & rank & "     ")

                startIndex += 1
            Next
        End Sub


        '	
        '	 * get ranked results using Collections's sort method
        '	 
        Public Overridable Function PageRankVector(LastRanking As Dictionary(Of String, Double)) As IList(Of PageRankNode)
            Dim nodeList As New List(Of PageRankNode)
            For Each identifier As String In LastRanking.Keys
                Dim node As New PageRankNode(identifier, LastRanking(identifier))
                nodeList.Add(node)
            Next
            nodeList.Sort()
            Return nodeList
        End Function

        Public Shared Sub Main(args As String())
            Dim startTime As Long = App.NanoTime

            '	    
            '	     * @readAddr:	set the path  to read graph file
            '	     * @topK:	present to user the top k results
            '	     * @iteration	number of iterations to compute pageRank, usually 10 iterations leads to convergence
            '	     * @writeAddr	set the path to save ranked results
            '	     
            Dim readAddr As String = "/tfacts_result.txt"
            Dim writeAddr As String = "D://rankedResults.txt"
            Dim iterations As Integer = 32
            Dim DumpingFactor As Double = 0.85
            Dim topK As Integer = 100

            Dim pagerank As New PageRank
            pagerank.initializeMap(readAddr)


            'pagerank.addEntry(1, 2)
            'pagerank.addEntry(1, 3)
            'pagerank.addEntry("1", "4")
            'pagerank.addEntry("2", "3")
            'pagerank.addEntry("2", "4")
            'pagerank.addEntry("3", "4")
            'pagerank.addEntry("4", "2")

            pagerank.addEntry("D", "A")
            pagerank.addEntry("D", "B")
            pagerank.addEntry("B", "C")
            pagerank.addEntry("C", "B")
            pagerank.addEntry("E", "D")
            pagerank.addEntry("E", "F")
            pagerank.addEntry("E", "B")
            pagerank.addEntry("F", "B")
            pagerank.addEntry("F", "E")
            pagerank.addEntry("G", "B")
            pagerank.addEntry("G", "E")
            pagerank.addEntry("H", "B")
            pagerank.addEntry("H", "E")
            pagerank.addEntry("I", "B")
            pagerank.addEntry("I", "E")
            pagerank.addEntry("J", "E")
            pagerank.addEntry("K", "E")

            pagerank.rank(iterations, DumpingFactor)
            pagerank.showResults(topK)

            'sava ranked results in text form
            pagerank.saveRankedResults(writeAddr)

            Dim endTime As Long = App.NanoTime
            Dim time As Long = endTime - startTime
            Console.WriteLine("program runs " & time & "ms")

            Pause()
        End Sub
    End Class

End Namespace