#Region "Microsoft.VisualBasic::351853de2326233d3504aeb74e280f20, Data_science\Mathematica\Math\Math\Scripting\VectorModel.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
