#Region "Microsoft.VisualBasic::baddeafd1f299b16fbd2c1832a93ba90, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\MultivariateAnalysisResult.vb"

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

    '   Total Lines: 49
    '    Code Lines: 32 (65.31%)
    ' Comment Lines: 8 (16.33%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 9 (18.37%)
    '     File Size: 2.14 KB


    ' Class MultivariateAnalysisResult
    ' 
    '     Properties: analysis, Coefficients, Contributions, CPreds, FilteredXArray
    '                 NFold, OptimizedFactor, OptimizedOrthoFactor, PoPreds, PPredCoeffs
    '                 PPredCovs, PPreds, PredictedYs, Presses, Q2Cums
    '                 Q2Values, Rmsee, SsCVs, SsPreds, StatisticsObject
    '                 StdevFilteredXs, stdevT, ToPreds, Totals, TPreds
    '                 UPreds, Vips, WoPreds, WPreds
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel

Public Class MultivariateAnalysisResult

    ''' <summary>
    ''' model set
    ''' </summary>
    ''' <returns></returns>
    Public Property StatisticsObject As StatisticsObject
    Public Property analysis As Type

    ' cv result
    Public Property NFold As Integer = 7
    Public Property OptimizedFactor As Integer = 0
    Public Property OptimizedOrthoFactor As Integer = 0
    Public Property SsCVs As New ObservableCollection(Of Double)()
    Public Property Presses As New ObservableCollection(Of Double)()
    Public Property Totals As New ObservableCollection(Of Double)()
    Public Property Q2Values As New ObservableCollection(Of Double)()
    Public Property Q2Cums As New ObservableCollection(Of Double)()


    ' modeled set
    Public Property SsPreds As New ObservableCollection(Of Double)()
    Public Property CPreds As New ObservableCollection(Of Double)()
    Public Property UPreds As New ObservableCollection(Of Double())()
    Public Property TPreds As New ObservableCollection(Of Double())()
    Public Property WPreds As New ObservableCollection(Of Double())()
    Public Property PPreds As New ObservableCollection(Of Double())()

    Public Property Coefficients As New ObservableCollection(Of Double)()
    Public Property Vips As New ObservableCollection(Of Double)()
    Public Property PredictedYs As New ObservableCollection(Of Double)()
    Public Property Rmsee As Double = 0.0

    ' opls
    Public Property ToPreds As New ObservableCollection(Of Double())()
    Public Property WoPreds As New ObservableCollection(Of Double())()
    Public Property PoPreds As New ObservableCollection(Of Double())()
    Public Property stdevT As Double = 0.0
    Public Property StdevFilteredXs As New ObservableCollection(Of Double)()
    Public Property FilteredXArray As Double(,)
    Public Property PPredCovs As New ObservableCollection(Of Double())()
    Public Property PPredCoeffs As New ObservableCollection(Of Double())()

    ' pca
    Public Property Contributions As New ObservableCollection(Of Double)()

End Class
