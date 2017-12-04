#Region "Microsoft.VisualBasic::c6a60345905c5bad746e50908f70b7c2, ..\sciBASIC#\Data\DataFrame\Extensions\StreamIOHandler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Module StreamIOHandler

    ''' <summary>
    ''' 初始化函数指针，为``>>``语法提供csv流的支持
    ''' </summary>
    Public Sub __initStreamIO_pointer()
        Call IOHandler.SetHandle(AddressOf ISaveCsv)
        Call $"Default IO handle has been changes to {DefaultSaveDescription}...".__INFO_ECHO
    End Sub

    Public Function ISaveCsv(source As IEnumerable, path As String, encoding As Encoding) As Boolean
        Dim o As Object = (From x In source Select x).FirstOrDefault
        Dim type As Type = o.GetType

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call EchoLine($"[CSV.Reflector::{type.FullName}]" & vbCrLf & $"Save data to file:///{path}")
        Call Reflector.__save(source, type, False, Nothing).SaveDataFrame(path, encoding:=encoding)
        Call EchoLine("CSV saved!")

        Return True
    End Function
End Module
