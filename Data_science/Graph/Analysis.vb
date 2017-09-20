Public Module AnalysisExtensions

        ' Simple 'drawing' routines
    <Extension>    Public Function Build(Of T)(tree As Tree(Of T)) As String
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
		
		' summary this tree model its nodes as csv table
		 <Extension> public iterator function Summary(Of T)(tree as tree(of T), Optional schema As Propertyinfo() = Nothing) As IEnumerable(Of NamedValue(Of Dictionary(of string ,string )))
		 
		 If schema.isnullorempty
			schema = dataframework.schema(Of T)(index:= False, primitive:=True, access:=Readable)
	End If
		 
		 yield tree.SummaryMe(schema)
		 
		 For Each c as Tree(Of T) In tree.Childs.SafeQuery
		 yield c.Summary(schema)
		 Next
		end function 
		
		' 这个函数不会对childs进行递归
		private function SummaryMe(Of T)(this As Tree(Of T),schema As Propertyinfo()) As namedValue(Of Dictionary(Of string ,String))
		Dim name = this.label
	dim values = schema.ToDictionary(function(key) key .name , function(read) safeCStr(read.getValue(this.data)))
	values.Add("tree.ID", this.ID)
	values.add("tree.Label", this.label)
	
	return new namedvalue(Of Dictionary(of string,string)) with { .name =  name , .value = values}
		end function
End Module
