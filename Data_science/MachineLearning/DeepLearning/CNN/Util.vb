Imports Microsoft.VisualBasic.Language
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace CNN

    Module Util

        ''' <summary>
        ''' the batch epochs helper
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="batchSize"></param>
        ''' <returns></returns>
        Public Function randomPerm(size As Integer, batchSize As Integer) As Integer()
            Dim [set] As ISet(Of Integer?) = New HashSet(Of Integer?)()
            While [set].Count < batchSize
                [set].Add(randf.Next(size))
            End While
            Dim randPerm = New Integer(batchSize - 1) {}
            Dim i As i32 = 0
            For Each value In [set]
                randPerm(++i) = value.Value
            Next
            Return randPerm
        End Function
    End Module
End Namespace