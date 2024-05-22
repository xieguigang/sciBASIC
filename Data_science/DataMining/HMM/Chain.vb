#Region "Microsoft.VisualBasic::ff6e6ce0ff44ac99345881ec94961ae3, Data_science\DataMining\HMM\Chain.vb"

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

    '   Total Lines: 58
    '    Code Lines: 34 (58.62%)
    ' Comment Lines: 14 (24.14%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 10 (17.24%)
    '     File Size: 1.63 KB


    ' Class Chain
    ' 
    '     Properties: length, obSequence
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: IndexOf, ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Class Chain

    ''' <summary>
    ''' 一般是在这里使用一个短的字符串来引用目标对象
    ''' </summary>
    ''' <returns></returns>
    Public Property obSequence As String()

    ''' <summary>
    ''' 可能会存在通过所引用的对象的相似度阈值来表示相等
    ''' </summary>
    Friend ReadOnly equalsTo As Func(Of String, String, Boolean)

    Default Public ReadOnly Property Item(i As Integer) As String
        Get
            Return _obSequence(i)
        End Get
    End Property

    Public ReadOnly Property length As Integer
        Get
            Return obSequence.Length
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="equalsTo">
    ''' 因为可能会存在通过所引用的对象的相似度阈值来表示相等
    ''' 所以在这里会多出来一个额外的函数来进行等价性计算
    ''' </param>
    Sub New(Optional equalsTo As Func(Of String, String, Boolean) = Nothing)
        If equalsTo Is Nothing Then
            Me.equalsTo = Function(str1, str2) str1 = str2
        Else
            Me.equalsTo = equalsTo
        End If
    End Sub

    Public Function IndexOf(obj As String) As Integer
        Dim i As Integer = 0

        For Each ref As String In obSequence
            If equalsTo(obj, ref) Then
                Return i
            End If

            i += 1
        Next

        Return -1
    End Function

    Public Overrides Function ToString() As String
        Return obSequence.JoinBy("->")
    End Function
End Class
