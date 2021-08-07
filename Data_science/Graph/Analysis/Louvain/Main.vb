Imports System
Imports System.Collections.Generic

Namespace Analysis.Louvain

    Public Class Main
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public static void writeOutputJson(String fileName, Louvain a) throws java.io.IOException
        Public Shared Sub writeOutputJson(ByVal fileName As String, ByVal a As Louvain)
            Dim bufferedWriter As StreamWriter
            bufferedWriter = New StreamWriter(fileName)
            bufferedWriter.Write("{" & vbLf & """nodes"": [" & vbLf)

            For i = 0 To a.global_n - 1
                bufferedWriter.Write("{""id"": """ & i & """, ""group"": " & a.global_cluster(i) & "}")

                If i + 1 <> a.global_n Then
                    bufferedWriter.Write(",")
                End If

                bufferedWriter.Write(vbLf)
            Next

            bufferedWriter.Write("]," & vbLf & """links"": [" & vbLf)

            For i = 0 To a.global_n - 1
                Dim j = a.global_head(i)

                While j <> -1
                    Dim k = a.global_edge(j).v
                    bufferedWriter.Write("{""source"": """ & i & """, ""target"": """ & k & """, ""value"": 1}")

                    If i + 1 <> a.global_n OrElse a.global_edge(j).next <> -1 Then
                        bufferedWriter.Write(",")
                    End If

                    bufferedWriter.Write(vbLf)
                    j = a.global_edge(j).next
                End While
            Next

            bufferedWriter.Write("]" & vbLf & "}" & vbLf)
            bufferedWriter.Close()
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: static void writeOutputCluster(String fileName, Louvain a) throws java.io.IOException
        Friend Shared Sub writeOutputCluster(ByVal fileName As String, ByVal a As Louvain)
            Dim bufferedWriter As StreamWriter
            bufferedWriter = New StreamWriter(fileName)

            For i = 0 To a.global_n - 1
                bufferedWriter.Write(Convert.ToString(a.global_cluster(i)))
                bufferedWriter.Write(vbLf)
            Next

            bufferedWriter.Close()
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: static void writeOutputCircle(String fileName, Louvain a) throws java.io.IOException
        Friend Shared Sub writeOutputCircle(ByVal fileName As String, ByVal a As Louvain)
            Dim bufferedWriter As StreamWriter
            bufferedWriter = New StreamWriter(fileName)
            Dim list = New List(Of Object)(a.global_n - 1) {}

            For i = 0 To a.global_n - 1
                list(i) = New List(Of Integer?)()
            Next

            For i = 0 To a.global_n - 1
                list(a.global_cluster(i)).Add(i)
            Next

            For i = 0 To a.global_n - 1

                If list(i).Count = 0 Then
                    Continue For
                End If

                For j = 0 To list(i).Count - 1
                    bufferedWriter.Write(list(i)(j).ToString() & " ")
                Next

                bufferedWriter.Write(vbLf)
            Next

            bufferedWriter.Close()
        End Sub
    End Class
End Namespace