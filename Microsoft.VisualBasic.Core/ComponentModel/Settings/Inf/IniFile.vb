#Region "Microsoft.VisualBasic::d220e33675187244412f75bac45df92c, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\IniFile.vb"

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

    '     Class IniFile
    ' 
    '         Properties: FileExists, path
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadValue, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, Flush, WriteValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Ini file I/O handler
    ''' </summary>
    Public Class IniFile : Implements IDisposable

        Public ReadOnly Property path As String

        Public ReadOnly Property FileExists As Boolean
            Get
                Return path.FileLength > -1
            End Get
        End Property

        ''' <summary>
        ''' 为了避免频繁的读写文件，会使用这个数组来做缓存
        ''' </summary>
        Dim dataLines As String()

        ''' <summary>
        ''' Open a ini file handle.
        ''' </summary>
        ''' <param name="INIPath"></param>
        Public Sub New(INIPath As String)
            path = IO.Path.GetFullPath(PathMapper.GetMapPath(INIPath))
            dataLines = path.ReadAllLines
        End Sub

        ''' <summary>
        ''' 将缓存数据写入文件之中
        ''' </summary>
        Public Sub Flush()
            Call dataLines.SaveTo(path)
        End Sub

        Public Overrides Function ToString() As String
            Return path.ToFileURL
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteValue(section$, key$, value$)
            dataLines = dataLines.AsList.WritePrivateProfileString(section, key, value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadValue(section$, key$) As String
            Return dataLines.GetPrivateProfileString(section, key)
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
