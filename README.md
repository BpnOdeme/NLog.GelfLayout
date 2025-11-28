# NLog.GelfLayout
[![Version](https://img.shields.io/nuget/v/NLog.GelfLayout.svg)](https://www.nuget.org/packages/NLog.GelfLayout) 

GelfLayout-package contains custom layout renderer for [NLog] to format log messages as [GELF] Json structures for [GrayLog]-server.

## Usage

### Install from Nuget
```
PM> Install-Package NLog.GelfLayout
```

### Parameters
- _IncludeEventProperties_ - Include all properties from the LogEvent. Boolean. Default = true
- _IncludeScopeProperties_ - Include all properties from NLog MDLC / MEL BeginScope. Boolean. Default = false
- _IncludeGdc_ - Include all properties from NLog GlobalDiagnosticsContext. Default = false
- _ExcludeProperties_ - Comma separated string with LogEvent property names to exclude. 
- _IncludeLegacyFields_ - Include deprecated fields no longer part of official GelfVersion 1.1 specification. Boolean. Default = false
- _Facility_ - Legacy Graylog Message Facility-field, when specifed it will fallback to legacy GelfVersion 1.0. Ignored when IncludeLegacyFields=False
- _HostName_ - Override Graylog Message Host-field. Default: `${hostname}`
- _FullMessage_ - Override Graylog Full-Message-field. Default: `${message}`
- _ShortMessage_ - Override Graylog Short-Message-field. Default: `${message}`

### Sample Usage with RabbitMQ Target
You can configure this layout for [NLog] Targets that respect Layout attribute. 
For instance the following configuration writes log messages to a [RabbitMQ-adolya] Exchange in [GELF] format.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >
  <extensions>
    <add assembly="NLog.Targets.RabbitMQ" />
    <add assembly="NLog.Layouts.GelfLayout" />
  </extensions>
  
  <targets async="true">
    <target name="RabbitMQTarget"
            xsi:type="RabbitMQ"
            hostname="mygraylog.mycompany.com"
            exchange="logmessages-gelf"
            durable="true"
            useJSON="false"
            layout="${gelf}"
    />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="RabbitMQTarget" />
  </rules>
</nlog>
```

In this example there would be a [Graylog2] server that consumes the queued [GELF] messages. 

### Sample Usage with NLog Network Target and HTTP
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >
  <extensions>
    <add assembly="NLog.Layouts.GelfLayout" />
  </extensions>
  
  <targets async="true">
	<target xsi:type="Network" name="GelfHttp" address="http://localhost:12201/gelf" layout="${gelf}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="GelfHttp" />
  </rules>
</nlog>
```

### Sample Usage with NLog Network Target and TCP
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >
  <extensions>
    <add assembly="NLog.Layouts.GelfLayout" />
  </extensions>
  
  <targets async="true">
	<target xsi:type="Network" name="GelfTcp" address="tcp://graylog:12200" layout="${gelf}" newLine="true" lineEnding="Null" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="GelfTcp" />
  </rules>
</nlog>
```

### Sample Usage with NLog Network Target and UDP

Notice the options `Compress="GZip"` and `compressMinBytes="1024"` requires NLog v5.0

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >
  <extensions>
    <add assembly="NLog.Layouts.GelfLayout" />
  </extensions>
  
  <targets async="true">
	<target xsi:type="Network" name="GelfUdp" address="udp://graylog:12201" layout="${gelf}" compress="GZip" compressMinBytes="1000" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="GelfUdp" />
  </rules>
</nlog>
```

Notice when message exceeds the default MTU-size (usually 1500 bytes), then the IP-network-layer will attempt to perform IP-fragmentation and handle messages up to 65000 bytes.
But IP fragmentation will fail if the network switch/router has been configured to have DontFragment enabled, where it will drop the network packets.
Usually one will only use UDP on the local network, since no authentication or security, and network switches on the local-network seldom has DontFragment enabled (or under your control to be configured).

### Sample Usage with custom extra fields

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >
  <extensions>
    <add assembly="NLog.Layouts.GelfLayout" />
  </extensions>
  
  <targets async="true">
	<target xsi:type="Network" name="GelfHttp" address="http://localhost:12201/gelf">
		<layout type="GelfLayout">
			<field name="threadid" layout="${threadid}" />
		</layout>
	</target>
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="GelfHttp" />
  </rules>
</nlog>
```

## Credits
[GELF] converter module is all taken from [Gelf4NLog] by [Ozan Seymen](https://github.com/seymen)

[NLog]: http://nlog-project.org/
[GrayLog]: https://www.graylog.org/features/gelf
[GELF]: https://docs.graylog.org/docs/gelf
[Gelf4NLog]: https://github.com/seymen/Gelf4NLog
[RabbitMQ-haf]: https://github.com/haf/NLog.RabbitMQ
[RabbitMQ-adolya]: https://www.nuget.org/packages/Nlog.RabbitMQ.Target/

# MaskingService — Alan Bazlı Veri Maskeleme Servisi

Bu kütüphane, **NLog Layouts** veya genel .NET loglama süreçlerinde hassas verileri maskelemek için tasarlanmıştır.  
Hem JSON hem de .NET nesneleri üzerinde çalışır, alan adlarına (property names) göre **ön ek / son ek koruma** veya **tam maskeleme / exclude** işlemleri uygular.

## Yeni bir projeye kütüphane eklenirken yapılacak değişiklikler:
- .nuget/nuget.config dosyasında ilgili değişiklikler yapılır.
- Dockerfile içerisindeki restore adımına --configfile .nuget/nuget.config ifadesi eklenir.
- .github/workflows/build-dotnet-api.yml içerisinde restore adımına --configfile .nuget/nuget.config ifadesi eklenir.

Bu kütüphaneyi kullanacak projelerde yalnızca aşağıdaki yapılandırmalara dikkat etmek gerekmektedir:
```csharp
// This method should always be above the Nlog configuration methods
builder.Services.AddMaskingService(configuration);

builder.Logging.ClearProviders();
builder.UseNLogWithServiceProviderSupport(); // MGW projesinde extension method olarak
```

Maskeleme servisi NLog Gelf Layouts yapılandırmalarından önce DI'ya eklenmelidir!

---

## Kurulum ve Konfigürasyon

### `appsettings.json` Örneği

```json
"MaskingConfiguration": {
    "Enabled": true,
    "MaskChar": "*",
    "FullExcludeAsEmpty": true, // Exclude=true ise "" döndür (false yaparsanız tümü * ile kaplanır)
    "Rules": [
        {
            "Field": "AccountNumber",
            "Prefix": 6,
            "Suffix": 4,
            "Exclude": false
        },
        {
            "Field": "NationalId",
            "Prefix": 4,
            "Suffix": 4,
            "Exclude": false
        },
        {
            "Field": "SerialNo",
            "Prefix": 3,
            "Suffix": 3,
            "Exclude": false
        },
        {
            "Field": "FirstName",
            "Prefix": 3,
            "Suffix": 3,
            "Exclude": false
        },
        {
            "Field": "LastName",
            "Prefix": 3,
            "Suffix": 3,
            "Exclude": false
        },
        {
            "Field": "MotherName",
            "Prefix": 3,
            "Suffix": 3,
            "Exclude": false
        },
        {
            "Field": "FatherName",
            "Prefix": 3,
            "Suffix": 3,
            "Exclude": false
        }
    ],
    "CaseInsensitive": true
}
```

---

## Kullanım — Dependency Injection Üzerinden
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Layouts.GelfLayout.Features.Masking;

// appsettings.json içeriği configuration'a yüklü olmalı
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// MaskingOptions binding
var options = configuration
    .GetSection("MaskingConfiguration")
    .Get<MaskingOptions>();

// DI kaydı
var services = new ServiceCollection();
services.AddSingleton<IMaskingService>(new MaskingService(options));

// resolve
var provider = services.BuildServiceProvider();
var maskingService = provider.GetRequiredService<IMaskingService>();
```

## Kullanım Örnekleri

### JSON String Üzerinde
```csharp
string json = @"{
  ""AccountNumber"": ""1234567890123456"",
  ""NationalId"": ""12345678901"",
  ""FirstName"": ""Jonathan"",
  ""LastName"": ""Doe""
}";

var masked = maskingService.Mask(json);
Console.WriteLine(masked);
```

#### Önce:
```json
{
  "AccountNumber": "1234567890123456",
  "NationalId": "12345678901",
  "FirstName": "Jonathan",
  "LastName": "Doe"
}
```

#### Sonra:
```json
{
  "AccountNumber": "123456******3456",
  "NationalId": "1234***8901",
  "FirstName": "Jo****an",
  "LastName": "D*e"
}
```

### Dictionary Üzerinde
```csharp
var data = new Dictionary<string, object?>
{
    ["AccountNumber"] = "1234567890123456",
    ["NationalId"] = "12345678901",
    ["FirstName"] = "Jonathan"
};

maskingService.Mask(data);

foreach (var kv in data)
    Console.WriteLine($"{kv.Key}: {kv.Value}");

```

#### Sonuç:
```
AccountNumber: 123456******3456
NationalId: 1234***8901
FirstName: Jo****an
```

### Sınıf (Strongly Typed Object) Üzerinde

```csharp
public class Customer
{
    public string AccountNumber { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}

var customer = new Customer
{
    AccountNumber = "1234567890123456",
    NationalId = "12345678901",
    FirstName = "Jonathan",
    LastName = "Doe"
};

maskingService.Mask(customer);

Console.WriteLine($"AccountNumber: {customer.AccountNumber}");
Console.WriteLine($"NationalId: {customer.NationalId}");
Console.WriteLine($"FirstName: {customer.FirstName}");
Console.WriteLine($"LastName: {customer.LastName}");
```

#### Sonuç:
```
AccountNumber: 123456******3456
NationalId: 1234***8901
FirstName: Jo****an
LastName: D*e
```

---

## Kuralların Açıklaması
| **Alan** | **Açıklama** |
|-----------|---------------|
| **Field** | Maske uygulanacak alan adı (case-insensitive olabilir). |
| **Prefix** | Değerin solundan korunacak karakter sayısı. |
| **Suffix** | Değerin sağından korunacak karakter sayısı. |
| **Exclude** | Alan tamamen dışlanacaksa `true` yapılır. |
| **MaskChar** | Maskelemede kullanılacak karakter (varsayılan `*`). |
| **FullExcludeAsEmpty** | `Exclude=true` olduğunda boş string mi (`true`) yoksa tam maske mi (`false`) döneceğini belirler. |


## Özellik Özeti
| **Özellik** | **Açıklama** |
|--------------|---------------|
| **Enabled** | Maskeleme aktif/pasif. |
| **MaskChar** | Maskeleme karakteri. |
| **CaseInsensitive** | Alan adında küçük/büyük harf farkı gözetilmez. |
| **FullExcludeAsEmpty** | `Exclude=true` olduğunda boş string mi dönecek. |
| **Rules** | Alan bazlı kural listesi. |
| **[Mask] Attribute** | Sınıf özelliklerinde manuel maskeleme kuralı tanımlamak için. |

## Performans
- Reflection planları (**MemberPlan**) ilk kullanımda oluşturulup cache’e alınır.  
- Aynı tipteki nesnelerde sonraki maskeleme işlemleri çok hızlıdır.  
- JSON parse işlemleri **System.Text.Json.Nodes** üzerinden yapılır.  

## Genel Akış
1. **Kurallar yüklenir (Rules).**  
2. **Alan adı eşleşirse** maskeleme uygulanır (**Prefix**, **Suffix**, **Exclude**).  
3. **Alan adı eşleşmezse**, içerik rekürsif olarak taranır (iç nesne / dizi / dictionary).  
4. **JSON string olarak gelirse**, parse edilip içindeki alanlar maske kurallarıyla işlenir.  
5. **Primitive tipler** dokunulmadan döndürülür.  
