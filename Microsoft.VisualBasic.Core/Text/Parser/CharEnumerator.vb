Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal

Namespace Text

    Public Class CharEnumerator : Inherits Pointer(Of Char)

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
            Call MyBase.New(data)
        End Sub
    End Class
End Namespace
