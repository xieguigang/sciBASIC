#Region "Microsoft.VisualBasic::dbaf9467465590201c2ad01f9a9df81b, Microsoft.VisualBasic.Core\src\Extensions\IO\Path\TemporaryEnvironment.vb"

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

    '   Total Lines: 65
    '    Code Lines: 31 (47.69%)
    ' Comment Lines: 23 (35.38%)
    '    - Xml Docs: 34.78%
    ' 
    '   Blank Lines: 11 (16.92%)
    '     File Size: 2.35 KB


    '     Class TemporaryEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: makeDirectoryExists
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace FileIO

    ''' <summary>
    ''' Workspace model
    ''' 
    ''' 在<see cref="TemporaryEnvironment.Dispose()"/>的时候，文件夹会切换回来
    ''' </summary>
    Public Class TemporaryEnvironment : Inherits Directory
        Implements IDisposable

        ReadOnly previous$

        ''' <summary>
        ''' 如果不存在会创建目标文件夹
        ''' </summary>
        ''' <param name="newLocation"></param>
        Sub New(newLocation As String)
            Call MyBase.New(directory:=makeDirectoryExists(newLocation))

            previous = App.CurrentDirectory
            App.CurrentDirectory = newLocation
        End Sub

        Private Shared Function makeDirectoryExists(dir As String) As String
            If Not dir.DirectoryExists Then
                Call dir.MakeDir
            End If

            Return dir
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    App.CurrentDirectory = previous
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
