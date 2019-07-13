#Region "Microsoft.VisualBasic::9419b8655e218d4b7f1b8f381fa51413, Microsoft.VisualBasic.Core\Text\Parser\CharEnumerator.vb"

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

    '     Class CharPtr
    ' 
    '         Properties: Remaining
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq

Namespace Text.Parser

    Public Class CharPtr : Inherits Pointer(Of Char)

        ''' <summary>
        ''' 查看还有多少字符没有被处理完
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Remaining As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Skip(index).CharString
            End Get
        End Property

        Sub New(data As String)
            ' 可能会存在空字符串
            Call MyBase.New(data.SafeQuery)
        End Sub

        ''' <summary>
        ''' 这个调试试图函数会将当前的读取位置给标记出来
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim previous$ = buffer.Take(index).CharString
            Dim current As Char = Me.Current
            Dim remaining$ = Me.Remaining

            Return $"{previous} ->[{current}]<- {remaining}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(str As String) As CharPtr
            Return New CharPtr(str)
        End Operator
    End Class
End Namespace
