# Deal with the github merge conflicts

### First: checkout branch

>
```bash
git checkout -b <branch-name> master
git pull git://github.com/{username}/{<repo>.git} master
```

### Second: edits the conflict file

After edits in your text editor, then commit the changes:
>
```batch
git commit -a
```

### Last: Push and merge

>
```batch
git checkout master
git merge --no-ff <branch-name>
git push origin master
```

### Example
>
```
# git checkout -b xieguigang-master master
Switched to a new branch 'xieguigang-master'
```
```
# git pull git://github.com/xieguigang/GCModeller.Core.git master
remote: Counting objects: 6, done.
remote: Compressing objects: 100% (4/4), done.
remote: Total 6 (delta 3), reused 5 (delta 2), pack-reused 0
Unpacking objects: 100% (6/6), done.
From git://github.com/xieguigang/GCModeller.Core
 * branch            master     -> FETCH_HEAD
Auto-merging README.md
CONFLICT (content): Merge conflict in README.md
Automatic merge failed; fix conflicts and then commit the result.
```
```
# git checkout master
README.md: needs merge
error: you need to resolve your current index first
```
```
# git commit -a
[xieguigang-master b5f50e6] Merge branch 'master' of git://github.com/xieguigang/GCModeller.Core into xieguigang-master
```
```
# git checkout master
Switched to branch 'master'
Your branch is up-to-date with 'origin/master'.
```
```
# git merge --no-ff xieguigang-master
Merge made by the 'recursive' strategy.
 README.md | 34 +++++++++++++++++++++++++++-------
 1 file changed, 27 insertions(+), 7 deletions(-)
```
```
# git push origin master
Counting objects: 9, done.
Delta compression using up to 4 threads.
Compressing objects: 100% (9/9), done.
Writing objects: 100% (9/9), 1.46 KiB | 0 bytes/s, done.
Total 9 (delta 5), reused 0 (delta 0)
To https://github.com/SMRUCC/GCModeller.Core.git
   ffa7199..1408682  master -> master
```
![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/guides/MergeGthubConflicts-example.png)
