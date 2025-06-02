using Azure.Core;
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

        public class OfficerRequestData
        {
            public CompanyOfficer Officer { get; set; }
            public string SelectedPosition { get; set; }
        }

        //public IActionResult OnPostSavePersonPositionData([FromBody] PositionRequestData data)
        //{
        //    try
        //    {
        //        TempData["PositionData"] = JsonConvert.SerializeObject(data);
        //        return new JsonResult(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public async Task<JsonResult> OnPostSaveOfficerDetailsAsync([FromBody] OfficerRequestData payload)
        {
            try
            {
                var officer = payload.Officer;
                var positions = payload.SelectedPosition;

                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                if (officer.OfficerId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE CompanyOfficers SET Title = @Title, FirstName = @FirstName, LastName = @LastName, DOB = @DOB, Nationality = @Nationality, Occupation = @Occupation, Authentication1 = @Authentication1, AuthenticationAns1 = @AuthenticationAns1, Authentication2 = @Authentication2, AuthenticationAns2 = @AuthenticationAns2, Authentication3 = @Authentication3, AuthenticationAns3 = @AuthenticationAns3 WHERE OfficerId = @OfficerId";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Title", officer.Title);
                            cmd.Parameters.AddWithValue("@FirstName", officer.FirstName);
                            cmd.Parameters.AddWithValue("@LastName", officer.LastName);
                            cmd.Parameters.AddWithValue("@DOB", officer.DOB);
                            cmd.Parameters.AddWithValue("@Nationality", officer.Nationality);
                            cmd.Parameters.AddWithValue("@Occupation", officer.Occupation);
                            cmd.Parameters.AddWithValue("@Authentication1", officer.Authentication1);
                            cmd.Parameters.AddWithValue("@AuthenticationAns1", officer.AuthenticationAns1);
                            cmd.Parameters.AddWithValue("@Authentication2", officer.Authentication2);
                            cmd.Parameters.AddWithValue("@AuthenticationAns2", officer.AuthenticationAns2);
                            cmd.Parameters.AddWithValue("@Authentication3", officer.Authentication1);
                            cmd.Parameters.AddWithValue("@AuthenticationAns3", officer.AuthenticationAns3);
                            cmd.Parameters.AddWithValue("@OfficerId", officer.OfficerId);
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
                    officer = new CompanyOfficer
                    {
                        PositionName = positions,
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
                    _context.CompanyOfficers.Add(officer);
                }

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, officerId = officer.OfficerId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public async Task<IActionResult> OnPostSaveAllTab()
        //{
        //    try
        //    {
        //        var userEmail = Request.Cookies["UserEmail"];
        //        var selectCompanyId = Request.Cookies["ComanyId"];
        //        var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
        //        var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

        //        var posData = TempData["PositionData"] as string;
        //        var offData = TempData["OfficerData"] as string;

        //        if (string.IsNullOrEmpty(posData) || string.IsNullOrEmpty(offData))
        //        {
        //            return new JsonResult(new { success = false, message = "Missing data" });
        //        }

        //        var positions = JsonConvert.DeserializeObject<PositionRequestData>(posData);
        //        var officer = JsonConvert.DeserializeObject<CompanyOfficer>(offData);

        //        //var data = new CompanyOfficer
        //        //{
        //        //    PositionName = positions.Position,
        //        //    FirstName = officer.FirstName,
        //        //    LastName = officer.LastName,
        //        //    Title = officer.Title,
        //        //    DOB = officer.DOB,
        //        //    Nationality = officer.Nationality,
        //        //    Occupation = officer.Occupation,
        //        //    Authentication1 = officer.Authentication1,
        //        //    Authentication2 = officer.Authentication2,
        //        //    Authentication3 = officer.Authentication3,
        //        //    AuthenticationAns1 = officer.AuthenticationAns1,
        //        //    AuthenticationAns2 = officer.AuthenticationAns2,
        //        //    AuthenticationAns3 = officer.AuthenticationAns3,
        //        //    UserId = userId,
        //        //    CompanyID = companyId
        //        //};
        //        //_context.CompanyOfficers.Add(data);
        //        //await _context.SaveChangesAsync();

        //        return new JsonResult(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public async Task<JsonResult> OnPostSavePersonResidentialAddressAsync([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            //var officerId = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.OfficerId;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            try
            {
                if (request.AddressId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsResidetialAddress = @IsResidetialAddress, OfficerId = @OfficerId WHERE AddressId = @AddressId";
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
                            cmd.Parameters.AddWithValue("@OfficerId", request.OfficerId);
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
                                           x.OfficerId == request.OfficerId &&
                                           x.UserId == userId &&
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

        public async Task<JsonResult> OnPostSavePersonServiceAddressAsync([FromBody] AddressData request)
        {
            try
            {
                var userEmail = Request.Cookies["UserEmail"];
                var selectCompanyId = Request.Cookies["ComanyId"];
                var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
                var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

                //var officerId = Convert.ToInt32(Request.Cookies["PersonOfficerId"]);
                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                if (request.AddressId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsServiceAddress = @IsServiceAddress, OfficerId = @OfficerId WHERE AddressId = @AddressId";
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
                            cmd.Parameters.AddWithValue("@OfficerId", request.OfficerId);
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
                    //request.OfficerId = officerId != null ? officerId : 0;

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
                                           x.OfficerId == request.OfficerId &&
                                           x.UserId == userId &&
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public class RequestPersonModel
        {
            public int Id { get; set; }
        }

        public async Task<IActionResult> OnPostRemoveCurrentOfficer([FromBody] RequestPersonModel request)
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
