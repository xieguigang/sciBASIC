Imports System.IO
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports TypeInfo = Microsoft.VisualBasic.Scripting.MetaData.TypeInfo

Namespace Serialization.BinaryDumping

    Public Class ObjectInputStream : Implements IDisposable

        Dim disposedValue As Boolean
        Dim stream As BinaryReader
        Dim network As New NetworkByteOrderBuffer

        Sub New(s As Stream)
            stream = New BinaryReader(s)
        End Sub

        Sub New(rd As BinaryReader)
            stream = rd
        End Sub

        Public Function ReadObject() As Object
            Dim flag As Integer = stream.ReadInt32

            If flag <= 0 Then
                Return Nothing
            End If

            Dim info As TypeInfo = Encoding.ASCII.GetString(Buffer.Parse(stream).buffer).LoadJSON(Of TypeInfo)
            Dim obj As Object = Activator.CreateInstance(info.GetType)
            Dim nsize As Integer = stream.ReadInt32
            Dim value As Object
            Dim fields As Dictionary(Of String, FieldInfo) = ObjectVisitor _
                .GetAllFields(obj.GetType) _
                .ToDictionary(Function(f)
                                  Return f.Name
                              End Function)

            For i As Integer = 0 To nsize - 1
                Dim name As String = Encoding.ASCII.GetString(Buffer.Parse(stream).buffer)
                Dim field As FieldInfo = fields(name)

                If DataFramework.IsPrimitive(field.FieldType) Then
                    Dim buf As Buffer = Buffer.Parse(stream)

                    Select Case field.FieldType
                        Case GetType(Integer) : value = BitConverter.ToInt32(buf.buffer, Scan0)
                        Case GetType(Double) : value = BitConverter.ToDouble(buf.buffer, Scan0)
                        Case Else
                            Throw New NotImplementedException($"{field.Name}: {field.FieldType.Name}")
                    End Select
                Else
                    value = Me.ReadObject
                End If

                Call field.SetValue(obj, value)
            Next

            Return obj
        End Function

        Public Sub Close()
            stream.Close()
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Try
                        Call stream.Close()
                    Catch ex As Exception

                    End Try
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace