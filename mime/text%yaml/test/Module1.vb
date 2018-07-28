Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar

Module Module1

    Sub Main()
        Dim doc = YamlParser.Load("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.2\samples\YAML_Sample.yaml")
        Dim singleDoc = YamlParser.Load("E:\VB_GamePads\runtime\sciBASIC#\mime\text%yaml\1.2\samples\test.yaml")

        Pause()
    End Sub

End Module
