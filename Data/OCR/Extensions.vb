Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    ReadOnly blank As DefaultValue(Of Color) = Color.White

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="image">Should be black and white</param>
    ''' <returns></returns>
    <Extension> Public Iterator Function ToVector(image As Image, Optional size As Size = Nothing, Optional background As Color = Nothing) As IEnumerable(Of Map(Of Point, Vector))
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            If size.IsEmpty Then
                Yield New Map(Of Point, Vector)(
                    Nothing,
                    bitmap.FullScan(background Or blank)
                )
            Else
                For Each x In bitmap.RegionScan(background Or blank, size)
                    Yield x
                Next
            End If
        End Using
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
        Dim query As Vector = obj.ToVector.First
        Dim area = query.Length
        Dim equals As ISimilarity(Of Double) =
            Function(a, b)
                If a = b Then
                    Return 1
                ElseIf a = -1.0R OrElse b = -1.0R Then
                    Return -1
                Else
                    Return 0
                End If
            End Function

        For Each block In view.ToVector(size)
            Dim subject As Vector = block.Maps
            Dim local As New GSW(Of Double)(query, subject, equals, AddressOf asChar)
            Dim match As Match = local.GetMatches(local.MaxScore * cutoff).FirstOrDefault

            If Not match Is Nothing AndAlso (match.ToA - match.FromA) / area >= cutoff Then
                Yield New Rectangle(block.Key, size)
            End If
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
