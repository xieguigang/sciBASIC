#Region "Microsoft.VisualBasic::6056984fdd055973546999149bb67056, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\WaveletTransform.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

''' <summary>
''' 小波变换工具
''' </summary>
''' <remarks>
''' 
''' The first DWT was invented by the Hungarian mathematician Alfréd Haar. For an input represented by a 
''' list of 2n numbers, the Haar wavelet transform may be considered to simply pair up input values, 
''' storing the difference and passing the sum. This process is repeated recursively, pairing up the sums 
''' to provide the next scale: finally resulting in 2n-1 differences and one final sum.
''' 
''' Suppose you are given N values
''' 
''' x = (x1, x2, … xN)
''' 
''' where N is even.(X向量的元素的个数必须是偶数)
''' 
''' We take pair-wise average of numbers
''' 
''' sk = (x2k + x2k+1)/2 for k=0, …, N/2 -1
''' 
''' For example,
''' 
''' x = (6, 12, 15, 15, 14, 12, 120, 116) -> s = (9, 15, 13, 118)
''' 
''' We need second list of data d so that the original list x can be recovered from s and d.
''' 
''' For dk (called directed distances), we have:
''' 
''' dk = (x2k - x2k+1)/2 for k=0, …, N/2 -1
''' 
''' The process is invertible since:
''' 
''' sk + dk = (x2k + x2k+1)/2 + (x2k - x2k+1)/2 = x2k
''' 
''' sk - dk = (x2k + x2k+1)/2 - (x2k - x2k+1)/2 = x2k+1
''' 
''' 
''' So we map x = (x1, x2, … , xN) to (s | d) = (s1, … , sN/2 | d1, … , dN/2).
''' 
''' Using our example values, we have:
''' 
''' (6, 12, 15, 15, 14, 12, 120, 116) -> (9, 15, 13, 118 | -3, 0, 1, 2)
''' 
''' This process is repeated recursively for s:
''' 
''' (9, 15, 13, 118 | -3, 0, 1, 2) -> (12, 65.5 | -3, -52.5 | -3, 0, 1, 2)
''' 
''' (12, 65.5 | -3, -52.5 | -3, 0, 1, 2) -> (38.75 | -26.75 | -3, -52.5 | -3, 0, 1, 2)
''' 
''' So final result is:
''' 
''' (38.75, -26.75, -3, -52.5, -3, 0, 1, 2)
''' 
''' Why might people prefer the data in this form?
''' 
''' We can identify large changes in the differences portion d of the transform.
''' It is easier to quantize the data in this form.
''' The transform concentrates the information (energy) in the signal in fewer values.
''' And the obvious answer: fewer digits!!
''' In case of images, we need 2D FWT. First, we perform 1D FWT for all rows, and next, for all columns. 
''' For color Images, we deal with RGB components of color, and perform Haar Transform for each component 
''' separately. Any component (R G B) has values from 0 to 255 to before transformation we scale this 
''' values. For displaying image after transformation, we scale back transformed values.
''' 
''' </remarks>
Public Module WaveletTransform

    Private Const w0 As Double = 0.5
    Private Const w1 As Double = -0.5
    Private Const s0 As Double = 0.5
    Private Const s1 As Double = 0.5

    ''' <summary>
    '''   Discrete Haar Wavelet Transform
    ''' </summary>
    ''' 
    Public Sub FWT(ByRef data As Double())
        Dim temp As Double() = New Double(data.Length - 1) {}

        Dim h As Integer = data.Length >> 1
        For i As Integer = 0 To h - 1
            Dim k As Integer = (i << 1)
            temp(i) = data(k) * s0 + data(k + 1) * s1
            temp(i + h) = data(k) * w0 + data(k + 1) * w1
        Next

        For i As Integer = 0 To data.Length - 1
            data(i) = temp(i)
        Next
    End Sub

    ''' <summary>
    '''   Discrete Haar Wavelet 2D Transform
    ''' </summary>
    ''' <param name="iterations">Iteration must be Integer from 1 to</param>
    Public Sub FWT(ByRef data As Double(,), iterations As Integer)
        Dim rows As Integer = data.GetLength(0)
        Dim cols As Integer = data.GetLength(1)

        Dim row As Double() = New Double(cols - 1) {}
        Dim col As Double() = New Double(rows - 1) {}

        For k As Integer = 0 To iterations - 1
            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To row.Length - 1
                    row(j) = data(i, j)
                Next

                FWT(row)

                For j As Integer = 0 To row.Length - 1
                    data(i, j) = row(j)
                Next
            Next

            For j As Integer = 0 To cols - 1
                For i As Integer = 0 To col.Length - 1
                    col(i) = data(i, j)
                Next

                FWT(col)

                For i As Integer = 0 To col.Length - 1
                    data(i, j) = col(i)
                Next
            Next
        Next
    End Sub

    ''' <summary>
    '''   Inverse Haar Wavelet Transform
    ''' </summary>
    ''' 
    Public Sub IWT(ByRef data As Double())
        Dim temp As Double() = New Double(data.Length - 1) {}

        Dim h As Integer = data.Length >> 1
        For i As Integer = 0 To h - 1
            Dim k As Integer = (i << 1)
            temp(k) = (data(i) * s0 + data(i + h) * w0) / w0
            temp(k + 1) = (data(i) * s1 + data(i + h) * w1) / s0
        Next

        For i As Integer = 0 To data.Length - 1
            data(i) = temp(i)
        Next
    End Sub

    ''' <summary>
    '''   Inverse Haar Wavelet 2D Transform
    ''' </summary>
    ''' <param name="iterations">Iteration must be Integer from 1 to</param>
    Public Sub IWT(ByRef data As Double(,), iterations As Integer)
        Dim rows As Integer = data.GetLength(0)
        Dim cols As Integer = data.GetLength(1)

        Dim col As Double() = New Double(rows - 1) {}
        Dim row As Double() = New Double(cols - 1) {}

        For l As Integer = 0 To iterations - 1
            For j As Integer = 0 To cols - 1
                For i As Integer = 0 To row.Length - 1
                    col(i) = data(i, j)
                Next

                IWT(col)

                For i As Integer = 0 To col.Length - 1
                    data(i, j) = col(i)
                Next
            Next

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To row.Length - 1
                    row(j) = data(i, j)
                Next

                IWT(row)

                For j As Integer = 0 To row.Length - 1
                    data(i, j) = row(j)
                Next
            Next
        Next
    End Sub

End Module
