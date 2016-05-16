Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.DataSourceModel

    Public Structure DataSample(Of T As IComparable)

        Public ReadOnly Property Min As T
            Get
                Return Ranges.Min
            End Get
        End Property
        Public ReadOnly Property Max As T
            Get
                Return Ranges.Max
            End Get
        End Property

        Public Average As Double
        Public data As T()
        Public Ranges As IRanges(Of T)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Module DataSampleAPI

        Public Function IntegerSample(data As IEnumerable(Of Integer)) As DataSample(Of Integer)
            Dim buf As Integer() = data.ToArray
            Return New DataSample(Of Integer) With {
                .Average = buf.Average,
                .Ranges = New IntRange(buf.Min, buf.Max),
                .data = buf
            }
        End Function

        Public Function DoubleSample(data As IEnumerable(Of Double)) As DataSample(Of Double)
            Dim buf As Double() = data.ToArray
            Return New DataSample(Of Double) With {
                .Average = buf.Average,
                .Ranges = New DoubleRange(buf.Min, buf.Max),
                .data = buf
            }
        End Function
    End Module
End Namespace