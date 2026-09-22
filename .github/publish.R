let nupkg_list  = list.files(relative_work(), pattern = "*.nupkg");
let api_key     = ?"--key" || stop("nuget api key for publish the package must be provided!");
let sourceName  = ?"--source" || "xieguigang_NuGet";

print("view list of the nuget package to publish:");
print(basename(nupkg_list));

for(file in nupkg_list) {
    @`dotnet nuget push "${file}" --source "${sourceName}" --api-key "${api_key}"`;
}