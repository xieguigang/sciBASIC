Imports Microsoft.VisualBasic.MIME.text.yaml
Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar

Module Module1

    Sub Main()
        Call v12()
        Call v11()

        Pause()
    End Sub

    Sub v11()
        'Dim docs = Grammar11 _
        '    .YamlParser _
        '    .PopulateDocuments("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.1\idle_anim.yaml") _
        '    .ToArray

        'Pause()
    End Sub

    Sub v12()
        Dim doc = YamlParser.Load("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.2\samples\YAML_Sample.yaml")
        Dim singleDoc = YamlParser.Load("X:\Ripped\00d197fedc13a9d41acf88e9375702e9\AnimationClip\rb_yang_passive.anim")

        Pause()
    End Sub

End Module
