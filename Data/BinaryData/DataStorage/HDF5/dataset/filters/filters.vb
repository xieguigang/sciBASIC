#Region "Microsoft.VisualBasic::5aba781c2931e519ae3824ee56acfcf9, Data\BinaryData\DataStorage\HDF5\dataset\filters\filters.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class DeflatePipelineFilter
    ' 
    '         Properties: id, name
    ' 
    '         Function: decode
    ' 
    '     Class Fletcher32CheckSum
    ' 
    '         Properties: id, name
    ' 
    '         Function: decode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.SecurityString

Namespace HDF5.dataset.filters

    ''' <summary>
    ''' GZip
    ''' </summary>
    Public Class DeflatePipelineFilter : Implements IFilter

        Public ReadOnly Property id As Integer Implements IFilter.id
        Public ReadOnly Property name As String Implements IFilter.name

        Public Function decode(encodedData() As Byte, filterData() As Integer) As Byte() Implements IFilter.decode
            Return encodedData.UnZipStream(noMagic:=True).ToArray
        End Function
    End Class

    Public Class Fletcher32CheckSum : Implements IFilter

        Public ReadOnly Property id As Integer Implements IFilter.id
        Public ReadOnly Property name As String Implements IFilter.name

        Public Function decode(encodedData() As Byte, filterData() As Integer) As Byte() Implements IFilter.decode
            Dim checksum = encodedData.Fletcher32(Scan0, encodedData.Length)
            Return BitConverter.GetBytes(checksum)
        End Function
    End Class
End Namespace
