#Region "Microsoft.VisualBasic::495a615b0ab73ab6a052c87645ce7e81, Data_science\DataMining\FeatureFrame\Encoder.vb"

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

    '   Total Lines: 80
    '    Code Lines: 44
    ' Comment Lines: 24
    '   Blank Lines: 12
    '     File Size: 2.66 KB


    ' Class Encoder
    ' 
    '     Function: AutoEncoding, Encode, Encoding
    ' 
    '     Sub: AddEncodingRule
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' A helper module for encode the feature vector into numeric feature
''' </summary>
Public Class Encoder

    ReadOnly encodings As New Dictionary(Of String, FeatureEncoder)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddEncodingRule(field As String, encoder As FeatureEncoder)
        encodings(field) = encoder
    End Sub

    ''' <summary>
    ''' Try to encode all non-numeric feature vector 
    ''' </summary>
    ''' <param name="data">
    ''' 不是所有的字段都需要进行编码操作的
    ''' </param>
    ''' <returns></returns>
    Public Function Encoding(data As DataFrame) As DataFrame
        For Each name As String In data.features.Keys.ToArray
            If Not encodings.ContainsKey(name) Then
                ' no needs for run data encoding
                Continue For
            End If

            Dim v As FeatureVector = data(name)
            Dim extends As DataFrame = encodings(name).Encode(v)

            data.delete(featureName:=name)
            data = data.Union(extends)
        Next

        Return data
    End Function

    ''' <summary>
    ''' encoding a feature dataframe to a new normalized
    ''' dataframe automatically.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Shared Function AutoEncoding(data As DataFrame) As DataFrame
        For Each name As String In data.features.Keys.ToArray
            Dim v As FeatureVector = data(name)
            Dim extends As DataFrame = Encode(v)

            data.delete(featureName:=name)
            data = data.Union(extends)
        Next

        Return data
    End Function

    ''' <summary>
    ''' auto encoder
    ''' </summary>
    ''' <param name="feature"></param>
    ''' <returns>
    ''' this will make all feature data type as the numeric value
    ''' </returns>
    Public Shared Function Encode(feature As FeatureVector) As DataFrame
        Select Case feature.type
            Case GetType(String) : Return New EnumEncoder().Encode(feature)
            Case GetType(Boolean) : Return New FlagEncoder().Encode(feature)
            Case GetType(Single),
                 GetType(Double),
                 GetType(Short),
                 GetType(Integer),
                 GetType(Long)

                Return New NumericEncoder().Encode(feature)
            Case Else
                Throw New NotImplementedException(feature.type.Name)
        End Select
    End Function
End Class
