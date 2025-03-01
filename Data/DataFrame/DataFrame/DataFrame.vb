﻿#Region "Microsoft.VisualBasic::a8cc7a392f0ed3c071c2273658751166, Data_science\Mathematica\Math\DataFrame\DataFrame\DataFrame.vb"

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

'   Total Lines: 331
'    Code Lines: 193 (58.31%)
' Comment Lines: 95 (28.70%)
'    - Xml Docs: 92.63%
' 
'   Blank Lines: 43 (12.99%)
'     File Size: 11.45 KB


' Class DataFrame
' 
'     Properties: dims, featureNames, features, nfeatures, nsamples
'                 rownames
' 
'     Constructor: (+4 Overloads) Sub New
'     Function: (+3 Overloads) add, ArrayPack, delete, foreachRow, row
'               slice, ToString, Union
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' R language liked dataframe object
''' </summary>
Public Class DataFrame : Implements INumericMatrix

    ''' <summary>
    ''' the dataframe columns
    ''' </summary>
    ''' <returns></returns>
    Public Property features As New Dictionary(Of String, FeatureVector)

    ''' <summary>
    ''' get the row names labels in current dataframe object, the size of 
    ''' this row names vector should be equals to the number of rows in 
    ''' current dataframe object.
    ''' </summary>
    ''' <returns></returns>
    Public Property rownames As String()

    ''' <summary>
    ''' the dimension size of current dataframe object, with data axis dimension 
    ''' mapping of:
    ''' 
    ''' 1. width: feature size, column size
    ''' 2. height: sample size, row size
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property dims As Size
        Get
            Return New Size(width:=features.Count, height:=rownames.Length)
        End Get
    End Property

    ''' <summary>
    ''' the column field names
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property featureNames As String()
        Get
            Return features.Keys.ToArray
        End Get
    End Property

    ''' <summary>
    ''' the n rows of the matrix
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property nsamples As Integer
        Get
            Return rownames.Length
        End Get
    End Property

    ''' <summary>
    ''' get the number of the feature columns inside current dataframe object
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property nfeatures As Integer
        Get
            Return features.Count
        End Get
    End Property

    ''' <summary>
    ''' get a column field from dataframe by given feature column name
    ''' </summary>
    ''' <param name="featureName"></param>
    ''' <returns></returns>
    Default Public Property Item(featureName As String) As FeatureVector
        Get
            Return features(featureName)
        End Get
        Set
            features(featureName) = Value
        End Set
    End Property

    ''' <summary>
    ''' make dataframe column fields projection
    ''' </summary>
    ''' <param name="cols"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property Item(cols As IEnumerable(Of String)) As DataFrame
        Get
            Return New DataFrame With {
                .rownames = rownames.ToArray,
                .features = cols _
                    .ToDictionary(Function(c) c,
                                  Function(c)
                                      Return Me(c)
                                  End Function)
            }
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(list As Dictionary(Of String, Double()))
        features = New Dictionary(Of String, FeatureVector)

        For Each col In list.SafeQuery
            Call features.Add(col.Key, New FeatureVector(col.Key, col.Value))
        Next
    End Sub

    Sub New(ParamArray cols As (name As String, col As Array)())
        features = New Dictionary(Of String, FeatureVector)

        For Each col In cols
            Call features.Add(col.name, FeatureVector.FromGeneral(col.name, col.col))
        Next
    End Sub

    Sub New(cols As IEnumerable(Of FeatureVector), labels As IEnumerable(Of String))
        For Each col As FeatureVector In cols
            Call features.Add(col.name, col)
        Next

        If labels IsNot Nothing Then
            rownames = labels.ToArray
        End If
    End Sub

    ''' <summary>
    ''' removes a column field from current dataframe object by a given field name
    ''' </summary>
    ''' <param name="featureName"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function delete(featureName As String) As Boolean
        Return features.Remove(featureName)
    End Function

    ''' <summary>
    ''' add a new feature column into current dataframe object
    ''' </summary>
    ''' <param name="featureName"></param>
    ''' <param name="v">
    ''' a data field column data in double numeric type
    ''' </param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function add(featureName As String, v As IEnumerable(Of Double)) As DataFrame
        Call features.Add(featureName, New FeatureVector(featureName, v))
        Return Me
    End Function

    ''' <summary>
    ''' add a new feature column into current dataframe object
    ''' </summary>
    ''' <param name="featureName"></param>
    ''' <param name="v">
    ''' a data field column data in integer type
    ''' </param>
    Public Function add(featureName As String, v As IEnumerable(Of Integer)) As DataFrame
        Call features.Add(featureName, New FeatureVector(featureName, v))
        Return Me
    End Function

    ''' <summary>
    ''' add a new feature column into current dataframe object
    ''' </summary>
    ''' <param name="feature"></param>
    Public Function add(feature As FeatureVector) As DataFrame
        Call features.Add(feature.name, feature)
        Return Me
    End Function

    ''' <summary>
    ''' get row by index
    ''' </summary>
    ''' <param name="i"></param>
    ''' <returns></returns>
    Public Function row(i As Integer) As Object()
        Return features.Select(Function(c) c.Value(i)).ToArray
    End Function

    ''' <summary>
    ''' iterates through all data rows inside current dataframe object
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function foreachRow() As IEnumerable(Of NamedCollection(Of Object))
        Dim cols = features.Select(Function(c) c.Value.Getter).ToArray
        Dim nrow As Integer = rownames.Length

        For i As Integer = 0 To nrow - 1
#Disable Warning
            Yield New NamedCollection(Of Object)(
                name:=rownames(i),
                data:=cols.Select(Function(v) v(i))
            )
#Enable Warning
        Next
    End Function

    ''' <summary>
    ''' slice the dataframe row by a collection of the given row names labels
    ''' </summary>
    ''' <param name="rownames"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function could be used for make dataframe rows re-order by rownames
    ''' </remarks>
    Public Function slice(rownames As IEnumerable(Of String)) As DataFrame
        Dim rowIndex As Index(Of String) = rownames.Indexing

        If rowIndex.Count = 0 Then
            Return Nothing
        End If

        Dim offset As Integer
        Dim cols = features.Select(Function(c) c.Value.Getter).ToArray
        Dim nrow As Integer = Me.rownames.Length
        Dim rowname As String
        Dim rowcopy As NamedCollection(Of Object)() = New NamedCollection(Of Object)(rowIndex.Count - 1) {}
        Dim rowdata As Object()

        For i As Integer = 0 To nrow - 1
            rowname = Me.rownames(i)
            offset = i

            If rowIndex.IndexOf(rowname) > -1 Then
                rowdata = cols.Select(Function(c) c(offset)).ToArray
                rowcopy(rowIndex.IndexOf(rowname)) = New NamedCollection(Of Object)(rowname, rowdata)
            End If
        Next

        For i As Integer = 0 To rowcopy.Length - 1
            If rowcopy(i).value Is Nothing Then
                Throw New MissingPrimaryKeyException($"missing the row data which its name is: '{rowIndex.Objects(i)}'! This is your rownames of the dataframe: {Me.rownames.JoinBy("; ")}.")
            End If
        Next

        Dim featureCols As New Dictionary(Of String, FeatureVector)
        Dim featureNames As String() = features.Keys.ToArray
        Dim vec As FeatureVector
        Dim dataArray As Array
        Dim firstValue As Object

        For i As Integer = 0 To featureNames.Length - 1
            offset = i
            dataArray = rowcopy.Select(Function(r) r(offset)).ToArray
            firstValue = (From xi As Object In dataArray Where Not xi Is Nothing).FirstOrDefault

            If Not firstValue Is Nothing Then
                dataArray = CreateArray(dataArray, dataArray(0).GetType)
            Else
                ' 20240428 all is nothing
                '
                ' cast to string array by default
                ' or the FeatureVector.FromGeneral function will throw error 
                ' due to the reason of dataarray default is an object array
                ' object array is not supported 
                dataArray = New String(dataArray.Length - 1) {}
            End If

            vec = FeatureVector.FromGeneral(featureNames(i), dataArray)
            featureCols.Add(featureNames(i), vec)
        Next

        Return New DataFrame With {
            .rownames = rowcopy _
                .Select(Function(r) r.name) _
                .ToArray,
            .features = featureCols
        }
    End Function

    ''' <summary>
    ''' current dataframe object append the additional data 
    ''' to right side and then create a new dataframe
    ''' </summary>
    ''' <param name="append"></param>
    ''' <returns></returns>
    Public Function Union(append As DataFrame) As DataFrame
        If append.nsamples <> nsamples Then
            Throw New InvalidConstraintException("the sample number between two matrix is not agree!")
        End If

        Dim join As New Dictionary(Of String, FeatureVector)(features)

        For Each key As String In append.featureNames
            Call join.Add(key, append(featureName:=key))
        Next

        Return New DataFrame With {
            .features = join,
            .rownames = rownames.ToArray
        }
    End Function

    Public Overrides Function ToString() As String
        Dim size As Size = dims
        Dim featureSet As String = features _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Value.type.Name.ToLower
                          End Function) _
            .GetJson

        Return $"[{size.Width}x{size.Height}] {featureSet}"
    End Function

    Public Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements INumericMatrix.ArrayPack
        Dim m As Double()() = New Double(nsamples - 1)() {}
        Dim colnames As String() = featureNames
        Dim getters As Func(Of Integer, Double)() = colnames _
            .Select(Function(name) features(name).NumericGetter) _
            .ToArray
        Dim offset As Integer

        For i As Integer = 0 To m.Length - 1
            offset = i
            m(i) = getters _
                .Select(Function(v) v(offset)) _
                .ToArray
        Next

        Return m
    End Function
End Class
