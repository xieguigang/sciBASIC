Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.ComponentModel.Collection

Public Enum ScaleMethod
    None
    MeanCenter
    ParetoScale
    AutoScale
End Enum

Public Enum TransformMethod
    None
    Log10
    QuadRoot
End Enum

Public Class BasicStats
    Public Property ID As Integer
    Public Property Average As Double
    Public Property Stdev As Double
    Public Property Legend As String
    Public Property MinValue As Double
    Public Property TwentyFiveValue As Double
    Public Property Median As Double
    Public Property SeventyFiveValue As Double
    Public Property MaxValue As Double
End Class
Public Class StatisticsObject

    Public Property YVariables As Double() = Nothing ' files
    Public Property YTransformed As Double() = Nothing
    Public Property YScaled As Double() = Nothing
    Public Property YMean As Double = 0
    Public Property YStdev As Double = 0

    Public Property XDataMatrix As Double(,) = Nothing ' [row] files [column] metabolites
    Public Property XTransformed As Double(,) = Nothing
    Public Property XScaled As Double(,) = Nothing
    Public Property XMeans As Double() = Nothing
    Public Property XStdevs As Double() = Nothing

    Public Property YIndexes As ObservableCollection(Of Integer) = New ObservableCollection(Of Integer)()
    Public Property XIndexes As ObservableCollection(Of Integer) = New ObservableCollection(Of Integer)()

    Public Property YLabels As ObservableCollection(Of String) = New ObservableCollection(Of String)()
    Public Property YLabels2 As ObservableCollection(Of String) = New ObservableCollection(Of String)()
    Public Property XLabels As ObservableCollection(Of String) = New ObservableCollection(Of String)()

    Public Property YColors As ObservableCollection(Of Byte()) = New ObservableCollection(Of Byte())() ' [0] R [1] G [2] B [3] A
    Public Property XColors As ObservableCollection(Of Byte()) = New ObservableCollection(Of Byte())() ' [0] R [1] G [2] B [3] A

    Public Property Scale As ScaleMethod = ScaleMethod.AutoScale
    Public Property Transform As TransformMethod = TransformMethod.None

    Sub New(x As Double()())
        XDataMatrix = x.ToMatrix
    End Sub

    Public Sub StatInitialization()

        Dim rowSize = XDataMatrix.GetLength(0) ' files
        Dim columnSize = XDataMatrix.GetLength(1) ' metabolites

        YTransformed = New Double(rowSize - 1) {}
        YScaled = New Double(rowSize - 1) {}

        XTransformed = New Double(rowSize - 1, columnSize - 1) {}
        XScaled = New Double(rowSize - 1, columnSize - 1) {}

        Select Case Transform
            Case TransformMethod.None
                'this.YTransformed = this.YVariables;
                XTransformed = XDataMatrix
            Case TransformMethod.Log10
                'this.YTransformed = StatisticsMathematics.LogTransform(this.YVariables);
                XTransformed = StatisticsMathematics.LogTransform(XDataMatrix)
            Case TransformMethod.QuadRoot
                'this.YTransformed = StatisticsMathematics.QuadRootTransform(this.YVariables);
                XTransformed = StatisticsMathematics.QuadRootTransform(XDataMatrix)
            Case Else
        End Select
        YTransformed = YVariables

        Dim xMeans, xStdevs As Double()
        StatisticsMathematics.StatisticsProperties(XTransformed, xMeans, xStdevs)
        Me.XMeans = xMeans
        Me.XStdevs = xStdevs

        Dim yMean, yStdev As Double
        StatisticsMathematics.StatisticsProperties(YTransformed, yMean, yStdev)
        Me.YMean = yMean
        Me.YStdev = yStdev

        Select Case Scale
            Case ScaleMethod.None
                'this.YScaled = this.YTransformed;
                XScaled = XTransformed
            Case ScaleMethod.MeanCenter
                'this.YScaled = StatisticsMathematics.MeanCentering(this.YTransformed, this.YMean);
                XScaled = StatisticsMathematics.MeanCentering(XTransformed, Me.XMeans)
            Case ScaleMethod.ParetoScale
                'this.YScaled = StatisticsMathematics.ParetoScaling(this.YTransformed, this.YMean, this.YStdev);
                XScaled = StatisticsMathematics.ParetoScaling(XTransformed, Me.XMeans, Me.XStdevs)
            Case ScaleMethod.AutoScale
                'this.YScaled = StatisticsMathematics.AutoScaling(this.YTransformed, this.YMean, this.YStdev);
                XScaled = StatisticsMathematics.AutoScaling(XTransformed, Me.XMeans, Me.XStdevs)
            Case Else
        End Select
        YScaled = YTransformed

    End Sub

    Public Function YBackTransform(scaledY As Double) As Double
        Return scaledY + YMean
        'var backY = scaledY;
        'switch (this.Scale) {
        '    case ScaleMethod.None:
        '        break;
        '    case ScaleMethod.MeanCenter:
        '        backY = scaledY + this.YMean;
        '        break;
        '    case ScaleMethod.ParetoScale:
        '        backY = scaledY * Math.Sqrt(this.YStdev) + this.YMean;
        '        break;
        '    case ScaleMethod.AutoScale:
        '        backY = scaledY * this.YStdev + this.YMean;
        '        break;
        '    default:
        '        break;
        '}

        'switch (this.Transform) {
        '    case TransformMethod.None:
        '        break;
        '    case TransformMethod.Log10:
        '        backY = Math.Pow(10, backY);
        '        break;
        '    case TransformMethod.QuadRoot:
        '        backY = Math.Pow(backY, 4);
        '        break;
        '    default:
        '        break;
        '}

        'return backY;
    End Function

    Public Function CopyY() As Double()
        Dim size = YScaled.Length
        Dim lCopyY = New Double(size - 1) {}
        For i = 0 To size - 1
            lCopyY(i) = YScaled(i)
        Next
        Return lCopyY
    End Function

    Public Function CopyX() As Double(,)
        Dim rowSize = XDataMatrix.GetLength(0) ' files
        Dim columnSize = XDataMatrix.GetLength(1) ' metabolites
        Dim lCopyX = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                lCopyX(i, j) = XScaled(i, j)
            Next
        Next
        Return lCopyX
    End Function

    Public Function RowSize() As Integer
        Return XDataMatrix.GetLength(0)
    End Function

    Public Function ColumnSize() As Integer
        Return XDataMatrix.GetLength(1)
    End Function
End Class
