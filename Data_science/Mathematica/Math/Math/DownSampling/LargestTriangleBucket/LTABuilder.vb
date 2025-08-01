#Region "Microsoft.VisualBasic::cb2682856060b8a7382ae1a9163a065c, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LTABuilder.vb"

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

    '   Total Lines: 46
    '    Code Lines: 32 (69.57%)
    ' Comment Lines: 3 (6.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (23.91%)
    '     File Size: 1.42 KB


    '     Class LTABuilder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: build, dynamic, fixed, oneBucket, threeBucket
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DownSampling.LargestTriangleBucket

    ''' <summary>
    ''' A builder class for LT Algorithms.
    ''' </summary>
    Public Class LTABuilder

        Public Shared ReadOnly S_FIXED As New FixedNumBucketSplitter(Of LTWeightedBucket, WeightedEvent)()
        Public Shared ReadOnly S_DYNAMIC As New LTDynamicBucketSplitter()
        Public Shared ReadOnly ONE_BUCKET As New LTOneBucketWeightCalculator()
        Public Shared ReadOnly THREE_BUCKET As New LTThreeBucketWeightCalculator()

        Private lta As LTAlgorithm

        Public Sub New()
            lta = New LTAlgorithm()
            lta.BucketFactory(New LTWeightedBucketFactory)
        End Sub

        Public Overridable Function fixed() As LTABuilder
            lta.SetSpliter(S_FIXED)
            Return Me
        End Function

        Public Overridable Function dynamic() As LTABuilder
            lta.SetSpliter(S_DYNAMIC)
            Return Me
        End Function

        Public Overridable Function oneBucket() As LTABuilder
            lta.Wcalc = ONE_BUCKET
            Return Me
        End Function

        Public Overridable Function threeBucket() As LTABuilder
            lta.Wcalc = THREE_BUCKET
            Return Me
        End Function

        Public Overridable Function build() As LTAlgorithm
            Return lta
        End Function

    End Class

End Namespace
