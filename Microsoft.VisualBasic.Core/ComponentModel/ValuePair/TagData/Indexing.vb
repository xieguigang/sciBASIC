#Region "Microsoft.VisualBasic::5fde000ea39b59dbe36acf5e98f7f01c, Microsoft.VisualBasic.Core\ComponentModel\ValuePair\TagData\Indexing.vb"

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

    '     Module IndexingExtensions
    ' 
    '         Function: (+2 Overloads) BinarySearch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.TagData

    Public Module IndexingExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(source As IEnumerable(Of T),
                                                                   key As K,
                                                                   getKey As Func(Of T, K),
                                                                   Optional [default] As T = Nothing) As T

            Return source _
                .OrderBy(getKey) _
                .ToArray _
                .BinarySearch(key, getKey, [default])
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="inputArray">传入的数组必须是经过升序排序的</param>
        ''' <param name="key"></param>
        ''' <param name="getKey"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(inputArray As T(), key As K, getKey As Func(Of T, K), Optional [default] As T = Nothing) As T
            Dim min = 0
            Dim max = inputArray.Length - 1
            Dim mid%

            Do While min <= max
                [mid] = (min + max) / 2

                If key.CompareTo(getKey(inputArray(mid))) = 0 Then
                    Return inputArray(mid)
                ElseIf key.CompareTo(getKey(inputArray(mid))) < 0 Then
                    max = mid - 1
                Else
                    min = mid + 1
                End If
            Loop

            Return [default]
        End Function
    End Module
End Namespace
