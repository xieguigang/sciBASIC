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
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class DataFrame

    Public Property features As New Dictionary(Of String, FeatureVector)
    Public Property rownames As String()

    Public ReadOnly Property dims As Size
        Get
            Return New Size(width:=features.Count, height:=rownames.Length)
        End Get
    End Property

    Public ReadOnly Property featureNames As String()
        Get
            Return features.Keys.ToArray
        End Get
    End Property

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

    Public Function delete(featureName As String) As Boolean
        Return features.Remove(featureName)
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
End Class
