#Region "Microsoft.VisualBasic::c99f394c67211c1be6ae0eff0094d23e, sciBASIC#\Data\OCR\Extensions.vb"

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

    '   Total Lines: 95
    '    Code Lines: 70
    ' Comment Lines: 14
    '   Blank Lines: 11
    '     File Size: 3.33 KB


    ' Module Extensions
    ' 
    '     Function: AsChar, Equals, FindObjects, ToVector, TranslateRegion
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    Friend ReadOnly blank As [Default](Of Color) = Color.White

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="image">Should be black and white</param>
    ''' <returns></returns>
    <Extension> Public Iterator Function ToVector(image As Image,
                                                  Optional size As Size = Nothing,
                                                  Optional background As Color = Nothing,
                                                  Optional fillDeli As Boolean = False) As IEnumerable(Of Map(Of Point, Vector))

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            If size.IsEmpty Then
                Yield New Map(Of Point, Vector)(
                    Nothing,
                    bitmap.FullScan(background Or blank, fillDeli)
                )
            Else
                For Each X As Map(Of Point, Vector) In bitmap.RegionScan(background Or blank, size, fillDeli)
                    Yield X
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

        For Each block In view.ToVector(size)
            Dim subject As Vector = block.Maps
            ' Dim local As New GSW(Of Double)(query, subject, AddressOf Equals, AddressOf AsChar)
            ' Dim match As Match = local.GetMatches(local.MaxScore * cutoff).FirstOrDefault
            Dim score = SSM(query, subject)

            If score >= cutoff Then
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

    Public Function Equals(a#, b#) As Double
        If a = b Then
            Return 1
        ElseIf a = -1.0R OrElse b = -1.0R Then
            Return -1
        Else
            Return 0
        End If
    End Function

    Public Function AsChar(d As Double) As Char
        If d = 0R OrElse d = 1.0R Then
            Return d.ToString.First
        ElseIf d = -1.0R Then
            Return "*"c
        Else
            Return "7"c
        End If
    End Function
End Module
