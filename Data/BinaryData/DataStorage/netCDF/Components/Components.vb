#Region "Microsoft.VisualBasic::c35378bef3278a60fc34b98cf1d87935, Data\BinaryData\DataStorage\netCDF\Components\Components.vb"

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

    '     Structure Dimension
    ' 
    '         Properties: [Byte], [Double], [Integer], [Long], [Short]
    '                     Float, Text
    ' 
    '         Function: ToString
    ' 
    '     Class DimensionList
    ' 
    '         Properties: dimensions, HaveRecordDimension, recordId, recordName
    ' 
    '         Function: ToString
    ' 
    '     Class recordDimension
    ' 
    '         Properties: id, length, name, recordStep
    ' 
    '         Function: ToString
    ' 
    '     Class attribute
    ' 
    '         Properties: name, type, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: getBytes, getObjectValue, ToString
    ' 
    '     Class variable
    ' 
    '         Properties: attributes, dimensions, name, offset, record
    '                     size, type, value
    ' 
    '         Function: FindAttribute, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace netCDF.Components

    ''' <summary>
    ''' ``[name => size]``
    ''' </summary>
    ''' 
    <XmlType("dim", [Namespace]:=Xml.netCDF)>
    Public Structure Dimension

        ''' <summary>
        ''' String with the name of the dimension
        ''' </summary>
        <XmlAttribute> Dim name As String
        ''' <summary>
        ''' Number with the size of the dimension
        ''' </summary>
        <XmlText>
        Dim size As Integer

        Public Overrides Function ToString() As String
            Return $"{name}(size={size})"
        End Function

        Public Shared ReadOnly Property [Double] As Dimension
            Get
                Return New Dimension With {.name = GetType(Double).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property [Long] As Dimension
            Get
                Return New Dimension With {.name = GetType(Long).FullName, .size = 8}
            End Get
        End Property

        Public Shared ReadOnly Property Float As Dimension
            Get
                Return New Dimension With {.name = GetType(Single).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Short] As Dimension
            Get
                Return New Dimension With {.name = GetType(Short).FullName, .size = 2}
            End Get
        End Property

        Public Shared ReadOnly Property [Integer] As Dimension
            Get
                Return New Dimension With {.name = GetType(Integer).FullName, .size = 4}
            End Get
        End Property

        Public Shared ReadOnly Property [Byte] As Dimension
            Get
                Return New Dimension With {.name = GetType(Byte).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property [Boolean] As Dimension
            Get
                Return New Dimension With {.name = GetType(Boolean).FullName, .size = 1}
            End Get
        End Property

        Public Shared ReadOnly Property Text(fixedChars As Integer) As Dimension
            Get
                Return New Dimension With {.name = GetType(String).FullName, .size = fixedChars}
            End Get
        End Property
    End Structure

    Public Class DimensionList

        <XmlAttribute> Public Property recordId As Integer?
        <XmlAttribute> Public Property recordName As String

        Public ReadOnly Property HaveRecordDimension As Boolean
            Get
                Return Not (recordId Is Nothing AndAlso recordName = "NA")
            End Get
        End Property

        Public Property dimensions As Dimension()

        Public Overrides Function ToString() As String
            Return $"[{recordId}] {recordName}"
        End Function
    End Class

    ''' <summary>
    ''' Metadata for the record dimension
    ''' </summary>
    Public Class recordDimension

        ''' <summary>
        ''' Number of elements in the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property length As Integer
        ''' <summary>
        ''' Id number In the list Of dimensions For the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property id As Integer
        ''' <summary>
        ''' String with the name of the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' Number with the record variables step size
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property recordStep As Integer

        Public Overrides Function ToString() As String
            Return $"[{id}] {name} ({recordStep}x{length})"
        End Function
    End Class

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
        ''' 并且类型应该设置为<see cref="CDFDataTypes.CHAR"/>, 因为在
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

        Public Function getObjectValue() As Object
            Select Case type
                Case CDFDataTypes.BYTE : Return Byte.Parse(value)
                Case CDFDataTypes.CHAR : Return value
                Case CDFDataTypes.DOUBLE : Return Double.Parse(value)
                Case CDFDataTypes.FLOAT : Return Single.Parse(value)
                Case CDFDataTypes.INT : Return Integer.Parse(value)
                Case CDFDataTypes.SHORT : Return Short.Parse(value)
                Case CDFDataTypes.LONG : Return Long.Parse(value)

                Case Else
                    Throw New NotSupportedException
            End Select
        End Function

        Public Function getBytes(Optional base64Bytes As Boolean = False) As Byte()
            Select Case type
                Case CDFDataTypes.BYTE : Return {Byte.Parse(value)}
                Case CDFDataTypes.CHAR
                    If base64Bytes Then
                        Return value.Base64RawBytes
                    Else
                        Return UTF8WithoutBOM.GetBytes(value)
                    End If
                Case CDFDataTypes.DOUBLE : Return BitConverter.GetBytes(Double.Parse(value))
                Case CDFDataTypes.FLOAT : Return BitConverter.GetBytes(Single.Parse(value))
                Case CDFDataTypes.INT : Return BitConverter.GetBytes(Integer.Parse(value))
                Case CDFDataTypes.SHORT : Return BitConverter.GetBytes(Short.Parse(value))
                Case CDFDataTypes.LONG : Return BitConverter.GetBytes(Long.Parse(value))

                Case Else
                    Throw New NotSupportedException
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type.Description} = {value}"
        End Function
    End Class

    ''' <summary>
    ''' 变量对象,CDF文件之中的实验数据之类的数据都是保存于这个对象之中的
    ''' </summary>
    Public Class variable

        ''' <summary>
        ''' String with the name of the variable
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' Array with the dimension IDs of the variable.
        ''' (<see cref="Header.dimensions"/>)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property dimensions As Integer()
        ''' <summary>
        ''' Array with the attributes of the variable
        ''' </summary>
        ''' <returns></returns>
        Public Property attributes As attribute()
        ''' <summary>
        ''' String with the type of the variable
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property type As CDFDataTypes
        ''' <summary>
        ''' Number with the size of the variable.(在文件之中的数据字节大小)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property size As Integer
        ''' <summary>
        ''' Number with the offset where of the variable begins
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property offset As UInteger
        ''' <summary>
        ''' True if Is a record variable, false otherwise
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property record As Boolean

        ''' <summary>
        ''' 惰性求值的属性
        ''' </summary>
        ''' <returns></returns>
        Public Property value As CDFData

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindAttribute(name As String) As attribute
            Return attributes.FirstOrDefault(Function(a) a.name = name)
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name}[offset={offset}] As {type.Description}"
        End Function
    End Class
End Namespace
