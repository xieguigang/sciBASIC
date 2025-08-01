Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' A bucket holds a subset of events and select significant events from it
    ''' </summary>
    Public Interface Bucket

        Sub selectInto(result As IList(Of ITimeSignal))

        Sub add(e As ITimeSignal)

    End Interface
End Namespace