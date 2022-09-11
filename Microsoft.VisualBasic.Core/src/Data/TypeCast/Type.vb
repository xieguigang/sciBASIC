Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Class StringCaster : Inherits TypeCaster(Of String)

        ReadOnly utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return utf8.GetBytes(DirectCast(value, String))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return value
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return utf8.GetString(bytes, Scan0, bytes.Length)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return str
        End Function
    End Class

    Public Class IntegerCaster : Inherits TypeCaster(Of Integer)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Integer))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Integer).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToInt32(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Integer.Parse(str)
        End Function
    End Class

    Public Class DoubleCaster : Inherits TypeCaster(Of Double)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Double))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Double).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToDouble(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Double.Parse(str)
        End Function
    End Class

    Public Class DateCaster : Inherits TypeCaster(Of Date)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Date).ToBinary)
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Date).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return Date.FromBinary(BitConverter.ToInt64(bytes, Scan0))
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Date.Parse(str)
        End Function
    End Class
End Namespace