using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Spendwise_WebApp.Models;
using System.ComponentModel.Design;

namespace Spendwise_WebApp.Pages.FormationPage
{
    [IgnoreAntiforgeryToken]
    public class CompanyFormationSubTabModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;
        public CompanyFormationSubTabModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            _context = context;
            _config = config;
            _environment = environment;
        }

        [BindProperty]
        public Particular Particular { get; set; } = default!;

        [BindProperty]
        public List<AddressData> AddressList { get; set; } = default!;

        public User User { get; set; } = default!;

        [BindProperty]
        public AddressData Address { get; set; } = default!;

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        [BindProperty]
        public List<SicCodes> SIC_CodeList { get; set; } = default!;

        public List<SelectListItem> SicCodeCategoryList { get; set; } = default!;

        [BindProperty]
        public List<CompanyOfficer> OfficersList { get; set; } = default!;

        //Document Tab
        [BindProperty]
        public List<DocumentUploadModel> IdentityFiles { get; set; }

        [BindProperty]
        public List<DocumentUploadModel> AddressFiles { get; set; }


        [BindProperty]
        public List<Document> UploadedDocuments { get; set; }

        public async Task<IActionResult> OnGet()
        {
            List<AddressData> addressData;
            List<CompanyOfficer> officerData;
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            addressData = await _context.AddressData.Where(m => m.UserId == userId).ToListAsync();
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId.ToString() == selectCompanyId);
            Particular = await _context.Particulars.FirstOrDefaultAsync(m => m.UserId == userId && m.CompanyId.ToString() == selectCompanyId);

            var SelectedSIC_Codes = Particular != null ? Particular.SIC_Code : string.Empty;
            var AddSIC_CodesItemIds = SelectedSIC_Codes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToList();

            SIC_CodeList = await _context.SicCodes.Where(x => AddSIC_CodesItemIds.Contains(x.SicCode)).ToListAsync();
            officerData = await _context.CompanyOfficers.Where(m => m.UserId == userId && m.CompanyID == companyId).ToListAsync();
            UploadedDocuments = await _context.Documents.Where(x => x.UserId == userId && x.CompanyId == companyId).ToListAsync();

            if (addressData == null && officerData == null)
            {
                return NotFound();
            }
            else
            {
                OfficersList = officerData;
                AddressList = addressData;
            }
            SicCodeCategoryList = await _context.SicCodeCategory.Select(p => new SelectListItem
            {
                Text = p.Category,
                Value = p.Section.ToString()
            }).ToListAsync();
            return Page();
        }

        public async Task<JsonResult> OnPostSaveParticularData([FromBody] Particular particular)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            var invoiceId = _context.InvoiceHistory.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault()?.InvoiceId;
            var orderId = _context.Orders.Where(c => c.CompanyId == companyId).FirstOrDefault().OrderId;
            var companyDetails = await _context.CompanyDetails.Where(x => x.Createdby == userId && x.CompanyId == companyId).FirstOrDefaultAsync();
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            Particular UserParticularsData = await _context.Particulars.FirstOrDefaultAsync(x => x.UserId == userId && x.CompanyId == companyId);
            if (UserParticularsData != null)
            {
                UserParticularsData.UserId = userId;
                UserParticularsData.CompanyId = companyId;
                UserParticularsData.CompanyName = particular.CompanyName;
                UserParticularsData.CompanyType = particular.CompanyType;
                UserParticularsData.Jurisdiction = particular.Jurisdiction;
                UserParticularsData.Activities = particular.Activities;
                UserParticularsData.SIC_Code = particular.SIC_Code;
              
                await _context.SaveChangesAsync();
            }
            else
            {
                particular.UserId = userId;
                particular.CompanyId = companyId;
                _context.Particulars.Add(particular);
            }

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE CompanyDetails SET CompanyName = @CompanyName WHERE CompanyId = @CompanyId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", particular.CompanyName);
                    cmd.Parameters.AddWithValue("@CompanyId", companyDetails.CompanyId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Data not found.");
                        return new JsonResult(new { success = false });
                    }
                }

                string orderQuery = "UPDATE Orders SET CompanyName = @CompanyName WHERE OrderId = @OrderId";
                using (var cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", particular.CompanyName);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Data not found.");
                        return new JsonResult(new { success = false });
                    }
                }

                string invoiceQuery = "UPDATE InvoiceHistory SET CompanyName = @CompanyName WHERE InvoiceId = @InvoiceId";
                using (var cmd = new SqlCommand(invoiceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", particular.CompanyName);
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Data not found.");
                        return new JsonResult(new { success = false });
                    }
                }
            }
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public JsonResult OnGetGetSicCodeByCategory(string section)
        {
            try
            {
                if (!string.IsNullOrEmpty(section))
                {
                    var sicCodeList = _context.SicCodes
                        .Where(x => x.Section.ToLower() == section.ToLower())
                        .ToList();

                    return new JsonResult(new { success = true, data = sicCodeList });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Section is required." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred.", error = ex.Message });
            }

        }

        public class SaveAddressWithEmailRequest
        {
            public string RegisteredEmail { get; set; }
            public AddressData Address { get; set; }
        }
        public async Task<JsonResult> OnPostSaveResidentialAddress([FromBody] SaveAddressWithEmailRequest request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            var addressData = request.Address;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            if (addressData.AddressId > 0)
            {

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsRegisteredOffice = @IsRegisteredOffice, CompanyId = @CompanyId WHERE AddressId = @AddressId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HouseName", addressData.HouseName);
                        cmd.Parameters.AddWithValue("@Street", addressData.Street);
                        cmd.Parameters.AddWithValue("@Town", addressData.Town);
                        cmd.Parameters.AddWithValue("@Locality", addressData.Locality);
                        cmd.Parameters.AddWithValue("@PostCode", addressData.PostCode);
                        cmd.Parameters.AddWithValue("@County", addressData.County);
                        cmd.Parameters.AddWithValue("@Country", addressData.Country);
                        cmd.Parameters.AddWithValue("@IsRegisteredOffice", true);
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@AddressId", addressData.AddressId);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError("", "Data not found.");
                            return new JsonResult(new { success = false });
                        }
                    }
                }
            }
            else
            {
                addressData.IsRegisteredOffice = true;
                addressData.CompanyId = companyId;
                addressData.UserId = userId;

                // STEP 1: Reset IsCurrent = false for any current address for this user & company
                var existingCurrentAddresses = _context.AddressData
                    .Where(x => x.CompanyId == companyId && x.UserId == userId && x.IsCurrent && x.IsRegisteredOffice == true);

                foreach (var addr in existingCurrentAddresses)
                {
                    addr.IsCurrent = false;
                    _context.AddressData.Update(addr);
                }

                // STEP 2: Check if exact same address already exists
                var existingAddress = _context.AddressData.FirstOrDefault(x =>
                                       x.HouseName == addressData.HouseName &&
                                       x.Street == addressData.Street &&
                                       x.Locality == addressData.Locality &&
                                       x.Town == addressData.Town &&
                                       x.County == addressData.County &&
                                       x.Country == addressData.Country &&
                                       x.PostCode == addressData.PostCode &&
                                       x.CompanyId == companyId &&
                                       x.IsRegisteredOffice == true
                                   );

                if (existingAddress != null)
                {
                    // Case 2: Same address exists → update IsCurrent to true
                    existingAddress.IsCurrent = true;
                    _context.AddressData.Update(existingAddress);
                }
                else
                {
                    // Case 3: New unique address → insert new record
                    addressData.IsCurrent = true;
                    _context.AddressData.Add(addressData);
                }


                //addressData.IsCurrent = true;
                //_context.AddressData.Add(addressData);
            }

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE CompanyDetails SET RegisteredEmail = @RegisteredEmail WHERE CompanyId = @CompanyId and Createdby = @Createdby";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RegisteredEmail", request.RegisteredEmail);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@Createdby", userId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    //if (rowsAffected == 0)
                    //{
                    //    ModelState.AddModelError("", "Data not found.");
                    //    return new JsonResult(new { success = false });
                    //}
                }
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<JsonResult> OnPostSaveBusinessAddress([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            if (request.AddressId > 0)
            {

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsBusiness = @IsBusiness, CompanyId = @CompanyId WHERE AddressId = @AddressId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HouseName", request.HouseName);
                        cmd.Parameters.AddWithValue("@Street", request.Street);
                        cmd.Parameters.AddWithValue("@Town", request.Town);
                        cmd.Parameters.AddWithValue("@Locality", request.Locality);
                        cmd.Parameters.AddWithValue("@PostCode", request.PostCode);
                        cmd.Parameters.AddWithValue("@County", request.County);
                        cmd.Parameters.AddWithValue("@Country", request.Country);
                        cmd.Parameters.AddWithValue("@IsBusiness", true);
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@AddressId", request.AddressId);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError("", "Data not found.");
                            return new JsonResult(new { success = false });
                        }
                    }
                }
            }
            else
            {
                request.IsBusiness = true;
                request.CompanyId = companyId;
                request.UserId = userId;


                // STEP 1: Reset IsCurrent = false for any current address for this user & company
                var existingCurrentAddresses = _context.AddressData
                    .Where(x => x.CompanyId == companyId && x.UserId == userId && x.IsCurrent && x.IsBusiness == true);

                foreach (var addr in existingCurrentAddresses)
                {
                    addr.IsCurrent = false;
                    _context.AddressData.Update(addr);
                }

                // STEP 2: Check if exact same address already exists
                var existingAddress = _context.AddressData.FirstOrDefault(x =>
                                       x.HouseName == request.HouseName &&
                                       x.Street == request.Street &&
                                       x.Locality == request.Locality &&
                                       x.Town == request.Town &&
                                       x.County == request.County &&
                                       x.Country == request.Country &&
                                       x.PostCode == request.PostCode &&
                                       x.CompanyId == companyId &&
                                       x.IsBusiness == true
                                   );

                if (existingAddress != null)
                {
                    // Case 2: Same address exists → update IsCurrent to true
                    existingAddress.IsCurrent = true;
                    _context.AddressData.Update(existingAddress);
                }
                else
                {
                    // Case 3: New unique address → insert new record
                    request.IsCurrent = true;
                    _context.AddressData.Add(request);
                }

                //_context.AddressData.Add(request);
            }
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public class DocumentUploadModel
        {
            public string DocumentType { get; set; }
            public IFormFile File { get; set; }
        }

        public async Task<IActionResult> OnPostSaveDocumentTabData()
        {
            var rootPath = Path.Combine(_environment.WebRootPath, "Documents");
            Directory.CreateDirectory(rootPath);

            await SaveFiles(IdentityFiles, "identity", rootPath);
            await SaveFiles(AddressFiles, "address", rootPath);

            return new JsonResult(new { success = true });
        }

        public async Task SaveFiles(List<DocumentUploadModel> files, string category, string rootPath)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            if (files == null) return;

            foreach (var item in files)
            {
                if (item.File != null && item.File.Length > 0)
                {
                    var fileName = $"{userId}_{companyId}_{Path.GetFileName(item.File.FileName)}";
                    var folderPath = Path.Combine(rootPath, category);
                    Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.File.CopyToAsync(stream);
                    }

                    // Save to database
                    _context.Documents.Add(new Document
                    {
                        DocumentType = category,
                        DocumentName = item.DocumentType,
                        FileName = $"{userId}_{companyId}_{item.File.FileName}",
                        FilePath = $"/Documents/{category}/{fileName}",
                        UploadedOn = DateTime.Now,
                        UserId = userId,
                        CompanyId = companyId
                    });
                }
            }



            await _context.SaveChangesAsync();
        }

        public class RequestModel
        {
            public int Id { get; set; }
        }

        public async Task<IActionResult> OnPostRemoveUploadedDocument([FromBody] RequestModel request)
        {
            var item = await _context.Documents.FindAsync(request.Id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(item);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditIdentityDocument()
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            var documentId = Convert.ToInt32(Request.Form["DocumentId"]);
            var documentType = Request.Form["DocumentType"];
            var documentName = Request.Form["DocumentName"];
            var uploadedFile = Request.Form.Files.FirstOrDefault();

            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                return new JsonResult(new { success = false, message = "Document not found" });
            }

            if (uploadedFile != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "Documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{userId}_{companyId}_{uploadedFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(stream);
                }

                document.FileName = $"{userId}_{companyId}_{uploadedFile.FileName}";
                document.FilePath = $"/Documents/{documentType}/{uniqueFileName}"; // For serving later
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditAddressDocument()
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            var documentId = Convert.ToInt32(Request.Form["DocumentId"]);
            var documentType = Request.Form["DocumentType"];
            var documentName = Request.Form["DocumentName"];
            var uploadedFile = Request.Form.Files.FirstOrDefault();

            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                return new JsonResult(new { success = false, message = "Document not found" });
            }

            if (uploadedFile != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "Documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{userId}_{companyId}_{uploadedFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(stream);
                }

                document.FileName = $"{userId}_{companyId}_{uploadedFile.FileName}";
                document.FilePath = $"/Documents/{documentType}/{uniqueFileName}"; // For serving later
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostRemoveCurrentOfficer([FromBody] RequestModel request)
        {
            var item = await _context.CompanyOfficers.FindAsync(request.Id);
            if (item == null)
            {
                return NotFound();
            }

            _context.CompanyOfficers.Remove(item);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
