# assemble the app path
# and then run mono cli
app="$DIR/{appName}.exe"
cli="$@"

mono "$app" $cli