#Region "Microsoft.VisualBasic::e7aa9d40f3cf288d0874301c169c301f, ..\visualbasic_App\Data\DataFrame\Excel\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.DocumentStream

Namespace Excel

    Public Module Extensions

        <Extension>
        Public Function ReadXlsx(file As String, Optional sheetName As String = "Sheet1") As DocumentStream.DataFrame
            Dim reader As New ExcelReader(file.GetFullPath, True, True)
            Dim data As DataTable = reader.GetWorksheet(sheetName)
            Dim df As DocumentStream.DataFrame = data.CreateDataReader.DataFrame
            Return df
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="file"></param>
        ''' <param name="sheetName"></param>
        ''' <param name="maps">Field(Csv) -> Class.Property Name</param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadXlsx(Of T As Class)(file As String,
                                                Optional sheetName As String = "Sheet1",
                                                Optional maps As Dictionary(Of String, String) = Nothing) As T()
            Dim df As DataFrame = file.ReadXlsx(sheetName)
            Return df.AsDataSource(Of T)(False, maps)
        End Function

        ''' <summary>
        ''' <see cref="DataSet"/>
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="sheetName"></param>
        ''' <param name="uidMaps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadDataSet(file As String,
                                    Optional sheetName As String = "Sheet1",
                                    Optional uidMaps As String = Nothing) As DataSet()
            Dim df As DataFrame = file.ReadXlsx(sheetName)
            Call df.__updateMaps(uidMaps)
            Return df.AsDataSource(Of DataSet)(False)
        End Function

        ''' <summary>
        ''' <see cref="EntityObject"/>
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="sheetName"></param>
        ''' <param name="uidMaps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadEntitySet(file As String,
                                      Optional sheetName As String = "Sheet1",
                                      Optional uidMaps As String = Nothing) As EntityObject()
            Dim df As DataFrame = file.ReadXlsx(sheetName)
            Call df.__updateMaps(uidMaps)
            Return df.AsDataSource(Of EntityObject)(False)
        End Function

        ''' <summary>
        ''' 仅限于 <see cref="DataSet"/>和<see cref="EntityObject"/>
        ''' </summary>
        ''' <param name="df"></param>
        ''' <param name="mapName"></param>
        ''' 
        <Extension>
        Private Sub __updateMaps(df As DataFrame, mapName As String)
            If String.IsNullOrEmpty(mapName) Then
                mapName = df.HeadTitles(Scan0)
            End If

            Dim maps As New Dictionary(Of String, String) From {
                {mapName, NameOf(DataSet.Identifier)}
            }

            Call df.ChangeMapping(maps)
        End Sub
    End Module
End Namespace
