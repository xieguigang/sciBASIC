Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Serialization

Public Class FieldAttribute : Inherits Field

    ''' <summary>
    ''' the array length
    ''' </summary>
    ''' <returns></returns>
    Public Property N As Integer
    ''' <summary>
    ''' the binary data file offset
    ''' </summary>
    ''' <returns></returns>
    Public Property offset As Long = -1

    Public ReadOnly Property ReadArray As Boolean
        Get
            Return N > 0
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ordinal">The ordinal position in the rawdata layout</param>
    ''' <param name="n">for array used only, means array length to read, default negative means scalar</param>
    Sub New(ordinal As Integer, Optional n As Integer = -1)
        Call MyBase.New(ordinal)
        Me.N = n
    End Sub

    Public Function Read(buf As BinaryDataReader, p As PropertyInfo) As Object
        Dim type As Type = p.PropertyType
        Dim code As TypeCode = Type.GetTypeCode(type)

        If offset >= 0 Then
            Call buf.Seek(offset, SeekOrigin.Begin)
        End If

        If type.IsArray Then
            Dim scalar As Type = type.GetElementType
            Dim sizeof As Integer = Marshal.SizeOf(scalar)
            Dim len As Integer = sizeof * N
            Dim view As New MemoryStream(buf.ReadBytes(len))

            Return RawStream.GetData(view, code:=Type.GetTypeCode(scalar))
        ElseIf type Is GetType(String) AndAlso ReadArray Then
            ' read chars array with fix length
            Dim chars As Char() = buf.ReadChars(N)
            Dim si As New String(chars)

            Return Strings.Trim(si)
        Else
            ' read scalar
            Return ReaderProvider.ReadScalar(code)(buf)
        End If
    End Function
End Class