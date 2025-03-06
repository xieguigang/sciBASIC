#Region "Microsoft.VisualBasic::f4f47a056528b8100e0a6a84eca82b1f, Data\DataFrame\DataSet\Excel\Extensions.vb"

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

    '   Total Lines: 93
    '    Code Lines: 48 (51.61%)
    ' Comment Lines: 34 (36.56%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 11 (11.83%)
    '     File Size: 3.63 KB


    '     Module Extensions
    ' 
    '         Function: LoadDataSet, LoadEntitySet, (+2 Overloads) ReadXlsx
    ' 
    '         Sub: doUpdateMaps
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET48 Then

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework.IO

Namespace Excel

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' Load data frame from the excel file.
        ''' </summary>
        ''' <param name="file$">*.xlsx file path.</param>
        ''' <param name="sheetName">Table name</param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadXlsx(file$, Optional sheetName As String = "Sheet1") As DataFrameResolver
            Dim reader As New ExcelReader(file.GetFullPath, True, True)
            Dim data As DataTable = reader.GetWorksheet(sheetName)
            Dim df As DataFrameResolver = data.CreateDataReader.DataFrame
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
                                                Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)
            Dim df As DataFrameResolver = file.ReadXlsx(sheetName)
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
                                    Optional uidMaps As String = Nothing) As IEnumerable(Of DataSet)
            Dim df As DataFrameResolver = file.ReadXlsx(sheetName)
            Call df.doUpdateMaps(uidMaps)
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
                                      Optional uidMaps As String = Nothing) As IEnumerable(Of EntityObject)
            Dim df As DataFrameResolver = file.ReadXlsx(sheetName)
            Call df.doUpdateMaps(uidMaps)
            Return df.AsDataSource(Of EntityObject)(False)
        End Function

        ''' <summary>
        ''' 仅限于 <see cref="DataSet"/>和<see cref="EntityObject"/>
        ''' </summary>
        ''' <param name="df"></param>
        ''' <param name="mapName"></param>
        ''' 
        <Extension>
        Private Sub doUpdateMaps(ByRef df As DataFrameResolver, mapName$)
            If String.IsNullOrEmpty(mapName) Then
                mapName = df.HeadTitles(Scan0)
            End If

            Dim maps As New Dictionary(Of String, String) From {
                {mapName, NameOf(DataSet.ID)}
            }

            Call df.ChangeMapping(maps)
        End Sub
    End Module
End Namespace

#End If
