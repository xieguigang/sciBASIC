#Region "Microsoft.VisualBasic::7223f7508fd056ff32e2b365feeb476e, Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\Stream\ObjectOutputStream.vb"

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

    '   Total Lines: 134
    '    Code Lines: 99
    ' Comment Lines: 14
    '   Blank Lines: 21
    '     File Size: 5.62 KB


    '     Class ObjectOutputStream
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Close, (+2 Overloads) Dispose, Flush, WriteObject, WriteObjectInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.ValueTypes
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
            Dim fields As FieldInfo() = ObjectVisitor.GetAllFields(obj.GetType).Distinct.ToArray
            Dim bytes As Byte()
            Dim value As Object

            Call stream.Write(New Buffer(Encoding.ASCII.GetBytes(json)).Serialize)
            Call stream.Write(fields.Length)

            For Each field As FieldInfo In fields
                Call stream.Write(New Buffer(Encoding.ASCII.GetBytes(field.Name)).Serialize)

                value = field.GetValue(obj)

                If DataFramework.IsPrimitive(field.FieldType) Then
                    ' write value at here
                    Select Case field.FieldType
                        Case GetType(Integer) : bytes = BitConverter.GetBytes(CInt(value))
                        Case GetType(Double) : bytes = network.GetBytes(CDbl(value))
                        Case GetType(String) : bytes = If(value Is Nothing, New Byte() {}, Encoding.UTF8.GetBytes(CStr(value)))
                        Case GetType(Single) : bytes = network.GetBytes(CSng(value))
                        Case GetType(Long) : bytes = BitConverter.GetBytes(CLng(value))
                        Case GetType(Short) : bytes = BitConverter.GetBytes(CShort(value))
                        Case GetType(Byte) : bytes = New Byte() {CByte(value)}
                        Case GetType(Boolean) : bytes = New Byte() {If(CBool(value), 1, 0)}
                        Case GetType(Date) : bytes = network.GetBytes(CDate(value).UnixTimeStamp)
                        Case Else
                            Throw New NotImplementedException(field.Name & ": " & field.FieldType.Name)
                    End Select

                    Call stream.Write(New Buffer(bytes).Serialize)
                ElseIf field.FieldType.IsArray Then
                    If DataFramework.IsPrimitive(field.FieldType.GetElementType) Then
                        ' write numeric/string vector
                        bytes = RawStream.GetBytes(DirectCast(value, Array))
                        stream.Write(New Buffer(bytes).Serialize)
                        stream.Write(CInt(field.FieldType.GetElementType.PrimitiveTypeCode))
                    Else
                        ' write object array
                        Dim array As Array = value
                        Dim vec_dims As Integer = If(array Is Nothing, 0, array.Length)

                        Call stream.Write(vec_dims)

                        For i As Integer = 0 To vec_dims - 1
                            Call WriteObject(array.GetValue(i))
                        Next
                    End If
                Else
                    ' class/struct contains properties
                    Call WriteObject(value)
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
