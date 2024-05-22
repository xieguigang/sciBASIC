#Region "Microsoft.VisualBasic::7a50c718c7c61fd0df9ad5896f955a16, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\Models\Model.vb"

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

    '   Total Lines: 56
    '    Code Lines: 41 (73.21%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (26.79%)
    '     File Size: 1.58 KB


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

    Public Class RangeTransformModel

        Public Property inputStart As Double()
        Public Property inputScale As Double()
        Public Property outputStart As Double
        Public Property outputScale As Double
        Public Property length As Integer

        Sub New()
        End Sub

        Sub New(range As RangeTransform)
            inputStart = range._inputStart
            inputScale = range._inputScale
            outputStart = range._outputStart
            outputScale = range._outputScale
            length = range._length
        End Sub

        Public Function GetTransform() As IRangeTransform
            Return New RangeTransform(inputStart, inputScale, outputStart, outputScale, length)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    Public Class GaussianTransformModel

        Public Property means As Double()
        Public Property stddevs As Double()

        Sub New()
        End Sub

        Sub New(gaussian As GaussianTransform)
            means = gaussian._means
            stddevs = gaussian._stddevs
        End Sub

        Public Function GetTransform() As IRangeTransform
            Return New GaussianTransform(means, stddevs)
        End Function

        Public Overrides Function ToString() As String
            Return means.GetJson
        End Function

    End Class
End Namespace
