Imports System.Runtime.InteropServices
Imports std = System.Math

Public Class StatisticsMathematics

    Public Shared Function LogTransform(dataArray As Double(,)) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                If dataArray(i, j) > 0 Then
                    resultArray(i, j) = std.Log10(dataArray(i, j))
                Else
                    resultArray(i, j) = 0
                End If
            Next
        Next
        Return resultArray
    End Function

    Public Shared Function QuadRootTransform(dataArray As Double(,)) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                If dataArray(i, j) > 0 Then
                    resultArray(i, j) = std.Sqrt(std.Sqrt(dataArray(i, j)))
                Else
                    resultArray(i, j) = 0
                End If
            Next
        Next
        Return resultArray
    End Function

    Public Shared Function MeanCentering(dataArray As Double(,), mean As Double()) As Double(,)

        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}

        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                resultArray(j, i) = dataArray(j, i) - mean(i)
            Next
        Next

        Return resultArray
    End Function

    Public Shared Function ParetoScaling(dataArray As Double(,), mean As Double(), stdev As Double()) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                If stdev(i) <> 0 Then
                    resultArray(j, i) = (dataArray(j, i) - mean(i)) / std.Sqrt(stdev(i))
                Else
                    resultArray(j, i) = dataArray(j, i) - mean(i)
                End If
            Next
        Next

        Return resultArray
    End Function

    Public Shared Function AutoScaling(dataArray As Double(,), mean As Double(), stdev As Double()) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                If stdev(i) <> 0 Then
                    resultArray(j, i) = (dataArray(j, i) - mean(i)) / stdev(i)
                Else
                    resultArray(j, i) = dataArray(j, i) - mean(i)
                End If

            Next
        Next

        Return resultArray
    End Function

    Public Shared Sub StatisticsProperties(dataArray As Double(,), <Out> ByRef mean As Double(), <Out> ByRef stdev As Double())

        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        mean = New Double(columnSize - 1) {}
        stdev = New Double(columnSize - 1) {}
        Dim sum As Double

        For i = 0 To columnSize - 1
            sum = 0
            For j = 0 To rowSize - 1
                sum += dataArray(j, i)
            Next
            mean(i) = sum / rowSize
        Next

        For i = 0 To columnSize - 1
            sum = 0
            For j = 0 To rowSize - 1
                sum += std.Pow(dataArray(j, i) - mean(i), 2)
            Next
            stdev(i) = std.Sqrt(sum / (rowSize - 1))
        Next
    End Sub

    Public Shared Sub StatisticsProperties(dataArray As Double(), <Out> ByRef mean As Double, <Out> ByRef stdev As Double)
        Dim size = dataArray.Length

        mean = 0.0
        stdev = 0.0
        Dim sum = 0.0
        For i = 0 To size - 1
            sum += dataArray(i)
        Next
        mean = sum / size

        sum = 0.0
        For i = 0 To size - 1
            sum += std.Pow(dataArray(i) - mean, 2)
        Next
        stdev = std.Sqrt(sum / (size - 1))
    End Sub
End Class
