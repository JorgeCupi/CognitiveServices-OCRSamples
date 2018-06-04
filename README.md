# CognitiveServices-OCRSamples
Just a small repo that will contain different ways to consume the OCR API from Azure Cognitive Services

The intention of this repo is also to build an OCR reader that transforms the OCR result to a single region separated by horizontal lines instead of separating the results into multiple regions that could be made of columns and/or rows depending on the grouping.

## How to use this sample ##
You'll need an Microosft Azure account and create a Compute Vision service in the portal. If you don't have an account yet you can create one for free on the [official page](https://azure.microsoft.com/en-us/).

Once you create a Computer Vision service, just grab the URL and Key from the portal and paste it on the <b>App.config</b> file:
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

Note that the URL in the portal will look like this:
<i>https://[yourAzureRegion].api.cognitive.microsoft.com/vision/v1.0</i>

We'll need to also add some parameters before pasting it to the config file. We'll add a language parameter set to unknown and a true flag for the service to detect the orientation of the image:

<i> /ocr?language=unk%26detectOrientation=true </i>

So, the final URL on the <b>App.config</b> file should look like this:

<i>https://[yourAzureRegion].api.cognitive.microsoft.com/vision/v1.0/ocr?language=unk%26detectOrientation=true</i>