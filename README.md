# AzureStorageProvider for Kentico EMS

This provider is designed to replace standard Kentico Azure provider in Kentico EMS to ensure faster loading times and smooth experience when working with larger sets of data such as Media Library files.

Currently version 12 of Kentico EMS is supported.

# Use the provider
If you want to use the provider in your Kentico EMS solution:
* install the provider using [NuGet package](https://github.com/Kentico/AzureStorageProvider/blob/master/k12/KenticoAzureStorageProvider.12.0.0.nupkg)
* configure the provider according to [instructions](https://github.com/Kentico/AzureStorageProvider/blob/master/INSTRUCTIONS.md)

# Installation
If you wish to participate on the implementation, follow these steps:
* open solution file of the provider in Visual Studio and make sure all references are loaded correctly
* to verify your changes, run tests

If you wish to test the provider with your Kentico solution, follow these steps:
* open your Kentico solution in Visual Studio
* add AzureStorageProvider projects using SLN file in this repository
* configure the solution according to [instructions](https://github.com/Kentico/AzureStorageProvider/blob/master/INSTRUCTIONS.md)

# Contributing
Want to improve Azure Storage Provider? Great! Read the [contributing guidelines](https://github.com/Kentico/AzureStorageProvider/blob/master/CONTRIBUTING.md).

If anything feels wrong or incomplete, please let us know. Create a new [issue](https://github.com/Kentico/AzureStorageProvider/issues/new) or submit a [pull request](https://help.github.com/articles/using-pull-requests/).

# Generate NuGet package
There is already .nuspec file in this repository. To generate a new NuGet package:

* navigate to `K12` folder
* if you don't have NuGet CLI installed, download it from [nuget.org](https://nuget.org/downloads)
* run `nuget pack Package.nuspec`
