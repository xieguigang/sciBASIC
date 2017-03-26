#Region "Microsoft.VisualBasic::58e46354b1d1f69edd44b894835706b9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Value\Uid.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language

    Public Class Uid : Implements INamedValue

        ''' <summary>
        ''' index collection of array <see cref="__chars"/>
        ''' </summary>
        Dim chars As List(Of Integer)

        ReadOnly __chars As Char() = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        ReadOnly __upbound As Integer = __chars.Length - 1

        ''' <summary>
        ''' 可以通过这个属性来重设uid的字符串的值
        ''' </summary>
        ''' <returns></returns>
        Public Property Key As String Implements IKeyedEntity(Of String).Key
            Get
                Return ToString()
            End Get
            Set(value As String)
                Call chars.Clear()

                For Each c As Char In value
                    Dim index% = Array.IndexOf(__chars, c)

                    If index = -1 Then
                        Call __error(c)
                    Else
                        Call chars.Add(index)
                    End If
                Next
            End Set
        End Property

        Private Sub __error(c As Char)
            Dim msg$ = $"Char '{c}' is not a valid ASCII char, valids list: " & __chars.GetJson
            Throw New NotSupportedException(msg)
        End Sub

        ''' <summary>
        ''' 使用自定义顺序的字符序列
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="_chars"></param>
        Sub New(n As Integer, _chars As IEnumerable(Of Char))
            chars += -1
            __chars = _chars.ToArray
            __upbound = __chars.Length - 1

            For i As Integer = 0 To n - 1
                Call __plus(chars.Count - 1)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="caseSensitive">
        ''' 假若是使用这个uid对象来生成临时文件名的话，由于Windows的文件系统是不区分大小写的，所以Aa的情况会出现同名的情况，
        ''' 所以在这里就需要设置为False了，大小写重名的情况在Linux或者Mac上面没有影响
        ''' </param>
        Sub New(n As Integer, Optional caseSensitive As Boolean = True)
            chars += -1

            If Not caseSensitive Then
                __upbound -= 26    ' 则只有小写字母
            End If

            For i As Integer = 0 To n - 1
                Call __plus(chars.Count - 1)
            Next
        End Sub

        Sub New(i As Uid, Optional caseSensitive As Boolean = True)
            chars = New List(Of Integer)(i.chars)

            If Not caseSensitive Then
                __upbound -= 26    ' 则只有小写字母
            End If
        End Sub

        ''' <summary>
        ''' ZERO
        ''' </summary>
        ''' <param name="caseSensitive">
        ''' 大小写敏感？假若是需要应用于文件名称，在Windows操作系统之上建议设置为False不敏感，否则会出现相同字母但是不同大小写的文件会被覆盖的情况出现
        ''' </param>
        Sub New(Optional caseSensitive As Boolean = True)
            Call Me.New(Scan0, caseSensitive)
        End Sub

        Private Function __plus(l As Integer) As Integer
            Dim n As Integer = chars(l) + 1
            Dim move As Integer = 0

            If n > __upbound Then
                n = 0
                Dim pl = l - 1

                If pl < 0 Then
                    Call chars.Insert(0, 1)
                    l += 1
                    move = 1
                Else
                    l += __plus(pl)
                End If
            End If

            chars(l) = n

            Return move
        End Function

        Public Function Plus() As String
            Call __plus(chars.Count - 1)
            Return ToString()
        End Function

        ''' <summary>
        ''' 请注意，这个操作是线程不安全的，所以请确保在执行这个命令之前使用``SyncLock``加锁
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator +(i As Uid, n As Integer) As Uid
            For o As Integer = 0 To n - 1
                Call i.__plus(i.chars.Count - 1)
            Next

            Return i
        End Operator

        ''' <summary>
        ''' 请注意，这个操作是线程不安全的，所以请确保在执行这个命令之前使用``SyncLock``加锁
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Shared Operator +(i As Uid) As Uid
            Call i.__plus(i.chars.Count - 1)
            Return i
        End Operator

        ''' <summary>
        ''' 直接字符串序列，不会产生步进前移
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return New String(
                chars.ToArray(Function(x) __chars(x)))
        End Function

        Public Shared Operator &(i As Uid, s$) As String
            Return i.ToString & s
        End Operator

        Public Shared Narrowing Operator CType(i As Uid) As String
            Return i.ToString
        End Operator
    End Class
End Namespace
