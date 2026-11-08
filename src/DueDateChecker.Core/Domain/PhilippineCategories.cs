using System.Text;

namespace DueDateChecker.Core.Domain;

public sealed record PhilippineCategory(string Id, string Name, string GroupId, string GroupName, string Icon, string SearchTerms = "");

/// <summary>F07 preset catalog. Stable IDs; due type/amount/recurrence policy is independent.</summary>
public static class PhilippineCategories
{
    public const string OtherId = "ph.other.general";
    public static IReadOnlyList<PhilippineCategory> All { get; } = Array.AsReadOnly(new PhilippineCategory[]
    {
        new("ph.housing.rent", "House Rent / Apartment Rent", "housing", "Housing & Property", "home", "renta upa bahay apartment"),
        new("ph.housing.mortgage", "Mortgage / Housing Loan", "housing", "Housing & Property", "home", ""),
        new("ph.housing.condo", "Condominium Dues", "housing", "Housing & Property", "home", ""),
        new("ph.housing.hoa", "HOA / Subdivision Dues", "housing", "Housing & Property", "home", ""),
        new("ph.housing.amilyar", "Real Property Tax (Amilyar)", "housing", "Housing & Property", "home", "amilyar property tax"),
        new("ph.utilities.electricity", "Electricity", "utilities", "Utilities & Telecommunications", "bolt", "kuryente electric power"),
        new("ph.utilities.water", "Water", "utilities", "Utilities & Telecommunications", "water", "tubig"),
        new("ph.utilities.internet", "Internet", "utilities", "Utilities & Telecommunications", "wifi", "wifi broadband"),
        new("ph.utilities.mobile", "Mobile Postpaid", "utilities", "Utilities & Telecommunications", "mobile", "cellphone phone postpaid"),
        new("ph.utilities.cable", "Cable TV", "utilities", "Utilities & Telecommunications", "tv", ""),
        new("ph.credit.card", "Credit Card Bills", "credit", "Loans & Credit", "credit", "creditcard"),
        new("ph.credit.personal", "Personal Loans", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.salary", "Salary Loans", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.vehicle", "Motorcycle / Car Loans", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.housing", "Housing Loans", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.gadget", "Gadget Installments", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.bnpl", "Buy Now, Pay Later", "credit", "Loans & Credit", "credit", ""),
        new("ph.credit.digital", "Digital Lending Payments", "credit", "Loans & Credit", "credit", ""),
        new("ph.government.sss", "SSS Contributions", "government", "Government Contributions & Taxes", "government", ""),
        new("ph.government.philhealth", "PhilHealth Contributions", "government", "Government Contributions & Taxes", "government", ""),
        new("ph.government.pagibig", "Pag-IBIG Contributions", "government", "Government Contributions & Taxes", "government", "pagibig pag ibig"),
        new("ph.government.mp2", "Pag-IBIG MP2 Savings", "government", "Government Contributions & Taxes", "government", "pagibig pag ibig"),
        new("ph.government.bir", "BIR Tax Payments", "government", "Government Contributions & Taxes", "government", ""),
        new("ph.government.permit", "Business Permit Renewals", "government", "Government Contributions & Taxes", "government", ""),
        new("ph.government.lto", "Vehicle Registration (LTO)", "government", "Government Contributions & Taxes", "government", ""),
        new("ph.insurance.life", "Life Insurance", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.insurance.health", "Health Insurance / HMO", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.insurance.vehicle", "Car / Motorcycle Insurance", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.insurance.property", "Property Insurance", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.insurance.medical", "Medical Bills", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.insurance.membership", "Healthcare Memberships", "insurance", "Insurance & Healthcare", "health", ""),
        new("ph.education.tuition", "Tuition Fees", "education", "Education", "school", "paaralan school matrikula"),
        new("ph.education.misc", "School Miscellaneous Fees", "education", "Education", "school", ""),
        new("ph.education.review", "Review Center Payments", "education", "Education", "school", ""),
        new("ph.education.loan", "Educational Loans", "education", "Education", "school", ""),
        new("ph.subscriptions.streaming", "Streaming Services", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.subscriptions.software", "Software Subscriptions", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.subscriptions.cloud", "Cloud Storage", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.subscriptions.gym", "Gym Memberships", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.subscriptions.professional", "Professional Memberships", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.subscriptions.other", "Other Recurring Subscriptions", "subscriptions", "Subscriptions & Memberships", "subscription", ""),
        new("ph.family.support", "Family Support / Allowance", "family", "Family & Personal Obligations", "family", ""),
        new("ph.family.childcare", "Childcare / Daycare Fees", "family", "Family & Personal Obligations", "family", ""),
        new("ph.family.household", "Household Services", "family", "Family & Personal Obligations", "family", ""),
        new("ph.family.association", "Association Contributions", "family", "Family & Personal Obligations", "family", ""),
        new("ph.family.personal", "Other Personal Commitments", "family", "Family & Personal Obligations", "family", ""),
        new("ph.business.rent", "Business Rent", "business", "Business & Professional", "business", ""),
        new("ph.business.supplier", "Supplier Payments", "business", "Business & Professional", "business", ""),
        new("ph.business.loan", "Business Loans", "business", "Business & Professional", "business", ""),
        new("ph.business.saas", "Software / SaaS Subscriptions", "business", "Business & Professional", "business", ""),
        new("ph.business.license", "Professional License Renewals", "business", "Business & Professional", "business", ""),
        new("ph.business.tax", "Business Taxes and Government Fees", "business", "Business & Professional", "business", ""),
        new("ph.other.general", "Other", "other", "Other", "other", ""),
    });
    private static readonly IReadOnlyDictionary<string, PhilippineCategory> ById =
        All.ToDictionary(item => item.Id, StringComparer.Ordinal);
    public static bool IsKnown(string? id) => id is not null && ById.ContainsKey(id);
    public static PhilippineCategory? Find(string? id) => id is not null && ById.TryGetValue(id, out var item) ? item : null;
    public static PhilippineCategory Get(string id) => Find(id) ?? throw new ArgumentException("Choose a supported Philippine category.", nameof(id));
    public static IReadOnlyList<PhilippineCategory> Search(string? query)
    {
        var words = Normalize(query ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return All.Where(item => words.All(word => Normalize(item.Name + " " + item.GroupName + " " + item.SearchTerms)
            .Contains(word, StringComparison.Ordinal))).ToArray();
    }
    private static string Normalize(string value) => value.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
}
