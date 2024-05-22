#Region "Microsoft.VisualBasic::c52e82dd2d90e99bcd94be42192b7545, Data\BinaryData\netCDF\Components\attribute.vb"

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

    '   Total Lines: 93
    '    Code Lines: 62 (66.67%)
    ' Comment Lines: 18 (19.35%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (13.98%)
    '     File Size: 3.65 KB


    '     Class attribute
    ' 
    '         Properties: name, type, value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: getBytes, getObjectValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace Components

    ''' <summary>
    ''' 属对象性,主要是记录一些注解信息
    ''' </summary>
    Public Class attribute

        ''' <summary>
        ''' String with the name of the attribute
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' String with the type of the attribute
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property type As CDFDataTypes
        ''' <summary>
        ''' A number or string with the value of the attribute.
        ''' (如果是bytes数组, 则应该编码为base64字符串之后赋值到这个属性, 
        ''' 并且类型应该设置为<see cref="CDFDataTypes.NC_CHAR"/>, 因为在
        ''' 属性这里不接受数组类型)
        ''' </summary>
        ''' <returns></returns>
        <XmlText>
        Public Property value As String

        Sub New()
        End Sub

        Sub New(name$, value$, type As CDFDataTypes)
            Me.name = name
            Me.value = value
            Me.type = type
        End Sub

        Sub New(name As String, chars As String)
            Call Me.New(name, chars, type:=CDFDataTypes.NC_CHAR)
        End Sub

        Public Function getObjectValue() As Object
            Select Case type
                Case CDFDataTypes.NC_BYTE : Return Byte.Parse(value)
                Case CDFDataTypes.NC_CHAR : Return value
                Case CDFDataTypes.NC_DOUBLE : Return Double.Parse(value)
                Case CDFDataTypes.NC_FLOAT : Return Single.Parse(value)
                Case CDFDataTypes.NC_INT : Return Integer.Parse(value)
                Case CDFDataTypes.NC_SHORT : Return Short.Parse(value)
                Case CDFDataTypes.NC_INT64 : Return Long.Parse(value)
                Case CDFDataTypes.BOOLEAN

                    If value.IsPattern("\d+") Then
                        Return Long.Parse(value) <> 0
                    Else
                        Return value.ParseBoolean
                    End If

                Case Else
                    Throw New NotSupportedException
            End Select
        End Function

        Public Function getBytes(Optional base64Bytes As Boolean = False) As Byte()
            Select Case type
                Case CDFDataTypes.NC_BYTE : Return {Byte.Parse(value)}
                Case CDFDataTypes.NC_CHAR
                    If base64Bytes Then
                        Return value.Base64RawBytes
                    Else
                        Return UTF8WithoutBOM.GetBytes(value)
                    End If
                Case CDFDataTypes.NC_DOUBLE : Return BitConverter.GetBytes(Double.Parse(value))
                Case CDFDataTypes.NC_FLOAT : Return BitConverter.GetBytes(Single.Parse(value))
                Case CDFDataTypes.NC_INT : Return BitConverter.GetBytes(Integer.Parse(value))
                Case CDFDataTypes.NC_SHORT : Return BitConverter.GetBytes(Short.Parse(value))
                Case CDFDataTypes.NC_INT64 : Return BitConverter.GetBytes(Long.Parse(value))

                Case Else
                    Throw New NotSupportedException
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type.Description} = {value}"
        End Function
    End Class

End Namespace
