Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace netCDF.Components

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

End Namespace