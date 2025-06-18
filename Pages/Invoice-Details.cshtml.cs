using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages
{
    public class Invoice_DetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public Invoice_DetailsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InvoiceHistory InvoiceHistory { get; set; } = default!;

        [BindProperty]
        public List<AdditionalPackageItem> additionalPackageItems { get; set; }

        [BindProperty]
        public List<PackageFeature> PackageFeature { get; set; }

        [BindProperty]
        public Package SelectedPackage { get; set; }

        public InvoiceHistory InvoiceOrder { get; set; } = default!;
        public AddressData billingAddress { get; set; } = default!;
        public User User { get; set; } = default!;

        public async Task<IActionResult> OnGet(int id)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var orderId = Request.Cookies["OrderId"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            InvoiceHistory = await _context.InvoiceHistory.Where(m => m.InvoiceId == id && m.InvoiceBy == userId).FirstOrDefaultAsync();

            SelectedPackage = await _context.packages.Where(x => x.PackageId == InvoiceHistory.PackageId).FirstOrDefaultAsync();

            //Package Features for selected package
            var features = _context.packages.Where(x => x.PackageName == InvoiceHistory.PackageName).FirstOrDefault().PackageFeatures;

            var SelectedPackageFeatures = features != null ? features : string.Empty;
            var AddPackageFeaturesItemIds = SelectedPackageFeatures.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToList();

            PackageFeature = _context.PackageFeatures.Where(x => AddPackageFeaturesItemIds.Contains(x.FeatureId)).ToList();

            // Additional Package
            var SelectedAddPackageItems = InvoiceHistory != null ? InvoiceHistory.AdditionalPackageItemIds : string.Empty;
            var AddPackageItemIds = SelectedAddPackageItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToList();

            additionalPackageItems = _context.AdditionalPackageItems.Where(x => AddPackageItemIds.Contains(x.AdditionalPackageItemId)).ToList();

            return Page();
        }

        public IActionResult OnGetViewPDF(int invoiceId)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var invoiceOrderId = Request.Cookies["OrderId"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            InvoiceOrder = _context.InvoiceHistory.Where(m => m.InvoiceId == invoiceId && m.InvoiceBy == userId).FirstOrDefault();

            billingAddress = _context.AddressData.FirstOrDefault(x => x.UserId == userId && x.IsBilling == true);
            User = _context.Users.FirstOrDefault(x => x.Email == userEmail);

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text($"Customer Ref: {InvoiceOrder?.InvoiceBy}").FontSize(10);
                            column.Item().Text($"Invoice Ref: {invoiceId}").FontSize(10);
                            column.Item().Text($"Order Ref: {InvoiceOrder?.OrderId}").FontSize(10);
                            column.Item().Text($"Invoice Date: {InvoiceOrder?.InvoiceDate.ToString("dd'/'MM'/'yyyy")}").FontSize(10);
                            column.Item().PaddingTop(40);
                            column.Item().Text($"{User?.Forename}").Bold().FontSize(10);
                            column.Item().Text($"{billingAddress?.HouseName}").FontSize(10);
                            column.Item().Text($"{billingAddress?.Street}").FontSize(10);
                            column.Item().Text($"{billingAddress?.Town}").FontSize(10);
                            column.Item().Text($"{billingAddress?.County}").FontSize(10);
                            column.Item().Text($"{billingAddress?.PostCode}").FontSize(10);
                            column.Item().Text($"{billingAddress?.Country}").FontSize(10);
                        });

                        row.ConstantItem(200).AlignRight().Column(column =>
                        {
                            column.Item().Text("Spendwise Limited").Bold().FontSize(10);
                            column.Item().Text("Registered in England & Wales").FontSize(10);
                            column.Item().Text("at 1st Floor 17 Albemarle Street, London").FontSize(10);
                            column.Item().Text("W1S 4HP").FontSize(10);
                            column.Item().Text("England").FontSize(10);
                            column.Item().Text("Email: info@spendwisefin.com").FontSize(10);
                            column.Item().Text("Tel: 020 3897 2233").FontSize(10);
                        });
                    });

                    page.Content().Element(ComposeContent);
                });
            });

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var pdfBytes = pdf.GeneratePdf();
            return File(pdfBytes, "application/pdf");
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Item().Text("INVOICE").FontSize(16).Bold().AlignCenter();
                column.Item().PaddingTop(10).Border(1).Background(Colors.Grey.Lighten5).Padding(5).Text($"{InvoiceOrder?.CompanyName}").Bold().FontSize(10);
                column.Item().PaddingTop(10).Element(ComposeTable);
                column.Item().PaddingTop(20).Border(1).Padding(5).Text("If this invoice is for ongoing services and you have requested us to take payment using the continuous authority credit or debit card details stored on our\r\nsystem, then we will do so and no further action is required");
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(50);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Product").Bold();
                    header.Cell().Element(CellStyle).Text("Qty").Bold();
                    header.Cell().Element(CellStyle).Text("Unit Price").Bold();
                    header.Cell().Element(CellStyle).Text("Net").Bold();
                    header.Cell().Element(CellStyle).Text("VAT").Bold();
                    header.Cell().Element(CellStyle).Text("Gross").Bold();

                    static IContainer CellStyle(IContainer container) =>
                        container.Border(1).Background(Colors.Grey.Lighten3).Padding(5).BorderColor(Colors.Grey.Medium);
                });

                decimal totalNet = InvoiceOrder.NetAmount;
                decimal totalVat = InvoiceOrder.VatAmount;
                decimal totalGross = InvoiceOrder.TotalAmount;

                //Bind Invoice details
                string[][] items = new[]
                {
                    new[] { $"{InvoiceOrder.PackageName}", "1", $"{InvoiceOrder.NetAmount}", $"{InvoiceOrder.NetAmount}", $"{InvoiceOrder.VatAmount}", $"{InvoiceOrder.TotalAmount}" },
                };

                foreach (var item in items)
                {
                    foreach (var value in item)
                    {
                        table.Cell().Element(CellStyle).Text(value);
                    }

                    static IContainer CellStyle(IContainer container) =>
                        container.BorderLeft(1).BorderRight(1).Padding(5).BorderColor(Colors.Grey.Medium);
                }

                //Package Features for selected package
                var features = _context.packages.Where(x => x.PackageName == InvoiceOrder.PackageName).FirstOrDefault().PackageFeatures;

                var SelectedPackageFeatures = features != null ? features : string.Empty;
                var AddPackageFeaturesItemIds = SelectedPackageFeatures.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse)
                       .ToList();

                PackageFeature = _context.PackageFeatures.Where(x => AddPackageFeaturesItemIds.Contains(x.FeatureId)).ToList();

                foreach (var item in PackageFeature)
                {
                    string[][] data = new[]
                    {
                        new[] { $"- {item.Feature}", "1", "", "", "", "" },
                    };

                    foreach (var featureItems in data)
                    {
                        foreach (var value in featureItems)
                        {
                            table.Cell().Element(CellStyle).Text(value).FontColor(Colors.Grey.Darken2);
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container.BorderLeft(1).BorderRight(1).Padding(5).BorderColor(Colors.Grey.Medium);
                    }
                }

                // Additional Package
                var SelectedAddPackageItems = InvoiceOrder != null ? InvoiceOrder.AdditionalPackageItemIds : string.Empty;
                var AddPackageItemIds = SelectedAddPackageItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse)
                       .ToList();

                additionalPackageItems = _context.AdditionalPackageItems.Where(x => AddPackageItemIds.Contains(x.AdditionalPackageItemId)).ToList();

                foreach (var item in additionalPackageItems)
                {
                    decimal price = item.price;
                    decimal vat = price * 0.20m;
                    decimal gross = price + vat;

                    // Update totals
                    totalNet += price;
                    totalVat += vat;
                    totalGross += gross;

                    string[][] data = new[]
                    {
                        new[] { $"{item.ItemName}", "1", $"{item.price}", $"{item.price}", $"{vat:0.00}", $"{gross:0.00}" },
                    };

                    foreach (var addItems in data)
                    {
                        foreach (var value in addItems)
                        {
                            table.Cell().Element(CellStyle).Text(value);
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container.BorderLeft(1).BorderRight(1).Padding(5).BorderColor(Colors.Grey.Medium);
                    }
                }

                string[][] emptyData = new[]
                {
                    new[] { "", "", "", "", "", "" },
                };

                foreach (var addItems in emptyData)
                {
                    foreach (var value in addItems)
                    {
                        table.Cell().Element(CellStyle).Text(value);
                    }

                    static IContainer CellStyle(IContainer container) =>
                        container.BorderLeft(1).BorderRight(1).BorderBottom(1).BorderColor(Colors.Black);
                }

                table.Cell().ColumnSpan(3).AlignRight().Padding(5).Text("Totals:").Bold().FontSize(10);
                table.Cell().Element(CellTotalStyle).Text($"{totalNet:0.00}").Bold(); // Net Amount
                table.Cell().Element(CellTotalStyle).Text($"{totalVat:0.00}").Bold(); // Vat Amount
                table.Cell().Element(CellTotalStyle).Text($"{totalGross:0.00}").Bold(); // Gross Amount

                static IContainer CellTotalStyle(IContainer container) =>
                            container.Border(1).Padding(5).BorderColor(Colors.Grey.Medium);
            });
        }
    }
}
