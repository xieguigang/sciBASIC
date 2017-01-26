Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace Language

    ''' <summary>
    ''' Variable model in VisualBasic
    ''' </summary>
    Public Class Value : Inherits Value(Of Object)
        Implements INamedValue

        Public Property Name As String Implements IKeyedEntity(Of String).Key
        Public Property Type As Type
        ''' <summary>
        ''' 这个变量所在的函数的位置记录
        ''' </summary>
        ''' <returns></returns>
        Public Property Trace As NamedValue(Of MethodBase)

        ''' <summary>
        ''' Is a numeric type?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNumeric As Boolean
            Get
                Dim tcode As TypeCode = Type.GetTypeCode(Type)

                If tcode = TypeCode.Byte OrElse
                    tcode = TypeCode.Decimal OrElse
                    tcode = TypeCode.Double OrElse
                    tcode = TypeCode.Int16 OrElse
                    tcode = TypeCode.Int32 OrElse
                    tcode = TypeCode.Int64 OrElse
                    tcode = TypeCode.Single OrElse
                    tcode = TypeCode.UInt16 OrElse
                    tcode = TypeCode.UInt32 OrElse
                    tcode = TypeCode.UInt64 Then

                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property IsString As Boolean
            Get
                Return Type.Equals(GetType(String))
            End Get
        End Property

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return Nothing
            End If
            Return value.ToString
        End Function
    End Class
End Namespace