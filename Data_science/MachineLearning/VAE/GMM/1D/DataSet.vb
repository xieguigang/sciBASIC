#Region "Microsoft.VisualBasic::6beeba3756fdfb39ca7ce45fc3d965e2, Data_science\MachineLearning\VAE\GMM\1D\DataSet.vb"

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

    '   Total Lines: 90
    '    Code Lines: 67
    ' Comment Lines: 8
    '   Blank Lines: 15
    '     File Size: 2.77 KB


    '     Class DatumList
    ' 
    '         Properties: Mean, Stdev, width
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [get], components, GetEnumerator, nI, size
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace GMM

    ''' <summary>
    ''' A collection of <see cref="Datum"/>
    ''' </summary>
    Public Class DatumList : Implements Enumeration(Of Datum)

        Private m_data As Datum()
        Private m_components As Integer

        Public ReadOnly Property width As Integer
            Get
                Return m_data(0).getVector.Length
            End Get
        End Property

        Public Sub New(data As IEnumerable(Of ClusterEntity), components As Integer)
            m_components = components
            m_data = data.Select(Function(d) New Datum(d, components)).ToArray
        End Sub

        Sub New(data As IEnumerable(Of Double), components As Integer)
            m_components = components
            m_data = data _
                .Select(Function(d, i) New Datum(d, components, i + 1)) _
                .ToArray
        End Sub

        Public Overridable ReadOnly Property Stdev As Double
            Get
                Dim mean = Me.Mean
                Dim lStdev = 0.0
                For Each d In m_data
                    lStdev += std.Pow(d.val - mean, 2)
                Next

                lStdev /= m_data.Count
                lStdev = std.Sqrt(lStdev)
                Return lStdev
            End Get
        End Property

        Public Overridable ReadOnly Property Mean As Double
            Get
                Dim lMean = 0.0
                For Each d In m_data
                    lMean += d.val
                Next

                lMean /= m_data.Count
                Return lMean
            End Get
        End Property

        Public Overridable Function components() As Integer
            Return m_components
        End Function

        ''' <summary>
        ''' sum of the all data probs of i-th component
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Overridable Function nI(i As Integer) As Double
            Dim sum = 0.0
            For Each d In m_data
                sum += d.getProb(i)
            Next
            Return sum
        End Function

        Public Overridable Function size() As Integer
            Return m_data.Count
        End Function

        Public Overridable Function [get](i As Integer) As Datum
            Return m_data(i)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Datum) Implements Enumeration(Of Datum).GenericEnumerator
            For Each xi As Datum In m_data
                Yield xi
            Next
        End Function
    End Class
End Namespace
