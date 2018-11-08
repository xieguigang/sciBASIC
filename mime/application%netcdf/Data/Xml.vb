Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components

''' <summary>
''' 将netCDF转储为XML文件
''' </summary>
<XmlType("netCDF", [Namespace]:=Xml.netCDF)> Public Class Xml : Inherits XmlDataModel

    Public Const netCDF$ = "https://www.unidata.ucar.edu/software/netcdf/docs/file_format_specifications.html"

    <XmlAttribute>
    Public Property version As String

    ''' <summary>
    ''' Number with the length of record dimension
    ''' </summary>
    ''' <returns></returns>
    Public Property recordDimension As recordDimension
    ''' <summary>
    ''' List of dimensions
    ''' </summary>
    ''' <returns></returns>
    Public Property dimensions As Dimension()
    ''' <summary>
    ''' List of global attributes
    ''' </summary>
    ''' <returns></returns>
    Public Property globalAttributes As attribute()
    ''' <summary>
    ''' List of variables
    ''' </summary>
    ''' <returns></returns>
    Public Property variables As variable()

    ''' <summary>
    ''' 这个函数方法只适用于比较小的数据文件
    ''' </summary>
    ''' <param name="out"></param>
    ''' <returns></returns>
    Public Shared Function SaveAsXml(cdf As netCDFReader, out As Stream) As Boolean
        Dim xml As New Xml With {
            .dimensions = cdf.dimensions,
            .globalAttributes = cdf.globalAttributes,
            .recordDimension = cdf.recordDimension,
            .version = cdf.version,
            .variables = cdf.variables _
                .Select(Function(var)
                            var.value = cdf.getDataVariable(var)
                            Return var
                        End Function) _
                .ToArray
        }

        Using writer As New StreamWriter(out)
            Call writer.WriteLine(xml.GetXml)
        End Using

        Return True
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function SaveAsXml(cdf As netCDFReader, path$) As Boolean
        Return SaveAsXml(cdf, path.Open)
    End Function
End Class
