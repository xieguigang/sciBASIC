Imports System
Imports System.Collections.Generic
Imports System.IO

Imports TextGraph = graphloading.TextGraph
Imports OrthographicEmbeddingOptimizer = optimization.OrthographicEmbeddingOptimizer
Imports SegmentLengthEmbeddingComparator = optimization.SegmentLengthEmbeddingComparator
Imports OrthographicEmbeddingResult = orthographicembedding.OrthographicEmbeddingResult
Imports SavePNG = util.SavePNG

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

''' 
''' <summary>
''' @author santi
''' </summary>
Public Class Main2
    Public Shared randomSeed As Long? = Nothing
    Public Shared r As Random = New Random() ' we will create a single "Random" object that will be used by
    ' all methods, so we can control it by setting a random seed, etc.

    Public Const output_type_txt As String = "txt"

    Friend Shared inputFileName As String = Nothing
    Friend Shared outputFileName As String = Nothing
    Friend Shared fixNonOrthogonal As Boolean = True
    Friend Shared simplify As Boolean = True
    Friend Shared optimize As Boolean = True
    Friend Shared numberOfAttempts As Integer = 1
    Friend Shared output_type As String = output_type_txt

    Friend Shared outputPNGName As String = Nothing
    Friend Shared PNGcellWidth As Integer = 48
    Friend Shared PNGcellHeight As Integer = 48
    Friend Shared PNGlabelVertices As Boolean = True


    Public Shared Sub Main(args As String())
        If Not parseArguments(args) Then
            printInstructions()
            Environment.Exit(0)
        End If

        Dim graph = loadGraph(inputFileName)
        If graph Is Nothing Then
            Console.Error.WriteLine("Cannot load input-file " & inputFileName)
            Environment.Exit(1)
        End If


        Dim disconnectedGraphs As IList(Of IList(Of Integer)) = DisconnectedGraphs.findDisconnectedGraphs(graph)
        Dim disconnectedEmbeddings As IList(Of OrthographicEmbeddingResult) = New List(Of OrthographicEmbeddingResult)()
        For Each nodeSubset In disconnectedGraphs
            ' calculate the embedding:
            Dim best_g_oe As OrthographicEmbeddingResult = Nothing
            Dim g As Integer()() = DisconnectedGraphs.subgraph(graph, nodeSubset)
            Dim comparator As SegmentLengthEmbeddingComparator = New SegmentLengthEmbeddingComparator()
            For attempt = 0 To numberOfAttempts - 1
                Dim g_oe As OrthographicEmbeddingResult = orthographicembedding.OrthographicEmbedding.orthographicEmbedding(g, simplify, fixNonOrthogonal, r)
                If g_oe Is Nothing Then
                    Continue For
                End If
                If Not g_oe.sanityCheck(False) Then
                    Throw New Exception("The orthographic projection contains errors!")
                End If
                If optimize Then
                    g_oe = OrthographicEmbeddingOptimizer.optimize(g_oe, g, comparator)
                    If Not g_oe.sanityCheck(False) Then
                        Throw New Exception("The orthographic projection after optimization contains errors!")
                    End If
                End If
                If best_g_oe Is Nothing Then
                    best_g_oe = g_oe
                Else
                    If comparator.compare(g_oe, best_g_oe) < 0 Then
                        best_g_oe = g_oe
                    End If
                End If
            Next
            disconnectedEmbeddings.Add(best_g_oe)
        Next
        Dim oe As OrthographicEmbeddingResult = DisconnectedGraphs.mergeDisconnectedEmbeddingsSideBySide(disconnectedEmbeddings, disconnectedGraphs, 1.0)

        ' save the results:
        saveEmbedding(outputFileName, oe)

        ' save image:
        If Not ReferenceEquals(outputPNGName, Nothing) Then
            SavePNG.savePNG(outputPNGName, oe, PNGcellWidth, PNGcellHeight, PNGlabelVertices)
        End If
    End Sub


    Public Shared Function parseArguments(args As String()) As Boolean
        'System.out.println(Arrays.toString(args));
        If args.Length < 2 Then
            Return False
        End If
        inputFileName = args(0)
        outputFileName = args(1)
        For i = 2 To args.Length - 1
            If args(i).StartsWith("-png:", StringComparison.Ordinal) Then
                outputPNGName = args(i).Substring(5)
            ElseIf args(i).StartsWith("-simplify", StringComparison.Ordinal) Then
                If args(i).Equals("-simplify:true") Then
                    simplify = True
                End If
                If args(i).Equals("-simplify:false") Then
                    simplify = False
                End If
            ElseIf args(i).StartsWith("-optimize", StringComparison.Ordinal) Then
                If args(i).Equals("-optimize:true") Then
                    optimize = True
                End If
                If args(i).Equals("-optimize:false") Then
                    optimize = False
                End If
            ElseIf args(i).StartsWith("-attempts:", StringComparison.Ordinal) Then
                Dim str = args(i).Substring(10)
                numberOfAttempts = Integer.Parse(str)
            ElseIf args(i).StartsWith("-rs:", StringComparison.Ordinal) Then
                Dim str = args(i).Substring(4)
                randomSeed = Long.Parse(str)
            Else
                Console.Error.WriteLine("Unrecognized parameter " & args(i))
                Return False
            End If
        Next

        Return True
    End Function


    Public Shared Sub printInstructions()
        Console.WriteLine("Orthographic Graph Embedder (OGE) v1.0 by Santiago Ontañón (2016)")
        Console.WriteLine("")
        Console.WriteLine("This tool computes an orthographic embedding of a plannar input graph. " & "Although the tool was originally designed to be part of a procedural-content generation (PCG) " & "module for a game, it is designed to be usable to find orthographic embeddings for any planar " & "input graphs via the use of PQ-trees.")
        Console.WriteLine("")
        Console.WriteLine("Example usage: java -classpath OGE.jar Main examples/graph1 examples/oe1.txt -png:examples/oe1.png")
        Console.WriteLine("")
        Console.WriteLine("parameters: input-file output-file options")
        Console.WriteLine("  input-file: a file containing the adjacency matrix of a graph")
        Console.WriteLine("  output-file: the desired output filename")
        Console.WriteLine("Options:")
        Console.WriteLine("  -output:[type] : the type of output desired, which can be:")
        Console.WriteLine("        txt (default): a text file with the connectivity matrix, and then a list of vertices, with their mapping to the original vertices, and their coordinates in the orthographic embedding.")
        Console.WriteLine("        (more output types might be added in the future)")
        Console.WriteLine("  -png:filename : saves a graphical version of the output as a .png file")
        Console.WriteLine("  -simplify:true/false : defaults to true, applies a filter to try to reduce unnecessary auxiliary vertices.")
        Console.WriteLine("  -optimize:true/false : defaults to true, postprocesses the output to try to make it more compact.")
        Console.WriteLine("  -attempts:XXX : defaults to 1, number of random embeddings that will be generated (only the best one will be finally selected).")
        Console.WriteLine("  -rs:XXX : specifies the random seed for the random number generator.")
        Console.WriteLine("")
    End Sub

    Public Shared Function loadGraph(fileName As String) As Integer()()

        If fileName.EndsWith(".txt", StringComparison.Ordinal) Then
            Return TextGraph.loadGraph(fileName)
        End If

        Console.Error.WriteLine("Unrecognized graph file format: " & fileName)
        Return Nothing
    End Function

    Private Shared Sub saveEmbedding(fileName As String, oe As OrthographicEmbeddingResult)
        Dim fw As StreamWriter = New StreamWriter(fileName)
        If output_type.Equals(output_type_txt) Then
            For i = 0 To oe.nodeIndexes.Length - 1
                For j = 0 To oe.nodeIndexes.Length - 1
                    If oe.edges(i)(j) OrElse oe.edges(j)(i) Then
                        fw.Write("1" & ", ")
                    Else
                        fw.Write("0" & ", ")
                    End If
                Next
                fw.Write(vbLf)
            Next
            For i = 0 To oe.nodeIndexes.Length - 1
                fw.Write(i.ToString() & ", " & oe.nodeIndexes(i).ToString() & ", " & oe.x(i).ToString() & ", " & oe.y(i).ToString() & vbLf)
            Next
        Else
            Console.Error.WriteLine("Unknown output type: " & output_type)
        End If
        fw.Close()
    End Sub


End Class
