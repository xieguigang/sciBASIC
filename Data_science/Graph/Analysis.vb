Public Module AnalysisExtensions

        ' Simple 'drawing' routines
    <Extension>    Public Function Build(tree As Tree(Of T)) As String
            If tree Is Nothing Then
                Return "()"
            End If

          if tree.isleaf then 
 return tree.ID
else 
dim children = tree _
.childs _
.select(function(t) t.build) _
.joinby(", ")

return $"{tree.ID}({children})" 
		 end if 
        End Function
End Module
