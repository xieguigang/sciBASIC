#Region "Microsoft.VisualBasic::b698da8c3a786595ef3d996ed0d8374d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Algorithm\base\SlideWindow\SlideWindowHandle.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' A slide window data model.(滑窗操作的数据模型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Structure SlideWindowHandle(Of T)
        Implements IEnumerable(Of T), IAddressHandle
        Implements IGrouping(Of Integer, T)

        ''' <summary>
        ''' The position of the current Windows in the Windows list.(在创建的滑窗的队列之中当前的窗口对象的位置)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property p As Integer Implements IAddressHandle.Address, IGrouping(Of Integer, T).Key
        ''' <summary>
        ''' The elements in this slide window.(这个划窗之中的元素的列表)
        ''' </summary>
        ''' <returns></returns>
        Public Property Elements As T()

        ''' <summary>
        ''' The left start position of the current slide Windows segment on the original sequence.
        ''' (当前窗口在原始的序列之中的左端起始位点)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Left As Integer

        Public ReadOnly Property Right As Integer
            Get
                Return Left + Length
            End Get
        End Property

        ''' <summary>
        ''' The length of the slide window.(窗口长度)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            Get
                If Elements.IsNullOrEmpty Then
                    Return 0
                Else
                    Return Elements.Length
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{p} --> {Elements.GetJson}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each element As T In Elements
                Yield element
            Next
        End Function

        Private Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            Elements = Nothing
        End Sub
    End Structure
End Namespace
