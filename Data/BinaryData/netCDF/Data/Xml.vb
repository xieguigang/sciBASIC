#Region "Microsoft.VisualBasic::e68c35df7de814ff71f534e6ae5e07c0, Data\BinaryData\netCDF\Data\Xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 71
    '    Code Lines: 38 (53.52%)
    ' Comment Lines: 24 (33.80%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (12.68%)
    '     File Size: 2.50 KB


    '     Class Xml
    ' 
    '         Properties: dimensions, globalAttributes, recordDimension, variables, version
    ' 
    '         Function: (+2 Overloads) SaveAsXml
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components

Namespace Data

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
End Namespace
