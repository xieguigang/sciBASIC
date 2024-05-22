#Region "Microsoft.VisualBasic::351853de2326233d3504aeb74e280f20, Data_science\Mathematica\Math\Math\Scripting\VectorModel.vb"

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

    '   Total Lines: 106
    '    Code Lines: 76 (71.70%)
    ' Comment Lines: 17 (16.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (12.26%)
    '     File Size: 4.36 KB


    '     Class IVector
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetVector, readInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Scripting

    Public Class IVector(Of T) : Inherits VectorShadows(Of T)

        ''' <summary>
        ''' <paramref name="name"/>大小写不敏感
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(name$) As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return readInternal(name)
            End Get
            Set(value As Vector)
                Dim writer As PropertyInfo = type.TryGetMember(name, caseSensitive:=False)

                Select Case writer.PropertyType
                    Case GetType(Double)
                        MyBase.Item(name) = value
                    Case GetType(Single)
                        MyBase.Item(name) = value.AsSingle
                    Case GetType(Integer)
                        MyBase.Item(name) = value.AsInteger
                    Case Else
                        Throw New NotImplementedException(writer.PropertyType.ToString)
                End Select
            End Set
        End Property

        Private Function readInternal(name As String) As Vector
            Dim v As Object = Nothing

            If Not TryGetMember(name, result:=v) Then
                Throw New EntryPointNotFoundException(name)
            Else
                Return GetVector(v)
            End If
        End Function

        Public Shared Function GetVector(v As Object) As Vector
            Select Case v.GetType
                Case GetType(VectorShadows(Of Double))
                    Return DirectCast(v, VectorShadows(Of Double)).AsVector
                Case GetType(VectorShadows(Of Single))
                    Return DirectCast(v, VectorShadows(Of Single)).Select(Function(x) CDbl(x)).AsVector
                Case GetType(VectorShadows(Of Integer))
                    Return DirectCast(v, VectorShadows(Of Integer)).Select(Function(x) CDbl(x)).AsVector
                Case GetType(VectorShadows(Of Long))
                    Return DirectCast(v, VectorShadows(Of Long)).Select(Function(x) CDbl(x)).AsVector
                Case GetType(VectorShadows(Of Boolean))
                    Return DirectCast(v, VectorShadows(Of Boolean)).Select(Function(x) CDbl(x)).AsVector
                Case GetType(VectorShadows(Of Char))
                    Return DirectCast(v, VectorShadows(Of Char)).CharCodes.Select(Function(x) CDbl(x)).AsVector
                Case GetType(VectorShadows(Of String))
                    Return DirectCast(v, VectorShadows(Of String)).Select(Function(s) s.ParseDouble).AsVector

                Case Else

                    Throw New NotSupportedException(v.GetType.FullName)

            End Select
        End Function

        ''' <summary>
        ''' 按照索引编号来取出元素
        ''' </summary>
        ''' <param name="index">
        ''' 目标在序列之中的位置索引编号，即数组的下标
        ''' </param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                buffer(index) = value
            End Set
        End Property

        ''' <summary>
        ''' 这个属性返回来一个新的向量子集
        ''' </summary>
        ''' <param name="booleans"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item(booleans As IEnumerable(Of Boolean)) As IVector(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New IVector(Of T)(Subset(booleans))
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(source)
        End Sub
    End Class
End Namespace
