#Region "Microsoft.VisualBasic::dba58e5313b88270744f1d2e9973b89d, Data_science\Visualization\Visualization\Tabular\Extensions.vb"

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

    '   Total Lines: 63
    '    Code Lines: 49 (77.78%)
    ' Comment Lines: 6 (9.52%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 8 (12.70%)
    '     File Size: 2.28 KB


    '     Module Extensions
    ' 
    '         Function: RemovesYOutlier, ScatterSerials
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

Namespace TabularRender

    Public Module Extensions

        <Extension>
        Public Function ScatterSerials(csv As File, fieldX$, fieldY$, color$, Optional ptSize! = 5) As Plots.Series
            With DataFrameResolver.CreateObject(csv)
                Dim index As (X%, y%) = (.GetOrdinal(fieldX), .GetOrdinal(fieldY))
                Dim columns = .GetColumnVectors.ToArray
                Dim X = columns(index.X)
                Dim Y = columns(index.y)
                Dim n = System.Math.Min(X.Length, Y.Length)
                Dim xs = New Double(n - 1) {}
                Dim ys = New Double(n - 1) {}

                For i As Integer = 0 To n - 1
                    xs(i) = X(i)
                    ys(i) = Y(i)
                Next

                Return New Plots.Series With {
                    .Color = color.TranslateColor(throwEx:=False),
                    .PointSize = ptSize,
                    .Name = $"Plot({fieldX}, {fieldY})",
                    .X = xs,
                    .Y = ys
                }
            End With
        End Function

        ''' <param name="s"></param>
        ''' <param name="q">默认值为1，表示不会移除任何值</param>
        ''' <returns></returns>
        ''' <summary>
        ''' 按给定的分位数裁掉 Y 方向上的离群点
        ''' </summary>
        <Extension>
        Public Function RemovesYOutlier(s As Plots.Series, Optional q# = 1) As Plots.Series
            If q = 1.0R Then
                Return s
            End If

            With s.Y.GKQuantile
                Dim threshold As Double = .Query(quantile:=q)
                Dim keep = Enumerable _
                    .Range(0, s.Y.Length) _
                    .Where(Function(i) s.Y(i) <= threshold) _
                    .ToArray
                Dim hasError = s.ErrorMinus IsNot Nothing AndAlso
                               s.ErrorPlus IsNot Nothing AndAlso
                               s.ErrorMinus.Length = s.Y.Length AndAlso
                               s.ErrorPlus.Length = s.Y.Length

                s.X = keep.Select(Function(i) s.X(i)).ToArray
                s.Y = keep.Select(Function(i) s.Y(i)).ToArray

                If hasError Then
                    s.ErrorMinus = keep.Select(Function(i) s.ErrorMinus(i)).ToArray
                    s.ErrorPlus = keep.Select(Function(i) s.ErrorPlus(i)).ToArray
                End If

                Return s
            End With
        End Function
    End Module
End Namespace
