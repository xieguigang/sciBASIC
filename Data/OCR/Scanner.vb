#Region "Microsoft.VisualBasic::c6eae44c478aead86e5b512150917a26, sciBASIC#\Data\OCR\Scanner.vb"

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

    '   Total Lines: 100
    '    Code Lines: 81
    ' Comment Lines: 2
    '   Blank Lines: 17
    '     File Size: 3.47 KB


    ' Module Scanner
    ' 
    '     Function: BackgroundMatch, DrawRegion, FullScan, RegionScan
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Scanner

    <Extension> Public Function FullScan(bitmap As BitmapBuffer, blank As Color, Optional fillDeli As Boolean = False) As Vector
        Dim vector As New List(Of Double)
        Dim pixelScan As Func(Of Color, Double) = BackgroundMatch(blank)

        ' 逐行扫描
        For y As Integer = 0 To bitmap.Height - 1
            For x As Integer = 0 To bitmap.Width - 1
                Dim pixel = bitmap.GetPixel(x, y)
                Call vector.Add(pixelScan(pixel))
            Next

            If fillDeli Then
                Call vector.Add(-1)
            End If
        Next

        Return vector.AsVector
    End Function

    Public Function BackgroundMatch(blank As Color) As Func(Of Color, Double)
        Return Function(pixel)
                   If GDIColors.Equals(pixel, blank) Then
                       Return 0
                   Else
                       Return 1
                   End If
               End Function
    End Function

    <Extension> Public Iterator Function RegionScan(bitmap As BitmapBuffer, blank As Color, size As Size, Optional fillDeli As Boolean = False) As IEnumerable(Of Map(Of Point, Vector))
        Dim pixelScan As Func(Of Color, Double) = BackgroundMatch(blank)

        For top As Integer = 0 To bitmap.Height - 1 - size.Height
            For left As Integer = 0 To bitmap.Width - 1 - size.Width
                Dim vector As New List(Of Double)

                For y As Integer = top To (size.Height + top) - 1

                    For x As Integer = left To (size.Width + left) - 1
                        Dim pixel = bitmap.GetPixel(x, y)
                        Call vector.Add(pixelScan(pixel))
                    Next

                    If fillDeli Then
                        Call vector.Add(-1)
                    End If
                Next

#If DEBUG Then
                If GDIColors.Equals(bitmap.GetPixel(left, top), blank) Then
                    Console.Write(" "c)
                Else
                    Console.Write("*"c)
                End If
#End If

                Yield New Map(Of Point, Vector)(
                    New Point(left, top),
                    vector.AsVector
                )
            Next

#If DEBUG Then
            Console.WriteLine()
#End If
        Next
    End Function

    <Extension>
    Public Function DrawRegion(pixels As Vector, size As Size) As Image
        Using g = size.CreateGDIDevice
            Dim i As VBInteger = 0

            For y As Integer = 0 To size.Height - 1
                For x As Integer = 0 To size.Width - 1
                    Select Case pixels(++i)
                        Case 0R
                            ' white
                        Case 1.0R
                            Call g.DrawRectangle(Pens.Black, New Rectangle(x, y, 1, 1))
                        Case -1.0R
                            Call g.DrawRectangle(Pens.Red, New Rectangle(x, y, 1, 1))
                    End Select
                Next
            Next

            Return g.ImageResource
        End Using
    End Function
End Module
