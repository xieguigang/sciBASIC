Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel

''' <summary>
''' File save model for binary tree
''' </summary>
Public Class BinaryTreeIndex(Of K, V) : Implements IAddress(Of Integer)

    Public Property Key As K
    Public Property Value As V

    <XmlElement>
    Public Property Additionals As V()

    <XmlAttribute>
    Public Property Left As Integer
    <XmlAttribute>
    Public Property Right As Integer
    <XmlAttribute>
    Public Property My As Integer Implements IAddress(Of Integer).Address

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Scripting.ToString(Key)
    End Function

End Class
