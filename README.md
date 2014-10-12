# Fabmunc

**The Fellow Automated (but Mostly Useless) Network Contributor**

Fabmunc leverages the [LibGit2Sharp][lg2s] library to simulate some activity
on a remote repository.

Given a remote repository and the appropriate credentials to push against it,
the tool will perform the following actions:

 - Fetch from the remote repository in a new temporary folder
 - Randomly create new branch or pick an exisiting branch
 - Randomly add commits to this branch
 - Push the branch to the remote repository

 [lg2s]: http://libgit2sharp.com
