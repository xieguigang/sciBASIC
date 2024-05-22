#Region "Microsoft.VisualBasic::31f272cd959d1eeede333a11c7271181, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\IniFile.vb"

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

    '   Total Lines: 135
    '    Code Lines: 81 (60.00%)
    ' Comment Lines: 36 (26.67%)
    '    - Xml Docs: 52.78%
    ' 
    '   Blank Lines: 18 (13.33%)
    '     File Size: 5.07 KB


    '     Class IniFile
    ' 
    '         Properties: FileExists, path
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadValue, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, Flush, WriteComment, WriteValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.UNIX

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Ini file I/O handler
    ''' </summary>
    Public Class IniFile : Implements IDisposable

        Public ReadOnly Property path As String

        Public ReadOnly Property FileExists As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return path.FileLength > -1
            End Get
        End Property

        ''' <summary>
        ''' 为了避免频繁的读写文件，会使用这个数组来做缓存
        ''' </summary>
        Dim data As Dictionary(Of String, Section)

        ''' <summary>
        ''' Open a ini file handle.
        ''' </summary>
        ''' <param name="INIPath"></param>
        Public Sub New(INIPath As String)
            path = IO.Path.GetFullPath(PathMapper.GetMapPath(INIPath))
            data = INIProfile _
                .PopulateSections(path) _
                .ToDictionary(Function(s)
                                  Return s.Name
                              End Function)
        End Sub

        ''' <summary>
        ''' 将缓存数据写入文件之中
        ''' </summary>
        Public Sub Flush()
            Using write As StreamWriter = path.OpenWriter
                For Each section As Section In data.Values
                    Call write.WriteLine(section.CreateDocFragment)
                Next
            End Using
        End Sub

        Public Overrides Function ToString() As String
            Return path.ToFileURL
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteValue(section$, key$, value$, Optional comments$ = Nothing)
            If Not data.ContainsKey(section) Then
                data(section) = New Section With {
                    .Name = section,
                    .Items = {}
                }
            End If

            data(section).SetValue(key, value, comments)
        End Sub

        ''' <summary>
        ''' 在给定的section,key上面写入注释
        ''' </summary>
        ''' <param name="section"></param>
        ''' <param name="key">如果这个键名称不存在的话，则是将注释写入到目标<paramref name="section"/>之中的</param>
        ''' <param name="comment">不需要添加注释符号,函数会自动添加</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteComment(section$, comment$, Optional key$ = Nothing)
            ' section和key都不存在的话，则找不到写入注释的位置
            If Not data.ContainsKey(section) Then
                Return
            Else
                If key.StringEmpty Then
                    ' 当key是空字符串，则将comment写在section之中
                    data(section).Comment = comment
                    Return
                Else
                    If Not data(section).Have(key) Then
                        Return
                    End If
                End If
            End If

            data(section).SetComments(key, comment)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadValue(section$, key$, Optional default$ = Nothing) As String
            If data.ContainsKey(section) Then
                Return data(section).GetValue(key, default$)
            Else
                Return [default]
            End If
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Flush()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
