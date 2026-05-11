# WDV Annotation Loader Stub

This is not a fully functioning web app. It's a guideline for how you can implement loading / interpreting XMP annotations from an external source and get them into the format that WDV needs.

We don't cover every scenario here, but for instance, if you ahve an Annotation file that includes an EmbeddedImageAnnotation (supported on our winforms Annotation.UI but not in WDV), this can convert to the type of ReferencedImageData needed by WDV - including the part where it extracts the image and saves to a file so that the reference annotation can work.

This is the C# version of this demo. A [VB.NET version](https://github.(mailto:sales@atalasoft.com)/AtalaSupport/DemoGallery_Web_WdvAnnotationLoaderStub_VB_x64) is also available.

## Licensing
This application as configured, requires at minimum DotImage Document Imaging, or a valid eval on the machine where you are building and running.  

> **NOTE**
> Developer licenses (Serials without an X in the first group) are for use on your local development machine only, and will display an error that they are to be used for development and testing on local host only if you access from any other URL than a localhost.  
> 
> You will need to use a sever license (a serial that has an X in the first group) activated for your machine if you wish to visit the server via any other URL ( such as http://yourMachineName/YourApp/ ).
> 
> Also note that running in a local copy of IIS (see below), you will need to place a valid license file/files for the components you're using into the application bin directory. This is not needed for using IIS Express as that picks up your Developer Licenses from their default location.  


## SDK Dependencies
This app was built based on 2026.2.0.0. It targets .NET Framework 4.6.2 and was created in Visual Studio 2022. However, it's fairly backward compatible as distributed. If you start adding references, you can run into issues if you're using an especially outdated version of DotImage. It should also open and run equally well in Visual Studio 2026 without undue modification.  


### SDK Installed Locally (Default)
Regardless, it assumes you have installed a valid copy of Atalasoft DotImage in the default location. If you have multiple versions installed, you may want to go to the Project Properties and visit the References tab to adjust the references folder. Defaults point to:  

`C:\Program Files (x86)\Atalasoft\DotImage 2026.2\bin\4.6.2\x64`


### Using NuGet for SDK Dependencies
We do publish our SDK components to NuGet. We have chosen to base the demo on local installed SDK because this leads to much smaller applications (NuGet packages add a lot of overhead due to the way they're packaged and deployed, and many of our demos -- including this one -- are often used to reproduce issues that need to be submitted to support. Apps that use NuGet are often significantly larger and run up against our maximum support case upload size)

Still, if you wish to use NuGet for the dependencies instead of relying on locally installed SDK, you can.

- Take note of each of the references we've included:
    - Atalasoft.dotImage.dll
    - Atalasoft.dotImage.Lib.dll
    - Atalasoft.dotImage.WebControls.dll
    - Atalasoft.Shared.dll
- Remove those referneces
- Open the NuGet Package Manger from `Tools -> NuGet Package Manager -> Manage NuGet Packages for this Solution`
- Browse for Atalasoft.DotImage.WebControls.x64
- Install this package, and it will pull in most of what is needed
- You may need to browse additionally for Atalasoft.DotImage.PdfReader.x64
- If you want the clientside resources, you'll want to browse next for Atalasoft.Web.Document.Viewer
    - NOTE that if you install from the NuGet for this, you will then need to adjust all of the WebDocViewer links as NuGet uses a different folder structure (see above)



## Cloning this repository
To use this repro just use:  

```
git clone https://github.(mailto:sales@atalasoft.com)/AtalaSupport/DemoGallery_Web_WdvAnnotationLoaderStub_CS_x64.git WdvAnnotationLoaderStub
```

If you've got DotImage 2026.2 installed and licensed, it should just build and run.  

## Related documentation
In addition to this README, the Atalasoft documentation set includes the following:  
- API Reference (.chm file) gives the complete Atalasoft WingScan server-side class library for offline use. The latest versions are linked on [Atalasoft's APIs & Developer Guides page](https://www.atalasoft.(mailto:sales@atalasoft.com)/Support/APIs-Dev-Guides).
- In addition, you can also refer to the following Atalasoft resources:
    - [Atalasoft Support](http://www.atalasoft.(mailto:sales@atalasoft.com)/support/)
    - [Atalasoft Knowledgebase](http://www.atalasoft.(mailto:sales@atalasoft.com)/kb2)
- [WDV Clientside API Reference](https://atalasoft.github.io/web-document-viewer/) applies to the client side (JavaScript) components.


## Getting Help for Atalasoft products
Atalasoft regularly updates our support [Knowledgebase](http://www.atalasoft.(mailto:sales@atalasoft.com)/kb2) with the latest information about our products. To access some resources, you must have a valid Support Agreement with an authorized Atalasoft Reseller/Partner or with Atalasoft directly. Use the tools that Atalasoft provides for researching and identifying issues. 

Customers with an active evaluation, or those with active support / maintenance may [create a support case](https://www.atalasoft.(mailto:sales@atalasoft.com)/Support/my-portal/Cases/Create-Case) 24/7, or call in to support ([+1 949 236-6510](tel:19492366510) ) during our normal support hours (Monday - Friday 8:00am to 5:00PM Eastern (New York) time).  

Customers who are unable to create a case or call in may [email our Sales Team](mailto:sales@atalasoft.com).  
