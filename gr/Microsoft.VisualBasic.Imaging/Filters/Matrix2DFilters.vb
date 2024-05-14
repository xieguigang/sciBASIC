#Region "Microsoft.VisualBasic::15cd722c4175754609d1da23676e423e, gr\Microsoft.VisualBasic.Imaging\Filters\Matrix2DFilters.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 131
    '    Code Lines: 108
    ' Comment Lines: 3
    '   Blank Lines: 20
    '     File Size: 4.86 KB


    '     Enum Matrix2DFilters
    ' 
    '         Max, Mean, Median, Min
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module Matrix2DFiltersAlgorithm
    ' 
    '         Function: sf_maxvalFilter, sf_meanFilter, sf_medianFilter, sf_minvalFilter
    ' 
    '         Sub: sf_BubbleSort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Filters

    Public Enum Matrix2DFilters
        Median
        Min
        Max
        Mean
    End Enum

    ''' <summary>
    ''' https://zhuanlan.zhihu.com/p/73421455
    ''' </summary>
    Module Matrix2DFiltersAlgorithm

        Public Function sf_medianFilter(sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim block = New Byte(8) {}

            For y As Integer = 1 To mH - 1 - 1
                For x As Integer = 1 To mW - 1 - 1
                    block(0) = sMat(y - 1, x + 1)
                    block(1) = sMat(y - 1, x)
                    block(2) = sMat(y - 1, x - 1)
                    block(3) = sMat(y, x + 1)
                    block(4) = sMat(y, x)
                    block(5) = sMat(y, x - 1)
                    block(6) = sMat(y + 1, x + 1)
                    block(7) = sMat(y + 1, x)
                    block(8) = sMat(y + 1, x - 1)
                    sf_BubbleSort(block)
                    Mat(y, x) = block(4)
                Next
            Next

            Return Mat
        End Function

        Public Sub sf_BubbleSort(ByRef Mat As Byte())
            Dim len = Mat.GetLength(0)
            Dim temp As Byte = 0

            For i = 0 To len - 1 - 1

                For j = i + 1 To len - 1

                    If Mat(i) > Mat(j) Then
                        temp = Mat(i)
                        Mat(i) = Mat(j)
                        Mat(j) = temp
                    End If
                Next
            Next
        End Sub

        Public Function sf_minvalFilter(sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp As Byte = 0

            For y As Integer = 1 To mH - 1 - 1
                For X As Integer = 1 To mW - 1 - 1
                    temp = sMat(y, X)
                    temp = If(temp < sMat(y + 1, X), temp, sMat(y + 1, X))
                    temp = If(temp < sMat(y - 1, X), temp, sMat(y - 1, X))
                    temp = If(temp < sMat(y, X - 1), temp, sMat(y, X - 1))
                    temp = If(temp < sMat(y + 1, X - 1), temp, sMat(y + 1, X - 1))
                    temp = If(temp < sMat(y - 1, X - 1), temp, sMat(y - 1, X - 1))
                    temp = If(temp < sMat(y, X + 1), temp, sMat(y, X + 1))
                    temp = If(temp < sMat(y + 1, X + 1), temp, sMat(y + 1, X + 1))
                    temp = If(temp < sMat(y - 1, X + 1), temp, sMat(y - 1, X + 1))
                    Mat(y, X) = temp
                Next
            Next

            Return Mat
        End Function

        Public Function sf_maxvalFilter(sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp As Byte = 0

            For y As Integer = 1 To mH - 1 - 1

                For X As Integer = 1 To mW - 1 - 1
                    temp = sMat(y, X)
                    temp = If(temp > sMat(y + 1, X), temp, sMat(y + 1, X))
                    temp = If(temp > sMat(y - 1, X), temp, sMat(y - 1, X))
                    temp = If(temp > sMat(y, X - 1), temp, sMat(y, X - 1))
                    temp = If(temp > sMat(y + 1, X - 1), temp, sMat(y + 1, X - 1))
                    temp = If(temp > sMat(y - 1, X - 1), temp, sMat(y - 1, X - 1))
                    temp = If(temp > sMat(y, X + 1), temp, sMat(y, X + 1))
                    temp = If(temp > sMat(y + 1, X + 1), temp, sMat(y + 1, X + 1))
                    temp = If(temp > sMat(y - 1, X + 1), temp, sMat(y - 1, X + 1))
                    Mat(y, X) = temp
                Next
            Next

            Return Mat
        End Function

        Public Function sf_meanFilter(sMat As Byte(,)) As Byte(,)
            Dim mH = sMat.GetLength(0)
            Dim mW = sMat.GetLength(1)
            Dim Mat = New Byte(mH - 1, mW - 1) {}
            Dim temp = 0

            For y As integer= 1 To mH - 1 - 1

                For x As Integer = 1 To mW - 1 - 1
                    temp = sMat(y - 1, x + 1)
                    temp += sMat(y - 1, x)
                    temp += sMat(y - 1, x - 1)
                    temp += sMat(y, x + 1)
                    temp += sMat(y, x)
                    temp += sMat(y, x - 1)
                    temp += sMat(y + 1, x + 1)
                    temp += sMat(y + 1, x)
                    temp += sMat(y + 1, x - 1)
                    Mat(y, x) = CByte(temp / 9)
                Next
            Next

            Return Mat
        End Function
    End Module
End Namespace
