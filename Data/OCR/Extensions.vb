Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    ReadOnly blank As DefaultValue(Of Color) = Color.White

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="image">Should be black and white</param>
    ''' <returns></returns>
    <Extension> Public Function ToVector(image As Image, Optional size As Size = Nothing, Optional background As Color = Nothing) As Vector
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            If size.IsEmpty Then
                Return bitmap.fullScan(background Or blank)
            Else
                Return bitmap.regionScan(background Or blank, size)
            End If
        End Using
    End Function

    <Extension>
    Private Function regionScan(bitmap As BitmapBuffer, blank As Color, size As Size) As Vector
        Dim vector As New List(Of Double)

        For top As Integer = 0 To bitmap.Height - 1 - size.Height
            For left As Integer = 0 To bitmap.Width - 1 - size.Width

                For y As Integer = top To size.Height - 1
                    For x As Integer = left To size.Width - 1
                        Dim pixel = bitmap.GetPixel(x, y)

                        If GDIColors.Equals(pixel, blank) Then
                            Call vector.Add(0)
                        Else
                            Call vector.Add(1)
                        End If
                    Next

                    ' Call vector.Add(-1)
                Next

            Next
        Next

        Return vector.AsVector
    End Function

    <Extension>
    Private Function fullScan(bitmap As BitmapBuffer, blank As Color) As Vector
        Dim vector As New List(Of Double)

        ' 逐行扫描
        For y As Integer = 0 To bitmap.Height - 1
            For x As Integer = 0 To bitmap.Width - 1
                Dim pixel = bitmap.GetPixel(x, y)

                If GDIColors.Equals(pixel, blank) Then
                    Call vector.Add(0)
                Else
                    Call vector.Add(1)
                End If
            Next

            ' Call vector.Add(-1)
        Next

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
        Dim size As Size = obj.Size
        Dim query As Vector = obj.ToVector
        Dim subject As Vector = view.ToVector(size)
        Dim equals As ISimilarity(Of Double) =
            Function(a, b)
                If a = b Then
                    Return 1
                Else
                    Return 0
                End If
            End Function
        Dim local As New GSW(Of Double)(subject, query, equals, AddressOf asChar)
        Dim objects = local.GetMatches(local.MaxScore * cutoff)
        Dim viewSize = view.Size

        For Each region As Match In objects
            Yield region.FromA.TranslateRegion(size, viewSize)
        Next
    End Function

    <Extension>
    Public Function TranslateRegion(left%, regionSize As Size, size As Size) As Rectangle
        Dim width = (size.Width - regionSize.Width)
        Dim x = left Mod width
        Dim y = Fix(left / width)

        Return New Rectangle With {
            .X = x,
            .Y = y,
            .Size = regionSize
        }
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
