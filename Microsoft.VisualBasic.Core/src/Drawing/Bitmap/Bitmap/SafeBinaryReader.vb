Imports System.IO

Namespace Imaging.BitmapImage.FileStream

    Public Class SafeBinaryReader : Inherits BinaryReader

        Public Sub New(stream As Stream)
            MyBase.New(stream)
        End Sub

        Public Overrides Function ReadInt16() As Short
            Dim data = MyBase.ReadBytes(2)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt16(data, 0)
        End Function

        Public Overrides Function ReadUInt16() As UShort
            Return CUShort(ReadInt16())
        End Function

        Public Overrides Function ReadInt32() As Integer
            Dim data = MyBase.ReadBytes(4)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt32(data, 0)
        End Function

        Public Overrides Function ReadUInt32() As UInteger
            Return CUInt(ReadUInt32())
        End Function

        Public Overrides Function ReadInt64() As Long
            Dim data = MyBase.ReadBytes(8)
            If Not BitConverter.IsLittleEndian Then Array.Reverse(data)
            Return BitConverter.ToInt64(data, 0)
        End Function

        Public Overrides Function ReadUInt64() As ULong
            Return CULng(ReadInt64())
        End Function
    End Class
End Namespace
