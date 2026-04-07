#Region "Microsoft.VisualBasic::cd584b6177200d3813c3d199f1c08c93, Data\DataFrame\DataFrame\FastLoader.vb"

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

    '   Total Lines: 66
    '    Code Lines: 41 (62.12%)
    ' Comment Lines: 18 (27.27%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 7 (10.61%)
    '     File Size: 2.51 KB


    ' Module FastLoader
    ' 
    '     Function: ParseFeature, ReadCsv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.TypeCast
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Module FastLoader

    ''' <summary>
    ''' 在这里仅是针对简单格式的csv文件进行快速文件读取操作，对于包含有复杂格式字符串的csv文件，
    ''' 任然需要通过csv文件模块进行读取，之后再通过相应的API进行对象转换
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 简单格式的含义：
    ''' 
    ''' 1. csv文件之中无注释元数据信息
    ''' 2. 单元格的字符串之中无逗号，制表符等分隔符
    ''' </remarks>
    <Extension>
    Public Function ReadCsv(file As Stream,
                            Optional delimiter As Char = ","c,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional verbose As Boolean = True) As DataFrame

        Dim read As New DataFrameReader(file, rowHeader, delimiter, encoding)

        If file.Length > 1 * ByteSize.GB AndAlso verbose Then
            Call read.ReadStreamLines()
        Else
            Call read.ReadLines()
        End If

        Return New DataFrame With {
            .features = read _
                .GetFeatures _
                .ToDictionary(Function(v)
                                  Return v.name
                              End Function),
            .rownames = read _
                .GetRowHeaders _
                .UniqueNames _
                .ToArray
        }
    End Function

    ''' <summary>
    ''' Parse the feature column
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function ParseFeature(data As List(Of String), name As String) As FeatureVector
        Dim type As Type = DataImports.SampleForType(data, checkNullFactor:=True)
        Dim parser = type.ParseVector
        Dim array As Array = parser(data)

        Return FeatureVector.FromGeneral(name, array)
    End Function
End Module
