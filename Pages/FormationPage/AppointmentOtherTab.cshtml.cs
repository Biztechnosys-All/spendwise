﻿using Microsoft.AspNetCore.Mvc;
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
    public class AppointmentOtherTabModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        public AppointmentOtherTabModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [BindProperty]
        public List<OtherLegalOfficers> OtherOfficersList { get; set; } = default!;

        [BindProperty]
        public AddressData OtherAddress { get; set; } = default!;

        [BindProperty]
        public List<AddressData> OtherAddressList { get; set; } = default!;

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        [BindProperty]
        public OtherLegalOfficers OtherOfficers { get; set; } = default!;

        public List<string> SelectedPosition { get; set; } = new List<string>();
        public async Task<IActionResult> OnGet()
        {
            List<OtherLegalOfficers> officerData;
            List<AddressData> addressData;
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId.ToString() == selectCompanyId);
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;
            var officerId = _context.OtherLegalOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.LegalOfficerId;
            var position = _context.OtherLegalOfficers.FirstOrDefault(x => x.UserId == userId && x.LegalOfficerId == officerId);

            officerData = await _context.OtherLegalOfficers.Where(m => m.UserId == userId && m.CompanyID == companyId).ToListAsync();

            addressData = await _context.AddressData.Where(m => m.UserId == userId && m.CompanyId == companyId).ToListAsync();
            OtherOfficers = position;

            if (position != null && !string.IsNullOrWhiteSpace(position.PositionName))
            {
                SelectedPosition = position.PositionName.Split(',').ToList();
            }

            if (addressData == null && officerData== null)
            {
                return NotFound();
            }
            else
            {
                OtherOfficersList = officerData;
                OtherAddressList = addressData;
            }
            return Page();
        }

        public class OtherRequestData
        {
            public OtherLegalOfficers Officer { get; set; }
            public string SelectedPosition { get; set; }
        }

        //public IActionResult OnPostSaveOtherPositionData([FromBody] PositionRequestData data)
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

        //public IActionResult OnPostSaveLegalOfficerDetails([FromBody] OtherLegalOfficers officer)
        //{
        //    try
        //    {
        //        TempData["LegalOfficerData"] = JsonConvert.SerializeObject(officer);
        //        return new JsonResult(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public async Task<JsonResult> OnPostSaveOtherResidentialAddress([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            //var officerId = _context.OtherLegalOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.LegalOfficerId;
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
                }
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<JsonResult> OnPostSaveOtherServiceAddress([FromBody] AddressData request)
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            //var officerId = _context.OtherLegalOfficers.Where(x => x.UserId == userId && x.CompanyID == companyId).FirstOrDefault()?.LegalOfficerId;
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            try
            {
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

                }
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> OnPostSaveLegalEntityDetails([FromBody] OtherRequestData payload)
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

                if (officer.LegalOfficerId > 0)
                {

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        string query = "UPDATE OtherLegalOfficers SET LegalName = @LegalName, LawGoverned = @LawGoverned, LegalForm = @LegalForm WHERE LegalOfficerId = @LegalOfficerId";
                        using (var cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@LegalName", officer.LegalName);
                            cmd.Parameters.AddWithValue("@LawGoverned", officer.LawGoverned);
                            cmd.Parameters.AddWithValue("@LegalForm", officer.LegalForm);
                            cmd.Parameters.AddWithValue("@LegalOfficerId", officer.LegalOfficerId);
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
                    officer = new OtherLegalOfficers
                    {
                        PositionName = positions,
                        LegalName = officer.LegalName,
                        LawGoverned = officer.LawGoverned,
                        LegalForm = officer.LegalForm,
                        UserId = userId,
                        CompanyID = companyId
                    };
                    _context.OtherLegalOfficers.Add(officer);
                }
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, officerId = officer.LegalOfficerId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public class RequestOtherModel
        {
            public int Id { get; set; }
        }

        public async Task<IActionResult> OnPostRemoveCurrentEntity([FromBody] RequestOtherModel request)
        {
            var item = await _context.OtherLegalOfficers.FindAsync(request.Id);
            if (item == null)
            {
                return NotFound();
            }

            _context.OtherLegalOfficers.Remove(item);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
