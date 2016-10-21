Imports System.Collections.Generic
Imports System.IO

Namespace LP

    Public Module InputReader

        Private Const DELIMINATOR As String = ","

        ''' <summary>
        ''' 这个方法是直接读取已经构件好的矩阵来初始化线性规划计算模型
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <returns></returns>
        Public Function readInput(fileName As String) As List(Of List(Of Double))
            Dim file As New FileStream(fileName, FileMode.Open)
            Dim input As New List(Of List(Of Double))

            Using br As New StreamReader(file)
                Dim line As String = br.ReadLine()

                Try
                    Do While line IsNot Nothing
                        Dim equation As List(Of Double) = convertToDouble(StringSplit(line, DELIMINATOR, True))
                        input.Add(equation)
                        line = br.ReadLine()
                    Loop
                Catch e As Exception
                    e = New Exception(line, e)
                    Throw e
                End Try
            End Using

            Return input
        End Function

        Public Function convertToDouble(strings As IEnumerable(Of String)) As List(Of Double)
            Return strings.ToList(AddressOf Val)
        End Function

        Public Function convertDoubles([in] As List(Of Double)) As Double()
            If [in] Is Nothing Then Return New Double() {}
            Return [in].ToArray
        End Function

        Public Function convertDoublesMatrix([in] As IEnumerable(Of List(Of Double))) As Double()()
            If [in].IsNullOrEmpty OrElse [in](0).Count = 0 Then
                Return New Double()() {}
            End If
            Dim doubles As Double()() = MAT(Of Double)([in].Count, [in](0).Count)
            For i As Integer = 0 To [in].Count - 1
                doubles(i) = convertDoubles([in](i))
            Next i
            Return doubles
        End Function
    End Module
End Namespace