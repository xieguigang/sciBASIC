
Namespace Drawing3D.Math3D.MarchingCubes

    Module Utils

        Public Function VertexIndexToString(index As Integer) As String
            Return String.Format("{0}:{1}:{2}", index And 1, index >> 1 And 1, index >> 2 And 1)
        End Function
    End Module
End Namespace