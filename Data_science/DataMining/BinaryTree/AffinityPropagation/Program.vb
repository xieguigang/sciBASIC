Imports Affinity_Propagation_Clustering.Cluster
Imports Affinity_Propagation_Clustering.DataSet
Imports Affinity_Propagation_Clustering.Utility

Namespace AffinityPropagation
    Friend Class Program

        Public Shared Sub Main()
            'This is a simple driver program 

            Console.WriteLine("Testing:Driver program for Affinity Propagation clustering algorithm.")
            Dim rnd = New ToyDataset()
            Dim s As Stopwatch = New Stopwatch()

            Dim data1 = rnd.DataSet()
            Dim sim = SparseSimilarityMatrix(data1)


            Console.WriteLine($"Data size:{data1.Length} ; SimilarityMatrix size:{sim.Length}")
            Console.WriteLine($"Start at:{Date.Now}")
            s.Start()
            Try
                Dim model As AffinityPropagation = New AffinityPropagation(data1.Length)
                Dim centers = model.Fit(sim)
                Print(centers)
                ClusterUtility.AssignClusterCenters(data1, centers)
                Dim centers_index = New Integer(model.Centers.Count - 1) {}
                model.Centers.CopyTo(centers_index)
                Dim t = ClusterUtility.GroupClusters(data1, centers, centers_index)
                'print the clusters (grouped)
                Print(t)
            Catch e As Exception
                Console.WriteLine($"{e.Message}")
            End Try
            s.Stop()
            Console.WriteLine($"
Ending at:{Date.Now}")
            Console.WriteLine($"Ellapsed time: {s.ElapsedMilliseconds} ms  | {s.Elapsed.TotalSeconds} s | {s.Elapsed.TotalMinutes} m")




        End Sub

        Public Shared Sub Print(clusteredData As Integer())
            Console.WriteLine()
            For Each s In clusteredData
                Console.Write($"{s} ")
            Next
            Console.WriteLine()
        End Sub
        Public Shared Sub Print(clusters As List(Of Point)())
            Dim i = 0

            While i < clusters.Length
                Console.Write($"[{i}]->")
                For Each b In clusters(i)
                    Console.Write($"[({b.Coordinates(0).ToString("n2")},{b.Coordinates(1).ToString("n2")})] ")
                Next
                Console.WriteLine(vbLf)
                i += 1
            End While
        End Sub

    End Class
End Namespace
