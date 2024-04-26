#Region "Microsoft.VisualBasic::8b6820478ebbfdd13ca8f4548d5b7b12, sciBASIC#\Data_science\Mathematica\Math\DataFrame\DataFrame\DataFrame.vb"

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

    '   Total Lines: 75
    '    Code Lines: 56
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 2.29 KB


    ' Class DataFrame
    ' 
    '     Properties: dims, featureNames, features, nsamples, rownames
    ' 
    '     Function: delete, ToString, Union
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
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

    Default Public Property Item(featureName As String) As FeatureVector
        Get
            Return features(featureName)
        End Get
        Set
            features(featureName) = Value
        End Set
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function delete(featureName As String) As Boolean
        Return features.Remove(featureName)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub add(featureName As String, v As IEnumerable(Of Double))
        Call features.Add(featureName, New FeatureVector(featureName, v))
    End Sub

    Public Sub add(featureName As String, v As IEnumerable(Of Integer))
        Call features.Add(featureName, New FeatureVector(featureName, v))
    End Sub

    Public Function row(i As Integer) As Object()
        Return features.Select(Function(c) c.Value(i)).ToArray
    End Function

    Public Iterator Function foreachRow() As IEnumerable(Of NamedCollection(Of Object))
        Dim cols = features.Select(Function(c) c.Value.Getter).ToArray
        Dim nrow As Integer = rownames.Length

        For i As Integer = 0 To nrow - 1
#Disable Warning
            Yield New NamedCollection(Of Object)(rownames(i), cols.Select(Function(v) v(i)))
#Enable Warning
        Next
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
