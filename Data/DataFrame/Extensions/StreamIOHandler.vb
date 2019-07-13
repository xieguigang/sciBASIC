#Region "Microsoft.VisualBasic::aaa42b0c45481e410d43d11f4e33f159, Data\DataFrame\Extensions\StreamIOHandler.vb"

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

    ' Module StreamIOHandler
    ' 
    '     Function: ISaveCsv, ISaveDataFrame, ISaveDataSet, ISaveEntitySet
    ' 
    '     Sub: __initStreamIO_pointer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Module StreamIOHandler

    ''' <summary>
    ''' 初始化函数指针，为``>>``语法提供csv流的支持
    ''' </summary>
    Friend Sub __initStreamIO_pointer()
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

    Public Function ISaveDataSet(source As IEnumerable(Of DataSet), path$, encoding As Encoding) As Boolean
        Return source.SaveTo(path, encoding:=encoding, layout:=New Dictionary(Of String, Integer) From {{NameOf(DataSet.ID), -999}})
    End Function

    Public Function ISaveEntitySet(source As IEnumerable(Of EntityObject), path$, encoding As Encoding) As Boolean
        Return source.SaveTo(path, encoding:=encoding, layout:=New Dictionary(Of String, Integer) From {{NameOf(EntityObject.ID), -999}})
    End Function

    Public Function ISaveCsv(source As File, path$, encoding As Encoding) As Boolean
        Return source.Save(path, encoding)
    End Function

    Public Function ISaveDataFrame(source As IEnumerable, path As String, encoding As Encoding) As Boolean
        Dim o As Object = (From x In source Select x).FirstOrDefault
        Dim type As Type = o.GetType

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call EchoLine($"[CSV.Reflector::{type.FullName}]" & vbCrLf & $"Save data to file:///{path}")
        Call Reflector.__save(source, type, False, Nothing).SaveDataFrame(path, encoding:=encoding)
        Call EchoLine("CSV saved!")

        Return True
    End Function
End Module
