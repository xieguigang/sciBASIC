Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language

Namespace Wavefront

    Public Class OBJ

        ''' <summary>
        ''' lib file name of mtl data
        ''' </summary>
        ''' <returns></returns>
        Public Property mtllib As String
        Public Property parts As ObjectPart()
        Public Property comment As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function ReadFile(file As StreamReader) As OBJ
            Return TextParser.ParseFile(file)
        End Function

    End Class

    Public Class ObjectPart

        Public Property g As String
        Public Property vertex As Point3D()
        Public Property vn As Point3D()
        Public Property usemtl As String
        Public Property f As Triangle()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return g.StringEmpty AndAlso
                    vertex.IsNullOrEmpty AndAlso
                    vn.IsNullOrEmpty AndAlso
                    usemtl.StringEmpty AndAlso
                    f.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{g Or "no_label".AsDefault}: {vertex.Length} vertexs and {f.Length} triangles"
        End Function

    End Class
End Namespace