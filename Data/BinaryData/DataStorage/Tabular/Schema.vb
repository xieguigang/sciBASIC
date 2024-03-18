﻿Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Class Schema

    Public Property rownames As String()
    Public Property cols As Dictionary(Of String, VectorSchema)
    Public Property dims As Integer()

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
    End Sub

End Class

Public Class VectorSchema

    Public Property type As TypeCode
    Public Property isScalar As Boolean
    Public Property offset As Long

    Sub New(feature As FeatureVector)
        type = feature.type.PrimitiveTypeCode
        isScalar = feature.isScalar
    End Sub

    Sub New()
    End Sub

End Class