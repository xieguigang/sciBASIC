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
