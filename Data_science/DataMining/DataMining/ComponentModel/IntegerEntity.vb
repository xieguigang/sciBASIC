#Region "Microsoft.VisualBasic::2f77dc50b99f1973007b4ee12bd77d7e, Data_science\DataMining\DataMining\ComponentModel\IntegerEntity.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class IntegerEntity
    ' 
    '         Properties: [Class]
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IntegerEntity : Inherits EntityBase(Of Integer)

        <XmlAttribute>
        Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", entityVector)}> --> {[Class]}"
        End Function

        Default Public Overloads ReadOnly Property ItemValue(Index As Integer) As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector(Index)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Double()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(properties As Integer()) As IntegerEntity
            Return New IntegerEntity With {
                .entityVector = properties
            }
        End Operator
    End Class
End Namespace
