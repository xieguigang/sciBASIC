#Region "Microsoft.VisualBasic::3eb8c8077260d3769dd80dce1e4eb1bd, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\Models\Model.vb"

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

    '   Total Lines: 121
    '    Code Lines: 41 (33.88%)
    ' Comment Lines: 65 (53.72%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (12.40%)
    '     File Size: 4.52 KB


    '     Class RangeTransformModel
    ' 
    '         Properties: inputScale, inputStart, length, outputScale, outputStart
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetTransform, ToString
    ' 
    '     Class GaussianTransformModel
    ' 
    '         Properties: means, stddevs
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetTransform, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM.StorageProcedure

    ''' <summary>
    ''' The JSON serializable data model of a <see cref="RangeTransform"/> object.
    ''' </summary>
    Public Class RangeTransformModel

        ''' <summary>
        ''' The minimum value of each input dimension.
        ''' </summary>
        ''' <returns>An array of the lower bound values.</returns>
        Public Property inputStart As Double()
        ''' <summary>
        ''' The scaling factor of each input dimension.
        ''' </summary>
        ''' <returns>An array of the scaling factor values.</returns>
        Public Property inputScale As Double()
        ''' <summary>
        ''' The lower bound of the output value range.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property outputStart As Double
        ''' <summary>
        ''' The scale factor of the output value range.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property outputScale As Double
        ''' <summary>
        ''' The number of the feature dimensions.
        ''' </summary>
        ''' <returns>An <see cref="Integer"/> value.</returns>
        Public Property length As Integer

        ''' <summary>
        ''' Create a new empty range transform data model.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create the range transform data model from a <see cref="RangeTransform"/> object.
        ''' </summary>
        ''' <param name="range">The source range transform object.</param>
        Sub New(range As RangeTransform)
            inputStart = range._inputStart
            inputScale = range._inputScale
            outputStart = range._outputStart
            outputScale = range._outputScale
            length = range._length
        End Sub

        ''' <summary>
        ''' Restore the <see cref="RangeTransform"/> object from this data model.
        ''' </summary>
        ''' <returns>A reconstructed <see cref="RangeTransform"/> object.</returns>
        Public Function GetTransform() As IRangeTransform
            Return New RangeTransform(inputStart, inputScale, outputStart, outputScale, length)
        End Function

        ''' <summary>
        ''' Display this data model as a json string.
        ''' </summary>
        ''' <returns>A json text which describes this range transform.</returns>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    ''' <summary>
    ''' The JSON serializable data model of a <see cref="GaussianTransform"/> object.
    ''' </summary>
    Public Class GaussianTransformModel

        ''' <summary>
        ''' The mean value of each feature dimension.
        ''' </summary>
        ''' <returns>An array of the mean values.</returns>
        Public Property means As Double()
        ''' <summary>
        ''' The standard deviation value of each feature dimension.
        ''' </summary>
        ''' <returns>An array of the standard deviation values.</returns>
        Public Property stddevs As Double()

        ''' <summary>
        ''' Create a new empty gaussian transform data model.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create the gaussian transform data model from a 
        ''' <see cref="GaussianTransform"/> object.
        ''' </summary>
        ''' <param name="gaussian">The source gaussian transform object.</param>
        Sub New(gaussian As GaussianTransform)
            means = gaussian._means
            stddevs = gaussian._stddevs
        End Sub

        ''' <summary>
        ''' Restore the <see cref="GaussianTransform"/> object from this data model.
        ''' </summary>
        ''' <returns>A reconstructed <see cref="GaussianTransform"/> object.</returns>
        Public Function GetTransform() As IRangeTransform
            Return New GaussianTransform(means, stddevs)
        End Function

        ''' <summary>
        ''' Display the mean values of this data model as a json string.
        ''' </summary>
        ''' <returns>A json text which contains the mean values.</returns>
        Public Overrides Function ToString() As String
            Return means.GetJson
        End Function

    End Class
End Namespace
