Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

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

        Public ReadOnly Property Length As Integer
            Get
                Return data.Length
            End Get
        End Property

        Public ReadOnly Property First As T
            Get
                Return data(Scan0)
            End Get
        End Property

        Public Function SlideWindows(winSize As Integer,
                                     Optional offset As Integer = 1,
                                     Optional extendTails As Boolean = False) As SlideWindowHandle(Of T)()
            Return data.CreateSlideWindows(winSize, offset, extendTails)
        End Function

        Public Iterator Function Split(partSize As Integer) As IEnumerable(Of T())
            For Each chunk As T() In data.SplitIterator(partSize)
                Yield chunk
            Next
        End Function

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