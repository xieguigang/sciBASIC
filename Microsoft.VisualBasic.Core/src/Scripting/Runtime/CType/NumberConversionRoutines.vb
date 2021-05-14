#Region "Microsoft.VisualBasic::5d263314bbddb400c829da18e7d03947, Microsoft.VisualBasic.Core\src\Scripting\Runtime\CType\NumberConversionRoutines.vb"

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

    '     Module NumberConversionRoutines
    ' 
    '         Function: CDblSafe, (+2 Overloads) CIntSafe, (+2 Overloads) CShortSafe, CStrInternal, CStrSafe
    '                   IsNumber
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting.Runtime

    ''' <summary>
    ''' 这个模块之中包含有安全的将字符串解析为不同的数值类型的方法函数
    ''' </summary>
    Public Module NumberConversionRoutines

        Public Function CDblSafe(strWork As String) As Double
            Dim dblValue As Double = 0

            If Double.TryParse(strWork, dblValue) Then
                Return dblValue
            Else
                Return 0
            End If
        End Function

        Public Function CShortSafe(dblWork As Double) As Int16
            If dblWork <= 32767 And dblWork >= -32767 Then
                Return CShort(dblWork)
            Else
                If dblWork < 0 Then
                    Return -32767
                Else
                    Return 32767
                End If
            End If
        End Function

        Public Function CShortSafe(strWork As String) As Int16
            Dim dblValue As Double = 0

            If Double.TryParse(strWork, dblValue) Then
                Return CShortSafe(dblValue)
            ElseIf strWork.ToLower() = "true" Then
                Return -1
            Else
                Return 0
            End If
        End Function

        Public Function CIntSafe(dblWork As Double) As Int32
            If dblWork <= Integer.MaxValue And dblWork >= Integer.MinValue Then
                Return CInt(dblWork)
            Else
                If dblWork < 0 Then
                    Return Integer.MinValue
                Else
                    Return Integer.MaxValue
                End If
            End If
        End Function

        Public Function CIntSafe(strWork As String) As Int32
            Dim dblValue As Double = 0

            If Double.TryParse(strWork, dblValue) Then
                Return CIntSafe(dblValue)
            ElseIf strWork.ToLower() = "true" Then
                Return -1
            Else
                Return 0
            End If
        End Function

        ReadOnly cstrCache As New Dictionary(Of Type, IImplictCTypeOperator(Of Object, String))
        ReadOnly objToStringCache As New Dictionary(Of Type, Boolean)

        ''' <summary>
        ''' 安全的将目标对象转换为字符串值
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns>
        ''' 如果目标是字节数组，则会被转换为base64字符串
        ''' </returns>
        Public Function CStrSafe(obj As Object, Optional default$ = "", Optional originToStringAsNothing As Boolean = False) As String
            If obj Is Nothing Then
                Return String.Empty
            ElseIf Convert.IsDBNull(obj) Then
                Return String.Empty
            Else

                Try
                    Return CStrInternal(obj, [default], originToStringAsNothing)
                Catch ex As Exception
                    Try
                        ' 调用ToString函数来返回字符串值
                        Return obj.ToString
                    Catch ex2 As Exception
                        Return [default]
                    End Try
                End Try

            End If
        End Function

        Private Function CStrInternal(obj As Object, default$, originToStringAsNothing As Boolean) As String
            Dim type As Type = obj.GetType
            Dim delg As IImplictCTypeOperator(Of Object, String)

            If type Is GetType(Byte()) OrElse type.IsInheritsFrom(GetType(IEnumerable(Of Byte))) Then
                With CType(obj, IEnumerable(Of Byte)).ToArray
                    Return .ToBase64String
                End With
            Else
                Select Case type
                    Case GetType(Integer())
                        Return DirectCast(obj, Integer()).GetJson
                    Case GetType(Long())
                        Return DirectCast(obj, Long()).GetJson
                    Case GetType(Double())
                        Return DirectCast(obj, Double()).GetJson
                    Case GetType(String())
                        Return DirectCast(obj, String()).GetJson
                End Select
            End If

            ' 2018-3-18 假若找不到操作符的话，函数会返回Nothing
            ' 同样也需要将这个Nothing添加进入字典之中，否则在运行linq代码的时候
            ' 查找函数会被重复执行，同样会造成性能浪费
            If Not cstrCache.ContainsKey(type) Then
                delg = type.GetNarrowingOperator(Of String)

                SyncLock cstrCache
                    Call cstrCache.Add(type, delg)
                End SyncLock
            Else
                delg = cstrCache(type)
            End If

            Try
                If delg Is Nothing Then
                    If originToStringAsNothing Then
                        If objToStringCache.ContainsKey(type) Then
                            Return False
                        End If

                        Dim isObjToString As Boolean = type.GetMethods() _
                            .Where(Function(m)
                                       Return m.Name = "ToString" AndAlso
                                              m.GetParameters.IsNullOrEmpty AndAlso
                                              m.GetGenericArguments.IsNullOrEmpty
                                   End Function) _
                            .First _
                            .DeclaringType Is GetType(Object)

                        If isObjToString Then
                            objToStringCache.Add(type, True)
                            Return Nothing
                        Else
                            Return obj.ToString
                        End If
                    Else
                        ' 调用ToString函数来返回字符串值
                        Return obj.ToString
                    End If
                Else
                    Return delg(obj)
                End If
            Catch ex2 As Exception
                Return [default]
            End Try
        End Function

        Public Function IsNumber(strValue As String) As Boolean
            Try
                Return Double.TryParse(strValue, 0)
            Catch ex As Exception
                Return False
            End Try
        End Function
    End Module
End Namespace
