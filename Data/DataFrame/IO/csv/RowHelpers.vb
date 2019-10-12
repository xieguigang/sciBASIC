Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace IO

    Public Module RowHelpers

        ''' <summary>
        ''' 获取一个数据行对象的游标操作符对象
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Pointer(row As RowObject) As Pointer(Of String)
            Return New Pointer(Of String)(row)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s">The cell value text</param>
        ''' <param name="deli">The value row delimiter</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Friend Function doDelimiterMask(s$, deli As Char) As String
            If String.IsNullOrEmpty(s) Then
                Return ""
            Else
                s = s.Replace("""", """""")
            End If

            ' 双引号可以转义换行
            If s.IndexOf(deli) > -1 OrElse
                s.IndexOf(ASCII.LF) > -1 OrElse
                s.IndexOf(ASCII.CR) > -1 Then

                Return $"""{s}"""
            Else
                Return s
            End If
        End Function

        ''' <summary>
        ''' 去除行集合中的重复的数据行
        ''' </summary>
        ''' <param name="rowList"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public Iterator Function Distinct(rowList As IEnumerable(Of RowObject)) As IEnumerable(Of RowObject)
            Dim source As IEnumerable(Of String) = From row In rowList
                                                   Let rowLine As String = CType(row, String)
                                                   Select rowLine
                                                   Distinct
                                                   Order By rowLine Ascending
            For Each line As String In source
                Yield New RowObject(line)
            Next
        End Function

        ''' <summary>
        ''' Is this row object contains any data?
        ''' </summary>
        ''' <param name="countAllEmpty">
        ''' All of the cell value which is empty string 
        ''' is also count as empty if this parameter is ``TRUE``.
        ''' </param>
        ''' <returns></returns>
        Public Function IsNullOrEmpty(row As RowObject, Optional countAllEmpty As Boolean = True) As Boolean
            If row Is Nothing OrElse row.buffer.Count = 0 Then
                Return True
            ElseIf Not countAllEmpty Then
                Return False
            End If

            ' all of the cell value which is empty string is also count as empty
            Dim LQuery = LinqAPI.DefaultFirst(Of Integer) _
 _
                () <= From colum As String
                      In row.buffer
                      Where Len(Strings.Trim(colum)) > 0
                      Select 100

            Return Not LQuery > 50
        End Function
    End Module
End Namespace