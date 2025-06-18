#Region "Microsoft.VisualBasic::c961dcfc3408f7c4bec12c8d7f7d0544, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\PLSData.vb"

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

    '   Total Lines: 115
    '    Code Lines: 89 (77.39%)
    ' Comment Lines: 4 (3.48%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 22 (19.13%)
    '     File Size: 3.71 KB


    ' Module PLSData
    ' 
    '     Function: GetComponents, GetPLSLoading, GetPLSScore
    ' 
    ' Class Component
    ' 
    '     Properties: Order, Press, Q2, Q2cum, SSCV
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' get pls-da result
''' </summary>
Public Module PLSData

    <Extension>
    Public Iterator Function GetComponents(mvar As MultivariateAnalysisResult) As IEnumerable(Of Component)
        For i As Integer = 0 To mvar.Presses.Count - 1
            Yield New Component With {
                .Order = (i + 1),
                .SSCV = mvar.SsCVs(i),
                .Press = mvar.Presses(i),
                .Q2 = mvar.Q2Values(i),
                .Q2cum = mvar.Q2Cums(i)
            }
        Next
    End Function

    <Extension>
    Public Function GetPLSScore(mvar As MultivariateAnalysisResult, opls As Boolean) As DataFrame
        Dim [class] As String() = mvar.StatisticsObject.YLabels.ToArray
        Dim smaple_id As String() = mvar.StatisticsObject.YLabels2.ToArray
        Dim Tscore As New List(Of Double())
        Dim fileSize = mvar.StatisticsObject.YIndexes.Count
        Dim yexp As Double() = mvar.StatisticsObject.YVariables
        Dim ypre As Double() = New Double([class].Length - 1) {}

        For i As Integer = 0 To If(opls, mvar.OptimizedOrthoFactor, mvar.OptimizedFactor) - 1
            Tscore.Add(New Double([class].Length - 1) {})
        Next

        ' scores
        For i As Integer = 0 To fileSize - 1
            If opls Then
                For j = 0 To mvar.ToPreds.Count - 1
                    Tscore(j)(i) = mvar.ToPreds(j)(i)
                Next
            Else
                For j = 0 To mvar.TPreds.Count - 1
                    Tscore(j)(i) = mvar.TPreds(j)(i)
                Next
            End If

            ypre(i) = mvar.PredictedYs(i)
        Next

        Dim df As New DataFrame With {.rownames = smaple_id}
        Dim index As i32 = 1

        For Each t As Double() In Tscore
            Call df.add($"T{++index}", t)
        Next

        Call df.add("class", [class])
        Call df.add("Y experiment", yexp)
        Call df.add("Y predicted", ypre)

        Return df
    End Function

    <Extension>
    Public Function GetPLSLoading(mvar As MultivariateAnalysisResult, opls As Boolean) As DataFrame
        Dim features As String() = mvar.StatisticsObject.XLabels.ToArray
        Dim Ploads As New List(Of Double())
        Dim metSize = mvar.StatisticsObject.XIndexes.Count
        Dim vips As Double() = mvar.Vips.ToArray
        Dim cors As Double() = mvar.Coefficients.ToArray

        For i = 0 To If(opls, mvar.OptimizedOrthoFactor, mvar.OptimizedFactor) - 1
            Call Ploads.Add(New Double(features.Length - 1) {})
        Next

        For i As Integer = 0 To metSize - 1
            If opls Then
                For j As Integer = 0 To mvar.PoPreds.Count - 1
                    Ploads(j)(i) = mvar.PoPreds(j)(i)
                Next
            Else
                For j As Integer = 0 To mvar.PPreds.Count - 1
                    Ploads(j)(i) = mvar.PPreds(j)(i)
                Next
            End If
        Next

        Dim df As New DataFrame With {.rownames = features}
        Dim index As i32 = 1

        For Each p As Double() In Ploads
            Call df.add($"P{++index}", p)
        Next

        Call df.add("VIP", vips)
        Call df.add("Coefficients", cors)

        Return df
    End Function

End Module

Public Class Component

    Public Property Order As Integer
    Public Property SSCV As Double
    Public Property Press As Double
    Public Property Q2 As Double
    Public Property Q2cum As Double

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class
