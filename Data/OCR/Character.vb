
Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Character

    ''' <summary>
    ''' 使用-1分割，按照行进行排列
    ''' </summary>
    Public PixelsVector As Vector

    Sub New()
    End Sub

    Sub New(character As Image)
        Dim vector As New List(Of Double)

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(character)
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
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="test">模板样本数据</param>
    ''' <returns></returns>
    Public Function Compare(test As Character) As Double
        Return SSM(PixelsVector, test.PixelsVector)
    End Function

    Public Overrides Function ToString() As String
        Dim lines = PixelsVector.Split(Function(x) x = -1.0R)
        Dim sb As New StringBuilder

        For Each line As Double() In lines
            For Each c As Double In line
                If c = 0R Then
                    Call sb.Append(" ")
                Else
                    Call sb.Append("$")
                End If
            Next

            Call sb.AppendLine()
        Next

        Return sb.ToString
    End Function
End Class