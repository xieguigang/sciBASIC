#Region "Microsoft.VisualBasic::cbb536d9f835d795193fda151e6b80ca, Data_science\Mathematica\Math\DataFittings\Linear\DoubleLinear.vb"

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

    ' Delegate Function
    ' 
    ' 
    ' Module DoubleLinear
    ' 
    '     Function: AutoPointDeletion, doFilterInternal, GetInputPoints
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Delegate Function Weights(x As Vector) As Vector

Public Module DoubleLinear

    <Extension>
    Public Function GetInputPoints(bestfit As IFitted) As PointF()
        Return bestfit.ErrorTest _
            .Select(Function(p) New PointF(DirectCast(p, TestPoint).X, p.Y)) _
            .ToArray
    End Function

    <Extension>
    Private Function doFilterInternal(pointVec As PointF(), ByRef removed As List(Of PointF), removesZeroY As Boolean) As PointF()
        Dim filter As PointF()

        If removed Is Nothing Then
            removed = New List(Of PointF)
        End If

        ' removes NaN, non-real
        filter = pointVec _
            .Where(Function(p)
                       Return p.X.IsNaNImaginary OrElse p.Y.IsNaNImaginary
                   End Function) _
            .ToArray
        removed.AddRange(filter)
        pointVec = pointVec _
            .Where(Function(p)
                       Return Not (p.X.IsNaNImaginary OrElse p.Y.IsNaNImaginary)
                   End Function) _
            .ToArray

        If removesZeroY Then
            filter = pointVec.Where(Function(p) Not p.Y > 0).ToArray
            removed.AddRange(filter)
            pointVec = pointVec.Where(Function(p) p.Y >= 0).ToArray
        End If

        Return pointVec
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="weighted"></param>
    ''' <param name="max">
    ''' Max number of the reference points that delete automatically by 
    ''' the linear modelling program.
    ''' 
    ''' + negative value means auto
    ''' + zero means no deletion
    ''' + positive means the max allowed point numbers for auto deletion by the program
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AutoPointDeletion(points As IEnumerable(Of PointF),
                                      Optional weighted As Weights = Nothing,
                                      Optional max As Integer = -1,
                                      Optional ByRef removed As List(Of PointF) = Nothing,
                                      Optional keepsLowestPoint As Boolean = False,
                                      Optional removesZeroY As Boolean = False) As IFitted

        Dim pointVec As PointF() = points _
            .OrderBy(Function(p) p.X) _
            .ToArray _
            .doFilterInternal(removed, removesZeroY)

        If pointVec.Length = 0 Then
            Return Nothing
        End If
        If max < 0 Then
            ' auto
            max = pointVec.Length / 2 - 1
        End If
        If max <= 0 Then
            ' 1. user specific no deletions
            ' 2. or can not delete any more points
            Return pointVec.LinearRegression(weighted)
        End If

        ' evaluate R2 for each point removes
        Dim measure As Vector = pointVec.X
        Dim ref As Vector = pointVec.Y
        Dim R2 As Double = -9999
        Dim bestfit As IFitted
        Dim model As IFitted

        model = LinearRegression(measure, ref, weighted)
        bestfit = model

        If Not model Is Nothing AndAlso model.R2 > R2 Then
            R2 = model.R2
            bestfit = model

            If R2 > 0.999 Then
                Return bestfit
            End If
        End If

        ' try to removes at least one point
        ' and the auto deletion is limited 
        ' to the max point
        For p As Integer = 1 To max
            ' 循环删除一个点，取R2最大的
            Dim X, Y As Vector
            Dim RMax As Double = -9999
            Dim modelBest As IFitted = Nothing
            Dim bestX As Vector = Nothing
            Dim bestY As Vector = Nothing
            Dim invalidIndex As Integer = -999

            ' if keeps the lowest point, then we start from the third point
            ' else we start from the first lowest point
            For i As Integer = If(keepsLowestPoint, 1, 0) To measure.Length - 1
                X = measure.Delete({i})
                Y = ref.Delete({i})
                model = LinearRegression(X, Y, weighted)

                If Not model Is Nothing AndAlso model.R2 > RMax Then
                    RMax = model.R2
                    modelBest = model
                    bestX = X
                    bestY = Y
                    invalidIndex = i
                End If
            Next

            If RMax > R2 Then
                If invalidIndex >= 0 Then
                    removed += New PointF With {
                        .X = measure(invalidIndex),
                        .Y = ref(invalidIndex)
                    }
                End If

                R2 = RMax
                bestfit = modelBest
                measure = bestX
                ref = bestY

                If R2 > 0.99 Then
                    Return bestfit
                End If
            End If
        Next

        Return bestfit
    End Function
End Module
