# Templates - Dotnet

A simple template leveraging [.Config][1] dotfiles submodule  for rapid project deployment. Simply rename Solution.{code-workspace,sln,sln.DotSettings} to your project name and get started! Afterwards, replace this readme with the actual documentation of your project.

Additionally, until GitHub properly supports submodule definitions from template repositories, after cloning you should run
```git submodule add https://github.com/kritikos-io/.config``` from the repository root. You can replace the submodule with a compatible fork (to preserve your own default namespace etc) **provided it keeps file naming intact** since most files are appearing as symlinks.

[1]: https://github.com/kritikos-io/.config
