Imports System.Text

Namespace CommonElements

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T)

        <Xml.Serialization.XmlAttribute> Public Property Properties As T()

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

        <Xml.Serialization.XmlAttribute> Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Dim sBuilder As System.Text.StringBuilder = New StringBuilder(1024)
            For Each p As Integer In Properties
                Call sBuilder.AppendFormat("{0}, ", p)
            Next
            Call sBuilder.Remove(sBuilder.Length - 1, length:=1)

            Return String.Format("<{0}> --> {1}", sBuilder.ToString, [Class])
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="row">第一个元素为分类，其余元素为属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CastTo(row As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.RowObject) As Entity
            Dim LQuery = From s As String In row.Skip(1) Select CType(Val(s), Integer) '
            Return New Entity With {.Class = Val(row.First), .Properties = LQuery.ToArray}
        End Function

        Default Public ReadOnly Property Item(Index As Integer) As Integer
            Get
                Return Properties(Index)
            End Get
        End Property

        Public Shared Widening Operator CType(properties As Double()) As Entity
            Return New Entity With {.Properties = (From e In properties Select CType(e, Integer)).ToArray}
        End Operator

        Public Shared Widening Operator CType(properties As Integer()) As Entity
            Return New Entity With {.Properties = properties}
        End Operator
    End Class
End Namespace