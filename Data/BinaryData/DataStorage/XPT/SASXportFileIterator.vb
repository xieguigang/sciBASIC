Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Xpt.Types

Namespace Xpt

    Public Class SASXportFileIterator
        Inherits SASXportConverter

        Private crecord As IList(Of String) = Nothing
        Private cPrimitiveRecord As IList(Of ReadstatValue) = Nothing
        Private crow As Byte() = Nothing

        Public Sub New(fileName As String)
            MyBase.New(fileName)
            MyBase.init()
        End Sub

        Public Sub New([is] As Stream)
            MyBase.New([is])
            MyBase.init()
        End Sub

        Public Sub New(fileName As String, offset As Integer)
            Me.New(fileName)
            MyBase.seek(offset)
        End Sub

        Public Overridable Function hasNext() As Boolean
            Return Not MyBase.Done
        End Function

        Public Overridable Function [next]() As IList(Of String)
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

        Public Shared Sub Main(args As String())
            Dim iterator As SASXportFileIterator = New SASXportFileIterator("/grid/data/xpt/test3.sasxpt")
            While iterator.hasNext()
                Dim row As IList(Of String) = iterator.next()
            End While
            Console.WriteLine("Total Rows: " & iterator.RowCount.ToString())
            iterator.Dispose()

            Dim cal As Date = New DateTime()
            cal = New DateTime(1960, 1, 1)
            cal.AddDays(19778)
            Console.WriteLine(cal.ToString())
        End Sub
    End Class

End Namespace
