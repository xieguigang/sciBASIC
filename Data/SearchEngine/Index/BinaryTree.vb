Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

''' <summary>
''' File save model for binary tree
''' </summary>
Public Class BinaryTree(Of K, V)

    Public Property Key As K
    Public Property Value As V

    <XmlElement>
    Public Property Additionals As V()

    <XmlAttribute>
    Public Property Left As Integer
    <XmlAttribute>
    Public Property Right As Integer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Scripting.ToString(Key)
    End Function

End Class
