#Region "Microsoft.VisualBasic::7a3156fb0868cd0be2c2da152b5023fc, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\SlideWindow\SlideWindow.vb"

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

    '   Total Lines: 101
    '    Code Lines: 60
    ' Comment Lines: 28
    '   Blank Lines: 13
    '     File Size: 3.59 KB


    '     Structure SlideWindow
    ' 
    '         Properties: Index, Items, left, Length, right
    ' 
    '         Function: GetEnumerator, GetEnumerator1, ToString
    ' 
    '         Sub: Assign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' A slide window data model.(滑窗操作的数据模型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Structure SlideWindow(Of T)
        Implements IEnumerable(Of T), IAddressOf
        Implements IGrouping(Of Integer, T)
        Implements Value(Of T()).IValueOf
        Implements IRange(Of Integer)

        ''' <summary>
        ''' The position of the current Windows in the Windows list.(在创建的滑窗的队列之中当前的窗口对象的位置)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Index As Integer Implements IAddressOf.Address, IGrouping(Of Integer, T).Key
        ''' <summary>
        ''' The elements in this slide window.(这个划窗之中的元素的列表)
        ''' </summary>
        ''' <returns></returns>
        Public Property Items As T() Implements Value(Of T()).IValueOf.Value

#Region "Index Range"

        Default Public ReadOnly Property Item(index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If index < 0 Then
                    Return Items(Length + index)
                Else
                    Return Items(index)
                End If
            End Get
        End Property

        ''' <summary>
        ''' The left start position of the current slide Windows segment on the original sequence.
        ''' (当前窗口在原始的序列之中的左端起始位点)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property left As Integer Implements IRange(Of Integer).Min

        Public ReadOnly Property right As Integer Implements IRange(Of Integer).Max
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return left + Length
            End Get
        End Property

#End Region

        ''' <summary>
        ''' The length of the slide window.(窗口长度)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            <DebuggerStepThrough>
            Get
                If Items.IsNullOrEmpty Then
                    Return 0
                Else
                    Return Items.Length
                End If
            End Get
        End Property

        <DebuggerStepThrough>
        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            Index = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Index} --> {Items.GetJson}"
        End Function

        <DebuggerStepThrough>
        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each o As T In Items
                Yield o
            Next
        End Function

        <DebuggerStepThrough>
        Private Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure
End Namespace
