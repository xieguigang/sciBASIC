setwd(@dir);

let nupkg_list      = list.files("./", pattern = "*.nupkg");
let YOUR_GITHUB_PAT = "";

print("view list of the nuget package to publish:");
print(basename(nupkg_list));

for(file in nupkg_list) {
    @`dotnet nuget push "${file}" --api-key ${YOUR_GITHUB_PAT} --source "github"`;
}