#Region "Microsoft.VisualBasic::66694617b20c7369b1e33346e177bfe2, Microsoft.VisualBasic.Core\Language\Language\Python\Collection.vb"

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

    '     Module Collection
    ' 
    '         Function: slice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Module Collection

        ''' <summary>
        ''' 将序列之中的指定区域的内容取出来
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="[set]"></param>
        ''' <param name="start"></param>
        ''' <param name="[stop]">
        ''' 可以接受负数，如果为负数，则表示终止的下标为长度将去这个stop值的结果，即从后往前数
        ''' </param>
        ''' <param name="[step]"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function slice(Of T)([set] As IEnumerable(Of T),
                                             Optional start% = 0,
                                             Optional stop% = -1,
                                             Optional step% = 1) As IEnumerable(Of T)
            Dim array As T()

            If start = 0 AndAlso [stop] = -1 Then
                ' [:] 所有的参数都被忽略掉了，返回序列的拷贝
                For Each x In [set]
                    Yield x
                Next

                Return
            ElseIf start = 0 Then
                array = [set].ToArray
            Else
                If start < 0 Then
                    array = [set].ToArray
                    start = array.Length + start
                    array = array.Skip(start).ToArray
                Else
                    array = [set].Skip(start).ToArray
                End If
            End If

            If [stop] < 0 Then
                [stop] = array.Length + [stop]
            End If

            [stop] -= start
            [stop] -= 1

            For i As Integer = 0 To [stop] Step [step]
                Yield array(i)
            Next
        End Function
    End Module
End Namespace
