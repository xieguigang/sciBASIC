#Region "Microsoft.VisualBasic::f1cab688fa1d486d7d88ead8f5676590, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Uid.vb"

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

Imports Microsoft.VisualBasic.Linq

Namespace Language

    Public Class Uid

        Dim chars As List(Of Integer)

        ReadOnly __chars As Char() = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        ReadOnly __upbound As Integer = __chars.Length - 1

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

        Public Shared Operator +(i As Uid, n As Integer) As Uid
            For o As Integer = 0 To n - 1
                Call i.__plus(i.chars.Count - 1)
            Next

            Return i
        End Operator

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

        Public Shared Narrowing Operator CType(i As Uid) As String
            Return i.ToString
        End Operator
    End Class
End Namespace
