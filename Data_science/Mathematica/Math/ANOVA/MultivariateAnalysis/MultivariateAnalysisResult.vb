Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Public Class MultivariateAnalysisResult

    ''' <summary>
    ''' model set
    ''' </summary>
    ''' <returns></returns>
    Public Property StatisticsObject As StatisticsObject

    ' cv result
    Public Property NFold As Integer = 7
    Public Property OptimizedFactor As Integer = 0
    Public Property OptimizedOrthoFactor As Integer = 0
    Public Property SsCVs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Presses As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Totals As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Q2Values As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Q2Cums As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()


    ' modeled set
    Public Property SsPreds As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property CPreds As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property UPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property TPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property WPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()

    Public Property Coefficients As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Vips As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property PredictedYs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property Rmsee As Double = 0.0

    ' opls
    Public Property ToPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property WoPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PoPreds As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property stdevT As Double = 0.0
    Public Property StdevFilteredXs As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
    Public Property FilteredXArray As Double(,)
    Public Property PPredCovs As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()
    Public Property PPredCoeffs As ObservableCollection(Of Double()) = New ObservableCollection(Of Double())()

    ' pca
    Public Property Contributions As ObservableCollection(Of Double) = New ObservableCollection(Of Double)()
End Class
