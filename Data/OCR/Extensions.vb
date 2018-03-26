Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="image">Should be black and white</param>
    ''' <returns></returns>
    <Extension> Public Function ToVector(image As Image) As Vector
        Dim vector As New List(Of Double)

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            For x As Integer = 0 To bitmap.Width - 1
                For y As Integer = 0 To bitmap.Height - 1
                    Dim pixel = bitmap.GetPixel(x, y)

                    If GDIColors.Equals(pixel, Color.White) Then
                        vector.Add(0)
                    Else
                        vector.Add(1)
                    End If
                Next

                vector.Add(-1)
            Next
        End Using

        Return vector.AsVector
    End Function

    ''' <summary>
    ''' Get all of the target match <paramref name="obj"/> theirs top left locations using dynamics programming.
    ''' </summary>
    ''' <param name="view"></param>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Iterator Function FindObjects(view As Image, obj As Image, Optional cutoff# = 0.95) As IEnumerable(Of Rectangle)
        Dim query = obj.ToVector
        Dim subject = view.ToVector
        Dim equals As ISimilarity(Of Double) =
            Function(a, b)
                If a = b Then
                    Return 1
                Else
                    Return 0
                End If
            End Function
        Dim local As New GSW(Of Double)(query, subject, equals, AddressOf asChar)
        Dim objects = local.GetMatches(local.MaxScore * cutoff)

        For Each region As Match In objects
            If (region.ToA - region.FromA) / query.Length >= 0.9 Then
                Dim left = region.FromB
                Dim length = region.ToB - left

            End If
        Next
    End Function

    Private Function asChar(d As Double) As Char
        If d = 0R OrElse d = 1.0R Then
            Return d.ToString.First
        ElseIf d = -1.0R Then
            Return "*"c
        Else
            Return "7"c
        End If
    End Function
End Module
