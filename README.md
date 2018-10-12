# AzureStorageProvider for Kentico EMS

This provider is designed to replace standard Kentico Azure provider in Kentico EMS to ensure faster loading times and smooth experience when working with larger sets of data such as Media Library files.

Currently versions 10 and 11 od Kentico EMS are supported.

# Get the provider
Follow these steps if you just want to leverage the provider and not care about its implementation / compiling / etc.
- Download already compiled libraries in 7zip package for your version of Kentico
- Copy them into BIN folder of your Kentico installation
- Follow the instructions in instructions.docx file

# Installation
If you wish to participate on the implementation, follow these steps:
- Decide which version you want to participate on
- Open solution file of target version (e.g. k10.sln) in Visual Studio and make sure all references are loaded correctly
- Copy k{versionNumber}/src/AzureStorageProvider.Tests/App.config.template file as App.config and fill Azure BLOB storage credentials for testing

If you wish to test the provider with your Kentico solution, follow these steps:
- Build AzureStorageProvider project
- Copy AzureStorageProvider.dll and Microsoft.WindowsAzure.Storage.dll into bin folder of your website

OR to enable in-time debugging:

- Open your Kentico solution
- Add existing project AzureStorageProvider (optionally change references to point to Lib folder of your instance)
- Follow the instructions in instructions.docx file

# Contributing
Want to improve Azure Storage Provider? Great! Read the [contributing guidelines](https://github.com/Kentico/AzureStorageProvider/blob/master/CONTRIBUTING.md).

If anything feels wrong or incomplete, please let us know. Create a new [issue](https://github.com/Kentico/AzureStorageProvider/issues/new) or submit a [pull request](https://help.github.com/articles/using-pull-requests/).

![Analytics](https://kentico-ga-beacon.azurewebsites.net/api/UA-69014260-4/Kentico/AzureStorageProvider?pixel)
