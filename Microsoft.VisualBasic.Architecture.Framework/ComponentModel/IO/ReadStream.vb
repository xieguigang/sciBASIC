#Region "Microsoft.VisualBasic::9f26531ddd9c78d3e984b007f6127b54, ..\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\IO\ReadStream.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
