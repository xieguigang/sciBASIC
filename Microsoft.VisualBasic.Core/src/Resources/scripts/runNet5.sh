# assemble the app path
# and then run mono cli
app="$DIR/{appName}.dll"
cli="$@"

dotnet "$app" $cli