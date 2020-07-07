## Advanced Clozer Plugin for SuperMemoAssistant

### Features

- Easily create cloze items with hints.
- Saves hint history.
- *Optionally* integrates with Mouseover Hints plugin to enable **experimental** 'Hide Cloze Context' and 'Mousover Cloze Hint' features

### Installation

#### Manual Installation

##### Prerequisites

- Install Visual Studio 2019 or higher.
- Select the following  VS components during the install:
+ .NET desktop development
+ .NET Core cross-platform development

##### Step-by-step Guide

1. Clone the project using git.

  `git clone https://github.com/bjsi/SuperMemoAssistant.Plugins.AdvancedClozer`

2. Open the cloned project folder.

3. Double click on the solution (AdvancedClozer.sln) to open the project in Visual Studio 2019.

4. Right click on the solution file in the **Solution Explorer**:

![Image of Solution Explorer](https://github.com/bjsi/docs/blob/master/SMA/plugins/images/solution-explorer.png)

5. Select **Build Solution**:

![Image of Build Solution Option](https://github.com/bjsi/docs/blob/master/SMA/plugins/images/build-solution.jpg)

6. Check that the build succeeded by confirming that the following folder exists and is not empty:

`C:\Users\<YOUR USERNAME>\SuperMemoAssistant\Plugins\Development`

7. Close Visual Studio and run SuperMemoAssistant.

### Manual

#### Usage

#### Configuration

##### Settings

> You can access the settings of any SuperMemoAssistant plugin by pressing Ctrl+Alt+Shift+O and clicking the gear icon.
-
-
-

#### Extensibility

Advanced Clozer does not support plugin service extensions.

### Contributing Guide

#### Issues and Suggestions

See the [contribution guide](https://github.com/bjsi/docs/blob/master/SMA/plugins/CONTRIBUTING.md) for information on how to report issues or make suggestions.

#### Code Contributions

Pull requests are welcome!

1. Firstly, go through the manual installation guide above.
2. You will also require [Git Hooks for VS](https://marketplace.visualstudio.com/items?itemName=AlexisIncogito.VisualStudio-Git-Hooks) which is used to enforce a consistent code style.
> Note: you do not need to build the entire SuperMemoAssistant project to make changes to or debug a plugin.
3. See the code section of the [contribution guide](https://github.com/bjsi/docs/blob/master/SMA/plugins/CONTRIBUTING.md) for pull request instructions.
4. If you need help, don't hesitate to get in touch with me (Jamesb) on the SMA discord channel.
