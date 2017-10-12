#Region "Microsoft.VisualBasic::73272ea5d5512347cada1a8d47b14208, ..\sciBASIC#\Data\DataFrame\DATA\DataFrame.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace DATA

    ''' <summary>
    ''' 提供了类似于R语言之中的``cbind``操作类似的按照列进行数据框合并的方法
    ''' </summary>
    Public Class DataFrame : Implements IEnumerable(Of EntityObject)

        ''' <summary>
        ''' Row data in the csv table
        ''' </summary>
        Dim entityList As Dictionary(Of EntityObject)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(list As IEnumerable(Of EntityObject))
            entityList = list.ToDictionary
        End Sub

        ''' <summary>
        ''' Convert row object as target .NET object
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of T As Class)() As T()
            Return entityList.Values _
                .ToCsvDoc _
                .AsDataSource(Of T)
        End Function

        ''' <summary>
        ''' Get all entity keys
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return entityList.Keys.ToArray.GetJson
        End Function

        ''' <summary>
        ''' Save as csv file.
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveTable(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
            Return entityList.Values.SaveTo(path, encoding:=encoding.CodePage, strict:=False)
        End Function

        ''' <summary>
        ''' Load from a csv file
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Load(path$, Optional encoding As Encodings = Encodings.Default) As DataFrame
            Return New DataFrame(EntityObject.LoadDataSet(path))
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of EntityObject) Implements IEnumerable(Of EntityObject).GetEnumerator
            For Each x In entityList.Values
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' ``cbind`` operation
        ''' </summary>
        ''' <param name="data">unique</param>
        ''' <param name="appends">multiple</param>
        ''' <returns></returns>
        Public Shared Operator +(data As DataFrame, appends As IEnumerable(Of EntityObject)) As DataFrame
            For Each x As EntityObject In appends
                If data.entityList.ContainsKey(x.ID) Then
                    With data.entityList(x.ID)
                        For Each [property] In x.Properties
                            .ItemValue([property].Key) = [property].Value
                        Next
                    End With
                Else
                    data.entityList += x
                End If
            Next

            Return data
        End Operator

        Public Shared Function Append(multiple As IEnumerable(Of EntityObject), unique As DataFrame) As IEnumerable(Of EntityObject)
            Return multiple _
                .Select(Function(query)
                            If Not unique.entityList.ContainsKey(query.ID) Then
                                Return query
                            Else
                                With unique.entityList(query.ID)
                                    For Each [property] In .Properties
                                        query.Properties([property].Key) = [property].Value
                                    Next
                                End With

                                Return query
                            End If
                        End Function)
        End Function
    End Class
End Namespace
