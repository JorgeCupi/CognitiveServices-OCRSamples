# CognitiveServices-OCRSamples
Just a small repo that will contain different ways to consume the OCR API from Azure Cognitive Services

The intention of this repo is also to build an OCR reader that transforms the OCR result to a single region separated by horizontal lines instead of separating the results into multiple regions that could be made of columns and/or rows depending on the grouping.

## How to use this sample ##
You'll need an Microosft Azure account and create a Compute Vision service in the portal. If you don't have an account yet you can create one for free on the [official page](https://azure.microsoft.com/en-us/).

Once you create a Computer Vision service, just grab the URL and Key from the portal and paste it on the App.config file:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup> 
        // ... Some configuration values here
    </startup>
    <appSettings>
      <add key="ComputerVisionKey" value="yourComputerVisionKey"/>
      <add key="ComputerVisionUri" value="yourComputerVisionUriBase"/>
    </appSettings>
</configuration>
```