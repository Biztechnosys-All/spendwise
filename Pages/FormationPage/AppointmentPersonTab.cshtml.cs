using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spendwise_WebApp.Models;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.Json.Nodes;
using static Spendwise_WebApp.Pages.FormationPage.CompanyFormationSubTabModel;

namespace Spendwise_WebApp.Pages.FormationPage
{
    [IgnoreAntiforgeryToken]
    public class AppointmentPersonTabModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        public AppointmentPersonTabModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [BindProperty]
        public List<CompanyOfficer> OfficersList { get; set; } = default!;

        [BindProperty]
        public AddressData PersonAddress { get; set; } = default!;

        [BindProperty]
        public List<AddressData> PersonAddressAddressList { get; set; } = default!;

        [BindProperty]
        public CompanyOfficer Officers { get; set; } = default!;

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        public List<string> SelectedPosition { get; set; } = new List<string>();
        public async Task<IActionResult> OnGet()
        {
            List<CompanyOfficer> officerData;
            List<AddressData> addressData;
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId.ToString() == selectCompanyId);
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            var officerId = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.OfficerId;
            var position = _context.CompanyOfficers.FirstOrDefault(x => x.UserId == userId && x.OfficerId == officerId);
            addressData = await _context.AddressData.Where(m => m.UserId == userId && m.CompanyId == companyId).ToListAsync();

            officerData = await _context.CompanyOfficers.Where(m => m.UserId == userId).ToListAsync();

            if (position != null && !string.IsNullOrWhiteSpace(position.PositionName))
            {
                SelectedPosition = position.PositionName.Split(',').ToList();
            }

            if (officerData == null && addressData == null)
            {
                return NotFound();
            }
            else
            {
                OfficersList = officerData;
                PersonAddressAddressList = addressData;
            }
            return Page();
        }

        public class PositionRequestData
        {
            public string Position { get; set; }
        }

        public IActionResult OnPostSavePersonPositionData([FromBody] PositionRequestData data)
        {
            try
            {
                TempData["PositionData"] = JsonConvert.SerializeObject(data);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> OnPostSaveOfficerDetails([FromBody] CompanyOfficer officer)
        {
            try
            {
                //officer.DOB = DateTime.ParseExact(officer.DOB.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                //TempData["OfficerData"] = JsonConvert.SerializeObject(officer);

                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

                var posData = TempData["PositionData"] as string;
                var positions = JsonConvert.DeserializeObject<PositionRequestData>(posData);
                var data = new CompanyOfficer
                {
                    PositionName = positions.Position,
                    FirstName = officer.FirstName,
                    LastName = officer.LastName,
                    Title = officer.Title,
                    DOB = officer.DOB,
                    Nationality = officer.Nationality,
                    Occupation = officer.Occupation,
                    Authentication1 = officer.Authentication1,
                    Authentication2 = officer.Authentication2,
                    Authentication3 = officer.Authentication3,
                    AuthenticationAns1 = officer.AuthenticationAns1,
                    AuthenticationAns2 = officer.AuthenticationAns2,
                    AuthenticationAns3 = officer.AuthenticationAns3,
                    UserId = userId,
                    CompanyID = companyId
                };
                _context.CompanyOfficers.Add(data);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> OnPostSaveAllTab()
        {
            try
            {
                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

                var posData = TempData["PositionData"] as string;
                var offData = TempData["OfficerData"] as string;

                if (string.IsNullOrEmpty(posData) || string.IsNullOrEmpty(offData))
                {
                    return new JsonResult(new { success = false, message = "Missing data" });
                }

                var positions = JsonConvert.DeserializeObject<PositionRequestData>(posData);
                var officer = JsonConvert.DeserializeObject<CompanyOfficer>(offData);

                //var data = new CompanyOfficer
                //{
                //    PositionName = positions.Position,
                //    FirstName = officer.FirstName,
                //    LastName = officer.LastName,
                //    Title = officer.Title,
                //    DOB = officer.DOB,
                //    Nationality = officer.Nationality,
                //    Occupation = officer.Occupation,
                //    Authentication1 = officer.Authentication1,
                //    Authentication2 = officer.Authentication2,
                //    Authentication3 = officer.Authentication3,
                //    AuthenticationAns1 = officer.AuthenticationAns1,
                //    AuthenticationAns2 = officer.AuthenticationAns2,
                //    AuthenticationAns3 = officer.AuthenticationAns3,
                //    UserId = userId,
                //    CompanyID = companyId
                //};
                //_context.CompanyOfficers.Add(data);
                //await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<JsonResult> OnPostSavePersonResidentialAddress([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            var officerId = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.OfficerId;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            try
            {
                if (request.AddressId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsResidetialAddress = @IsResidetialAddress, CompanyId = @CompanyId WHERE AddressId = @AddressId";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@HouseName", request.HouseName);
                            cmd.Parameters.AddWithValue("@Street", request.Street);
                            cmd.Parameters.AddWithValue("@Town", request.Town);
                            cmd.Parameters.AddWithValue("@Locality", request.Locality);
                            cmd.Parameters.AddWithValue("@PostCode", request.PostCode);
                            cmd.Parameters.AddWithValue("@County", request.County);
                            cmd.Parameters.AddWithValue("@Country", request.Country);
                            cmd.Parameters.AddWithValue("@IsResidetialAddress", true);
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
                    request.IsResidetialAddress = true;
                    request.CompanyId = companyId;
                    request.UserId = userId;
                    request.OfficerId = officerId != null ? officerId : 0;

                    // STEP 1: Reset IsCurrent = false for any current address for this user & company
                    var existingCurrentAddresses = _context.AddressData
                        .Where(x => x.CompanyId == companyId && x.UserId == userId && x.IsCurrent && x.IsResidetialAddress == true);

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
                                           x.IsResidetialAddress == true
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

        public async Task<JsonResult> OnPostSavePersonServiceAddress([FromBody] AddressData request)
        {
            try
            {
                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

                var officerId = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.OfficerId;
                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                if (request.AddressId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsServiceAddress = @IsServiceAddress, CompanyId = @CompanyId WHERE AddressId = @AddressId";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@HouseName", request.HouseName);
                            cmd.Parameters.AddWithValue("@Street", request.Street);
                            cmd.Parameters.AddWithValue("@Town", request.Town);
                            cmd.Parameters.AddWithValue("@Locality", request.Locality);
                            cmd.Parameters.AddWithValue("@PostCode", request.PostCode);
                            cmd.Parameters.AddWithValue("@County", request.County);
                            cmd.Parameters.AddWithValue("@Country", request.Country);
                            cmd.Parameters.AddWithValue("@IsServiceAddress", true);
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
                    request.IsServiceAddress = true;
                    request.CompanyId = companyId;
                    request.UserId = userId;
                    request.OfficerId = officerId != null ? officerId : 0;

                    // STEP 1: Reset IsCurrent = false for any current address for this user & company
                    var existingCurrentAddresses = _context.AddressData
                        .Where(x => x.CompanyId == companyId && x.UserId == userId && x.IsCurrent && x.IsServiceAddress == true);

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
                                           x.IsServiceAddress == true
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
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
