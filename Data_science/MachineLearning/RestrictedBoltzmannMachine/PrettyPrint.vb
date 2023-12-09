Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace utils

    ''' <summary>
    ''' Created by kenny on 5/13/14.
    ''' </summary>
    Public Class PrettyPrint

        Private Sub New()
        End Sub

        Public Overloads Shared Function ToString(array As Object()) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            stringBuilder.Append("[")
            For i = 0 To array.Length - 1
                stringBuilder.Append(array(i))
                If i < array.Length - 1 Then
                    stringBuilder.Append(ChrW(10))
                End If
            Next
            stringBuilder.Append("]")
            Return stringBuilder.ToString()
        End Function

        Public Overloads Shared Function ToString(arrays As Double()()) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            stringBuilder.AppendLine("[")
            For i = 0 To arrays.Length - 1
                stringBuilder.Append("   " & arrays(i).GetJson())
                stringBuilder.Append(ChrW(10))
            Next
            stringBuilder.Append("]")
            Return stringBuilder.ToString()
        End Function

        Public Shared Function toPixelBox(arrays As Double()(), threshold As Double) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            For Each array In arrays
                For i = 0 To array.Length - 1
                    If array(i) >= threshold Then
                        stringBuilder.Append("■")
                    Else
                        stringBuilder.Append("□")
                    End If
                Next
                stringBuilder.Append(ChrW(10))
            Next
            Return stringBuilder.ToString()
        End Function

        Public Shared Function toPixelBox(array As Double(), columnSize As Integer, threshold As Double) As String

            Dim rowSize As Integer = array.Length / columnSize

            Dim matrix = RectangularArray.Matrix(Of Double)(rowSize, columnSize)
            For i = 0 To rowSize - 1
                For j = 0 To columnSize - 1
                    matrix(i)(j) = array(i * columnSize + j)
                Next
            Next
            Return toPixelBox(matrix, threshold)
        End Function
    End Class

End Namespace
