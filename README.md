Ricoh.NET
=========

Provides a .NET wrapper for the proprietary Ricoh / Lanier device services. Currently supports the uDirectory and deviceManagement service endpoints.

In order to interface with any Ricoh / Lanier copier or printer you can either use their clunky website administration
or you can use their proprietary SmartDeviceAdmin tool which requires itself to be run as an Administrator. Doing a
traffic capture of the tool, we can see that it uses a combination of SNMP as well as XML web services. Unfortunately,
Ricoh does not publish the *.wsdl file to the public instead requiring customers to purchase a developers licenses and
sign NDA disclosures. 

Thankfully, a few other pioneers worked through the majority of the *.wsdl file generation and oddities of the Ricoh 
service API.

- [libmfd](https://github.com/adam-nielsen/libmfd/)
- [Various Python modules to remote-control a Ricoh Aficio 2060 printer](http://opensource.fsmi.uni-karlsruhe.de/gitweb/?p=python-aficio2060.git;a=summary)
- [Ricoh Multi Function Printer (MFP) Address Book PowerShell Module](http://gallery.technet.microsoft.com/scriptcenter/Ricoh-Multi-Function-27aeea71)

It's worth noting that none of the libraries are feature complete and may not follow the exact specification since 
Ricoh doesn't publish that specification except to people enrolled in their developers program. So most of the work
you see is from watching TCP traffic and analysing the output.

__Note__

The Ricoh service uses obsolete encoded SOAP requests (soap-enc) which prevents the new .NET tools from working 
with the endpoints correctly even through the WSDL file is recognized and parsed. Essentially when the request is
made by .NET, the encoded portions are broken out into their own references. However the receiving service doesn't 
know how to glue the SOAP message back together. In order to get around that, custom WCF EndpointBehavior and 
MessageInspector were created to modify the incoming and outgoing SOAP messages.

```xml
  <xsd:complexType name="objectRelationList">
    <xsd:complexContent>
      <xsd:restriction base="soap-enc:Array">
        <xsd:attribute ref="soap-enc:arrayType" wsdl:arrayType="itt:objectRelation[]"/>
      </xsd:restriction>
    </xsd:complexContent>
  </xsd:complexType>
```

Sample Code
-----------
The following code is an example for retrieving and clearing totals from a copier.

```csharp
  var hostname = "copier.yourdomain.com";
  var username = "admin";
  var password = "yourpassword";
  var clearTotals = false;
  
  try {
    using (DeviceManagement deviceManager = new DeviceManagement(hostname, username, password)) {
      deviceManager.TimeLimit = 300; // Increase the default timelimit to keep session from expiring.

      // Retrieve our totals
      IEnumerable<UserCounter> counters = deviceManager.GetUserCounters();
      
      // Output totals
      foreach(var counter in counters) {
        Console.WriteLine(counter.ToString());
      }
      
      // Clear the totals
      if (clearTotals) {
        if (counters != null) deviceManager.Clear(counters);
        else deviceManager.Clear();
      }
    }
  } catch (Exception) {
    Console.WriteLine("An error occurred when exporting copier totals for {0}.", hostname);
  }
```

Licensed under the MIT license.

---------------------------------------
