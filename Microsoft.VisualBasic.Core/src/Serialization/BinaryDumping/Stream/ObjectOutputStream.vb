Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports TypeInfo = Microsoft.VisualBasic.Scripting.MetaData.TypeInfo

Namespace Serialization.BinaryDumping

    Public Class ObjectOutputStream : Implements IDisposable

        Dim disposedValue As Boolean
        Dim stream As BinaryWriter
        Dim network As New NetworkByteOrderBuffer

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(s As Stream)
            stream = New BinaryWriter(s)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(wr As BinaryWriter)
            stream = wr
        End Sub

        Public Sub WriteObject(obj As Object)
            If obj Is Nothing Then
                Call stream.Write(-1)
            Else
                Call WriteObjectInternal(obj)
            End If
        End Sub

        Private Sub WriteObjectInternal(obj As Object)
            Dim info As New TypeInfo(obj.GetType)
            Dim json As String = info.GetJson
            Dim fields As FieldInfo() = ObjectVisitor.GetAllFields(obj.GetType).ToArray
            Dim bytes As Byte()
            Dim value As Object

            Call stream.Write(New Buffer(Encoding.ASCII.GetBytes(json)).Serialize)
            Call stream.Write(fields.Length)

            For Each field As FieldInfo In fields
                Call stream.Write(New Buffer(Encoding.ASCII.GetBytes(field.Name)).Serialize)

                If DataFramework.IsPrimitive(field.FieldType) Then
                    value = field.GetValue(obj)

                    ' write value at here
                    Select Case field.FieldType
                        Case GetType(Integer) : bytes = BitConverter.GetBytes(CInt(value))
                        Case GetType(Double) : bytes = network.GetBytes(CDbl(value))
                        Case GetType(String) : bytes = Encoding.UTF8.GetBytes(CStr(value))
                        Case Else
                            Throw New NotImplementedException(field.Name & ": " & field.FieldType.Name)
                    End Select

                    Call stream.Write(New Buffer(bytes).Serialize)
                Else
                    ' class/struct contains properties
                    Call WriteObject(obj)
                End If
            Next
        End Sub

        Public Sub Flush()
            Call stream.Flush()
        End Sub

        Public Sub Close()
            Call stream.Close()
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Try
                        Call stream.Flush()
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