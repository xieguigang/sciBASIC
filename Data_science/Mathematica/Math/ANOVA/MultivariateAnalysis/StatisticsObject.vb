#Region "Microsoft.VisualBasic::93809bea614973964a3b1a4a4cd4f663, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\StatisticsObject.vb"

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

    '   Total Lines: 193
    '    Code Lines: 124 (64.25%)
    ' Comment Lines: 36 (18.65%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 33 (17.10%)
    '     File Size: 6.78 KB


    ' Enum ScaleMethod
    ' 
    '     AutoScale, MeanCenter, None, ParetoScale
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum TransformMethod
    ' 
    '     Log10, None, QuadRoot
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class BasicStats
    ' 
    '     Properties: Average, ID, Legend, MaxValue, Median
    '                 MinValue, SeventyFiveValue, Stdev, TwentyFiveValue
    ' 
    ' Class StatisticsObject
    ' 
    '     Properties: decoder, Scale, Transform, XColors, XDataMatrix
    '                 XIndexes, XLabels, XMeans, XScaled, XStdevs
    '                 XTransformed, YColors, YIndexes, YLabels, YLabels2
    '                 YMean, YScaled, YStdev, YTransformed, YVariables
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: ColumnSize, CopyX, CopyY, RowSize, YBackTransform
    ' 
    '     Sub: StatInitialization
    ' 
    ' /********************************************************************************/

#End Region

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

#Region "y - class labels"
    Public Property decoder As Dictionary(Of String, Integer)
#End Region

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

    Public Property YIndexes As New ObservableCollection(Of Integer)()
    Public Property XIndexes As New ObservableCollection(Of Integer)()

    Public Property YLabels As New ObservableCollection(Of String)()
    Public Property YLabels2 As New ObservableCollection(Of String)()
    Public Property XLabels As New ObservableCollection(Of String)()

    Public Property YColors As New ObservableCollection(Of Byte())() ' [0] R [1] G [2] B [3] A
    Public Property XColors As New ObservableCollection(Of Byte())() ' [0] R [1] G [2] B [3] A

    Public Property Scale As ScaleMethod = ScaleMethod.AutoScale
    Public Property Transform As TransformMethod = TransformMethod.None

    Sub New(x As Double()(), y As Double())
        XDataMatrix = x.ToMatrix
        YVariables = y

        StatInitialization()
    End Sub

    Sub New()
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

        Dim xMeans As Double() = Nothing
        Dim xStdevs As Double() = Nothing

        Call StatisticsMathematics.StatisticsProperties(XTransformed, xMeans, xStdevs)

        Me.XMeans = xMeans
        Me.XStdevs = xStdevs

        Dim yMean, yStdev As Double

        If Not YTransformed.IsNullOrEmpty Then
            Call StatisticsMathematics.StatisticsProperties(YTransformed, yMean, yStdev)
        End If

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
