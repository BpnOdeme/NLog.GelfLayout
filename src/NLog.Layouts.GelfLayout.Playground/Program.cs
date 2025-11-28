using NLog.Layouts.GelfLayout.Features.Masking;
using NLog.Layouts.GelfLayout.Playground.Extensions;
using System.Text.Json;

namespace NLog.Layouts.GelfLayout.Playground;

internal class Program
{
    static void Main()
    {
        MaskingOptions maskingOptions = new()
        {
            CaseInsensitive = true,
            Enabled = true,
            FullExcludeAsEmpty = false,
            MaskChar = '*',
        };

        MaskingService _maskingService = new(maskingOptions);

        Console.WriteLine("=== MASKING PLAYGROUND ===\n");

        // Test Verisi Oluştur (Kapsamlı Model)
        // Buradaki alanlar maskingrules.json içindeki tüm alanları kapsayacak şekilde hazırlanmıştır.
        var model = new ComprehensiveCustomer
        {
            // --- Exclude True olanlar (Tamamen gizlenenler) ---
            Identity = "12345678901",
            NationalId = "99887766554",
            Birth = "1990-01-01",
            Cvv = "123",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            RefreshToken = "d777-8888-9999-aaaa",
            Password = "MySecretPassword123!",
            Pin = "1234",
            OtpCode = "556677",
            SubscriberNo = "SUB123456",
            PassportNo = "U12345678",
            Secret = "TopSecretData", // Attribute ile exclude

            // --- Prefix 2, Suffix 2 olanlar ---
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            FullName = "Ahmet Yılmaz",
            Surname = "Demir",
            UserName = "ahmet.y",
            MotherName = "Ayşe",
            FatherName = "Mehmet",

            // --- Prefix 3, Suffix 3 olanlar ---
            Address = "Atatürk Mah. Cumhuriyet Cad. No:1",
            FullAddress = "Atatürk Mah. Cumhuriyet Cad. No:1 D:5 İstanbul",
            ZipPostalCode = "34000",
            PhoneNumber = "+905551234567",
            Email = "ahmet.yilmaz@example.com",
            WalletNumber = "WL123456789",
            CustomerId = "CUST987654",
            Iban = "TR123456789012345678901234",
            FastIban = "TR987654321098765432109876",
            CardNo = "1111222233334444",
            CCNo = "5555666677778888",
            TaxNumber = "1234567890",
            VatNumber = "9876543210",
            
            // --- Özel Durumlar ve Diğerleri ---
            Product = "Laptop", // Kuralı yok, açık görünmeli
            AccountId = "ACC123", // Kuralı yok
            
            // --- Kısmi Eşleşme ve Attribute Testleri ---
            TCKN = "11223344556", // Attribute: Prefix=0, Suffix=0, Exclude=false (Full Mask)
            RawJson = """
            {
              "email": "jsonmail@example.com",
              "iban": "TR98000670000000000000000",
              "nested": {
                "password": "hiddenpassword",
                "note": "ok"
              }
            }
            """,
            Extra = new Dictionary<string, object?>
            {
                 ["father_name"] = "Mustafa", // snake_case testi (FatherName kuralına uymalı)
                 ["user_name"] = "mehmet.k", // snake_case
                 ["credit_card_no"] = "4444555566667777", // "CCNo" veya "CardNo" ile eşleşmez, ancak "CardNo" varsa? Hayır, exact match veya canonical match.
                 
                 ["passport_no"] = "P998877", // PassportNo (exclude)
                 ["cvv"] = "999" // Exclude
            }
        };

        // --- Min 3 Char Masking Logic Test ---
        var edgeCases = new EdgeCaseModel
        {
            Short2 = "AB",      // Length=2. MinMask=3 -> Full Mask
            Short3 = "ABC",     // Length=3. MinMask=3 -> Full Mask (Normalde 2+2 olsa bile)
            Short4 = "ABCD",    // Length=4. Prefix=2, Suffix=2 -> Mask=0. Adjusted: P=1, S=1 -> Mask=2 >= 2 mi? Hayır < 3. A**D
            Short5 = "ABCDE",   // Length=5. P=2, S=2 -> Mask=1 < 3. Adjusted P=1, S=1 -> Mask=3. -> A***E
            Exact6 = "ABCDEF",  // Length=6. P=3, S=3 -> Mask=0. Adjusted P=1, S=1 -> Mask=4. -> A****F
        };

        Console.WriteLine("--- Original Model (Before Mask) ---");

        // Maskeleme
        var masked = _maskingService.Mask(model);
        var maskedEdge = _maskingService.Mask(edgeCases);

        Console.WriteLine("\n--- Masked Comprehensive Model ---");
        Console.WriteLine(JsonSerializer.Serialize(masked, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine("\n--- Masked Edge Cases (Min 2-3 Char Logic) ---");
        Console.WriteLine(JsonSerializer.Serialize(maskedEdge, new JsonSerializerOptions { WriteIndented = true }));

        // Kısa String / Edge Case Testleri (Dictionary ile)
        Console.WriteLine("\n--- Edge Case Tests (Short Strings via Dictionary) ---");

        // Mask metodu IEnumerable görünce List<object> döndürdüğü için, giriş tipini de List<object> yapıyoruz.
        var edgeCaseList = new List<object>
        {
            new Dictionary<string, object> { 
                ["Scenario"] = "FirstName (3 chars) -> 2+2>3 -> Full Mask", 
                ["Data"] = new Dictionary<string, string> { ["FirstName"] = "Ali" } 
            },
            new Dictionary<string, object> { 
                ["Scenario"] = "FirstName (4 chars) -> 2+2=4 -> Reduce Keep(1,1) -> V**i", 
                ["Data"] = new Dictionary<string, string> { ["FirstName"] = "Veli" } 
            },
            new Dictionary<string, object> { 
                ["Scenario"] = "FirstName (5 chars) -> 2+2=4 -> Mask 1 < 3 -> Reduce Keep(1,1) -> A***t", 
                ["Data"] = new Dictionary<string, string> { ["FirstName"] = "Ahmet" } 
            },
            new Dictionary<string, object> { 
                ["Scenario"] = "Iban (6 chars) -> 3+3=6 -> Mask 0 -> Reduce Keep(1,1) -> T****4", 
                ["Data"] = new Dictionary<string, string> { ["Iban"] = "TR1234" } 
            }
        };
        
        var maskedEdgeCasesDict = _maskingService.Mask(edgeCaseList);
        Console.WriteLine(JsonSerializer.Serialize(maskedEdgeCasesDict, new JsonSerializerOptions { WriteIndented = true }));

		Console.WriteLine("\n--- Masked JSON Model ---");

		string jsonTest = """
        {
          "Identity": "12345678901",
          "NationalId": "99887766554",
          "Birth": "1990-01-01",
          "Cvv": "123",
          "ExpiryMonth": "12",
          "ExpiryYear": "2025",
          "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
          "RefreshToken": "d777-8888-9999-aaaa",
          "Password": "MySecretPassword123!",
          "Pin": "1234",
          "OtpCode": "556677",
          "SubscriberNo": "SUB123456",
          "PassportNo": "U12345678",
          "Secret": "TopSecretData",
          
          "FirstName": "Ahmet",
          "LastName": "Yılmaz",
          "FullName": "Ahmet Yılmaz",
          "Surname": "Demir",
          "UserName": "ahmet.y",
          "MotherName": "Ayşe",
          "FatherName": "Mehmet",
        
          "Address": "Atatürk Mah. Cumhuriyet Cad. No:1",
          "FullAddress": "Atatürk Mah. Cumhuriyet Cad. No:1 D:5 İstanbul",
          "ZipPostalCode": "34000",
          "PhoneNumber": "+905551234567",
          "Email": "ahmet.yilmaz@example.com",
          "WalletNumber": "WL123456789",
          "CustomerId": "CUST987654",
          "Iban": "TR123456789012345678901234",
          "FastIban": "TR987654321098765432109876",
          "CardNo": "1111222233334444",
          "CCNo": "5555666677778888",
          "TaxNumber": "1234567890",
          "VatNumber": "9876543210",
        
          "Product": "Laptop",
          "AccountId": "ACC123",
        
          "TCKN": "11223344556",
          
          "SenderName": "Ayşe Fatma",
          "SenderIban": "TR123456789012345678901234",
          "SenderIdentityNumber": "11223344556",
          "ReceiverName": "Ahmet Demir",
          "ReceiverIban": "TR987654321098765432109876",
          "ReceiverIdentityNumber": "99887766554"
        }
        """;
		var maskedJsonTest = _maskingService.Mask(jsonTest);
		Console.WriteLine(JsonSerializer.Serialize(maskedJsonTest).JsonPrettify());

		Console.WriteLine("\n=== TEST COMPLETED ===");
    }
}

public class EdgeCaseModel
{
    // Prefix=2, Suffix=2 varsayalım (FirstName gibi)
    [Mask(Prefix = 2, Suffix = 2)]
    public string Short2 { get; set; } = string.Empty; // "AB" -> "**"

    [Mask(Prefix = 2, Suffix = 2)]
    public string Short3 { get; set; } = string.Empty; // "ABC" -> "***"

	[Mask(Prefix = 2, Suffix = 2)]
    public string Short4 { get; set; } = string.Empty; // "ABCD" -> "A**D"

	[Mask(Prefix = 2, Suffix = 2)]
    public string Short5 { get; set; } = string.Empty; // "ABCDE" -> "A***E"

	// Prefix=3, Suffix=3 varsayalım (Email gibi)
	[Mask(Prefix = 3, Suffix = 3)]
    public string Exact6 { get; set; } = string.Empty; // "ABCDEF" -> "A****F" (Adjusted to 1-1)
}

public class ComprehensiveCustomer
{
    // Excluded Fields
    public string Identity { get; set; } = string.Empty;
	public string NationalId { get; set; } = string.Empty;
	public string Birth { get; set; } = string.Empty;
	public string Cvv { get; set; } = string.Empty;
	public string ExpiryMonth { get; set; } = string.Empty;
	public string ExpiryYear { get; set; } = string.Empty;
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string Pin { get; set; } = string.Empty;
	public string OtpCode { get; set; } = string.Empty;
	public string SubscriberNo { get; set; } = string.Empty;
	public string PassportNo { get; set; } = string.Empty;

	// Prefix 2, Suffix 2
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string Surname { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public string MotherName { get; set; } = string.Empty;
	public string FatherName { get; set; } = string.Empty;

	// Prefix 3, Suffix 3
	public string Address { get; set; } = string.Empty;
	public string FullAddress { get; set; } = string.Empty;
    public string ZipPostalCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WalletNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string FastIban { get; set; } = string.Empty;
    public string CardNo { get; set; } = string.Empty;
    public string CCNo { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;

    // Other / Manual Attributes
    [Mask(Exclude = true)]
    public string? Secret { get; set; }

    [Mask(Prefix = 0, Suffix = 0, Exclude = false)]
    public string? TCKN { get; set; } 

    public string Product { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;

    public Dictionary<string, object?>? Extra { get; set; }
    public string? RawJson { get; set; }
}