Imports System.IO
Imports Microsoft.VisualBasic.Net.Http

Namespace HDF5.dataset.filters

    ''' <summary>
    ''' GZip
    ''' </summary>
    Public Class DeflatePipelineFilter : Implements IFilter

        Public ReadOnly Property id As Integer Implements IFilter.id
        Public ReadOnly Property name As String Implements IFilter.name

        Public Function decode(encodedData() As Byte, filterData() As Integer) As Byte() Implements IFilter.decode
            Return GZipStream.Deflate(New MemoryStream(encodedData)).ToArray
        End Function
    End Class
End Namespace