#Region "Microsoft.VisualBasic::b2b99777c0624840b092e0b6ad899e15, Data\DataFrame\Extensions\StreamIOHandler.vb"

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

    '   Total Lines: 54
    '    Code Lines: 40 (74.07%)
    ' Comment Lines: 3 (5.56%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (20.37%)
    '     File Size: 2.73 KB


    ' Module StreamIOHandler
    ' 
    '     Function: ISaveCsv, ISaveDataFrame, ISaveDataSet, ISaveEntitySet
    ' 
    '     Sub: initStreamIOHandlers
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.My.FrameworkInternal

Module StreamIOHandler

    ''' <summary>
    ''' 初始化函数指针，为``>>``语法提供csv流的支持
    ''' </summary>
    Friend Sub initStreamIOHandlers()
        Call IOHandler.RegisterHandle(AddressOf ISaveDataFrame, GetType(IEnumerable))
        Call IOHandler.RegisterHandle(AddressOf ISaveCsv, GetType(File))
        Call IOHandler.RegisterHandle(AddressOf ISaveCsv, GetType(DataFrame))

        Call IOHandler.RegisterHandle(AddressOf ISaveDataSet, GetType(IEnumerable(Of DataSet)))
        Call IOHandler.RegisterHandle(AddressOf ISaveDataSet, GetType(DataSet()))
        Call IOHandler.RegisterHandle(AddressOf ISaveDataSet, GetType(List(Of DataSet)))

        Call IOHandler.RegisterHandle(AddressOf ISaveEntitySet, GetType(IEnumerable(Of EntityObject)))
        Call IOHandler.RegisterHandle(AddressOf ISaveEntitySet, GetType(EntityObject()))
        Call IOHandler.RegisterHandle(AddressOf ISaveEntitySet, GetType(List(Of EntityObject)))
    End Sub

    Public Function ISaveDataSet(source As IEnumerable, path$, encoding As Encoding) As Boolean
        Return DirectCast(source, IEnumerable(Of DataSet)).SaveTo(path, encoding:=encoding, layout:=New Dictionary(Of String, Integer) From {{NameOf(DataSet.ID), -999}})
    End Function

    Public Function ISaveEntitySet(source As IEnumerable, path$, encoding As Encoding) As Boolean
        Return DirectCast(source, IEnumerable(Of EntityObject)).SaveTo(path, encoding:=encoding, layout:=New Dictionary(Of String, Integer) From {{NameOf(EntityObject.ID), -999}})
    End Function

    Public Function ISaveCsv(source As IEnumerable, path$, encoding As Encoding) As Boolean
        If TypeOf source Is File Then
            Return DirectCast(source, File).Save(path, encoding)
        Else
            Return DirectCast(source, DataFrame).csv.Save(path, encoding)
        End If
    End Function

    Public Function ISaveDataFrame(source As IEnumerable, path As String, encoding As Encoding) As Boolean
        Dim o As Object = (From x In source Select x).FirstOrDefault
        Dim type As Type = o.GetType

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call EchoLine($"[CSV.Reflector::{type.FullName}]" & vbCrLf & $"Save data to file:///{path}")
        Call Reflector.doSave(source, type, False, Nothing).SaveDataFrame(path, encoding:=encoding)
        Call EchoLine("CSV saved!")

        Return True
    End Function
End Module
