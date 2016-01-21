'Imports System.IO

'Namespace ComponentModel.IO

'    Public Delegate Function LoadObject(Of T)(source As String) As T
'    ''' <summary>
'    ''' 由于程序是按行读取的，所以这个应该是判断当前行是否为分割的行
'    ''' </summary>
'    ''' <param name="line"></param>
'    ''' <returns></returns>
'    Public Delegate Function IsDelimiter(line As String) As Boolean

'    ''' <summary>
'    ''' 按行读取的
'    ''' </summary>
'    ''' <typeparam name="T"></typeparam>
'    Public Class ReadStream(Of T)

'        ReadOnly __reader As System.IO.StreamReader
'        ReadOnly __loadObject As LoadObject(Of T)
'        ReadOnly __isDelimiter As IsDelimiter

'        Sub New(handle As String, loadObject As LoadObject(Of T), isDelimiter As IsDelimiter, Optional encoding As Encodings = Encodings.UTF8)
'            __reader = New StreamReader(handle, encoding.GetEncodings)
'            __loadObject = loadObject
'            __isDelimiter = isDelimiter
'        End Sub

'    End Class
'End Namespace