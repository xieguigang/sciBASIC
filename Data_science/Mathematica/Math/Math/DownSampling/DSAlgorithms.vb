Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.DownSampling.LargestTriangleBucket
Imports Microsoft.VisualBasic.Math.DownSampling.MaxMin

Namespace DownSampling

    ''' <summary>
    ''' Downsampling algorithms for time series data（LTOB, LTTB, LTD, OSI-PI Plot）
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/avina/downsampling
    ''' </remarks>
    Public NotInheritable Class DSAlgorithms : Implements DownSamplingAlgorithm

        ''' <summary>
        ''' OSIsoft PI PlotValues </summary>
        Public Shared ReadOnly PIPLOT As New DSAlgorithms("PIPLOT", InnerEnum.PIPLOT, New PIPlotAlgorithm())
        ''' <summary>
        ''' Largest Triangle Three Bucket </summary>
        Public Shared ReadOnly LTTB As New DSAlgorithms("LTTB", InnerEnum.LTTB, (New LTABuilder()).threeBucket().fixed().build())
        ''' <summary>
        ''' Largest Triangle One Bucket </summary>
        Public Shared ReadOnly LTOB As New DSAlgorithms("LTOB", InnerEnum.LTOB, (New LTABuilder()).oneBucket().fixed().build())
        ''' <summary>
        ''' Largest Triangle Dynamic </summary>
        Public Shared ReadOnly LTD As New DSAlgorithms("LTD", InnerEnum.LTD, (New LTABuilder()).threeBucket().dynamic().build())
        ''' <summary>
        ''' Maximum and minimum value </summary>
        Public Shared ReadOnly MAXMIN As New DSAlgorithms("MAXMIN", InnerEnum.MAXMIN, New MMAlgorithm())

        Private Shared ReadOnly valueList As IList(Of DSAlgorithms) = New List(Of DSAlgorithms)()

        Shared Sub New()
            valueList.Add(PIPLOT)
            valueList.Add(LTTB)
            valueList.Add(LTOB)
            valueList.Add(LTD)
            valueList.Add(MAXMIN)
        End Sub

        Public Enum InnerEnum
            PIPLOT
            LTTB
            LTOB
            LTD
            MAXMIN
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Private [delegate] As DownSamplingAlgorithm

        Friend Sub New(name As String, thisInnerEnumValue As InnerEnum, [delegate] As DownSamplingAlgorithm)
            Me.delegate = [delegate]

            nameValue = name
            ordinalValue = nextOrdinal
            nextOrdinal += 1
            innerEnumValue = thisInnerEnumValue
        End Sub

        Public Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal) Implements DownSamplingAlgorithm.process
            Return [delegate].process(data, threshold)
        End Function


        Public Shared Function values() As IList(Of DSAlgorithms)
            Return valueList
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As DSAlgorithms
            For Each enumInstance As DSAlgorithms In DSAlgorithms.valueList
                If enumInstance.nameValue = name Then
                    Return enumInstance
                End If
            Next
            Throw New System.ArgumentException(name)
        End Function
    End Class

End Namespace