#Region "Microsoft.VisualBasic::c2bc2e3eb115487852ccd6423f30d941, Data_science\Mathematica\Math\Math\Distributions\Sample.vb"

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

    '     Class SampleDistribution
    ' 
    '         Properties: average, max, min, quantile, size
    '                     stdErr
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetRange, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Distributions

    ''' <summary>
    ''' The data sample xml model
    ''' </summary>
    Public Class SampleDistribution

        <XmlAttribute> Public Property min As Double
        <XmlAttribute> Public Property max As Double
        <XmlAttribute> Public Property average As Double
        <XmlAttribute> Public Property stdErr As Double
        <XmlAttribute> Public Property size As Integer

        ''' <summary>
        ''' 分别为0%, 25%, 50%, 75%, 100%
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property quantile As Double()

        Sub New()
        End Sub

        Sub New(data As IEnumerable(Of Double))
            Call Me.New(data.SafeQuery.ToArray)
        End Sub

        Sub New(v As Double())
            Dim q As QuantileEstimationGK = v.GKQuantile

            min = v.Min
            max = v.Max
            average = v.Average
            stdErr = v.StdError
            size = v.Length

            quantile = {
                q.Query(0),
                q.Query(0.25),
                q.Query(0.5),
                q.Query(0.75),
                q.Query(1)
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRange() As DoubleRange
            Return {min, max}
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
