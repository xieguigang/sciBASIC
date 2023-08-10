Imports Microsoft.VisualBasic.DataMining.KMeans
Imports std = System.Math

Namespace GMM

    ''' <summary>
    ''' A collection of <see cref="Datum"/>
    ''' </summary>
    Public Class DatumList : Implements IEnumerable(Of Datum)

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

        Public Overridable ReadOnly Property Stdev(m As Mixture) As Double
            Get
                Dim mean = Me.Mean(m)
                Dim lStdev = 0.0
                For Each d In m_data
                    lStdev += std.Pow(d.val(m) - mean, 2)
                Next

                lStdev /= m_data.Count
                lStdev = std.Sqrt(lStdev)
                Return lStdev
            End Get
        End Property

        Public Overridable ReadOnly Property Mean(m As Mixture) As Double
            Get
                Dim lMean = 0.0
                For Each d In m_data
                    lMean += d.val(m)
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

        Public Iterator Function GetEnumerator() As IEnumerator(Of Datum) Implements IEnumerable(Of Datum).GetEnumerator
            For Each xi As Datum In m_data
                Yield xi
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace