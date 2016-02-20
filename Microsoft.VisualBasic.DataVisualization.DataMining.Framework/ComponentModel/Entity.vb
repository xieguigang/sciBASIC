Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.DocumentFormat.Csv

Namespace ComponentModel

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T)

        <XmlAttribute("T")>
        Public Property Properties As T()

        Public ReadOnly Property Length As Integer
            Get
                Return Properties.Length
            End Get
        End Property
    End Class

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Entity : Inherits EntityBase(Of Integer)

        <XmlAttribute> Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", Properties)}> --> {[Class]}"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="row">第一个元素为分类，其余元素为属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CastTo(row As DocumentStream.RowObject) As Entity
            Dim LQuery = From s As String In row.Skip(1) Select CType(Val(s), Integer) '
            Return New Entity With {
                .Class = Val(row.First),
                .Properties = LQuery.ToArray
            }
        End Function

        Default Public ReadOnly Property Item(Index As Integer) As Integer
            Get
                Return Properties(Index)
            End Get
        End Property

        Public Shared Widening Operator CType(properties As Double()) As Entity
            Return New Entity With {
                .Properties = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        Public Shared Widening Operator CType(properties As Integer()) As Entity
            Return New Entity With {
                .Properties = properties
            }
        End Operator
    End Class
End Namespace