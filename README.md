# ![BetaMax Icon](https://raw.githubusercontent.com/MaslasBros/betamax/prod/Docs/betamax.png)

# Table of Contents

* [Description](#description)

* [Screenshots](#screenshots)

* [Features](#features)

* [Unity Folder Structure](#unity-folder-structure)

* [Tool Flowchart](#tool-flowchart)

* [Base Handler](#base-handler)
  
  * [Submission Handler](#the-submission-handler-class)
  
  * [Data Container Structs](#the-data-containers)

* [Dependencies](#dependencies)

# Description

A **Unity3D runtime** tool used for beta testing and issue reporting. It supports the HTTP/HTTPS, FTP, SFTP upload protocols. The user with the press of a button can submit his issue to your server with ease and also provides the user with the functionality of adding additional files to the final zipped file that you will receive and  automatically save a copy of his sent info to his device upon uploading. All of this fully customizable and extensible for your use.

# Screenshots

## UI Examples provided

![](https://raw.githubusercontent.com/MaslasBros/betamax/prod/Docs/configPanel.png)

![](https://raw.githubusercontent.com/MaslasBros/betamax/prod/Docs/submitPanel.png)

# Features

## User Persistent Data

The tool provides you and the user with a simple JSON file that can be edited, serialized and saved on the beta tester's device, so no need to re-enter all the needed information every time the user presses the submit button.

## Supported Protocols

The supported protocols: *FTP*, *SFTP*, *HTTP*, *HTTPS*.

***No encryption on the inspector fields is provided, although you can extend this.***

HTTP/S uses ***BasicAuth***.

## UI Examples Included

Inside the package you'll also find a fully setted up and functional UI template which you can freely use and modify to your needs.

Located at [Screenshots](#screenshots)

# Unity Folder Structure

Upon opening the package.

```mermaid
graph LR;
    A[Assets]-->B[BetaMax];
    A-->C[Exmaples];
```

The BetaMax Folder is where the whole tool is contained in. All the essential scripts and plugins are also included in this folder. The single script you'll need will be the **SubmissionHandler** class.

```mermaid
graph LR;
    A[BetaMax];
    B[_Core];
    C[IO];
    D[Posts];

    A-->B
    A-->C
    A-->D

    E[SubmissionHandler.cs]
    F[Device file handlers]
    G[Uploading handlers]

    B-->E
    C-->F
    D-->G
```

The Examples folder contains a fully functional scene and some example panel scripts so you can have a starting point on how to set up your UI. Although none of the example scripts are needed for the tool.

```mermaid
graph LR;
    A[Example]
    B[Scripts]
    C[Scenes]
    D[Dummy Scene]

    A-->B
    A-->C
    C-->D
```

# Tool Flowchart

```mermaid
flowchart TD
    strt([Issue Identified])
    act1[/Press Hot Key/]
    evnt2[Runtime Paused]
    evnt3[Capture Runtime Screenshot]
    evnt4[Collect Runtime Log]
    evnt5[Show Issue Form]
    act2[/Fill Form Entries/]
    act3[/Submit Form/]
    evnt6[Package data in Archive]
    evnt7[Upload and/or send package to destination]
    evnt8[Runtime Resumed]
    evnt9[Show a success notification on UI]
    evnt10>Emit IssueCommited Event]
    evnt11(Execute Auxiliary Process)
    evnt12[Collect Optional Data]
    evnt13[Save Package Locally]
    evnt14[Show an error notification on UI]
    nd((End))
    if1{On Issue Pause}
    if2{On Issue Pause}
    if3{Aux Process ?}
    if4{On Submit Download}
    if5{Success?}

    strt --> act1 --> if1
    act1 --> evnt10
    if1 --Yes--> evnt2 --> evnt3
    if1 --No--> evnt3
    evnt3 --> evnt4 --> evnt5
    evnt5 --> act2 --> act3 --> if3
    if3 --Yes--> evnt11 --> evnt12
    if3 --No--> evnt12 --> evnt6 --> evnt7 --> if5
    if5 --No--> evnt14
    evnt14 --> if2
    evnt9 --> if2
    if5 --Yes--> evnt9
    if2 --Yes--> evnt8 --> if4
    if2 --No--> if4
    if4 --Yes--> evnt13
    if4 --No--> nd
    evnt13 --> nd
```

# Base Handler

## The Submission Handler class

The SubmissionHandler class (SubmissionHandler.cs) is the main handler of the tool. This is the place where you will set up everything you need for the tool to work properly. From save folders to server info and the screenshot handler.

*Place on a gameObject.*

Below comes the **inspector modifiable** fields and how you can **edit** them.

### File Naming Settings

| Name                | Type   | Field Info                                                                                                                                       |
| ------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Source Folder       | string | The application's configuration main folder name which will be placed inside the AppData of the user.                                            |
| JSON_FILE_NAME      | string | The filename along with its extension of the file containing the user's configuration.                                                           |
| TEMP_FOLDER_NAME    | string | The name of the folder in which the files will get copied to before getting zipped for uploading.                                                |
| USER_OPT_FOLDER     | string | The name of the folder in which the user's optional files will get copied to inside the TEMP_FOLDER_NAME before getting zipped for uploading.    |
| MAIN_ZIP_NAME       | string | The name of the zip you will receive in your server.                                                                                             |
| Main Zip Format     | string | A string used to format the MAIN_ZIP_NAME with the place of the date and the name. Modifiable keywords: {date}, {name} for example {name}_{date} |
| Date Format         | string | The format you want the {date} to be formatted as, for example: dd_MM_yyyy_HH_mm_ss                                                              |
| DOWNLOADED_ZIP_NAME | string | The name of the zip saved locally to the users device. The final name will be formatted as {fixedDateFormat}_{DOWNLOADED_ZIP_NAME}               |

### Submitter Settings

| Name             | Type     | Field Info                                                                                         |
| ---------------- | -------- | -------------------------------------------------------------------------------------------------- |
| Submit Panel Key | KeyCode  | The button that will toggle the submit panel ON/OFF.                                               |
| Config Panel Key | KeyCode  | The button that will toggle the configuration panel ON/OFF.                                        |
| Issue Categories | string[] | The categories of issues that your UI handler can automatically retrieve and feed to the dropdown. |

### Server Info

| Name            | Type   | Field Info                                                                                                                                                 |
| --------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Hostname        | string | The transfer server URL. The protocol is automatically determined in the handler. **URL Format:** {PROTOCOL}://{address}:{PORT}/{Abs\|Rel path}/{fileName} |
| Server Username | string | The server login username.                                                                                                                                 |
| Server Password | string | The server login password.                                                                                                                                 |

### Screenshot Camera

| Name              | Type         | Field Info                                      |
| ----------------- | ------------ | ----------------------------------------------- |
| Screenshot camera | Unity Camera | The camera that will be used for screenshoting. |

### UI Panels:

| Name                | Type       | Field Info                                                 |
| ------------------- | ---------- | ---------------------------------------------------------- |
| Submission Panel    | GameObject | Reference to the gameObject of the submission panel.       |
| Config Panel        | GameObject | Reference to the gameObject of the configuration panel.    |
| Message Text        | TMP_UGUI   | The UI element used to show tool messages to the interface |
| Close After Seconds | float      | The time which the message text will get displayed for.    |

### Debugging Log

| Name            | Type | Field Info                                              |
| --------------- | ---- | ------------------------------------------------------- |
| Show Debug Logs | bool | Whether to show debug logs or not in the unity console. |

A log file is also created in the runtime and placed inside the TEMP_FOLDER_NAME which logs everything that is happening in the tool, whether or not Show Debug Logs is toggled on.

## The Data Containers

Two structs are used to transfer the panel data to the Submission Handler.

1) ConfigInfo

2) SubmitInfo

The SubmissionHandler must know what field data to serialize and to achieve this a ConfigStruct must be passed to the Submission Handler when it's time to save the user's info.

The submission process follows the same path, when the user wants to upload the data a SubmitInfo struct must be passed to the Submission Handler so the field info can be collected from the interface.

# Dependencies

* **[SSH.NET](https://github.com/sshnet/SSH.NET)**: SSH.NET is a Secure Shell (SSH) library for .NET, optimized for parallelism.
