namespace LemonCo.Core.Models;

/// <summary>
/// Represents a supplier (AutoCount AP Creditor)
/// </summary>
public class Supplier
{
    /// <summary>
    /// Supplier code (AutoCount Creditor AccNo)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Supplier company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Phone number (primary)
    /// </summary>
    public string? Phone1 { get; set; }

    /// <summary>
    /// Phone number (secondary)
    /// </summary>
    public string? Phone2 { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Address line 1
    /// </summary>
    public string? Address1 { get; set; }

    /// <summary>
    /// Address line 2
    /// </summary>
    public string? Address2 { get; set; }

    /// <summary>
    /// Address line 3
    /// </summary>
    public string? Address3 { get; set; }

    /// <summary>
    /// Address line 4
    /// </summary>
    public string? Address4 { get; set; }
}

