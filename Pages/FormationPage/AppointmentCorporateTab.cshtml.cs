using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spendwise_WebApp.Models;
using System.Globalization;
using static Spendwise_WebApp.Pages.FormationPage.AppointmentPersonTabModel;

namespace Spendwise_WebApp.Pages.FormationPage
{
    [IgnoreAntiforgeryToken]
    public class AppointmentCorporateTabModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        public AppointmentCorporateTabModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [BindProperty]
        public List<CorporateCompanyOfficers> CorporateOfficersList { get; set; } = default!;

        [BindProperty]
        public AddressData CorporateAddress { get; set; } = default!;

        [BindProperty]
        public List<AddressData> CorporateAddressList { get; set; } = default!;

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        [BindProperty]
        public CorporateCompanyOfficers CorporateOfficers { get; set; } = default!;
        public List<string> CorporateSelectedPosition { get; set; } = new List<string>();
        public string CorporateRegisteredInUk { get; set; }
        public async Task<IActionResult> OnGet()
        {
            List<AddressData> addressData;
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            var officerId = _context.CorporateCompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.CorporateOfficerId;
            var position = _context.CorporateCompanyOfficers.FirstOrDefault(x => x.UserId == userId && x.CorporateOfficerId == officerId);
            Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId.ToString() == selectCompanyId);
            addressData = await _context.AddressData.Where(m => m.UserId == userId && m.CompanyId == companyId).ToListAsync();

            CorporateRegisteredInUk = position?.RegisteredInUK;
            CorporateOfficers = position;
            if (position != null && !string.IsNullOrWhiteSpace(position.PositionName))
            {
                CorporateSelectedPosition = position.PositionName.Split(',').ToList();
            }

            if (addressData == null)
            {
                return NotFound();
            }
            else
            {
                CorporateAddressList = addressData;
            }
            return Page();
        }

        public class CorporatePositionRequestData
        {
            public string Position { get; set; }
        }

        public IActionResult OnPostSaveCorporatePositionData([FromBody] CorporatePositionRequestData data)
        {
            try
            {
                TempData["CorporatePositionData"] = JsonConvert.SerializeObject(data);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> OnPostSaveCorporateDetails([FromBody] CorporateCompanyOfficers corporateOfficer)
        {
            try
            {
                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

                var posData = TempData["CorporatePositionData"] as string;

                if (string.IsNullOrEmpty(posData))
                {
                    return new JsonResult(new { success = false, message = "Missing data" });
                }

                var positions = JsonConvert.DeserializeObject<PositionRequestData>(posData);

                var data = new CorporateCompanyOfficers
                {
                    PositionName = positions.Position,
                    Title = corporateOfficer.Title,
                    FirstName = corporateOfficer.FirstName,
                    LastName = corporateOfficer.LastName,
                    LegalName = corporateOfficer.LegalName,
                    RegisteredInUK = corporateOfficer.RegisteredInUK,
                    RegistrationNumber = corporateOfficer.RegistrationNumber,
                    PlaceRegistered = corporateOfficer.PlaceRegistered,
                    RegistryHeld = corporateOfficer.RegistryHeld,
                    LawGoverned = corporateOfficer.LawGoverned,
                    LegalForm = corporateOfficer.LegalForm,
                    UserId = userId,
                    CompanyID = companyId
                };
                _context.CorporateCompanyOfficers.Add(data);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<JsonResult> OnPostSaveCorporateAddress([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            var officerId = _context.CorporateCompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.CorporateOfficerId;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            try
            {
                if (request.AddressId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsRegisteredOffice = @IsRegisteredOffice, OfficerId = @OfficerId WHERE AddressId = @AddressId";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@HouseName", request.HouseName);
                            cmd.Parameters.AddWithValue("@Street", request.Street);
                            cmd.Parameters.AddWithValue("@Town", request.Town);
                            cmd.Parameters.AddWithValue("@Locality", request.Locality);
                            cmd.Parameters.AddWithValue("@PostCode", request.PostCode);
                            cmd.Parameters.AddWithValue("@County", request.County);
                            cmd.Parameters.AddWithValue("@Country", request.Country);
                            cmd.Parameters.AddWithValue("@IsRegisteredOffice", true);
                            cmd.Parameters.AddWithValue("@OfficerId", officerId);
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
                    request.IsRegisteredOffice = true;
                    request.CompanyId = companyId;
                    request.UserId = userId;
                    request.OfficerId = officerId != null ? officerId : 0;

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
                                           x.HouseName == request.HouseName &&
                                           x.Street == request.Street &&
                                           x.Locality == request.Locality &&
                                           x.Town == request.Town &&
                                           x.County == request.County &&
                                           x.Country == request.Country &&
                                           x.PostCode == request.PostCode &&
                                           x.CompanyId == companyId &&
                                           x.OfficerId == officerId &&
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
                        request.IsCurrent = true;
                        _context.AddressData.Add(request);
                    }


                    //addressData.IsCurrent = true;
                    //_context.AddressData.Add(addressData);
                }

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
