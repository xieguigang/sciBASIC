#Region "Microsoft.VisualBasic::ef71211ac06b685d7ea2841796dc5e9d, Data_science\DataMining\DataMining\ComponentModel\Entity.vb"

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

    '     Class EntityBase
    ' 
    '         Properties: entityVector, Length
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel

    ''' <summary>
    ''' An abstract property vector 
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T)

        ''' <summary>
        ''' Properties vector of current entity.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("T")>
        Public Overridable Property entityVector As T()

        ''' <summary>
        ''' Get/Set property value by index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(i As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector(i)
            End Get
            Set(value As T)
                entityVector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' Length of <see cref="entityVector"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return entityVector.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return entityVector.GetJson
        End Function
    End Class
End Namespace
