#Region "Microsoft.VisualBasic::774724e2dbab67a1f5d15eb8e532d4a7, ..\sciBASIC#\Data\DataFrame\Extensions\StreamIOHandler.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.FileIO

Module StreamIOHandler

    ''' <summary>
    ''' 初始化函数指针，为``>>``语法提供csv流的支持
    ''' </summary>
    Public Sub __initStreamIO_pointer()
        Call CollectionIO.SetHandle(AddressOf ISaveCsv)
        Call VBDebugger.Warning($"Default IO handle has been changes to {DefaultSaveDescription}...")
    End Sub

    Public Function ISaveCsv(source As IEnumerable, path As String, encoding As Encoding) As Boolean
        Dim o As Object = (From x In source Select x).FirstOrDefault
        Dim type As Type = o.GetType

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call Console.WriteLine("[CSV.Reflector::{0}]" & vbCrLf & "Save data to file:///{1}", type.FullName, path)
        Call Reflector.__save(source, type, False, Nothing).SaveDataFrame(path, encoding:=encoding)
        Call Console.WriteLine("CSV saved!")

        Return True
    End Function
End Module
