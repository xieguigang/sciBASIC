#Region "Microsoft.VisualBasic::c35791ae5a5057c4038352448034a017, Data\BinaryData\DataStorage\Tabular\Schema.vb"

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

    '   Total Lines: 52
    '    Code Lines: 36 (69.23%)
    ' Comment Lines: 4 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (23.08%)
    '     File Size: 1.36 KB


    ' Class Schema
    ' 
    '     Properties: cols, dims, ordinals, rownames
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' Class VectorSchema
    ' 
    '     Properties: isScalar, offset, type
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Class Schema

    Public Property rownames As String()
    Public Property cols As Dictionary(Of String, VectorSchema)
    Public Property dims As Integer()

    Public Property name As String
    Public Property description As String

    ''' <summary>
    ''' the orders of the feature names(keys of the <see cref="cols"/>)
    ''' </summary>
    ''' <returns></returns>
    Public Property ordinals As String()

    Default Public ReadOnly Property field(name As String) As VectorSchema
        Get
            Return cols(name)
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(df As DataFrame)
        rownames = df.rownames
        dims = {df.dims.Height, df.dims.Width}
        cols = df.features _
            .ToDictionary(Function(f) f.Key,
                          Function(f)
                              Return New VectorSchema(f.Value)
                          End Function)
        ordinals = df.featureNames
        name = df.name
        description = df.description
    End Sub

End Class

Public Class VectorSchema

    Public Property type As TypeCode
    Public Property isScalar As Boolean
    Public Property offset As Long
    Public Property attrs As Dictionary(Of String, String)

    Sub New(feature As FeatureVector)
        type = feature.type.PrimitiveTypeCode
        isScalar = feature.isScalar
        attrs = feature.attributes
    End Sub

    Sub New()
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{StringFormats.Lanudry(offset)}] {type.ToString}"
    End Function

    Public Function GetTypeInfo() As Type
        Return type.CreatePrimitiveType
    End Function

    Public Function CreateEmpty() As Array
        Return Array.CreateInstance(GetTypeInfo, 0)
    End Function

End Class
