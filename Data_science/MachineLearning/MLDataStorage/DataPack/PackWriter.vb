Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DataPack

    Public Class PackWriter : Implements IDisposable

        Dim disposedValue As Boolean
        Dim stream As StreamPack

        Sub New(stream As Stream)
            Me.stream = New StreamPack(stream, meta_size:=32 * 1024 * 1024, [readonly]:=False)
        End Sub

        Private Sub AddSamples(samples As IEnumerable(Of Sample))
            Dim allSamples As New List(Of Sample)
            Dim check_duplicates As New Index(Of String)

            For Each sample As Sample In samples
                If sample.ID Like check_duplicates Then
                    Call $"there is a duplicated sample data: {sample.ID}!".Warning
                    Call stream.Delete($"/samples/{sample.ID}.dat")
                Else
                    Call check_duplicates.Add(sample.ID)
                End If

                Using file As Stream = stream.OpenBlock($"/samples/{sample.ID}.dat")
                    Dim buf As New BinaryDataWriter(file, byteOrder:=ByteOrder.BigEndian)

                    Call buf.Write(sample.label, BinaryStringFormat.DwordLengthPrefix)
                    Call buf.Write(sample.ID, BinaryStringFormat.DwordLengthPrefix)
                    Call buf.Write(sample.target)
                    Call buf.Write(sample.vector)
                End Using
            Next
        End Sub

        Private Sub WriteEncoder(norm As NormalizeMatrix)
            For i As Integer = 0 To norm.names.Length - 1
                Dim feature As SampleDistribution = norm.matrix.items(i)

                Using file As Stream = stream.OpenBlock($"/features/{norm.names(i)}.dat")
                    Dim buf As New BinaryDataWriter(file, byteOrder:=ByteOrder.BigEndian)
                    Dim v As Double() = New Double(10) {}

                    v(0) = feature.min
                    v(1) = feature.max
                    v(2) = feature.average
                    v(3) = feature.stdErr
                    v(4) = feature.size

                    ' length = 5
                    If Not feature.quantile Is Nothing Then
                        Call Array.ConstrainedCopy(feature.quantile, Scan0, v, 5, 5)
                    End If

                    v(10) = feature.mode

                    Call buf.Write(v)
                End Using
            Next
        End Sub

        Public Sub WriteDataSet(ds As DataSet)
            Dim attributes As New Dictionary(Of String, String())
            Dim dims As New Dictionary(Of String, Integer) From {
                {"features", ds.NormalizeMatrix.names.Length},
                {"samples", ds.DataSamples.size},
                {"outputs", ds.output.Length}
            }

            Call attributes.Add("feature_names", ds.NormalizeMatrix.names)
            Call attributes.Add("id", ds.DataSamples.AsEnumerable.Select(Function(a) a.ID).ToArray)
            Call attributes.Add("labels", ds.output)

            Call stream.WriteText({attributes.GetJson}, "/.etc/attributes.json")
            Call stream.WriteText({dims.GetJson}, "/.etc/dimension.json")

            Call AddSamples(ds.DataSamples.items)
            Call WriteEncoder(ds.NormalizeMatrix)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call stream.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace