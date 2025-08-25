Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Xpt.Types

Namespace Xpt

    ''' <summary>
    ''' SAS XPT file reader
    ''' </summary>
    Public Class SASXportFileIterator : Inherits SASXportConverter

        Private crecord As IEnumerable(Of Object)
        Private cPrimitiveRecord As IList(Of ReadstatValue)
        Private crow As Byte() = Nothing

        Public Sub New(fileName As String)
            MyBase.New(fileName)
        End Sub

        Public Sub New([is] As Stream)
            MyBase.New([is])
        End Sub

        Public Sub New(fileName As String, offset As Integer)
            Me.New(fileName)
            MyBase.seek(offset)
        End Sub

        Public Overridable Function hasNext() As Boolean
            Return Not MyBase.Done
        End Function

        ''' <summary>
        ''' read the data frame line by line
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function [next]() As IEnumerable(Of Object)
            crecord = MyBase.Record
            cPrimitiveRecord = MyBase.PrimitiveRecord
            crow = MyBase.Row

            MyBase.readNextRecord()

            Return crecord
        End Function

        Public Overridable Function nextPrimitive() As IList(Of ReadstatValue)
            [next]()
            Return cPrimitiveRecord
        End Function

        Public Overridable Function nextRaw() As Byte()
            [next]()
            Return crow
        End Function
    End Class

End Namespace
