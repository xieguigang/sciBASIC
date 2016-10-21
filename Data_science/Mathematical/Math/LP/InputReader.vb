Imports System.Collections.Generic
Imports System.IO

Namespace LP


    Public Module InputReader

        Private Const DELIMINATOR As String = ","

        Public Function readInput(ByVal fileName As String) As IList(Of IList(Of Double))
            Dim file As New FileStream(fileName, FileMode.Open)
            Dim input As New List(Of IList(Of Double))
            Using br As New StreamReader(file)
                Try
                    Dim line As String
                    line = br.ReadLine()
                    Do While line IsNot Nothing
                        Dim equation As IList(Of Double) = convertToDouble(StringSplit(line, DELIMINATOR, True))
                        input.Add(equation)
                        line = br.ReadLine()
                    Loop
                Catch e As Exception
                    Console.WriteLine(e.ToString())
                    Console.Write(e.StackTrace)
                End Try
            End Using
            Return input
        End Function

        Public Function convertToDouble(ByVal strings As IEnumerable(Of String)) As List(Of Double)
            Return strings.ToList(AddressOf Val)
        End Function

        Public Function convertDoubles(ByVal [in] As IList(Of Double)) As Double()
            If [in] Is Nothing Then Return New Double() {}
            Return [in].ToArray
        End Function

        Public Function convertDoublesMatrix(ByVal [in] As IEnumerable(Of IList(Of Double))) As Double()()
            If [in] Is Nothing OrElse [in].Count = 0 OrElse [in](0).Count = 0 Then Return New Double()() {}
            Dim doubles As Double()() = MAT(Of Double)([in].Count, [in](0).Count)
            For i As Integer = 0 To [in].Count - 1
                doubles(i) = convertDoubles([in](i))
            Next i
            Return doubles
        End Function
    End Module

End Namespace