Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module writerTest

    Const json As String = "{
    'vec': [23,4,44,23,42,34,2,3,42,3,4,2.34],
    'strs': [
        'dsadsadad',
'asdadasdsd','qeqeqweqwewq','4444444444','3333'
],
'flags':[true,true,true,false,false,true],
'nest_obj':{
    'a': true,
    'b': [5,23,53,2.4244,2.323],
    'c': 'xxxxxxxxxxxxxxxxxx'
},
'objs': [

{'a':1,'c':false},
{'a':-1,'c':false,'v':[23,4,32,43,24]},
{'a':1,'c':true}
]
}"

    Sub Main()
        Dim json As JsonElement = JsonParser.Parse(writerTest.json)
        Dim json_str As String = json.BuildJsonString(indent:=True)
        Dim json_line As String = json.BuildJsonString(indent:=False)

        Call Console.WriteLine(json_str)
        Call Console.WriteLine(json_line)

        Pause()
    End Sub
End Module
